using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TRPK.KR.v1
{
    public partial class Form1 : Form
    {
        private SqlConnection sqlConnection = null;
        public Form1()
        {
            InitializeComponent();
        }
        int IDs = new int();
        int IDs2 = new int();
        string selectedspec = "";

        private void Form1_Load(object sender, EventArgs e)
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["DEKANAT"].ConnectionString);
            sqlConnection.Open();
            filling();
            filling2();
            list();
            list2();


            comboBox1.SelectedIndex = 0;
            //dopcombo();
            // dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.DisplayedCells);
            comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;


            dataGridView1.Columns[0].Visible = false;
            dataGridView1.AutoSizeColumnsMode = (DataGridViewAutoSizeColumnsMode)DataGridViewAutoSizeColumnMode.Fill;
            dataGridView1.Columns[1].HeaderText = "СПЕЦИАЛЬНОСТЬ";
            dataGridView1.Columns[2].HeaderText = "ВИД ОТЧЕТНОСТИ";
            dataGridView1.Columns[3].HeaderText = "СЕМЕСТР";
            dataGridView1.Columns[4].HeaderText = "НАЗВАНИЕ ПРЕДМЕТА";
            dataGridView1.Columns[5].HeaderText = "КОЛ-ВО ЧАСОВ В СЕМЕСТРЕ";

            dataGridView2.Columns[0].Visible = false;
            dataGridView2.AutoSizeColumnsMode = (DataGridViewAutoSizeColumnsMode)DataGridViewAutoSizeColumnMode.Fill;
            dataGridView2.Columns[1].HeaderText = "НАЗВАНИЕ ПРЕДМЕТА";
            dataGridView2.Columns[2].HeaderText = "КОЛ-ВО ЧАСОВ ДЛЯ УСВОЕНИЕ ПРЕДМЕТА";

            string[] typerep = { "ЭКЗАМЕН", "ЗАЧЁТ", "КУРС.ПРОЕКТ", "КУРС.РАБОТА" };
            comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox2.Items.AddRange(typerep);
            comboBox2.SelectedIndex = 0;

        }
        private void filling() ///////////////////////////////////////ЗАПОЛНЕНИЕ ТАБЛИЦЫ 1///////////////////////////////////////
        {
            string sqlcommand = "SELECT Studyload.Id, Studyload.speciality, Studyload.typerep, Studyload.semester, Plansubject.Namesubject, Studyload.NhoursSem FROM Studyload INNER JOIN Plansubject ON Studyload.IdPlansubject=Plansubject.Id";
            if (selectedspec != "")
            {
                sqlcommand = sqlcommand + " AND Studyload.speciality = N'" + selectedspec + "'";
            }
            SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlcommand, sqlConnection);
            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet);
            dataGridView1.DataSource = dataSet.Tables[0];
        }
        private void fillingwhere() ///////////////////////////////////////ЗАПОЛНЕНИЕ ТАБЛИЦЫ 1///////////////////////////////////////
        {
            SqlDataAdapter dataAdapter = new SqlDataAdapter("SELECT Studyload.Id, Studyload.speciality, Studyload.typerep, Studyload.semester, Plansubject.Namesubject, Studyload.NhoursSem FROM Studyload LEFT JOIN Plansubject ON Studyload.IdPlansubject=Plansubject.Id", sqlConnection);
            DataSet dataSet = new DataSet();
            dataAdapter.Fill(dataSet);
            dataGridView1.DataSource = dataSet.Tables[0];
        }
        private void filling2() ///////////////////////////////////////ЗАПОЛНЕНИЕ ТАБЛИЦЫ 2///////////////////////////////////////
        {
            SqlDataAdapter dataAdapter2 = new SqlDataAdapter("SELECT Plansubject.Id, Plansubject.Namesubject, Plansubject.Nhours FROM Plansubject", sqlConnection);
            DataSet dataSet2 = new DataSet();
            dataAdapter2.Fill(dataSet2);
            dataGridView2.DataSource = dataSet2.Tables[0];
        }
        ///////////////////////////////////////ПРОВЕРКА ДАННЫХ ПЕРЕД ИЗМЕНЕНИЕМ БД///////////////////////////////////////
        private bool inputvalid(bool typevalid)
            {
            int t, t2;
            string obj;
            string sqlCommand;
            if (!int.TryParse(textBox2.Text, out t)    ||  //ПРОВЕРКА НА НЕВЕРНЫЙ ТИП ВВОДИМЫХ ДАННЫХ И ПУСТОТУ БОКСОВ
                (!int.TryParse(textBox3.Text, out t2)) ||  //
                (string.IsNullOrEmpty(comboBox2.Text)) ||  //
                (string.IsNullOrEmpty(comboBox1.Text)) ||  //
                (string.IsNullOrEmpty(textBox4.Text)))     //
            {
                return true;
            }
            else
                if ((t < 0) || (t > 16) || (t2 < 0))      //ПРОВЕРКА ГРАНИЦ ЦЕЛОЧИСЛЕННЫХ ЗНАЧЕНИЙ
            {
                return true;
            }
            //ПРОВЕРКА, СВЯЗАННАЯ С ВОЗМОЖНЫМ НАЛИЧИЕМ ЛИШЬ ОДНОГО ЭКЗАМЕНА ИЛИ ЗАЧЁТА ПО КОНКРЕТНОМУ ПРЕДМЕТУ В ОДНОМ СЕМЕСТРЕ 
            if ((comboBox2.Text == "ЭКЗАМЕН" || comboBox2.Text == "ЗАЧЁТ") && typevalid)  
            {
                sqlCommand = "IF exists(SELECT * FROM Studyload WHERE " +                                             // ЕСЛИ  В ТАБЛИЦЕ ЕСТЬ ЗАПИСИ ГДЕ:
                    "(typerep = N'ЗАЧЁТ' OR typerep = N'ЭКЗАМЕН') AND" +                                              // (ВИД ОТЧЁТНОСТИ  - ЗАЧЁТ ИЛИ ЭКЗАМЕН) И
                    " semester = " + t + " AND speciality = N'" + textBox4.Text + "' AND" +                           // СЕМЕСТР СО СПЕЦИАЛЬНОСТЬЮ ИМЕЮТ ЗНАЧЕНИЯ РАВНЫЕ ВВОДИМЫМ КАК И
                    " IdPlansubject = (SELECT Id FROM Plansubject WHERE Namesubject = N'" + comboBox1.Text + "')) " + // ПРЕДМЕТ, ПРИКРЕПЛЕННЫЙ К ЗАПИСИ ПО ID 
                    " SELECT 'TRUE' ELSE SELECT 'FALSE'";                                                             // ВЫВЕСТИ 'TRUE'; В ОБРАТНОМ СЛУЧАЕ - 'FALSE'
                SqlCommand command2 = new SqlCommand(sqlCommand, sqlConnection);
                 obj = Convert.ToString(command2.ExecuteScalar());


                if (obj == "TRUE")
                {
                    return true;
                }

            }
            return false;
   
        }

        private void list()///////////////////////////////////////ЗАПОЛНЕНИЕ КОМБОБОКСА(СПИСКА ПРЕДМЕТОВ)///////////////////////////////////////  *СОЗДАНИЕ ДИНАМИЧЕСКОГО СПИСКА ПО ОДНОМУ ИЗ КОЛОНОК БД
        {
            SqlDataAdapter dataAdapter2 = new SqlDataAdapter("SELECT * FROM Plansubject", sqlConnection);
            DataSet dataSet = new DataSet();
            dataAdapter2.Fill(dataSet);
            List<string> comboboxList = new List<string>();
            comboBox1.Items.Clear();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                comboboxList.Add(row["Namesubject"].ToString());
            }
            string[] comboboxArr = comboboxList.ToArray();
            for (int i = 0; i < comboboxArr.Length; i++)
            {
                comboBox1.Items.Add(comboboxArr[i].ToString());
            }

        }

        private void list2()///////////////////////////////////////ЗАПОЛНЕНИЕ ЛИСТБОКСОВ(СПИСКА СПЕЦИАЛЬНОСТЕЙ)///////////////////////////////////////
        {
            listBox1.Items.Clear();
            listBox2.Items.Clear();
            SqlDataAdapter dataAdapter2 = new SqlDataAdapter("SELECT * FROM Studyload", sqlConnection);
            DataSet dataSet = new DataSet();
            dataAdapter2.Fill(dataSet);
            List<string> specialityList = new List<string>();

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                specialityList.Add(row["speciality"].ToString());
            }
            listBox1.Items.Insert(0, "Специальности:");
            listBox2.Items.Insert(0, "Специальности:");
            string[] specialityArr = specialityList.ToArray();
            for (int i = 0; i < specialityArr.Length; i++)
            {
                if (!listBox1.Items.Contains(specialityArr[i].ToString()))
                {
                    listBox1.Items.Insert(i + 1, specialityArr[i].ToString());
                    listBox2.Items.Insert(i + 1, specialityArr[i].ToString());
                }
            }

        }

        private void dopcombo()///////////////////////////////////////ЗАПОЛНЕНИЕ ТЕКСТБОКСА #3 ЗНАЧЕНИЕМ ЧАСОВ СООТВ-ГО ПРЕДМЕТА ИЗ КОМБОБОКСА///////////////////////////////////////
        {
            SqlCommand command = new SqlCommand("SELECT Plansubject.Nhours FROM Plansubject WHERE Namesubject=@Namesubject", sqlConnection);
            command.Parameters.AddWithValue("Namesubject", comboBox1.Text);
            SqlDataReader dataReader = command.ExecuteReader();
            List<int> nhours = new List<int>();
            while (dataReader.Read())
            {
                nhours.Add(dataReader.GetInt32(0));
                textBox3.Text = nhours[0].ToString();
            }
            
            dataReader.Close();
        }

        ///////////////////////////////////////ДОБАВИТЬ///////////////////////////////////////
        private void button1_Click(object sender, EventArgs e)
        {
            if (inputvalid(true))
            {
                return;
            }
   
           
            SqlCommand command = new SqlCommand("INSERT INTO [Studyload] (speciality, typerep, semester, NhoursSem, IdPlansubject) VALUES (@speciality, @typerep, @semester, @NhoursSem, (SELECT Id FROM Plansubject WHERE Namesubject=@Namesubject) )", sqlConnection);
            command.Parameters.AddWithValue("speciality", textBox4.Text);
            command.Parameters.AddWithValue("typerep", comboBox2.Text);
            command.Parameters.AddWithValue("semester", int.Parse(textBox2.Text));
            command.Parameters.AddWithValue("NhoursSem", int.Parse(textBox3.Text));
            command.Parameters.AddWithValue("Namesubject", comboBox1.Text);

            command.ExecuteNonQuery().ToString();
            filling();
            list2();
            comboBox2.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            comboBox1.Text = "";

        }
        ///////////////////////////////////////ИЗМЕНИТЬ///////////////////////////////////////
        private void button2_Click(object sender, EventArgs e)
        {
            string sqlCommand,obj;
            bool typevalid;

            sqlCommand = "SELECT typerep FROM [Studyload] WHERE (Id = @IDs) ";
            SqlCommand command3 = new SqlCommand(sqlCommand, sqlConnection);
            command3.Parameters.AddWithValue("IDs", IDs);
            obj = Convert.ToString(command3.ExecuteScalar());
            if (obj == "ЭКЗАМЕН" || obj == "ЗАЧЁТ") //ЕСЛИ ИЗМЕНЯЕМАЯ ЗАПИСЬ ИМЕЕТ ТИП ОТЧЕТНОСТИ "ЭКЗАМЕН" ИЛИ "ЗАЧЁТ" ТО ПРОВЕРКА НА ТИП ОТЧЕТНОСТИ НЕ ТРЕБУЕТСЯ
            {
                typevalid = false;
            }
            else
            {
                typevalid = true;
            }

            if (inputvalid(typevalid))
            {
                return;
            }

            SqlCommand command = new SqlCommand("UPDATE [Studyload] SET speciality=@speciality, typerep=@typerep, semester=@semester, NhoursSem=@NhoursSem, IdPlansubject=(SELECT Id FROM Plansubject WHERE Namesubject=@Namesubject) WHERE Id=@IDs", sqlConnection);
            command.Parameters.AddWithValue("IDs", IDs);
            command.Parameters.AddWithValue("speciality", textBox4.Text);
            command.Parameters.AddWithValue("typerep", comboBox2.Text);
            command.Parameters.AddWithValue("semester", int.Parse(textBox2.Text));
            command.Parameters.AddWithValue("NhoursSem", int.Parse(textBox3.Text));
            command.Parameters.AddWithValue("Namesubject", comboBox1.Text);
            command.ExecuteNonQuery().ToString();
            filling();
        }
       
        ///////////////////////////////////////УДАЛИТЬ///////////////////////////////////////
        private void button3_Click(object sender, EventArgs e)
        {
            string sqlcommand;
            if (IDs != 0)
            {
                sqlcommand = "DELETE FROM [Studyload] WHERE (Id = @IDs) ";
                SqlCommand command = new SqlCommand(sqlcommand, sqlConnection);
                command.Parameters.AddWithValue("IDs", IDs);
                command.ExecuteNonQuery().ToString();
                IDs = 0;
            }
            else
            {
                sqlcommand = "DELETE FROM [Studyload] WHERE (Id = (SELECT MAX(Studyload.Id) Id FROM Studyload ";
                if (selectedspec!="")
                {
                    sqlcommand = sqlcommand + "WHERE Studyload.speciality = N'"+ selectedspec + "'";
                }
                sqlcommand = sqlcommand + "))";
                SqlCommand command = new SqlCommand(sqlcommand, sqlConnection);
                command.ExecuteNonQuery().ToString();
            }
            filling();
            list2();
            comboBox2.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            comboBox1.Text = "";
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            int rowIndex = dataGridView1.CurrentCell.RowIndex;
            IDs = int.Parse(dataGridView1[0, rowIndex].Value.ToString());
            comboBox2.Text = dataGridView1[2, rowIndex].Value.ToString();
            textBox2.Text = dataGridView1[3, rowIndex].Value.ToString();
            comboBox1.Text = dataGridView1[4, rowIndex].Value.ToString();
            textBox3.Text = dataGridView1[5, rowIndex].Value.ToString();
            textBox4.Text = dataGridView1[1, rowIndex].Value.ToString();
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
           // dopcombo();
        }


        private void listBox1_MouseClick(object sender, MouseEventArgs e)
        {
            ///"Специальности:"
            ///


            int t;
            if (!int.TryParse(listBox1.SelectedIndex.ToString(), out t))
            {
                return;
            }
            else
            if (t == 0)
            {
                textBox4.Text = "";
                selectedspec = "";
                filling();
                return;
            }
            else
                 if (t > 0)
            {
                selectedspec = listBox1.SelectedItem.ToString();
                string sqlcomm = "SELECT Studyload.Id, Studyload.speciality, Studyload.typerep, Studyload.semester, Plansubject.Namesubject, Studyload.NhoursSem FROM Studyload LEFT JOIN Plansubject ON Studyload.IdPlansubject=Plansubject.Id WHERE Studyload.speciality = N'" + selectedspec + "'";
                textBox4.Text = selectedspec;
                SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlcomm, sqlConnection);
                DataSet dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                dataGridView1.DataSource = dataSet.Tables[0];
            }
        }
        ///////////////////////////////////////ОЧИСТИТЬ ПОЛЯ///////////////////////////////////////
        private void button4_Click(object sender, EventArgs e)
        {
            comboBox2.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            comboBox1.Text = "";
            IDs = 0;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox1.Items.Contains(textBox4.Text);
            if (listBox1.Items.Contains(textBox4.Text))
            {
                string sqlcomm = "DELETE FROM [Studyload] WHERE speciality = N'" + textBox4.Text + "'";
                SqlCommand command = new SqlCommand(sqlcomm, sqlConnection);
                command.Parameters.AddWithValue("IDs", IDs);
                command.ExecuteNonQuery().ToString();
                IDs = 0;
            }
            filling();
            list2();
            comboBox2.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            comboBox1.Text = "";

        }
        //################################################//ПРЕДМЕТЫ ПЛАНА//################################################//
        private void button6_Click(object sender, EventArgs e)
        {
            int t;
            if (!int.TryParse(textBox5.Text, out t))
            {
                return;
            }
            else
                if (t < 0)
            {
                return;
            }
            SqlCommand command = new SqlCommand("INSERT INTO [Plansubject] (Namesubject, Nhours) VALUES (@Namesubject, @Nhours)", sqlConnection);
            command.Parameters.AddWithValue("Namesubject", textBox1.Text);
            command.Parameters.AddWithValue("Nhours", int.Parse(textBox5.Text));
            command.ExecuteNonQuery().ToString();
            filling2();
            list();
            textBox1.Text = "";
            textBox5.Text = "";

        }

        private void button7_Click(object sender, EventArgs e)
        {
            int t;
            if (!int.TryParse(textBox5.Text, out t))
            {
                return;
            }
            else
                if (t < 0)
            {
                return;
            }
            SqlCommand command = new SqlCommand("UPDATE [Plansubject] SET Namesubject=@Namesubject, Nhours=@Nhours WHERE Id=@IDs", sqlConnection);
            command.Parameters.AddWithValue("IDs", IDs2);
            command.Parameters.AddWithValue("Namesubject", textBox1.Text);
            command.Parameters.AddWithValue("Nhours", int.Parse(textBox5.Text));
            command.ExecuteNonQuery().ToString();
            filling2();
            list();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (IDs2 != 0)
            {
                SqlCommand command = new SqlCommand("DELETE FROM [Plansubject] WHERE (Id = @IDs) ", sqlConnection);
                command.Parameters.AddWithValue("IDs", IDs2);
                command.ExecuteNonQuery().ToString();
                IDs2 = 0;
            }
            else
            {
                SqlCommand command = new SqlCommand("DELETE FROM [Plansubject] WHERE (Id = (SELECT MAX(Plansubject.Id) Id FROM Plansubject)) ", sqlConnection);
                command.ExecuteNonQuery().ToString();
            }
            filling2();
            list();
            textBox1.Text = "";
            textBox5.Text = "";

        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex2 = dataGridView2.CurrentCell.RowIndex;
            IDs2 = int.Parse(dataGridView2[0, rowIndex2].Value.ToString());
            textBox1.Text = dataGridView2[1, rowIndex2].Value.ToString();
            textBox5.Text = dataGridView2[2, rowIndex2].Value.ToString();
        }

        private void listBox2_MouseClick(object sender, MouseEventArgs e)
        {
            ///"Специальности:"
            ///


            int t;
            if (!int.TryParse(listBox2.SelectedIndex.ToString(), out t))
            {
                return;
            }
            else
            if (t == 0)
            {
                filling2();
                return;
            }
            else
                 if (t > 0)
            {
               //ЧАСЫ, СЕМЕСТР КОТОРЫХ ОПРЕДЕЛЁН string sqlcomm = "SELECT Plansubject.Id, Plansubject.Namesubject, Plansubject.Nhours, Summa = (SELECT SUM(Studyload.NhoursSem)  FROM  Studyload WHERE (Studyload.speciality = N'" + listBox2.SelectedItem + "') AND (Plansubject.Id = Studyload.IdPlansubject)) FROM Plansubject";
                string sqlcomm = "SELECT Plansubject.Id, Plansubject.Namesubject, Plansubject.Nhours," +/*Находим id, название прдмета, кол-во часов*/
                    " Undhours = (SELECT Plansubject.Nhours - SUM(Studyload.NhoursSem) FROM " +            /*и часы, которые недостают предметам*/
                    " Studyload WHERE (Studyload.speciality = N'" + listBox2.SelectedItem + "') AND " + /*По условию выбранной специальности*/
                    "(Plansubject.Id = Studyload.IdPlansubject)) FROM Plansubject";                     /*и совпадения предмета и учебной нагрузки по id*/
                SqlDataAdapter dataAdapter2 = new SqlDataAdapter(sqlcomm, sqlConnection);
                DataSet dataSet2 = new DataSet();
                dataAdapter2.Fill(dataSet2);
                dataGridView2.DataSource = dataSet2.Tables[0];
                dataGridView2.Columns[3].HeaderText = "ЧАСЫ, СЕМЕСТР ДЛЯ КОТОРЫХ НЕ ОПРЕДЕЛЁН";
            }
        }
    }
}

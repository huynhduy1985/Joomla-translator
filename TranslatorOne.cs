using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace RavSoft.GoogleTranslator
{
    public partial class TranslatorOne : Form
    {
        public TranslatorOne()
        {
            InitializeComponent();
        }
        private static string[] enDataFirst;
        private static string[] viDataFirst;
        public SimpleDataFile DataTranslator{ get; set; }
        private static BindingSource obj;
        private void LoadTranslate()
        {
            int j = 0;
            var enData = enDataFirst.Where(w => !w.StartsWith(";") && w != "").ToArray();
            var viData = enData;
            try
            {
                viData = viDataFirst.Where(w => !w.StartsWith(";") && w != "").ToArray();
            }
            catch (Exception)
            {
            }
            obj = new BindingSource();
            for (int i = 0; i < enData.Length; i++)
            {
                string khoachinh = "";
                string en = "";
                string vi = "";
                string[] tenpEn = enData[i].Split('=');
                if (tenpEn.Length > 1)
                {
                    khoachinh = tenpEn[0];
                    en = enData[i].Remove(0, tenpEn[0].Length + 1).Trim('"');
                }
                string[] iVi = viData.Where(w => w.StartsWith(khoachinh + "=")).ToArray();
                if (iVi.Length > 0)
                {
                    string[] tenpvi = iVi[0].Split('=');
                    if (tenpvi.Length > 1)
                    {
                        if (khoachinh == tenpvi[0])
                        {
                            vi = iVi[0].Remove(0, tenpvi[0].Length + 1).Trim('"');
                            j++;
                        }
                    }
                }

                if (khoachinh.Length>0)
                {
                    obj.Add(new SimpleData { Chon = false, KhoaChinh = khoachinh, En = en, Vi = vi });
                }
            }
            gvData.DataSource = obj;
            
        }

        private void TranslatorOne_Load(object sender, EventArgs e)
        {

            this._comboFrom.Items.AddRange(Translator.Languages.ToArray());
            this._comboTo.Items.AddRange(Translator.Languages.ToArray());
            this._comboFrom.SelectedItem = "English";
            this._comboTo.SelectedItem = "Vietnamese";
            try
            {
                enDataFirst = System.IO.File.ReadAllLines(DataTranslator.PathEn);
                if (DataTranslator.PathVi != string.Empty)
                {
                    viDataFirst = System.IO.File.ReadAllLines(DataTranslator.PathVi);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            LoadTranslate();
            _lblStatus.Text = string.Empty;
        }

        private void gvData_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in gvData.Rows)
            {
                SimpleData temp = (SimpleData)row.DataBoundItem;
                if (temp.Vi == "")
                {
                    row.Cells[1].Style.BackColor = Color.Coral;
                    row.Cells[2].Style.BackColor = Color.Coral;
                    row.Cells[3].Style.BackColor = Color.Coral;
                }
                else if (temp.Chon)
                {
                    row.Cells[1].Style.BackColor = Color.LightBlue;
                    row.Cells[2].Style.BackColor = Color.LightBlue;
                    row.Cells[3].Style.BackColor = Color.LightBlue;
                }
                if (temp.Vi.Contains("%1$s") || temp.Vi.Contains("%s") || temp.Vi.Contains("%d"))
                {
                    row.Cells[3].Style.BackColor = Color.DodgerBlue;
                }
            }
        }

        private void bTranslate_Click(object sender, EventArgs e)
        {
            Translator t = new Translator();
            var bindingSource = new BindingSource();
            pBar.Maximum = obj.List.Count;
            pBar.Value = 1;
            pBar.Step = 1;
            for (int i = 0; i < obj.List.Count; i++)
            {
                SimpleData temp = (SimpleData)obj.List[i];
                if (chkIsEmpty.Checked && temp.Vi == string.Empty)
                {
                    temp = TranslateLang(t, temp);
                    temp.Chon = true;
                }
                else if(chkFull.Checked)
                {
                    temp = TranslateLang(t, temp);
                }
                bindingSource.Add(temp);
                pBar.PerformStep();
                pBar.Update();
            }
            gvData.DataSource = bindingSource;
            obj = bindingSource;
            gvData.Update();
        }

        private SimpleData TranslateLang(Translator t, SimpleData temp)
        {
            t.SourceLanguage = (string)this._comboFrom.SelectedItem;
            t.TargetLanguage = (string)this._comboTo.SelectedItem;
            t.SourceText = temp.En;
            try
            {
                // Forward translation
                this.Cursor = Cursors.WaitCursor;
                this._lblStatus.Text = "Being translation..." + temp.KhoaChinh + "...Key";
                this._lblStatus.Update();
                t.Translate();
                var stext = t.Translation;
                stext = stext.Replace("&gt;", ">");
                if (stext.StartsWith("gt;"))
                {
                    stext = stext.Replace("gt;", ">");
                }
                stext = stext.Replace("&lt;", "<");
                if (stext.StartsWith("lt;"))
                {
                    stext = stext.Replace("&lt;", "<");
                }
                stext = stext.Replace("% 1 $ s", " %1$s ");
                stext = stext.Replace("% 2 $ s", " %2$s ");
                stext = stext.Replace("% 3 $ s", " %3$s ");
                stext = stext.Replace("% 4 $ s", " %4$s ");
                stext = stext.Replace("% 5 $ s", " %5$s ");
                stext = stext.Replace("% 6 $ s", " %6$s ");
                stext = stext.Replace("% d", " %d ");
                stext = stext.Replace("% s", " %s ");
                if (stext.StartsWith("D "))
                {
                    stext = stext.Replace("D ", "%d ");
                }
                if (stext.StartsWith("S "))
                {
                    stext = stext.Replace("S ", "%s "); 
                }
                stext = stext.Replace("_QQ_", " \"_QQ_\" ");
                temp.Vi = stext;

                this._lblStatus.Text = "Translate completed..." + temp.KhoaChinh + "...Key";
                this._lblStatus.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                this._lblStatus.Text = string.Empty;
                this.Cursor = Cursors.Default;
            }
            return temp;
        }
        
        private void bSaveToFile_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Đang trong quá trình xử lý mong các bạn thông cảm","Thông báo");
            Savetofile();
        }

        private void Savetofile()
        {
            try
            {
                var viData = viDataFirst.Where(w => w.StartsWith(";")).ToArray();
                int iCount = obj.List.Count + viData.Length;
                string[] temp = new string[iCount];
                for (int i = 0; i < viData.Length; i++)
                {
                    temp[i] = viData[i];
                }
                int j = 0;
                for (int i = viData.Length; i < temp.Length; i++)
                {

                    SimpleData tranTemp = (SimpleData)obj.List[j];
                    temp[i] = tranTemp.KhoaChinh + "=\"" + tranTemp.Vi + "\"";
                    j++;
                }
                System.IO.File.WriteAllLines(DataTranslator.PathVi, temp);
                MessageBox.Show("Save completed a file in PATH :" + DataTranslator.PathVi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void gvData_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 3)
            {
                obj = (BindingSource)gvData.DataSource;
                SimpleData tranTemp = (SimpleData)obj.Current;
                tranTemp.Vi = e.FormattedValue.ToString();
                obj.List[e.RowIndex] = tranTemp;
            }
        }


        private void gvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex ==3)
            {
                gvData.CurrentCell = gvData[e.ColumnIndex, e.RowIndex];
                gvData.BeginEdit(false);
            }
        }

        private void TranslatorOne_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true && e.KeyCode == Keys.S)
            {

                bSaveToFile.PerformClick();

            }
        }

        private void gvData_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control == true && e.KeyCode == Keys.S)
            {

                bSaveToFile.PerformClick();

            }
        }

       
    }
}

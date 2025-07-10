using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.TNT
{
	/// <summary>
	/// Summary description for Exit2ULDForm.
	/// </summary>
	public partial class Exit2ULDForm : ZChildForm
	{
		public Exit2ULDForm(string consol, string mawb, string mBagNo)
		{
			this.ConsolTextBox.Text = consol;
			this.MawbTextBox.Text = mawb;
			this.MbagTextBox.Text = mBagNo;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>

		private void OKButton_Click(object sender, System.EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(ULDTextBox.Text))
			{
				Globals.Message.ShowError("Please enter a ULD no. <Enter NA if not required>");
			}
			else
			{
				DialogResult = DialogResult.OK;
				ULDNo = ULDTextBox.Text.Trim();
				Close();
			}
		}

		public string ULDNo
		{
			get { return fULDNo; }
			set { fULDNo = ULDTextBox.Text.Trim(); }
		}
		protected string fULDNo;
	}
}

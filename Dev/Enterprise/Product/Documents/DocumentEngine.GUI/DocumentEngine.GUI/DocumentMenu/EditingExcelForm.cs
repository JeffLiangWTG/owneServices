using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu
{
	internal partial class EditingExcelForm : ZChildForm
	{
		public EditingExcelForm()
		{
			InitializeComponent();
		}

		public delegate bool CheckEditFinishedDelegate();
		public event CheckEditFinishedDelegate CheckEditFinished;

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void OKBtn_Click(object sender, System.EventArgs e)
		{
			if (CheckEditFinished != null && CheckEditFinished())
			{
				this.DialogResult = DialogResult.OK;
				Close();
			}
			else
			{
				EditingLabel.Text = Res.GetString("339b0abb-b44f-4ef2-b059-5adbe2d853d4", "Please save and close the Excel template before continuing.");
			}
		}
	}
}

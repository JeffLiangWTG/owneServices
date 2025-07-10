using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CustomerService.GUI
{
	public partial class ImportantNoticeForm : ZChildForm
	{
		public ImportantNoticeForm()
		{
			InitializeComponent();
			HeadingLabel.Font = new System.Drawing.Font("Arial", 13F, System.Drawing.FontStyle.Regular);
		}

		void SendButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}

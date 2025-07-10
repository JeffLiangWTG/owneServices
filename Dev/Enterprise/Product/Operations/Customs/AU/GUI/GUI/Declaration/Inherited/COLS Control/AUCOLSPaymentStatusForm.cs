using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class AUCOLSPaymentStatusForm : ZChildForm
	{
		public AUCOLSPaymentStatusForm()
		{
			InitializeComponent();
			SetupControlProperty();
		}

		void SetupControlProperty()
		{
			ClientAccountNumberextBox.MaxLength = 12;
		}

		void cancelButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void SendButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		void ClientAccountNumberextBox_TextChanged(object sender, System.EventArgs e)
		{
			SendButton.Enabled = ClientAccountNumberextBox.Text.Length > 0;
		}
	}
}

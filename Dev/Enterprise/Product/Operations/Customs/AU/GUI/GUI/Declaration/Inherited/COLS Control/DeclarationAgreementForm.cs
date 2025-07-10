using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class DeclarationAgreementForm : ZChildForm
	{
		public DeclarationAgreementForm(COLSDeclarationAcceptance declarationAcceptance)
			: base(declarationAcceptance)
		{
			InitializeComponent();
			SetupControlProperty();
		}

		void SetupControlProperty()
		{
			SendButton.Enabled = DeclarationAgreementControl.AcceptCheckBox.Checked;
			DeclarationAgreementControl.AcceptCheckBox.CheckedChanged += AcceptCheckBox_CheckedChanged;
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

		void AcceptCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			SendButton.Enabled = DeclarationAgreementControl.AcceptCheckBox.Checked;
		}
	}
}

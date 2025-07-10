using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class AUCOLSReassessmentForm : ZChildForm
	{
		public AUCOLSReassessmentForm(QuarantineColsHeader colsHeader, COLSDeclarationAcceptance declarationAcceptance)
			: base(declarationAcceptance)
		{
			InitializeComponent();
			SetupControlsProperties();
		}

		public string ReassessmentReason => ReassessmentReasonTextBox.Text;

		public bool RequireDocumentation => RequireDocumentationCheckBox.Checked;

		void SetupControlsProperties()
		{
			SendButton.Enabled = false;
			ReassessmentReasonTextBox.MaxLength = 1000;
			CharacterCountTextBox.Text = "0";
			DeclarationAgreementControl.AcceptCheckBox.CheckedChanged += AcceptCheckBox_CheckedChanged;
		}

		void AcceptCheckBox_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateSendButtonEnabledProperty();
		}

		void ReassessmentReasonTextBox_TextChanged(object sender, System.EventArgs e)
		{
			CharacterCountTextBox.Text = ReassessmentReason.Length.ToString();
			UpdateSendButtonEnabledProperty();
		}

		void UpdateSendButtonEnabledProperty()
		{
			SendButton.Enabled = DeclarationAgreementControl.AcceptCheckBox.Checked && !string.IsNullOrWhiteSpace(ReassessmentReason);
		}

		void SendButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		void cancelButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}

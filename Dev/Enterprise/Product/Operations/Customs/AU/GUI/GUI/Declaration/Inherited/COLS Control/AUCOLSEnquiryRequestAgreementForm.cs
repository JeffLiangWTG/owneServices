using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class AUCOLSEnquiryRequestAgreementForm : ZChildForm
	{
		public AUCOLSEnquiryRequestAgreementForm(COLSEnquiryAdditionalInformation additionalInformation)
			: base(additionalInformation)
		{
			this.enquiryAdditionalInfo = additionalInformation;
			InitializeComponent();
			SetupControlProperty();
		}
		readonly COLSEnquiryAdditionalInformation enquiryAdditionalInfo;

		void SetupControlProperty()
		{
			SendButton.Enabled = DeclarationAgreementControl.AcceptCheckBox.Checked;
			enquiryAdditionalInfo.EnquiryTypeInfo.ValueChanged += SetSendButtonAvailability;
			enquiryAdditionalInfo.ContactNameInfo.ValueChanged += SetSendButtonAvailability;
			enquiryAdditionalInfo.ContactPhoneInfo.ValueChanged += SetSendButtonAvailability;
			enquiryAdditionalInfo.ContactEmailInfo.ValueChanged += SetSendButtonAvailability;
			DeclarationAgreementControl.AcceptCheckBox.CheckedChanged += SetSendButtonAvailability;
		}

		void SetSendButtonAvailability(object sender, System.EventArgs e)
		{
			SendButton.Enabled = IsReadyToSend;
		}

		bool IsReadyToSend
		{
			get
			{
				var readyToSend = false;
				if (DeclarationAgreementControl.AcceptCheckBox.Checked)
				{
					enquiryAdditionalInfo.Validation.ValidateAll();
					readyToSend = !enquiryAdditionalInfo.HasMessageErrors;
				}
				return readyToSend;
			}
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

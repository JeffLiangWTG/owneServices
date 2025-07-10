using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.Testing
{
	[TestedType(typeof(AUCOLSEnquiryRequestAgreementForm))]
	sealed class AUCOLSEnquiryRequestAgreementFormTest : ZFormBasherTest
	{
		public void TestSendButton()
		{
			var additionalInfo = new COLSEnquiryAdditionalInformation(colsHeader);
			using (var form = new AUCOLSEnquiryRequestAgreementForm(additionalInfo))
			{
				additionalInfo.EnquiryType = COLSEnquiryTypeList.Codes.ConsignmentSpecificEnquiry;
				additionalInfo.ContactName = "Test name";
				additionalInfo.ContactPhone = "+61 2 9744 8000";
				additionalInfo.ContactEmail = "Test email";
				form.DeclarationAgreementControl.AcceptCheckBox.Checked = true;
				AssertEquals("No message errors", false, additionalInfo.HasMessageErrors);
				AssertEquals("Send button is enabled", true, form.SendButton.Enabled);

				additionalInfo.EnquiryType = ZString.Empty;
				AssertEquals("Has message error", true, additionalInfo.HasMessageErrors);
				AssertEquals("Send button is disabled", false, form.SendButton.Enabled);

				additionalInfo.ContactName = ZString.Empty;
				additionalInfo.EnquiryType = COLSEnquiryTypeList.Codes.ConsignmentSpecificEnquiry;
				AssertEquals("Has message error", true, additionalInfo.HasMessageErrors);
				AssertEquals("Send button is disabled", false, form.SendButton.Enabled);

				additionalInfo.ContactPhone = ZString.Empty;
				additionalInfo.ContactName = "Test name";
				AssertEquals("Has message error", true, additionalInfo.HasMessageErrors);
				AssertEquals("Send button is disabled", false, form.SendButton.Enabled);

				additionalInfo.ContactEmail = ZString.Empty;
				additionalInfo.ContactPhone = "+61 2 9744 8000";
				AssertEquals("Has message error", true, additionalInfo.HasMessageErrors);
				AssertEquals("Send button is disabled", false, form.SendButton.Enabled);

				form.DeclarationAgreementControl.AcceptCheckBox.Checked = false;
				AssertEquals("Send button is disabled", false, form.SendButton.Enabled);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new AUCOLSEnquiryRequestAgreementForm(new COLSEnquiryAdditionalInformation(colsHeader));
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			colsHeader = Factory.New<QuarantineColsHeader>();
			colsHeader.QCH_CH_CusEntryHeader = entryHeader.PK;
		}
		QuarantineColsHeader colsHeader;
	}
}

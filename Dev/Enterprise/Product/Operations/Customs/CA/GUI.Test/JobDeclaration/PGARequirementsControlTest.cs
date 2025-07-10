using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class PGARequirementsControlTest : TestCaseWithFactory
	{
		public void TestShouldUpdateIndicatorEvent()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var header = declaration.Invoices.AddNew();

			var invoiceLine = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.CFIAPGAHeader;
			pgaHeader.CA_AIRSExtensionCode = "TEST";

			var collection = invoiceLine.PGARequirements;
			var programRequirement = invoiceLine.PGARequirements.PGARequirement(Common.CA.PGACodes.Codes.CFIA).ProgramCodeRequirements[0];
			programRequirement.DeclareYes = true;

			using (var frm = new ZForm(collection))
			{
				var ctr = new PGARequirementsControl();
				frm.Controls.Add(ctr);

				frm.Show();
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				programRequirement.DeclareYes = false;

				var expectedMessage = "Making this change may delete some existent data on All Programs for CFIA. Do you want to continue?";

				Assert("Should not be changed as the last dialog result of message is no.", programRequirement.DeclareYes);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				programRequirement.DeclareYes = false;

				Assert("Should be changed as the last dialog result of message is yes.", !programRequirement.DeclareYes);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				programRequirement.DeclareYes = true;

				Assert("Should be changed.", programRequirement.DeclareYes);
				Assert("Should be null as there is no need to prevent the user from ticking the option.", UnitTestUserNotification.Instance.LastMessage.WasNone);

				UnitTestUserNotification.Instance.ClearMessages();
				programRequirement.DeclareYes = false;

				Assert("Should be changed.", !programRequirement.DeclareYes);
				Assert("Should be null as there is no data on the CFIA PGA program.", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}
	}
}

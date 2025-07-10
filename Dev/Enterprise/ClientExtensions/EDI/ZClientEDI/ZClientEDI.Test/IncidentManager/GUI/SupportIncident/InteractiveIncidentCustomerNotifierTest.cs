using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Mail.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	class InteractiveIncidentCustomerNotifierTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			var incident = new SupportIncidentTestHelper(Factory).CreateIncidentWithLicencedContact();
			incident.IM_Product = ProductTypes.Codes.Enterprise;
			incident.IM_Module = MandatoryCustomerServiceMenuSectionList.Codes.Eservices;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Support;
			incident.IM_Status = SupportIncidentLookups.Status.Open;
			incident.IM_Priority = Enterprise.Core.Constants.CustomerService.CriticalityCodes.CR4_SingleFunctionWithWorkAround;
			incident.IM_Description = "Test Incident";
			incident.DetailNoteText = "1 2 3 4 5";
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			var email = SupportIncidentEmail.New(incident);
			email.Subject = "client email subject";
			email.Body = "This is the client email body";
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var sender = new EmailOnlyCustomerNotificationSender();
			var notifier = new InteractiveIncidentCustomerNotifier(incident, sender);
			email.UserCanEdit = true;
			notifier.SendEmailNow(email);
			AssertEquals(typeof(CustomerServiceEmailForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertContains("client email subject", ((SupportIncidentEmail)ZFormModaliser.LastIBusinessShownOnDialogForTest).Subject);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var sentEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("client email subject", sentEmail.Subject);
		}
	}
}

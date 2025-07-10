using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.CustomerService.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Xml;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class ERequestEmailCustomerNotificationSenderTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "DDDSYD";
			org.CreateAndLoadLicenceForOrg();
			var db = org.LicCompany.LicDatabases.AddNew();
			db.LD_ServerCode = "PRD";
			db.LD_PublicEmailAddressForUpdate = "test@test.com";
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_LD = db.PK;
			clientCompany.LCC_Code = org.LicCompany.LC_CompanyCode;
			Factory.Save();

			SupportIncident incident = Factory.New<SupportIncident>();
			incident.IM_IncidentNumber = "IM12345";
			incident.IM_Description = "Test Incident";
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = IncidentMainLookups.Status.Open;
			incident.IM_LD = db.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_ClientIncidentReference = "CL222";

			var sender = new ERequestEmailCustomerNotificationSender();
			sender.SendChanges(incident, null);

			AssertEquals("email count", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("FromDisplayName", SupportIncident.SupportDisplayName, email.FromDisplayName);
			AssertEquals("FromAddress", EDIDataRegistry.Instance.IncidentFromEmailAddress.Value, email.FromAddress);
			AssertEquals("recipients", 1, email.Recipients.Count);
			AssertEquals("recipient", "test@test.com", email.Recipients[0].Email);
			AssertEquals("subject", ServiceRequestResponsesProcessor.ServiceRequestResponsesSubject + ": " + incident.IM_ClientIncidentReference, email.Subject);
			AssertEquals("Incident Details.xml", email.Attachments[0].DisplayName);

			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(Xsd.CustomerServiceResponse));
			Xsd.CustomerServiceResponse response2;
			using (MemoryStream ms = new MemoryStream(email.Attachments[0].Data))
			{
				response2 = (Xsd.CustomerServiceResponse)serializer.Deserialize(ms);
			}

			AssertEquals("response ClientReferenceNumber", "CL222", response2.ClientReferenceNumber);

			Env.OutgoingMailManager.EmailsCreated.Clear();
			incident.IM_LD = ZGuid.Empty;
			incident.IM_LCC = ZGuid.Empty;
			sender.SendChanges(incident, null);
			AssertEquals("no email since no licence", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			incident.IM_LD = db.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.Database.LD_PublicEmailAddressForUpdate = "";
			sender.SendChanges(incident, null);
			AssertEquals("no email since no LD_PublicEmailAddressForUpdate", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}
	}
}

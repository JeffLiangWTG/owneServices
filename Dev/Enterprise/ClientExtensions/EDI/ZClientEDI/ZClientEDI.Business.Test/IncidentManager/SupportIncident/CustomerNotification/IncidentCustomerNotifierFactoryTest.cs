using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentCustomerNotifierFactoryTest : TestCaseWithFactory
	{
		public void TestCreateNotifier()
		{
			var incident = Factory.New<SupportIncident>();

			using (Env.SetTemporaryUserContext(User.WebUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertType<CustomerChangeIncidentCustomerNotifier>(IncidentCustomerNotifierFactory.CreateNotifier(incident));
			}

			AssertType<LocalSystemChangeIncidentCustomerNotifier>(IncidentCustomerNotifierFactory.CreateNotifier(incident));
		}

		public void TestCreateSender()
		{
			var legacyBuild = Factory.New<ReleaseBuild>();
			legacyBuild.VersionNumber = new VersionNumber(LicenceDatabase.FirstReleaseCSBiDirectionMessage).AddMinor(-1);

			var eHubBuild = Factory.New<ReleaseBuild>();
			eHubBuild.VersionNumber = new VersionNumber(LicenceDatabase.FirstReleaseCSBiDirectionMessage);

			var legacyEntLicence = BillingTestHelper.CreateLicence(Factory, "ENT", "COM", "SRC");
			legacyEntLicence.Database.LD_HL_CurrentRunningVersion = legacyBuild.PK;

			var eHubLicence = BillingTestHelper.CreateAnotherDatabase(legacyEntLicence, "CO2");
			eHubLicence.Database.LD_HL_CurrentRunningVersion = eHubBuild.PK;

			var otherProductLicence = BillingTestHelper.CreateAnotherDatabase(legacyEntLicence, "CO3", false);
			otherProductLicence.Database.LD_Product = "TLX";

			var orgWithoutLicence = Factory.New<EDIOrgHeader>();
			orgWithoutLicence.OH_Code = "AAABBB";
			var contact = orgWithoutLicence.Contacts.AddNew();
			contact.OC_Email = "other@test.com";

			Factory.Save();

			var legacyEntIncident = Factory.New<SupportIncident>();
			legacyEntIncident.IM_ClientIncidentReference = "SR0001";
			legacyEntIncident.IM_Product = ProductTypes.Codes.Enterprise;
			legacyEntIncident.IM_OH_Client = legacyEntLicence.Company.LC_OH;
			legacyEntIncident.IM_OC_Contact = legacyEntLicence.Company.Header.Contacts[0].PK;
			legacyEntIncident.IM_LD = legacyEntLicence.LA_LD;
			legacyEntIncident.IM_LCC = legacyEntLicence.ClientCompany.PK;

			var eHubIncident = Factory.New<SupportIncident>();
			eHubIncident.IM_ClientIncidentReference = "SR0001";
			eHubIncident.IM_OH_Client = eHubLicence.Company.LC_OH;
			eHubIncident.IM_OC_Contact = eHubLicence.Company.Header.Contacts[0].PK;
			eHubIncident.IM_LD = eHubLicence.LA_LD;
			eHubIncident.IM_LCC = eHubLicence.ClientCompany.PK;

			var otherProductIncident = Factory.New<SupportIncident>();
			otherProductIncident.IM_OH_Client = otherProductLicence.Company.LC_OH;
			otherProductIncident.IM_OC_Contact = otherProductLicence.Company.Header.Contacts[0].PK;
			otherProductIncident.IM_LD = otherProductLicence.LA_LD;
			otherProductIncident.IM_Product = otherProductLicence.Database.LD_Product;

			var webIncident = Factory.New<SupportIncident>();
			webIncident.IM_OH_Client = orgWithoutLicence.PK;
			webIncident.IM_OC_Contact = orgWithoutLicence.Contacts[0].PK;
			webIncident.Request.INC_SystemCreateUser = User.WebUserCode;

			AssertType<ERequestEmailCustomerNotificationSender>(IncidentCustomerNotifierFactory.CreateSender(legacyEntIncident));
			AssertType<ERequestEHubCustomerNotificationSender>(IncidentCustomerNotifierFactory.CreateSender(eHubIncident));
			AssertType<EmailOnlyCustomerNotificationSender>(IncidentCustomerNotifierFactory.CreateSender(otherProductIncident));
			AssertType<WebRequestNotificationSender>(IncidentCustomerNotifierFactory.CreateSender(webIncident));
		}

		public void TestIInteractiveIncidentCustomerNotifierFactory()
		{
			var incident = Factory.New<SupportIncident>();
			var notifier = ObjectFactory.Get<IInteractiveIncidentCustomerNotifierFactory>()
				.CreateNotifier(incident, new EmailOnlyCustomerNotificationSender());
			AssertNotNull(notifier);
		}
	}
}
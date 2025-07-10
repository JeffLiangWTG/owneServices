using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DeviceManagement.Business.Test
{
	[TestedType(typeof(ClientTelRimRegistration))]
	public class ClientTelRimRegistrationTest : EnterpriseBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			var org = BillingTestHelper.CreateOrganisation(Factory, "DDD", "ABC", "SYD");
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 41243;
			clientCompany = ClientCompany.FindOrCreate(Factory, "ABC", db.PK, org.PK, "", "");
			clientDevice = Factory.New<ClientDeviceHeader>();
			clientDevice.CDH_Identifier = "1";
			orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			var clientTelRimRegistration = GetRegistration(clientCompany, clientDevice, orgCusCode, "01020304");
			Factory.Save();
			pk = clientTelRimRegistration.PK;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewClientTelRimRegistrationBusinessObject(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewClientTelRimRegistrationBusinessObject(factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewClientTelRimRegistrationBusinessObject(Factory);
		}

		BusinessObject GetNewClientTelRimRegistrationBusinessObject(BusinessObjectFactory factory)
		{
			var code = Factory.NewWithValidTestData<OrgCusCode>();
			var org = BillingTestHelper.CreateOrganisation(Factory, "DAB", "BAC", "MLB");
			var db = org.LicCompany.ActiveOrAllLicDatabases[0];
			db.LD_DatabaseNumber = 41244;
			var company = ClientCompany.FindOrCreate(Factory, "BAC", db.PK, org.PK, "", "");
			var device = Factory.New<ClientDeviceHeader>();
			device.CDH_Identifier = "2";

			return GetRegistration(company, device, code, "0102030405060708090A0B0C");
		}

		ClientTelRimRegistration GetRegistration(ClientCompany company, ClientDeviceHeader device, OrgCusCode code, string enrolmentId)
		{
			var clientTelRimRegistration = Factory.New<ClientTelRimRegistration>();
			clientTelRimRegistration.TRR_LCC_ClientCompany = company.PK;
			clientTelRimRegistration.TRR_CDH_ClientDeviceHeader = device.PK;
			clientTelRimRegistration.TRR_OK_OrgCusCode = code.PK;
			clientTelRimRegistration.TRR_EnrolmentId = enrolmentId;
			clientTelRimRegistration.TRR_EnrolmentScheme = "OSOM";
			clientTelRimRegistration.TRR_StartTime = ZDateTimeOffset.UtcNow;
			clientTelRimRegistration.TRR_VehicleIdentificationNumber = "12345678901234567";
			clientTelRimRegistration.TRR_VehicleRegistration = "ABC-123";
			clientTelRimRegistration.TRR_InstallationDateTimeOffset = ZDateTimeOffset.UtcNow;
			return clientTelRimRegistration;
		}

		ZGuid pk;
		ClientCompany clientCompany;
		ClientDeviceHeader clientDevice;
		OrgCusCode orgCusCode;

		public void TestBusinessObjectProperties()
		{
			// Arrange
			// Act
			var clientTelRimRegistration = Factory.LoadTop1<ClientTelRimRegistration>(new ZQuery(ClientTelRimRegistrationSchema.PK, pk));

			// Assert
			CombineAssertions(() =>
			{
				AssertEquals(clientCompany.PK, clientTelRimRegistration.ClientCompany.PK);
				AssertEquals(clientDevice.PK, clientTelRimRegistration.ClientDevice.PK);
				AssertEquals(orgCusCode.PK, clientTelRimRegistration.OrganisationCustomCodes.PK);
			});
		}
	}
}

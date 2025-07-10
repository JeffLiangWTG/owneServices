using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class LicenceDatabaseRegistrationImporterTest : TestCaseWithFactory
	{
		public void TestImportLicenceDatabase_NewDatabase()
		{
			var reg1 = new LicenceDatabaseRegistration() { Product = "ABU", SystemId = "C524897875544", SystemEndpointUrl = "https://www.somedomain/product/endpoints/id=C524897875544", LicenceType = "TST" };
			var reg2 = new LicenceDatabaseRegistration() { Product = "DHL", SystemId = "HW5gtGh8b6", SystemEndpointUrl = "https://www.somedomain/product/endpoints/id=HW5gtGh8b6" };

			var result1 = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(reg1);

			var email = Env.OutgoingMailManager.EmailsCreated.Single();
			AssertEquals("Licence Database Registration Successful", email.Subject);
			AssertEquals(@"
Product:ABU
SystemID:C524897875544
LicenceType:TST
SystemEndpointUrl:https://www.somedomain/product/endpoints/id=C524897875544

New Database Imported

You are receiving this email because you are a member of the group in registry WiseTech Global Client Extensions/My Account/Product Registration > Product Registration WebAPI Notification Group", email.Body);

			var result2 = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(reg2);
			AssertEquals(true, result1.Success);
			AssertEquals("New Database Imported", result1.OutputMessage);
			AssertEquals(true, result2.Success);
			AssertEquals("New Database Imported", result2.OutputMessage);

			var enterprise = new BusinessObjectFactory().LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, DefaultEnterpriseID));
			var database1 = enterprise.Databases.OfType<LicenceDatabase>().Single(x => x.LD_Product == "ABU");
			var database2 = enterprise.Databases.OfType<LicenceDatabase>().Single(x => x.LD_Product == "DHL");

			AssertEquals("C524897875544", database1.LD_TenantID);
			AssertEquals("https://www.somedomain/product/endpoints/id=C524897875544", database1.TrustedSystem.ETS_SystemEndpointUrl);
			AssertEquals("NON", database1.LD_Status);
			AssertEquals("CBB", database1.LD_ServerCode);
			AssertEquals(ZGuid.Empty, database1.LD_OH_WebAccessOrg);
			AssertEquals(false, database1.LD_AllowAutoLogin);
			AssertEquals(database1.LD_DatabaseNumber, result1.DatabaseNumber);
			AssertEquals("https://www.somedomain/product/endpoints/id=C524897875544", database1.TrustedSystem.ETS_SystemEndpointUrl);
			AssertEquals(@"Product:ABU
SystemID:C524897875544
LicenceType:TST
SystemEndpointUrl:https://www.somedomain/product/endpoints/id=C524897875544", database1.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description).Single().ST_NoteDataAsText);

			AssertEquals("HW5gtGh8b6", database2.LD_TenantID);
			AssertEquals("https://www.somedomain/product/endpoints/id=HW5gtGh8b6", database2.TrustedSystem.ETS_SystemEndpointUrl);
			AssertEquals("NON", database2.LD_Status);
			AssertEquals("CBC", database2.LD_ServerCode);
			AssertEquals(ZGuid.Empty, database2.LD_OH_WebAccessOrg);
			AssertEquals(false, database2.LD_AllowAutoLogin);
			AssertEquals(database2.LD_DatabaseNumber, result2.DatabaseNumber);
		}

		public void TestImportLicenceDatabase_Preregistered()
		{
			var db1 = DefaultEnterprise.Databases.AddNew();
			db1.LD_ServerCode = "DB1";
			db1.LD_Product = "ABU";
			db1.LD_TenantID = "C524897875544";
			db1.LD_Status = "PRE";
			db1.LD_PreRegistrationExpiryDateUTC = ZDateTime.UtcNow.AddDays(10);

			var db2 = DefaultEnterprise.Databases.AddNew();
			db2.LD_ServerCode = "DB2";
			db2.LD_Product = "DHL";
			db2.LD_TenantID = "HW5gtGh8b6";
			db2.LD_Status = "PRE";
			db2.LD_PreRegistrationExpiryDateUTC = ZDateTime.UtcNow.AddDays(-10);
			Factory.Save();

			var reg1 = new LicenceDatabaseRegistration() { Product = "ABU", SystemId = "C524897875544", SystemEndpointUrl = "https://www.somedomain/product/endpoints/id=C524897875544", LicenceType = "PRD" };
			var reg2 = new LicenceDatabaseRegistration() { Product = "DHL", SystemId = "HW5gtGh8b6", SystemEndpointUrl = "https://www.somedomain/product/endpoints/id=HW5gtGh8b6", LicenceType = "PRD" };

			var result1 = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(reg1);
			var result2 = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(reg2);
			AssertEquals(true, result1.Success);
			AssertEquals(null, result1.OutputMessage);
			AssertEquals(false, result2.Success);
			AssertEquals("Pre-Registration Expired", result2.OutputMessage);

			var enterprise = new BusinessObjectFactory().LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, DefaultEnterpriseID));
			var database1 = enterprise.Databases.OfType<LicenceDatabase>().Single(x => x.LD_Product == "ABU");
			var database2 = enterprise.Databases.OfType<LicenceDatabase>().Single(x => x.LD_Product == "DHL");

			AssertEquals("C524897875544", database1.LD_TenantID);
			AssertEquals("https://www.somedomain/product/endpoints/id=C524897875544", database1.TrustedSystem.ETS_SystemEndpointUrl);
			AssertEquals("REG", database1.LD_Status);

			AssertEquals("HW5gtGh8b6", database2.LD_TenantID);
			AssertEquals("", database2.TrustedSystem?.ETS_SystemEndpointUrl ?? "");
			AssertEquals("PRE", database2.LD_Status);

			var email = Env.OutgoingMailManager.EmailsCreated.Single();
			AssertEquals("Licence Database Registration Failed", email.Subject);
			AssertEquals(@"
Product:DHL
SystemID:HW5gtGh8b6
LicenceType:PRD
SystemEndpointUrl:https://www.somedomain/product/endpoints/id=HW5gtGh8b6

Pre-Registration Expired

You are receiving this email because you are a member of the group in registry WiseTech Global Client Extensions/My Account/Product Registration > Product Registration WebAPI Notification Group", email.Body);
		}

		public void TestImportLicenceDatabase_NotRegistered()
		{
			var db1 = DefaultEnterprise.Databases.AddNew();
			db1.LD_ServerCode = "DB1";
			db1.LD_Product = "ABU";
			db1.LD_TenantID = "C524897875544";
			db1.LD_Status = "NON";
			Factory.Save();

			var reg1 = new LicenceDatabaseRegistration() { Product = "ABU", SystemId = "C524897875544", SystemEndpointUrl = "https://www.somedomain/product/endpoints/id=C524897875544" };

			var result1 = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(reg1);
			AssertEquals(true, result1.Success);
			AssertEquals(null, result1.OutputMessage);

			var enterprise = new BusinessObjectFactory().LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, DefaultEnterpriseID));
			var database1 = enterprise.Databases.OfType<LicenceDatabase>().Single(x => x.LD_Product == "ABU");

			AssertEquals("C524897875544", database1.LD_TenantID);
			AssertEquals("https://www.somedomain/product/endpoints/id=C524897875544", database1.TrustedSystem.ETS_SystemEndpointUrl);
			AssertEquals("NON", database1.LD_Status);
		}

		public void TestImportLicenceDatabase_RegisteredAlready()
		{
			var db1 = DefaultEnterprise.Databases.AddNew();
			db1.LD_ServerCode = "DB1";
			db1.LD_Product = "ABU";
			db1.LD_TenantID = "C524897875544";
			db1.LD_Status = "REG";
			Factory.Save();

			var reg1 = new LicenceDatabaseRegistration() { Product = "ABU", SystemId = "C524897875544", SystemEndpointUrl = "https://www.somedomain/product/endpoints/id=C524897875544", LicenceType = "PRD" };

			var result1 = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(reg1);
			AssertEquals(false, result1.Success);
			AssertEquals("Database Registered Already", result1.OutputMessage);

			var enterprise = new BusinessObjectFactory().LoadTop1<LicenceEnterprise>(new ZQuery(LicenceEnterpriseSchema.LE_EnterpriseID, DefaultEnterpriseID));
			var database1 = enterprise.Databases.OfType<LicenceDatabase>().Single(x => x.LD_Product == "ABU");

			AssertEquals("C524897875544", database1.LD_TenantID);
			AssertEquals("", database1.GetOrCreateTrustedSystem().ETS_SystemEndpointUrl);
			AssertEquals("REG", database1.LD_Status);

			var email = Env.OutgoingMailManager.EmailsCreated.Single();
			AssertEquals("Licence Database Registration Failed", email.Subject);
			AssertEquals(@"
Product:ABU
SystemID:C524897875544
LicenceType:PRD
SystemEndpointUrl:https://www.somedomain/product/endpoints/id=C524897875544

Database Registered Already

You are receiving this email because you are a member of the group in registry WiseTech Global Client Extensions/My Account/Product Registration > Product Registration WebAPI Notification Group", email.Body);
		}

		public void TestImportLicenceDatabase_LicenceType()
		{
			var db1 = DefaultEnterprise.Databases.AddNew();
			db1.LD_ServerCode = "DB1";
			db1.LD_Product = "ABU";
			db1.LD_TenantID = "C524897875544";
			db1.LD_Status = "NON";
			Factory.Save();

			var reg1 = new LicenceDatabaseRegistration() { Product = "ABU", SystemId = "C524897875544", SystemEndpointUrl = "https://www.somedomain/product/endpoints/id=C524897875544", LicenceType = "XXX" };
			var result1 = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(reg1);
			AssertEquals(false, result1.Success);
			AssertEquals("Invalid Licence Type", result1.OutputMessage);

			var reg2 = new LicenceDatabaseRegistration() { Product = "ABU", SystemId = "C524897875544", SystemEndpointUrl = "https://www.somedomain/product/endpoints/id=C524897875544", LicenceType = "TST" };
			var result2 = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(reg2);
			AssertEquals(true, result2.Success);
			AssertEquals(null, result2.OutputMessage);
		}

		public void TestImportLicenceDatabase_TenantRegistrationInfo()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "TST";
			enterprise.LE_OH = org.PK;

			var trustedSystem = Factory.New<EdiTrustedSystem>();
			trustedSystem.ETS_Product = "CSP";
			trustedSystem.ETS_SystemID = "Production";
			Factory.Save();

			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enterprise.LE_EnterpriseID);

			var info = new TenantRegistrationInfo()
			{
				Product = trustedSystem.ETS_Product,
				SystemId = trustedSystem.ETS_SystemID,
				TenantId = "DEMO",
				InfoExpires = DateTime.UtcNow.AddMinutes(10),
				LicenceType = "PRD",
				Address1 = "Addr 1",
				Address2 = "Addr 2",
				BusinessRegoNo = "ABN #123",
				City = "Sydney",
				OrgCountry = "AU",
				OrgName = "Org Name 123",
				Postcode = "2000",
				State = "NSW",
				EnterpriseID = "enterprise_id_1",
				EnterpriseCode = "enterprise_code_2",
				ServerCode = "server_code_3",
				CargowiseCompanyCode = "cargowise_company_code_4",
				DatabaseNumber = "",
			};

			var result = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(info);
			Assert(result.Success);

			var database = enterprise.Databases.OfType<LicenceDatabase>().Single(x => x.LD_Product == "CSP");
			AssertEquals(trustedSystem.PK, database.LD_ETS_TrustedSystem);
			AssertEquals("DEMO", database.LD_TenantID);
			AssertEquals("NON", database.LD_Status);
			AssertEquals("CBB", database.LD_ServerCode);
			AssertEquals(ZGuid.Empty, database.LD_OH_WebAccessOrg);
			AssertEquals(false, database.LD_AllowAutoLogin);
			AssertEquals(database.LD_DatabaseNumber, result.DatabaseNumber);

			var expectedNoteText =
@"Product:CSP
SystemID:Production
TenantID:DEMO
LicenceType:PRD
OrgName:Org Name 123
OrgCountry:AU
Address1:Addr 1
Address2:Addr 2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:ABN #123
EnterpriseID:enterprise_id_1
EnterpriseCode:enterprise_code_2
ServerCode:server_code_3
CargowiseCompanyCode:cargowise_company_code_4
DatabaseNumber:";
			AssertEquals(expectedNoteText, database.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description).Single().ST_NoteDataAsText);

			var email = Env.OutgoingMailManager.EmailsCreated.Single();
			AssertEquals("Licence Database Registration Successful", email.Subject);
			AssertEquals(
@"
Product:CSP
SystemID:Production
TenantID:DEMO
LicenceType:PRD
OrgName:Org Name 123
OrgCountry:AU
Address1:Addr 1
Address2:Addr 2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:ABN #123
EnterpriseID:enterprise_id_1
EnterpriseCode:enterprise_code_2
ServerCode:server_code_3
CargowiseCompanyCode:cargowise_company_code_4
DatabaseNumber:

New Database Imported

You are receiving this email because you are a member of the group in registry WiseTech Global Client Extensions/My Account/Product Registration > Product Registration WebAPI Notification Group", email.Body);
		}

		public void TestImportLicenceDatabase_TenantRegistrationInfo_ExistingNonregisteredDatabase()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "TST";
			enterprise.LE_OH = org.PK;

			var db1 = enterprise.Databases.AddNew();
			db1.LD_ServerCode = "DB1";
			db1.LD_Product = "CSP";
			db1.LD_TenantID = "DEMO";
			db1.LD_Status = DatabaseStatusList.Codes.NON;
			db1.LD_PreRegistrationExpiryDateUTC = ZDateTime.UtcNow.AddDays(10);
			Factory.Save();

			var trustedSystem = Factory.New<EdiTrustedSystem>();
			trustedSystem.ETS_Product = "CSP";
			trustedSystem.ETS_SystemID = "Production";
			Factory.Save();

			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enterprise.LE_EnterpriseID);

			var info = new TenantRegistrationInfo()
			{
				Product = trustedSystem.ETS_Product,
				SystemId = trustedSystem.ETS_SystemID,
				TenantId = "DEMO",
				InfoExpires = DateTime.UtcNow.AddMinutes(10),
				LicenceType = "PRD",
				Address1 = "Addr 1",
				Address2 = "Addr 2",
				BusinessRegoNo = "ABN #123",
				City = "Sydney",
				OrgCountry = "AU",
				OrgName = "Org Name 123",
				Postcode = "2000",
				State = "NSW",
				EnterpriseID = "enterprise_id_1",
				EnterpriseCode = "enterprise_code_2",
				ServerCode = "server_code_3",
				CargowiseCompanyCode = "cargowise_company_code_4",
				DatabaseNumber = "",
			};

			var result = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(info);
			Assert(result.Success);

			var database = enterprise.Databases.OfType<LicenceDatabase>().Single(x => x.LD_Product == "CSP");
			AssertEquals("Should not enter a tenant ID for S2ST registrations", "DEMO", database.LD_TenantID);
			AssertEquals("Should remain as non-registered", DatabaseStatusList.Codes.NON, database.LD_Status);
			AssertEquals(database.LD_DatabaseNumber, result.DatabaseNumber);

			var expectedNoteText =
@"Product:CSP
SystemID:Production
TenantID:DEMO
LicenceType:PRD
OrgName:Org Name 123
OrgCountry:AU
Address1:Addr 1
Address2:Addr 2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:ABN #123
EnterpriseID:enterprise_id_1
EnterpriseCode:enterprise_code_2
ServerCode:server_code_3
CargowiseCompanyCode:cargowise_company_code_4
DatabaseNumber:";
			AssertEquals(expectedNoteText, database.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description).Single().ST_NoteDataAsText);
		}

		public void TestImportLicenceDatabase_SystemToSystemTenantRegistrationInfo()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "TST";
			enterprise.LE_OH = org.PK;
			Factory.Save();

			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enterprise.LE_EnterpriseID);

			var info = new SystemToSystemTrustTenantRegistrationInfo()
			{
				Product = "CSP",
				TenantId = "BELLO",
				InfoTimestamp = DateTime.UtcNow,
				LicenceType = "PRD",
				Address1 = "Addr 1",
				Address2 = "Addr 2",
				BusinessRegoNo = "ABN #123",
				City = "Sydney",
				OrgCountry = "AU",
				OrgName = "Org Name 123",
				Postcode = "2000",
				State = "NSW",
				EnterpriseID = "enterprise_id_1",
				EnterpriseCode = "enterprise_code_2",
				ServerCode = "server_code_3",
				CargowiseCompanyCode = "cargowise_company_code_4",
				DatabaseNumber = "",
			};

			var result = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(info, null);
			Assert(result.Success);
			AssertEquals("New Database Imported", result.OutputMessage);

			var database = enterprise.Databases.OfType<LicenceDatabase>().Single(x => x.LD_Product == "CSP");
			AssertEquals("Tenant ID should match import", info.TenantId, database.LD_TenantID);
			AssertEquals("NON", database.LD_Status);
			AssertEquals("CBB", database.LD_ServerCode);
			AssertEquals(ZGuid.Empty, database.LD_OH_WebAccessOrg);
			AssertEquals(false, database.LD_AllowAutoLogin);
			AssertEquals(database.LD_DatabaseNumber, result.DatabaseNumber);

			var expectedNoteText =
@"Product:CSP
TenantId:BELLO
LicenceType:PRD
OrgName:Org Name 123
OrgCountry:AU
Address1:Addr 1
Address2:Addr 2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:ABN #123
EnterpriseID:enterprise_id_1
EnterpriseCode:enterprise_code_2
ServerCode:server_code_3
CargowiseCompanyCode:cargowise_company_code_4
DatabaseNumber:";
			AssertEquals(expectedNoteText, database.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description).Single().ST_NoteDataAsText);

			var email = Env.OutgoingMailManager.EmailsCreated.Single();
			AssertEquals("Licence Database Registration Successful", email.Subject);
			AssertEquals(
@"
Product:CSP
TenantId:BELLO
LicenceType:PRD
OrgName:Org Name 123
OrgCountry:AU
Address1:Addr 1
Address2:Addr 2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:ABN #123
EnterpriseID:enterprise_id_1
EnterpriseCode:enterprise_code_2
ServerCode:server_code_3
CargowiseCompanyCode:cargowise_company_code_4
DatabaseNumber:

New Database Imported

You are receiving this email because you are a member of the group in registry WiseTech Global Client Extensions/My Account/Product Registration > Product Registration WebAPI Notification Group", email.Body);
		}

		public void TestImportLicenceDatabase_SystemToSystemTenantRegistrationInfo_ExistingPreregisteredDatabase()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "TST";
			enterprise.LE_OH = org.PK;

			var db1 = enterprise.Databases.AddNew();
			db1.LD_ServerCode = "DB1";
			db1.LD_Product = "CSP";
			db1.LD_TenantID = string.Empty;
			db1.LD_Status = DatabaseStatusList.Codes.Preregistered;
			db1.LD_PreRegistrationExpiryDateUTC = ZDateTime.UtcNow.AddDays(10);
			Factory.Save();

			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enterprise.LE_EnterpriseID);

			var info = new SystemToSystemTrustTenantRegistrationInfo()
			{
				Product = "CSP",
				TenantId = "BELLO",
				InfoTimestamp = DateTime.UtcNow,
				LicenceType = "PRD",
				Address1 = "Addr 1",
				Address2 = "Addr 2",
				BusinessRegoNo = "ABN #123",
				City = "Sydney",
				OrgCountry = "AU",
				OrgName = "Org Name 123",
				Postcode = "2000",
				State = "NSW",
				EnterpriseID = "enterprise_id_1",
				EnterpriseCode = "enterprise_code_2",
				ServerCode = "server_code_3",
				CargowiseCompanyCode = "cargowise_company_code_4",
				DatabaseNumber = "",
			};

			var result = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(info, db1);
			Assert(result.Success);

			var database = new BusinessObjectFactory().Load<LicenceDatabase>(db1.PK);
			AssertEquals("Should not enter a tenant ID for S2ST registrations", string.Empty, database.LD_TenantID);
			AssertEquals("Should have registered", DatabaseStatusList.Codes.REG, database.LD_Status);
			AssertEquals(database.LD_DatabaseNumber, result.DatabaseNumber);

			var expectedNoteText =
@"Product:CSP
TenantId:BELLO
LicenceType:PRD
OrgName:Org Name 123
OrgCountry:AU
Address1:Addr 1
Address2:Addr 2
City:Sydney
Postcode:2000
State:NSW
BusinessRegoNo:ABN #123
EnterpriseID:enterprise_id_1
EnterpriseCode:enterprise_code_2
ServerCode:server_code_3
CargowiseCompanyCode:cargowise_company_code_4
DatabaseNumber:";
			AssertEquals(expectedNoteText, database.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description).Single().ST_NoteDataAsText);
		}

		public void TestImportLicenceDatabase_SystemToSystemTenantRegistrationInfo_ExistingAlreadyRegisteredDatabase()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "TST";
			enterprise.LE_OH = org.PK;

			var db1 = enterprise.Databases.AddNew();
			db1.LD_ServerCode = "DB1";
			db1.LD_Product = "CSP";
			db1.LD_TenantID = string.Empty;
			db1.LD_Status = DatabaseStatusList.Codes.REG;
			db1.LD_PreRegistrationExpiryDateUTC = ZDateTime.UtcNow.AddDays(10);
			Factory.Save();

			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enterprise.LE_EnterpriseID);

			var info = new SystemToSystemTrustTenantRegistrationInfo()
			{
				Product = "CSP",
				TenantId = "BELLO",
				InfoTimestamp = DateTime.UtcNow,
				LicenceType = "PRD",
				Address1 = "Addr 1",
				Address2 = "Addr 2",
				BusinessRegoNo = "ABN #123",
				City = "Sydney",
				OrgCountry = "AU",
				OrgName = "Org Name 123",
				Postcode = "2000",
				State = "NSW",
				EnterpriseID = "enterprise_id_1",
				EnterpriseCode = "enterprise_code_2",
				ServerCode = "server_code_3",
				CargowiseCompanyCode = "cargowise_company_code_4",
				DatabaseNumber = "",
			};

			var result = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(info, db1);
			Assert(!result.Success);

			var database = enterprise.Databases.OfType<LicenceDatabase>().Single(x => x.LD_Product == "CSP");
			AssertEquals("Should not enter a tenant ID for S2ST registrations", string.Empty, database.LD_TenantID);
			AssertEquals("Should remain registered", DatabaseStatusList.Codes.REG, database.LD_Status);
			AssertEquals(0, result.DatabaseNumber);
			AssertEquals("Should return database already registered message", "Database Registered Already", result.OutputMessage);
			AssertEquals("Should not have any notes", 0, database.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description).Length);
		}

		public void TestImportLicenceDatabase_SystemToSystemTenantRegistrationInfo_MissingProductRegistrationWebAPIDefaultEnterpriseID()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "TST";
			enterprise.LE_OH = org.PK;
			Factory.Save();

			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			var info = new SystemToSystemTrustTenantRegistrationInfo()
			{
				Product = "CSP",
				TenantId = "BELLO",
				InfoTimestamp = DateTime.UtcNow,
				LicenceType = "PRD",
				Address1 = "Addr 1",
				Address2 = "Addr 2",
				BusinessRegoNo = "ABN #123",
				City = "Sydney",
				OrgCountry = "AU",
				OrgName = "Org Name 123",
				Postcode = "2000",
				State = "NSW",
				EnterpriseID = "enterprise_id_1",
				EnterpriseCode = "enterprise_code_2",
				ServerCode = "server_code_3",
				CargowiseCompanyCode = "cargowise_company_code_4",
				DatabaseNumber = "",
			};

			var result = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(info, null);
			Assert(!result.Success);

			AssertEquals("Should not create a database", false, enterprise.Databases.OfType<LicenceDatabase>().Any(x => x.LD_Product == "CSP"));
			AssertEquals(0, result.DatabaseNumber);
			AssertEquals("Should return invalid id message", "Invalid Enterprise ID", result.OutputMessage);
		}

		public void TestImportLicenceDatabase_SystemToSystemTenantRegistrationInfo_InvalidProductRegistrationWebAPIDefaultEnterpriseID()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "TST";
			enterprise.LE_OH = org.PK;
			Factory.Save();

			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "hello");

			var info = new SystemToSystemTrustTenantRegistrationInfo()
			{
				Product = "CSP",
				TenantId = "BELLO",
				InfoTimestamp = DateTime.UtcNow,
				LicenceType = "PRD",
				Address1 = "Addr 1",
				Address2 = "Addr 2",
				BusinessRegoNo = "ABN #123",
				City = "Sydney",
				OrgCountry = "AU",
				OrgName = "Org Name 123",
				Postcode = "2000",
				State = "NSW",
				EnterpriseID = "enterprise_id_1",
				EnterpriseCode = "enterprise_code_2",
				ServerCode = "server_code_3",
				CargowiseCompanyCode = "cargowise_company_code_4",
				DatabaseNumber = "",
			};

			var result = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(info, null);
			Assert(!result.Success);

			AssertEquals("Should not create a database", false, enterprise.Databases.OfType<LicenceDatabase>().Any(x => x.LD_Product == "CSP"));
			AssertEquals(0, result.DatabaseNumber);
			AssertEquals("Should return invalid id message", "Invalid Enterprise ID", result.OutputMessage);
		}

		public void TestImportLicenceDatabase_SystemToSystemTenantRegistrationInfo_MissingTenantId()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "TST";
			enterprise.LE_OH = org.PK;
			Factory.Save();

			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enterprise.LE_EnterpriseID);

			var info = new SystemToSystemTrustTenantRegistrationInfo()
			{
				Product = "CSP",
				InfoTimestamp = DateTime.UtcNow,
				LicenceType = "PRD",
				Address1 = "Addr 1",
				Address2 = "Addr 2",
				BusinessRegoNo = "ABN #123",
				City = "Sydney",
				OrgCountry = "AU",
				OrgName = "Org Name 123",
				Postcode = "2000",
				State = "NSW",
				EnterpriseID = "enterprise_id_1",
				EnterpriseCode = "enterprise_code_2",
				ServerCode = "server_code_3",
				CargowiseCompanyCode = "cargowise_company_code_4",
				DatabaseNumber = "",
			};

			var result = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(info, null);
			Assert(!result.Success);

			AssertEquals("Should not create a database", false, enterprise.Databases.OfType<LicenceDatabase>().Any(x => x.LD_Product == "CSP"));
			AssertEquals(0, result.DatabaseNumber);
			AssertEquals("Should return missing tenant id message", "Missing Tenant ID", result.OutputMessage);
		}

		public void TestImportLicenceDatabase_SystemToSystemTenantRegistrationInfo_ReRegistration()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "TST";
			enterprise.LE_OH = org.PK;

			var db1 = enterprise.Databases.AddNew();
			db1.LD_ServerCode = "DB1";
			db1.LD_Product = "CSP";
			db1.LD_TenantID = "DFNE";
			db1.LD_Status = DatabaseStatusList.Codes.REG;
			db1.LD_PreRegistrationExpiryDateUTC = ZDateTime.UtcNow.AddDays(10);
			Factory.Save();

			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enterprise.LE_EnterpriseID);

			var info = new SystemToSystemTrustTenantRegistrationInfo()
			{
				Product = "CSP",
				TenantId = "DFNE",
				InfoTimestamp = DateTime.UtcNow,
				LicenceType = "PRD",
				Address1 = "Addr 1",
				Address2 = "Addr 2",
				BusinessRegoNo = "ABN #123",
				City = "Sydney",
				OrgCountry = "AU",
				OrgName = "Org Name 123",
				Postcode = "2000",
				State = "NSW",
				EnterpriseID = "enterprise_id_1",
				EnterpriseCode = "enterprise_code_2",
				ServerCode = "server_code_3",
				CargowiseCompanyCode = "cargowise_company_code_4",
				DatabaseNumber = "",
			};

			var result = LicenceDatabaseRegistrationImporter.TryImportLicenceDatabase(info, null);
			Assert("Should return success when someone tries to register a second time", result.Success);
			AssertEquals("Should return existing licence database number", db1.LD_DatabaseNumber, result.DatabaseNumber);

			var database = enterprise.Databases.OfType<LicenceDatabase>().Single(x => x.LD_Product == "CSP");
			AssertEquals("Tenant ID should remain the same", "DFNE", database.LD_TenantID);
			AssertEquals("Should remain registered", DatabaseStatusList.Codes.REG, database.LD_Status);
			AssertEquals("No message required", true, string.IsNullOrEmpty(result.OutputMessage));
			AssertEquals("Should not add any notes", 0, database.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description).Length);
		}

		ZString DefaultEnterpriseID;
		LicenceEnterprise DefaultEnterprise;

		protected override void SetUp()
		{
			base.SetUp();

			var licHeader = BillingTestHelper.CreateLicence(Factory, "ENT");
			DefaultEnterprise = licHeader.Database.LicEnterprise;
			DefaultEnterpriseID = licHeader.Database.EnterpriseID;

			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DefaultEnterpriseID);

			var staff1 = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff1.GS_Code = "SO1";
			staff1.GS_FullName = "Staff One";
			staff1.GS_EmailAddress = "newemail1@wisetechglobal.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff1);
			EDIDataRegistry.Instance.ProductRegistrationWebAPINotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Factory.Save();
		}
	}
}

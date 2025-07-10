using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[HttpContextEnabledTest]
	public class LicenceRegistrationTrustedSystemBaseControllerTest : TestCaseWithFactory
	{
		public void TestRegisterTenant()
		{
			var logger = new NLogWrapperForTest(GetType());

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

			var controller = CreateController(logger);
			var context = new TrustedContextForTest<TenantRegistrationInfo, ProductRegistrationResponse>
					(info.Product, info.SystemId, info, trustedSystem, controller)
			{ Success = true };
			controller.RegisterTenantCore_Exposed(context);
			AssertNotNull(context.ResponseInfo);

			var query = new ZQuery(LicenceDatabaseSchema.LD_ETS_TrustedSystem, trustedSystem.PK);
			query.AddToFilter(new ZQuery(LicenceDatabaseSchema.LD_TenantID, "DEMO"));
			var db = Factory.LoadTop1<LicenceDatabase>(query);

			var data = context.ResponseInfo;
			AssertEquals(db.LD_DatabaseNumber, data.DatabaseNumber);

			AssertNotNull(db);
			AssertEquals("DEMO", db.LD_TenantID);
			AssertEquals("PRD", db.LD_LicenceType);

			var notes = db.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description).Single();
			AssertEquals(
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
DatabaseNumber:", notes.ST_NoteDataAsText);
		}

		public void TestRegisterTenant_ExistingTenant()
		{
			var logger = new NLogWrapperForTest(GetType());

			var trustedSystem = Factory.New<EdiTrustedSystem>();
			trustedSystem.ETS_Product = "CSP";
			trustedSystem.ETS_SystemID = "Production";
			Factory.Save();

			var info = new TenantRegistrationInfo()
			{
				Product = trustedSystem.ETS_Product,
				SystemId = trustedSystem.ETS_SystemID,
				TenantId = "DEMO",
				InfoExpires = DateTime.UtcNow.AddMinutes(10),
				LicenceType = "PRD",
				OrgName = "Org Name 123",
			};

			var controller = CreateController(logger);
			var context = new TrustedContextForTest<TenantRegistrationInfo, ProductRegistrationResponse>
					(info.Product, info.SystemId, info, trustedSystem, controller)
			{ Success = true };
			controller.RegisterTenantCore_Exposed(context);
			AssertNull(context.ResponseInfo);
			AssertEquals("2003", context.Messages.Messages[0].Code);
			AssertEquals("No Country or Licencing Info", context.Messages.Messages[0].Message);
		}

		public void TestRegisterTenant_Validation()
		{
			var logger = new NLogWrapperForTest(GetType());

			var org = Factory.NewWithValidTestData<OrgHeader>();

			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "TST";
			enterprise.LE_OH = org.PK;

			var trustedSystem = Factory.New<EdiTrustedSystem>();
			trustedSystem.ETS_Product = "CSP";
			trustedSystem.ETS_SystemID = "Production";

			var existingDb = Factory.NewWithValidTestData<LicenceDatabase>();
			existingDb.LD_ETS_TrustedSystem = trustedSystem.PK;
			existingDb.LD_TenantID = "DEMO";

			Factory.Save();

			EDIDataRegistry.Instance.ProductRegistrationWebAPIDefaultEnterpriseID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, enterprise.LE_EnterpriseID);

			// Blank tenant_id
			var info = new TenantRegistrationInfo()
			{
				Product = trustedSystem.ETS_Product,
				SystemId = trustedSystem.ETS_SystemID,
				OrgName = "Demo Company",
				LicenceType = "PRD",
				OrgCountry = "AU",
				InfoExpires = DateTime.UtcNow.AddMinutes(10),
			};
			var controller = CreateController(logger);
			var context = new TrustedContextForTest<TenantRegistrationInfo, ProductRegistrationResponse>
					(info.Product, info.SystemId, info, trustedSystem, controller)
			{ Success = true };
			controller.RegisterTenantCore_Exposed(context);
			AssertNull(context.ResponseInfo);
			AssertEquals("2003", context.Messages.Messages[0].Code);
			AssertEquals("The tenant_id is blank", context.Messages.Messages[0].Message);

			// No country
			info.OrgCountry = string.Empty;
			info.TenantId = "DEMO";
			context.Messages = null;
			controller.RegisterTenantCore_Exposed(context);
			AssertNull(context.ResponseInfo);
			AssertEquals("2003", context.Messages.Messages[0].Code);
			AssertEquals("No Country or Licencing Info", context.Messages.Messages[0].Message);

			// Tenant_id has registered
			info.OrgCountry = "AU";
			context.Messages = null;
			controller.RegisterTenantCore_Exposed(context);
			AssertNull(context.ResponseInfo);
			AssertEquals("2003", context.Messages.Messages[0].Code);
			AssertEquals("The tenant_id has been already registered", context.Messages.Messages[0].Message);

			// Should pass validation
			info.TenantId = "Test";
			info.OrgCountry = string.Empty;
			info.EnterpriseCode = "DDD";
			info.CargowiseCompanyCode = "ABC";
			info.ServerCode = "SYD";
			context.Messages = null;
			controller.RegisterTenantCore_Exposed(context);
			AssertNotNull(context.ResponseInfo);
			AssertNull(context.Messages);
		}

		public void TestRegisterTenant_DuplicateDatabase()
		{
			var logger = new NLogWrapperForTest(GetType());

			var licence1 = BillingTestHelper.CreateLicence(Factory, "EN1", "CM1", "DB1");
			var db1 = licence1.Database;
			db1.LD_TenantID = "";
			db1.LD_DatabaseNumber = 7000;
			db1.LD_Product = "CSP";
			db1.WebAccessOrg.OH_FullName = "Org Name 123";
			db1.WebAccessOrg.MainAddress.OA_RN_NKCountryCode = "AU";

			var trustedSystem = Factory.New<EdiTrustedSystem>();
			trustedSystem.ETS_Product = "CSP";
			trustedSystem.ETS_SystemID = "Production";
			Factory.Save();

			var info = new TenantRegistrationInfo()
			{
				Product = trustedSystem.ETS_Product,
				SystemId = trustedSystem.ETS_SystemID,
				TenantId = "DEMO",
				InfoExpires = DateTime.UtcNow.AddMinutes(10),
				LicenceType = "PRD",
				OrgName = "Org Name 123",
				OrgCountry = "AU",
			};

			var controller = CreateController(logger);
			var context = new TrustedContextForTest<TenantRegistrationInfo, ProductRegistrationResponse>
					(info.Product, info.SystemId, info, trustedSystem, controller)
			{ Success = true };
			controller.RegisterTenantCore_Exposed(context);
			AssertNotNull(context.ResponseInfo);

			var newFactory = new BusinessObjectFactory();
			var db1InNewFactory = newFactory.Load<LicenceDatabase>(db1.PK);
			AssertEquals("DEMO", db1InNewFactory.LD_TenantID);
			AssertEquals(trustedSystem.PK, db1InNewFactory.LD_ETS_TrustedSystem);

			var data = context.ResponseInfo;
			AssertEquals(db1InNewFactory.LD_DatabaseNumber, data.DatabaseNumber);

			var notes = db1InNewFactory.Notes.FindByDescription(EDIPredefinedNoteTypes.Instance.LicenceDatabaseRegistrationImportNote.Description).Single();
			AssertEquals(
@"Product:CSP
SystemID:Production
TenantID:DEMO
LicenceType:PRD
OrgName:Org Name 123
OrgCountry:AU
Address1:
Address2:
City:
Postcode:
State:
BusinessRegoNo:
EnterpriseID:
EnterpriseCode:
ServerCode:
CargowiseCompanyCode:
DatabaseNumber:", notes.ST_NoteDataAsText);
		}

		LicenceRegistrationTrustedSystemBaseControllerForTest CreateController(NLogWrapper logger)
		{
			var requestMessage = new HttpRequestMessage(HttpMethod.Post, "http://unit-testing/api/registration");
			requestMessage.Content = new StringContent("{ }", Encoding.UTF8, "application/json");
			var controller = new LicenceRegistrationTrustedSystemBaseControllerForTest(logger);
			controller.Request = requestMessage;
			return controller;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var product = "ABU";
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var db = licence.Database;
			db.LD_TenantID = "SYS0399481";
			db.LD_DatabaseNumber = 8000;
			db.LD_Product = product;
			db.GetOrCreateTrustedSystem().ETS_SystemEndpointUrl = "";
			db.GetOrCreateTrustedSystem().ETS_SystemID = "Production";

			var staff1 = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff1.GS_Code = "SO1";
			staff1.GS_FullName = "Staff One";
			staff1.GS_EmailAddress = "newemail1@wisetechglobal.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(staff1);
			EDIDataRegistry.Instance.ProductRegistrationWebAPINotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			Factory.Save();
		}

		public class LicenceRegistrationTrustedSystemBaseControllerForTest : LicenceRegistrationTrustedSystemBaseController
		{
			public LicenceRegistrationTrustedSystemBaseControllerForTest() : base()
			{
			}

			public LicenceRegistrationTrustedSystemBaseControllerForTest(NLogWrapper logger) : base(logger)
			{
			}

			public void RegisterTenantCore_Exposed(TrustedContext<TenantRegistrationInfo, ProductRegistrationResponse> context)
			{
				RegisterTenantCore(context);
			}
		}
	}
}

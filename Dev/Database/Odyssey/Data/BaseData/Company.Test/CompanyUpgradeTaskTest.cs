using System;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Data.BaseData.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.BaseData.Company
{
	sealed class CompanyUpgradeTaskTest : TransactionedTestCase
	{
		public void TestRun()
		{
			// Prepare test data
			Guid newCompanyPk = Guid.NewGuid();
			Guid newBranchPk = Guid.NewGuid();
			Guid newGroupPk = Guid.NewGuid();
			Guid newStaffPk = Guid.NewGuid();
			Guid newGroupLinkPk = Guid.NewGuid();
			Guid demCompanyPk = new Guid("03052ed3-2c64-49ac-97d8-c6079d5015b5");
			Guid demBranchPk = new Guid("2fdba7fb-60ba-4a03-8336-0defac4f9673");
			Guid allGroupPk = new Guid("94755e71-a87a-4034-8dfa-785773a49607");
			Guid adminStaffPk = new Guid("4d001790-4a73-43fb-9393-3786fb8bcf84");
			Guid postmasterStaffPk = new Guid("82592349-F1FB-4159-8E00-91AC33D8FAAA");

			string sqlText = String.Format(@"
				-- GlbCompany
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES ('{0}', '~C1', 'AU company', 'AUD', 'AU');
				UPDATE dbo.GlbCompany SET GC_Name = 'Some~Name~Not~To~Be~Changed', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = '{1}';
				-- GlbBranch
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES ('{2}', '~B1', '{0}');
				UPDATE dbo.GlbBranch SET GB_BranchName = 'Some~Name~Not~To~Be~Changed', GB_SystemLastEditUser = 'E', GB_SystemLastEditTimeUtc = GetDate() WHERE GB_PK = '{3}';
				-- GlbGroup
				INSERT dbo.GlbGroup (GG_PK, GG_Code) VALUES ('{4}', '~Grupo1');
				UPDATE dbo.GlbGroup SET GG_Desc = 'Some~Name~Not~To~Be~Changed', GG_SystemLastEditUser = 'E', GG_SystemLastEditTimeUtc = GetDate() WHERE GG_PK = '{5}';
				-- GlbStaff
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{6}', '~S1', GetUtcDate(), '~BP', GetUtcDate(), '~BP');
				DELETE dbo.GlbStaff WHERE GS_Code = '~BP';
				-- GlbGroupLink
				INSERT dbo.GlbGroupLink (GK_PK, GK_GG, GK_GS, GK_SystemLastEditTimeUtc, GK_SystemLastEditUser) VALUES ('{7}', '{4}', '{6}', GetUtcDate(), '~BP');
				DELETE dbo.GlbGroupLink WHERE GK_GG = '{5}' AND GK_GS = '{8}';
				UPDATE dbo.GlbGroupLink SET GK_GS = '{8}', GK_SystemLastEditTimeUtc = GetUtcDate(), GK_SystemLastEditUser = '~BP' WHERE GK_GG = '{5}' AND GK_GS = '{9}';
				-- GlbGroupRole
				DELETE dbo.GlbGroupRole WHERE GGR_GG_Group = 'a99e7f0e-8379-4f50-8560-9b4bb804c0de' AND GGR_RoleName = 'db_datawriter';
				DELETE dbo.GlbGroupRole WHERE GGR_GG_Group = '6f0eb310-fc5c-4696-9594-f8ce156542c6' AND GGR_RoleName = 'cwRestrictedReaderRole';
				DELETE dbo.GlbGroupRole WHERE GGR_GG_Group = '208068b6-3383-44bf-8e0d-dbd827f9d675' AND GGR_RoleName = 'db_backupoperator';
				-- NEO Groups
				DELETE GU
				FROM dbo.GlbSecurity GU
				JOIN dbo.GlbGroup ON GG_PK = GU_GG
				WHERE GG_Code IN ('DOC_ALL_ACV','NEOROLES','RepCustoms00001');
				DELETE GGR
				FROM dbo.GlbGroupRole GGR
				JOIN dbo.GlbGroup ON GG_PK = GGR_GG_Group
				WHERE GG_Code IN ('DOC_ALL_ACV','NEOROLES','RepCustoms00001');
				DELETE dbo.GlbGroup
				WHERE GG_Code IN ('DOC_ALL_ACV','NEOROLES','RepCustoms00001');

				",
				newCompanyPk.ToString(),
				demCompanyPk.ToString(),
				newBranchPk.ToString(),
				demBranchPk.ToString(),
				newGroupPk.ToString(),
				allGroupPk.ToString(),
				newStaffPk.ToString(),
				newGroupLinkPk.ToString(),
				adminStaffPk.ToString(),
				postmasterStaffPk.ToString());
			TestConnection.ExecuteNonQuery(sqlText);

			Guid bpStaffPk = new Guid("9cf2bd44-7a64-4214-97b3-321aae2f12d9");

			// GlbCompany
			AssertEquals("[BEFORE] New GlbCompany in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbCompanySchema.PK, newCompanyPk));
			AssertEquals("[BEFORE] Existing GlbCompany in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbCompanySchema.PK, demCompanyPk));
			// GlbBranch
			AssertEquals("[BEFORE] New GlbBranch in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbBranchSchema.PK, newBranchPk));
			AssertEquals("[BEFORE] Existing GlbBranch in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbBranchSchema.PK, demBranchPk));
			// GlbGroup
			AssertEquals("[BEFORE] New GlbGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbGroupSchema.PK, newGroupPk));
			AssertEquals("[BEFORE] Existing GlbGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbGroupSchema.PK, allGroupPk));
			// GlbStaff
			AssertEquals("[BEFORE] New GlbStaff in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbStaffSchema.PK, newStaffPk));
			AssertEquals("[BEFORE] BP GlbStaff in database?", false, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbStaffSchema.PK, bpStaffPk));
			// GlbGroupLink
			AssertEquals("[BEFORE] New GlbGroupLink in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbGroupLinkSchema.PK, newGroupLinkPk));
			// GlbGroupRole
			AssertEquals("[BEFORE] db_datawriter GlbGroupRole in database?", false, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT GGR_PK FROM dbo.GlbGroupRole WHERE GGR_GG_Group = 'a99e7f0e-8379-4f50-8560-9b4bb804c0de' AND GGR_RoleName = 'db_datawriter'"));
			AssertEquals("[BEFORE] cwRestrictedReaderRole GlbGroupRole in database?", false, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT GGR_PK FROM dbo.GlbGroupRole WHERE GGR_GG_Group = '6f0eb310-fc5c-4696-9594-f8ce156542c6' AND GGR_RoleName = 'cwRestrictedReaderRole'"));
			AssertEquals("[BEFORE] db_backupoperator GlbGroupRole in database?", false, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT GGR_PK FROM dbo.GlbGroupRole WHERE GGR_GG_Group = '208068b6-3383-44bf-8e0d-dbd827f9d675' AND GGR_RoleName = 'db_backupoperator'"));
			// NEO Groups
			AssertEquals("[BEFORE] DOC_ALL_ACV GlbGroup in database?", false, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT 1 FROM dbo.GlbGroup WHERE GG_Code = 'DOC_ALL_ACV'"));
			AssertEquals("[BEFORE] DOC_ALL_BOA GlbGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT 1 FROM dbo.GlbGroup WHERE GG_Code = 'DOC_ALL_BOA'"));
			AssertEquals("[BEFORE] NEOROLES GlbGroup in database?", false, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT 1 FROM dbo.GlbGroup WHERE GG_Code = 'NEOROLES'"));
			AssertEquals("[BEFORE] NEODOCACCESS GlbGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT 1 FROM dbo.GlbGroup WHERE GG_Code = 'NEODOCACCESS'"));
			AssertEquals("[BEFORE] RepCustoms00001 GlbGroup in database?", false, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT 1 FROM dbo.GlbGroup WHERE GG_Code = 'RepCustoms00001'"));
			AssertEquals("[BEFORE] RepCustoms00002 GlbGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT 1 FROM dbo.GlbGroup WHERE GG_Code = 'RepCustoms00002'"));

			CompanyUpgradeTask testTask = new CompanyUpgradeTask();
			testTask.Run();

			// Assert results

			// GlbCompany
			AssertEquals("New GlbCompany in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbCompanySchema.PK, newCompanyPk));
			AssertEquals("Update dbo.GlbCompany name not changed back", "Some~Name~Not~To~Be~Changed", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, GlbCompanySchema.PK, demCompanyPk, GlbCompanySchema.GC_Name));
			// GlbBranch
			AssertEquals("New GlbBranch in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbBranchSchema.PK, newBranchPk));
			AssertEquals("Update dbo.GlbBranch name not changed back", "Some~Name~Not~To~Be~Changed", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, GlbBranchSchema.PK, demBranchPk, GlbBranchSchema.GB_BranchName));
			// GlbGroup
			AssertEquals("New GlbGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbGroupSchema.PK, newGroupPk));
			AssertEquals("Update dbo.GlbGroup desc not changed back", "Some~Name~Not~To~Be~Changed", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, GlbGroupSchema.PK, allGroupPk, GlbGroupSchema.GG_Desc));
			// GlbStaff
			AssertEquals("New GlbStaff in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbStaffSchema.PK, newStaffPk));
			AssertEquals("BP GlbStaff re-inserted", "~BP", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, GlbStaffSchema.PK, bpStaffPk, GlbStaffSchema.GS_Code));
			AssertEquals("BP GlbStaff IsActive", true, BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, GlbCompanySchema.PK, demCompanyPk, GlbCompanySchema.GC_IsActive));
			AssertEquals("'sysadmin' shoudl be non-Operational", false, BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, GlbStaffSchema.PK, adminStaffPk, GlbStaffSchema.GS_IsOperational));
			//// GlbGroupLink
			AssertEquals("New GlbGroupLink in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbGroupLinkSchema.PK, newGroupLinkPk));
			// GlbGroupRole
			AssertEquals("[AFTER] db_datawriter GlbGroupRole in database?", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT GGR_PK FROM dbo.GlbGroupRole WHERE GGR_GG_Group = 'a99e7f0e-8379-4f50-8560-9b4bb804c0de' AND GGR_RoleName = 'db_datawriter'"));
			AssertEquals("[AFTER] cwRestrictedReaderRole GlbGroupRole in database?", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT GGR_PK FROM dbo.GlbGroupRole WHERE GGR_GG_Group = '6f0eb310-fc5c-4696-9594-f8ce156542c6' AND GGR_RoleName = 'cwRestrictedReaderRole'"));
			AssertEquals("[AFTER] db_backupoperator GlbGroupRole in database?", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT GGR_PK FROM dbo.GlbGroupRole WHERE GGR_GG_Group = '208068b6-3383-44bf-8e0d-dbd827f9d675' AND GGR_RoleName = 'db_backupoperator'"));
			// NEO Groups
			AssertEquals("[AFTER] DOC_ALL_ACV GlbGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT 1 FROM dbo.GlbGroup WHERE GG_PK = '7a58af74-4e62-4591-a56c-93289e407b75' AND GG_Code = 'DOC_ALL_ACV'"));
			AssertEquals("[AFTER] NEOROLES GlbGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT 1 FROM dbo.GlbGroup WHERE GG_PK = '97e6d83f-91fc-4eac-ab57-7a6b208293ac' AND GG_Code = 'NEOROLES'"));
			AssertEquals("[AFTER] RepCustoms00001 GlbGroup in database?", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT 1 FROM dbo.GlbGroup WHERE GG_PK = 'ea576285-794b-4784-bd85-3178e7b3db8a' AND GG_Code = 'RepCustoms00001'"));
			AssertEquals("[AFTER] GlbSecurity RefDocType:ALL:ACV in database?", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT 1 FROM dbo.GlbSecurity WHERE GU_PK = '960bfa74-8251-4b87-a618-58cf273144bf' AND GU_GG = '7a58af74-4e62-4591-a56c-93289e407b75' AND GU_SecurityRight = 'RefDocType:ALL:ACV'"));
			AssertEquals("[AFTER] GlbGroupRole webwarehousedocketeditor?", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT 1 FROM dbo.GlbGroupRole WHERE GGR_PK = '12f44ee3-fec9-4fef-a582-8749c1cdf824' AND GGR_GG_Group = '97e6d83f-91fc-4eac-ab57-7a6b208293ac' AND GGR_RoleName = 'webwarehousedocketeditor'"));
			AssertEquals("[AFTER] GlbSecurity AllowReport?", true, BaseDataUpgradeTaskTestHelper.IsRecordInDatabase(TestConnection, "SELECT 1 FROM dbo.GlbSecurity WHERE GU_PK = '0ae893a5-52e6-49ed-8f36-7bf4d45eb1d8' AND GU_GG = 'ea576285-794b-4784-bd85-3178e7b3db8a' AND GU_ItemGUID = 'd413ae7d-0182-4fe1-87f1-9267ad5a69c9'"));
		}

		public void TestInsertsCompanyWithMissingNks()
		{
			string sqlText = string.Format(@"
				-- GlbGroupRole
				DELETE dbo.GlbGroupRole WHERE GGR_GG_Group = 'a99e7f0e-8379-4f50-8560-9b4bb804c0de' AND GGR_RoleName = 'db_datawriter';
				DELETE dbo.GlbGroupRole WHERE GGR_GG_Group = '6f0eb310-fc5c-4696-9594-f8ce156542c6' AND GGR_RoleName = 'cwRestrictedReaderRole';
				DELETE dbo.GlbGroupRole WHERE GGR_GG_Group = '208068b6-3383-44bf-8e0d-dbd827f9d675' AND GGR_RoleName = 'db_backupoperator';
				");
			TestConnection.ExecuteNonQuery(sqlText);

			Guid newCompanyPk = Guid.NewGuid();
			Guid demCompanyPk = new Guid("03052ed3-2c64-49ac-97d8-c6079d5015b5");
			Guid newBranchPk = Guid.NewGuid();
			Guid demBranchPk = new Guid("2fdba7fb-60ba-4a03-8336-0defac4f9673");

			AssertEquals("[BEFORE] Demo GlbCompany in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbCompanySchema.PK, demCompanyPk));
			AssertEquals("[BEFORE] New GlbCompany in database?", false, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbCompanySchema.PK, newCompanyPk));
			AssertEquals("[BEFORE] Demo GlbBranch in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbBranchSchema.PK, demBranchPk));
			AssertEquals("[BEFORE] New GlbBranch in database?", false, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbBranchSchema.PK, newBranchPk));

			CompanyUpgradeTask testTask = new CompanyUpgradeTask();
			testTask.ResourceFile.DataSet.Tables[GlbCompanySchema.Constants.TableName].Rows[0][GlbCompanySchema.PK.Name] = newCompanyPk;
			testTask.ResourceFile.DataSet.Tables[GlbCompanySchema.Constants.TableName].Rows[0][GlbCompanySchema.GC_Code.Name] = "NW~";
			testTask.ResourceFile.DataSet.Tables[GlbCompanySchema.Constants.TableName].Rows[0][GlbCompanySchema.GC_RN_NKCountryCode.Name] = "A~";
			testTask.ResourceFile.DataSet.Tables[GlbCompanySchema.Constants.TableName].Rows[0][GlbCompanySchema.GC_RX_NKLocalCurrency.Name] = "AU~";
			testTask.ResourceFile.DataSet.Tables[GlbBranchSchema.Constants.TableName].Rows[0][GlbBranchSchema.PK.Name] = newBranchPk;
			testTask.ResourceFile.DataSet.Tables[GlbBranchSchema.Constants.TableName].Rows[0][GlbBranchSchema.GB_Code.Name] = "NW~";
			testTask.Run();

			AssertEquals("Demo GlbCompany in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbCompanySchema.PK, demCompanyPk));
			AssertEquals("New GlbCompany in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbCompanySchema.PK, newCompanyPk));
			AssertEquals("New GlbCompany code", "NW~", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, GlbCompanySchema.PK, newCompanyPk, GlbCompanySchema.GC_Code));
			AssertEquals("New GlbCompany country NK", "A~", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, GlbCompanySchema.PK, newCompanyPk, GlbCompanySchema.GC_RN_NKCountryCode));
			AssertEquals("New GlbCompany currency NK", "AU~", BaseDataUpgradeTaskTestHelper.GetValueFieldFromPk(TestConnection, GlbCompanySchema.PK, newCompanyPk, GlbCompanySchema.GC_RX_NKLocalCurrency));
			AssertEquals("Demo GlbBranch in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbBranchSchema.PK, demBranchPk));
			AssertEquals("New GlbBranch in database?", true, BaseDataUpgradeTaskTestHelper.IsPkInDatabase(TestConnection, GlbBranchSchema.PK, newBranchPk));
		}

		public void TestBaseDataIsActive()
		{
			string sqlText = @"
				DELETE dbo.TagRule
				DELETE dbo.GlbGroupRole;
				DELETE dbo.GlbGroup;
				DELETE dbo.GlbStaff;
				DELETE dbo.GlbBranch;";
			TestConnection.ExecuteNonQuery(sqlText);
			new CompanyUpgradeTask().Run();

			AssertEquals("Any GlbGroup where IsActive = FALSE?", false, AnyRowsWithGivenValue(GlbGroupSchema.GG_IsActive, false));
			AssertEquals("Any GlbGroup where IsActive = TRUE?", true, AnyRowsWithGivenValue(GlbGroupSchema.GG_IsActive, true));
			AssertEquals("Any GlbStaff where IsActive = FALSE?", true, AnyRowsWithGivenValue(GlbStaffSchema.GS_IsActive, false));
			AssertEquals("Any GlbStaff where IsActive = TRUE?", true, AnyRowsWithGivenValue(GlbStaffSchema.GS_IsActive, true));
			AssertEquals("Any GlbBranch where IsActive = FALSE?", false, AnyRowsWithGivenValue(GlbBranchSchema.GB_IsActive, false));
			AssertEquals("Any GlbBranch where IsActive = TRUE?", true, AnyRowsWithGivenValue(GlbBranchSchema.GB_IsActive, true));
		}

		public void TestNEOGroups_Transform()
		{
			var sqlText = @"DELETE dbo.GlbGroupRole WHERE GGR_RoleName = 'TransitWarehouseCustomer';";
			TestConnection.ExecuteNonQuery(sqlText);

			AssertNEOGroups();
		}

		public void TestNEOGroups_Company_XML()
		{
			var sqlText = @"
DELETE GlbSecurity;
DELETE GlbGroupRole;
DELETE GlbGroup;
";
			TestConnection.ExecuteNonQuery(sqlText);
			var testTask = new CompanyUpgradeTask();
			testTask.Run();

			AssertNEOGroups();
		}

		void AssertNEOGroups()
		{
			var logs = new StringBuilder();

			using (var errorMessages = DataUtils.GetDataTableFromQuery(Db.Connection,
	@"
DECLARE @NEOGroup TABLE
(
	NGG_WebSecurityCode VARCHAR(35) NULL
	,NGG_IsGrantedByDefault BIT NOT NULL
	,NGG_GroupCode VARCHAR(15) NULL
);

INSERT INTO @NEOGroup
(
	NGG_WebSecurityCode
	,NGG_IsGrantedByDefault
	,NGG_GroupCode
)
VALUES
('eCommerce Autocreate Booking Header', 0, 'ECMAUTOBKG'),
('eCommerce Calculate Depot and LMC', 0, 'ECMCALCDEPLMC'),
('eCommerce Confirm Booking Header', 0, 'ECMCONFIRMBKG'),
('eCommerce Destination Depot Portal', 0, 'ECMDSTDEPPORTAL'),
('eCommerce Last Mile Carrier Booking', 0, 'ECMLASTMILEBKG'),
('eCommerce Lodge Origin Load List', 0, 'ECMLODGEORGLL'),
('eCommerce Origin Depot Portal', 0, 'ECMORGDEPPORTAL'),
('eCommerce Receive Booking Header', 0, 'ECMRCVBKG'),
('eCommerce Shipper Portal', 0, 'ECMSHPPORTAL'),
('eCommerce View Carriers and Depots', 0, 'ECMVIEWCARDEP'),
('LContainers (Add/Edit) Number', 1, 'LCTADDEDITNUM'),
('LContainers (Edit)', 1, 'LCTEDIT'),
('LContainers (View)', 1, 'LCTVIEW'),
('Map Consignment via Data Wizard', 0, 'MAPCONSIGNDW'),
('Netting Participant Portal', 0, 'NETPORTAL'),
('Tracking Portal', 1, 'TRACKPORTAL'),
('Transit Warehouse Client Portal', 0, 'WHSCLIENTPORTAL'),
('Transport Customer Portal', 0, 'LTPCUSTPORTAL'),
('Transport Sub-Contractor Portal', 0, 'LTPSUBCNTPORTAL'),
('US AMS', 0, 'USAMS'),
('US AMS (Add/Edit)', 0, 'USAMSADDEDIT'),
('US AMS (Delete)', 0, 'USAMSDELETE'),
('US AMS (Send)', 0, 'USAMSSEND'),
('US AMS (View)', 0, 'USAMSVIEW'),
('Web Booking (Add/Edit)', 1, 'WEBBKGADDEDIT'),
('Web Booking (View)', 1, 'WEBBKGVIEW'),
('Web CFS Shipment (View)', 1, 'WEBCFSVIEW'),
('Web Containers (Add/Edit) Number', 1, 'WEBCTADDEDITNUM'),
('Web Containers (Edit)', 1, 'WEBCTEDIT'),
('Web Containers (View)', 1, 'WEBCTVIEW'),
('Web Declaration (View)', 1, 'WEBDECVIEW'),
('Web Events (View)', 0, 'WEBEVENTSVIEW'),
('Web Inventory (View)', 1, 'WEBINVVIEW'),
('Web Invoicing and Statements', 1, 'WEBINVSTA'),
('Web ISF (Add/Edit)', 0, 'WEBISFADDEDIT'),
('Web ISF (Delete)', 0, 'WEBISFDELETE'),
('Web ISF (Send)', 0, 'WEBISFSEND'),
('Web ISF (View)', 0, 'WEBISFVIEW'),
('Web Orders (Add/Edit)', 1, 'WEBORDADDEDIT'),
('Web Orders (View)', 1, 'WEBORDVIEW'),
('Web Quoting', 1, 'WEBQUOTE'),
('Web Shipments (View)', 1, 'WEBSHIPVIEW'),
('Web Shipping Bills of Lading (View)', 1, 'WEBBOLVIEW'),
('Web Shipping Bookings (Add/Edit)', 1, 'WEBSHBKGADDEDIT'),
('Web Shipping Bookings (View)', 1, 'WEBSHBKGVIEW'),
('Web Shipping Fwd Instruction (Edit)', 1, 'WEBSHFWDINSEDIT'),
('Web Transport Job (View)', 1, 'WEBTRANSVIEW'),
('Web Warehouse Orders (Add/Edit)', 1, 'WEBWHSORDADDEDT'),
('Web Warehouse Products (View)', 1, 'WEBWHSPROVIEW'),
('Web Warehouse Receipts (Add/Edit)', 1, 'WEBWHSRECEDIT'),
('Web Flight Schedules (View)', 1, 'WEBAIRSCHDVIEW'),
('Web Sailing Schedules (View)', 1, 'WEBSEASCHDVIEW'),
('Web Road Schedules (View)', 1, 'WEBROADSCHDVIEW'),
('Web Rail Schedules (View)', 1, 'WEBRAILSCHDVIEW'),
('Map entities via Data Wizard', 0, 'DATAIMPORTMAP'),
('Web Reports', 1, 'WEBREPORTSVIEW'),
('Web Warehouse Orders (View)', 1, 'WEBWHSORDVIEW'),
('Web Market Intelligence (View)', 1, 'WEBMARKETINTEL'),
('Web Message (Send)', 1, 'WEBMSGSEND'),
('Web Message (View)', 1, 'WEBMSGVIEW'),
('Web Documents Uploader', 0, 'WEBEDOCUPLOAD'),
('Web Booking (Select Schedules)', 0, 'WEBBKGSCHEDULE')
;

DECLARE @NEOGroupRole TABLE
(
	NGR_GroupCode VARCHAR(15) NULL,
	NGR_RoleName VARCHAR(50) NULL
);

INSERT INTO @NEOGroupRole
(
	NGR_GroupCode,
	NGR_RoleName
)
VALUES
('ECMAUTOBKG', 'BookingHeaderImporter'),
('ECMCALCDEPLMC', 'DepotAndLastMileCarrierCalculator'),
('ECMCONFIRMBKG', 'BookingHeaderConfirmer'),
('ECMLASTMILEBKG', 'LastMileCarrierBooker'),
('ECMLODGEORGLL', 'OriginLoadListLodger'),
('ECMRCVBKG', 'BookingHeaderReceiver'),
('ECMVIEWCARDEP', 'CarriersAndDepotsViewer'),
('LCTADDEDITNUM', 'webcontainereditor'),
('LCTEDIT', 'webcontainereditor'),
('LCTVIEW', 'weblineragencycontainerviewer'),
('MAPCONSIGNDW', 'MapConsignmentViaDataWizardUser'),
('NETPORTAL', 'NettingParticipant'),
('TRACKPORTAL', 'OrderManagerCustomer'),
('WHSCLIENTPORTAL', 'transitwarehouseclientcontact'),
('LTPCUSTPORTAL', 'module-CSP'),
('LTPSUBCNTPORTAL', 'module-CAP'),
('USAMS', 'USAMSUser'),
('USAMSADDEDIT', 'usamseditor'),
('USAMSDELETE', 'usamseditor'),
('USAMSSEND', 'usamseditor'),
('USAMSVIEW', 'usamsviewer'),
('WEBBKGADDEDIT', 'webbookingeditor'),
('WEBBKGVIEW', 'webbookingviewer'),
('WEBCFSVIEW', 'webcfsshipmentviewer'),
('WEBCTADDEDITNUM', 'webcontainereditor'),
('WEBCTEDIT', 'webcontainereditor'),
('WEBCTVIEW', 'webcontainerviewer'),
('WEBDECVIEW', 'webdeclarationviewer'),
('WEBEVENTSVIEW', 'eventsViewer'),
('WEBINVVIEW', 'webinventoryviewer'),
('WEBINVSTA', 'webinvoiceviewer'),
('WEBISFADDEDIT', 'webisfeditor'),
('WEBISFDELETE', 'webisfeditor'),
('WEBISFSEND', 'webisfeditor'),
('WEBISFVIEW', 'webisfviewer'),
('WEBORDADDEDIT', 'webpurchaseordereditor'),
('WEBORDVIEW', 'weborderviewer'),
('WEBQUOTE', 'webquoteviewer'),
('WEBSHIPVIEW', 'webshipmentviewer'),
('WEBBOLVIEW', 'weblineragencybillsofladingviewer'),
('WEBSHBKGADDEDIT', 'webbookingeditor'),
('WEBSHBKGVIEW', 'weblineragencybookingviewer'),
('WEBSHFWDINSEDIT', 'webbookingeditor'),
('WEBTRANSVIEW', 'webtransportjobviewer'),
('WEBWHSORDADDEDT', 'webwarehousedocketeditor'),
('WEBWHSPROVIEW', 'webwarehouseproductviewer'),
('WEBWHSRECEDIT', 'webwarehousedocketeditor'),
('WEBAIRSCHDVIEW', 'webflightschedulesviewer'),
('WEBSEASCHDVIEW', 'websailingschedulesviewer'),
('WEBROADSCHDVIEW', 'webroadschedulesviewer'),
('WEBRAILSCHDVIEW', 'webrailschedulesviewer'),
('DATAIMPORTMAP', 'MapEntitiesViaDataWizardUser'),
('ECMDSTDEPPORTAL', 'eCommerceDestinationDepotViewer'),
('ECMORGDEPPORTAL', 'eCommerceOriginDepotViewer'),
('ECMSHPPORTAL', 'eCommerceShipperAndNEOPortalsViewer'),
('TRACKPORTAL', 'TrackingPortalUser'),
('WEBREPORTSVIEW', 'webreportsviewer'),
('WEBWHSORDVIEW', 'webwarehouseorderviewer'),
('WEBMARKETINTEL', 'webmarketintelligenceviewer'),
('WEBMSGSEND', 'webmessagesender'),
('WEBMSGVIEW', 'webmessageviewer'),
('WEBEDOCUPLOAD', 'webdocumentsuploader'),
('WEBBKGSCHEDULE', 'WebBookingScheduleSelector')
;

-- All doc types should have NEO groups linked.
SELECT ErrorMessage = CONCAT('NEO Group [', RT_GG_Code ,'] is missing')
FROM
(
	SELECT
	RT_ReferenceType, RT_DocType
	,RT_SecurityItemName = 'RefDocType:' + RT_ReferenceType + ':' + RT_DocType
	,RT_GG_Code = 'DOC_' + RT_ReferenceType + '_' + RT_DocType
	FROM RefDocType
	WHERE RT_IsActive = 1
) T
WHERE RT_GG_Code NOT IN
(
	SELECT GG_Code
	FROM GlbSecurity
	JOIN GlbGroup ON GG_PK = GU_GG
	WHERE GU_SecurityItemIsAllowed = 1
	AND GU_SecurityRight = RT_SecurityItemName
	AND GG_Type = 'ORG'
)
UNION
-- All reports should have NEO groups linked.
SELECT ErrorMessage = CONCAT('NEO Group [', SU_BusinessContext, ':', SU_MenuName, '] is missing')
FROM StmMenuItem
WHERE SU_MenuType = 'WEB' AND SU_BusinessContext LIKE 'REP%'
AND SU_PK NOT IN
(
	SELECT GU_ItemGUID FROM
	GlbSecurity
	JOIN GlbGroup ON GG_PK = GU_GG
	WHERE GU_SecurityItemIsAllowed = 1
	AND GU_SecurityRight = 'AllowReport'
	AND GG_Type = 'ORG'
)
UNION
-- The NEODOCACCESS should have ALL doc types linked.
SELECT ErrorMessage = CONCAT('NEODOCACCESS GlbSecurity [', RT_SecurityItemName,  '] is missing')
FROM
(
	SELECT
	RT_ReferenceType, RT_DocType
	,RT_SecurityItemName = 'RefDocType:' + RT_ReferenceType + ':' + RT_DocType
	,RT_GG_Code = 'DOC_' + RT_ReferenceType + '_' + RT_DocType
	FROM RefDocType
	WHERE RT_IsActive = 1
) T
WHERE RT_SecurityItemName NOT IN
(
	SELECT GU_SecurityRight
	FROM GlbSecurity
	JOIN GlbGroup ON GG_PK = GU_GG
	WHERE GU_SecurityItemIsAllowed = 1
	AND GG_Type = 'ORG'
	AND GG_Code = 'NEODOCACCESS'
)
UNION
-- All NEO Groups should have groups and roles matched.
SELECT ErrorMessage = CONCAT('NEO Group mismatched, NGG_GroupCode:', NGG_GroupCode,  ', NGR_RoleName:', NGR_RoleName, ', GG_Code:', GG_Code, ', GGR_RoleName:', GGR_RoleName)
FROM
(
	SELECT NGG_GroupCode, NGR_RoleName
	FROM @NEOGroup
	JOIN @NEOGroupRole ON NGR_GroupCode = NGG_GroupCode
) NEO
FULL JOIN
(
	SELECT GG_Code, GGR_RoleName
	FROM GlbGroup
	JOIN @NEOGroup ON NGG_GroupCode = GG_Code
	JOIN GlbGroupRole ON GGR_GG_Group = GG_PK
	WHERE GG_Type = 'ORG'
) DB ON NGG_GroupCode = GG_Code AND NGR_RoleName = GGR_RoleName
WHERE NGG_GroupCode IS NULL OR GG_Code IS NULL
UNION
-- NEOROLES Groups should have all roles matched.
SELECT ErrorMessage = CONCAT('NEOROLES GroupRole mismatched, NGR_RoleName:', NGR_RoleName, ', GGR_RoleName:', GGR_RoleName)
FROM
(
	SELECT NGR_RoleName
	FROM @NEOGroupRole
	JOIN @NEOGroup ON NGR_GroupCode = NGG_GroupCode
	WHERE NGG_IsGrantedByDefault = 1
) NEO
FULL JOIN
(
	SELECT GGR_RoleName
	FROM GlbGroup
	JOIN GlbGroupRole ON GGR_GG_Group = GG_PK
	WHERE GG_Type = 'ORG' AND GG_Code = 'NEOROLES'
) NEOROLES ON NGR_RoleName = GGR_RoleName
WHERE NGR_RoleName IS NULL OR GGR_RoleName IS NULL;

"))
			{
				foreach (var errorMessage in errorMessages.Rows.OfType<DataRow>().Select(x => x[0].ToString()))
				{
					logs.AppendLine(errorMessage);
				}
			}

			AssertEquals(@"Please add the missing items to this unit test and $Dev/Database/Odyssey/Data/BaseData/Company/Company.xml
And please create new transforms accordingly, sample: WI00664250", "", logs.ToString());
		}

		bool AnyRowsWithGivenValue(SchemaBoolColumn flagColumn, bool flagValue)
		{
			string sqlText = String.Format(
				"IF EXISTS(SELECT null FROM {0} WHERE {1} = {2}) SELECT 1 ELSE SELECT 0",
				flagColumn.TableName, flagColumn.Name, (flagValue ? "1" : "0"));
			return Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText));
		}

		public void TestStaffCanLogin()
		{
			string sqlText = @"
				DELETE dbo.TagRule
				DELETE dbo.GlbGroupRole;
				DELETE dbo.GlbGroup;
				DELETE dbo.GlbStaff;
				DELETE dbo.GlbBranch;";
			TestConnection.ExecuteNonQuery(sqlText);
			new CompanyUpgradeTask().Run();

			AssertEquals("Unknown User's CanLogin should be false", false, AssertStaffCanLogin("Unknown User"));
			AssertEquals("CWPostMaster's CanLogin should be false", false, AssertStaffCanLogin("CWPostMaster"));
			AssertEquals("CWAutoDataImport's CanLogin should be false", false, AssertStaffCanLogin("CWAutoDataImport"));

			// These CanLogin need to remain true
			AssertEquals("CWService's CanLogin should be true", true, AssertStaffCanLogin("CWService"));
			AssertEquals("sysadmin's CanLogin should be true", true, AssertStaffCanLogin("sysadmin"));
			AssertEquals("CWSupport's CanLogin should be true", true, AssertStaffCanLogin("CWSupport"));
			AssertEquals("CWWeb's CanLogin should be true", true, AssertStaffCanLogin("CWWeb"));
		}

		bool AssertStaffCanLogin(string loginName)
		{
			using (var sqlCmd = TestConnection.Command(@"SELECT GS_CanLogin FROM dbo.GlbStaff WHERE GS_LoginName = @loginName"))
			{
				sqlCmd.AddParameter("@loginName", System.Data.SqlDbType.NVarChar, loginName);
				return Convert.ToBoolean(sqlCmd.ExecuteScalar());
			}
		}
	}
}

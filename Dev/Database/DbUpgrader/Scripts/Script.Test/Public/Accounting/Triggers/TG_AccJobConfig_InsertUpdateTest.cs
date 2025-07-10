using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Triggers;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Triggers.Testing
{
	[TestedType(typeof(TG_AccJobConfig_InsertUpdate))]
	class TG_AccJobConfig_InsertUpdateTest : DBCreateTriggerScriptTest
	{
		public void TestERT_Insert_PickupPreference()
		{
			var helper = new TestDbHelper(TestConnection);
			var orgPK = helper.InsertOrgHeader("ORG", "ORGHEADER");
			AssertNoExceptionThrown(() => helper.InsertAccJobConfig("ERT", parentTableCode: "OH", parentId: orgPK, code: "BUY", code2: "PIC"));
		}

		public void TestERT_Insert_DeliveryPreference()
		{
			var helper = new TestDbHelper(TestConnection);
			var orgPK = helper.InsertOrgHeader("ORG", "ORGHEADER");
			AssertNoExceptionThrown(() => helper.InsertAccJobConfig("ERT", parentTableCode: "OH", parentId: orgPK, code: "BUY", code2: "DEL"));
		}

		public void TestCFX_Insert_PreventDuplicate()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			InsertCfxUpliftConfig();

			AssertDuplicateExceptionThrown(() => InsertCfxUpliftConfig());
		}

		public void TestCFX_Insert_PreventDuplicatesInSameTransaction()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			//Check the duplicate exception is thrown first
			AssertDuplicateExceptionThrown(() => DbHelper.RunSQL(new { JCF_GC = TestDbHelper.DefaultCompanyPK }, @"
DECLARE @Timestamp AS SMALLDATETIME = GETUTCDATE();

INSERT INTO [dbo].[AccJobConfig]
	([JCF_PK],[JCF_ConfigType],[JCF_GC],[JCF_Ledger],[JCF_JobType],[JCF_ServiceDirection],[JCF_TransportMode],[JCF_SystemCreateTimeUtc],[JCF_SystemCreateUser],[JCF_SystemLastEditTimeUtc],[JCF_SystemLastEditUser],[JCF_StartDate],[JCF_ExpiryDate])
VALUES
	(NEWID(),'CFX',@JCF_GC,'AR','ALL','ALL','ALL',@Timestamp,'TST',@Timestamp,'TST',NULL,NULL),
	(NEWID(),'CFX',@JCF_GC,'AR','ALL','ALL','ALL',@Timestamp,'TST',@Timestamp,'TST',NULL,NULL),
	(NEWID(),'CFX',@JCF_GC,'AR','ALL','ALL','ALL',@Timestamp,'TST',@Timestamp,'TST','2025-02-01','2025-02-05'),
	(NEWID(),'CFX',@JCF_GC,'AR','ALL','ALL','ALL',@Timestamp,'TST',@Timestamp,'TST','2025-02-03','2025-02-06')"));
		}

		public void TestCFX_Insert_NoDuplicates()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			InsertCfxUpliftConfig();

			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(companyPK: TestDbHelper.OtherCompanyPK));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(parentTableCode: "OH", parentId: TestDbHelper.DefaultCompanyOrgProxyPK));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(parentId: TestDbHelper.BranchBrnPK2));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(jobType: "BRK"));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(serviceDirection: "IMP"));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(transportMode: "SEA"));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(origin: "AU"));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(destination: "NZ"));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(currency: "NZD"));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(startDate: new DateTime(2025, 01, 11), expiryDate: new DateTime(2025, 01, 15)));
		}

		public void TestCFX_Insert_PreventDatesOverlappingWithSameDateRange()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			var startDate = new DateTime(2025, 01, 05);
			var expiryDate = new DateTime(2025, 01, 10);
			InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate);

			AssertOverlappingDatesExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate));
		}

		public void TestCFX_Insert_PreventDatesOverlappingWithExistingStartDate()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			var startDate = new DateTime(2025, 01, 05);
			var expiryDate = new DateTime(2025, 01, 10);
			InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate);

			AssertOverlappingDatesExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate.AddDays(-1), expiryDate: startDate));
		}

		public void TestCFX_Insert_PreventDatesOverlappingWithExistingExpiryDate()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			var startDate = new DateTime(2025, 01, 05);
			var expiryDate = new DateTime(2025, 01, 10);
			InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate);

			AssertOverlappingDatesExceptionThrown(() => InsertCfxUpliftConfig(startDate: expiryDate, expiryDate: expiryDate.AddDays(1)));
		}

		public void TestCFX_Insert_PreventDatesFallingInsideExistingDateRange()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			var startDate = new DateTime(2025, 01, 05);
			var expiryDate = new DateTime(2025, 01, 10);
			InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate);

			AssertOverlappingDatesExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate.AddDays(1), expiryDate: expiryDate.AddDays(-1)));
		}

		public void TestCFX_Insert_PreventDatesCoveringExistingDateRange()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			var startDate = new DateTime(2025, 01, 05);
			var expiryDate = new DateTime(2025, 01, 10);
			InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate);

			AssertOverlappingDatesExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate.AddDays(-1), expiryDate: expiryDate.AddDays(1)));
		}

		public void TestCFX_Insert_PreventOverlappingDatesInSameTransaction()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			//Check the overlapping dates exception is thrown even if there are other rows being inserted
			AssertOverlappingDatesExceptionThrown(() => DbHelper.RunSQL(new { JCF_GC = TestDbHelper.DefaultCompanyPK }, @"
		DECLARE @Timestamp AS SMALLDATETIME = GETUTCDATE();

		INSERT INTO [dbo].[AccJobConfig]
			([JCF_PK],[JCF_ConfigType],[JCF_GC],[JCF_Ledger],[JCF_JobType],[JCF_ServiceDirection],[JCF_TransportMode],[JCF_SystemCreateTimeUtc],[JCF_SystemCreateUser],[JCF_SystemLastEditTimeUtc],[JCF_SystemLastEditUser],[JCF_StartDate],[JCF_ExpiryDate])
		VALUES
			(NEWID(),'CFX',@JCF_GC,'AR','ALL','ALL','ALL',@Timestamp,'TST',@Timestamp,'TST',NULL,NULL),
			(NEWID(),'CFX',@JCF_GC,'AR','ALL','ALL','ALL',@Timestamp,'TST',@Timestamp,'TST','2025-02-01','2025-02-05'),
			(NEWID(),'CFX',@JCF_GC,'AR','ALL','ALL','ALL',@Timestamp,'TST',@Timestamp,'TST','2025-02-03','2025-02-06')"));
		}

		public void TestCFX_Insert_NoOverlappingDates()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			var cfxPk = InsertCfxUpliftConfig();
			var startDate = new DateTime(2025, 01, 05);
			var expiryDate = new DateTime(2025, 01, 10);
			UpdateCfxUpliftConfig(cfxPk, startDate: startDate, expiryDate: expiryDate);

			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate.AddDays(-2), expiryDate: startDate.AddDays(-1)));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(startDate: expiryDate.AddDays(1), expiryDate: expiryDate.AddDays(2)));

			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate, companyPK: TestDbHelper.OtherCompanyPK));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate, parentTableCode: "OH", parentId: TestDbHelper.DefaultCompanyOrgProxyPK));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate, parentId: TestDbHelper.BranchBrnPK2));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate, jobType: "BRK"));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate, serviceDirection: "IMP"));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate, transportMode: "SEA"));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate, origin: "AU"));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate, destination: "NZ"));
			AssertNoExceptionThrown(() => InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate, currency: "NZD"));
		}

		public void TestCFX_Update_PreventDuplicate()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			InsertCfxUpliftConfig();
			var cfxPk = InsertCfxUpliftConfig(startDate: new DateTime(2025, 01, 01), expiryDate: new DateTime(2025, 01, 05));

			AssertDuplicateExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk));
		}

		public void TestCFX_Update_NoDuplicates()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			InsertCfxUpliftConfig();
			var cfxPk = InsertCfxUpliftConfig(startDate: new DateTime(2025, 01, 01), expiryDate: new DateTime(2025, 01, 05));

			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, companyPK: TestDbHelper.OtherCompanyPK));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, parentTableCode: "OH", parentId: TestDbHelper.DefaultCompanyOrgProxyPK));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, parentId: TestDbHelper.BranchBrnPK2));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, jobType: "BRK"));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, serviceDirection: "IMP"));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, transportMode: "SEA"));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, origin: "AU"));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, destination: "NZ"));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, currency: "NZD"));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: new DateTime(2025, 01, 11), expiryDate: new DateTime(2025, 01, 15)));
		}

		public void TestCFX_Update_PreventDatesOverlappingWithSameDateRange()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			var startDate = new DateTime(2025, 01, 05);
			var expiryDate = new DateTime(2025, 01, 10);
			InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate);
			var cfxPk = InsertCfxUpliftConfig();

			AssertOverlappingDatesExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate, expiryDate: expiryDate));
		}

		public void TestCFX_Update_PreventDatesOverlappingWithExistingStartDate()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			var startDate = new DateTime(2025, 01, 05);
			var expiryDate = new DateTime(2025, 01, 10);
			InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate);
			var cfxPk = InsertCfxUpliftConfig();

			AssertOverlappingDatesExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate.AddDays(-1), expiryDate: startDate));
		}

		public void TestCFX_Update_PreventDatesOverlappingWithExistingExpiryDate()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			var startDate = new DateTime(2025, 01, 05);
			var expiryDate = new DateTime(2025, 01, 10);
			InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate);
			var cfxPk = InsertCfxUpliftConfig();

			AssertOverlappingDatesExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: expiryDate, expiryDate: expiryDate.AddDays(1)));
		}

		public void TestCFX_Update_PreventDatesFallingInsideExistingDateRange()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			var startDate = new DateTime(2025, 01, 05);
			var expiryDate = new DateTime(2025, 01, 10);
			InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate);
			var cfxPk = InsertCfxUpliftConfig();

			AssertOverlappingDatesExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate.AddDays(1), expiryDate: expiryDate.AddDays(-1)));
		}

		public void TestCFX_Update_PreventDatesCoveringExistingDateRange()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			var startDate = new DateTime(2025, 01, 05);
			var expiryDate = new DateTime(2025, 01, 10);
			InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate);
			var cfxPk = InsertCfxUpliftConfig();

			AssertOverlappingDatesExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate.AddDays(-1), expiryDate: expiryDate.AddDays(1)));
		}

		public void TestCFX_Update_NoOverlappingDates()
		{
			Assert("Precondition: TG_AccJobConfig_InsertUpdate trigger exists", TG_AccJobConfig_InsertUpdate_Exists());

			var startDate = new DateTime(2025, 01, 05);
			var expiryDate = new DateTime(2025, 01, 10);
			InsertCfxUpliftConfig(startDate: startDate, expiryDate: expiryDate);
			var cfxPk = InsertCfxUpliftConfig();

			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate.AddDays(-2), expiryDate: startDate.AddDays(-1)));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: expiryDate.AddDays(1), expiryDate: expiryDate.AddDays(2)));

			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate, expiryDate: expiryDate, companyPK: TestDbHelper.OtherCompanyPK));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate, expiryDate: expiryDate, parentTableCode: "OH", parentId: TestDbHelper.DefaultCompanyOrgProxyPK));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate, expiryDate: expiryDate, parentId: TestDbHelper.BranchBrnPK2));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate, expiryDate: expiryDate, jobType: "BRK"));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate, expiryDate: expiryDate, serviceDirection: "IMP"));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate, expiryDate: expiryDate, transportMode: "SEA"));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate, expiryDate: expiryDate, origin: "AU"));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate, expiryDate: expiryDate, destination: "NZ"));
			AssertNoExceptionThrown(() => UpdateCfxUpliftConfig(cfxPk, startDate: startDate, expiryDate: expiryDate, currency: "NZD"));
		}

		Guid InsertCfxUpliftConfig(Guid? companyPK = null, string parentTableCode = "GB", Guid? parentId = null, string jobType = "SHP", string serviceDirection = "ALL", string origin = "", string destination = "", string transportMode = "ALL", string currency = "", DateTime? startDate = null, DateTime? expiryDate = null)
		{
			return DbHelper.InsertAccJobConfig("CFX", companyPK: companyPK ?? TestDbHelper.DefaultCompanyPK, ledger: "AR", parentTableCode: parentTableCode, parentId: parentId ?? TestDbHelper.BranchBrnPK,
				jobType: jobType, serviceDirection: serviceDirection, transportMode: transportMode, origin: origin, destination: destination, code: currency,
				startDate: startDate, expiryDate: expiryDate);
		}

		void UpdateCfxUpliftConfig(Guid cfxJobConfigPk, Guid? companyPK = null, string parentTableCode = "GB", Guid? parentId = null, string jobType = "SHP", string serviceDirection = "ALL", string origin = "", string destination = "", string transportMode = "ALL", string currency = "", DateTime? startDate = null, DateTime? expiryDate = null)
		{
			var sql = @"
UPDATE dbo.AccJobConfig
SET
	JCF_GC = @JCF_GC,
	JCF_ParentTableCode = @JCF_ParentTableCode,
	JCF_ParentId = @JCF_ParentId,
	JCF_JobType = @JCF_JobType,
	JCF_ServiceDirection = @JCF_ServiceDirection,
	JCF_RN_NKOriginCountry = @JCF_RN_NKOriginCountry,
	JCF_RN_NKDestinationCountry = @JCF_RN_NKDestinationCountry,
	JCF_TransportMode = @JCF_TransportMode,
	JCF_Code = @JCF_Code,
	JCF_StartDate = @JCF_StartDate,
	JCF_ExpiryDate = @JCF_ExpiryDate,
	JCF_SystemLastEditTimeUtc = GETUTCDATE(),
	JCF_SystemLastEditUser = 'TST'
WHERE JCF_PK = @JCF_PK";

			DbHelper.RunSQL(new
			{
				JCF_PK = cfxJobConfigPk,
				JCF_GC = companyPK ?? TestDbHelper.DefaultCompanyPK,
				JCF_ParentTableCode = parentTableCode,
				JCF_ParentId = parentId ?? TestDbHelper.BranchBrnPK,
				JCF_JobType = jobType,
				JCF_ServiceDirection = serviceDirection,
				JCF_RN_NKOriginCountry = origin,
				JCF_RN_NKDestinationCountry = destination,
				JCF_TransportMode = transportMode,
				JCF_Code = currency,
				JCF_StartDate = startDate,
				JCF_ExpiryDate = expiryDate
			}, sql);
		}

		bool TG_AccJobConfig_InsertUpdate_Exists() => TestConnection.Exists("FROM sys.triggers WHERE name = 'TG_AccJobConfig_InsertUpdate'");

		void AssertDuplicateExceptionThrown(AnonymousMethod dbAction) => AssertDbException(
			"Duplicate CFX Uplift configuration.", dbAction);

		void AssertOverlappingDatesExceptionThrown(AnonymousMethod dbAction) => AssertDbException(
			"Overlapping date range for CFX Uplift configuration.", dbAction);

		void AssertDbException(string expectedMessage, AnonymousMethod dbAction) => AssertExceptionThrown<SqlException>(
			$"Should be: {expectedMessage}",
			$"{expectedMessage}\r\nThe transaction ended in the trigger. The batch has been aborted.", dbAction);
	}
}

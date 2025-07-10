using Enterprise.Integration.Billing;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	abstract class CaptureUSLowValueByBillUsageTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2020, 8);

		protected override void PrepareTestData()
		{
			var sqlText = @"
				DECLARE @UlhPk1 UNIQUEIDENTIFIER = newid();
				DECLARE @UlhPk2 UNIQUEIDENTIFIER = newid();
				DECLARE @UlhPk3 UNIQUEIDENTIFIER = newid();
				DECLARE @UlhPk4 UNIQUEIDENTIFIER = newid();

				DECLARE @UlbPk1 UNIQUEIDENTIFIER = newid();
				DECLARE @UlbPk2 UNIQUEIDENTIFIER = newid();
				DECLARE @UlbPk3 UNIQUEIDENTIFIER = newid();
				DECLARE @UlbPk4 UNIQUEIDENTIFIER = newid();

				DECLARE @CePk1 UNIQUEIDENTIFIER = newid();
				DECLARE @CePk2 UNIQUEIDENTIFIER = newid();
				DECLARE @CePk3 UNIQUEIDENTIFIER = newid();
				DECLARE @CePk4 UNIQUEIDENTIFIER = newid();

				DECLARE @GcPk UNIQUEIDENTIFIER = newid();
				DECLARE @GbPk UNIQUEIDENTIFIER = newid();

				INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
					(@GcPk, 'USC', 'US company', 'US');
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
					(@GbPk, 'USB', @GcPk);

				INSERT INTO dbo.CusUSLVClearance (ULH_PK, ULH_JobNumber, ULH_UseCode, ULH_GB, ULH_SystemCreateUser, ULH_SystemCreateTimeUtc, ULH_SystemLastEditUser, ULH_SystemLastEditTimeUtc, ULH_ClusterKey) VALUES
					(@UlhPk1, 'ULH01', 'AAA', @GbPk, 'FFF', '2020-08-01', 'HHH', '2020-08-05', 11),
					(@UlhPk2, 'ULH02', 'BBB', @GbPk, 'GGG', '2020-08-02', 'III', '2020-08-06', 22),
					(@UlhPk3, 'ULH03', 'HVL', @GbPk, 'HHH', '2020-08-03', 'JJJ', '2020-08-07', 33),
					(@UlhPk4, 'ULH04', 'CCC', @GbPk, 'III', '2020-08-04', 'KKK', '2020-08-08', 44);

				INSERT INTO dbo.CusUSLVConsignment (ULB_PK, ULB_ULH, ULB_HouseBill, ULB_ClusterKey) VALUES
					(@UlbPk1, @UlhPk1, 'PPP', 11),
					(@UlbPk2, @UlhPk2, 'QQQ', 22),
					(@UlbPk3, @UlhPk3, 'RRR', 33),
					(@UlbPk4, @UlhPk4, 'SSS', 44);

				INSERT INTO dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_SystemCreateUser, CE_SystemCreateTimeUtc, CE_EntryType, CE_Category, CE_RN_NKCountryCode, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
					(newid(), @UlbPk1, 'CusUSLVConsignment', '1234', 'DDD', '2020-08-10', 'ENS', 'CUS', 'US', getutcdate(), '~BP'),
					(newid(), @UlbPk2, 'CusUSLVConsignment', '5678', 'EEE', '2020-08-11', 'ENS', 'CUS', 'US', getutcdate(), '~BP'),
					(newid(), @UlbPk3, 'CusUSLVConsignment', '9012', 'FFF', '2020-08-12', 'ENS', 'CUS', 'US', getutcdate(), '~BP'),
					(newid(), @UlbPk4, 'CusUSLVConsignment', '', 'GGG', '2020-08-13', 'ENS', 'CUS', 'US', getutcdate(), '~BP');
";
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}

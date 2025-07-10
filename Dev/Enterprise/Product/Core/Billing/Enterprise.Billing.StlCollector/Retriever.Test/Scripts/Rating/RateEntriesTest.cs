using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Rating;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Rating
{
	[TestedType(typeof(RateEntries))]
	sealed class RateEntriesTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			string sqlText = $@"
			-- Declare variables for the primary keys of the GlbCompany, GlbBranch, and GlbDepartment tables
			DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
			DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
			DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);

			-- Add two more branches
			INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(NEWID(), 'SY1',  @GcPk),
					(NEWID(), 'TK1',  @GcPk);

			-- Declare variables for new primary keys
			DECLARE @GcPkDe UNIQUEIDENTIFIER = newid();
			DECLARE @GbPkDe UNIQUEIDENTIFIER = newid();
			DECLARE @GcPkNz UNIQUEIDENTIFIER = newid();
			DECLARE @GbPkNz UNIQUEIDENTIFIER = newid();

			-- Insert rows into the GlbCompany and GlbBranch tables
			INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
			  (@GcPkDe, 'DEN', 'DE company', 'DE'),
			  (@GcPkNz, 'NZN', 'NZ company', 'NZ');

			INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
			  (@GbPkDe, 'DEB', @GcPkDe),
			  (@GbPkNz, 'NZB', @GcPkNz),
			  (NEWID(), 'SY2', @GcPkDe),
			  (NEWID(), 'TK2', @GcPkDe),
			  (NEWID(), 'SY3', @GcPkNz),
			  (NEWID(), 'TK3', @GcPkNz);

			-- Declare variables for new primary keys
			DECLARE @thPK1 UNIQUEIDENTIFIER = newid();
			DECLARE @thPK2 UNIQUEIDENTIFIER = newid();
			DECLARE @thPK3 UNIQUEIDENTIFIER = newid();
			DECLARE @OhPK1 UNIQUEIDENTIFIER = newid();
			DECLARE @OhPK2 UNIQUEIDENTIFIER = newid();
			DECLARE @OhPK3 UNIQUEIDENTIFIER = newid();

			-- Insert rows into the OrgHeader table
			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES
			  (@OhPK1, 'EDICUS1'),
			  (@OhPK2, 'EDICUS2'),
			  (@OhPK3, 'EDICUS3');

			INSERT dbo.RatingHeader (TH_PK, TH_GC, TH_OH, TH_RateType, TH_QuoteNumber, TH_OneTimeQuote, TH_SystemCreateUser, TH_SystemCreateTimeUtc, TH_QuoteDate, TH_IsCancelled, TH_SystemLastEditTimeUtc, TH_SystemLastEditUser,TH_Accepted) VALUES
				(@thPK1,  @GcPk,   @OhPK1, 'SAL', '',             0, 'US1', GetUtcDate(), null,         0, GetUtcDate(), '~BP', null),
				(@thPK2,  @GcPkDe, @OhPK1, 'QTE', 'QBNE00001002', 1, 'US2', GetUtcDate(), '2022-11-01', 0, GetUtcDate(), '~BP', GetUtcDate()),
				(@thPK3,  @GcPkNz, @OhPK1, 'COS', 'QBNE00001003', 0, 'US3', GetUtcDate(), '2022-11-02', 0, GetUtcDate(), '~BP', GetUtcDate()),
				(newid(), @GcPk,   @OhPK2, 'SAL', '',             0, 'US1', GetUtcDate(), null,         0, GetUtcDate(), '~BP', null),
				(newid(), @GcPkDe, @OhPK2, 'QTE', 'QBNE00001005', 0, 'US5', GetUtcDate(), '2022-11-03', 0, GetUtcDate(), '~BP', GetUtcDate()),
				(newid(), @GcPkNz, @OhPK2, 'SAL', '',             0, 'US1', GetUtcDate(), null,         0, GetUtcDate(), '~BP', null),
				(newid(), @GcPk,   @OhPK3, 'COS', 'QBNE00001007', 1, 'US7', GetUtcDate(), '2022-11-04', 1, GetUtcDate(), '~BP', GetUtcDate()),
				(newid(), @GcPkDe, @OhPK3, 'QTE', 'QBNE00001008', 1, 'US8', GetUtcDate(), '2022-11-05', 0, GetUtcDate(), '~BP', GetUtcDate()),
				(newid(), @GcPkNz, @OhPK3, 'QTE', 'QBNE00001009', 1, 'US9', GetUtcDate(), '2022-11-06', 0, GetUtcDate(), '~BP', GetUtcDate());

			INSERT dbo.RateEntry (TI_PK, TI_TH, TI_GC_Publisher, TI_RateStartDate, TI_RateEndDate,TI_RateCategory, TI_Mode, TI_CreationSource, TI_SystemCreateTimeUtc, TI_SystemCreateUser, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser) VALUES
				(newid(), @thPK1, @GcPk, '2019-11-19 01:00:00', '2020-11-17 01:00:00', 'FCL', 'SEA', ''   , '2022-10-22 01:00:00', '~BP', GetUtcDate(), '~BP'), -- not reported as creation time too old
				(newid(), @thPK3, @GcPk, '2021-11-19 01:00:00', '2022-11-18 01:00:00', 'AIR', 'LSE', ''   , '2022-12-16 01:00:00', '~BP', GetUtcDate(), '~BP'), -- not reported as creation time too new

				(newid(), @thPK1, @GcPk, '2022-11-19 01:00:00', '2023-11-19 01:00:00', 'AIR', 'LSE', ''   , '2022-11-12 08:00:00', '~BP', GetUtcDate(), '~BP'),
				(newid(), @thPK1, @GcPk, '2021-11-19 01:00:00', '2022-11-18 01:00:00', 'AIR', 'LSE', ''   , '2022-11-12 01:00:00', '~BP', GetUtcDate(), '~BP'),
				(newid(), @thPK1, @GcPk, '2020-11-19 01:00:00', '2021-11-18 01:00:00', 'FCL', 'SEA', ''   , '2022-11-22 01:00:00', '~BP', GetUtcDate(), '~BP'),
				(newid(), @thPK2, @GcPk, '2022-11-19 01:00:00', '2023-11-19 01:00:00', 'FCL', 'SEA', ''   , '2022-11-11 01:00:00', '~BP', GetUtcDate(), '~BP'),
				(newid(), @thPK2, @GcPk, '2021-11-19 01:00:00', '2022-11-18 01:00:00', 'FCL', 'SEA', ''   , '2022-11-11 04:00:00', '~BP', GetUtcDate(), '~BP'),
				(newid(), @thPK2, @GcPk, '2020-11-19 01:00:00', '2021-11-18 01:00:00', 'FCL', 'SEA', ''   , '2022-11-11 06:00:00', '~BP', GetUtcDate(), '~BP'),
				(newid(), @thPK3, @GcPk, '2022-11-19 01:00:00', '2023-11-19 01:00:00', 'AIR', 'LSE', ''   , '2022-11-16 01:00:00', '~BP', GetUtcDate(), '~BP'),

				(newid(), @thPK1, @GcPk, '2019-11-01 01:01:00', '2019-11-03 01:00:00', 'LCL', 'LRO', 'QUO', '2019-12-16 01:00:00', '~BP', GetUtcDate(), '~BP'), -- not reported as creation time too old
				(newid(), @thPK1, @GcPk, '2019-11-04 01:02:00', '2019-11-06 01:00:00', 'LCL', 'FTL', 'QUO', '2022-12-16 01:00:00', '~BP', GetUtcDate(), '~BP'), -- not reported as creation time too new

				(newid(), @thPK1, @GcPk, '2022-11-07 01:03:00', '2022-11-09 01:00:00', 'LCL', 'FTL', 'QUO', '2022-11-16 01:00:00', '~BP', GetUtcDate(), '~BP'),
				(newid(), @thPK1, @GcPk, '2022-11-10 01:04:00', '2022-11-12 01:00:00', 'LCL', 'FTL', 'ADW', '2022-11-16 01:00:00', '~BP', GetUtcDate(), '~BP'),
				(newid(), @thPK1, @GcPk, '2022-11-13 01:05:00', '2022-11-15 01:00:00', 'LCL', 'FTL', 'ADW', '2022-11-16 01:00:00', '~BP', GetUtcDate(), '~BP'),
				(newid(), @thPK1, @GcPk, '2022-11-16 01:06:00', '2022-11-18 01:00:00', 'LCL', 'LRO', 'ADW', '2022-11-16 01:00:00', '~BP', GetUtcDate(), '~BP'),
				(newid(), @thPK1, @GcPk, '2022-11-19 01:07:00', '2022-11-21 01:00:00', 'LCL', 'LRO', 'TCT', '2022-11-16 01:00:00', '~BP', GetUtcDate(), '~BP');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 8, transactions.Count());
			AssertRow(transactions, "SAL", "AIR", "LSE", null, 2);
			AssertRow(transactions, "SAL", "FCL", "SEA", null, 1);
			AssertRow(transactions, "QTE", "FCL", "SEA", null, 3);
			AssertRow(transactions, "COS", "AIR", "LSE", null, 1);

			AssertRow(transactions, "SAL", "LCL", "FTL", "QUO", 1);
			AssertRow(transactions, "SAL", "LCL", "FTL", "ADW", 2);
			AssertRow(transactions, "SAL", "LCL", "LRO", "ADW", 1);
			AssertRow(transactions, "SAL", "LCL", "LRO", "TCT", 1);
		}

		void AssertRow(IEnumerable<IStlTransaction> transactions, string expectedRateType, string expectedRateCategory, string expectedRateMode, string expectedCreationSource, int expectedCount)
		{
			var transaction = transactions.Single(t =>
				t.Reference1 == expectedRateType &&
				t.Reference2 == expectedRateCategory &&
				t.Reference3 == expectedRateMode &&
				t.Reference4 == expectedCreationSource);
			AssertEquals("Item Count", expectedCount, transaction.BillableCount);

			var expectedValues = new Dictionary<string, string>()
			{
				{ "RateType" , expectedRateType },
				{ "RateCategory", expectedRateCategory },
				{ "RateMode", expectedRateMode },
				{ "CreationSource", expectedCreationSource }
			};
			var expectedValuesString = expectedValues
				.Select((kv) => $"\"{kv.Key}\":\"{kv.Value}\"")
				.Aggregate((current, next) => current + "," + next);
			AssertEquals("AdditionalRefs", $"{{{expectedValuesString}}}", transaction.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 11);

		public void TestIsSystemLevelFeature()
		{
			AssertEquals("IsSystemLevel", false, ScriptToTest.IsSystemLevel);
		}
	}
}

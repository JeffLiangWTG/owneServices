using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(ExtensionsEuropeanNctsMovement))]
	sealed class ExtensionsEuropeanNctsMovementTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @BhPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @BhPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @BhPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @BhPk04 UNIQUEIDENTIFIER = newid();

				DECLARE @GcPkSg UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkGb UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkIt UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkFr UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkCa UNIQUEIDENTIFIER = newid();

				DECLARE @GbPkSg UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkGb UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkIt UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkFr UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkCa UNIQUEIDENTIFIER = newid();

				DECLARE @ZzbPkEuctp UNIQUEIDENTIFIER = newid();
				DECLARE @ZzbPkXxxxx UNIQUEIDENTIFIER = newid();

				INSERT RefDatabase_RefDataGrouping (ZZZ_PK, ZZZ_DataGrouping, ZZZ_Description) VALUES
					(newid(), 'EUN', 'European Union');

				INSERT RefDatabase_RefCusTradeGroup (ZZA_PK, ZZA_TradeGroup, ZZA_Description, ZZA_ZZZ_NKDataGrouping) VALUES
					(@ZzbPkEuctp, 'EUCTP', 'EU Common Transit Procedure', 'EUN'),
					(@ZzbPkXxxxx, 'XXXXX', 'XXXXX Trade Group', 'EUN');

				INSERT RefDatabase_RefCusTradeGroupCountry (ZZB_PK, ZZB_ZZA_TradeGroup, ZZB_RN_NKTradeGroupCountryCode) VALUES
					(newid(), @ZzbPkEuctp, 'GB'),
					(newid(), @ZzbPkEuctp, 'IT'),
					(newid(), @ZzbPkEuctp, 'FR'),
					(newid(), @ZzbPkXxxxx, 'SG');

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES
					(@GcPkSg, '~SG', 'SG company', 'SG'),
					(@GcPkGb, '~GB', 'GB company', 'GB'),
					(@GcPkIt, '~IT', 'IT company', 'IT'),
					(@GcPkFr, '~FR', 'FR company', 'FR'),
					(@GcPkCa, '~CA', 'CA company', 'CA');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(@GbPkSg, '^SG', @GcPkSg),
					(@GbPkGb, '^GB', @GcPkGb),
					(@GbPkIt, '^IT', @GcPkIt),
					(@GbPkFr, '^FR', @GcPkFr),
					(@GbPkCa, '^CA', @GcPkCa);

				INSERT dbo.CusInbondHeader (BH_PK, BH_GB, BH_JobReference, BH_HeaderType, BH_ApplicationCode, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser) VALUES
					(@BhPk01, @GbPkGb, 'LRN01', 'D', 'NCT', '2012-12-01', 'U01', GetUtcDate(), '~BP'),
					(@BhPk02, @GbPkGb, 'LRN02', '' , 'NCT', '2012-12-02', 'U02', GetUtcDate(), '~BP'),
					(@BhPk03, @GbPkIt, 'LRN03', 'A', 'NCT', '2012-12-03', 'U03', GetUtcDate(), '~BP'),
					(@BhPk04, @GbPkFr, 'LRN04', 'N', 'NCT', '2012-12-04', 'U04', GetUtcDate(), '~BP'),
					(newid(), @GbPkSg, 'LRN05', 'D', 'NCT', '2012-12-05', 'U05', GetUtcDate(), '~BP'),
					(newid(), @GbPkGb, 'LRN06', 'D', 'NCT', '2012-11-06', 'U06', GetUtcDate(), '~BP'),
					(newid(), @GbPkFr, 'LRN07', 'A', 'XXX', '2012-12-07', 'U07', GetUtcDate(), '~BP'),
					(newid(), @GbPkCa, 'LRN08', 'N', 'NCT', '2012-12-08', 'U08', GetUtcDate(), '~BP'),
					(newid(), @GbPkFr, 'LRN09', 'N', 'NC5', '2012-12-09', 'U09', GetUtcDate(), '~BP');

				INSERT dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_RN_NKCountryCode, CE_EntryType, CE_Category, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
					(newid(), @BhPk01, 'CusInBondHeader', 'MRN01', 'GB', 'MRN', 'CUS', '2014-01-01', 'USR', '2014-01-01', 'USR'),
					(newid(), @BhPk04, 'CusInBondHeader', 'MRN04', 'FR', 'MRN', 'CUS', '2014-01-01', 'USR', '2014-01-01', 'USR');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 5, transactions.Count());

			CombineAssertions(() =>
			{
				AssertRowMatchingRef1(transactions, "FR-04", "~FR", "^FR", new DateTime(2012, 12, 04), "U04", 1, "LRN: LRN04", "MRN: MRN04", "FR", "N");
				AssertRowMatchingRef1(transactions, "GB-01", "~GB", "^GB", new DateTime(2012, 12, 01), "U01", 1, "LRN: LRN01", "MRN: MRN01", "GB", "D");
				AssertRowMatchingRef1(transactions, "GB-02", "~GB", "^GB", new DateTime(2012, 12, 02), "U02", 1, "LRN: LRN02", "MRN: (not yet accepted)", "GB", null);
				AssertRowMatchingRef1(transactions, "IT-03", "~IT", "^IT", new DateTime(2012, 12, 03), "U03", 1, "LRN: LRN03", "MRN: (not yet accepted)", "IT", "A");
				AssertRowMatchingRef1(transactions, "FR-09", "~FR", "^FR", new DateTime(2012, 12, 09), "U09", 1, "LRN: LRN09", "MRN: (not yet accepted)", "FR", "N");
			});
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2012, 12);
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(SPTSDeclarationsCount))]
	sealed class SPTSDeclarationsCountTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @BhPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @BhPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @BhPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @BhPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @BhPk05 UNIQUEIDENTIFIER = newid();

				DECLARE @GcPkSg UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkGb UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkIt UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkFr UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkTr UNIQUEIDENTIFIER = newid();

				DECLARE @GbPkSg UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkGb UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkIt UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkFr UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkTr UNIQUEIDENTIFIER = newid();

				DECLARE @ZzbPkEuctp UNIQUEIDENTIFIER = newid();
				DECLARE @ZzbPkXxxxx UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES
					(@GcPkSg, '~SG', 'SG company', 'SG'),
					(@GcPkGb, '~GB', 'GB company', 'GB'),
					(@GcPkIt, '~IT', 'IT company', 'IT'),
					(@GcPkFr, '~FR', 'FR company', 'FR'),
					(@GcPkTr, '~TR', 'TR company', 'TR');

				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
					(@GbPkSg, '^SG', @GcPkSg),
					(@GbPkGb, '^GB', @GcPkGb),
					(@GbPkIt, '^IT', @GcPkIt),
					(@GbPkFr, '^FR', @GcPkFr),
					(@GbPkTr, '^TR', @GcPkTr);

				INSERT dbo.CusInbondHeader (BH_PK, BH_GB, BH_JobReference, BH_HeaderType, BH_ApplicationCode, BH_SystemCreateTimeUtc, BH_SystemCreateUser, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser) VALUES
					(@BhPk01, @GbPkGb, 'LRN01', 'D', 'SPT', '2012-12-01', 'U01', GetUtcDate(), '~BP'),
					(@BhPk02, @GbPkGb, 'LRN02', '' , 'SPT', '2012-12-02', 'U02', GetUtcDate(), '~BP'),
					(@BhPk03, @GbPkIt, 'LRN03', 'A', 'SPT', '2012-12-03', 'U03', GetUtcDate(), '~BP'),
					(@BhPk04, @GbPkFr, 'LRN04', 'N', 'SPT', '2012-12-04', 'U04', GetUtcDate(), '~BP'),
					(newid(), @GbPkSg, 'LRN05', 'D', 'SPT', '2012-12-05', 'U05', GetUtcDate(), '~BP'),
					(newid(), @GbPkGb, 'LRN06', 'D', 'SPT', '2012-11-06', 'U06', GetUtcDate(), '~BP'),
					(newid(), @GbPkFr, 'LRN07', 'A', 'XXX', '2012-12-07', 'U07', GetUtcDate(), '~BP'),
					(@BhPk05, @GbPkTr, 'LRN08', 'N', 'SPT', '2012-12-08', 'U08', GetUtcDate(), '~BP');

				INSERT dbo.CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_EntryNum, CE_RN_NKCountryCode, CE_EntryType, CE_Category, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
					(newid(), @BhPk01, 'CusInBondHeader', 'MRN01', 'GB', 'SPT', 'CUS', '2014-01-01', 'USR', '2014-01-01', 'USR'),
					(newid(), @BhPk05, 'CusInBondHeader', 'MRN05', 'TR', 'SPT', 'CUS', '2012-12-01', 'USR', '2012-12-01', 'USR');";

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 1, transactions.Count());
				AssertRowMatchingRef1(transactions, "TR-08", "~TR", "^TR", new DateTime(2012, 12, 01), "U08", 1, "MRN05", "LRN08", null, null);
			});
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2012, 12);
	}
}

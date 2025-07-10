using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(GlobalManifestCountTransactionForGMH))]
	sealed class GlobalManifestCountTransactionForGMHTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = $@"DECLARE @GcPkGB UNIQUEIDENTIFIER = newid();
								DECLARE @GcPkTR UNIQUEIDENTIFIER = newid();

								DECLARE @GbPkGB UNIQUEIDENTIFIER = newid();
								DECLARE @GbPkTR UNIQUEIDENTIFIER = newid();

								INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
									(@GcPkGB, '~GB', 'GB company', 'GB'),
									(@GcPkTR, '~TR', 'TR company', 'TR');

								INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
									(@GbPkGB, '^GB', @GcPkGB),
									(@GbPkTR, '^TR', @GcPkTR);

								INSERT dbo.AsycudaManifestHeader (AMA_PK, AMA_JobReference, AMA_ManifestType, AMA_ApplicationCode, AMA_SystemCreateTimeUtc, AMA_SystemLastEditTimeUtc, AMA_SystemCreateUser, AMA_SystemLastEditUser, AMA_ClusterKey, AMA_RN_NKCountry, AMA_GB) VALUES
								('{amaPk01}', 'C0000ABC1', 'IAM', 'VOC', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 1, 'TR', @GbPkTR),
								(newid(), 'C0000ABC2', 'NON', 'NVC', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 2, 'GB', @GbPkGB),
								(newid(), 'C0000ABC3', 'IAM', 'VOC', '2022-08-01', '2022-08-01', 'YGT', 'KNZ', 3, 'GB', @GbPkGB),
								(newid(), 'C0000ABC4', 'NON', 'NVC', '2022-09-01', '2022-09-01', 'YGT', 'KNZ', 4, 'TR', @GbPkTR);";

			_ = TestConnection.ExecuteNonQuery(sqlText);
		}

		readonly string amaPk01 = "F50E163F-0767-424D-AE67-FF12981EDF09";

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 8);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions("Below test is for the number of transactions and the macthing row as expected", () =>
			{
				AssertEquals("Number of Transactions", 1, transactions.Count());
				AssertRowMatchingRef1(transactions, "TR-Correct for first record", "~TR", "^TR", new DateTime(2022, 08, 01), "YGT", 1, "C0000ABC1", null, null, null);
			});
		}
	}
}

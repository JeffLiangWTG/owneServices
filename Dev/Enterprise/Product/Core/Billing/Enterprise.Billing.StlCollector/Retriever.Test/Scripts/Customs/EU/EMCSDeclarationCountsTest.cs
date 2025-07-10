using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts.Customs
{
	[TestedType(typeof(EMCSDeclarationCounts))]
	sealed class EMCSDeclarationCountsTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 8);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 3, transactions.Count());
				AssertRowMatchingRef1(transactions, "IT-08", "~IT", "^IT", new DateTime(2022, 8, 01), "IT2", 1, "DECIT02");
				AssertRowMatchingRef1(transactions, "GB-08", "~GB", "^GB", new DateTime(2022, 8, 01), "GB2", 1, "DECGB02");
				AssertRowMatchingRef1(transactions, "DE-08", "~DE", "^DE", new DateTime(2022, 8, 01), "DE2", 1, "DECDE02");
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText =
				"""
				DECLARE @GcPkUS UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkIT UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkGB UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkDE UNIQUEIDENTIFIER = newid();
				
				DECLARE @GbPkUS UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkIT UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkGB UNIQUEIDENTIFIER = newid();
				DECLARE @GbPkDE UNIQUEIDENTIFIER = newid();
				
				INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES 
					(@GcPkUS, '~US', 'US company', 'US'),
					(@GcPkIT, '~IT', 'IT company', 'IT'),
					(@GcPkGB, '~GB', 'GB company', 'GB'),
					(@GcPkDE, '~DE', 'DE company', 'DE');
				
				INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES 
					(@GbPkUS, '^US', @GcPkUS),
					(@GbPkIT, '^IT', @GcPkIT),
					(@GbPkGB, '^GB', @GcPkGB),
					(@GbPkDE, '^DE', @GcPkDE);
				
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_TransportMode, JE_ApplicationCode, JE_ClusterKey) VALUES
					(newid(), 'US', 'DECUS01', 'IMP', @GbPkUS, @GcPkUS, '2022-08-01', 'US1', 'SEA', '', 1),
					(newid(), 'IT', 'DECIT01', 'IMP', @GbPkIT, @GcPkIT, '2022-08-01', 'IT1', 'SEA', '', 2),
					(newid(), 'IT', 'DECIT02', 'IMP', @GbPkIT, @GcPkIT, '2022-08-01', 'IT2', 'SEA', 'EMC', 3),
					(newid(), 'GB', 'DECGB01', 'IMP', @GbPkGB, @GcPkGB, '2022-08-01', 'GB1', 'SEA', '', 4),
					(newid(), 'GB', 'DECGB02', 'IMP', @GbPkGB, @GcPkGB, '2022-08-01', 'GB2', 'SEA', 'EMC', 5),
					(newid(), 'DE', 'DECDE01', 'IMP', @GbPkDE, @GcPkDE, '2022-08-01', 'DE1', 'SEA', 'ITF', 6),
					(newid(), 'DE', 'DECDE02', 'IMP', @GbPkDE, @GcPkDE, '2022-08-01', 'DE2', 'AIR', 'EMC', 7);
				""";
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}

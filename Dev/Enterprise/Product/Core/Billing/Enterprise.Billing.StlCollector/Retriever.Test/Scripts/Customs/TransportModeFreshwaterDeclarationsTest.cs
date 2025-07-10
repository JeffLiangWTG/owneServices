using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Billing.StlCollector.Retriever.Scripts;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing.Scripts.Customs
{
	[TestedType(typeof(TransportModeFreshwaterDeclarations))]
	sealed class TransportModeFreshwaterDeclarationsTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2014, 3);

		protected override void PrepareTestData()
		{
			string sqlText = @"
					DECLARE @GcPkGB UNIQUEIDENTIFIER = newid();
					DECLARE @GcPkAU UNIQUEIDENTIFIER = newid();
					DECLARE @GbPkGB UNIQUEIDENTIFIER = newid();
					DECLARE @GbPkAU UNIQUEIDENTIFIER = newid();
					INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode) VALUES (@GcPkGB, 'GBC', 'GB company', 'GB'), (@GcPkAU, 'AUC', 'AU company', 'AU');
					INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES (@GbPkGB, 'GBB', @GcPkGB), (@GbPkAU, 'AUB', @GcPkAU);
					INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_TransportMode, JE_DeclarationReference, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_ClusterKey, JE_MessageType) VALUES
						(newid(), 'GB', 'IWT', 'DEC01', @GbPkGb, @GcPkGb, '2014-03-10', 'US1', 1, 'IMP'),
						(newid(), 'AU', 'IWT', 'DEC02', @GbPkAu, @GcPkAu, '2014-03-10', 'US2', 2, 'IMP'),
						(newid(), 'GB', 'IWT', 'DEC03', @GbPkGb, @GcPkGb, '2014-04-10', 'US3', 3, 'IMP'),
						(newid(), 'GB', 'SEA', 'DEC04', @GbPkGb, @GcPkGb, '2014-03-10', 'US4', 4, 'IMP'),
						(newid(), 'GB', 'BWB', 'DEC05', @GbPkGb, @GcPkGb, '2014-03-10', 'US5', 5, 'IMP'),
						(newid(), 'GB', 'LAK', 'DEC06', @GbPkGb, @GcPkGb, '2014-03-10', 'US6', 6, 'IMP'),
						(newid(), 'GB', 'RIV', 'DEC07', @GbPkGb, @GcPkGb, '2014-03-10', 'US7', 7, 'IMP');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 5, transactions.Count());

			var transaction1 = transactions.First(x => x.Reference1 == "DEC01");
			AssertEquals("CompanyCode", "GBC", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "GBB", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2014, 3, 10), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("TransactionReference", "DEC01", transaction1.Reference1);
			AssertEquals("Country", "GB", transaction1.Reference3);
			AssertEquals("TransactionReference04", "IMP", transaction1.Reference4);

			var transaction2 = transactions.First(x => x.Reference1 == "DEC02");
			AssertEquals("CompanyCode", "AUC", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "AUB", transaction2.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2014, 3, 10), transaction2.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction2.ClientStaffCode);
			AssertEquals("TransactionReference", "DEC02", transaction2.Reference1);
			AssertEquals("Country", "AU", transaction2.Reference3);
			AssertEquals("TransactionReference04", "IMP", transaction2.Reference4);

			var transaction5 = transactions.First(x => x.Reference1 == "DEC05");
			AssertEquals("CompanyCode", "GBC", transaction5.GetCompanyCode());
			AssertEquals("BranchCode", "GBB", transaction5.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2014, 3, 10), transaction5.ServiceOccuredUTC);
			AssertEquals("UserCode", "US5", transaction5.ClientStaffCode);
			AssertEquals("TransactionReference", "DEC05", transaction5.Reference1);
			AssertEquals("Country", "GB", transaction5.Reference3);
			AssertEquals("TransactionReference04", "IMP", transaction5.Reference4);

			var transaction6 = transactions.First(x => x.Reference1 == "DEC06");
			AssertEquals("CompanyCode", "GBC", transaction6.GetCompanyCode());
			AssertEquals("BranchCode", "GBB", transaction6.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2014, 3, 10), transaction6.ServiceOccuredUTC);
			AssertEquals("UserCode", "US6", transaction6.ClientStaffCode);
			AssertEquals("TransactionReference", "DEC06", transaction6.Reference1);
			AssertEquals("Country", "GB", transaction6.Reference3);
			AssertEquals("TransactionReference04", "IMP", transaction6.Reference4);

			var transaction7 = transactions.First(x => x.Reference1 == "DEC07");
			AssertEquals("CompanyCode", "GBC", transaction7.GetCompanyCode());
			AssertEquals("BranchCode", "GBB", transaction7.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2014, 3, 10), transaction7.ServiceOccuredUTC);
			AssertEquals("UserCode", "US7", transaction7.ClientStaffCode);
			AssertEquals("TransactionReference", "DEC07", transaction7.Reference1);
			AssertEquals("Country", "GB", transaction7.Reference3);
			AssertEquals("TransactionReference04", "IMP", transaction7.Reference4);
		}
	}
}

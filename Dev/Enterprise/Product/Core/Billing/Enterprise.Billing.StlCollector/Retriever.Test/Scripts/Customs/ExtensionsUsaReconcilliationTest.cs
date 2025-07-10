using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(ExtensionsUsaReconcilliation))]
	sealed class ExtensionsUsaReconcilliationTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JePk UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkPr UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkPr UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkPr);
				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'PR', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkPr;
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_ClusterKey) VALUES
					(@JePk  , 'PR', 'DEC01', 'REC', @GbPkPr, @GcPkPr, '2013-03-10', 'US1', 1),
					(newid(), 'SG', 'DEC02', 'REC', @GbPkSg, @GcPkSg, '2013-03-30', 'US2', 2),
					(newid(), 'PR', 'DEC03', 'REC', @GbPkPr, @GcPkPr, '2014-09-13', 'US3', 3),
					(newid(), 'PR', 'DEC04', 'XXX', @GbPkPr, @GcPkPr, '2013-03-31', 'US4', 4);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2013, 3, 10), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "DEC01", transaction1.Reference1);
			AssertEquals("TransactionReference02", null, transaction1.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2013, 3);
			}
		}
	}
}

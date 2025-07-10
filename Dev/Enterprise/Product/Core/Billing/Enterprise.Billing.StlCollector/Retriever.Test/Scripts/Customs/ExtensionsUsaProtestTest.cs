using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(ExtensionsUsaProtest))]
	sealed class ExtensionsUsaProtestTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JePk UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkUs UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkUs UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkUs);
				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'US', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkUs;
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_ClusterKey) VALUES
					(newid(), 'SG', 'DEC01', 'PRO', @GbPkSg, @GcPkSg, '2013-02-26', 'US1', 1),
					(newid(), 'US', 'DEC02', 'PRO', @GbPkUs, @GcPkUs, '2014-09-13', 'US2', 2),
					(newid(), 'US', 'DEC03', 'XXX', @GbPkUs, @GcPkUs, '2013-02-28', 'US3', 3),
					(@JePk  , 'US', 'DEC04', 'PRO', @GbPkUs, @GcPkUs, '2013-02-11', 'US4', 4);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("TransactionDateUtc", new DateTime(2013, 2, 11), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US4", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "DEC04", transaction1.Reference1);
			AssertEquals("TransactionReference02", null, transaction1.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2013, 2);
			}
		}
	}
}

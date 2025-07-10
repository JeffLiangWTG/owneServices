using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(ExtensionsCanadaLowValueShipment))]
	sealed class ExtensionsCanadaLowValueShipmentTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkCa UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkCa UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkCa);
				UPDATE dbo.GlbCompany SET GC_RN_NKCountryCode = 'CA', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() WHERE GC_PK = @GcPkCa;
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_ClusterKey) VALUES
					(newid(), 'CA', 'DEC01', 'LVX', @GbPkCa, @GcPkCa, '2014-03-10', 'US1', 1),
					(newid(), 'CA', 'DEC02', 'LVX', @GbPkCa, @GcPkCa, '2014-08-31', 'US2', 2),
					(newid(), 'SG', 'DEC03', 'LVX', @GbPkSg, @GcPkSg, '2014-08-01', 'US3', 3),
					(newid(), 'CA', 'DEC04', 'LVS', @GbPkCa, @GcPkCa, '2014-08-04', 'US4', 4),
					(newid(), 'CA', 'DEC05', 'XXX', @GbPkCa, @GcPkCa, '2014-08-30', 'US5', 5);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2014, 8, 31), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "DEC02", transaction1.Reference1);
			AssertEquals("TransactionReference02", null, transaction1.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 8);
			}
		}
	}
}

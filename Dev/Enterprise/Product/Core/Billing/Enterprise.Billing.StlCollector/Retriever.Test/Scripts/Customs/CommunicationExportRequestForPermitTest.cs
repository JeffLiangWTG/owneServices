using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CommunicationExportRequestForPermit))]
	sealed class CommunicationExportRequestForPermitTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JePk UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkAu UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkAu UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkAu);
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_ClusterKey) VALUES
					(newid(), 'AU', 'DEC01', 'XXX', @GbPkAu, @GcPkAu, '2014-04-01', 'US1', 1),
					(@JePk  , 'AU', 'DEC02', 'AQS', @GbPkAu, @GcPkAu, '2014-04-30', 'US2', 2),
					(newid(), 'SG', 'DEC03', 'AQS', @GbPkSg, @GcPkSg, '2014-04-01', 'US3', 3),
					(newid(), 'AU', 'DEC04', 'AQS', @GbPkAu, @GcPkAu, '2014-06-11', 'US4', 4);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2014, 4, 30), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "DEC02", transaction1.Reference1);
			AssertEquals("TransactionReference02", null, transaction1.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 4);
			}
		}
	}
}

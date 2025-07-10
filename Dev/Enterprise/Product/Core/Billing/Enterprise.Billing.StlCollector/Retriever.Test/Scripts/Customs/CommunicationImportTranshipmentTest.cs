using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CommunicationImportTranshipment))]
	sealed class CommunicationImportTranshipmentTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = $@"
				DECLARE @JePk UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk UNIQUEIDENTIFIER = newid();
				DECLARE @GcPkSg UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'SIN');
				DECLARE @GcPkAu UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPkSg UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkSg);
				DECLARE @GbPkAu UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPkAu);
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);
				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsForwardRegistered, JS_ISBooking) VALUES
				(@JsPk, 'SHP01', 1, 0);
				INSERT dbo.JobDeclaration (JE_PK, JE_DataModel, JE_DeclarationReference, JE_MessageType, JE_GB, JE_GC, JE_SystemCreateTimeUtc, JE_SystemCreateUser, JE_ClusterKey, JE_JS) VALUES
					(@JePk  , 'SG', 'DEC01', 'TNP', @GbPkSg, @GcPkSg, '2014-03-10', 'US1', 1, null),
					(newid(), 'SG', 'DEC01X', 'TNP', @GbPkSg, @GcPkSg, '2014-03-11', 'US1', 5, @JsPk),
					(newid(), 'AU', 'DEC02', 'TNP', @GbPkAu, @GcPkAu, '2014-03-01', 'US2', 2, null),
					(newid(), 'SG', 'DEC03', 'TNP', @GbPkSg, @GcPkSg, '2014-04-01', 'US3', 3, null),
					(newid(), 'SG', 'DEC04', 'XXX', @GbPkSg, @GcPkSg, '2014-03-31', 'US4', 4, null);
				INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_JobNum, JH_SystemCreateTimeUtc, JH_GB, JH_GE, JH_SystemCreateUser, JH_Status, JH_IsDisbursement) VALUES
					('{JH1}', @GcPkSg, @JePk, 'JE01', '2014-03-10', @GbPkSg, @GePk, 'US1', 'WRK', 1),
					(newid(), @GcPkSg, @JsPk, 'SHP01', '2014-03-11', @GbPkSg, @GePk, 'US1', 'WRK', 1);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());
			var transaction1 = transactions.OrderBy(x => x.Reference1).First();
			AssertEquals("CompanyCode", "SIN", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "SIN", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2014, 3, 10), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "DEC01", transaction1.Reference1);
			AssertEquals("TransactionReference02", null, transaction1.Reference2);
			AssertEquals("TransactionReference04", JH1.ToString().ToUpper(), transaction1.Reference4);

			var transaction2 = transactions.OrderBy(x => x.Reference1).Last();
			AssertEquals("CompanyCode", "SIN", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "SIN", transaction2.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2014, 3, 11), transaction2.ServiceOccuredUTC);
			AssertEquals("UserCode", "US1", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference01", "DEC01X", transaction2.Reference1);
			AssertEquals("TransactionReference02", null, transaction2.Reference2);
			AssertEquals("TransactionReference04", null, transaction2.Reference4);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 3);
			}
		}

		readonly Guid JH1 = Guid.NewGuid();
	}
}

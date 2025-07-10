using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(AgencyBillOfLading))]
	sealed class AgencyBillOfLadingTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);
				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsForwardRegistered, JS_IsShipping, JS_ShipmentStatus) VALUES
					(@JsPk01, 'SHP01', 0, 1, 'WFI'),
					(@JsPk02, 'SHP02', 0, 0, 'CNF'),
					(@JsPk03, 'SHP03', 0, 1, 'CNF'),
					(newid(), 'SHP04', 0, 1, 'WFI');
				INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_JobNum, JH_SystemCreateTimeUtc, JH_GB, JH_GE, JH_SystemCreateUser, JH_Status) VALUES
					(newid(), @GcPk, @JsPk01, 'JHS01', '2014-01-01 00:00:00', @GbPk, @GePk, 'US1', 'WRK'),
					(newid(), @GcPk, @JsPk02, 'JHS02', '2013-12-05 14:00:00', @GbPk, @GePk, 'US2', 'WRK'),
					(newid(), @GcPk, @JsPk03, 'JHS03', '2013-12-05 07:00:00', @GbPk, @GePk, 'US3', 'WRK');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2013, 12, 5, 7, 0, 0), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US3", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "JHS03", transaction1.Reference1);
			AssertEquals("TransactionReference02", "SHP03", transaction1.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				DateTime startDate = new DateTime(2013, 12, 5);
				return new RecurringRange(startDate, startDate.AddDays(1));
			}
		}
	}
}

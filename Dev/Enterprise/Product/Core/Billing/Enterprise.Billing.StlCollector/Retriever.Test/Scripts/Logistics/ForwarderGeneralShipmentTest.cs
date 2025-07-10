using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderGeneralShipment))]
	sealed class ForwarderGeneralShipmentTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = $@"
				DECLARE @JsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk05 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk06 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);
				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsForwardRegistered, JS_ISBooking) VALUES
					(@JsPk01, 'SHP01', 1, 0),
					(@JsPk02, 'SHP02', 1, 0),
					(newid(), 'SHP03', 1, 0),
					(@JsPk03, 'SHP04', 0, 1),
					(@JsPk04, 'SHP05', 0, 1),
					(newid(), 'SHP06', 0, 1),
					(@JsPk05, 'SHP07', 1, 1),
					(@JsPk06, 'SHP08', 1, 1),
					(newid(), 'SHP09', 1, 1);
				INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_JobNum, JH_SystemCreateTimeUtc, JH_GB, JH_GE, JH_SystemCreateUser, JH_Status, JH_IsDisbursement) VALUES
					(newid(), @GcPk, @JsPk01, 'SHP01', '2014-01-01', @GbPk, @GePk, 'US1', 'WRK', 1),
					(newid(), @GcPk, @JsPk02, 'SHP02', '2013-12-04', @GbPk, @GePk, 'US2', 'WRK', 0),
					(newid(), @GcPk, @JsPk03, 'SHP04', '2014-01-01', @GbPk, @GePk, 'US1', 'WRK', 0),
					('{JobHeaderPK5}', @GcPk, @JsPk04, 'SHP05', '2013-12-04', @GbPk, @GePk, 'US2', 'WRK', 1),
					(newid(), @GcPk, @JsPk05, 'SHP07', '2014-01-01', @GbPk, @GePk, 'US1', 'WRK', 1),
					(newid(), @GcPk, @JsPk06, 'SHP08', '2013-12-04', @GbPk, @GePk, 'US2', 'WRK', 0);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			//Shipment
			AssertEquals("Number of Transactions", 3, transactions.Count());
			var transaction1 = FindRowByRef1(transactions, "SHP02");
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2013, 12, 4), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction1.BillableCount);
			AssertEquals("TransactionReference02", null, transaction1.Reference2);
			AssertEquals("TransactionReference04", null, transaction1.Reference4);
			//Quick booking
			var transaction2 = FindRowByRef1(transactions, "SHP05");
			AssertEquals("CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2013, 12, 4), transaction2.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction2.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction2.BillableCount);
			AssertEquals("TransactionReference02", null, transaction2.Reference2);
			AssertEquals("TransactionReference04", null, transaction2.Reference4);
			AssertEquals("TransactionReference05", JobHeaderPK5.ToString().ToUpper(), transaction2.Reference5);
			//Quick booking converted to shipment
			var transaction3 = FindRowByRef1(transactions, "SHP08");
			AssertEquals("CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2013, 12, 4), transaction3.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction3.ClientStaffCode);
			AssertEquals("ItemCount", 1, transaction3.BillableCount);
			AssertEquals("TransactionReference02", null, transaction3.Reference2);
			AssertEquals("TransactionReference04", null, transaction3.Reference4);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2013, 12);
			}
		}

		readonly Guid JobHeaderPK5 = Guid.NewGuid();
	}
}

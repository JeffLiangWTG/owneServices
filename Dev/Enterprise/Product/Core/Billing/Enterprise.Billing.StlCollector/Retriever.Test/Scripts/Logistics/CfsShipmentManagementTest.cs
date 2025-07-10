using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(CfsShipmentManagement))]
	sealed class CfsShipmentManagementTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @JsPk05 UNIQUEIDENTIFIER = newid();
				DECLARE @MstJsA UNIQUEIDENTIFIER = newid();
				DECLARE @MstJsB UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);
				INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsCFS) VALUES
					(@JkPk01, 'CON01', 1),
					(@JkPk02, 'CON02', 1),
					(@JkPk03, 'CON03', 0),
					(newid(), 'CON04', 1);
				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_ShipmentType) VALUES
					(@MstJsA, 'MSP_A', 'STD'),
					(@MstJsB, 'MSP_B', 'CLB');
				INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_IsCFSRegistered, JS_ShipmentType, JS_JS_ColoadMasterShipment) VALUES
					(@JsPk01, 'SHP01', 1, 'STD', @MstJsA),
					(@JsPk02, 'SHP02', 1, 'STD', null),
					(@JsPk03, 'SHP03', 1, 'STD', @MstJsB),
					(@JsPk04, 'SHP04', 1, 'ASM', @MstJsA),
					(@JsPk05, 'SHP05', 0, 'STD', @MstJsA);
				INSERT dbo.JobConShipLink (JN_PK, JN_JK, JN_JS) VALUES
					(newid(), @JkPk01, @JsPk01),
					(newid(), @JkPk01, @JsPk02),
					(newid(), @JkPk01, @JsPk03),
					(newid(), @JkPk02, @JsPk01),
					(newid(), @JkPk02, @JsPk02),
					(newid(), @JkPk02, @JsPk03),
					(newid(), @JkPk02, @JsPk04),
					(newid(), @JkPk02, @JsPk05),
					(newid(), @JkPk03, @JsPk02),
					(newid(), @JkPk03, @JsPk03),
					(newid(), @JkPk03, @JsPk04);
				INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_JobNum, JH_SystemCreateTimeUtc, JH_GB, JH_GE, JH_SystemCreateUser, JH_Status) VALUES
					(newid(), @GcPk, @JkPk01, 'CON01', '2014-01-01', @GbPk, @GePk, 'US1', 'WRK'),
					(newid(), @GcPk, @JkPk02, 'CON02', '2014-02-02', @GbPk, @GePk, 'US2', 'WRK'),
					(newid(), @GcPk, @JkPk03, 'CON03', '2014-02-03', @GbPk, @GePk, 'US3', 'WRK');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "SHP01");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 2, 2), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "CON02", transaction1.Reference1);

			var transaction2 = FindRowByRef2(transactions, "SHP02");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 2, 2), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US2", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "CON02", transaction2.Reference1);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 2);
			}
		}
	}
}

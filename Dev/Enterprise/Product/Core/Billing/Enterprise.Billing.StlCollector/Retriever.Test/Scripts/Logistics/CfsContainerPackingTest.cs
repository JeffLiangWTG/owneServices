using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(CfsContainerPacking))]
	sealed class CfsContainerPackingTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @JkPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);
				INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsCFS) VALUES
					(@JkPk01, 'CON01', 1),
					(@JkPk02, 'CON02', 0),
					(@JkPk03, 'CON03', 1),
					(@JkPk04, 'CON04', 1),
					(newid(), 'CON05', 1);
				INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_JobNum, JH_SystemCreateTimeUtc, JH_GB, JH_GE, JH_SystemCreateUser, JH_Status) VALUES
					(newid(), @GcPk, @JkPk01, 'JH01', '2014-01-01', @GbPk, @GePk, 'US1', 'WRK'),
					(newid(), @GcPk, @JkPk02, 'JH02', '2014-02-02', @GbPk, @GePk, 'US2', 'WRK'),
					(newid(), @GcPk, @JkPk03, 'JH03', '2014-02-03', @GbPk, @GePk, 'US3', 'WRK'),
					(newid(), @GcPk, @JkPk04, 'JH04', '2014-02-04', @GbPk, @GePk, 'US4', 'WRK');
				INSERT dbo.JobContainer (JC_PK, JC_JK) VALUES
					(newid(), @JkPk04),
					(newid(), @JkPk04);";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "JH03");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 2, 3), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US3", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", "CON03", transaction1.Reference2);

			var transaction2 = FindRowByRef1(transactions, "JH04");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 2, 4), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US4", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 2, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference02", "CON04", transaction2.Reference2);
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

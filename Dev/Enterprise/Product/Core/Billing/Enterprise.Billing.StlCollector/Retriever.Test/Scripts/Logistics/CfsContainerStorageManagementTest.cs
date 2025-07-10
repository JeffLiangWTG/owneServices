using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(CfsContainerStorageManagement))]
	sealed class CfsContainerStorageManagementTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @JcPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @JcPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @JcPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @JcPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @JcPk05 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);
				INSERT dbo.JobContainer (JC_PK, JC_ContainerJobID, JC_IsCFSRegistered, JC_Purpose) VALUES
					(@JcPk01, 'JC01', 1, 'STR'),
					(@JcPk02, 'JC02', 1, 'XXX'),
					(@JcPk03, 'JC03', 0, 'STR'),
					(@JcPk04, 'JC04', 1, 'STR'),
					(@JcPk05, 'JC05', 1, 'STR');
				INSERT dbo.JobHeader (JH_PK, JH_GC, JH_ParentID, JH_JobNum, JH_SystemCreateTimeUtc, JH_GB, JH_GE, JH_SystemCreateUser, JH_Status) VALUES
					(newid(), @GcPk, @JcPk01, 'JH01', '2015-03-01', @GbPk, @GePk, 'US1', 'WRK'),
					(newid(), @GcPk, @JcPk02, 'JH02', '2015-03-02', @GbPk, @GePk, 'US2', 'WRK'),
					(newid(), @GcPk, @JcPk03, 'JH03', '2015-03-03', @GbPk, @GePk, 'US3', 'WRK'),
					(newid(), @GcPk, @JcPk04, 'JH04', '2015-01-04', @GbPk, @GePk, 'US4', 'WRK'),
					(newid(), @GcPk, @JcPk05, 'JH05', '2015-03-05', @GbPk, @GePk, 'US5', 'WRK');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "JH01");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2015, 3, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", "JC01", transaction1.Reference2);

			var transaction2 = FindRowByRef1(transactions, "JH05");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2015, 3, 5), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US5", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference02", "JC05", transaction2.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2015, 3);
			}
		}
	}
}

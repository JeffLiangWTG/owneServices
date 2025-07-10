using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(LandTransportManagement))]
	sealed class LandTransportManagementTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			// The maintainers of unit tests of STL billing collectors have indicated that they wish to use SQL for these unit tests. Add in values for audit columns by modifying the SQL instead of using SQL Data Objects, even though WI00437562 advocates the avoidance of direct SQL.
			string sqlText = @"
				DECLARE @jcPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @jjPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @jjPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @jjPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @jjPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk);
				INSERT dbo.JobContainer (JC_PK) VALUES
					(@jcPk01);
				INSERT dbo.JobCartage (JJ_PK, JJ_GB, JJ_ConsignmentID, JJ_SystemCreateTimeUtc, JJ_SystemCreateUser, JJ_SystemLastEditTimeUtc, JJ_SystemLastEditUser) VALUES
					(@jjPk01, @GbPk, 'JJ01', '2012-04-21', 'US1', '2012-04-21', 'US1'),
					(@jjPk02, @GbPk, 'JJ02', '2012-03-22', 'US2', '2012-03-22', 'US2'),
					(@jjPk03, @GbPk, 'JJ03', '2012-04-23', 'US3', '2012-04-23', 'US3'),
					(@jjPk04, @GbPk, 'JJ04', '2012-04-24', 'US4', '2012-04-24', 'US4');
				INSERT dbo.JobBookedCtgMove (EW_PK, EW_JJ, EW_JC_Container, EW_SystemCreateTimeUtc, EW_SystemCreateUser, EW_SystemLastEditTimeUtc, EW_SystemLastEditUser) VALUES
					(newid(), @jjPk01, null,    '2012-04-21', 'US1', '2012-04-21', 'US1'),
					(newid(), @jjPk02, null,    '2012-03-22', 'US2', '2012-03-22', 'US2'),
					(newid(), @jjPk03, null,    '2012-04-23', 'US3', '2012-04-23', 'US3'),
					(newid(), @jjPk03, null,    '2012-04-23', 'US3', '2012-04-23', 'US3'),
					(newid(), @jjPk03, null,    '2012-04-23', 'US3', '2012-04-23', 'US3'),
					(newid(), @jjPk04, null,    '2012-04-24', 'US4', '2012-04-24', 'US4'),
					(newid(), @jjPk04, @jcPk01, '2012-04-24', 'US4', '2012-04-24', 'US4');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "JJ01");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2012, 4, 21), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", null, transaction1.Reference2);

			var transaction2 = FindRowByRef1(transactions, "JJ03");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2012, 4, 23), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US3", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference02", null, transaction2.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2012, 4);
			}
		}
	}
}

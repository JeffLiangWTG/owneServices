using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(PortTransportContainerTeu))]
	sealed class PortTransportContainerTeuTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			// The maintainers of unit tests of STL billing collectors have indicated that they wish to use SQL for these unit tests. Add in values for audit columns by modifying the SQL instead of using SQL Data Objects, even though WI00437562 advocates the avoidance of direct SQL.
			string sqlText = @"
				DECLARE @jcPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @jcPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @jcPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @jcPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @jjPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @jjPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @RcPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @RcPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk);
				INSERT dbo.RefContainer (RC_PK, RC_TEU, RC_Code) VALUES
					(@RcPk01,  3, '~RC@#$001~'),
					(@RcPk02,  5, '~RC@#$002~');
				INSERT dbo.JobContainer (JC_PK, JC_RC, JC_ContainerNum, JC_SystemCreateTimeUtc, JC_SystemCreateUser) VALUES
					(@jcPk01, @RcPk01, 'JC01', '2012-02-01', 'US1'),
					(@jcPk02, @RcPk01, 'JC02', '2012-03-02', 'US2'),
					(@jcPk03, @RcPk02, 'JC03', '2012-02-03', 'US3'),
					(@jcPk04, @RcPk02, 'JC04', '2012-02-04', 'US4');
				INSERT dbo.JobCartage (JJ_PK, JJ_GB, JJ_ConsignmentID, JJ_SystemCreateTimeUtc, JJ_SystemCreateUser, JJ_SystemLastEditTimeUtc, JJ_SystemLastEditUser) VALUES
					(@jjPk01, @GbPk, 'JJ01', '2012-04-21', 'US1', '2012-04-21', 'US1'),
					(@jjPk02, @GbPk, 'JJ02', '2012-03-22', 'US2', '2012-03-22', 'US2');
				INSERT dbo.JobBookedCtgMove (EW_PK, EW_JJ, EW_JC_Container, EW_SystemCreateTimeUtc, EW_SystemCreateUser, EW_SystemLastEditTimeUtc, EW_SystemLastEditUser) VALUES
					(newid(), @jjPk01, null,    '2012-04-21', 'US1', '2012-04-21', 'US1'),
					(newid(), @jjPk01, @jcPk01, '2012-04-21', 'US1', '2012-04-21', 'US1'),
					(newid(), @jjPk01, @jcPk02, '2012-04-21', 'US1', '2012-04-21', 'US1'),
					(newid(), @jjPk02, @jcPk03, '2012-03-22', 'US2', '2012-03-22', 'US2'),
					(newid(), @jjPk02, @jcPk04, '2012-03-22', 'US2', '2012-03-22', 'US2');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByRef2(transactions, "JC01");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2012, 2, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 3, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference01", "JJ01", transaction1.Reference1);

			var transaction2 = FindRowByRef2(transactions, "JC03");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2012, 2, 3), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US3", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 5, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference01", "JJ02", transaction2.Reference1);

			var transaction3 = FindRowByRef2(transactions, "JC04");
			AssertEquals("[T3] CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2012, 2, 4), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "US4", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 5, transaction3.BillableCount);
			AssertEquals("[T3] TransactionReference01", "JJ02", transaction3.Reference1);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2012, 2);
			}
		}
	}
}

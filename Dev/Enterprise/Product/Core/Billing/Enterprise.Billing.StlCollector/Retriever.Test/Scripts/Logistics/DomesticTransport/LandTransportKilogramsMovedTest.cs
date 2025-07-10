using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(LandTransportKilogramsMoved))]
	sealed class LandTransportKilogramsMovedTest : RefStlScriptWithDefaultsTest
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
				DECLARE @jjPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @jjPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk);
				INSERT dbo.JobContainer (JC_PK, JC_GrossWeight, JC_TareWeight, JC_GrossWeightUQ) VALUES
					(@jcPk01,  150, 50, 'LB'),
					(@jcPk02,    5,  2, 'KG'),
					(@jcPk03,   10, 11, 'KG'),
					(@jcPk04, 2148,  0, 'KT');
				INSERT dbo.JobCartage (JJ_PK, JJ_GB, JJ_ConsignmentID, JJ_SystemCreateTimeUtc, JJ_SystemCreateUser, JJ_SystemLastEditTimeUtc, JJ_SystemLastEditUser) VALUES
					(@jjPk01, @GbPk, 'JJ01', '2012-03-11', 'US1', '2012-03-11', 'US1'),
					(@jjPk02, @GbPk, 'JJ02', '2012-04-22', 'US2', '2012-04-22', 'US2'),
					(@jjPk03, @GbPk, 'JJ03', '2012-03-30', 'US3', '2012-03-30', 'US3'),
					(@jjPk04, @GbPk, 'JJ04', '2012-03-24', 'US4', '2012-03-24', 'US4');
				INSERT dbo.JobBookedCtgMove (EW_PK, EW_JJ, EW_JC_Container, EW_SystemCreateTimeUtc, EW_SystemCreateUser, EW_SystemLastEditTimeUtc, EW_SystemLastEditUser) VALUES
					(newid(), @jjPk01, null   , '2012-03-11', 'US1', '2012-03-11', 'US1'),
					(newid(), @jjPk01, @jcPk01, '2012-03-11', 'US1', '2012-03-11', 'US1'),
					(newid(), @jjPk01, @jcPk02, '2012-03-11', 'US1', '2012-03-11', 'US1'),
					(newid(), @jjPk02, @jcPk02, '2012-04-22', 'US2', '2012-04-22', 'US2'),
					(newid(), @jjPk03, @jcPk03, '2012-03-30', 'US3', '2012-03-30', 'US3'),
					(newid(), @jjPk04, @jcPk04, '2012-03-24', 'US4', '2012-03-24', 'US4');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 3, 11), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US1", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 48, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "JJ01", transaction1.Reference1);
			AssertEquals("TransactionReference02", null, transaction1.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2012, 3);
			}
		}
	}
}

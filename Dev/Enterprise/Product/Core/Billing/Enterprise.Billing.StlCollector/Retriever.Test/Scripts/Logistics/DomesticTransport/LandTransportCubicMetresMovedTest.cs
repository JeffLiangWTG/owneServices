using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(LandTransportCubicMetresMoved))]
	sealed class LandTransportCubicMetresMovedTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			// The maintainers of unit tests of STL billing collectors have indicated that they wish to use SQL for these unit tests. Add in values for audit columns by modifying the SQL instead of using SQL Data Objects, even though WI00437562 advocates the avoidance of direct SQL.
			string sqlText = @"
				DECLARE @jcPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @jcPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @jcPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @jjPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @jjPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @jjPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT TOP (1) GB_PK FROM dbo.GlbBranch WHERE GB_GC = @GcPk);
				INSERT dbo.JobContainer (JC_PK, JC_GrossVolume, JC_GrossVolumeUQ) VALUES
					(@jcPk01,    5, 'M3'),
					(@jcPk02, 2000, 'D3'),
					(@jcPk03,    0, 'M3');
				INSERT dbo.JobCartage (JJ_PK, JJ_GB, JJ_ConsignmentID, JJ_SystemCreateTimeUtc, JJ_SystemCreateUser, JJ_SystemLastEditTimeUtc, JJ_SystemLastEditUser) VALUES
					(@jjPk01, @GbPk, 'JJ01', '2012-01-01', 'US1', '2012-01-01', 'US1'),
					(@jjPk02, @GbPk, 'JJ02', '2012-02-02', 'US2', '2012-02-02', 'US2'),
					(@jjPk03, @GbPk, 'JJ03', '2012-02-03', 'US3', '2012-02-03', 'US3');
				INSERT dbo.JobBookedCtgMove (EW_PK, EW_JJ, EW_JC_Container, EW_SystemCreateTimeUtc, EW_SystemCreateUser, EW_SystemLastEditTimeUtc, EW_SystemLastEditUser) VALUES
					(newid(), @jjPk01, null   , '2012-01-01', 'US1', '2012-01-01', 'US1'),
					(newid(), @jjPk01, @jcPk01, '2012-01-01', 'US1', '2012-01-01', 'US1'),
					(newid(), @jjPk02, @jcPk01, '2012-02-02', 'US1', '2012-02-02', 'US1'),
					(newid(), @jjPk02, @jcPk02, '2012-02-02', 'US1', '2012-02-02', 'US1'),
					(newid(), @jjPk03, @jcPk03, '2012-02-03', 'US1', '2012-02-03', 'US1');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertEquals("CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("TransactionDateUtc", new DateTime(2012, 2, 2), transaction1.ServiceOccuredUTC);
			AssertEquals("UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("ItemCount", 7, transaction1.BillableCount);
			AssertEquals("TransactionReference01", "JJ02", transaction1.Reference1);
			AssertEquals("TransactionReference02", null, transaction1.Reference2);
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

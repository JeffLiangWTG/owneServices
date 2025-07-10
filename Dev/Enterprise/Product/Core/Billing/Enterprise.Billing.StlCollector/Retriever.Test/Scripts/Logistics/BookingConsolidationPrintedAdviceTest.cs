using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(BookingConsolidationPrintedAdvice))]
	sealed class BookingConsolidationPrintedAdviceTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			// The maintainers of unit tests of STL billing collectors have indicated that they wish to use SQL for these unit tests. Add in values for audit columns by modifying the SQL instead of using SQL Data Objects, even though WI00437562 advocates the avoidance of direct SQL.
			string sqlText = @"
				DECLARE @KbPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @KbPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @KbPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @BranchPK UNIQUEIDENTIFIER = (SELECT TOP 1 GB_PK FROM dbo.GlbBranch);
				INSERT dbo.DtbBookingConsolidation (KB_PK, KB_JobType, KB_IsOverridden, KB_JobID, KB_SystemCreateTimeUtc, KB_SystemLastEditTimeUtc, KB_SystemCreateUser, KB_SystemLastEditUser) VALUES
					(@KbPk01, 'CSN', 0, 'KB01', '2014-01-11', '2014-01-11', 'US1', 'US1'),
					(@KbPk02, 'BKG', 0, 'KB02', '2014-01-21', '2014-01-21', 'US2', 'US2'),
					(@KbPk03, 'BKG', 1, 'KB03', '2014-01-31', '2014-01-31', 'US5', 'US5');
				INSERT dbo.DtbBooking (KM_PK, KM_KB_Booking, KM_JobID, KM_JobType, KM_SystemCreateTimeUtc, KM_SystemCreateUser, KM_GB_Branch, KM_SystemLastEditTimeUtc, KM_SystemLastEditUser) VALUES
					(newid(), @KbPk01, 'KM11', 'CSN', '2014-01-11', 'US1', @BranchPK, '2014-01-11', 'US1'),
					(newid(), @KbPk02, 'KM21', 'BKG', '2014-01-21', 'US2', @BranchPK, '2014-01-21', 'US2'),
					(newid(), @KbPk02, 'KM22', 'BKG', '2014-02-22', 'US3', @BranchPK, '2014-02-22', 'US3'),
					(newid(), @KbPk02, 'KM23', 'BKG', '2014-01-23', 'US4', @BranchPK, '2014-01-23', 'US4'),
					(newid(), @KbPk03, 'KM31', 'BKG', '2014-01-31', 'US5', @BranchPK, '2014-01-31', 'US5');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "KM21");
			AssertEquals("[T1] CompanyCode", null, transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 1, 21), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US2", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", null, transaction1.Reference2);

			var transaction2 = FindRowByRef1(transactions, "KM23");
			AssertEquals("[T2] CompanyCode", null, transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", null, transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 1, 23), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US4", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference02", null, transaction2.Reference2);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 1);
			}
		}
	}
}

using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueForComplianceReport_AJL))]
	class QueueForComplianceReport_AJLTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);
			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");

			var glAccount1PK = helper.InsertGLAccount("1234.56.09", "TestGLAccount 9");
			var glAccount2PK = helper.InsertGLAccount("1234.56.10", "TestGLAccount 10");
			InsertAccountingPeriods(helper, 2020, 2022);

			var (ajlPK1, ajlLinePK11, ajlLinePK12) = InsertTransactions(helper, branchPK, departmentPK, "2020-07-31 23:59:00", "2022-06-30 23:59:00", glAccount1PK, glAccount2PK, "001");
			var (ajlPK2, ajlLinePK21, ajlLinePK22) = InsertTransactions(helper, branchPK, departmentPK, "2020-10-31 23:59:00", "2021-03-31 23:59:00", glAccount1PK, glAccount2PK, "002");
			var (ajlPK3, ajlLinePK31, ajlLinePK32) = InsertTransactions(helper, branchPK, departmentPK, "2021-04-30 23:59:00", "2021-09-30 23:59:00", glAccount1PK, glAccount2PK, "003");
			var (ajlPK4, ajlLinePK41, ajlLinePK42) = InsertTransactions(helper, branchPK, departmentPK, "2021-07-31 23:59:00", "2021-08-31 23:59:00", glAccount1PK, glAccount2PK, "004");
			var (ajlPK5, ajlLinePK51, ajlLinePK52) = InsertTransactions(helper, branchPK, departmentPK, "2021-07-31 23:59:00", "2021-12-31 23:59:00", glAccount1PK, glAccount2PK, "005");
			var (ajlPK6, ajlLinePK61, ajlLinePK62) = InsertTransactions(helper, branchPK, departmentPK, "2021-08-31 23:59:00", "2021-10-31 23:59:00", glAccount1PK, glAccount2PK, "006");
			var (ajlPK7, ajlLinePK71, ajlLinePK72) = InsertTransactions(helper, branchPK, departmentPK, "2021-10-31 23:59:00", "2021-12-31 23:59:00", glAccount1PK, glAccount2PK, "007");
			var (ajlPK8, ajlLinePK81, ajlLinePK82) = InsertTransactions(helper, branchPK, departmentPK, "2021-10-31 23:59:00", "2022-03-31 23:59:00", glAccount1PK, glAccount2PK, "008");
			var (ajlPK9, ajlLinePK91, ajlLinePK92) = InsertTransactions(helper, branchPK, departmentPK, "2022-04-30 23:59:00", "2022-06-30 23:59:00", glAccount1PK, glAccount2PK, "009");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueForComplianceReport_AJL 'LIB', '{0}', NULL, '{1}', '{2}'", TestDbHelper.DefaultCompanyPK, "Jul 1 2021", "Jan 1 2022"));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have rows", 52, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date >= 'Jul 01 2021' AND ACQ_Date <= 'Dec 31 2021' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have rows for compliance report", 52, result.Rows.Count);

			var expectedReportSubCodes = new string[] { "*GL*AJL**202107", "*GL*AJL**202108", "*GL*AJL**202109", "*GL*AJL**202110", "*GL*AJL**202111", "*GL*AJL**202112" };
			AssertContainsExactElementsInAnyOrder("ReportSubCodes", expectedReportSubCodes, result.Rows.Cast<DataRow>().Select(x => x[0].ToString().TrimEnd(' ')).Distinct().ToArray());

			AssertNumberOfRows("Result for AJL001 should have rows for compliance report", ajlLinePK11, ajlLinePK12, 6);
			AssertNumberOfRows("Result for AJL002 should have rows for compliance report", ajlLinePK21, ajlLinePK22, 0);
			AssertNumberOfRows("Result for AJL003 should have rows for compliance report", ajlLinePK31, ajlLinePK32, 3);
			AssertNumberOfRows("Result for AJL004 should have rows for compliance report", ajlLinePK41, ajlLinePK42, 2);
			AssertNumberOfRows("Result for AJL005 should have rows for compliance report", ajlLinePK51, ajlLinePK52, 6);
			AssertNumberOfRows("Result for AJL006 should have rows for compliance report", ajlLinePK61, ajlLinePK62, 3);
			AssertNumberOfRows("Result for AJL007 should have rows for compliance report", ajlLinePK71, ajlLinePK72, 3);
			AssertNumberOfRows("Result for AJL008 should have rows for compliance report", ajlLinePK81, ajlLinePK82, 3);
			AssertNumberOfRows("Result for AJL009 should have rows for compliance report", ajlLinePK91, ajlLinePK92, 0);
		}

		void AssertNumberOfRows(string message, Guid ajlLine1PK, Guid ajlLine2PK, int count)
		{
			const string sql = "SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_ParentID = '{0}'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sql, ajlLine1PK));
			AssertEquals(message, count, result.Rows.Count);
			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sql, ajlLine2PK));
			AssertEquals(message, count, result.Rows.Count);
		}

		public static (Guid ajlPK, Guid ajlLinePK1, Guid ajlLinePK2) InsertTransactions(TestDbHelper helper, Guid branchPK, Guid departmentPK, string ajlPostDate, string ajlDueDate, Guid glAccount1PK, Guid glAccount2PK, string transactionNum)
		{
			var ajlPK = helper.InsertTransactionHeader("GL", "AJL", transactionNum, 0, helper.ToDate(ajlPostDate), branchPK, departmentPK, null, helper.ToDate(ajlDueDate));
			var ajlLinePK1 = helper.InsertTransactionLine(ajlPK, null, null, glAccount1PK, branchPK, departmentPK, null, 200, "AJL", helper.ToDate(ajlPostDate), helper.ToDate(ajlDueDate));
			var ajlLinePK2 = helper.InsertTransactionLine(ajlPK, null, null, glAccount2PK, branchPK, departmentPK, null, -200, "AJL", helper.ToDate(ajlPostDate), helper.ToDate(ajlDueDate));
			return (ajlPK, ajlLinePK1, ajlLinePK2);
		}

		public static void InsertAccountingPeriods(TestDbHelper helper, int fromYear, int toYear)
		{
			while (fromYear <= toYear)
			{
				var month = 1;
				while (month <= 12)
				{
					helper.InsertAccPeriod(fromYear, month);
					month++;
				}
				fromYear++;
			}
		}
	}
}


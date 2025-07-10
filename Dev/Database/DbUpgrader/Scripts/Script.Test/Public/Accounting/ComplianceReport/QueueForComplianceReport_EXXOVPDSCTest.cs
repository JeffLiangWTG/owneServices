using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueForComplianceReport_EXXOVPDSC))]
	class QueueForComplianceReport_EXXOVPDSCTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);

			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			InsertTransactions(helper, branchPK, departmentPK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueForComplianceReport_EXXOVPDSC 'LIB', '{0}', NULL, 'Nov 9 2015', 'Nov 10 2015'", TestDbHelper.DefaultCompanyPK));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have rows", 12, result.Rows.Count);

			var expectedCodes = new string[] {
				"*AR*OVP**-",
				"*AR*DSC**-",
				"*AR*EXX**-",
				"*AP*OVP**-",
				"*AP*DSC**-",
				"*AP*EXX**-",
				"*AR*OVP*ARCtrl*",
				"*AR*DSC*ARCtrl*",
				"*AR*EXX*ARCtrl*",
				"*AP*OVP*APCtrl*",
				"*AP*DSC*APCtrl*",
				"*AP*EXX*APCtrl*" };

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 9 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AH' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have rows", 12, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes", expectedCodes, result.Rows.Cast<DataRow>().Select(x => x[0].ToString().TrimEnd(' ')).ToArray());
		}

		public static void InsertTransactions(TestDbHelper helper, Guid branchPK, Guid departmentPK)
		{
			helper.InsertTransactionHeader("AR", "OVP", "001", 100, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			helper.InsertTransactionHeader("AR", "DSC", "002", 200, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			helper.InsertTransactionHeader("AR", "EXX", "003", 300, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			helper.InsertTransactionHeader("AP", "OVP", "004", 400, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			helper.InsertTransactionHeader("AP", "DSC", "005", 500, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			helper.InsertTransactionHeader("AP", "EXX", "006", 600, helper.ToDate("2015-11-09"), branchPK, departmentPK);
		}
	}
}


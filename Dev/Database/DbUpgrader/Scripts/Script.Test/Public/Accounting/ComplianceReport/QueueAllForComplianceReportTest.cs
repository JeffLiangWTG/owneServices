using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueAllForComplianceReport))]
	class QueueForComplianceReportTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);
			var reportPK = helper.InsertComplianceReport("LIB", helper.ToDate("2015-10-31"), helper.ToDate("2015-12-31"));

			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");

			var openingCategory = "OPN";
			var closingCategory = "CLS";

			var glAccount1PK = helper.InsertGLAccount("1234.56.09", "TestGLAccount 9");
			var glAccount2PK = helper.InsertGLAccount("1234.56.10", "TestGLAccount 10");
			QueueForComplianceReport_AJLTest.InsertAccountingPeriods(helper, 2015, 2015);
			QueueForComplianceReport_AJLTest.InsertTransactions(helper, branchPK, departmentPK, "2015-10-31 23:59:00", "2015-11-30 23:59:00", glAccount1PK, glAccount2PK, "001");
			QueueForComplianceReport_ARAPCTRTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_ARAPJNLTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_ARAPTRFTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_CBTRFEXXTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_DRCDPYTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_EXXOVPDSCTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_GJLRJLTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_INVCRDADJTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_JCJNLTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_JCJRJTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_PAYRECTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_WIPACRTest.InsertTransactions(helper, branchPK, departmentPK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueAllForComplianceReport @ReportPK = '{0}', @OpeningCategory = '{1}', @ClosingCategory = '{2}'", reportPK, openingCategory, closingCategory));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have rows", 146, result.Rows.Count);

			var expectedCodes = new string[] {
				"*AR*CRD*ARCtrl*Total",
				"*AR*INV*ARCtrl*Total",
				"*AR*ADJ*ARCtrl*Total",
				"*AR*CTR*ARCtrl*",
				"*AR*REC*Bank*-",
				"*AR*PAY*Bank*-",
				"*AR*REC*ARCtrl*",
				"*AR*PAY*ARCtrl*",
				"*AR*JNL**-",
				"*AR*JNL*ARCtrl*",
				"*AR*DSC**-",
				"*AR*OVP**-",
				"*AR*EXX**-",
				"*AR*DSC*ARCtrl*",
				"*AR*OVP*ARCtrl*",
				"*AR*EXX*ARCtrl*",
				"*AP*ADJ*APCtrl*Total",
				"*AP*CRD*APCtrl*Total",
				"*AP*INV*APCtrl*Total",
				"*AP*CTR*APCtrl*",
				"*AP*REC*Bank*-",
				"*AP*PAY*Bank*-",
				"*AP*REC*APCtrl*",
				"*AP*PAY*APCtrl*",
				"*AP*JNL**-",
				"*AP*JNL*APCtrl*",
				"*AP*DSC**-",
				"*AP*EXX**-",
				"*AP*OVP**-",
				"*AP*DSC*APCtrl*",
				"*AP*EXX*APCtrl*",
				"*AP*OVP*APCtrl*",
				"*CB*TRF*Bank*",
				"*CB*EXX*Bank*",
				"*CB*EXX**-",
				"*CB*DPY*Bank*Total",
				"*CB*DRC*Bank*Total",
				"*AR*TRF*ARCtrl*",
				"*AP*TRF*APCtrl*" };

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 9 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AH' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have rows", 43, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes", expectedCodes, result.Rows.Cast<DataRow>().Select(x => x[0].ToString().TrimEnd(' ')).Distinct().ToArray());

			expectedCodes = new string[] {
				"*JC*JRJ*ARSusp*-",
				"*JC*JRJ*JRJCtrl*",
				"*JC*JRJ**Rev-",
				"*JC*JRJ*ARSusp*Rev",
				"*CB*DPY**-",
				"*CB*DRC**-",
				"*CB*DRC*GSTOut*-",
				"*CB*DPY*GSTIn*-",
				"*CB*DPY**GSTNotRec-",
				"*JC*JNL*ARSusp*-",
				"*JC*JNL*CFX*",
				"*JC*JNL**Rev-",
				"*JC*JNL*ARSusp*Rev",
				"*JC*WIP**",
				"*JC*ACR**",
				"*JC*WIP*WIPCtrl*-",
				"*JC*ACR*ACRCtrl*-",
				"*JC*WIP**Rev-",
				"*JC*ACR**Rev-",
				"*JC*WIP*WIPCtrl*Rev",
				"*JC*ACR*ACRCtrl*Rev",
				"*AR*CRD*ARSusp*-",
				"*AR*INV*ARSusp*-",
				"*AR*ADJ*ARSusp*-",
				"*AP*ADJ*APSusp*-",
				"*AP*CRD*APSusp*-",
				"*AP*INV*APSusp*-",
				"*AR*INV*GSTOut*-",
				"*AR*INV*PenGSTOut*-",
				"*AR*CRD*GSTOut*-",
				"*AR*CRD*PenGSTOut*-",
				"*AR*ADJ*GSTOut*-",
				"*AR*ADJ*PenGSTOut*-",
				"*AP*INV*GSTIn*-",
				"*AP*INV*PenGSTIn*-",
				"*AP*CRD*GSTIn*-",
				"*AP*CRD*PenGSTIn*-",
				"*AP*ADJ*GSTIn*-",
				"*AP*ADJ*PenGSTIn*-",
				"*AP*INV**GSTNotRec-",
				"*AP*CRD**GSTNotRec-",
				"*AP*ADJ**GSTNotRec-",
				"*AR*INV**Rev-",
				"*AR*CRD**Rev-",
				"*AR*ADJ**Rev-",
				"*AP*INV**Rev-",
				"*AP*CRD**Rev-",
				"*AP*ADJ**Rev-",
				"*AR*INV*ARSusp*Rev",
				"*AR*CRD*ARSusp*Rev",
				"*AR*ADJ*ARSusp*Rev",
				"*AP*INV*APSusp*Rev",
				"*AP*CRD*APSusp*Rev",
				"*AP*ADJ*APSusp*Rev",
				"*GL*GJL**",
				"*GL*RJL**",
				"*GL*RJL**Rev-",
				"*GL*AJL**201510",
				"*GL*AJL**201511" };    // 201512 period is after AJL Due Date(Reverse Date) and should not be queued 

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date >= 'Oct 31 2015' AND ACQ_Date <= 'Nov 30 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have rows", 88, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes", expectedCodes, result.Rows.Cast<DataRow>().Select(x => x[0].ToString().TrimEnd(' ')).Distinct().ToArray());

			expectedCodes = new string[] {
				"*CT*CBT*GSTOut*-",
				"*CT*CBT*GSTIn*-",
				"*CT*CBT*PenGSTOut*",
				"*CT*CBT*PenGSTIn*",
				"*CT*CBT**-" };

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 9 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'YC' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have rows", 15, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes", expectedCodes, result.Rows.Cast<DataRow>().Select(x => x[0].ToString().TrimEnd(' ')).Distinct().ToArray());
		}
	}
}


using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueForComplianceReport_INVCRDADJ))]
	class QueueForComplianceReport_INVCRDADJTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);

			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var cTaxLinePKsAsString = InsertTransactions(helper, branchPK, departmentPK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueForComplianceReport_INVCRDADJ 'LIB', '{0}', NULL, 'Nov 9 2015', 'Nov 10 2015'", TestDbHelper.DefaultCompanyPK));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have rows", 60, result.Rows.Count);

			Func<DataTable, string[]> getReportSubCodes = table => table.Rows.Cast<DataRow>().Select(x => x[0].ToString().TrimEnd(' ')).ToArray();
			var expectedCodes = new string[] {
				"*AR*INV*ARCtrl*Total",
				"*AR*CRD*ARCtrl*Total",
				"*AR*ADJ*ARCtrl*Total",
				"*AP*INV*APCtrl*Total",
				"*AP*CRD*APCtrl*Total",
				"*AP*ADJ*APCtrl*Total" };

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 9 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AH' AND ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have AH based rows", 6, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes for AH based rows", expectedCodes, getReportSubCodes(result));

			expectedCodes = new string[] {
				"*AR*INV*ARSusp*-",
				"*AR*CRD*ARSusp*-",
				"*AR*ADJ*ARSusp*-",
				"*AP*INV*APSusp*-",
				"*AP*CRD*APSusp*-",
				"*AP*ADJ*APSusp*-",
				"*AR*INV*GSTOut*-",
				"*AR*CRD*GSTOut*-",
				"*AR*ADJ*GSTOut*-",
				"*AP*INV*GSTIn*-",
				"*AP*CRD*GSTIn*-",
				"*AP*ADJ*GSTIn*-",
				"*AP*INV**GSTNotRec-",
				"*AP*CRD**GSTNotRec-",
				"*AP*ADJ**GSTNotRec-" };

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 9 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AL' AND ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}' AND ACQ_ParentID NOT IN ({2})",
				TestDbHelper.DefaultCompanyPK, branchPK, cTaxLinePKsAsString));
			AssertEquals("Result should have rows", 15, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes for A Tax Lines", expectedCodes, getReportSubCodes(result));

			expectedCodes = new string[] {
				"*AR*INV*ARSusp*-",
				"*AR*CRD*ARSusp*-",
				"*AR*ADJ*ARSusp*-",
				"*AP*INV*APSusp*-",
				"*AP*CRD*APSusp*-",
				"*AP*ADJ*APSusp*-",
				"*AR*INV*PenGSTOut*-",
				"*AR*CRD*PenGSTOut*-",
				"*AR*ADJ*PenGSTOut*-",
				"*AR*INV**Rev-",
				"*AR*CRD**Rev-",
				"*AR*ADJ**Rev-",
				"*AR*INV*ARSusp*Rev",
				"*AR*CRD*ARSusp*Rev",
				"*AR*ADJ*ARSusp*Rev",
				"*AP*INV*PenGSTIn*-",
				"*AP*CRD*PenGSTIn*-",
				"*AP*ADJ*PenGSTIn*-",
				"*AP*INV**Rev-",
				"*AP*CRD**Rev-",
				"*AP*ADJ**Rev-",
				"*AP*INV*APSusp*Rev",
				"*AP*CRD*APSusp*Rev",
				"*AP*ADJ*APSusp*Rev" };

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 9 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AL' AND ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}' AND ACQ_ParentID IN ({2})",
				TestDbHelper.DefaultCompanyPK, branchPK, cTaxLinePKsAsString));
			AssertEquals("Result should have rows", 24, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("ReportSubCodes for C Tax Lines", expectedCodes, getReportSubCodes(result));

			expectedCodes = new string[] {
				"*CT*CBT*GSTOut*-",
				"*CT*CBT*GSTIn*-",
				"*CT*CBT*PenGSTOut*",
				"*CT*CBT*PenGSTIn*",
				"*CT*CBT**-" };

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 9 2015' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'YC' AND ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have rows", 15, result.Rows.Count);
			AssertContainsExactElementsInAnyOrder("Distinct ReportSubCodes for YC Cash Tax Lines", expectedCodes, getReportSubCodes(result).Distinct());
		}

		public static string InsertTransactions(TestDbHelper helper, Guid branchPK, Guid departmentPK)
		{
			var glAccountPK = helper.InsertGLAccount("1234.56.03", "TestGLAccount 3");
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
			var creditorOrgPK = helper.InsertOrgHeader("ZC1", "Creditor 1");
			var debtorOrgPK = helper.InsertOrgHeader("ZD1", "Debtor 1");
			var shipmentPK = helper.InsertShipment("S00000001", helper.ToDate("2015-11-09"));
			var shipmentJobPK = helper.InsertJob("S00000001", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", helper.ToDate("2015-11-09"));
			var cTaxLinePKs = new List<Guid>();

			var arInvPK = helper.InsertTransactionHeader("AR", "INV", "001", 110, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			helper.InsertTransactionLine(arInvPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 50, "REV", helper.ToDate("2015-11-09"), null, 5);
			cTaxLinePKs.Add(helper.InsertTransactionLine(arInvPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 50, "REV", helper.ToDate("2015-11-09"), helper.ToDate("2015-11-09"), 5, 0.5m, "C"));

			var arCrdPK = helper.InsertTransactionHeader("AR", "CRD", "002", 220, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			helper.InsertTransactionLine(arCrdPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 100, "REV", helper.ToDate("2015-11-09"), null, 10);
			cTaxLinePKs.Add(helper.InsertTransactionLine(arCrdPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 100, "REV", helper.ToDate("2015-11-09"), helper.ToDate("2015-11-09"), 10, 0.5m, "C"));

			var arAdjPK = helper.InsertTransactionHeader("AR", "ADJ", "003", 330, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			helper.InsertTransactionLine(arAdjPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 150, "REV", helper.ToDate("2015-11-09"), null, 15);
			cTaxLinePKs.Add(helper.InsertTransactionLine(arAdjPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 150, "REV", helper.ToDate("2015-11-09"), helper.ToDate("2015-11-09"), 15, 0.5m, "C"));

			var apInvPK = helper.InsertTransactionHeader("AP", "INV", "001", 110, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			helper.InsertTransactionLine(apInvPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 50, "CST", helper.ToDate("2015-11-09"), null, 5, 0.5m, "A");
			cTaxLinePKs.Add(helper.InsertTransactionLine(apInvPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 50, "CST", helper.ToDate("2015-11-09"), helper.ToDate("2015-11-09"), 5, 0.5m, "C"));

			var apCrdPK = helper.InsertTransactionHeader("AP", "CRD", "002", 220, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			helper.InsertTransactionLine(apCrdPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 100, "CST", helper.ToDate("2015-11-09"), null, 10, 0.5m, "A");
			cTaxLinePKs.Add(helper.InsertTransactionLine(apCrdPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 100, "CST", helper.ToDate("2015-11-09"), helper.ToDate("2015-11-09"), 10, 0.5m, "C"));

			var apAdjPK = helper.InsertTransactionHeader("AP", "ADJ", "003", 330, helper.ToDate("2015-11-09"), branchPK, departmentPK);
			helper.InsertTransactionLine(apAdjPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 150, "CST", helper.ToDate("2015-11-09"), null, 15, 0.5m, "A");
			cTaxLinePKs.Add(helper.InsertTransactionLine(apAdjPK, shipmentJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, creditorOrgPK, 150, "CST", helper.ToDate("2015-11-09"), helper.ToDate("2015-11-09"), 15, 0.5m, "C"));

			foreach (var linePK in cTaxLinePKs)
			{
				helper.InsertCashBasisVAT(linePK, helper.ToDate("2015-11-09"));
			}

			var pksBuilder = new StringBuilder();
			cTaxLinePKs.ForEach(x => pksBuilder.AppendFormat("'{0}',", x));
			return pksBuilder.ToString().TrimEnd(',');
		}
	}
}


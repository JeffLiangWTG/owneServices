using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueTransactionsForZMReport))]
	class QueueTransactionsForZMReportTest : DbCreateScriptTest
	{
		TestDbHelper Helper => helper ?? (helper = new TestDbHelper(TestConnection));
		TestDbHelper helper;

		[ExpectNoExceptions]
		public void TestSampleCall()
		{
			var reportPK = Helper.InsertComplianceReport("ZMD", helper.ToDate("2021-01-01"), helper.ToDate("2021-01-31"));

			var debtorGermanyPK = CreateDebtorWithAddressAndRegNo("DEB1", "DebtorDE", "Hamburg", "DE", "UST", "123456");
			var debtorFrancePK = CreateDebtorWithAddressAndRegNo("DEB2", "DebtorFR", "Paris", "FR", "TVA", "234567");
			var debtorAustraliaPK = CreateDebtorWithAddressAndRegNo("DEB3", "DebtorAU", "Sydney", "AU", "LSC", "345678");
			var debtorFranceTwoAddressesPK = CreateDebtorWithAddressAndRegNo("DEB4", "DebtorFR2", "Tour", "FR", "TVA", "765432");
			var addressPK = Helper.InsertOrgAddress(debtorFranceTwoAddressesPK, "Paris", "PARISADDRESS", "FR");
			Helper.Insert("OrgAddressCapability", new { PZ_PK = Guid.NewGuid(), PZ_IsValid = 1, PZ_AddressType = "DLV", PZ_IsMainAddress = 0, PZ_OA = addressPK });
			// create 2 debtors without VAT ID
			var debtor2FrancePK = CreateDebtorWithAddressAndRegNo("DEB5", "Debtor2FR", "Paris", "FR");
			var debtor3FrancePK = CreateDebtorWithAddressAndRegNo("DEB6", "Debtor3FR", "Grenoble", "FR");

			var reverseTax = Helper.InsertTaxRate("REV", true, "Reverse Tax", "RVS");

			var inv1 = Helper.InsertTransactionHeader("AR", "INV", "ARINV001", 100m, helper.ToDate("2021-01-11"), org: debtorGermanyPK);
			var line1 = Helper.InsertTransactionLine(inv1, taxRatePK: reverseTax);
			var inv2 = Helper.InsertTransactionHeader("AR", "INV", "ARINV002", 200m, helper.ToDate("2021-01-11"), org: debtorFrancePK);
			var line2 = Helper.InsertTransactionLine(inv2, taxRatePK: reverseTax);
			var inv3 = Helper.InsertTransactionHeader("AR", "INV", "ARINV003", 300m, helper.ToDate("2021-01-11"), org: debtorAustraliaPK);
			var line3 = Helper.InsertTransactionLine(inv3, taxRatePK: reverseTax);
			var inv4 = Helper.InsertTransactionHeader("AR", "INV", "ARINV004", 400m, helper.ToDate("2021-01-11"), org: debtorFranceTwoAddressesPK);
			var line4 = Helper.InsertTransactionLine(inv4, taxRatePK: reverseTax);
			// create AR transactions for the debtors without VAT ID
			var inv5 = Helper.InsertTransactionHeader("AR", "INV", "ARINV005", 500m, helper.ToDate("2021-01-11"), org: debtor2FrancePK);
			var line5 = Helper.InsertTransactionLine(inv5, taxRatePK: reverseTax);
			var inv6 = Helper.InsertTransactionHeader("AR", "INV", "ARINV006", 600m, helper.ToDate("2021-01-11"), org: debtor3FrancePK);
			var line6 = Helper.InsertTransactionLine(inv6, taxRatePK: reverseTax);
			// cancel the invoice of DEB6 (debtor3FrancePK)
			DataUtils.GetDataTableFromQuery(TestConnection, $@"UPDATE dbo.AccTransactionHeader SET AH_IsCancelled = 1, AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK = '{inv6}'");
			var inv7 = Helper.InsertTransactionHeader("AR", "CRD", "ARCRD007", -600m, helper.ToDate("2021-01-12"), org: debtor3FrancePK);
			var line7 = Helper.InsertTransactionLine(inv6, taxRatePK: reverseTax);
			DataUtils.GetDataTableFromQuery(TestConnection, $@"UPDATE dbo.AccTransactionHeader SET AH_IsCancelled = 1, AH_TransactionBelongsToGroup = '{inv6}', AH_ReceiptType = 'WOR', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_PK = '{inv7}'");

			using (var command = TestConnection.Command("EXEC QueueTransactionsForZMReport @ReportPK, @CountryBusinessRegCodeMapping"))
			{
				var countryToTaxRegistrationCode = new Dictionary<string, string>();
				countryToTaxRegistrationCode.Add("FR", "TVA");  // France
				countryToTaxRegistrationCode.Add("NL", "BTW");  // Netherlands
				var mappingTable = CountryAndBusinessRegistrationTypeToTable(countryToTaxRegistrationCode);
				command.AddParameter("@ReportPK", SqlDbType.UniqueIdentifier, reportPK);
				command.AddTableValuedParameter("@CountryBusinessRegCodeMapping", "dbo.TVP_CountryAndBusinessRegType", mappingTable);
				command.ExecuteNonQuery();
			}

			// we expect only the 3 invoice from the French debtors with and w/o VAT ID but not the cancelled transactions, because Australia is not part of the EU and Germany is not outside of Germany
			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID, ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue WHERE ACQ_Date = '2021-01-11' AND ACQ_ReportType = 'ZMD' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}'",
				TestDbHelper.DefaultCompanyPK));
			AssertEquals("Result should have rows", 3, result.Rows.Count);
			var sortedRows = result.AsEnumerable().OrderBy(v => v.Field<string>("ACQ_ReportSubCode")).ToArray();
			AssertEquals("ParentID of first row", line2, (Guid)sortedRows[0][0]);
			AssertEquals("ReportSubCode of first row", "FR234567", (string)sortedRows[0][1]);
			AssertEquals("ParentID of second row", line4, (Guid)sortedRows[1][0]);
			AssertEquals("ReportSubCode of second row", "FR765432", (string)sortedRows[1][1]);
			AssertEquals("ParentID of third row", line5, (Guid)sortedRows[2][0]);
			AssertEquals("ReportSubCode of third row", "VAT ID missing", (string)sortedRows[2][1]);
		}

		public void TestRaiseError()
		{
			var reportPK = Helper.InsertComplianceReport("ZMD", helper.ToDate("2021-01-01"), helper.ToDate("2021-01-31"));

			var debtorFranceTwoAddressesPK = CreateDebtorWithAddressAndRegNo("DEB1", "DebtorFR", "Tour", "FR", "TVA", "765432");
			var addressPK = Helper.InsertOrgAddress(debtorFranceTwoAddressesPK, "Amsterdam", "AMSADDRESS", "NL");
			Helper.Insert("OrgCusCode", new { OK_PK = Guid.NewGuid(), OK_OH = debtorFranceTwoAddressesPK, OK_CodeType = "BTW", OK_RN_NKCodeCountry = "NL", OK_CustomsRegNo = "111222" });
			Helper.Insert("OrgAddressCapability", new { PZ_PK = Guid.NewGuid(), PZ_IsValid = 1, PZ_AddressType = "OFC", PZ_IsMainAddress = 1, PZ_OA = addressPK });

			var reverseTax = Helper.InsertTaxRate("REV", true, "Reverse Tax", "RVS");

			var inv1 = Helper.InsertTransactionHeader("AR", "INV", "ARINV001", 100m, helper.ToDate("2021-01-11"), org: debtorFranceTwoAddressesPK);
			var line1 = Helper.InsertTransactionLine(inv1, taxRatePK: reverseTax);

			using (var command = TestConnection.Command("EXEC QueueTransactionsForZMReport @ReportPK, @CountryBusinessRegCodeMapping"))
			{
				var countryToTaxRegistrationCode = new Dictionary<string, string>();
				countryToTaxRegistrationCode.Add("FR", "TVA");  // France
				countryToTaxRegistrationCode.Add("NL", "BTW");  // Netherlands
				var mappingTable = CountryAndBusinessRegistrationTypeToTable(countryToTaxRegistrationCode);
				command.AddParameter("@ReportPK", SqlDbType.UniqueIdentifier, reportPK);
				command.AddTableValuedParameter("@CountryBusinessRegCodeMapping", "dbo.TVP_CountryAndBusinessRegType", mappingTable);
				AssertExceptionThrown(typeof(SqlException), "The DEB1 Organisation has more than one registration number eligible for the ZMD Compliance report type.", () =>
				{
					command.ExecuteNonQuery();
				});
			}

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID, ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue WHERE ACQ_Date = '2021-01-11' AND ACQ_ReportType = 'ZMD' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}'",
				TestDbHelper.DefaultCompanyPK));
			AssertEquals("Result should have no rows", 0, result.Rows.Count);
		}

		Guid CreateDebtorWithAddressAndRegNo(string orgCode, string fullName, string address, string country, string codeType = null, string businessRegNo = null)
		{
			var debtorPK = Helper.InsertOrgHeader(orgCode, fullName);
			Helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = TestDbHelper.DefaultCompanyPK, OB_OH = debtorPK, OB_IsDebtor = 1, OB_IsCreditor = 0 });
			if (!string.IsNullOrEmpty(codeType) && !string.IsNullOrEmpty(businessRegNo))
			{
				Helper.Insert("OrgCusCode", new { OK_PK = Guid.NewGuid(), OK_OH = debtorPK, OK_CodeType = codeType, OK_RN_NKCodeCountry = country, OK_CustomsRegNo = businessRegNo });
			}
			var addressPK = Helper.InsertOrgAddress(debtorPK, address, orgCode + "ADR1", country);
			Helper.Insert("OrgAddressCapability", new { PZ_PK = Guid.NewGuid(), PZ_IsValid = 1, PZ_AddressType = "OFC", PZ_IsMainAddress = 1, PZ_OA = addressPK });
			return debtorPK;
		}

		DataTable CountryAndBusinessRegistrationTypeToTable(Dictionary<string, string> dictionary)
		{
			DataTable temp = null;
			DataTable result = null;

			try
			{
				temp = new DataTable();
				temp.Locale = CultureInfo.InvariantCulture;
				temp.Columns.Add("Country", typeof(string)); // Part of SQL code
				temp.Columns.Add("BusinessRegType", typeof(string)); // Part of SQL code

				foreach (var entry in dictionary)
				{
					var row = temp.NewRow();
					row["Country"] = entry.Key;
					row["BusinessRegType"] = entry.Value;
					temp.Rows.Add(row);
				}

				result = temp;
				temp = null;
			}
			finally
			{
				if (temp != null)
				{
					temp.Dispose();
				}
			}
			return result;
		}
	}
}

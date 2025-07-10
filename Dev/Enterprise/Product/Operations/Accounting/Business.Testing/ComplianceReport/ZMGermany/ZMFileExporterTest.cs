using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.Testing;
using Enterprise.Accounting.Business.ComplianceReport.ZMGermany;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.ZMGermany
{
	public class ZMFileExporterTest : TestCaseWithFactory
	{
		ZMFileExporter Exporter;
		ZMGermanyReport ZmReport;
		AccComplianceReport ComplianceReport;
		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		protected override void SetUp()
		{
			base.SetUp();

			Creator.CreateTestPeriods(new ZDateTime(2022, 1, 1));
			Creator.CreateTestPeriods(new ZDateTime(2023, 1, 1));
			ComplianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			ComplianceReport.ACR_ReportType = AccountingConstants.ComplianceReportTypes.ZusammenfassendeMeldungGermanyReportType;
			ComplianceReport.ACR_DateFrom = new ZDate(2022, 1, 1);
			ComplianceReport.ACR_DateTo = new ZDate(2022, 1, 31);
			ComplianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor, "FR", "123456", 1000.0m));
			ComplianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor, "FR", "123456", 500.0m));
			ComplianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor, "FR", "123456", 250.0m));
			ComplianceReport.ReportLines.Add(CreateReportLine(Creator.DebtorTR, "PT", "654321", 4000.0m));
			ComplianceReport.ReportLines.Add(CreateReportLine(Creator.DebtorDE, "PT", "555555", 5000.0m));

			ZmReport = new ZMGermanyReport(ComplianceReport);
			_ = ZmReport.TaxReturnHeaderList;
			ZmReport.SelectedVersion = "1";
			ZmReport.MarkSelectedVersionAsGenerated();
			Factory.Save();

			Exporter = new ZMFileExporter(ZmReport, ComplianceReport);
		}

		[TestDate(2022, 10, 4)]
		public void TestExport()
		{
			var exportResult = Exporter.Export();
			var expectedFileContent = "0      20221004EDI CUSTOMS BROKERS                          10 HUTCHESON STREET ALBIO4010 Brisbane                      "
									+ "1DE41 065 89102122FR123456      000000001750S1010                                                                       "
									+ "1DE41 065 89102122PT654321      000000004000S1010                                                                       "
									+ "1DE41 065 89102122PT555555      000000005000S1010                                                                       "
									+ "2DE41 065 8921220000000001075000003                                                                                     ";
			AssertEquals("File content for version 1", expectedFileContent, exportResult.FileContent);
			AssertEquals("Number of new records for version 1", 3, exportResult.NewRecords);
			AssertEquals("Number of updated records for version 1", 0, exportResult.UpdatedRecords);
			AssertEquals("Filenamefor version 1", "m5_zm__001EDI_v01_z20220101_m22038_ca.mgp", exportResult.Filename);

			AddNewReportVersion();

			exportResult = Exporter.Export();
			expectedFileContent = "0      20221004EDI CUSTOMS BROKERS                          10 HUTCHESON STREET ALBIO4010 Brisbane                      "
								+ "1DE41 065 89102122PT999999      00000002000-S1010                                                                       "
								+ "1DE41 065 89112122FR123456      000000001850S1010                                                                       "
								+ "2DE41 065 8921220000000000150-00002                                                                                     ";
			AssertEquals("File content for version 2", expectedFileContent, exportResult.FileContent);
			AssertEquals("Number of new records for version 2", 1, exportResult.NewRecords);
			AssertEquals("Number of updated records for version 2", 1, exportResult.UpdatedRecords);
			AssertEquals("Filenamefor version 2", "m5_zm__002EDI_v01_z20220101_m22038_ca.mgp", exportResult.Filename);
		}

		#region Implementation Tests

		public void TestGenerateFilename()
		{
			AssertEquals("Filename for initial message with monthly processing", "m5_zm_SENDERM_001EDI_v01_z20220101_m22038_ca.mgp", Exporter.GenerateFilename("SENDERM"));
			ComplianceReport.ACR_DateTo = new ZDate(2022, 3, 31);
			AssertEquals("Filename for initial message with quarterly processing", "m5_zm_SENDERQ_001EDI_v01_z20220101_q22094_ca.mgp", Exporter.GenerateFilename("SENDERQ"));
			ComplianceReport.ACR_DateTo = new ZDate(2022, 12, 31);
			AssertEquals("Filename for initial message with yearly processing", "m5_zm_SENDERY_001EDI_v01_z20221231_j23002_ca.mgp", Exporter.GenerateFilename("SENDERY"));

			AddNewReportVersion();

			ComplianceReport.ACR_DateTo = new ZDate(2022, 1, 31);
			AssertEquals("Filename for update message with monthly processing", "m5_zm_SENDERM_002EDI_v01_z20220101_m22038_ca.mgp", Exporter.GenerateFilename("SENDERM"));
			ComplianceReport.ACR_DateTo = new ZDate(2022, 3, 31);
			AssertEquals("Filename for update message with quarterly processing", "m5_zm_SENDERQ_002EDI_v01_z20220101_q22094_ca.mgp", Exporter.GenerateFilename("SENDERQ"));
			ComplianceReport.ACR_DateTo = new ZDate(2022, 12, 31);
			AssertEquals("Filename for update message with yearly processing", "m5_zm_SENDERY_002EDI_v01_z20221231_j23002_ca.mgp", Exporter.GenerateFilename("SENDERY"));
		}

		public void TestCalculateProcessInterval()
		{
			AssertEquals("Process interval for 1 month and end date 2022-01-31", "m22038", Exporter.CalculateProcessInterval());
			ComplianceReport.ACR_DateFrom = new ZDate(2022, 2, 1);
			ComplianceReport.ACR_DateTo = new ZDate(2022, 2, 28);
			AssertEquals("Process interval for 1 month and end date 2022-02-28", "m22066", Exporter.CalculateProcessInterval());
			ComplianceReport.ACR_DateFrom = new ZDate(2022, 1, 1);
			ComplianceReport.ACR_DateTo = new ZDate(2022, 3, 31);
			AssertEquals("Process interval for 1 quarter and end date 2022-03-31", "q22094", Exporter.CalculateProcessInterval());
			ComplianceReport.ACR_DateFrom = new ZDate(2022, 10, 1);
			ComplianceReport.ACR_DateTo = new ZDate(2022, 12, 31);
			AssertEquals("Process interval for 1 quarter and end date 2022-12-31", "q23002", Exporter.CalculateProcessInterval());
			ComplianceReport.ACR_DateFrom = new ZDate(2022, 1, 1);
			ComplianceReport.ACR_DateTo = new ZDate(2022, 12, 31);
			AssertEquals("Process interval for 1 year and end date 2022-12-31", "j23002", Exporter.CalculateProcessInterval());
			ComplianceReport.ACR_DateFrom = new ZDate(2023, 1, 1);
			ComplianceReport.ACR_DateTo = new ZDate(2023, 12, 31);
			AssertEquals("Process interval for 1 year and end date 2023-12-31", "j24001", Exporter.CalculateProcessInterval());
		}

		public void TestGetReportRange()
		{
			for (var month = 1; month <= 12; month++)
			{
				ComplianceReport.ACR_DateFrom = new ZDate(2022, month, 1);
				ComplianceReport.ACR_DateTo = ComplianceReport.ACR_DateFrom.AddMonths(1).AddDays(-1);
				AssertEquals($"Report range for {ComplianceReport.ACR_DateFrom.ToShortDateString()} to {ComplianceReport.ACR_DateTo.ToShortDateString()}", $"{(month + 20):D2}22", Exporter.GetReportRange());
			}

			for (var quarter = 1; quarter <= 4; quarter++)
			{
				ComplianceReport.ACR_DateFrom = new ZDate(2022, quarter * 3 - 2, 1);
				ComplianceReport.ACR_DateTo = ComplianceReport.ACR_DateFrom.AddMonths(3).AddDays(-1);
				AssertEquals($"Report range for {ComplianceReport.ACR_DateFrom.ToShortDateString()} to {ComplianceReport.ACR_DateTo.ToShortDateString()}", $"{quarter:D2}22", Exporter.GetReportRange());
			}

			ComplianceReport.ACR_DateFrom = new ZDate(2023, 1, 1);
			ComplianceReport.ACR_DateTo = new ZDate(2023, 12, 1);
			AssertEquals($"Report range for {ComplianceReport.ACR_DateFrom.ToShortDateString()} to {ComplianceReport.ACR_DateTo.ToShortDateString()}", "0523", Exporter.GetReportRange());
		}

		[TestDate(2022, 2, 15)]
		public void TestProcessCompanyData()
		{
			var actual = Exporter.ProcessCompanyData();
			var expected = "0      20220215EDI CUSTOMS BROKERS                          10 HUTCHESON STREET ALBIO4010 Brisbane                      ";
			AssertEquals("Company's org. proxy record", expected, actual);
		}

		public void TestCheckOrganisationStatus()
		{
			AddNewReportVersion();

			var taxReturnHeader = ZmReport.CreateTaxReturnHeader(ComplianceReport, 1);
			var recordToCheck = ZmReport.CreateTaxReturnLine(taxReturnHeader, Creator.ABIGAS, "ES", "333333", 1000);
			AssertEquals("New organisation record", ZMFileExporter.OrganisationStatus.New, Exporter.CheckOrganisationStatus(recordToCheck));

			recordToCheck = ZmReport.CreateTaxReturnLine(taxReturnHeader, Creator.DebtorDE, "PT", "555555", 7000);
			AssertEquals("Modified organisation record", ZMFileExporter.OrganisationStatus.Modified, Exporter.CheckOrganisationStatus(recordToCheck));
		}

		public void TestProcessOrganisationData()
		{
			var actual = Exporter.ProcessOrganisationData(CreateTaxReturnLine("ES444444", 1500), "DE123456789", false, "2122");
			var expected = "1DE123456789102122ES444444      000000001500S1010                                                                       ";
			AssertEquals("Organisation ES444444 with amount 1500", expected, actual);

			actual = Exporter.ProcessOrganisationData(CreateTaxReturnLine("PT555555", -1600), "DE123456789", true, "3222");
			expected = "1DE123456789113222PT555555      00000001600-S1010                                                                       ";
			AssertEquals("Organisation PT555555 with corrected amount -1600", expected, actual);
		}

		public void TestCreateUSTIDNumber()
		{
			AssertEquals("USTID without country code", "PT555555", Exporter.CreateUSTIDNumber("555555", "PT"));
			AssertEquals("USTID with country code", "ES999999", Exporter.CreateUSTIDNumber("ES999999", "ES"));
		}

		public void TestCreateSummary()
		{
			var exportedRecords = ZmReport.TaxReturnHeaderList.FirstOrDefault(v => v.ATR_Version == 1).Lines.Find(v => true).ToList();
			var actual = Exporter.CreateSummary(exportedRecords, "DE123456789", "0122");
			var expected = "2DE12345678901220000000001075000003                                                                                     ";
			AssertEquals("Summary record", expected, actual);
		}

		#endregion

		AccComplianceReportLine CreateReportLine(OrgHeader organisation, string countryCode, string businessRegNo, decimal amount)
		{
			var row = AccComplianceReportLineTest.GetDataRow(Factory);
			row[AccComplianceReportLine.Schema.OH_Code] = organisation.OH_Code;
			row[AccComplianceReportLine.Schema.OrgCountryCode] = countryCode;
			row[AccComplianceReportLine.Schema.OK_CustomsRegNo] = businessRegNo;
			row[AccComplianceReportLine.Schema.TotalExTaxAmount] = amount;
			return new AccComplianceReportLine(Factory, row);
		}

		AccTaxReturnLine CreateTaxReturnLine(ZString orgRegNo, ZDecimal amount)
		{
			var taxReturnHeader = Creator.CreateAccTaxReturn(ComplianceReport, false, true);
			var taxReturnLine = taxReturnHeader.Lines[0];
			taxReturnLine.ARL_OrgRegNo = orgRegNo;
			taxReturnLine.ARL_RN_NKCountryCode = orgRegNo.Substring(0, 2);
			taxReturnLine.ARL_TotalAmountIncludingTax = amount;
			return taxReturnLine;
		}

		void AddNewReportVersion()
		{
			ComplianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor, "FR", "123456", 100.0m));
			ComplianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor1, "PT", "999999", -2000.0m));
			ZmReport = new ZMGermanyReport(ComplianceReport);
			_ = ZmReport.TaxReturnHeaderList;
			Factory.Save();
			Exporter = new ZMFileExporter(ZmReport, ComplianceReport);
		}
	}
}

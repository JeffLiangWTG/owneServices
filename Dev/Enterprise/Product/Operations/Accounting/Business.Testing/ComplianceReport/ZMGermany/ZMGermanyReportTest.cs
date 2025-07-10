using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.ComplianceReport.Testing;
using Enterprise.Accounting.Business.ComplianceReport.ZMGermany;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.Business.Testing.ComplianceReport.ZMGermany
{
	[TestedType(typeof(ZMGermanyReport))]
	public class ZMGermanyReportTest : NonPersistentBusinessObjectTestCase
	{
		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		public void TestGenerateAndGetTaxReturnLines()
		{
			var complianceConfig = CreateComplianceReportConfiguration();

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig))
			{
				var complianceReport = CreateZMComplianceReport(
					CreateReportLine(Creator.Debtor, "FR", "123456", 1000.0m),
					CreateReportLine(Creator.Debtor1, "FR", "987654", 2000.0m),
					CreateReportLine(Creator.Debtor, "FR", "123456", 3000.0m),
					CreateReportLine(Creator.DebtorTR, "PT", "123456", 4000.0m),
					CreateReportLine(Creator.DebtorDE, "PT", "555555", 5000.0m));
				Factory.Save();

				var zmGenerator = new ZMGermanyReport(complianceReport);
				var linesToDisplay = zmGenerator.GridData;
				var headers = zmGenerator.TaxReturnHeaderList;
				AssertEquals("Number of generated headers", 1, headers.Count);
				var header = headers.First();
				AssertEquals("Header status", AccTaxReturn.Status.Saved, header.ATR_Status);
				AssertEquals("Number of generated lines", 4, header.Lines.Count);
				var linesDictionary = headers.First().Lines.OfType<AccTaxReturnLine>().ToDictionary(v => ZMGermanyReport.GetLineKey(v), v => v);
				AssertEquals("Amount for FR123456", 4000.0m, linesDictionary["FR123456"].ARL_TotalAmountIncludingTax);
				AssertEquals("Amount for FR987654", 2000.0m, linesDictionary["FR987654"].ARL_TotalAmountIncludingTax);
				AssertEquals("Amount for PT123456", 4000.0m, linesDictionary["PT123456"].ARL_TotalAmountIncludingTax);
				AssertEquals("Amount for PT555555", 5000.0m, linesDictionary["PT555555"].ARL_TotalAmountIncludingTax);
				zmGenerator.SelectedVersion = "1";
				zmGenerator.MarkSelectedVersionAsGenerated();
				Factory.Save();

				// repeat the same process; but this time the data is read from dbo.AccTaxReturn/AccTaxReturnLine table
				zmGenerator = new ZMGermanyReport(complianceReport);
				linesToDisplay = zmGenerator.GridData;
				headers = zmGenerator.TaxReturnHeaderList;
				AssertEquals("Number of retrieved headers (second run)", 1, headers.Count);
				header = headers.First();
				AssertEquals("Header status (second run)", AccTaxReturn.Status.Generated, header.ATR_Status);
				AssertEquals("Number of retrieved lines (second run)", 4, linesToDisplay.Count);
				var versionedLinesDictionary = linesToDisplay.OfType<AccTaxReturnLine>().ToDictionary(v => ZMGermanyReport.GetLineKeyWithVersion(v), v => v);
				AssertEquals("Amount for FR123456", 4000.0m, versionedLinesDictionary["1:FR123456"].ARL_TotalAmountIncludingTax);
				AssertEquals("Amount for FR987654", 2000.0m, versionedLinesDictionary["1:FR987654"].ARL_TotalAmountIncludingTax);
				AssertEquals("Amount for PT123456", 4000.0m, versionedLinesDictionary["1:PT123456"].ARL_TotalAmountIncludingTax);
				AssertEquals("Amount for PT555555", 5000.0m, versionedLinesDictionary["1:PT555555"].ARL_TotalAmountIncludingTax);
			}
		}

		public void TestMultiVersion()
		{
			var complianceConfig = CreateComplianceReportConfiguration();

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig))
			{
				var complianceReport = CreateZMComplianceReport();

				// generate version 1
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor, "FR", "123456", 1000.0m));
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor1, "PT", "555555", 2000.0m));
				complianceReport.ReportLines.Add(CreateReportLine(Creator.DebtorDE, "ES", "987654", 500.0m));
				var zmGenerator = new ZMGermanyReport(complianceReport);
				_ = zmGenerator.TaxReturnHeaderList;
				zmGenerator.SelectedVersion = "1";
				zmGenerator.MarkSelectedVersionAsGenerated();
				Factory.Save();

				// generate version 2 (more transactions for FR + ES and reversal for PT)
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor, "FR", "123456", 500.0m));
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor1, "PT", "555555", -2000.0m));
				complianceReport.ReportLines.Add(CreateReportLine(Creator.DebtorDE, "ES", "987654", 100.0m));
				zmGenerator = new ZMGermanyReport(complianceReport);
				_ = zmGenerator.TaxReturnHeaderList;
				zmGenerator.SelectedVersion = "2";
				zmGenerator.MarkSelectedVersionAsGenerated();
				Factory.Save();

				// generate version 3 (credit note for FR, reversal to version 1 amount for ES, no changes for PT and a new debtor)
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor, "FR", "123456", -800.0m));
				complianceReport.ReportLines.Add(CreateReportLine(Creator.DebtorDE, "ES", "987654", -100.0m));
				complianceReport.ReportLines.Add(CreateReportLine(Creator.DebtorTR, "BE", "444444", 400.0m));
				zmGenerator = new ZMGermanyReport(complianceReport);
				_ = zmGenerator.TaxReturnHeaderList;
				Factory.Save();

				zmGenerator = new ZMGermanyReport(complianceReport);
				var linesToDisplay = zmGenerator.GridData;
				AssertEquals("Number of retrieved lines", 9, linesToDisplay.Count);
				var linesDictionary = linesToDisplay.OfType<AccTaxReturnLine>().ToDictionary(v => ZMGermanyReport.GetLineKeyWithVersion(v), v => v);
				AssertEquals("Version 1 amount for FR123456", 1000.0m, linesDictionary["1:FR123456"].ARL_TotalAmountIncludingTax);
				AssertEquals("Version 2 amount for FR123456", 1500.0m, linesDictionary["2:FR123456"].ARL_TotalAmountIncludingTax);
				AssertEquals("Version 3 amount for FR123456", 700.0m, linesDictionary["3:FR123456"].ARL_TotalAmountIncludingTax);
				AssertEquals("Version 1 amount for PT555555", 2000.0m, linesDictionary["1:PT555555"].ARL_TotalAmountIncludingTax);
				AssertEquals("Version 2 amount for PT555555", 0.0m, linesDictionary["2:PT555555"].ARL_TotalAmountIncludingTax);
				AssertEquals("Version 1 amount for ES987654", 500.0m, linesDictionary["1:ES987654"].ARL_TotalAmountIncludingTax);
				AssertEquals("Version 2 amount for ES987654", 600.0m, linesDictionary["2:ES987654"].ARL_TotalAmountIncludingTax);
				AssertEquals("Version 3 amount for ES987654", 500.0m, linesDictionary["3:ES987654"].ARL_TotalAmountIncludingTax);
				AssertEquals("Version 3 amount for BE444444", 400.0m, linesDictionary["3:BE444444"].ARL_TotalAmountIncludingTax);
			}
		}

		public void TestVersionAffectsGrid()
		{
			var complianceConfig = CreateComplianceReportConfiguration();

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig))
			{
				var complianceReport = CreateZMComplianceReport();

				// generate version 1
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor, "FR", "123456", 1000.0m));
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor1, "PT", "555555", 2000.0m));
				complianceReport.ReportLines.Add(CreateReportLine(Creator.DebtorDE, "ES", "987654", 500.0m));
				var zmGenerator = new ZMGermanyReport(complianceReport);
				_ = zmGenerator.TaxReturnHeaderList;
				zmGenerator.SelectedVersion = "1";
				zmGenerator.MarkSelectedVersionAsGenerated();
				Factory.Save();

				// generate version 2 (one more transaction for FR)
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor, "FR", "123456", 500.0m));
				zmGenerator = new ZMGermanyReport(complianceReport);
				_ = zmGenerator.TaxReturnHeaderList;
				Factory.Save();

				zmGenerator.SelectedVersion = ZMGermanyReport.AllVersions;
				AssertEquals("Filter on all versions", 4, zmGenerator.GridData.Count);
				zmGenerator.SelectedVersion = "1";
				AssertEquals("Filter on version 1", 3, zmGenerator.GridData.Count);
				zmGenerator.SelectedVersion = "2";
				AssertEquals("Filter on version 2", 1, zmGenerator.GridData.Count);
				zmGenerator.SelectedVersion = "9";
				AssertEquals("Filter on invalid version", 0, zmGenerator.GridData.Count);
			}
		}

		public void TestValidateCurrentTaxReturn()
		{
			var complianceReport = CreateZMComplianceReport(
				CreateReportLine(Creator.Debtor, "FR", "123456", 1000.0m));

			var zmGenerator = new ZMGermanyReport(complianceReport);
			var taxReturnHeader = zmGenerator.TaxReturnHeaderList.FirstOrDefault();

			AssertEquals("Missing registration ID and sender ID", "Please enter your BUNDESZENTRALAMT FUER STEUERN REGISTRATION-ID as registration code in your company.\r\nPlease enter your BUNDESZENTRALAMT FUER STEUERN SENDERKENNUNG as registration code in your company.", zmGenerator.ValidateCurrentTaxReturn());
			taxReturnHeader.ATR_VATRegNo = "REGNO";
			AssertEquals("Missing sender ID", "Please enter your BUNDESZENTRALAMT FUER STEUERN SENDERKENNUNG as registration code in your company.", zmGenerator.ValidateCurrentTaxReturn());
			taxReturnHeader.ATR_VATRegNo = ZString.Empty;
			taxReturnHeader.ATR_GovtReturnIdentifier = "SENDERID";
			AssertEquals("Missing registration ID", "Please enter your BUNDESZENTRALAMT FUER STEUERN REGISTRATION-ID as registration code in your company.", zmGenerator.ValidateCurrentTaxReturn());
			taxReturnHeader.ATR_VATRegNo = "REGNO";
			AssertEquals("No missing information", ZString.Empty, zmGenerator.ValidateCurrentTaxReturn());
		}

		public void TestCanGenerate()
		{
			var complianceConfig = CreateComplianceReportConfiguration();

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig))
			{
				var complianceReport = CreateZMComplianceReport();

				// generate version 1
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor1, "PT", "555555", 2000.0m));
				var zmGenerator = new ZMGermanyReport(complianceReport);
				_ = zmGenerator.TaxReturnHeaderList;
				zmGenerator.SelectedVersion = "1";
				zmGenerator.MarkSelectedVersionAsGenerated();
				Factory.Save();

				// generate version 2
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor, "FR", "123456", 500.0m));
				zmGenerator = new ZMGermanyReport(complianceReport);
				_ = zmGenerator.TaxReturnHeaderList;
				Factory.Save();

				var taxReturnHeaderVersion1 = zmGenerator.TaxReturnHeaderList.FirstOrDefault(v => v.ATR_Version == 1);
				var taxReturnHeaderVersion2 = zmGenerator.TaxReturnHeaderList.FirstOrDefault(v => v.ATR_Version == 2);
				taxReturnHeaderVersion1.ATR_Status = AccTaxReturn.Status.Generated;
				AssertEquals("Status GEN + SAV", false, zmGenerator.CanGenerate());

				taxReturnHeaderVersion1.ATR_Status = AccTaxReturn.Status.Submitted;
				AssertEquals("Status SUB + SAV", true, zmGenerator.CanGenerate());

				taxReturnHeaderVersion2.ATR_Status = AccTaxReturn.Status.Generated;
				AssertEquals("Status SUB + GEN", false, zmGenerator.CanGenerate());

				taxReturnHeaderVersion2.ATR_Status = AccTaxReturn.Status.Submitted;
				AssertEquals("Status SUB + SUB", true, zmGenerator.CanGenerate());
			}
		}

		public void TestMarkGeneratedVersionRowAsSubmitted()
		{
			var complianceConfig = CreateComplianceReportConfiguration();

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig))
			{
				var complianceReport = CreateZMComplianceReport();

				// generate version 1
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor1, "PT", "555555", 2000.0m));
				var zmGenerator = new ZMGermanyReport(complianceReport);
				_ = zmGenerator.TaxReturnHeaderList;
				zmGenerator.SelectedVersion = "1";
				zmGenerator.MarkSelectedVersionAsGenerated();
				Factory.Save();

				// generate version 2
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor, "FR", "123456", 500.0m));
				zmGenerator = new ZMGermanyReport(complianceReport);
				_ = zmGenerator.TaxReturnHeaderList;
				Factory.Save();

				var taxReturnHeaderVersion1 = zmGenerator.TaxReturnHeaderList.FirstOrDefault(v => v.ATR_Version == 1);
				var taxReturnHeaderVersion2 = zmGenerator.TaxReturnHeaderList.FirstOrDefault(v => v.ATR_Version == 2);

				AssertEquals("Status of generated version 1", AccTaxReturn.Status.Generated, taxReturnHeaderVersion1.ATR_Status);
				AssertEquals("Status of new version 2", AccTaxReturn.Status.Saved, taxReturnHeaderVersion2.ATR_Status);

				zmGenerator.MarkGeneratedVersionRowAsSubmitted();
				AssertEquals("Status of submitted version 1", AccTaxReturn.Status.Submitted, taxReturnHeaderVersion1.ATR_Status);
				AssertEquals("Status of new version 2", AccTaxReturn.Status.Saved, taxReturnHeaderVersion2.ATR_Status);

				zmGenerator.SelectedVersion = "2";
				zmGenerator.MarkSelectedVersionAsGenerated();
				zmGenerator.MarkGeneratedVersionRowAsSubmitted();
				AssertEquals("Status of submitted version 1", AccTaxReturn.Status.Submitted, taxReturnHeaderVersion1.ATR_Status);
				AssertEquals("Status of submitted version 2", AccTaxReturn.Status.Submitted, taxReturnHeaderVersion2.ATR_Status);
			}
		}

		public void TestTemporaryTaxReturnHeaderWillNotBeSaved()
		{
			var complianceConfig = CreateComplianceReportConfiguration();

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig))
			{
				// generate version 1 (temporary, not saved)
				var complianceReport = CreateZMComplianceReport(
					CreateReportLine(Creator.Debtor, "FR", "123456", 1000.0m),
					CreateReportLine(Creator.DebtorDE, "ES", "987654", 500.0m));

				var zmGenerator = new ZMGermanyReport(complianceReport);
				var taxReturnHeader = zmGenerator.TaxReturnHeaderList.Single();
				zmGenerator.SelectedVersion = "1";

				Assert("Version 1 should have changes", taxReturnHeader.HasChanges);
				Factory.Save();
				Assert("Version 1 should not have been saved to the database", !taxReturnHeader.IsInDatabase);
				AssertEquals("Version 1 should have status 'SAV'", AccTaxReturn.Status.Saved, taxReturnHeader.ATR_Status);

				taxReturnHeader = Factory.Load<AccTaxReturn>(taxReturnHeader.PK);
				AssertNull("Version 1 should not exist in the database", taxReturnHeader);
			}
		}

		public void TestTemporaryTaxReturnHeaderWillBeSavedOnGenerate()
		{
			var complianceConfig = CreateComplianceReportConfiguration();

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig))
			{
				// generate version 1 and simulate export
				var complianceReport = CreateZMComplianceReport(
					CreateReportLine(Creator.Debtor, "FR", "123456", 1000.0m),
					CreateReportLine(Creator.DebtorDE, "ES", "987654", 500.0m));

				var zmGenerator = new ZMGermanyReport(complianceReport);
				var taxReturnHeader = zmGenerator.TaxReturnHeaderList.Single();
				zmGenerator.SelectedVersion = "1";
				zmGenerator.MarkSelectedVersionAsGenerated();

				Assert("Version 1 should have changes", taxReturnHeader.HasChanges);
				Factory.Save();
				Assert("Version 1 should have been saved to the database", taxReturnHeader.IsInDatabase);
				AssertEquals("Version 1 should have status 'GEN'", AccTaxReturn.Status.Generated, taxReturnHeader.ATR_Status);

				taxReturnHeader = Factory.Load<AccTaxReturn>(taxReturnHeader.PK);
				AssertNotNull("Version 1 should exist in the database", taxReturnHeader);
			}
		}

		public void TestTemporaryTaxReturnHeaderMultiVersion()
		{
			var complianceConfig = CreateComplianceReportConfiguration();

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig))
			{
				// generate version 1 and simulate export
				var complianceReport = CreateZMComplianceReport();
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor, "FR", "123456", 1000.0m));
				complianceReport.ReportLines.Add(CreateReportLine(Creator.DebtorDE, "ES", "987654", 500.0m));

				var zmGenerator = new ZMGermanyReport(complianceReport);
				var taxReturnHeader = zmGenerator.TaxReturnHeaderList.Single();
				zmGenerator.SelectedVersion = "1";
				zmGenerator.MarkSelectedVersionAsGenerated();

				Assert("Version 1 should have changes", taxReturnHeader.HasChanges);
				Factory.Save();
				Assert("Version 1 should have been saved to the database", taxReturnHeader.IsInDatabase);
				AssertEquals("Version 1 should have status 'GEN'", AccTaxReturn.Status.Generated, taxReturnHeader.ATR_Status);

				taxReturnHeader = Factory.Load<AccTaxReturn>(taxReturnHeader.PK);
				AssertNotNull("Version 1 should exist in the database", taxReturnHeader);

				// generate version 2 (more transactions for FR, new transaction for PT)
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor, "FR", "123456", 500.0m));
				complianceReport.ReportLines.Add(CreateReportLine(Creator.Debtor1, "PT", "555555", 2000.0m));

				zmGenerator = new ZMGermanyReport(complianceReport);
				taxReturnHeader = zmGenerator.TaxReturnHeaderList.OrderBy(x => x.ATR_Version).Last();
				zmGenerator.SelectedVersion = "2";

				Assert("Version 2 should have changes", taxReturnHeader.HasChanges);
				Factory.Save();
				Assert("Version 2 should not have been saved to the database", !taxReturnHeader.IsInDatabase);
				AssertEquals("Version 2 should have status 'SAV'", AccTaxReturn.Status.Saved, taxReturnHeader.ATR_Status);

				taxReturnHeader = Factory.Load<AccTaxReturn>(taxReturnHeader.PK);
				AssertNull("Version 2 should not exist in the database", taxReturnHeader);
			}
		}

		public void TestReportLines_AmountSignsAreReversed_WhenAllSettingsAreAP()
		{
			var assertion = "Amount sign should be reversed because all the ledger types are AP in the report settings.";
			var settings = new ComplianceReportConfigurationSettingCollection();
			var setting1 = settings.AddNew();
			setting1.LedgerType = LedgerTypes.AccountsPayable;
			setting1.InvoiceType = "PAY";
			setting1.OriginalRule = "ALL";
			setting1.DisbursementRule = "ALL";
			setting1.TaxInvoiceRule = "ALL";
			setting1.Country = Constants.CountryCodes.Germany;
			var setting2 = settings.AddNew();
			setting2.LedgerType = LedgerTypes.AccountsPayable;
			setting2.InvoiceType = "CRD";
			setting2.OriginalRule = "ALL";
			setting2.DisbursementRule = "ALL";
			setting2.TaxInvoiceRule = "ALL";
			setting2.Country = Constants.CountryCodes.Germany;

			AssertReportLines_AmountSigns(settings, assertion, isReverseSign: true);
		}

		public void TestReportLines_AmountSignsAreNotReversed_WhenNotAllSettingsAreAP()
		{
			var assertion = "Amount sign should be as provided and not be reversed because not all the ledger types are AP in the report settings.";
			var settings = new ComplianceReportConfigurationSettingCollection();
			var setting1 = settings.AddNew();
			setting1.LedgerType = LedgerTypes.AccountsPayable;
			setting1.InvoiceType = "PAY";
			setting1.OriginalRule = "ALL";
			setting1.DisbursementRule = "ALL";
			setting1.TaxInvoiceRule = "ALL";
			setting1.Country = Constants.CountryCodes.Germany;
			var setting2 = settings.AddNew();
			setting2.LedgerType = LedgerTypes.AccountsReceivable;
			setting2.InvoiceType = "CRD";
			setting2.OriginalRule = "ALL";
			setting2.DisbursementRule = "ALL";
			setting2.TaxInvoiceRule = "ALL";
			setting2.Country = Constants.CountryCodes.Germany;

			AssertReportLines_AmountSigns(settings, assertion, isReverseSign: false);
		}

		public void TestReportLines_AmountSignsAreNotReversed_WhenSettingsIsEmpty()
		{
			var assertion = "Amount sign should be as provided and not be reversed when the report settings is empty.";
			var settings = new ComplianceReportConfigurationSettingCollection();
			AssertReportLines_AmountSigns(settings, assertion, isReverseSign: false);
		}

		void AssertReportLines_AmountSigns(ComplianceReportConfigurationSettingCollection settings, string assertion, bool isReverseSign)
		{
			using var country = GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Germany);
			var reportConfiguration = Creator.EnsureComplianceReportConfigInRegistry("ZMD", "UST", ReportPeriodicityCodes.MonthlyQuarterlyYearly, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TransactionHeaderAndReportSubCode, Constants.CountryCodes.Germany);
			reportConfiguration.Settings.AddRange(settings);
			var reportConfigurations = new ComplianceReportConfigurationCollection { reportConfiguration };

			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations))
			{
				var complianceReport = CreateZMComplianceReport();
				Factory.Save();

				var invoice = Creator.CreateInvoiceWithLine(typeof(ARInvoice), "I0001", Creator.EUR, 1m, 200m, 0m, 200m, 0m);
				invoice.AH_OH = Creator.AALSHI.PK;
				Assert("Has Lines", invoice.Lines.Count > 0);

				var line = invoice.Lines[0];
				line.AL_AC = Creator.CC10.PK;
				line.AL_AT = Creator.GST1.PK;
				Factory.Save();

				Creator.CreateComplianceReportTransactionPivot(complianceReport, invoice);
				Creator.CreateComplianceReportTransactionPivot(complianceReport, line);
				var zmReport = new ZMGermanyReport(complianceReport);
				zmReport.SelectedVersion = "1";
				zmReport.MarkSelectedVersionAsGenerated();
				Factory.Save();

				AssertEquals(1, zmReport.ComplianceReport.ReportLines.Count);
				var reportLine = zmReport.ComplianceReport.ReportLines[0];
				AssertEquals(reportLine.AH_InvoiceAmount, 200.0m);
				var signFactor = isReverseSign ? -1 : 1;
				AssertEquals(assertion, reportLine.ServiceExTaxAmount, signFactor * 200.0m);
				AssertEquals(assertion, reportLine.TotalExTaxAmount, signFactor * 200.0m);
				AssertEquals(assertion, reportLine.ServiceTaxAmount, signFactor * 20.0m);
				AssertEquals(assertion, reportLine.TotalTaxAmount, signFactor * 20.0m);
			}
		}

		ComplianceReportConfigurationCollection CreateComplianceReportConfiguration()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var report = complianceConfig.AddNew();
			report.ReportCode = "ZMD";
			report.ReportTitle = "Zusammenfassende Meldung";
			report.ReportPeriodicity = "PER";
			report.Country = Env.CurrentCompany.Country.Code;
			report.TaxRegistrationType = "GCR";
			report.ReportBaseTablePrefix = "**";
			return complianceConfig;
		}

		AccComplianceReportLine CreateReportLine(OrgHeader organisation, string countryCode, string businessRegNo, decimal amount)
		{
			var row = AccComplianceReportLineTest.GetDataRow(Factory);
			row[AccComplianceReportLine.Schema.OH_Code] = organisation.OH_Code;
			row[AccComplianceReportLine.Schema.OrgCountryCode] = countryCode;
			row[AccComplianceReportLine.Schema.OK_CustomsRegNo] = businessRegNo;
			row[AccComplianceReportLine.Schema.TotalExTaxAmount] = amount;
			return new AccComplianceReportLine(Factory, row);
		}

		AccComplianceReport CreateZMComplianceReport(params AccComplianceReportLine[] reportLines)
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_ReportType = ComplianceReportTypes.ZusammenfassendeMeldungGermanyReportType;
			complianceReport.ACR_DateFrom = new ZDate(2021, 1, 1);
			complianceReport.ACR_DateTo = new ZDate(2021, 1, 31);

			if (!reportLines.IsNullOrEmpty())
			{
				complianceReport.ReportLines.AddRange(reportLines);
			}

			return complianceReport;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			return new ZMGermanyReport(complianceReport);
		}
	}
}

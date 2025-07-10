using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.GUI.ComplianceReport;
using Enterprise.Accounting.GUI.ComplianceReport.PTRS;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;
using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AccComplianceReportForm))]
	internal sealed class AccComplianceReportForm_BasherTest : ZFormBasherTest
	{
		public void TestReportButtonClicksShowErrorWhenFormHasChanges()
		{
			AssertReportButtonClicksShowErrorWhenFormHasChanges("generateButton", "Please, save Report before generating");
			AssertReportButtonClicksShowErrorWhenFormHasChanges("finaliseButton", "Please, save Report before finalizing");
			AssertReportButtonClicksShowErrorWhenFormHasChanges("reQueueButton", "Please, save Report before re-queuing");

			void AssertReportButtonClicksShowErrorWhenFormHasChanges(string buttonName, string expectedMessage)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
				{
					var objectCreator = new TestObjectCreator(Factory);
					objectCreator.CreateTestPeriods(ZDateTime.Today);

					var report = objectCreator.CreateComplianceReport(AccComplianceReport.ReportTypes.SAFT, AccComplianceReport.Status.ReportGenerated);
					Factory.Save();

					using (var form = new AccComplianceReportForm(report))
					{
						form.Show();
						report.ACR_Status = AccComplianceReport.Status.ReportFinalised;
						Assert(report.HasChanges);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

						var button = (ZButton)form.Controls.Find(buttonName, true)[0];
						button.PerformClick();

						AssertNullOrEmpty("No error reports generated", ErrorReporter.LastMessageReported);
						AssertEquals("Error message shown to the user", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestComplianceReportGLWithoutTabs()
		{
			using (var testForm = (AccComplianceReportForm)GetFormToBash())
			{
				testForm.Show();
				var formBizo = (AccComplianceReport)testForm.BusinessEntity;
				Assert(!formBizo.SupportsGLBalanceDetails);
				var reportTabControl = testForm.GetControl<ZTemplateTabControl>("reportTabControl");
				Assert("reportLinesCurrentPeriodTabPage.TabVisible", reportTabControl.TabPages.ContainsKey("reportLinesCurrentPeriodTabPage"));
				Assert("reportLinesPreviousPeriodTabPage.TabVisible", !reportTabControl.TabPages.ContainsKey("reportLinesPreviousPeriodTabPage"));
				Assert("GLAccountsOpeningBalanceTabPage.TabVisible", !reportTabControl.TabPages.ContainsKey("GLAccountsOpeningBalanceTabPage"));
				Assert("GLAccountsClosingBalanceTabPage.TabVisible", !reportTabControl.TabPages.ContainsKey("GLAccountsClosingBalanceTabPage"));
				Assert("GLAccountsMovementsTabPage.TabVisible", !reportTabControl.TabPages.ContainsKey("GLAccountsMovementsTabPage"));
			}
		}

		public void TestComplianceReportGLWithTabs()
		{
			var objectCreator = new TestObjectCreator(Factory);
			objectCreator.EnsureComplianceReportConfigInRegistry("TST", "ABN", ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);
			var report = objectCreator.CreateComplianceReport("TST", status: AccComplianceReport.Status.ReportFinalised);

			using (var testForm = new AccComplianceReportForm(report))
			{
				testForm.Show();
				var formBizo = (AccComplianceReport)testForm.BusinessEntity;

				Assert(formBizo.SupportsGLBalanceDetails);
				var reportTabControl = testForm.GetControl<ZTemplateTabControl>("reportTabControl");
				Assert("reportLinesCurrentPeriodTabPage.TabVisible", reportTabControl.TabPages.ContainsKey("reportLinesCurrentPeriodTabPage"));
				Assert("reportLinesPreviousPeriodTabPage.TabVisible", !reportTabControl.TabPages.ContainsKey("reportLinesPreviousPeriodTabPage"));
				Assert("GLAccountsOpeningBalanceTabPage.TabVisible", reportTabControl.TabPages.ContainsKey("GLAccountsOpeningBalanceTabPage"));
				Assert("GLAccountsClosingBalanceTabPage.TabVisible", reportTabControl.TabPages.ContainsKey("GLAccountsClosingBalanceTabPage"));
				Assert("GLAccountsMovementsTabPage.TabVisible", reportTabControl.TabPages.ContainsKey("GLAccountsMovementsTabPage"));
				AssertEquals("Report Lines", reportTabControl.AllTabPages.Cast<ZTabPage>().FirstOrDefault(p => p.Name == "reportLinesCurrentPeriodTabPage").CaptionResourceString.Caption);
			}
		}

		public void TestReportLinesGeneratedAndShown()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			var creator = new TestObjectCreator(Factory);
			creator.CreateConfigurationForComplianceReport(report, "AL", "");
			Factory.Save();

			var invoice = creator.CreateAPInvoice<APInvoice>("I001", creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, creator.ABIGAS);
			Assert(invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = creator.GST2.PK;
			var line2 = creator.CreateInvoiceLine(invoice, creator.AUD, 1m, 250m);
			line2.AL_AT = creator.GSTFREE1.PK;
			Factory.Save();

			creator.CreateComplianceReportQueueEntry(report, line1, line2);
			Factory.Save();

			using (var testForm = new AccComplianceReportForm(report))
			{
				testForm.Show();
				var gridLines = testForm.Controls.Find("reportCurrentPeriodLinesGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull(gridLines);
				AssertEquals(0, gridLines.List.Count);

				var generateButton = testForm.Controls.Find("generateButton", true).FirstOrDefault() as ZButton;
				AssertNotNull(generateButton);
				generateButton.PerformClick();

				var formReloaded = new ZFormUtilitiesTest().GetFormCreatedByReloading(testForm);
				gridLines = formReloaded.Controls.Find("reportCurrentPeriodLinesGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull(gridLines);
				AssertEquals(2, gridLines.List.Count);
				formReloaded.Close();
			}
		}

		public void TestReportLinesPreviousPeriodTabPage()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			var creator = new TestObjectCreator(Factory);
			creator.CreateConfigurationForComplianceReport(report, "AL", "", includeQueuedForPreviousPeriod: true);

			var previousReport = Factory.NewWithValidTestData<AccComplianceReport>();
			previousReport.ACR_ReportType = report.ACR_ReportType;
			previousReport.ACR_DateFrom = report.ACR_DateFrom.AddDays(-10);
			previousReport.ACR_DateTo = report.ACR_DateFrom.AddDays(-1);
			previousReport.ACR_Status = AccComplianceReport.Status.ReportFinalised;
			Factory.Save();

			var previousPeriodInvoice = creator.CreateAPInvoice<APInvoice>("I000" + report.ReportLines.Count.ToString(), creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, creator.ABIGAS);
			previousPeriodInvoice.AH_PostDate = report.ACR_DateFrom.AddDays(-1);
			Assert("Has Lines", previousPeriodInvoice.Lines.Count > 0);
			var line01 = previousPeriodInvoice.Lines[0];
			line01.AL_AT = creator.GST1.PK;
			var line02 = creator.CreateInvoiceLine(previousPeriodInvoice, creator.AUD, 1m, 200m);
			line02.AL_AT = creator.GSTFREE1.PK;

			var invoice = creator.CreateAPInvoice<APInvoice>("I001" + report.ReportLines.Count.ToString(), creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m, creator.ABIGAS);
			Assert("Has Lines", invoice.Lines.Count > 0);
			var line1 = invoice.Lines[0];
			line1.AL_AT = creator.GST2.PK;
			var line2 = creator.CreateInvoiceLine(invoice, creator.AUD, 1m, 250m);
			line2.AL_AT = creator.GSTFREE1.PK;
			Factory.Save();

			creator.CreateComplianceReportQueueEntry(report, line01, line02);
			creator.CreateComplianceReportQueueEntry(report, line1, line2);
			report.GenerateFromQueue();

			report.ClearReportLines_ForTestOnly();

			using (var testForm = new AccComplianceReportForm(report))
			{
				testForm.Show();
				var formBizo = (AccComplianceReport)testForm.BusinessEntity;

				var reportTabControl = testForm.GetControl<ZTemplateTabControl>("reportTabControl");
				Assert("reportLinesCurrentPeriodTabPage.TabVisible", reportTabControl.TabPages.ContainsKey("reportLinesCurrentPeriodTabPage"));
				Assert("reportLinesPreviousPeriodTabPage.TabVisible", reportTabControl.TabPages.ContainsKey("reportLinesPreviousPeriodTabPage"));
				Assert("GLAccountsOpeningBalanceTabPage.TabVisible", !reportTabControl.TabPages.ContainsKey("GLAccountsOpeningBalanceTabPage"));
				Assert("GLAccountsClosingBalanceTabPage.TabVisible", !reportTabControl.TabPages.ContainsKey("GLAccountsClosingBalanceTabPage"));
				Assert("GLAccountsMovementsTabPage.TabVisible", !reportTabControl.TabPages.ContainsKey("GLAccountsMovementsTabPage"));

				AssertEquals(2, report.ReportLinesCurrentPeriod.Count);
				AssertEquals(2, report.ReportLinesPreviousPeriod.Count);

				AssertEquals("Report Lines Current Period", reportTabControl.AllTabPages.Cast<ZTabPage>().FirstOrDefault(p => p.Name == "reportLinesCurrentPeriodTabPage").CaptionResourceString.Caption);
				AssertEquals("Report Lines Previous Period", reportTabControl.AllTabPages.Cast<ZTabPage>().FirstOrDefault(p => p.Name == "reportLinesPreviousPeriodTabPage").CaptionResourceString.Caption);
			}
		}

		public void TestReportLinesPreviousPeriodTabPageWithoutLines()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			var creator = new TestObjectCreator(Factory);
			creator.CreateConfigurationForComplianceReport(report, "AL", "", includeQueuedForPreviousPeriod: true);

			report.ClearReportLines_ForTestOnly();

			using (var testForm = new AccComplianceReportForm(report))
			{
				testForm.Show();
				var formBizo = (AccComplianceReport)testForm.BusinessEntity;

				var reportTabControl = testForm.GetControl<ZTemplateTabControl>("reportTabControl");
				Assert("reportLinesCurrentPeriodTabPage.TabVisible", reportTabControl.TabPages.ContainsKey("reportLinesCurrentPeriodTabPage"));
				Assert("reportLinesPreviousPeriodTabPage.TabVisible", !reportTabControl.TabPages.ContainsKey("reportLinesPreviousPeriodTabPage"));
				Assert("GLAccountsOpeningBalanceTabPage.TabVisible", !reportTabControl.TabPages.ContainsKey("GLAccountsOpeningBalanceTabPage"));
				Assert("GLAccountsClosingBalanceTabPage.TabVisible", !reportTabControl.TabPages.ContainsKey("GLAccountsClosingBalanceTabPage"));
				Assert("GLAccountsMovementsTabPage.TabVisible", !reportTabControl.TabPages.ContainsKey("GLAccountsMovementsTabPage"));
				AssertEquals(0, report.ReportLinesCurrentPeriod.Count);
				AssertEquals(0, report.ReportLinesPreviousPeriod.Count);
				AssertEquals("Report Lines", reportTabControl.AllTabPages.Cast<ZTabPage>().FirstOrDefault(p => p.Name == "reportLinesCurrentPeriodTabPage").CaptionResourceString.Caption);
			}
		}

		public void TestComplianceReportGenerateSAFTXmlIsNotVisible()
		{
			using (var testForm = (AccComplianceReportForm)GetFormToBash())
			{
				testForm.Show();
				var formBizo = (AccComplianceReport)testForm.BusinessEntity;
				AssertNotEquals(Core.Constants.CountryCodes.Portugal, GlbCompany.CurrentCompany.Country);

				var menuItem = testForm.Menu.MenuItems.FindByText("Generate Monthly Transaction XML", true);
				AssertNull("Generate SAFT Xml menuItem is not visible for non Portugal countries", menuItem);

				var generateXmlButton = (ZButton)testForm.Controls.Find("generateXmlButton", true)[0];
				Assert("Generate SAFT Xml button is not visible", !generateXmlButton.Visible);
				Assert("Generate SAFT Xml button is not enabled", !generateXmlButton.Enabled);
			}
		}

		public void TestComplianceReportGenerateSAFTXmlIsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateTestPeriods(ZDateTime.Today);

				objectCreator.EnsureComplianceReportConfigInRegistry("TST", "IVA", ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);
				var reportTst = objectCreator.CreateComplianceReport("TST", AccComplianceReport.Status.ReportFinalised);
				Factory.Save();
				Assert("SAFT report does not support XML generation for ReportType 'TST'.", !reportTst.SupportsSAFTXmlGeneration);
				using (var form = new AccComplianceReportForm(reportTst))
				{
					form.Show();

					var menuItem = form.Menu.MenuItems.FindByText("Generate Monthly Transaction XML", true);
					AssertNull("Generate SAFT Xml menuItem is not visible when does not support XML generation", menuItem);

					var generateXmlButton = (ZButton)form.Controls.Find("generateXmlButton", true)[0];
					Assert("Generate Xml button is hidden", !generateXmlButton.Visible);
					Assert("Generate Xml button is disabled", !generateXmlButton.Enabled);
				}

				objectCreator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, "IVA", ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);
				var report = objectCreator.CreateComplianceReport(AccComplianceReport.ReportTypes.SAFT, AccComplianceReport.Status.ReportFinalised);
				Factory.Save();
				Assert("SAFT report must have ReportType 'SAF' to support XML generation.", report.SupportsSAFTXmlGeneration);
				using (var form = new AccComplianceReportForm(report))
				{
					form.Show();

					var menuItem = form.Menu.MenuItems.FindByText("Generate Monthly Transaction XML", true);
					AssertNotNull("Generate SAFT Xml menuItem is visible", menuItem);
					Assert("Generate SAFT Xml menuItem is enabled", menuItem.Enabled);

					var generateXmlButton = (ZButton)form.Controls.Find("generateXmlButton", true)[0];
					Assert("Generate SAFT Xml button is visible", generateXmlButton.Visible);
					Assert("Generate SAFT Xml button is enabled", generateXmlButton.Enabled);

					AssertEquals("Generate SAFT Xml button Tooltip should have Generate Monthly Transaction XML", "Generate Monthly Transaction XML", generateXmlButton.ToolTipCaption);
					AssertEquals("Generate SAFT Xml button text should have Generate XML", "Generate XML", generateXmlButton.Text);
				}
			}
		}

		public void TestComplianceReportMTDViewAndSubmitIsNotVisible()
		{
			using (var testForm = (AccComplianceReportForm)GetFormToBash())
			{
				testForm.Show();
				var formBizo = (AccComplianceReport)testForm.BusinessEntity;
				AssertNotEquals(Core.Constants.CountryCodes.UnitedKingdom, GlbCompany.CurrentCompany.Country);

				var menuItem = testForm.Menu.MenuItems.FindByText("View and Submit", true);
				AssertNull("MTD View and Submit menuItem is not visible for non UK countries", menuItem);

				var mtdViewAndSubmitButton = (ZButton)testForm.Controls.Find("viewAndSubmitButton", true)[0];
				Assert("MTD View and Submit button is not visible", !mtdViewAndSubmitButton.Visible);
				Assert("MTD View and Submit button is not enabled", !mtdViewAndSubmitButton.Enabled);
			}
		}

		public void TestComplianceReportMTDViewAndSubmitIsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateTestPeriods(ZDateTime.Today);

				var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				var reportConfig = complianceConfig.Cast<ComplianceReportConfiguration>().FirstOrDefault(x => x.ReportCode == "MTD");
				AssertNotNull(reportConfig);
				AssertEquals(ReportPeriodicityCodes.DateRange, reportConfig.ReportPeriodicity);
				AssertEquals(Env.CurrentCompany.Country.Code, reportConfig.Country);
				AssertEquals("VAT", reportConfig.TaxRegistrationType);
				AssertEquals(ReportBaseTablePrefixListCodes.TransactionLine, reportConfig.ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.TaxReporting, reportConfig.ReportLineGrouping);

				var report = objectCreator.CreateComplianceReport("MTD", AccComplianceReport.Status.ReportFinalised);
				Factory.Save();
				Assert(report.SupportsMTD);

				using (var form = new AccComplianceReportForm(report))
				{
					form.Show();

					var menuItem = form.Menu.MenuItems.FindByText("View and Submit", true);
					AssertNotNull("MTD View and Submit menuItem is visible", menuItem);
					Assert("MTD View and Submit menuItem is enabled", menuItem.Enabled);

					var mtdViewAndSubmitButton = (ZButton)form.Controls.Find("viewAndSubmitButton", true)[0];
					Assert("MTD View and Submit button is visible", mtdViewAndSubmitButton.Visible);
					Assert("MTD View and Submit button is enabled", mtdViewAndSubmitButton.Enabled);
				}
			}
		}

		public void TestComplianceReportMTDViewAndSubmitButtonPosition()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateTestPeriods(ZDateTime.Today);

				var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				var reportConfig = complianceConfig.Cast<ComplianceReportConfiguration>().FirstOrDefault(x => x.ReportCode == "MTD");
				AssertNotNull(reportConfig);
				AssertEquals(ReportPeriodicityCodes.DateRange, reportConfig.ReportPeriodicity);
				AssertEquals(Env.CurrentCompany.Country.Code, reportConfig.Country);
				AssertEquals("VAT", reportConfig.TaxRegistrationType);
				AssertEquals(ReportBaseTablePrefixListCodes.TransactionLine, reportConfig.ReportBaseTablePrefix);
				AssertEquals(ReportLineGroupingListCodes.TaxReporting, reportConfig.ReportLineGrouping);

				var report = objectCreator.CreateComplianceReport("MTD", AccComplianceReport.Status.ReportFinalised);
				Factory.Save();
				Assert(report.SupportsMTD);

				var controller = ZControllerFactory.Create(ControllerIDs.AccComplianceReport);
				using (var form = (AccComplianceReportForm)controller.ShowEditForm(report))
				{
					var viewAndSubmitButton = (ZButton)form.Controls.Find("viewAndSubmitButton", true)[0];
					var previousNextControl = form.Controls.Find("ZPreviousNextControl", true);
					AssertNotNull("Pre-Condition: PreviousNextControl should exists", previousNextControl);

					var previousNextControlEndPoint = previousNextControl[0].PointToScreen(Point.Empty).X + previousNextControl[0].Size.Width;
					var viewAndSubmitButtonStartPoint = viewAndSubmitButton.PointToScreen(Point.Empty).X;
					Assert("PreviousNextControl and viewAndSubmitButton should not overlap each other, viewAndSubmitButton start point should be equal or greater than PreviousNextControl end point.", viewAndSubmitButtonStartPoint >= previousNextControlEndPoint);
				}
			}
		}

		public void TestComplianceReportGridColumnsVisibleForPTRS_AllPayments()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateTestPeriods(ZDateTime.Today);

				objectCreator.EnsureComplianceReportConfigInRegistry(ComplianceReportTypes.PaymentTimesAllPaymentsReportType, "ABN", ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionHeader, ReportLineGroupingListCodes.PaymentTimesAll);
				var report = objectCreator.CreateComplianceReport(ComplianceReportTypes.PaymentTimesAllPaymentsReportType, AccComplianceReport.Status.ReportCreated);
				Factory.Save();

				foreach (var status in new[] { AccComplianceReport.Status.ReportCreated, AccComplianceReport.Status.ReportGenerated })
				{
					var supportedStatus = status == AccComplianceReport.Status.ReportGenerated;
					report.ACR_Status = status;
					AssertPtrsSupport(report, allPayments: true, supportedStatus: supportedStatus);

					var removedColumnNames = new string[] {
						AccComplianceReportLine.Schema.RepCountryRegNo,
						AccComplianceReportLine.Schema.AH_ComplianceSubType,
						AccComplianceReportLine.Schema.AT_Code,
						AccComplianceReportLine.Schema.AT_Type,
						AccComplianceReportLine.Schema.TaxMessage,
						AccComplianceReportLine.Schema.ReportSubCode,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeInputAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeOutputAmount,
						AccComplianceReportLine.Schema.AG_AccountNum,
						AccComplianceReportLine.Schema.AG_Description,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
						nameof(AccComplianceReportLine.PreCalculatedCount),
						nameof(AccComplianceReportLine.DaysRange),
					};

					var requiredColumnNames = new string[] {
						AccComplianceReportLine.Schema.ACL_ReportSequence,
						AccComplianceReportLine.Schema.GB_Code,
						AccComplianceReportLine.Schema.GE_Code,
						AccComplianceReportLine.Schema.OH_Code,
						AccComplianceReportLine.Schema.OH_FullName,
						AccComplianceReportLine.Schema.OK_CustomsRegNo,
						AccComplianceReportLine.Schema.AH_Ledger,
						AccComplianceReportLine.Schema.AH_TransactionType,
						AccComplianceReportLine.Schema.AH_TransactionNum,
						AccComplianceReportLine.Schema.AH_TransactionReference,
						AccComplianceReportLine.Schema.ComplianceSequence,
						AccComplianceReportLine.Schema.PostDate,
						AccComplianceReportLine.Schema.InvoiceDate,
						AccComplianceReportLineBase.Schema.TotalExTaxAmount,
						AccComplianceReportLineBase.Schema.TotalTaxAmount,
						AccComplianceReportLineBase.Schema.PreCalculatedAmount,
					};

					AssertRemovedAndRequiredColumnsOfCurrentPeriodLinesGrid(report, "reportCurrentPeriodLinesGrid", removedColumnNames, requiredColumnNames);

					var removedTotalColumnNames = new string[] {
						AccComplianceReportLineBase.Schema.LineNum,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
					};

					var requiredTotalColumnNames = new string[] {
						AccComplianceReportLineBase.Schema.Comment,
						AccComplianceReportLineBase.Schema.TotalExTaxAmount,
						AccComplianceReportLineBase.Schema.TotalTaxAmount,
						AccComplianceReportLineBase.Schema.PreCalculatedAmount,
					};

					AssertRemovedAndRequiredColumnsOfCurrentPeriodLinesGrid(report, "reportTotalsCurrentPeriodGrid", removedTotalColumnNames, requiredTotalColumnNames);
				}
			}
		}

		public void TestComplianceReportGridColumnsVisibleForPTRS_SmallBusiness()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateTestPeriods(ZDateTime.Today);

				objectCreator.EnsureComplianceReportConfigInRegistry(ComplianceReportTypes.PaymentTimesSmallBusinessReportType, "ABN", ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionHeader, ReportLineGroupingListCodes.PaymentTimesSmallBusinessReportable);
				var report = objectCreator.CreateComplianceReport(ComplianceReportTypes.PaymentTimesSmallBusinessReportType, AccComplianceReport.Status.ReportCreated);
				Factory.Save();

				foreach (var status in new[] { AccComplianceReport.Status.ReportCreated, AccComplianceReport.Status.ReportGenerated })
				{
					var abnImport = status == AccComplianceReport.Status.ReportCreated;
					var supportedStatus = status == AccComplianceReport.Status.ReportGenerated;
					report.ACR_Status = status;
					AssertPtrsSupport(report, abnImport: abnImport, smallBusiness: true, supportedStatus: supportedStatus);

					var removedColumnNames = new string[] {
						AccComplianceReportLine.Schema.RepCountryRegNo,
						AccComplianceReportLine.Schema.AH_ComplianceSubType,
						AccComplianceReportLine.Schema.AT_Code,
						AccComplianceReportLine.Schema.AT_Type,
						AccComplianceReportLine.Schema.TaxMessage,
						AccComplianceReportLine.Schema.ReportSubCode,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeInputAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeOutputAmount,
						AccComplianceReportLine.Schema.AG_AccountNum,
						AccComplianceReportLine.Schema.AG_Description,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
					};

					var requiredColumnNames = new string[] {
						AccComplianceReportLine.Schema.ACL_ReportSequence,
						AccComplianceReportLine.Schema.GB_Code,
						AccComplianceReportLine.Schema.GE_Code,
						AccComplianceReportLine.Schema.OH_Code,
						AccComplianceReportLine.Schema.OH_FullName,
						AccComplianceReportLine.Schema.OK_CustomsRegNo,
						AccComplianceReportLine.Schema.AH_Ledger,
						AccComplianceReportLine.Schema.AH_TransactionType,
						AccComplianceReportLine.Schema.AH_TransactionNum,
						AccComplianceReportLine.Schema.AH_TransactionReference,
						AccComplianceReportLine.Schema.ComplianceSequence,
						AccComplianceReportLine.Schema.PostDate,
						AccComplianceReportLine.Schema.InvoiceDate,
						AccComplianceReportLineBase.Schema.TotalExTaxAmount,
						AccComplianceReportLineBase.Schema.TotalTaxAmount,
						AccComplianceReportLineBase.Schema.PreCalculatedAmount,
						nameof(AccComplianceReportLine.PreCalculatedCount),
						nameof(AccComplianceReportLine.DaysRange),
					};

					AssertRemovedAndRequiredColumnsOfCurrentPeriodLinesGrid(report, "reportCurrentPeriodLinesGrid", removedColumnNames, requiredColumnNames);

					var removedTotalColumnNames = new string[] {
						AccComplianceReportLineBase.Schema.LineNum,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
					};

					var requiredTotalColumnNames = new string[] {
						AccComplianceReportLineBase.Schema.Comment,
						AccComplianceReportLineBase.Schema.TotalExTaxAmount,
						AccComplianceReportLineBase.Schema.TotalTaxAmount,
						AccComplianceReportLineBase.Schema.PreCalculatedAmount,
					};

					AssertRemovedAndRequiredColumnsOfCurrentPeriodLinesGrid(report, "reportTotalsCurrentPeriodGrid", removedTotalColumnNames, requiredTotalColumnNames);
				}
			}
		}

		public void TestComplianceReportGridColumnsVisibleForPTRS2024_AllPayments()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateTestPeriods(ZDateTime.Today);

				objectCreator.EnsureComplianceReportConfigInRegistry(ComplianceReportTypes.PaymentTimesAllPayments2024ReportType, "ABN", ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionHeader, ReportLineGroupingListCodes.PaymentTimesAll);
				var report = objectCreator.CreateComplianceReport(ComplianceReportTypes.PaymentTimesAllPayments2024ReportType, AccComplianceReport.Status.ReportCreated);
				Factory.Save();

				foreach (var status in new[] { AccComplianceReport.Status.ReportCreated, AccComplianceReport.Status.ReportGenerated })
				{
					var supportedStatus = status == AccComplianceReport.Status.ReportGenerated;
					report.ACR_Status = status;
					AssertPtrsSupport(report, allPayments: true, ptrs2024: true, supportedStatus: supportedStatus);

					var removedColumnNames = new string[] {
						AccComplianceReportLine.Schema.RepCountryRegNo,
						AccComplianceReportLine.Schema.AH_ComplianceSubType,
						AccComplianceReportLine.Schema.AT_Code,
						AccComplianceReportLine.Schema.AT_Type,
						AccComplianceReportLine.Schema.TaxMessage,
						AccComplianceReportLine.Schema.ReportSubCode,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeInputAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeOutputAmount,
						AccComplianceReportLine.Schema.AG_AccountNum,
						AccComplianceReportLine.Schema.AG_Description,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
						nameof(AccComplianceReportLine.PreCalculatedCount),
						nameof(AccComplianceReportLine.PreCalculatedCount2),
						nameof(AccComplianceReportLine.PreCalculatedCount3),
						nameof(AccComplianceReportLine.DaysRange),
						nameof(AccComplianceReportLine.DaysRange30_60),
					};

					var requiredColumnNames = new string[] {
						AccComplianceReportLine.Schema.ACL_ReportSequence,
						AccComplianceReportLine.Schema.GB_Code,
						AccComplianceReportLine.Schema.GE_Code,
						AccComplianceReportLine.Schema.OH_Code,
						AccComplianceReportLine.Schema.OH_FullName,
						AccComplianceReportLine.Schema.OK_CustomsRegNo,
						AccComplianceReportLine.Schema.AH_Ledger,
						AccComplianceReportLine.Schema.AH_TransactionType,
						AccComplianceReportLine.Schema.AH_TransactionNum,
						AccComplianceReportLine.Schema.AH_TransactionReference,
						AccComplianceReportLine.Schema.ComplianceSequence,
						AccComplianceReportLine.Schema.PostDate,
						AccComplianceReportLine.Schema.InvoiceDate,
						AccComplianceReportLineBase.Schema.TotalExTaxAmount,
						AccComplianceReportLineBase.Schema.TotalTaxAmount,
						AccComplianceReportLineBase.Schema.PreCalculatedAmount,
						nameof(AccComplianceReportLine.IsSmallBusiness),
						nameof(AccComplianceReportLine.IsFullyPaid),
					};

					AssertRemovedAndRequiredColumnsOfCurrentPeriodLinesGrid(report, "reportCurrentPeriodLinesGrid", removedColumnNames, requiredColumnNames);

					var removedTotalColumnNames = new string[] {
						AccComplianceReportLineBase.Schema.LineNum,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
					};

					var requiredTotalColumnNames = new string[] {
						AccComplianceReportLineBase.Schema.Comment,
						AccComplianceReportLineBase.Schema.TotalExTaxAmount,
						AccComplianceReportLineBase.Schema.TotalTaxAmount,
						AccComplianceReportLineBase.Schema.PreCalculatedAmount,
					};

					AssertRemovedAndRequiredColumnsOfCurrentPeriodLinesGrid(report, "reportTotalsCurrentPeriodGrid", removedTotalColumnNames, requiredTotalColumnNames);
				}
			}
		}

		public void TestComplianceReportGridColumnsVisibleForPTRS2024_SmallBusiness()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateTestPeriods(ZDateTime.Today);

				objectCreator.EnsureComplianceReportConfigInRegistry(ComplianceReportTypes.PaymentTimesSmallBusiness2024ReportType, "ABN", ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionHeader, ReportLineGroupingListCodes.PaymentTimesSmallBusinessReportable);
				var report = objectCreator.CreateComplianceReport(ComplianceReportTypes.PaymentTimesSmallBusiness2024ReportType, AccComplianceReport.Status.ReportCreated);
				Factory.Save();

				foreach (var status in new[] { AccComplianceReport.Status.ReportCreated, AccComplianceReport.Status.ReportGenerated })
				{
					var abnImport = status == AccComplianceReport.Status.ReportCreated;
					var supportedStatus = status == AccComplianceReport.Status.ReportGenerated;
					report.ACR_Status = status;
					AssertPtrsSupport(report, abnImport: abnImport, smallBusiness: true, ptrs2024: true, supportedStatus: supportedStatus);

					var removedColumnNames = new string[] {
						AccComplianceReportLine.Schema.RepCountryRegNo,
						AccComplianceReportLine.Schema.AH_ComplianceSubType,
						AccComplianceReportLine.Schema.AT_Code,
						AccComplianceReportLine.Schema.AT_Type,
						AccComplianceReportLine.Schema.TaxMessage,
						AccComplianceReportLine.Schema.ReportSubCode,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeInputAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeOutputAmount,
						AccComplianceReportLine.Schema.AG_AccountNum,
						AccComplianceReportLine.Schema.AG_Description,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
						nameof(AccComplianceReportLine.DaysRange),
						nameof(AccComplianceReportLine.IsSmallBusiness),
						nameof(AccComplianceReportLine.IsFullyPaid),
					};

					var requiredColumnNames = new string[] {
						AccComplianceReportLine.Schema.ACL_ReportSequence,
						AccComplianceReportLine.Schema.GB_Code,
						AccComplianceReportLine.Schema.GE_Code,
						AccComplianceReportLine.Schema.OH_Code,
						AccComplianceReportLine.Schema.OH_FullName,
						AccComplianceReportLine.Schema.OK_CustomsRegNo,
						AccComplianceReportLine.Schema.AH_Ledger,
						AccComplianceReportLine.Schema.AH_TransactionType,
						AccComplianceReportLine.Schema.AH_TransactionNum,
						AccComplianceReportLine.Schema.AH_TransactionReference,
						AccComplianceReportLine.Schema.ComplianceSequence,
						AccComplianceReportLine.Schema.PostDate,
						AccComplianceReportLine.Schema.InvoiceDate,
						AccComplianceReportLineBase.Schema.TotalExTaxAmount,
						AccComplianceReportLineBase.Schema.TotalTaxAmount,
						AccComplianceReportLineBase.Schema.PreCalculatedAmount,
						nameof(AccComplianceReportLine.PreCalculatedCount),
						nameof(AccComplianceReportLine.PreCalculatedCount2),
						nameof(AccComplianceReportLine.PreCalculatedCount3),
						nameof(AccComplianceReportLine.DaysRange30_60),
					};

					AssertRemovedAndRequiredColumnsOfCurrentPeriodLinesGrid(report, "reportCurrentPeriodLinesGrid", removedColumnNames, requiredColumnNames);

					var removedTotalColumnNames = new string[] {
						AccComplianceReportLineBase.Schema.LineNum,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
					};

					var requiredTotalColumnNames = new string[] {
						AccComplianceReportLineBase.Schema.Comment,
						AccComplianceReportLineBase.Schema.TotalExTaxAmount,
						AccComplianceReportLineBase.Schema.TotalTaxAmount,
						AccComplianceReportLineBase.Schema.PreCalculatedAmount,
					};

					AssertRemovedAndRequiredColumnsOfCurrentPeriodLinesGrid(report, "reportTotalsCurrentPeriodGrid", removedTotalColumnNames, requiredTotalColumnNames);
				}
			}
		}

		public void TestTransactionNumColumnIsRenamed()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			objectCreator.CreateConfigurationForComplianceReport(report, baseTablePrefix: ReportBaseTablePrefixListCodes.AllTransactions, reportLineGrouping: ReportLineGroupingListCodes.DayBookWithoutGrouping, reportLineOrdering: ReportLineOrderingListCodes.ComplianceSubType);
			Factory.Save();
			using (var testForm = new AccComplianceReportForm(report))
			{
				testForm.Show();
				var reportCurrentPeriodLinesGrid = testForm.GetControl<ZGrid>("reportCurrentPeriodLinesGrid");
				var transactionNumColumn = reportCurrentPeriodLinesGrid.ColumnStyles.Cast<ZGridColumnInfo>().First(x => x.ColumnName == "AH_TransactionNum");
				AssertEquals(transactionNumColumn.CaptionResourceString.Caption, "Transaction #");
			}
		}

		public void TestComplianceReportGridColumnsVisibleForPortugalSAFTReports_SAFT()
		{
			TestComplianceReportGridColumnsVisibleForPortugalSAFTReports(AccComplianceReport.ReportTypes.SAFT, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);
		}

		public void TestComplianceReportGridColumnsVisibleForPortugalSAFTReports_SAFTOnlyTransactions()
		{
			TestComplianceReportGridColumnsVisibleForPortugalSAFTReports(AccComplianceReport.ReportTypes.SAFTOnlyTransactions, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.NoGrouping);
		}

		public void TestComplianceReportGridColumnsVisibleForPortugalSAFTReports(string reportType, string tablePrefix, string lineGrouping)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var creator = new TestObjectCreator(Factory);
				var reportConfiguration = creator.EnsureComplianceReportConfigInRegistry(reportType, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.AccountingPeriod, tablePrefix, lineGrouping);

				var reportConfigurations = new ComplianceReportConfigurationCollection();
				reportConfigurations.Add(reportConfiguration);
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);

				var taxRate = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "IVAREV13"));
				var taxMessage = Factory.NewWithValidTestData<AccInvMsg>();
				taxMessage.A9_TaxGroupCode = "M30";
				taxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				taxMessage.A9_LocalMsg = "Local Tax Message";

				var invoice = creator.CreateInvoice(typeof(ARInvoice), "I0001", creator.EUR, 1m, creator.ABIGAS);
				var invoiceLine = creator.CreateInvoiceLine(invoice, creator.EUR, 1M, 100M, 10M, 0M, creator.CC1.PK);
				invoiceLine.AL_AT = taxRate.PK;
				invoiceLine.AL_A9_VATClass = taxMessage.PK;

				var report = creator.CreateComplianceReport(reportType, AccComplianceReport.Status.ReportDataQueued, ReportPeriodicityCodes.AccountingPeriod);
				Factory.Save();
				creator.CreateComplianceReportQueueEntry(report, invoice);
				creator.CreateComplianceReportQueueEntry(report, invoiceLine);
				Factory.Save();
				report.GenerateFromQueue();
				Factory.Save();
				var removedColumnNames = Array.Empty<string>();
				var requiredColumnNames = Array.Empty<string>();

				if (reportType == AccComplianceReport.ReportTypes.SAFTOnlyTransactions)
				{
					removedColumnNames = new string[] {
						AccComplianceReportLineBase.Schema.PK,
						AccComplianceReportLine.Schema.OrgCountryCode,
						AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
						AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
						AccComplianceReportLine.Schema.AH_PK,
						AccComplianceReportLine.Schema.AH_InvoiceAmount,
						AccComplianceReportLine.Schema.AH_GSTAmount,
						AccComplianceReportLine.Schema.SPVTaxAmount,
						AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
						AccComplianceReportLine.Schema.AL_A9_VATClass,
						AccComplianceReportLine.Schema.GLAccountPK,
						AccComplianceReportLine.Schema.AL_TaxRate,
						AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
						AccComplianceReportLine.Schema.AG_AccountNum,
						AccComplianceReportLine.Schema.AG_Description,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
					};

					requiredColumnNames = new string[] {
						AccComplianceReportLine.Schema.ACL_ReportSequence,
						AccComplianceReportLine.Schema.OH_Code,
						AccComplianceReportLine.Schema.OH_FullName,
						AccComplianceReportLine.Schema.OK_CustomsRegNo,
						AccComplianceReportLine.Schema.RepCountryRegNo,
						AccComplianceReportLine.Schema.AH_Ledger,
						AccComplianceReportLine.Schema.AH_TransactionType,
						AccComplianceReportLine.Schema.PostDate,
						AccComplianceReportLine.Schema.InvoiceDate,
						AccComplianceReportLine.Schema.AH_TransactionNum,
						AccComplianceReportLine.Schema.AH_TransactionReference,
						AccComplianceReportLine.Schema.AH_ComplianceSubType,
						AccComplianceReportLine.Schema.ReportSubCode,
						AccComplianceReportLine.Schema.AT_Type,
						AccComplianceReportLine.Schema.AT_Code,
						AccComplianceReportLine.Schema.TaxMessage,
						AccComplianceReportLine.Schema.ComplianceSequence,
						AccComplianceReportLine.Schema.TaxGroupDescription,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.TotalExTaxAmount,
						AccComplianceReportLineBase.Schema.TotalTaxAmount,
						AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
					};
				}
				else
				{
					removedColumnNames = new string[] {
						AccComplianceReportLineBase.Schema.PK,
						AccComplianceReportLine.Schema.OrgCountryCode,
						AccComplianceReportLine.Schema.GC_RN_NKCountryCode,
						AccComplianceReportLine.Schema.GC_RX_NKLocalCurrency,
						AccComplianceReportLine.Schema.AH_PK,
						AccComplianceReportLine.Schema.AH_InvoiceAmount,
						AccComplianceReportLine.Schema.AH_GSTAmount,
						AccComplianceReportLine.Schema.SPVTaxAmount,
						AccComplianceReportLine.Schema.AT_ExtraTaxRateType,
						AccComplianceReportLine.Schema.AL_A9_VATClass,
						AccComplianceReportLine.Schema.GLAccountPK,
						AccComplianceReportLine.Schema.AL_TaxRate,
						AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
					};

					requiredColumnNames = new string[] {
						AccComplianceReportLine.Schema.ACL_ReportSequence,
						AccComplianceReportLine.Schema.OH_Code,
						AccComplianceReportLine.Schema.OH_FullName,
						AccComplianceReportLine.Schema.OK_CustomsRegNo,
						AccComplianceReportLine.Schema.RepCountryRegNo,
						AccComplianceReportLine.Schema.AH_Ledger,
						AccComplianceReportLine.Schema.AH_TransactionType,
						AccComplianceReportLine.Schema.PostDate,
						AccComplianceReportLine.Schema.InvoiceDate,
						AccComplianceReportLine.Schema.AH_TransactionNum,
						AccComplianceReportLine.Schema.AH_TransactionReference,
						AccComplianceReportLine.Schema.AH_ComplianceSubType,
						AccComplianceReportLine.Schema.ReportSubCode,
						AccComplianceReportLine.Schema.AG_AccountNum,
						AccComplianceReportLine.Schema.AG_Description,
						AccComplianceReportLine.Schema.AT_Type,
						AccComplianceReportLine.Schema.AT_Code,
						AccComplianceReportLine.Schema.TaxMessage,
						AccComplianceReportLine.Schema.ComplianceSequence,
						AccComplianceReportLine.Schema.TaxGroupDescription,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.TotalExTaxAmount,
						AccComplianceReportLineBase.Schema.TotalTaxAmount,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
					};
				}

				AssertRemovedAndRequiredColumnsOfCurrentPeriodLinesGrid(report, "reportCurrentPeriodLinesGrid", removedColumnNames, requiredColumnNames);
			}
		}

		void AssertRemovedAndRequiredColumnsOfCurrentPeriodLinesGrid(AccComplianceReport report, string gridControlName, string[] removedColumnNames, string[] requiredColumnNames)
		{
			using (var testForm = new AccComplianceReportForm(report))
			{
				testForm.Show();
				var formBizo = (AccComplianceReport)testForm.BusinessEntity;
				var reportCurrentPeriodLinesGrid = testForm.GetControl<ZGrid>(gridControlName);
				AssertNotNull(gridControlName, reportCurrentPeriodLinesGrid);

				CombineAssertions(() =>
				{
					foreach (var columnName in removedColumnNames)
					{
						Assert($"Column: {columnName} should be removed in " + gridControlName, !reportCurrentPeriodLinesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(x => x.ColumnName == columnName));
					}
				});

				CombineAssertions(() =>
				{
					foreach (var columnName in requiredColumnNames)
					{
						Assert($"Column: {columnName} should be presented in " + gridControlName, reportCurrentPeriodLinesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(x => x.ColumnName == columnName && x.IsVisible));
					}
				});
			}
		}

		public void TestPTRSComplianceReportImportABNsViewAndSubmit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateTestPeriods(ZDateTime.Today);

				var report = objectCreator.CreateComplianceReport(ComplianceReportTypes.PaymentTimesSmallBusinessReportType, AccComplianceReport.Status.ReportCreated);
				Factory.Save();
				AssertPtrsSupport(report, abnImport: true);

				CombineAssertions("if user did not generate report, user can import ABNs", () =>
				{
					var importAbnStatuses = new[] {
						AccComplianceReport.Status.ReportCreated,
						AccComplianceReport.Status.ReportDataQueued,
						AccComplianceReport.Status.ReportPendingQueueing,
					};
					foreach (var status in importAbnStatuses)
					{
						report.ACR_Status = status;
						AssertPtrsSupport(report, abnImport: true);

						using (var form = new AccComplianceReportForm(report))
						{
							form.Show();

							var menuItem = form.Menu.MenuItems.FindByText("Import ABNs", true);
							AssertNotNull("Import ABNs menuItem is visible", menuItem);
							AssertEquals("Import ABNs menuItem is enabled", true, menuItem.Enabled);
							menuItem = form.Menu.MenuItems.FindByText("View and Submit", true);
							AssertNull("View and Submit is not visible", menuItem);

							var viewAndSubmitButton = (ZButton)form.Controls.Find("viewAndSubmitButton", true)[0];
							AssertEquals("View and Submit button is visible", true, viewAndSubmitButton.Visible);
							AssertEquals("View and Submit button is enabled", true, viewAndSubmitButton.Enabled);
							AssertEquals("It does Import ABNs", "Import ABNs", viewAndSubmitButton.Text);
							AssertEquals("Import Reportable Small Business ABNs", viewAndSubmitButton.ToolTipCaption.ToString());

							viewAndSubmitButton.PerformClick();

							AssertType<ImportABNsForm>(ZFormModaliser.LastFormShownDialogForTest);
							ZFormModaliser.LastFormShownDialogForTest.Close();
						}
					}
				});

				CombineAssertions("if user has generated report, user can not import ABNs but can View and Submit", () =>
				{
					var viewAndSubmitStatuses = new[] {
						AccComplianceReport.Status.ReportFinalised,
						AccComplianceReport.Status.ReportGenerated,
					};
					foreach (var status in viewAndSubmitStatuses)
					{
						report.ACR_Status = status;
						AssertPtrsSupport(report, smallBusiness: true, supportedStatus: true);

						using (var form = new AccComplianceReportForm(report))
						{
							form.Show();
							var menuItem = form.Menu.MenuItems.FindByText("Import ABNs", true);
							AssertNull("Import ABNs menuItem is not visible", menuItem);

							var viewAndSubmitButton = (ZButton)form.Controls.Find("viewAndSubmitButton", true)[0];
							AssertEquals("View and Submit button is visible", true, viewAndSubmitButton.Visible);
							AssertEquals("View and Submit button is enabled", true, viewAndSubmitButton.Enabled);
							AssertEquals("It does View and Submit", "View and Submit", viewAndSubmitButton.Text);
							AssertEquals("View and Submit", viewAndSubmitButton.ToolTipCaption.ToString());

							viewAndSubmitButton.PerformClick();

							AssertType<PTRSForm>(ZFormModaliser.LastFormShownDialogForTest);
							ZFormModaliser.LastFormShownDialogForTest.Close();
						}
					}
				});
			}
		}

		public void TestCompliacneReportNotSupportingImportABNs()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateTestPeriods(ZDateTime.Today);

				var report = objectCreator.CreateComplianceReport("TPR", AccComplianceReport.Status.ReportCreated);
				Factory.Save();
				AssertEquals(false, report.SupportsImportABNs);

				using (var form = new AccComplianceReportForm(report))
				{
					form.Show();
					var menuItem = form.Menu.MenuItems.FindByText("Import ABNs", true);
					AssertNull("Import ABNs menuItem is not visible", menuItem);

					var importABNsButton = (ZButton)form.Controls.Find("viewAndSubmitButton", true)[0];
					AssertEquals("Import ABNs button is not visible", false, importABNsButton.Visible);
					AssertNotEquals("Import ABNs", importABNsButton.Text);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateTestPeriods(ZDateTime.Today);

				var report = objectCreator.CreateComplianceReport("MTD", AccComplianceReport.Status.ReportCreated);
				Factory.Save();
				AssertEquals(false, report.SupportsImportABNs);

				using (var form = new AccComplianceReportForm(report))
				{
					form.Show();
					var menuItem = form.Menu.MenuItems.FindByText("Import ABNs", true);
					AssertNull("Import ABNs menuItem is not visible", menuItem);

					var importABNsButton = (ZButton)form.Controls.Find("viewAndSubmitButton", true)[0];
					AssertEquals("Import ABNs button is not visible", false, importABNsButton.Visible);
					AssertNotEquals("Import ABNs", importABNsButton.Text);
				}
			}
		}

		public void TestPTRSAllPaymentsComplianceReportViewAndSubmit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var objectCreator = new TestObjectCreator(Factory);
				objectCreator.CreateTestPeriods(ZDateTime.Today);

				var report = objectCreator.CreateComplianceReport(ComplianceReportTypes.PaymentTimesAllPaymentsReportType, AccComplianceReport.Status.ReportCreated);
				Factory.Save();
				AssertPtrsSupport(report);

				CombineAssertions("if user did not generate report, user cannot View and Submit", () =>
				{
					var notSupportedStatuses = new[] {
						AccComplianceReport.Status.ReportCreated,
						AccComplianceReport.Status.ReportDataQueued,
						AccComplianceReport.Status.ReportPendingQueueing,
					};
					foreach (var status in notSupportedStatuses)
					{
						report.ACR_Status = status;
						AssertPtrsSupport(report);

						using (var form = new AccComplianceReportForm(report))
						{
							form.Show();

							var menuItem = form.Menu.MenuItems.FindByText("Import ABNs", true);
							AssertNull("Import ABNs menuItem is visible", menuItem);

							var viewAndSubmitButton = (ZButton)form.Controls.Find("viewAndSubmitButton", true)[0];
							AssertEquals("View and Submit button is not visible", false, viewAndSubmitButton.Visible);
						}
					}
				});

				CombineAssertions("if user has generated report, user can not import ABNs but can View and Submit", () =>
				{
					var viewAndSubmitStatuses = new[] {
						AccComplianceReport.Status.ReportFinalised,
						AccComplianceReport.Status.ReportGenerated,
					};
					foreach (var status in viewAndSubmitStatuses)
					{
						report.ACR_Status = status;
						AssertPtrsSupport(report, allPayments: true, supportedStatus: true);

						using (var form = new AccComplianceReportForm(report))
						{
							form.Show();
							var menuItem = form.Menu.MenuItems.FindByText("Import ABNs", true);
							AssertNull("Import ABNs menuItem is not visible", menuItem);

							var viewAndSubmitButton = (ZButton)form.Controls.Find("viewAndSubmitButton", true)[0];
							AssertEquals("View and Submit button is visible", true, viewAndSubmitButton.Visible);
							AssertEquals("View and Submit button is enabled", true, viewAndSubmitButton.Enabled);
							AssertEquals("It does View and Submit", "View and Submit", viewAndSubmitButton.Text);
							AssertEquals("View and Submit", viewAndSubmitButton.ToolTipCaption.ToString());

							viewAndSubmitButton.PerformClick();

							AssertType<PTRSAllPaymentsForm>(ZFormModaliser.LastFormShownDialogForTest);
							ZFormModaliser.LastFormShownDialogForTest.Close();
						}
					}
				});
			}
		}

		void AssertPtrsSupport(AccComplianceReport report, bool abnImport = false, bool smallBusiness = false, bool allPayments = false, bool ptrs2024 = false, bool supportedStatus = false)
		{
			AssertEquals("SupportsImportABNs", abnImport, report.SupportsImportABNs);
			AssertEquals("HasPreCalculatedAmount", (allPayments || smallBusiness) && supportedStatus, report.HasPreCalculatedAmount);

			AssertEquals("SupportsPTRSSmallBusiness", smallBusiness && !ptrs2024 && supportedStatus, report.SupportsPTRSSmallBusiness);
			AssertEquals("SupportsPTRSAllPayments", allPayments && !ptrs2024 && supportedStatus, report.SupportsPTRSAllPayments);
			AssertEquals("SupportsPTRS2024SmallBusiness", smallBusiness && ptrs2024 && supportedStatus, report.SupportsPTRS2024SmallBusiness);
			AssertEquals("SupportsPTRS2024AllPayments", allPayments && ptrs2024 && supportedStatus, report.SupportsPTRS2024AllPayments);

			AssertEquals("IsPTRS2024SmallBusiness", smallBusiness && ptrs2024, report.IsPTRS2024SmallBusiness);
			AssertEquals("IsPTRS2024AllPayments", allPayments && ptrs2024, report.IsPTRS2024AllPayments);
		}

		public void TestComplianceReportGenerateEsterometroXmlIsNotVisible()
		{
			using (var testForm = (AccComplianceReportForm)GetFormToBash())
			{
				testForm.Show();
				var formBizo = (AccComplianceReport)testForm.BusinessEntity;
				AssertNotEquals(Core.Constants.CountryCodes.Italy, GlbCompany.CurrentCompany.Country);

				var menuItem = testForm.Menu.MenuItems.FindByText("Generate Esterometro XML", true);
				AssertNull("Generate Esterometro Xml menuItem is not visible for non Italy countries", menuItem);

				var generateXmlButton = (ZButton)testForm.Controls.Find("generateXmlButton", true)[0];
				Assert("Generate Esterometro Xml button is not visible", !generateXmlButton.Visible);
				Assert("Generate Esterometro Xml button is not enabled", !generateXmlButton.Enabled);
			}
		}

		public void TestComplianceReportGenerateEsterometroXmlIsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			using (var form = new AccComplianceReportForm(CreateValidEsterometroReport()))
			{
				form.Show();

				var menuItem = form.Menu.MenuItems.FindByText("Generate Esterometro XML", true);
				AssertNotNull("Generate Esterometro Xml menuItem is visible", menuItem);
				Assert("Generate Esterometro Xml menuItem is enabled", menuItem.Enabled);

				var generateXmlButton = (ZButton)form.Controls.Find("generateXmlButton", true)[0];
				Assert("Generate Esterometro Xml button is visible", generateXmlButton.Visible);
				Assert("Generate Esterometro Xml button is enabled", generateXmlButton.Enabled);

				AssertEquals("Generate Esterometro Xml button Tooltip should have Generate Monthly Transaction XML", "Generate Monthly Transaction XML", generateXmlButton.ToolTipCaption);
				AssertEquals("Generate Esterometro Xml button text should have Generate XML", "Generate XML", generateXmlButton.Text);
			}
		}

		public void TestComplianceReportGenerateEsterometroButtonClick()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var report = CreateValidEsterometroReport();
				using (var form = new AccComplianceReportForm(report))
				{
					report.HandleGenerateEsterometroXml(form, showXsdValidationErrors: true);
					AssertEquals("An error message should be shown when XSD validation fails of Esterometro XML file.", "The Esterometro XML file was generated, but has validation errors.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Show();

					var generateXmlButton = (ZButton)form.Controls.Find("generateXmlButton", true)[0];
					AssertNotNull("Precondition: button must exist.", generateXmlButton);
					generateXmlButton.PerformClick();
					AssertEquals("Generate XML click should not show XSD validation message, even when there are XSD validation errors.", "The Esterometro XML file was generated successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestComplianceReportExportOpenFormat_IsNotVisible()
		{
			using (var testForm = (AccComplianceReportForm)GetFormToBash())
			{
				testForm.Show();
				AssertNotEquals(Core.Constants.CountryCodes.Israel, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

				var menuItem = testForm.Menu.MenuItems.FindByText("Export Open Format Files", true);
				AssertNull("Export open format files action is not visible for non Israel countries", menuItem);

				var exportButton = (ZButton)testForm.Controls.Find("generateXmlButton", true)[0];
				Assert("Export TXT button is not visible", !exportButton.Visible);
				Assert("Export TXT button is not enabled", !exportButton.Enabled);
			}
		}

		public void TestComplianceReportExportOpenFormat_IsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Israel))
			using (var form = new AccComplianceReportForm(CreateValidOpenFormatReport()))
			{
				form.Show();

				var menuItem = form.Menu.MenuItems.FindByText("Export Open Format Files", true);
				AssertNotNull("Export open format files action is visible for Israel", menuItem);
				Assert("action is enabled", menuItem.Enabled);

				var exportButton = (ZButton)form.Controls.Find("generateXmlButton", true)[0];
				Assert("Export TXT button is visible", exportButton.Visible);
				Assert("Export TXT button is enabled", exportButton.Enabled);

				AssertEquals("Export Open Format Files", exportButton.ToolTipCaption);
				AssertEquals("Export Files", exportButton.Text);
			}
		}

		public void TestLoadComplianceReportFormGridLines()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var creator = new TestObjectCreator(Factory);
				int transactionCount = 1;
				var firstDayOfMonth = ZDateTime.Today.AddDays(-ZDateTime.Today.Day + 1).Date;

				creator.CreateTestPeriods(firstDayOfMonth);

				var report = Factory.NewWithValidTestData<AccComplianceReport>();

				creator.CreateConfigurationForComplianceReport(report, "AL", "");

				report.ACR_ReportType = AccComplianceReport.ReportTypes.LiquidazioneIVA;

				Factory.Save();

				creator.AddTransactionsToComplianceReport(20, report, ref transactionCount);

				report.ClearReportLines_ForTestOnly();
				AssertEquals("ReportLines.Count", 0, report.ReportLines.Count);
				report.GenerateFromQueue();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				report.ClearReportLines_ForTestOnly();
				using (var form = new AccComplianceReportForm(report))
				{
					form.Show();

					AssertNull("There should be no error message.", UnitTestUserNotification.Instance.LastMessage.Text);

					var gridLines = form.Controls.Find("reportCurrentPeriodLinesGrid", true).FirstOrDefault() as ZGrid;
					AssertNotNull(gridLines);
					AssertEquals(40, gridLines.List.Count);
				}

				creator.AddTransactionsToComplianceReport(55, report, ref transactionCount);

				report.GenerateFromQueue();

				AssertComplianceReportFormGridLinesWarningMessage(DialogResult.No);

				AssertComplianceReportFormGridLinesWarningMessage(DialogResult.Yes);

				void AssertComplianceReportFormGridLinesWarningMessage(DialogResult button)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					report.ClearReportLines_ForTestOnly();
					UnitTestUserNotification.Instance.AddAnswer(button);
					using (var form = new AccComplianceReportForm(report))
					{
						form.Show();

						AssertEquals("A warning message can be present.", @"The report contains more than 100 rows. Click Yes to load the report form with only the first 100 records displayed.
Otherwise, if you wish to display all records, click No. Please be advised that it may take a while to load the full report.

Would you like to preview the report with only first 100 rows displayed?", UnitTestUserNotification.Instance.LastMessage.Text);

						var gridLines = form.Controls.Find("reportCurrentPeriodLinesGrid", true).FirstOrDefault() as ZGrid;
						AssertNotNull(gridLines);
						AssertEquals(button == DialogResult.Yes ? 100 : 150, gridLines.List.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						report.ClearReportLines_ForTestOnly();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

						report.HandleLiquidazioneIVAViewAndSubmit();

						var warnigMessage = @"The report contains more than 100 rows. In order to proceed, all records must be retrieved.
Please be advised that it may take a while to load the data.";

						if (button == DialogResult.Yes)
						{
							Assert(UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(warnigMessage));
						}
						else
						{
							Assert(!UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(warnigMessage));
						}
					}
				}
			}
		}

		public void TestComplianceReportVatSummaryButtonIsNotVisible()
		{
			using (var testForm = (AccComplianceReportForm)GetFormToBash())
			{
				testForm.Show();
				var formBizo = (AccComplianceReport)testForm.BusinessEntity;
				AssertNotEquals(Core.Constants.CountryCodes.Italy, GlbCompany.CurrentCompany.Country.Code);

				var menuItem = testForm.Menu.MenuItems.FindByText("VAT Summary Report", true);
				AssertNull("VAT Summary Report menuItem is not visible for non Italy countries", menuItem);

				var vatSummaryButton = (ZButton)testForm.Controls.Find("vatSummaryButton", true)[0];
				Assert("VAT Summary Report button is not visible", !vatSummaryButton.Visible);
				Assert("VAT Summary Report button is not enabled", !vatSummaryButton.Enabled);
			}
		}

		public void TestComplianceReportVatSummaryButtonIsVisible()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			using (var form = new AccComplianceReportForm(CreateValidLiquidazioneIvaReport()))
			{
				form.Show();

				var menuItem = form.Menu.MenuItems.FindByText("VAT Summary Report", true);
				AssertNotNull("VAT Summary Report menuItem is visible", menuItem);
				Assert("VAT Summary Report menuItem is enabled", menuItem.Enabled);

				var vatSummaryButton = (ZButton)form.Controls.Find("vatSummaryButton", true)[0];
				Assert("VAT Summary Report button is visible", vatSummaryButton.Visible);
				Assert("VAT Summary Report button is enabled", vatSummaryButton.Enabled);

				AssertEquals("VAT Summary Report button text should have VSR= VAT Summary Report", "VAT Summary Report", vatSummaryButton.Text);
			}
		}

		public void TestComplianceReportVatSummaryButtonPosition()
		{
			var controller = ZControllerFactory.Create(ControllerIDs.AccComplianceReport);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			using (var form = (AccComplianceReportForm)controller.ShowEditForm(CreateValidLiquidazioneIvaReport()))
			{
				var vatSummaryButton = (ZButton)form.Controls.Find("vatSummaryButton", true)[0];
				var previousNextControl = form.Controls.Find("ZPreviousNextControl", true);
				AssertNotNull("Pre-Condition: PreviousNextControl should exists", previousNextControl);

				var previousNextControlEndPoint = previousNextControl[0].PointToScreen(Point.Empty).X + previousNextControl[0].Size.Width;
				var vatSummaryButtonStartPoint = vatSummaryButton.PointToScreen(Point.Empty).X;
				Assert("PreviousNextControl and viewAndSubmitButton should not overlap each other, viewAndSubmitButton start point should be equal or greater than PreviousNextControl end point.", vatSummaryButtonStartPoint >= previousNextControlEndPoint);
			}
		}

		public void TestComplianceReportReportingBooking_Visible()
		{
			var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
			Creator.CreateConfigurationForComplianceReport(report1,
				baseTablePrefix: ReportBaseTablePrefixListCodes.GeneralLedgerData, reportLineGrouping: ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBook,
				reportLineOrdering: ComplianceReportConfigurationLookups.ReportLineOrderingListCodes.ComplianceSubType);

			Assert("BaseTablePrefix is GLD", report1.IsUsingGLDTablePrefix);

			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();

			Assert("BaseTablePrefix is not GLD", !report2.IsUsingGLDTablePrefix);

			var report3 = Factory.NewWithValidTestData<AccComplianceReport>();
			report3.ACR_ReportType = ZString.Empty;

			AssertReportingBookingVisible("Reporting Book find box should not be added when report is not using GLD table prefix.", report2, true, false);
			AssertReportingBookingVisible("Reporting Book find box should not be added when EnableReportingBooksFeature is false.", report1, false, false);
			AssertReportingBookingVisible("Reporting Book find box should be added when EnableReportingBooksFeature is and report is using GLD table prefix.", report1, true, true);

			Assert("BaseTablePrefix is not GLD", !report3.IsUsingGLDTablePrefix);
			AssertReportingBookingVisible("Reporting Book find box should be added when EnableReportingBooksFeature is and default report is using GLD table prefix.", report3, true, true);
			Assert("BaseTablePrefix is GLD", report3.IsUsingGLDTablePrefix);
		}

		void AssertReportingBookingVisible(string message, AccComplianceReport report, bool enableReportingBooksFeature, bool isVisible)
		{
			using (AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableReportingBooksFeature))
			using (var testForm = new AccComplianceReportForm(report))
			{
				var aCR_ARB_ReportingBookGuidFindBox = testForm.Controls.Find("ACR_ARB_ReportingBookGuidFindBox", true).FirstOrDefault();

				if (isVisible)
				{
					AssertNotNull(message, aCR_ARB_ReportingBookGuidFindBox);
				}
				else
				{
					AssertNull(message, aCR_ARB_ReportingBookGuidFindBox);
				}
			}
		}

		public void TestComplianceReportGridColumnsVisibleForComplianceDocumentHeader()
		{
			var objectCreator = new TestObjectCreator(Factory);
			objectCreator.EnsureComplianceReportConfigInRegistry("TST", "ABN", ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.ComplianceDocumentHeader, null);
			var report = objectCreator.CreateComplianceReport("TST", null);
			Factory.Save();

			using (var testForm = new AccComplianceReportForm(report))
			{
				testForm.Show();
				var formBizo = (AccComplianceReport)testForm.BusinessEntity;

				var columnNames = new string[] {
						AccComplianceReportLine.Schema.AG_AccountNum,
						AccComplianceReportLine.Schema.AG_Description,
						AccComplianceReportLine.Schema.GB_Code,
						AccComplianceReportLine.Schema.GE_Code,
						AccComplianceReportLine.Schema.RepCountryRegNo,
						AccComplianceReportLine.Schema.InvoiceDate,
						AccComplianceReportLine.Schema.AT_Code,
						AccComplianceReportLine.Schema.TaxMessage,
						AccComplianceReportLine.Schema.ReportSubCode,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeInputAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeOutputAmount,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
						nameof(AccComplianceReportLine.PreCalculatedAmount),
						nameof(AccComplianceReportLine.PreCalculatedCount),
					};

				var columnNamesMapping = new Dictionary<string, ResourceStringData>();
				columnNamesMapping.Add(AccComplianceReportLineBase.Schema.TotalExTaxAmount, Res.GetData("24F7A694-406C-4EAC-8CDC-660EAF3A4597", "Amount"));
				columnNamesMapping.Add(AccComplianceReportLineBase.Schema.TotalTaxAmount, Res.GetData("92465163-C3B6-4A7D-82B9-114CEAE96395", "Tax Amount"));
				columnNamesMapping.Add(AccComplianceReportLine.Schema.PostDate, Res.GetData("512565C0-99FB-4FD6-9475-A8CCCA8E05BB", "Document Date"));
				columnNamesMapping.Add(AccComplianceReportLine.Schema.AH_TransactionNum, Res.GetData("94F92F0A-2378-4C2E-93D3-44FF963F90B7", "Document Number"));
				columnNamesMapping.Add(AccComplianceReportLine.Schema.AH_TransactionReference, Res.GetData("09574EBC-B4A7-49FD-B0B2-0A8AF9519284", "Reporting Period"));
				columnNamesMapping.Add(AccComplianceReportLine.Schema.AH_TransactionType, Res.GetData("91DF8B65-FC16-4870-8FFE-4F6AEB21C793", "Compliance Book"));

				var reportCurrentPeriodLinesGrid = testForm.GetControl<ZGrid>("reportCurrentPeriodLinesGrid");
				foreach (var columnName in columnNames)
				{
					AssertEquals($"column: {columnName} should be removed in reportCurrentPeriodLinesGrid", false, reportCurrentPeriodLinesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Any(x => x.ColumnName == columnName));
				}

				foreach (var mapping in columnNamesMapping)
				{
					var lineColumn = reportCurrentPeriodLinesGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(x => x.ColumnName == mapping.Key);
					if (lineColumn != null)
					{
						AssertEquals($"column should be renamed to {lineColumn.CaptionResourceString.ShortCaption} in reportCurrentPeriodLinesGrid", mapping.Value, lineColumn.CaptionResourceString);
					}
				}
			}
		}

		public void TestReportTypeTPRBehaviors()
		{
			var creator = new TestObjectCreator(Factory);
			AssertEquals("Pre-condition: Current country is Australia", Core.Constants.CountryCodes.Australia, GlbCompany.CurrentCompany.Country.Code);

			using (MasterFiles.Business.AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				creator.CreateTestPeriods(ZDateTime.Today);

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = "TPR";
				AssertEquals("ACR_Periodicity set from Report Type TPR", ReportPeriodicityCodes.ComplianceFinancialYear, report.ACR_Periodicity);
				AssertEquals(false, report.AccountingPeriodInfo.ReadOnly);

				var yearFilter = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, report.ACR_GC_Company);
				var periods = Factory.Load<AccPeriodManagement>(yearFilter);
				AssertEquals(12, periods.Length);
				var financialYear = periods.First().AM_Year;
				var financialYearRange = new AustraliaComplianceFinancialYear().FinancialYearRange(financialYear);

				report.AccountingPeriod = financialYear;
				AssertEquals("ACR_DateFrom", financialYearRange.Start, report.ACR_DateFrom);
				AssertEquals("ACR_DateTo", financialYearRange.End, report.ACR_DateTo);

				AssertEquals(AccComplianceReport.Status.ReportCreated, report.ACR_Status);
				Assert("Report is not generated yet", !report.SupportsTPAR);

				using (var testForm = new AccComplianceReportForm(report))
				{
					testForm.Show();
					AssertForm(testForm, false);
					testForm.Close();
				}

				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				Assert("Report is generated and ready to make TPAR file", report.SupportsTPAR);

				using (var testForm = new AccComplianceReportForm(report))
				{
					testForm.Show();
					AssertForm(testForm, true);
					testForm.Close();
				}
			}

			void AssertForm(AccComplianceReportForm testForm, bool viewAndSubmitVisible)
			{
				var periodCalcEdit = testForm.GetControl<ZCalcEdit>("periodCalcEdit");
				AssertEquals("When Periodicity is FYR Period label should be 'Financial Year'", "Financial Year", periodCalcEdit.CaptionResourceString.Caption);

				var columnNames = new string[] {
						AccComplianceReportLine.Schema.AG_AccountNum,
						AccComplianceReportLine.Schema.AG_Description,
						AccComplianceReportLine.Schema.RepCountryRegNo,
						AccComplianceReportLine.Schema.AT_Code,
						AccComplianceReportLine.Schema.TaxMessage,
						AccComplianceReportLine.Schema.ReportSubCode,
						AccComplianceReportLine.Schema.AH_ComplianceSubType,
						AccComplianceReportLineBase.Schema.GoodsExTaxAmount,
						AccComplianceReportLineBase.Schema.GoodsTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceExTaxAmount,
						AccComplianceReportLineBase.Schema.ServiceTaxAmount,
						AccComplianceReportLineBase.Schema.TaxRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxNotRecoverableAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeInputAmount,
						AccComplianceReportLineBase.Schema.TaxReverseChargeOutputAmount,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountCR,
						AccComplianceReportLineBase.Schema.GeneralLedgerAmountDR,
						nameof(AccComplianceReportLine.PreCalculatedAmount),
						nameof(AccComplianceReportLine.PreCalculatedCount),
					};

				assertRemovedColumns("reportCurrentPeriodLinesGrid");
				assertRemovedColumns("reportTotalsCurrentPeriodGrid");

				void assertRemovedColumns(string gridName)
				{
					var grid = testForm.GetControl<ZGrid>(gridName);
					AssertNotNull(gridName, grid);

					foreach (var columnName in columnNames)
					{
						AssertEquals($"column: {columnName} should be removed in {gridName}", false, grid.ColumnStyles.Cast<ZGridColumnInfo>().Any(x => x.ColumnName == columnName));
					}
				}

				var menuItem = testForm.Menu.MenuItems.FindByText("View and Submit", true);
				var viewAndSubmitButton = (ZButton)testForm.Controls.Find("viewAndSubmitButton", true).FirstOrDefault();
				AssertNotNull("View and Submit button", viewAndSubmitButton);
				if (viewAndSubmitVisible)
				{
					AssertNotNull("View and Submit menu item is visible", menuItem);
					Assert("View and Submit menuItem is enabled", menuItem.Enabled);

					AssertNotNull("View and Submit button", viewAndSubmitButton);
					Assert("View and Submit button is visible", viewAndSubmitButton.Visible);
					Assert("View and Submit button is enabled", viewAndSubmitButton.Enabled);
				}
				else
				{
					AssertNull("View and Submit menuItem", menuItem);
					Assert("View and Submit button is not enabled", !viewAndSubmitButton.Enabled);
				}
			}
		}

		public void TestSetDefaultReportType()
		{
			using (AddConfiguration("LIB", "PTR", "PTA"))
			{
				var report = Factory.New<AccComplianceReport>();
				AssertNullOrEmpty(report.ACR_ReportType);
				using (var form = new AccComplianceReportForm(report))
				{
					form.Show();
					AssertEquals("LIB", report.ACR_ReportType);
				}
			}
		}

		public void TestSetDefaultReportType_NoConfiguration()
		{
			using (AddConfiguration())
			{
				var report = Factory.New<AccComplianceReport>();
				AssertNullOrEmpty(report.ACR_ReportType);
				using (var form = new AccComplianceReportForm(report))
				{
					form.Show();
					AssertNullOrEmpty(report.ACR_ReportType);
				}
			}
		}

		public void TestHasError_WhenComplianceReportFromGLDConfiguration()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var allTransactionsReportConfig = complianceConfig.AddNew();
			allTransactionsReportConfig.ReportCode = "TST";
			allTransactionsReportConfig.ReportTitle = "Test Tax Report";
			allTransactionsReportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
			allTransactionsReportConfig.ReportPeriodicity = ReportPeriodicityCodes.AccountingPeriod;
			allTransactionsReportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			allTransactionsReportConfig.TaxRegistrationType = "ABN";
			allTransactionsReportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBook;

			var gldReportConfig = complianceConfig.AddNew();
			gldReportConfig.ReportCode = "TSG";
			gldReportConfig.ReportTitle = "Test Tax Report";
			gldReportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.GeneralLedgerData;
			gldReportConfig.ReportPeriodicity = ReportPeriodicityCodes.AccountingPeriod;
			gldReportConfig.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gldReportConfig.TaxRegistrationType = "ABN";
			gldReportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBook;

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceConfig);

			var gldComplianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			gldComplianceReport.ACR_ReportType = gldReportConfig.ReportCode;
			gldComplianceReport.ACR_DateFrom = ZDate.Today.AddDays(-2);
			gldComplianceReport.ACR_DateTo = ZDate.Today.AddDays(2);
			gldComplianceReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			var allTransactionsComplianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			allTransactionsComplianceReport.ACR_ReportType = allTransactionsReportConfig.ReportCode;
			allTransactionsComplianceReport.ACR_DateFrom = ZDate.Today.AddDays(-2);
			allTransactionsComplianceReport.ACR_DateTo = ZDate.Today.AddDays(2);
			allTransactionsComplianceReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			Factory.Save();

			AssertHasErrorForbidReQueue(gldComplianceReport, true);
			AssertHasErrorForbidReQueue(allTransactionsComplianceReport, false);

			void AssertHasErrorForbidReQueue(AccComplianceReport complianceReport, bool result)
			{
				using (var form = new AccComplianceReportForm(complianceReport))
				{
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var button = (ZButton)form.Controls.Find("reQueueButton", true)[0];
					button.PerformClick();

					var expectedMesg = "The compliance report is generated using accounting journals data source. The queue and re-queue actions are not applicable.";
					AssertEquals(result, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(expectedMesg));
				}
			}
		}

		IDisposable AddConfiguration(params ZString[] reportCodes)
		{
			return AddConfigurationWithOptions(ReportLineGroupingListCodes.DayBook, reportCodes);
		}

		IDisposable AddConfigurationWithOptions(string reportLineGrouping, ZString[] reportCodes)
		{
			var reportConfigurations = new ComplianceReportConfigurationCollection();
			foreach (ZString reportCode in reportCodes)
			{
				var reportConfig = reportConfigurations.AddNew();
				reportConfig.Country = Env.CurrentCompany.Country.Code;
				reportConfig.ReportCode = reportCode;
				reportConfig.ReportTitle = ZString.Format("{0} Report", reportCode);
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
				reportConfig.ReportLineGrouping = reportLineGrouping;
				reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
				reportConfig.TaxRegistrationType = reportConfig.Lookups.TaxRegistrationTypeList[0].Code;
			}

			return AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, reportConfigurations);
		}

		public void TestReportLinesGridNotVisibleForIDEAReport()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (AddConfigurationWithOptions(ReportLineGroupingListCodes.DayBook, new ZString[] { "IDE" }))
			using (var form = new AccComplianceReportForm(report))
			{
				form.Show();
				var labelNoReportLinesHint = GetControlByName(form, "LabelNoReportLinesHint");
				AssertNotNull("LabelNoReportLinesHint is shown", labelNoReportLinesHint);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			using (AddConfigurationWithOptions(ReportLineGroupingListCodes.DayBookWithPresentation, new ZString[] { "IDE" }))
			using (var form = new AccComplianceReportForm(report))
			{
				form.Show();
				var labelNoReportLinesHint = GetControlByName(form, "LabelNoReportLinesHint");
				AssertNotNull("LabelNoReportLinesHint is shown", labelNoReportLinesHint);
			}
		}

		public void TestReportLinesGridNotVisibleForFECReport()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = AccComplianceReport.ReportTypes.FEC;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (var form = new AccComplianceReportForm(report))
			{
				form.Show();
				var labelNoReportLinesHint = GetControlByName(form, "LabelNoReportLinesHint");
				AssertNotNull("LabelNoReportLinesHint is shown", labelNoReportLinesHint);
			}
		}

		public void TestReportLinesGridVisible()
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			var creator = new TestObjectCreator(Factory);
			using (var form = new AccComplianceReportForm(report))
			{
				form.Show();
				var labelNoReportLinesHint = GetControlByName(form, "LabelNoReportLinesHint");
				AssertNull("LabelNoReportLinesHint is not shown", labelNoReportLinesHint);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var report = Factory.New<AccComplianceReport>();
			return new AccComplianceReportForm(report);
		}

		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		#endregion

		#region Test Helpers
		AccComplianceReport CreateValidEsterometroReport()
		{
			var creator = new TestObjectCreator(Factory);
			creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.Esterometro, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TaxReporting);
			var report = creator.CreateComplianceReport(AccComplianceReport.ReportTypes.Esterometro, AccComplianceReport.Status.ReportGenerated, ReportPeriodicityCodes.AccountingPeriod);
			Factory.Save();
			Assert(report.SupportsEsterometroXmlGeneration);
			return report;
		}

		AccComplianceReport CreateValidOpenFormatReport()
		{
			var creator = new TestObjectCreator(Factory);
			creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.OpenFormatOnlyTransactions, OrgCusCode.CodeTypes.VATCode, ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.NoGrouping);
			var report = creator.CreateComplianceReport(AccComplianceReport.ReportTypes.OpenFormatOnlyTransactions, AccComplianceReport.Status.ReportGenerated);
			Factory.Save();
			Assert(report.SupportsExportOpenFormatFile);
			return report;
		}

		AccComplianceReport CreateValidLiquidazioneIvaReport()
		{
			var creator = new TestObjectCreator(Factory);
			creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.LiquidazioneIVA, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TaxReporting);
			var report = creator.CreateComplianceReport(AccComplianceReport.ReportTypes.LiquidazioneIVA, AccComplianceReport.Status.ReportGenerated, ReportPeriodicityCodes.AccountingPeriod);
			Factory.Save();
			Assert(report.SupportsLiquidazioneIVA);
			return report;
		}

		Control GetControlByName(AccComplianceReportForm form, string controlName)
		{
			var controls = form.Controls.Find(controlName, true);
			if (controls == null || controls.Length == 0)
			{
				return null;
			}

			return controls[0];
		}
		#endregion
	}
}

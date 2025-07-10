namespace Enterprise.Accounting.GUI.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Security.AccessControl;
	using System.Windows.Forms;
	using CargoWise.Application;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.IO;
	using CargoWise.Types;
	using Enterprise.Accounting.Business;
	using Enterprise.Accounting.Business.AccountingCountryFactory;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.Accounting.Business.ComplianceReport;
	using Enterprise.Accounting.Business.GeneralLedger.GLBudget;
	using Enterprise.Accounting.GUI.ComplianceReport;
	using Enterprise.Accounting.GUI.ComplianceReport.PTRS;
	using Enterprise.Accounting.Registry.Business;
	using Enterprise.Accounting.Utility.Testing;
	using Enterprise.DocumentScanning.Business;
	using Enterprise.Environment;
	using Enterprise.Integration.Accounting;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Business.CustomValues;
	using Enterprise.MasterFiles.Business.Testing;
	using Enterprise.MasterFiles.Integration;
	using Enterprise.Messaging.Integration;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.GUI;
	using Enterprise.ZArchitecture.Schema;
	using Moq;
	using Newtonsoft.Json.Linq;
	using NUnit.Framework;
	using static Enterprise.Accounting.Business.AccountingConstants;
	using static Enterprise.Core.Constants;
	using static Enterprise.Registry.Business.ComplianceReportConfigurationLookups;

	public class AccComplianceReportGuiHelperTest : TestCaseWithFactory
	{
		public void TestHandleFinalize()
		{
			Env.Security.FinalizeComplianceReport.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.HandleFinalize();

			AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> General Ledger -> Compliance Reports -> Finalize", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Security rights: Report's last message type wasn't Error.", UnitTestUserNotification.Instance.LastMessage.WasError);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.Security.FinalizeComplianceReport.IsAllowed = true;
			Factory.Save();
			report.Finalise();
			Assert(report.ACR_IsFinalised);
			report.HandleFinalize();
			AssertEquals("This Report is already finalized.", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Report already finialized: Report's last message type wasn't Error.", UnitTestUserNotification.Instance.LastMessage.WasInformation);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
			report1.ACR_ReportType = "TST";
			report1.ACR_DateFrom = new ZDate(2018, 3, 1);
			report1.ACR_DateTo = new ZDate(2018, 3, 31);
			report1.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			report2.ACR_ReportType = "TST";
			report2.ACR_DateFrom = new ZDate(2018, 4, 1);
			report2.ACR_DateTo = new ZDate(2018, 4, 30);
			report2.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			Assert(!report2.IsPreviousReportFinalized());
			report2.HandleFinalize();
			AssertEquals("You cannot finalize this report because the previous report is not finalized.", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Previous report not finialized: Report's last message type wasn't Error.", UnitTestUserNotification.Instance.LastMessage.WasError);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			report1.ACR_Status = AccComplianceReport.Status.ReportFinalised;
			Assert(report2.IsPreviousReportFinalized());
			Factory.Save();

			report2.HandleFinalize();
			var expectedMessage = "You would not be able to change this Report after finalizing. Are you sure you want to proceed?";
			AssertEquals("Last Confirmation", "Yes", UnitTestUserNotification.Instance.LastConfirmationStringShown);
			Assert("Report2: Report's last message type wasn't Question.", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals(AccComplianceReport.Status.ReportFinalised, report2.ACR_Status);

			var today = ZDate.Today;
			var report3 = Factory.NewWithValidTestData<AccComplianceReport>();
			report3.ACR_ReportType = "TST";
			report3.ACR_DateFrom = today.AddDays(-10);
			report3.ACR_DateTo = today.AddDays(10);
			report3.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			Factory.Save();

			report3.HandleFinalize();
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Report3: Report's last message type wasn't Question.", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals(AccComplianceReport.Status.ReportFinalised, report3.ACR_Status);

			GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan);
			var configs = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var config = configs.AddNew();
			config.Country = "TW";
			config.ReportCode = "TXC";
			config.ReportPeriodicity = "RNG";
			config.ReportLineOrdering = "FDN";
			config.ReportBaseTablePrefix = "ADH";
			config.TaxRegistrationType = "VAT";
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, configs);

			var report4 = Factory.NewWithValidTestData<AccComplianceReport>();
			report4.ACR_ReportType = "TXC";
			report4.ACR_DateFrom = today.AddDays(-10);
			report4.ACR_DateTo = today.AddDays(10);
			report4.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			report4.HandleFinalize();
			AssertEquals("You cannot finalize this report because current date is not bigger than report's end date.", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("End date not reached: Report's last message type wasn't Error.", UnitTestUserNotification.Instance.LastMessage.WasError);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals(AccComplianceReport.Status.ReportGenerated, report4.ACR_Status);

			var report5 = Factory.NewWithValidTestData<AccComplianceReport>();
			report5.ACR_Status = AccComplianceReport.Status.ReportDataQueued;
			Factory.Save();

			report5.HandleFinalize();
			AssertEquals("Report can be finalized only when it is in 'Generated' status.", UnitTestUserNotification.Instance.LastMessage.Text);
			Assert("Report not Generated: Report's last message type wasn't Error.", UnitTestUserNotification.Instance.LastMessage.WasError);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals(AccComplianceReport.Status.ReportDataQueued, report5.ACR_Status);
		}

		public void TestHandleFinalizeTPAR()
		{
			var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
			report2.ACR_ReportType = ComplianceReportTypes.TaxablePaymentsAnnualReportType;
			report2.ACR_DateFrom = new ZDate(2018, 4, 1);
			report2.ACR_DateTo = new ZDate(2018, 4, 30);
			report2.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			Assert(report2.SupportsTPAR);
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			report2.HandleFinalize();
			var expectedMessage = @"Ensure that the TPAR lodgement file has been generated and successfully lodged for this TPAR reporting period. After finalizing:
You can no longer add or adjust any balances for this TPAR reporting period.
You cannot regenerate the report for this TPAR reporting period.
The report data is locked and cannot be unlocked.";
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Last Confirmation", "Yes", UnitTestUserNotification.Instance.LastConfirmationStringShown);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestHandleGenerate()
		{
			var securityRight = Env.Security.GenerateComplianceReport;
			securityRight.IsAllowed = false;
			var userNotif = UnitTestUserNotification.Instance;
			userNotif.ClearMessagesAndAnswers();
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.HandleGenerate();

			AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> General Ledger -> Compliance Reports -> Generate", userNotif.LastMessage.Text);

			var registry = AccountingMasterFilesRegistry.Instance;
			var currCompany = Env.CurrentCompany;
			var complianceConfig = registry.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "TST";
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
			reportConfig.Country = currCompany.Country.Code;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.NoGrouping;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionHeader;

			using (registry.ComplianceReportConfiguration.SetTemporaryValue(currCompany.PK, Guid.Empty, Guid.Empty, complianceConfig))
			{
				report.ACR_ReportType = "TST";
				report.ACR_DateFrom = ZDate.Today.AddDays(1);
				report.ACR_DateTo = ZDate.Today.AddDays(10);
				Factory.Save();

				userNotif.ClearMessagesAndAnswers();
				securityRight.IsAllowed = true;
				Assert(!report.ACR_IsFinalised);
				report.HandleGenerate();
				AssertNull(userNotif.LastMessage.Text);
				userNotif.ClearMessagesAndAnswers();

				using (var form = new AccComplianceReportForm(report))
				{
					report.HandleGenerate(form);
				}
				AssertEquals("The Report was successfully generated.\r\nCompliance Report form will be reloaded to update details.", userNotif.LastMessage.Text);
				userNotif.ClearMessagesAndAnswers();

				report.Finalise();
				Assert(report.ACR_IsFinalised);
				report.HandleGenerate();
				AssertEquals("Report data has been finalized. No change can be made after finalization.", userNotif.LastMessage.Text);
				userNotif.ClearMessagesAndAnswers();
			}
		}

		public void TestHandleGenerateByCRQServiceTask()
		{
			var securityRight = Env.Security.GenerateComplianceReport;
			securityRight.IsAllowed = true;
			var userNotif = UnitTestUserNotification.Instance;
			userNotif.ClearMessagesAndAnswers();
			var report = Factory.NewWithValidTestData<AccComplianceReport>();

			var registry = AccountingMasterFilesRegistry.Instance;
			var currCompany = Env.CurrentCompany;
			var complianceConfig = registry.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "FEC";
			reportConfig.ReportTitle = "Test FEC Report";
			reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.RangeAccountingPeriod;
			reportConfig.Country = Core.Constants.CountryCodes.France;
			reportConfig.TaxRegistrationType = "TVA";
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			using (registry.ComplianceReportConfiguration.SetTemporaryValue(currCompany.PK, Guid.Empty, Guid.Empty, complianceConfig))
			{
				report.ACR_ReportType = "FEC";
				report.ACR_DateFrom = ZDate.Today.AddDays(1);
				report.ACR_DateTo = ZDate.Today.AddDays(10);
				Factory.Save();
				Assert(report.GeneratedByCRQServiceTask);

				userNotif.ClearMessagesAndAnswers();
				Assert(!report.ACR_IsFinalised);
				report.HandleGenerate();
				AssertEquals("This report is automatically generated in the background when you add or re-queue the report.\r\nWhen ready, status shows as 'Generated' and a report file is saved to eDocs.", userNotif.LastMessage.Text);
				Assert("Report's last message type wasn't Error.", UnitTestUserNotification.Instance.LastMessage.WasError);
				userNotif.ClearMessagesAndAnswers();

				using (var form = new AccComplianceReportForm(report))
				{
					report.HandleGenerate(form);
				}
				AssertEquals("This report is automatically generated in the background when you add or re-queue the report.\r\nWhen ready, status shows as 'Generated' and a report file is saved to eDocs.", userNotif.LastMessage.Text);
				Assert("Report's last message type wasn't Error.", UnitTestUserNotification.Instance.LastMessage.WasError);
				userNotif.ClearMessagesAndAnswers();
			}
		}

		public void TestHandleGenerateQueuedReportDataNotReady()
		{
			var userNotifications = UnitTestUserNotification.Instance;
			userNotifications.ClearMessagesAndAnswers();

			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			var registry = AccountingMasterFilesRegistry.Instance;
			var company = Env.CurrentCompany;
			var complianceConfig = registry.ComplianceReportConfiguration.Value;

			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "TST";
			reportConfig.ReportTitle = "Report for Unit Test";
			reportConfig.Country = company.Country.Code;
			reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;    // This causes the report to need queueing

			var expectedErrorMessage =
					"This report can only be generated when it is in status 'Queued' (QUE).\r\n" +
					"Please check the status message for additional information and wait for status 'Queued' (QUE) or re-queue if necessary.";

			using (registry.ComplianceReportConfiguration.SetTemporaryValue(company.PK, Guid.Empty, Guid.Empty, complianceConfig))
			{
				report.ACR_ReportType = "TST";
				report.ACR_DateFrom = ZDate.Today.AddDays(1);
				report.ACR_DateTo = ZDate.Today.AddDays(10);
				Factory.Save();

				// Get all report status codes that are not "Queued"
				var reportStatusCodesToTest = typeof(AccComplianceReport.Status)
					.GetFields(BindingFlags.Public | BindingFlags.Static)
					.Where(x => x.IsLiteral && x.FieldType == typeof(string))
					.Select(x => (string)x.GetValue(null))
					.Where(x => x != AccComplianceReport.Status.ReportDataQueued)
					.ToArray();

				foreach (var reportStatus in reportStatusCodesToTest)
				{
					userNotifications.ClearMessagesAndAnswers();
					report.ACR_Status = reportStatus;
					Factory.Save();
					AssertNull(userNotifications.LastMessage.Text);

					report.HandleGenerate();
					AssertEquals(expectedErrorMessage, userNotifications.LastMessage.Text);
				}

				userNotifications.ClearMessagesAndAnswers();
			}
		}

		public void TestHandleReQueue()
		{
			Env.Security.ReQueueComplianceReport.IsAllowed = false;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.HandleReQueue();

			AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> General Ledger -> Compliance Reports -> Re-Queue", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Env.Security.ReQueueComplianceReport.IsAllowed = true;
			Assert(!report.SupportsReQueue);
			report.HandleReQueue();
			AssertEquals("This Report type does not support re-queue.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "TST";
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
			reportConfig.Country = Env.CurrentCompany.Country.Code;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			report.ACR_ReportType = "TST";
			report.ACR_DateFrom = ZDate.Today.AddDays(1);
			report.ACR_DateTo = ZDate.Today.AddDays(10);
			Factory.Save();

			Assert(report.SupportsReQueue);
			report.HandleReQueue();
			AssertContains("Queue will be re-created by a background service task for all relevant transactions in the Report's date range.\r\nPlease click Yes to confirm you want to re-queue the report.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "TPR";
			reportConfig.ReportTitle = "Test TPAR Report";
			reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.FinancialYear;
			reportConfig.Country = Env.CurrentCompany.Country.Code;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.TransactionPayments;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.TransactionHeader;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			report.ACR_ReportType = "TPR";
			report.ACR_DateFrom = ZDate.Today.AddDays(1);
			report.ACR_DateTo = ZDate.Today.AddDays(10);
			Factory.Save();

			Assert(report.SupportsReQueue);
			report.HandleReQueue();
			AssertContains("Queue will be re-created by a background service task for all relevant transactions in the Report's date range.\r\nPlease click Yes to confirm you want to re-queue the report.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertNotContains("IDEA ZIP files", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				reportConfig = complianceConfig.AddNew();
				reportConfig.ReportCode = AccComplianceReport.ReportTypes.IDEA;
				reportConfig.ReportTitle = "IDEA Data Extract";
				reportConfig.ReportPeriodicity = ReportPeriodicityCodes.DateRange;
				reportConfig.Country = CountryCodes.Germany;
				reportConfig.TaxRegistrationType = "UST";
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

				var ideaReport = Factory.NewWithValidTestData<AccComplianceReport>();
				ideaReport.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;
				ideaReport.ACR_DateFrom = ZDate.Today.AddDays(1);
				ideaReport.ACR_DateTo = ZDate.Today.AddDays(365);
				ideaReport.NextProcessingStepFromDate = ZDate.Today.AddDays(30);
				Factory.Save();
				Assert("IDEA report should support re-queuing", ideaReport.SupportsReQueue);
				var genAddOn = Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_Name, "NextProcessingStepFromDate"));
				Assert("At least one GenAddOnColumn record created", genAddOn.Length > 0);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ideaReport.HandleReQueue();
				AssertContains("Queue will be re-created by a background service task for all relevant transactions in the Report's date range.\r\nPlease click Yes to confirm you want to re-queue the report.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertCountOfAttachedEDocs(ideaReport.DocManagerInfo(), "No XML files should be attached to eDocs", 0);
				Factory.Save();
				genAddOn = Factory.Load<GenAddOnColumn>(new ZQuery(GenAddOnColumnSchema.XA_Name, "NextProcessingStepFromDate"));
				AssertEquals("GenAddOnColumn data deleted", 0, genAddOn.Length);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestDeleteEDocsIfRequired()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				using (Factory.AddDisposableService())
				{
					AccountingMasterFilesRegistryTest.EnableAllGermanComplianceReports();
					var ideaReport = CreateTestComplianceReport(AccComplianceReport.ReportTypes.IDEA);
					Factory.Save();
					UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
					var allEDocs = ideaReport.DocManagerInfo().AllEDocs;
					AssertEquals("Number of documents attached to IDEA report before re-queuing", 5, allEDocs.Count);
					AssertEquals("Number of system-generated documents attached to IDEA report before re-queuing", 2, allEDocs.Cast<StorageDocsBase>().Count(x => x.SC_IsSystemGenerated));
					ideaReport.HandleReQueue();
					AssertEquals("Number of documents attached to IDEA report after re-queuing", 3, allEDocs.Count);
					AssertEquals("Number of system-generated documents attached to IDEA report after re-queuing", 0, allEDocs.Cast<StorageDocsBase>().Count(x => x.SC_IsSystemGenerated));
					AssertEquals("Name of first retained document", "1.xxx", allEDocs[0].FileName);
					AssertEquals("Name of second retained document", "IDEA-Export-2.xxx", allEDocs[1].FileName);
					AssertEquals("Name of third retained document", "Documents4.zip", allEDocs[2].FileName);

					var otherReport = CreateTestComplianceReport(AccComplianceReport.ReportTypes.ZMGermany);
					Factory.Save();
					UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);
					otherReport.HandleReQueue();
					allEDocs = otherReport.DocManagerInfo().AllEDocs;
					AssertEquals("Number of documents attached to non-IDEA report", 5, allEDocs.Count);
				}
			}

			AccComplianceReport CreateTestComplianceReport(string reportType)
			{
				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_ReportType = reportType;
				complianceReport.ACR_DateFrom = ZDate.Today.AddDays(1);
				complianceReport.ACR_DateTo = ZDate.Today.AddDays(365);
				var fileContent = new byte[] { 1, 2, 3, 4, };
				var docManager = complianceReport.DocManagerInfo();

				const string testData = "Test data";
				const string fileDescription = "File Description";

				docManager.AddFileOrDocument(fileContent, "1.xxx", RefDocTypes.MiscellaneousDocument);
				docManager.AddFileOrDocument(fileContent, "IDEA-Export-2.xxx", RefDocTypes.MiscellaneousDocument);

				var testFileFullPath3 = Path.Combine(Env.TempPath, "IDEA-Export-3.zip");
				File.WriteAllText(testFileFullPath3, testData);
				complianceReport.AttachFileToEdoc(testFileFullPath3, fileDescription);

				docManager.AddFileOrDocument(fileContent, "Documents4.zip", RefDocTypes.MiscellaneousDocument);

				var testFileFullPath5 = Path.Combine(Env.TempPath, $"IDEA-Export-{ZDateTime.Now.ToString("yyyyMMdd-HHmm", CultureInfo.InvariantCulture)}.zip");
				File.WriteAllText(testFileFullPath5, testData);
				complianceReport.AttachFileToEdoc(testFileFullPath5, fileDescription);

				Factory.Save();
				File.Delete(testFileFullPath3);
				File.Delete(testFileFullPath5);

				return complianceReport;
			}
		}

		public void TestHandleGenerateSAFTXml_CompressWhenExceedMaxSize()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				Env.Security.ExportComplianceReport.IsAllowed = true;
				var creator = new TestObjectCreator(Factory);
				creator.CreateTestPeriods(ZDateTime.Today.AddYears(-1));

				var taxRateCollection = new AccTaxRateCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var validTaxRate = taxRateCollection.AddNew();
				validTaxRate.AT_Code = "TGST";
				validTaxRate.AT_Type = "RAT";
				validTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxRate.SetRateNumerator_ForTestOnly(10);
				validTaxRate.AT_ExtraTaxRateType = "QST";
				validTaxRate.SetExtraRate_ForTestOnly(4, 2);

				var taxMessageCollection = new AccInvMsgCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var validTaxMessage = taxMessageCollection.AddNew();
				validTaxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxMessage.A9_TaxGroupCode = "N1";

				Factory.Save();

				var taxMessageGroupsManagement = new CodeDescriptionBoolRelatedItemCollection
				{
					{ "N1", (NoResString)"Description N1", true, "N1.0" },
				};
				AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, taxMessageGroupsManagement);

				Factory.Save();

				var config = creator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Revenue, validTaxRate, validTaxMessage));
				AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);
				var reportConfig = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, OrgCusCode.NorwayCodeTypes.MVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = AccComplianceReport.ReportTypes.SAFT;
				report.ACR_DateFrom = ZDate.Today.AddDays(1);
				report.ACR_DateTo = ZDate.Today.AddDays(10);
				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				Factory.Save();

				report.HandleGenerateSAFTXml(form);
				var expectedMessage = @"The SAFT XML file was generated successfully.
";
				AssertExportedFile(report.DocManagerInfo(), expectedMessage, false);

				var mockIComplianceReportGUIActionProvider = new Mock<IComplianceReportGUIActionProvider>();
				mockIComplianceReportGUIActionProvider.Setup(x => x.IsCountrySupportGenerateSAFT).Returns(true);
				mockIComplianceReportGUIActionProvider.Setup(x => x.GetSAFTFileCompressionInfo()).Returns((true, 1));

				var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
				mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceReportGUIActionProvider>>().Setup(x => x.Get()).Returns(mockIComplianceReportGUIActionProvider.Object);
				mockIAccountingCountryFactory.As<IInstanceProvider<IReportModeAndCreditorSelectorDefault>>().Setup(x => x.Get()).Returns(new NorwayReportModeAndCreditorSelectorDefault());
				mockIAccountingCountryFactory.As<IInstanceProvider<IReportSAFTWriter>>().Setup(x => x.Get()).Returns(new NorwayReportSAFTWriterProvider());

				var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
				mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

				using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
				{
					((StorageDocsBase)report.DocManagerInfo().Files[0]).Delete();
					Factory.Save();
					report.HandleGenerateSAFTXml(form);

					expectedMessage = @"The SAFT XML file was generated successfully.
As the XML exceeded the maximum file size accepted by the Tax Authority, the XML is compressed into a ZIP file.
The exported file exceed the maximum file size accepted by the Tax Authority, please raise an eRequest for assistance.
";
					AssertExportedFile(report.DocManagerInfo(), expectedMessage, true);
				}

				mockIComplianceReportGUIActionProvider.Setup(x => x.GetSAFTFileCompressionInfo()).Returns((true, 1000));
				mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceReportGUIActionProvider>>().Setup(x => x.Get()).Returns(mockIComplianceReportGUIActionProvider.Object);
				mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);
				using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
				{
					((StorageDocsBase)report.DocManagerInfo().Files[0]).Delete();
					Factory.Save();

					report.HandleGenerateSAFTXml(form);
					expectedMessage = @"The SAFT XML file was generated successfully.
As the XML exceeded the maximum file size accepted by the Tax Authority, the XML is compressed into a ZIP file.
";
					AssertExportedFile(report.DocManagerInfo(), expectedMessage, true);
				}
			}

			void AssertExportedFile(DocManagerInfo docInfo, string expectedMessage, bool isAfterCompressed)
			{
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertCountOfAttachedEDocs(docInfo, "One XML file should be attached to eDocs", 1);
				var fileName = docInfo.Files[0].FileName;
				var fileLength = new FileInfo(fileName).Length;

				if (isAfterCompressed)
				{
					Assert("XML file is compressed to zip file.", fileLength < 1000);
					Assert(fileName.Contains(".zip"));
				}
				else
				{
					Assert("XML file is not compressed", fileLength > 1000);
				}
			}
		}

		public void TestHandleGenerateSAFTXml()
		{
			foreach (var reportType in new[] { AccComplianceReport.ReportTypes.SAFT, AccComplianceReport.ReportTypes.SAFTOnlyTransactions })
			{
				using (var form = new ZForm())
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
				{
					Env.Security.ExportComplianceReport.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var report = Factory.NewWithValidTestData<AccComplianceReport>();
					report.HandleGenerateSAFTXml(form);

					AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> General Ledger -> Compliance Reports -> Export Compliance Report", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Env.Security.ExportComplianceReport.IsAllowed = true;
					Assert("Precondition", !report.SupportsSAFT);
					report.HandleGenerateSAFTXml(form);
					AssertEquals("SAFT XML can be generated only for Report Types 'SAF' and 'SAT'.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var creator = new TestObjectCreator(Factory);
					creator.CreateTestPeriods(ZDateTime.Today);

					creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TransactionHeaderWithLines);

					report.ACR_ReportType = reportType;
					report.ACR_DateFrom = ZDate.Today.AddDays(1);
					report.ACR_DateTo = ZDate.Today.AddDays(10);
					report.ACR_Status = AccComplianceReport.Status.ReportCreated;
					Factory.Save();

					Assert("Precondition", report.SupportsSAFT);
					Assert("Precondition", !report.SupportsSAFTXmlGeneration);
					report.HandleGenerateSAFTXml(form);
					AssertEquals("Please, the report must be generated or finalized before generating the SAFT XML", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
					Assert("Precondition", report.SupportsSAFTXmlGeneration);
					report.HandleGenerateSAFTXml(form);
					AssertEquals(typeof(MultistepProgressForm), ZFormModaliser.LastFormShownForTest.GetType());
					var expectedMessage = "The SAFT XML file was generated successfully.\r\n";
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					const string expectedEventLogMessage = "Purpose: SAFT Single XML Exported";
					AssertOneMatchingEventLog(report.Logs, Events.DataExport, expectedEventLogMessage);
					AssertAllAttachedEDocs(report.DocManagerInfo(), RefDocTypes.ComplianceReport);
					AssertCountOfAttachedEDocs(report.DocManagerInfo(), "One XML file should be attached to eDocs", 1);

					var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
					report2.ACR_ReportType = reportType;
					report2.ACR_DateFrom = ZDate.Today.AddDays(1);
					report2.ACR_DateTo = ZDate.Today.AddDays(10);
					report2.ACR_Status = AccComplianceReport.Status.ReportFinalised;
					Factory.Save();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.LastFormShownForTest = null;
					Assert(report2.SupportsSAFTXmlGeneration);
					report2.HandleGenerateSAFTXml(form);
					AssertEquals(typeof(MultistepProgressForm), ZFormModaliser.LastFormShownForTest.GetType());
					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertOneMatchingEventLog(report2.Logs, Events.DataExport, expectedEventLogMessage);
					AssertAllAttachedEDocs(report2.DocManagerInfo(), RefDocTypes.ComplianceReport);
					AssertCountOfAttachedEDocs(report2.DocManagerInfo(), "One XML file should be attached to eDocs", 1);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		public void TestHandleGenerateSAFTXml_ReportDevErrorWhenExceptionIsThrown()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				Env.Security.ExportComplianceReport.IsAllowed = true;

				var creator = new TestObjectCreator(Factory);
				creator.CreateTestPeriods(ZDateTime.Today);

				creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);

				report.ACR_ReportType = AccComplianceReport.ReportTypes.SAFT;
				report.ACR_DateFrom = ZDate.Today.AddDays(1);
				report.ACR_DateTo = ZDate.Today.AddDays(10);
				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				Factory.Save();

				Assert("Precondition", report.SupportsSAFTXmlGeneration);

				var mockIComplianceReportGUIActionProvider = new Mock<IComplianceReportGUIActionProvider>();
				mockIComplianceReportGUIActionProvider.Setup(x => x.IsCountrySupportGenerateSAFT).Returns(true);
				mockIComplianceReportGUIActionProvider.Setup(x => x.GetSAFTFileCompressionInfo()).Returns((true, 1));

				var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
				mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceReportGUIActionProvider>>().Setup(x => x.Get()).Returns(mockIComplianceReportGUIActionProvider.Object);
				mockIAccountingCountryFactory.As<IInstanceProvider<IReportModeAndCreditorSelectorDefault>>().Setup(x => x.Get()).Returns(new NorwayReportModeAndCreditorSelectorDefault());
				mockIAccountingCountryFactory.As<IInstanceProvider<IReportSAFTWriter>>().Setup(x => x.Get()).Returns(new NorwayReportSAFTWriterProvider());

				var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
				mockIGlobalAccountingCountryFactory.SetupSequence(x => x.GetCountryFactory(It.IsAny<ZString>()))
					.Returns(mockIAccountingCountryFactory.Object)
					.Returns(mockIAccountingCountryFactory.Object)
					.Returns(mockIAccountingCountryFactory.Object)
					.Returns(mockIAccountingCountryFactory.Object)
					.Throws(new InvalidOperationException());

				ErrorReporter.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
				{
					//I need to throw an exception in the try block of HandleGenerateSAFTXml
					//If you add another call of GetCountryFactory before the try block you will have to add another ".Returns(mockIAccountingCountryFactory.Object)" in the mockIGlobalAccountingCountryFactory.SetupSequence above
					AssertNoExceptionThrown(() => report.HandleGenerateSAFTXml(form));
				}

				var expectedMessage = @"There was an error during the SAFT XML generation.
Operation is not valid due to the current state of the object.";
				AssertEquals("we still show an error message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				AssertContains("There was an error during the SAFT XML generation.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestHandleGenerateSAFTXml_Norway_ValidateBeforeGenerateSAFT()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				var report = GetComplianceReportForNorway_ValidateBeforeGenerateSAFT();
				var creator = new TestObjectCreator(Factory);

				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				var config = creator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration();
				AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);
				report.HandleGenerateSAFTXml(form);
				AssertEquals(@"Please configure the 'Tax ID and Tax Message Combination Rules' registry before exporting the SAF-T file.
This registry cannot be empty.

Please create GL Mappings with Language = NB-NO and Country = NO for all active GL Accounts before exporting the SAF-T file.

The Parent GL Accounts are:
A100.00.00, A200.00.00.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2020, 07, 12)]
		public void TestHandleGenerateAnnualSAFTXml()
		{
			foreach (var reportType in new[] { AccComplianceReport.ReportTypes.SAFT, AccComplianceReport.ReportTypes.SAFTOnlyTransactions })
			{
				using (var form = new ZForm())
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
				{
					Env.Security.ExportComplianceReport.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var report = Factory.NewWithValidTestData<AccComplianceReport>();
					report.HandleGenerateSAFTXml(form);

					AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> General Ledger -> Compliance Reports -> Export Compliance Report", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					Env.Security.ExportComplianceReport.IsAllowed = true;
					Assert("Precondition", !report.SupportsSAFT);
					report.HandleGenerateSAFTXml(form);
					AssertEquals("SAFT XML can be generated only for Report Types 'SAF' and 'SAT'.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var creator = new TestObjectCreator(Factory);
					creator.CreateTestPeriods(new ZDate(2020, 7, 1));

					var reportConfig = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.CalendarMonth, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TransactionHeaderWithLines);

					report.ACR_ReportType = reportType;
					report.ACR_Periodicity = ReportPeriodicityCodes.CalendarMonth;
					report.ACR_DateFrom = new ZDate(2020, 7, 13);
					report.ACR_DateTo = new ZDate(2020, 7, 23);
					report.ACR_Status = AccComplianceReport.Status.ReportCreated;
					Factory.Save();
					Assert("Precondition", report.SupportsSAFT);
					Assert("Precondition", !report.SupportsSAFTXmlGeneration);
					report.HandleGenerateSAFTXml(form);
					AssertEquals("Please, the report must be generated or finalized before generating the SAFT XML", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
					Assert("Precondition", report.SupportsSAFTXmlGeneration);
					AssertEquals(ComplianceReportConfigurationLookups.ReportPeriodicityCodes.CalendarMonth, report.ACR_Periodicity);
					report.HandleGenerateAnnualSAFTXml(form);
					AssertEquals("The report must be based on the accounting periods to be able to generate the Annual SAFT XML", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
					report.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
					report.ACR_DateFrom = new ZDate(2020, 7, 1);
					report.ACR_DateTo = new ZDate(2020, 7, 31);
					Factory.Save();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					var exportedReports = report.HandleGenerateAnnualSAFTXml(form);
					AssertEquals(@"The Annual SAFT XML file was generated successfully.
", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(typeof(MultistepProgressForm), ZFormModaliser.LastFormShownForTest.GetType());
					const string expectedEventLogMessage = "Purpose: SAFT Annual XML Exported";
					AssertOneMatchingEventLog(report.Logs, Events.DataExport, expectedEventLogMessage);
					AssertAllAttachedEDocs(report.DocManagerInfo(), RefDocTypes.ComplianceReport);
					AssertCountOfAttachedEDocs(report.DocManagerInfo(), "One XML file should be attached to eDocs", 1);
					AssertContainsExactElementsInAnyOrder(new ZGuid[] { report.PK }, exportedReports);

					report.ACR_Status = AccComplianceReport.Status.ReportFinalised;
					Factory.Save();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					exportedReports = report.HandleGenerateAnnualSAFTXml(form);
					AssertEquals(@"The Annual SAFT XML file was generated successfully.
", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals(typeof(MultistepProgressForm), ZFormModaliser.LastFormShownForTest.GetType());
					AssertMatchingEventLogCount(report.Logs, Events.DataExport, expectedEventLogMessage, 2);
					AssertAllAttachedEDocs(report.DocManagerInfo(), RefDocTypes.ComplianceReport);
					AssertCountOfAttachedEDocs(report.DocManagerInfo(), "Two XML files should be attached to eDocs", 2);
					AssertContainsExactElementsInAnyOrder(new ZGuid[] { report.PK }, exportedReports);
				}
			}
		}

		[TestDate(2020, 07, 12)]
		public void TestHandleGenerateAnnualSAFTXml_Norway_ValidateBeforeGenerateSAFT()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				var report = GetComplianceReportForNorway_ValidateBeforeGenerateSAFT();
				var creator = new TestObjectCreator(Factory);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var config = creator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration();
				AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);
				report.HandleGenerateAnnualSAFTXml(form);
				AssertEquals(@"Please configure the 'Tax ID and Tax Message Combination Rules' registry before exporting the SAF-T file.
This registry cannot be empty.

Please create GL Mappings with Language = NB-NO and Country = NO for all active GL Accounts before exporting the SAF-T file.

The Parent GL Accounts are:
A100.00.00, A200.00.00.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestHandleGenerateAnnualSAFTXml_CompressWhenExceedMaxSize_NoCompress_NoWarning()
			=> AssertHandleGenerateAnnualSAFTXml_CompressWhenExceedMaxSize(
				allowedMaxSize: 200 * 1024 * 1024,
				expectedMessage: @"The Annual SAFT XML file was generated successfully.
",
				isAfterCompressed: false,
				countOfAttachedEdocs: 1,
				useMock: false);

		[TestDate(2020, 01, 01)]
		public void TestHandleGenerateAnnualSAFTXml_CompressWhenExceedMaxSize_Compressed_Warning()
			=> AssertHandleGenerateAnnualSAFTXml_CompressWhenExceedMaxSize(
				allowedMaxSize: 1,
				expectedMessage: @"The Annual SAFT XML file was generated successfully.
As the XML exceeded the maximum file size accepted by the Tax Authority, the XML is compressed into a ZIP file.
The exported file exceed the maximum file size accepted by the Tax Authority, please raise an eRequest for assistance.
",
				isAfterCompressed: true,
				countOfAttachedEdocs: 7,
				useMock: true);

		[TestDate(2020, 01, 01)]
		public void TestHandleGenerateAnnualSAFTXml_CompressWhenExceedMaxSize_Compressed_NoWarning()
			=> AssertHandleGenerateAnnualSAFTXml_CompressWhenExceedMaxSize(
				allowedMaxSize: 1800,
				expectedMessage: @"The Annual SAFT XML file was generated successfully.
As the XML exceeded the maximum file size accepted by the Tax Authority, the XML is compressed into a ZIP file.
",
				isAfterCompressed: true,
				countOfAttachedEdocs: 1,
				useMock: true);

		[TestDate(2020, 01, 01)]
		public void TestHandleGenerateAnnualSAFTXml_CompressWhenExceedMaxSize_NoCompress_NoWarningReTest()
			=> AssertHandleGenerateAnnualSAFTXml_CompressWhenExceedMaxSize(
				allowedMaxSize: 2000,
				expectedMessage: @"The Annual SAFT XML file was generated successfully.
",
				isAfterCompressed: false,
				countOfAttachedEdocs: 1,
				useMock: true);

		void AssertHandleGenerateAnnualSAFTXml_CompressWhenExceedMaxSize(
			int allowedMaxSize,
			string expectedMessage,
			bool isAfterCompressed,
			int countOfAttachedEdocs,
			bool useMock)
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Norway))
			{
				Env.Security.ExportComplianceReport.IsAllowed = true;
				var creator = new TestObjectCreator(Factory);
				creator.CreateTestPeriods(ZDateTime.Today.AddYears(-1));

				var taxRateCollection = new AccTaxRateCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var validTaxRate = taxRateCollection.AddNew();
				validTaxRate.AT_Code = "TGST";
				validTaxRate.AT_Type = "RAT";
				validTaxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxRate.SetRateNumerator_ForTestOnly(10);
				validTaxRate.AT_ExtraTaxRateType = "QST";
				validTaxRate.SetExtraRate_ForTestOnly(4, 2);

				var taxMessageCollection = new AccInvMsgCollection(Factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var validTaxMessage = taxMessageCollection.AddNew();
				validTaxMessage.A9_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				validTaxMessage.A9_TaxGroupCode = "N1";

				Factory.Save();

				var taxMessageGroupsManagement = new CodeDescriptionBoolRelatedItemCollection
				{
					{ "N1", (NoResString)"Description N1", true, "N1.0" },
				};
				AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, taxMessageGroupsManagement);

				Factory.Save();

				var config = creator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Revenue, validTaxRate, validTaxMessage));
				AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);
				var reportConfig = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, OrgCusCode.NorwayCodeTypes.MVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);
				var reports = new List<AccComplianceReport>();

				for (var index = 0; index < 12; index++)
				{
					var report = Factory.NewWithValidTestData<AccComplianceReport>();
					report.ACR_ReportType = AccComplianceReport.ReportTypes.SAFT;
					report.ACR_DateFrom = ZDate.Today.AddYears(-1).AddMonths(index);
					report.ACR_DateTo = ZDate.Today.AddYears(-1).AddMonths(index + 1).AddDays(-1);
					report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

					reports.Add(report);
				}

				Factory.Save();
				var lastReport = reports.LastOrDefault();

				if (!useMock)
				{
					DoAssert();
				}
				else
				{
					var mockIComplianceReportGUIActionProvider = new Mock<IComplianceReportGUIActionProvider>();
					mockIComplianceReportGUIActionProvider.Setup(x => x.IsCountrySupportGenerateSAFT).Returns(true);
					mockIComplianceReportGUIActionProvider.Setup(x => x.GetSAFTFileCompressionInfo()).Returns((true, allowedMaxSize));

					var mockIAccountingCountryFactory = new Mock<IAccountingCountryFactory>();
					mockIAccountingCountryFactory.As<IInstanceProvider<IComplianceReportGUIActionProvider>>().Setup(x => x.Get()).Returns(mockIComplianceReportGUIActionProvider.Object);
					mockIAccountingCountryFactory.As<IInstanceProvider<IReportModeAndCreditorSelectorDefault>>().Setup(x => x.Get()).Returns(new NorwayReportModeAndCreditorSelectorDefault());
					mockIAccountingCountryFactory.As<IInstanceProvider<IReportSAFTWriter>>().Setup(x => x.Get()).Returns(new NorwayReportSAFTWriterProvider());

					var mockIGlobalAccountingCountryFactory = new Mock<IGlobalAccountingCountryFactory>();
					mockIGlobalAccountingCountryFactory.Setup(x => x.GetCountryFactory(It.IsAny<ZString>())).Returns(mockIAccountingCountryFactory.Object);

					using (ObjectFactory.Substitute(mockIGlobalAccountingCountryFactory.Object))
					{
						DoAssert();
					}
				}

				void DoAssert()
				{
					lastReport.HandleGenerateAnnualSAFTXml(form);

					AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertCountOfAttachedEDocs(lastReport.DocManagerInfo(), "XML file should be attached to eDocs", countOfAttachedEdocs);

					foreach (IeDoc file in lastReport.DocManagerInfo().Files)
					{
						var fileName = file.FileName;
						var fileLength = new FileInfo(fileName).Length;

						if (isAfterCompressed)
						{
							Assert("XML file is compressed to zip file.", fileName.Contains(".zip"));
						}
						else
						{
							Assert("XML file is not compressed", !fileName.Contains(".zip"));
						}
					}
				}
			}
		}

		AccComplianceReport GetComplianceReportForNorway_ValidateBeforeGenerateSAFT()
		{
			Env.Security.ExportComplianceReport.IsAllowed = true;
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			var creator = new TestObjectCreator(Factory);
			creator.CreateTestPeriods(new ZDate(2020, 7, 1));

			var reportConfig = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, OrgCusCode.CodeTypes.AccountsPayableSuppliersReference, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);

			report.ACR_ReportType = AccComplianceReport.ReportTypes.SAFT;
			report.ACR_Periodicity = ReportPeriodicityCodes.AccountingPeriod;
			report.ACR_DateFrom = new ZDate(2020, 7, 1);
			report.ACR_DateTo = new ZDate(2020, 7, 31);
			report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			Factory.Save();

			var debitGLAccount = creator.GLHeader1;
			debitGLAccount.AG_Description = "debit GL account";
			debitGLAccount.AG_AccountNum = "A100.00.00";
			debitGLAccount.AG_DebitCredit = DebitCreditTypeList.Codes.debit;
			var creditGLAccount = creator.GLHeader2;
			creditGLAccount.AG_Description = "credit GL Account";
			creditGLAccount.AG_AccountNum = "A200.00.00";
			creditGLAccount.AG_DebitCredit = DebitCreditTypeList.Codes.credit;
			Factory.Save();

			var debitDescriptor = creator.CreateAccountDescriptor(creator.GLHeader1, "61000.95", "COA", "P&L", Core.SharedConstants.Languages.English, "", Core.Constants.CountryCodes.Norway, Core.Constants.DebitCredit.Debit);
			debitDescriptor.AJ_AG = debitGLAccount.PK;
			creator.CreateGLDescriptorPivot(debitDescriptor, creator.GLHeader1, "P&L", "D11");

			var periodCalculator = new AccountingPeriodCalculator(Factory);
			var currentPeriod = periodCalculator.GetPeriodManagementFromDate(report.ACR_DateTo, report.ACR_GC_Company);
			var previousPeriod = periodCalculator.GetPreviousPeriod(currentPeriod.AM_Period);

			var openingBalance1 = Factory.New<AccGLAggregate>();
			openingBalance1.AA_AG = debitGLAccount.PK;
			openingBalance1.AA_GE = GlbDepartment.CurrentDepartment.PK;
			openingBalance1.AA_GB = GlbBranch.CurrentBranch.PK;
			openingBalance1.AA_GC = GlbCompany.CurrentCompany.PK;
			openingBalance1.AA_Period = previousPeriod;
			openingBalance1.AA_Amount = 123m;

			var openingBalance2 = Factory.New<AccGLAggregate>();
			openingBalance2.AA_AG = creditGLAccount.PK;
			openingBalance2.AA_GE = GlbDepartment.CurrentDepartment.PK;
			openingBalance2.AA_GB = GlbBranch.CurrentBranch.PK;
			openingBalance2.AA_GC = GlbCompany.CurrentCompany.PK;
			openingBalance2.AA_Period = previousPeriod;
			openingBalance2.AA_Amount = -123m;

			Factory.Save();

			report.ClearReportLines_ForTestOnly();

			return report;
		}

		[TestDate(2020, 04, 28)]
		public void TestHandleGenerateAnnualSAFTXml_ReportIsInTheFuture()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Env.Security.ExportComplianceReport.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var firstDayOfNextMonth = new ZDate(2020, 05, 1);
				var creator = new TestObjectCreator(Factory);
				creator.CreateTestPeriods(firstDayOfNextMonth);

				var reportConfig = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);
				var report = CreateGeneratedReport(firstDayOfNextMonth);
				Factory.Save();
				Assert("Precondition", report.SupportsSAFT);
				Assert("Precondition", report.SupportsSAFTXmlGeneration);
				AssertEquals(ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod, report.ACR_Periodicity);
				Factory.Save();

				var expectedError = @"You are attempting to create a SAFT XML file from a compliance report that is in the future.
SAFT files can only be created for current or past periods.";

				report.HandleGenerateAnnualSAFTXml(form);
				AssertEquals(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2020, 12, 28)]
		public void TestHandleGenerateAnnualSAFTXml_FirstReportIsNotInJanuary()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Env.Security.ExportComplianceReport.IsAllowed = true;

				var firstDayOfPeriod = new ZDate(2020, 11, 1);
				var creator = new TestObjectCreator(Factory);
				creator.CreateTestPeriods(firstDayOfPeriod);
				var reportConfig = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);

				var report = CreateGeneratedReport(new ZDate(2020, 11, 1));
				var report2 = CreateGeneratedReport(new ZDate(2020, 12, 1));
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var exportedReports = report2.HandleGenerateAnnualSAFTXml(form);
				AssertEquals("Last Message", @"You are attempting to create a SAFT XML file that does not contain all months of the corresponding fiscal year.
This is permitted only when your Login Company started operations in CargoWise is in any month after January.
Please confirm that this is correct.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2020, 05, 15)]
		public void TestHandleGenerateAnnualSAFTXml_ReportInCurrentYearAndDoesNotIncludeTodayDate()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Env.Security.ExportComplianceReport.IsAllowed = true;

				var firstDayOfPeriod = new ZDate(2020, 03, 01);
				var creator = new TestObjectCreator(Factory);
				creator.CreateTestPeriods(firstDayOfPeriod);
				var reportConfig = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);

				var report = CreateGeneratedReport(firstDayOfPeriod);
				var report2 = CreateGeneratedReport(new ZDate(2020, 04, 01));
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var exportedReports = report2.HandleGenerateAnnualSAFTXml(form);

				var expectedError = @"You are attempting to create a SAFT XML file from a compliance report that does not contain all relevant dates of the corresponding year.
SAFT XML files must contain all transactions of a fiscal year or all transactions up to the date of creation if the file belongs to the current year.
Please generate all relevant compliance reports of the fiscal year before creating the SAFT XML file.";

				AssertEquals("selected report is in current year and does not include Today's date", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2020, 05, 12)]
		public void TestHandleGenerateAnnualSAFTXml_FullYearFromJanuary()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Env.Security.ExportComplianceReport.IsAllowed = true;

				var firstDayOfPeriod = new ZDate(2019, 01, 01);
				var creator = new TestObjectCreator(Factory);
				creator.CreateTestPeriods(firstDayOfPeriod);
				var reportConfig = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);

				var reports = new List<AccComplianceReport>();
				for (int i = 1; i <= 12; i++)
				{
					reports.Add(CreateGeneratedReport(new ZDate(2019, i, 1)));
				}
				AssertEquals(12, reports.Count);
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var exportedReports = reports.First(x => x.ACR_DateFrom.Month == 12).HandleGenerateAnnualSAFTXml(form);
				AssertEquals(@"The Annual SAFT XML file was generated successfully.
", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContainsExactElementsInAnyOrder(reports.Select(x => x.PK), exportedReports);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var expectedError = @"You are attempting to create a SAFT XML file from a compliance report that does not contain all relevant dates of the corresponding year.
SAFT XML files must contain all transactions of a fiscal year or all transactions up to the date of creation if the file belongs to the current year.
Please generate all relevant compliance reports of the fiscal year before creating the SAFT XML file.";

				reports.First(x => x.ACR_DateFrom.Month == 10).HandleGenerateAnnualSAFTXml(form);
				AssertEquals("Cannot generate Annual SAFT XML when selected report is in a past year and month is not december", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

				var septemberReport = reports.First(x => x.ACR_DateFrom.Month == 9);
				reports.Remove(septemberReport);
				septemberReport.Delete();
				Factory.Save();

				reports.First(x => x.ACR_DateFrom.Month == 12).HandleGenerateAnnualSAFTXml(form);
				AssertEquals("Cannot generate Annual SAFT XML when there is a missing report in the period", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2020, 05, 12)]
		public void TestHandleGenerateAnnualSAFTXml_FullYearFromJune()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				Env.Security.ExportComplianceReport.IsAllowed = true;

				var creator = new TestObjectCreator(Factory);
				creator.CreateTestPeriods(new ZDate(2019, 6, 1));
				var reportConfig = creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.SAFT, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.AllTransactions, ReportLineGroupingListCodes.DayBookWithoutGrouping);

				var reports = new List<AccComplianceReport>();
				for (int i = 6; i <= 12; i++)
				{
					reports.Add(CreateGeneratedReport(new ZDate(2019, i, 1)));
				}
				for (int i = 1; i <= 5; i++)
				{
					reports.Add(CreateGeneratedReport(new ZDate(2020, i, 1)));
				}
				AssertEquals(12, reports.Count);
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var exportedReports = reports.First(x => x.ACR_DateFrom.Year == 2020 && x.ACR_DateFrom.Month == 5).HandleGenerateAnnualSAFTXml(form);
				AssertEquals(@"The Annual SAFT XML file was generated successfully.
", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContainsExactElementsInAnyOrder(reports.Select(x => x.PK), exportedReports);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var expectedError = @"You are attempting to create a SAFT XML file from a compliance report that does not contain all relevant dates of the corresponding year.
SAFT XML files must contain all transactions of a fiscal year or all transactions up to the date of creation if the file belongs to the current year.
Please generate all relevant compliance reports of the fiscal year before creating the SAFT XML file.";

				reports.First(x => x.ACR_DateFrom.Year == 2020 && x.ACR_DateFrom.Month == 2).HandleGenerateAnnualSAFTXml(form);
				AssertEquals("selected report is in current year and does not include Today's date", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

				var septemberReport = reports.First(x => x.ACR_DateFrom.Year == 2019 && x.ACR_DateFrom.Month == 9);
				reports.Remove(septemberReport);
				septemberReport.Delete();
				Factory.Save();

				reports.First(x => x.ACR_DateFrom.Year == 2020 && x.ACR_DateFrom.Month == 5).HandleGenerateAnnualSAFTXml(form);
				AssertEquals("Cannot generate Annual SAFT XML when there is a missing report in the period", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		AccComplianceReport CreateGeneratedReport(ZDate dateFrom, ZDate? dateTo = null)
		{
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = AccComplianceReport.ReportTypes.SAFT;
			report.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
			report.ACR_DateFrom = dateFrom;
			report.ACR_DateTo = dateTo ?? report.ACR_DateFrom.AddMonths(1).AddDays(-1);
			report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

			return report;
		}

		public void TestHandleGenerateEsterometroXml()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				Env.Security.ExportComplianceReport.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				var filesGenerated = report.HandleGenerateEsterometroXml(form);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> General Ledger -> Compliance Reports -> Export Compliance Report", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No files should be generated when security validation fails.", 0, filesGenerated.Count());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.ExportComplianceReport.IsAllowed = true;
				Assert("Precondition: incorrect report code does not support Esterometro", !report.SupportsEsterometro);
				filesGenerated = report.HandleGenerateEsterometroXml(form);
				AssertEquals("Esterometro XML can be generated only for Report Type 'EST' and country/region 'IT'.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No files should be generated when report isn't generated.", 0, filesGenerated.Count());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var objectCreator = new TestObjectCreator(Factory);
				var firstDayOfMonth = ZDateTime.Today.AddDays(-ZDateTime.Today.Day + 1).Date;
				var creator = new TestObjectCreator(Factory);
				creator.CreateTestPeriods(firstDayOfMonth);

				creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.Esterometro, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TaxReporting);
				report.ACR_ReportType = AccComplianceReport.ReportTypes.Esterometro;
				report.ACR_Periodicity = ReportPeriodicityCodes.DateRange;
				report.ACR_DateFrom = firstDayOfMonth;
				report.ACR_DateTo = firstDayOfMonth.AddMonths(1).AddDays(-1);
				report.ACR_Status = AccComplianceReport.Status.ReportCreated;
				Factory.Save();
				Assert("Precondition: after report is correctly configured, should support Esterometro", report.SupportsEsterometro);
				Assert("Precondition: report status prevents generating XML", !report.SupportsEsterometroXmlGeneration);
				filesGenerated = report.HandleGenerateEsterometroXml(form);
				AssertEquals("Please, the report must be generated or finalized before generating the Esterometro XML", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No files should be generated when report isn't generated.", 0, filesGenerated.Count());
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				Assert("After compliance report registry is configured, and report is generated, Esterometro report XML generation should be supported.", report.SupportsEsterometroXmlGeneration);

				filesGenerated = report.HandleGenerateEsterometroXml(form, showXsdValidationErrors: false);
				AssertEquals(typeof(MultistepProgressForm), ZFormModaliser.LastFormShownForTest.GetType());
				AssertEquals("Esterometro XML file should be generated when status is Generated with zero transactions.", "The Esterometro XML file was generated successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("One file should be generated when report has zero transactions.", 1, filesGenerated.Count());
				AssertAllFilesExistAndHaveContent(filesGenerated);
				var fileSizeForZeroTransactions = SumOfFileSizes(filesGenerated);
				const string expectedEventLogMessage = "Purpose: Esterometro XML Exported";
				AssertOneMatchingEventLog(report.Logs, Events.DataExport, expectedEventLogMessage);
				AssertAllAttachedEDocs(report.DocManagerInfo(), RefDocTypes.ComplianceReport);
				AssertCountOfAttachedEDocs(report.DocManagerInfo(), "One XML file should be attached to eDocs", 1);
				DeleteFiles(filesGenerated);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.LastFormShownForTest = null;

				int transactionCount = 0;
				AddTransactionsToComplianceReport(1);
				filesGenerated = report.HandleGenerateEsterometroXml(form, showXsdValidationErrors: false);
				AssertEquals(typeof(MultistepProgressForm), ZFormModaliser.LastFormShownForTest.GetType());
				AssertEquals("Esterometro XML file should be generated when status is Generated.", "The Esterometro XML file was generated successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("One file should be generated when report has one transaction.", 1, filesGenerated.Count());
				AssertAllFilesExistAndHaveContent(filesGenerated);
				var fileSizeForOneTransaction = SumOfFileSizes(filesGenerated);
				AssertGreaterThan("File with one transaction should be larger than file with zero.", fileSizeForOneTransaction, fileSizeForZeroTransactions);
				AssertMatchingEventLogCount(report.Logs, Events.DataExport, expectedEventLogMessage, 2);
				AssertAllAttachedEDocs(report.DocManagerInfo(), RefDocTypes.ComplianceReport);
				AssertCountOfAttachedEDocs(report.DocManagerInfo(), "Two XML files should be attached to eDocs", 2);
				DeleteFiles(filesGenerated);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.LastFormShownForTest = null;

				report.ACR_Status = AccComplianceReport.Status.ReportFinalised;
				Factory.Save();
				filesGenerated = report.HandleGenerateEsterometroXml(form, showXsdValidationErrors: false);
				AssertEquals(typeof(MultistepProgressForm), ZFormModaliser.LastFormShownForTest.GetType());
				AssertEquals("Esterometro XML file should be generated when status is Finalised.", "The Esterometro XML file was generated successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("One file should be generated when report has one transaction.", 1, filesGenerated.Count());
				AssertAllFilesExistAndHaveContent(filesGenerated);
				AssertMatchingEventLogCount(report.Logs, Events.DataExport, expectedEventLogMessage, 3);
				AssertAllAttachedEDocs(report.DocManagerInfo(), RefDocTypes.ComplianceReport);
				AssertCountOfAttachedEDocs(report.DocManagerInfo(), "Three XML files should be attached to eDocs", 3);
				DeleteFiles(filesGenerated);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.LastFormShownForTest = null;

				AddTransactionsToComplianceReport(2);
				filesGenerated = report.HandleGenerateEsterometroXml(form, showXsdValidationErrors: false, writerCreator: () => new EsterometroXmlWriterWithCustomisablePageSize(report, indentXmlOutput: false, pageSize: 2));
				AssertEquals(typeof(MultistepProgressForm), ZFormModaliser.LastFormShownForTest.GetType());
				AssertEquals("Esterometro XML file should be generated when status is Finalised.", "The Esterometro XML file was generated successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Two files should be generated when report has three transaction, and page size is two.", 2, filesGenerated.Count());
				AssertAllFilesExistAndHaveContent(filesGenerated);
				var fileSizeForThreeTransactions = SumOfFileSizes(filesGenerated);
				AssertGreaterThan("Files with three transactions should be larger than file with one.", fileSizeForThreeTransactions, fileSizeForOneTransaction);
				AssertMatchingEventLogCount(report.Logs, Events.DataExport, expectedEventLogMessage, 4);
				AssertAllAttachedEDocs(report.DocManagerInfo(), RefDocTypes.ComplianceReport);
				AssertCountOfAttachedEDocs(report.DocManagerInfo(), "Five XML files should be attached to eDocs when page size is two", 5);
				DeleteFiles(filesGenerated);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.LastFormShownForTest = null;

				using (var otherMutex = new DataTransfer.ComplianceReport.Esterometro.EsterometroXmlWriter(report).CreateMutexForWriting())
				{
					Assert("Precondition: a mutex should be acquired", otherMutex.Lock());
					filesGenerated = report.HandleGenerateEsterometroXml(form, showXsdValidationErrors: false);
					AssertEquals("Mutex should prevent a second user from generating Esterometro XML", "Another user is generating an Esterometro XML file. Please wait for a few moments and try again.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("No files should be generated when report isn't generated.", 0, filesGenerated.Count());
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.LastFormShownForTest = null;
				}

				void AddTransactionsToComplianceReport(int numberOfInvoices)
				{
					for (int i = 0; i < numberOfInvoices; i++)
					{
						var invoiceNumber = transactionCount + i;
						var apInvoice = objectCreator.CreateInvoiceWithLine(typeof(APInvoice), $"I{invoiceNumber:0000}", objectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
						var line2 = objectCreator.CreateInvoiceLine(apInvoice, objectCreator.AUD, 1, 80m, 0m, 0m, 80m, 0m, 0m);
						objectCreator.Factory.Save();

						objectCreator.CreateComplianceReportTransactionPivot(report, apInvoice.Lines[0], 1 * (i + 1), "");
						objectCreator.CreateComplianceReportTransactionPivot(report, apInvoice.Lines[1], 2 * (i + 1), "");
					}

					transactionCount = transactionCount + numberOfInvoices;
					report.ClearReportLines_ForTestOnly();
				}

				var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
				report2.ACR_ReportType = AccComplianceReport.ReportTypes.Esterometro;
				report2.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange;
				report2.ACR_DateFrom = firstDayOfMonth.AddMonths(1);
				report2.ACR_DateTo = firstDayOfMonth.AddMonths(2).AddDays(-1);
				report2.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				Factory.Save();

				Assert(report2.SupportsEsterometroXmlGeneration);
				AssertEquals(ComplianceReportConfigurationLookups.ReportPeriodicityCodes.DateRange, report2.ACR_Periodicity);
				filesGenerated = report2.HandleGenerateEsterometroXml(form, showXsdValidationErrors: false);
				AssertEquals(typeof(MultistepProgressForm), ZFormModaliser.LastFormShownForTest.GetType());
				AssertEquals("A second AccComplianceReport should be able to generate an Esterometro XML file.", "The Esterometro XML file was generated successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("One file should be generated when report has one transaction.", 1, filesGenerated.Count());
				AssertAllFilesExistAndHaveContent(filesGenerated);
				AssertOneMatchingEventLog(report2.Logs, Events.DataExport, expectedEventLogMessage);
				AssertAllAttachedEDocs(report2.DocManagerInfo(), RefDocTypes.ComplianceReport);
				AssertCountOfAttachedEDocs(report2.DocManagerInfo(), "One XML file should be attached to eDocs", 1);
				DeleteFiles(filesGenerated);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.LastFormShownForTest = null;
				report2.HandleGenerateEsterometroXml(form, showXsdValidationErrors: true);
				AssertEquals(typeof(MultistepProgressForm), ZFormModaliser.LastFormShownForTest.GetType());
				AssertEquals("An error message should be shown when XSD validation fails of Esterometro XML file.", "The Esterometro XML file was generated, but has validation errors.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertMatchingEventLogCount(report2.Logs, Events.DataExport, expectedEventLogMessage, 2);
				AssertAllAttachedEDocs(report2.DocManagerInfo(), RefDocTypes.ComplianceReport);
				AssertCountOfAttachedEDocs(report2.DocManagerInfo(), "Two XML files should be attached to eDocs; files should be attached even if XSD validation errors.", 2);
				DeleteFiles(filesGenerated);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.LastFormShownForTest = null;
			}
		}

		#region Israel Open Format

		public void TestHandleExportOpenFormat_Security()
		{
			using (var form = new ZForm())
			using (var tempDir = new TempDirectory(Path.Combine(Temp.TempPath, Guid.NewGuid().ToString())))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfigurationRootFileExportPath.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, tempDir.DirectoryName))
			{
				form.Show();
				AccComplianceReportGuiHelper.IgnoreLongPathOnlyForTest = true;

				var report = GetOpenFormatReport();

				Env.Security.ExportComplianceReport.IsAllowed = false;
				AssertOpenFormatFileExport(report, form, false);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> General Ledger -> Compliance Reports -> Export Compliance Report", UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.ExportComplianceReport.IsAllowed = true;
				AssertOpenFormatFileExport(report, form, true);
			}
		}

		public void TestHandleExportOpenFormat_ReportStatus()
		{
			using (var form = new ZForm())
			using (var tempDir = new TempDirectory(Path.Combine(Temp.TempPath, Guid.NewGuid().ToString())))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfigurationRootFileExportPath.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, tempDir.DirectoryName))
			{
				form.Show();
				AccComplianceReportGuiHelper.IgnoreLongPathOnlyForTest = true;

				var report = GetOpenFormatReport();

				report.ACR_Status = AccComplianceReport.Status.ReportCreated;
				Assert(!report.SupportsExportOpenFormatFile);
				AssertOpenFormatFileExport(report, form, false);
				AssertEquals("Please, the report must be generated or finalized before export.", UnitTestUserNotification.Instance.LastMessage.Text);

				report.ACR_Status = AccComplianceReport.Status.ReportFinalised;
				Assert(report.SupportsExportOpenFormatFile);
				AssertOpenFormatFileExport(report, form, true);
			}
		}

		[TestDate(2025, 2, 18, 10, 15, 59)]
		public void TestHandleExportOpenFormat_ShowFilesSuccessfullyExportedMessage()
		{
			using (var form = new ZForm())
			using (var tempDir = new TempDirectory(Path.Combine(Temp.TempPath, Guid.NewGuid().ToString())))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfigurationRootFileExportPath.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, tempDir.DirectoryName))
			{
				form.Show();
				AccComplianceReportGuiHelper.IgnoreLongPathOnlyForTest = true;

				var report = GetOpenFormatReport();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				report.HandleExportOpenFormatFile(form);

				var expectedPath = $"{tempDir.DirectoryName}\\OPENFRM\\123456789.25\\02181015";
				AssertEquals($"Your Open Format files were successfully generated and saved.\r\nYou can find them at {expectedPath}.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleExportOpenFormat_ShowPathDoesNotExistErrorMessage()
		{
			const string nonExistingPath = "Q:\\";

			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfigurationRootFileExportPath.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, nonExistingPath))
			{
				AccComplianceReportGuiHelper.IgnoreLongPathOnlyForTest = true;

				var report = GetOpenFormatReport();
				AssertOpenFormatFileExport(report, form, false);
				AssertEquals("File Operation Failed. Details: Operation failed when saving to client. The path entered in the registry is not valid. Please check the registry settings under: Accounting -> Government Compliance Invoice Document -> Israel (IL) -> Open Format - Root File Export Path (CargoWiseOne Support Only).", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleExportOpenFormat_ShowEmptyRegistryExportPathErrorMessage()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			{
				var report = GetOpenFormatReport();

				AssertOpenFormatFileExport(report, form, false);
				AssertEquals("File Operation Failed. Details: Operation failed when saving to client. Export path not specified. Please fill the registry settings under: Accounting -> Government Compliance Invoice Document -> Israel (IL) -> Open Format - Root File Export Path (CargoWiseOne Support Only).", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleExportOpenFormat_ShowPathNotWritableErrorMessage()
		{
			var notAccessiblePath = CreateNotAccessiblePath();
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfigurationRootFileExportPath.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notAccessiblePath))
			{
				AccComplianceReportGuiHelper.IgnoreLongPathOnlyForTest = true;

				var report = GetOpenFormatReport();

				AssertOpenFormatFileExport(report, form, false);
				AssertEquals("File Operation Failed. Details: Operation failed when saving to client. The path entered in the registry is not accessible. Please check the registry settings under: Accounting -> Government Compliance Invoice Document -> Israel (IL) -> Open Format - Root File Export Path (CargoWiseOne Support Only).", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			string CreateNotAccessiblePath()
			{
				var path = Path.Combine(Path.GetTempPath(), "OnlySystemFolder");
				var dirInfo = Directory.CreateDirectory(path);

				var dirSecurity = dirInfo.GetAccessControl();
				foreach (FileSystemAccessRule rule in dirSecurity.GetAccessRules(true, true, typeof(System.Security.Principal.NTAccount)))
				{
					dirSecurity.RemoveAccessRule(rule);
				}

				dirSecurity.AddAccessRule(new FileSystemAccessRule("SYSTEM", FileSystemRights.FullControl, AccessControlType.Allow));
				dirSecurity.AddAccessRule(new FileSystemAccessRule("Everyone",FileSystemRights.FullControl,AccessControlType.Deny));
				dirInfo.SetAccessControl(dirSecurity);

				return path;
			}
		}

		public void TestHandleExportOpenFormat_ShowPathTooLongErrorMessage()
		{
			var pathLongerThan50 = CreateLongPath();
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfigurationRootFileExportPath.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, pathLongerThan50))
			{
				AccComplianceReportGuiHelper.IgnoreLongPathOnlyForTest = false;

				var report = GetOpenFormatReport();

				AssertOpenFormatFileExport(report, form, false);
				var reportGenDateTimeFromEvent = report.Logs.MostRecentLogByEventTime(Events.StatusUpdated, "TYP=GEN");
				var genYear = reportGenDateTimeFromEvent.SL_EventTime.ToString("yy");
				var genFormattedDateTime = reportGenDateTimeFromEvent.SL_EventTime.ToString("MMddHHmm");

				AssertEquals($"The path {pathLongerThan50}\\OPENFRM\\123456789.{genYear}\\{genFormattedDateTime} is too long. Please check the registry: Accounting -> Government Compliance Invoice Document -> Israel (IL) -> Open Format - Root File Export Path (CargoWiseOne Support Only).", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			string CreateLongPath()
			{
				var path = Path.Combine(Path.GetTempPath(), "FolderWithVeryLongName");
				var dirInfo = Directory.CreateDirectory(path);

				return path;
			}
		}

		public void TestHandleExportOpenFormat_ShowReportDataErrorMessage()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfigurationRootFileExportPath.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Path.GetTempPath()))
			{
				var report = GetOpenFormatReport();
				report.ACR_ReferenceNumber = string.Empty;

				AssertOpenFormatFileExport(report, form, false);
				AssertEquals("Israel Open Format unique identifier must be filled with 15 digits. Current value is: .", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleExportOpenFormat_ShowInvalidCompliancePathErrorMessage()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Israel))
			using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfigurationRootFileExportPath.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Path.GetTempPath()))
			{
				var report = GetOpenFormatReport();
				var genLog = report.Logs.MostRecentLogByEventTime(Events.StatusUpdated, "TYP=GEN");
				genLog.Cancel();

				AssertOpenFormatFileExport(report, form, false);
				AssertEquals($"The path {Path.GetTempPath()}OPENFRM\\123456789. is invalid. Please check the customer's data and the registry: Accounting -> Government Compliance Invoice Document -> Israel (IL) -> Open Format - Root File Export Path (CargoWiseOne Support Only).", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHandleExportOpenFormat_CheckValidCompliancePath()
		{
			var validPath1 = "C:\\OPENFRM\\51231211.22\\01051455";
			Assert("Path is valid.", AccComplianceReportGuiHelper.IsValidCompliancePath(validPath1));

			var validPath2 = "C:\\MyFolder\\OPENFRM\\73235212.24\\12051855";
			Assert("Path is valid.", AccComplianceReportGuiHelper.IsValidCompliancePath(validPath2));

			var invalidPath1 = "C:\\MyFolder\\OPENFRM\\.24\\12051855";
			Assert("Path is not valid, VAT is empty.", !AccComplianceReportGuiHelper.IsValidCompliancePath(invalidPath1));

			var invalidPath2 = "C:\\MyFolder\\DOCS\\51231211.24\\12051855";
			Assert("Path is not valid, cannot find OPENFRM root folder.", !AccComplianceReportGuiHelper.IsValidCompliancePath(invalidPath2));

			var invalidPath3 = "C:\\MyFolder\\OPENFRM\\5A2B121Z.24\\12051855";
			Assert("Path is not valid, VAT number contains letters.", !AccComplianceReportGuiHelper.IsValidCompliancePath(invalidPath3));

			var invalidPath4 = "C:\\MyFolder\\OPENFRM\\73235212.24\\13051855";
			Assert("Path is not valid, Month 13 doesn't exist.", !AccComplianceReportGuiHelper.IsValidCompliancePath(invalidPath4));

			var invalidPath5 = "C:\\MyFolder\\OPENFRM\\73235212.24\\10321855";
			Assert("Path is not valid, Day 32 doesn't exist.", !AccComplianceReportGuiHelper.IsValidCompliancePath(invalidPath5));

			var invalidPath6 = "C:\\MyFolder\\OPENFRM\\73235212.24\\10312555";
			Assert("Path is not valid, Hour 25 doesn't exist.", !AccComplianceReportGuiHelper.IsValidCompliancePath(invalidPath6));

			var invalidPath7 = "C:\\MyFolder\\OPENFRM\\73235212.24\\10312061";
			Assert("Path is not valid, Minutes 61 doesn't exist.", !AccComplianceReportGuiHelper.IsValidCompliancePath(invalidPath7));
		}

		public AccComplianceReport GetOpenFormatReport()
		{
			var creator = new TestObjectCreator(Factory);
			creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.OpenFormatOnlyTransactions, OrgCusCode.CodeTypes.VATCode, ReportPeriodicityCodes.DateRange, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.NoGrouping);
			var report = creator.CreateComplianceReport(AccComplianceReport.ReportTypes.OpenFormatOnlyTransactions, AccComplianceReport.Status.ReportGenerated);
			report.Logs.AddNew(Events.StatusUpdated, "TYP=GEN");
			creator.CreateCustomsCodes(report.Company.OrgProxy, CountryCodes.Israel, OrgCusCode.CodeTypes.VATCode, "123456789");
			creator.CreateCustomsCodes(report.Company.OrgProxy, CountryCodes.Israel, OrgCusCode.CodeTypes.TaxFileCode, "987654321");

			Factory.Save();
			Assert(report.SupportsExportOpenFormatFile);
			Assert(!report.ACR_ReferenceNumber.IsEmpty);

			return report;
		}

		void AssertOpenFormatFileExport(AccComplianceReport report, ZForm form, bool expectExport)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			report.HandleExportOpenFormatFile(form);

			if (expectExport)
			{
				AssertContains("Your Open Format files were successfully generated and saved.\r\nYou can find them at ", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertOneMatchingEventLog(report.Logs, Events.DataExport, "Purpose: Open format files exported");
				AssertAllAttachedEDocs(report.DocManagerInfo(), RefDocTypes.ComplianceReport);
				AssertCountOfAttachedEDocs(report.DocManagerInfo(), "Three files are exported in eDocs", 3);
				AssertStoredOpenFormatFilesInRootExportPath(report, "BKMVDATA.txt");
				AssertStoredOpenFormatFilesInRootExportPath(report, "INI.txt");
				report.DocManagerInfo().Files.ContainsDocType("REPORT.pdf");
			}
			else
			{
				AssertCountOfAttachedEDocs(report.DocManagerInfo(), "No file exported in eDocs", 0);
			}

			void AssertStoredOpenFormatFilesInRootExportPath(AccComplianceReport report, string fileName)
			{
				var path = AccountingMasterFilesRegistry.Instance.ComplianceReportConfigurationRootFileExportPath.Value;
				var reportGenDateTimeFromEvent = report.Logs.MostRecentLogByEventTime(Events.StatusUpdated, "TYP=GEN");
				var genYear = reportGenDateTimeFromEvent.SL_EventTime.ToString("yy");
				var genFormattedDateTime = reportGenDateTimeFromEvent.SL_EventTime.ToString("MMddHHmm");
				var expectedPath = Path.Combine(path, $"OPENFRM\\123456789.{genYear}\\{genFormattedDateTime}");
				var expectedFilePath = Path.Combine(expectedPath, fileName);
				Assert($"{fileName} file is correctly created in {expectedPath}", File.Exists(expectedFilePath));
			}
		}

		#endregion

		public void TestHandleMTDViewAndSubmit()
		{
			using (var form = new ZForm())
			{
				Env.Security.FinalizeComplianceReport.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				Factory.Save();

				report.HandleMTDViewAndSubmit();
				Assert(!report.ACR_IsFinalised);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> General Ledger -> Compliance Reports -> Finalize", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Env.Security.FinalizeComplianceReport.IsAllowed = true;
				report.HandleMTDViewAndSubmit();
				AssertEquals("This Report isn't finalized and MTD data can not be submitted.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				report.Finalise();
				Assert(!report.SupportsMTD);
				report.HandleMTDViewAndSubmit();
				AssertEquals("This Report type does not support MTD.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
				{
					var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
					var reportConfig = complianceConfig.AddNew();
					reportConfig.ReportCode = "MTD";
					reportConfig.ReportTitle = "Test Tax Report";
					reportConfig.ReportPeriodicity = ReportPeriodicityCodes.AccountingPeriod;
					reportConfig.Country = Env.CurrentCompany.Country.Code;
					reportConfig.TaxRegistrationType = "VAT";
					reportConfig.ReportLineGrouping = ReportLineGroupingListCodes.DayBookWithoutGrouping;
					reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.AllTransactions;

					report.ACR_ReportType = "MTD";
					report.ACR_DateFrom = ZDate.Today.AddDays(1);
					report.ACR_DateTo = ZDate.Today.AddDays(10);
					report.ACR_Status = AccComplianceReport.Status.ReportCreated;
					Factory.Save();

					Assert(!report.SupportsMTD);
					report.HandleMTDViewAndSubmit();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					report.GenerateFromQueue();
					Assert(report.SupportsMTD);
					report.HandleMTDViewAndSubmit();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					report.Finalise();
					Assert(report.SupportsMTD);
					report.HandleMTDViewAndSubmit();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		public void TestHandleMTDViewAndSubmit_GroupMemberSubmission()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
				testObjectCreator.CreateTestPeriods(ZDateTime.Today);
				MTDSubmissionDataTestEnvironmentCreator.CreateCustomsCode(testObjectCreator);

				var groupCompany = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(testObjectCreator, "UKG", "UKG");
				var memberCompany = MTDSubmissionDataTestEnvironmentCreator.CreateUKCompany(testObjectCreator, "UK1", "UK1");

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, groupCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					AssertEquals("Precondition: Current Company should be UKG", "UKG", Env.CurrentCompany.Code);
					var groupReport = testObjectCreator.CreateComplianceReport("MTD", AccComplianceReport.Status.ReportFinalised);
					groupReport.ACR_DateFrom = ZDate.Today;
					groupReport.ACR_DateTo = ZDate.Today.AddDays(10);
					Factory.Save();
				}

				using (AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.SetTemporaryValue(memberCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, groupCompany.PK.ToGuid()))
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, memberCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var report = testObjectCreator.CreateComplianceReport("MTD", AccComplianceReport.Status.ReportFinalised);
					AssertEquals("Precondition: Current Company should be UK1", "UK1", report.Company.GC_Code);
					report.ACR_DateFrom = ZDate.Today;
					report.ACR_DateTo = ZDate.Today.AddDays(10);
					Factory.Save();

					Assert(report.IsInDatabase);
					Assert(report.SupportsMTD);
					report.OAuthClientAuthorisation += Report_OAuthClientAuthorisation;
					report.HandleMTDViewAndSubmit();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}

				void Report_OAuthClientAuthorisation(object sender, AccComplianceReport.OAuthClientAuthorisationEventArgs e)
				{
					Assert("HMRC API call(CheckMTDReportDateRange) is not required for Group Member.", false);
				}
			}
		}

		public void TestHandleExportPurchaseAndSalesVATDataFile()
		{
			AssertExportVATFile((AccComplianceReport report, ZForm form) => report.HandleExportPurchaseAndSalesVATDataFile(form));
		}

		public void TestHandleExportZeroRatedSalesVATDataFile()
		{
			AssertExportVATFile((AccComplianceReport report, ZForm form) => report.HandleExportZeroRatedSalesVATDataFile(form));
		}

		void AssertExportVATFile(Action<AccComplianceReport, ZForm> exportAction)
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				Env.Security.ExportComplianceReport.IsAllowed = false;
				Factory.Save();

				exportAction(report, form);

				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> General Ledger -> Compliance Reports -> Export Compliance Report", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.ExportComplianceReport.IsAllowed = true;
				Factory.Save();

				Assert(!report.SupportsExportVATFile);
				exportAction(report, form);
				AssertEquals("Please, the report must be generated or finalized before exporting", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				new TestObjectCreator(Factory).CreateTestPeriods(ZDateTime.Today);

				var cusCode1 = GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.AddNew();
				cusCode1.OK_CodeType = "VAT";
				cusCode1.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
				cusCode1.OK_CustomsRegNo = "12345675";

				var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
				var reportConfig = complianceConfig.AddNew();
				reportConfig.ReportCode = "TST";
				reportConfig.ReportTitle = "Test Tax Report";
				reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
				reportConfig.Country = Env.CurrentCompany.Country.Code;
				reportConfig.TaxRegistrationType = "APC";
				reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.ComplianceDocumentHeader;
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

				report.ACR_ReportType = "TST";
				report.ACR_DateFrom = ZDate.Today.AddDays(1);
				report.ACR_DateTo = ZDate.Today.AddDays(10);
				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				Factory.Save();

				Assert(report.SupportsExportVATFile);
				exportAction(report, form);
				AssertEquals(typeof(ProgressForm), ZFormModaliser.LastFormShownForTest.GetType());
				AssertEquals("The VAT file was exported successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestHandleImportABNs()
		{
			var creator = new TestObjectCreator(Factory);
			var report = creator.CreateComplianceReport(ComplianceReportTypes.PaymentTimesSmallBusinessReportType, AccComplianceReport.Status.ReportCreated);
			AssertEquals(false, report.IsInDatabase);
			report.HandleImportABNs();
			AssertEquals("user need save report first", "This report does not support ABN import.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				CombineAssertions("if user did not generate report, user can import ABNs", () =>
				{
					var allowStatus = new[] {
						AccComplianceReport.Status.ReportCreated,
						AccComplianceReport.Status.ReportDataQueued,
						AccComplianceReport.Status.ReportPendingQueueing,
					};
					foreach (var status in allowStatus)
					{
						report.ACR_Status = status;
						Assert(report.SupportsImportABNs);
						report.HandleImportABNs();
						AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertType<ImportABNsForm>(ZFormModaliser.LastFormShownDialogForTest);
						ZFormModaliser.LastFormShownDialogForTest.Close();
					}
				});

				CombineAssertions("if user has generated report, user can not import ABNs", () =>
				{
					var denyStatus = new[] {
						AccComplianceReport.Status.ReportFinalised,
						AccComplianceReport.Status.ReportGenerated,
						AccComplianceReport.Status.ReportInvalidated,
						AccComplianceReport.Status.ReportError
					};
					foreach (var status in denyStatus)
					{
						report.ACR_Status = status;
						Assert(!report.SupportsImportABNs);
						report.HandleImportABNs();
						AssertEquals("This report does not support ABN import.", UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}
				});
			}
		}

		[TestDate(2022, 04, 01)]
		public void TestHandleLiquidazioneIVAViewAndSubmit()
		{
			using (var form = new ZForm())
			{
				var creator = new TestObjectCreator(Factory);

				Factory.Load<AccPeriodManagement>(new ZQuery()).DeleteAll();
				Factory.Save();
				AssertEquals("AccPeriodManagement must contain 0 rows.", 0, Factory.Load(typeof(AccPeriodManagement), new ZQuery()).Length);

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
				{
					var report = creator.CreateComplianceReport(AccComplianceReport.ReportTypes.LiquidazioneIVA, AccComplianceReport.Status.ReportGenerated, ReportPeriodicityCodes.AccountingPeriod);
					Factory.Save();
					Assert(!report.SupportsLiquidazioneIVA);
					AssertEquals("AccPeriodManagement must contain 12 rows.", 12, Factory.Load(typeof(AccPeriodManagement), new ZQuery()).Length);
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
				{
					Factory.Load<AccPeriodManagement>(new ZQuery()).DeleteAll();
					Factory.Save();
					AssertEquals("AccPeriodManagement must contain 0 rows.", 0, Factory.Load(typeof(AccPeriodManagement), new ZQuery()).Length);

					creator.EnsureComplianceReportConfigInRegistry(AccComplianceReport.ReportTypes.LiquidazioneIVA, OrgCusCode.CodeTypes.IVA, ReportPeriodicityCodes.AccountingPeriod, ReportBaseTablePrefixListCodes.TransactionLine, ReportLineGroupingListCodes.TaxReporting);
					var report = creator.CreateComplianceReport(AccComplianceReport.ReportTypes.LiquidazioneIVA, AccComplianceReport.Status.ReportCreated, ReportPeriodicityCodes.AccountingPeriod);
					Factory.Save();

					Assert(!report.SupportsLiquidazioneIVA);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					report.HandleLiquidazioneIVAViewAndSubmit();
					AssertEquals("This Report type does not support VAT Summary.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					report.GenerateFromQueue();
					Assert(report.SupportsLiquidazioneIVA);
					report.HandleLiquidazioneIVAViewAndSubmit();
					AssertEquals("No Liquidazione VAT Summary Report found for previous period. If this is your first Liquidazione IVA, you may enter a manual Adjustment. Otherwise, please ensure a 'LIQ' Compliance Report with VAT Summary exists in Generated or Finalized status.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					report.Finalise();
					Assert(!report.SupportsLiquidazioneIVA);
					report.HandleLiquidazioneIVAViewAndSubmit();
					AssertEquals("This Report type does not support VAT Summary.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestHandleZMDViewAndSubmit()
		{
			AccountingMasterFilesRegistryTest.EnableAllGermanComplianceReports();
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = "ZMD";
			report.ACR_DateFrom = ZDate.Today.AddDays(1);
			report.ACR_DateTo = ZDate.Today.AddDays(10);
			report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Germany))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				report.HandleZMDViewAndSubmit();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2020, 01, 12, 10, 25, 55)]
		public void TestMonthlySAFTXmlFileName_Norway()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Norway))
			{
				SetupForNorwaySAFTXmlFileName();

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = AccComplianceReport.ReportTypes.SAFT;
				report.ACR_DateFrom = new ZDate(2020, 1, 1);
				report.ACR_DateTo = new ZDate(2020, 1, 31);
				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				Factory.Save();

				report.HandleGenerateSAFTXml(form);
				var expectedMessage = "The SAFT XML file was generated successfully.\r\n";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("SAF-T Financial_123456789X_20200101_20200131_20200112102555", report.DocManagerInfo().Files[0].FileName);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.RemoveAll();
				Factory.Save();

				var report2 = Factory.NewWithValidTestData<AccComplianceReport>();
				report2.ACR_ReportType = AccComplianceReport.ReportTypes.SAFT;
				report2.ACR_DateFrom = new ZDate(2020, 1, 1);
				report2.ACR_DateTo = new ZDate(2020, 1, 31);
				report2.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				Factory.Save();

				report2.HandleGenerateSAFTXml(form);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("SAF-T Financial_20200101_20200131_20200112102555", report2.DocManagerInfo().Files[0].FileName);
			}
		}

		[TestDate(2020, 02, 12, 10, 25, 55)]
		public void TestAnnualSAFTXmlFileName_Norway()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Norway))
			{
				SetupForNorwaySAFTXmlFileName();

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = AccComplianceReport.ReportTypes.SAFT;
				report.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
				report.ACR_DateFrom = new ZDate(2020, 1, 1);
				report.ACR_DateTo = new ZDate(2020, 1, 31);
				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
				report1.ACR_ReportType = AccComplianceReport.ReportTypes.SAFT;
				report1.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
				report1.ACR_DateFrom = new ZDate(2020, 2, 1);
				report1.ACR_DateTo = new ZDate(2020, 2, 29);
				report1.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				Factory.Save();

				report1.HandleGenerateAnnualSAFTXml(form);
				AssertEquals(@"The Annual SAFT XML file was generated successfully.
", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("SAF-T Financial_123456789X_20200101_20200229_20200212102555", report1.DocManagerInfo().Files[0].FileName);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		[TestDate(2020, 03, 12, 10, 25, 55)]
		public void TestAnnualSAFTXmlFileName_MissingReport_Norway()
		{
			using (var form = new ZForm())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Norway))
			{
				SetupForNorwaySAFTXmlFileName();

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_ReportType = AccComplianceReport.ReportTypes.SAFT;
				report.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
				report.ACR_DateFrom = new ZDate(2020, 2, 1);
				report.ACR_DateTo = new ZDate(2020, 2, 29);
				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;

				var report1 = Factory.NewWithValidTestData<AccComplianceReport>();
				report1.ACR_ReportType = AccComplianceReport.ReportTypes.SAFT;
				report1.ACR_Periodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
				report1.ACR_DateFrom = new ZDate(2020, 3, 1);
				report1.ACR_DateTo = new ZDate(2020, 3, 31);
				report1.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				report1.HandleGenerateAnnualSAFTXml(form);
				AssertEquals(@"The Annual SAFT XML file was generated successfully.
", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("SAF-T Financial_123456789X_20200201_20200331_20200312102555", report1.DocManagerInfo().Files[0].FileName);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestSupportsReQueue_WhenTablePrefixIsGLD()
		{
			var report = CreateComplianceReportTablePrefixGLD();

			Assert(report.SupportsReQueue);
		}

		public void TestGetComplianceReportLinesBasedOnGroupByAndTablePrefix_NoException_WhenTablePrefixIsGLD()
		{
			var report = CreateComplianceReportTablePrefixGLD();

			AssertNoExceptionThrown(() => report.CheckExeedsMaxReportLinesToLoad());
		}

		AccComplianceReport CreateComplianceReportTablePrefixGLD()
		{
			var complianceConfig = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.Value;
			var reportConfig = complianceConfig.AddNew();
			reportConfig.ReportCode = "TST";
			reportConfig.ReportTitle = "Test Tax Report";
			reportConfig.ReportPeriodicity = ComplianceReportConfigurationLookups.ReportPeriodicityCodes.AccountingPeriod;
			reportConfig.Country = Env.CurrentCompany.Country.Code;
			reportConfig.TaxRegistrationType = "ABN";
			reportConfig.ReportLineGrouping = ComplianceReportConfigurationLookups.ReportLineGroupingListCodes.DayBookWithoutGrouping;
			reportConfig.ReportBaseTablePrefix = ReportBaseTablePrefixListCodes.GeneralLedgerData;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, complianceConfig);

			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_ReportType = "TST";
			report.ACR_Status = AccComplianceReport.Status.ReportCreated;
			report.ACR_DateFrom = ZDate.Today.AddDays(1);
			report.ACR_DateTo = ZDate.Today.AddDays(10);
			Factory.Save();

			return report;
		}

		void SetupForNorwaySAFTXmlFileName()
		{
			var creator = new TestObjectCreator(Factory);
			creator.CreateTestPeriodsForEntireYear(2020);

			creator.Debtor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GovBusinessCode, "123456789x", CountryCodes.Norway);
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = creator.Debtor.PK;

			creator.TaxMsg1.A9_TaxGroupCode = "N1";
			Factory.Save();

			var taxMessageGroupsManagement = new CodeDescriptionBoolRelatedItemCollection();
			taxMessageGroupsManagement.Add("N1", (NoResString)"Description N1", true, "N1.0");
			AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, taxMessageGroupsManagement);

			var config = creator.CreateTaxIdAndTaxMessageCombinationRulesConfiguration((TransactionLineTypes.Revenue, creator.ServiceTax, creator.TaxMsg1));
			AccountingMasterFilesRegistry.Instance.TaxIdAndTaxMessageCombinationRules.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);
		}

		[TestDate(2024, 02, 01)]
		public void TestHandleGenerateSAFTXml_CollectData()
		{
			AssertGenerateFiles(CountryCodes.Portugal, nameof(AccComplianceReportUsageCollectorAction.GeneratingFiles), AccComplianceReport.ReportTypes.SAFT, 1,
				(report, form) => report.HandleGenerateSAFTXml(form));
		}

		[TestDate(2024, 02, 01)]
		public void TestHandleGenerateAnnualSAFTXml_CollectData()
		{
			AssertGenerateFiles(CountryCodes.Portugal, nameof(AccComplianceReportUsageCollectorAction.GeneratingFiles), AccComplianceReport.ReportTypes.SAFT, 1,
				(report, form) =>
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					report.HandleGenerateAnnualSAFTXml(form);
				});
		}

		[TestDate(2024, 02, 01)]
		public void TestHandleGenerateEsterometroXml_CollectData()
		{
			AssertGenerateFiles(CountryCodes.Italy, nameof(AccComplianceReportUsageCollectorAction.GeneratingFiles), AccComplianceReport.ReportTypes.Esterometro, 1,
				(report, form) => report.HandleGenerateEsterometroXml(form));
		}

		AccComplianceReport CreateComplianceReport(string reportType)
		{
			var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
			complianceReport.ACR_ReportType = reportType;
			complianceReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			complianceReport.ACR_DateFrom = new ZDate("2024-02-01");
			complianceReport.ACR_DateTo = new ZDate("2024-02-29");
			Factory.Save();

			return complianceReport;
		}

		void AssertGenerateFiles(string country, string collectAction, string reportType, int noOfFiles, Action<AccComplianceReport, Form> generateFileAction)
		{
			var creator = new TestObjectCreator(Factory);
			creator.CreateTestPeriodsForEntireYear(2024);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			using (var form = new Form())
			{
				var report = CreateComplianceReport(reportType);
				generateFileAction(report, form);

				var ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
				AssertEquals("Wrong number of rows in EDIMessage table.", 1, ediMessages.Length);

				var jObjects = ediMessages.Select(msg => JObject.Parse(msg.EM_MessageTextDetail));
				var jObject = jObjects.First();

				var acrValue = jObject.Properties().FirstOrDefault(kp => kp.Name.Equals("ACR", StringComparison.InvariantCulture))?.Value;
				AssertNotNull("ACR is missing", acrValue);
				AssertEquals(country, acrValue["CountryCode"].ToString());
				AssertEquals(collectAction, acrValue["Action"].ToString());
				AssertEquals(reportType, acrValue["ReportConfiguration"]["ReportCode"].ToString());
				if (noOfFiles > 0)
				{
					AssertEquals(noOfFiles, int.Parse(acrValue["eDocs"]["NoOfFiles"].ToString()));
				}
			}
		}
		#region Test Helpers
		void DeleteFiles(IEnumerable<string> paths)
		{
			foreach (var path in paths)
			{
				System.IO.File.Delete(path);
			}
		}

		void AssertAllFilesExistAndHaveContent(IEnumerable<string> paths)
		{
			foreach (var path in paths)
			{
				Assert("File should exist: " + path, File.Exists(path));
				AssertGreaterThan("File should have content: " + path, File.ReadAllBytes(path).Length, 0);
			}
		}

		int SumOfFileSizes(IEnumerable<string> paths) => paths.Select(p => new FileInfo(p)).Sum(fi => (int)fi.Length);

		void AssertOneMatchingEventLog(Logs logs, Event expectedEvent, string expectedReferenceText)
		{
			AssertMatchingEventLogCount(logs, expectedEvent, expectedReferenceText, 1);
		}

		void AssertMatchingEventLogCount(Logs logs, Event expectedEvent, string expectedReferenceText, int expectedCount)
		{
			var eventLogs = logs.Find(l => l.Event.SE_Code == expectedEvent.Code && l.ReferenceFreeText == expectedReferenceText);
			AssertEquals($"{expectedCount} Event Log(s) with code '{expectedEvent.Code}' and reference text '{expectedReferenceText}' should be created.", expectedCount, eventLogs.Count());
			foreach (var logAndIndex in eventLogs.Select((log, idx) => new { log, idx }))
			{
				Assert($"Event Log #{logAndIndex.idx} should be saved to database.", logAndIndex.log.IsInDatabase);
			}
		}

		void AssertAllAttachedEDocs(DocManagerInfo docManagerInfo, ZString expectedDocType)
		{
			for (int i = 0; i < docManagerInfo.Files.Count; i++)
			{
				var storageFile = (BusinessObject)docManagerInfo.Files[i];
				var docType = (RefDocType)storageFile["DocType"];
				Assert($"StorageFile #{i} should be saved to database.", storageFile.IsInDatabase);
				var docAssertionMessage = $"StorageFile #{i} RefDocType should match '{expectedDocType}'";
				if (docType.RT_DocType.IsEmpty)
				{
					docAssertionMessage += " (did you forget to do Testing > DB Upgrade > Reload DocumentsComplete.xml)";
				}
				AssertEquals(docAssertionMessage, expectedDocType, docType.RT_DocType);
				var fileAsAttachment = (Enterprise.Integration.DocumentEngine.IDeliveryEmailAttachment)storageFile;
				AssertGreaterThan($"StorageFile #{i} should be larger than zero bytes.", fileAsAttachment.FileSizeInBytes, 0);
			}
		}

		void AssertCountOfAttachedEDocs(DocManagerInfo docManagerInfo, string message, int expectedCount)
		{
			AssertEquals(message, expectedCount, docManagerInfo.Files.Count);
		}

		internal class EsterometroXmlWriterWithCustomisablePageSize : DataTransfer.ComplianceReport.Esterometro.EsterometroXmlWriter
		{
			public EsterometroXmlWriterWithCustomisablePageSize(AccComplianceReport report, bool indentXmlOutput, int pageSize)
				: base(report, indentXmlOutput)
			{
				fMaximumTransactionsPerFile = pageSize;
			}

			readonly int fMaximumTransactionsPerFile;
			protected override int MaximumTransactionsPerFile => fMaximumTransactionsPerFile;
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using BaseTablePrefixListCodes = Enterprise.Registry.Business.ComplianceReportConfigurationLookups.ReportBaseTablePrefixListCodes;
using GroupingCodes = Enterprise.Registry.Business.ComplianceReportConfigurationLookups.ReportLineGroupingListCodes;
using PeriodicityCodes = Enterprise.Registry.Business.ComplianceReportConfigurationLookups.ReportPeriodicityCodes;

namespace Enterprise.Accounting.ServiceTasks.ComplianceReport.Testing
{
	[TestedType(typeof(ComplianceReportQueuingServiceTask))]
	public class ComplianceReportQueuingServiceTaskTest : ServiceTaskTestCase<ComplianceReportQueuingServiceTask>
	{
		readonly ZDate AUComplianceReportReportingFromDate = new ZDate(2024, 07, 01);
		readonly ZDate AUComplianceReportReportingToDate = new ZDate(2024, 12, 31);

		[TestDate(2015, 10, 26)]
		public void TestComplianceReportQueuingServiceTask_SuccessfulRun()
		{
			var report = PrepareTestData();

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			report.Reload();

			AssertEquals("Queue records created", 10, GetNoOfQueueRows());
			AssertEquals("Report status set to Queued", AccComplianceReport.Status.ReportDataQueued, report.ACR_Status);
			AssertEquals("Report status Message set to Queued", "Report data has been queued. Please click Generate to proceed.", report.ACR_StatusMessage);

			var log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process starting.", log);
			AssertContains("Debug|Started queue building for company: 'EDI', report type 'LIB', dates from 01-Oct-15 to 31-Oct-15.", log);
			AssertContains("Debug|Completed queue building for company: 'EDI', report type 'LIB', dates from 01-Oct-15 to 31-Oct-15.", log);
			AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);

			// This test calls ComplianceReportQueuingServiceTask.QueueForCompany() which starts the usage collector.
			// Test usage data written to DATABASE.
			var ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			AssertEquals("Expected one usage record for report status changes: ADD -> QUE.", 1, ediMessages.Length);
		}

		[TestDate(2019, 12, 31, 16, 27, 15)]
		public void TestComplianceReportQueuingServiceTask_SuccessfulRun_TestUsageCollector()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				var complianceReport = PrepareTestDataFEC();

				var stopWatchMock = new Mock<IStopwatch>();
				stopWatchMock.Setup(x => x.ElapsedMilliseconds).Returns(1234);
				using (ObjectFactory.Substitute(stopWatchMock.Object))
				{
					var serviceTask = new ComplianceReportQueuingServiceTask(() => Guid.Parse("eb73e135-7e95-4c3d-a6b4-d3e736d3a4ee"));
					var logger = InitialiseAndRunTaskSchedule(serviceTask);
					complianceReport.Reload();

					// test usage data written to DATABASE
					var ediMessages = Factory.Load<IUsageEDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
					AssertEquals("Expected 6 usage records for generating the report: ADD (1x) -> QUE (3x) -> GEN (2x).", 6, ediMessages.Length);

					foreach (var message in ediMessages)
					{
						AssertEquals("Incorrect Branch Code", "BNE", message.Branch.Code);
					}

					var jObjects = ediMessages.Select(msg => JObject.Parse(msg.EM_MessageTextDetail));
					var stringBuilder = new StringBuilder();
					foreach (var jObject in jObjects)
					{
						var x = (ZString)JsonConvert.SerializeObject(jObject, Newtonsoft.Json.Formatting.Indented);
						x = x.Replace("\r", string.Empty).Replace("\n", string.Empty).TrimEnd(new[] { '\r', '\n' });
						stringBuilder.AppendLine(x);
					}

					var expectedAsString = GetEmbeddedResourceAsZString("ComplianceReportQueuingServiceTaskUsageCollector.txt");
					AssertMultilineASCIIEquals("Wrong values in EDIMessage table.", expectedAsString, stringBuilder.ToString());
				}
			}
		}

		[TestDate(2015, 10, 26)]
		public void TestIterationsCountWithReportsUnderTimeLimit()
		{
			PrepareTestData();

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			var log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);

			using (Env.Instance.TemporaryServiceTaskContext(ComplianceReportQueuingServiceTask.Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}
		}

		[TestDate(2015, 10, 26)]
		public void TestIterationsCountWithoutReportsUnderTimeLimit()
		{
			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			var log = logger.ToString();
			AssertContains("Compliance report transaction queuing process iteration 1 started.", log);
			AssertContains("Information|Compliance report transaction queuing process completed after 1 iterations.", log);
		}

		[TestDate(2015, 10, 26)]
		public void TestIterationsCountWithReportsOverTimeLimit()
		{
			PrepareTestData();

			TestServiceLogger logger;

			using (AccountingMasterFilesRegistry.Instance.MaximumTimeForCRQServiceTaskToRun.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var serviceTask = new ComplianceReportQueuingServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);
			}

			var log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process ended after 1 iterations as it reached timeout: 00:00:00.", log);
		}

		[TestDate(2015, 10, 26)]
		public void TestComplianceReportQueuingServiceTask_SuccessfulRun_CompanyWithoutActiveBranch()
		{
			var report = PrepareTestData();

			GlbCompany.CurrentCompany.Branches.ForEach(x => x.GB_IsActive = false);
			GlbCompany.CurrentCompany.Factory.Save();

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			report.Reload();

			AssertEquals("Queue records created", 10, GetNoOfQueueRows());
			AssertEquals("Report status set to Queued", AccComplianceReport.Status.ReportDataQueued, report.ACR_Status);

			var log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process starting.", log);
			AssertContains("Debug|Started queue building for company: 'EDI', report type 'LIB', dates from 01-Oct-15 to 31-Oct-15.", log);
			AssertContains("Debug|Completed queue building for company: 'EDI', report type 'LIB', dates from 01-Oct-15 to 31-Oct-15.", log);
			AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);
		}

		AccComplianceReport PrepareTestData()
		{
			Creator.CreateReportConfigurationWithQueuing();
			var report = CreateReport();
			CreateTransactions();
			Factory.Save();

			AssertEquals("Precondition: no Queue records", 0, GetNoOfQueueRows());

			return report;
		}

		AccComplianceReport PrepareTestDataFEC()
		{
			Creator.CreateReportConfigurationWithQueuing(null, BaseTablePrefixListCodes.AllTransactions, null, "FEC", PeriodicityCodes.RangeAccountingPeriod);
			var report = CreateReport("FEC", PeriodicityCodes.RangeAccountingPeriod, new ZDate(2020, 1, 1), new ZDate(2020, 3, 31));
			CreateTransactions();
			var retainedEarningsAccount = Creator.CreateRetainedEarningsAccount();
			Creator.CreateLocalAccountMappingForGLHeader(retainedEarningsAccount, Constants.Languages.French, Constants.CountryCodes.France);
			Creator.CreateLocalAccountMappingForGLHeader(Creator.GLHeader1, Constants.Languages.French, Constants.CountryCodes.France);

			Factory.Save();

			AssertEquals("Precondition: no Queue records", 0, GetNoOfQueueRows());

			return report;
		}

		[TestDate(2015, 10, 26)]
		public void TestComplianceReportQueuingServiceTask_WithOpeningClosingJournalRun()
		{
			Creator.CreateReportConfigurationWithQueuing(GroupingCodes.DayBook);
			var report = CreateReport();

			CreateTransactions();
			CreateAndSaveOpeningAndClosingGLJournal();
			Factory.Save();

			AssertEquals("Precondition: no Queue records", 0, GetNoOfQueueRows());

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			report.Reload();

			AssertEquals("Queue records created", 14, GetNoOfQueueRows());
			AssertEquals("Queue records created", 4, GetNoOfQueueRows("Where ACQ_ReportSubCode = '*GL*GJL**'"));
			AssertEquals("Report status set to Queued", AccComplianceReport.Status.ReportDataQueued, report.ACR_Status);

			var log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process starting.", log);
			AssertContains("Debug|Started queue building for company: 'EDI', report type 'LIB', dates from 01-Oct-15 to 31-Oct-15.", log);
			AssertContains("Debug|Completed queue building for company: 'EDI', report type 'LIB', dates from 01-Oct-15 to 31-Oct-15.", log);
			AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);
		}

		[TestDate(2015, 10, 26)]
		public void TestComplianceReportQueuingServiceTask_NoTransactions()
		{
			Creator.CreateReportConfigurationWithQueuing();
			AssertEquals("Precondition: no Queue records", 0, GetNoOfQueueRows());

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertEquals("No Queue records", 0, GetNoOfQueueRows());

			var log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process starting.", log);
			AssertContains("Information|Compliance report transaction queuing process completed after 1 iterations.", log);

			var report = CreateReport();
			Factory.Save();

			serviceTask = new ComplianceReportQueuingServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);
			report.Reload();

			AssertEquals("Still no Queue records as there are no Transactions", 0, GetNoOfQueueRows());
			AssertEquals("Report status set to Queued as Queuing was ran", AccComplianceReport.Status.ReportDataQueued, report.ACR_Status);

			log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process starting.", log);
			AssertContains("Debug|Started queue building for company: 'EDI', report type 'LIB', dates from 01-Oct-15 to 31-Oct-15.", log);
			AssertContains("Debug|Completed queue building for company: 'EDI', report type 'LIB', dates from 01-Oct-15 to 31-Oct-15.", log);
			AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);
		}

		public void TestHasComplianceReportConfigurations()
		{
			Creator.DefaultCompany.GC_IsActive = false;
			Creator.NonCurrentCompany.GC_IsActive = false;
			Creator.Factory.Save();
			AssertEquals("Precondition: no report configurations", "There are no active login companies with compliance report configuration.", ComplianceReportQueuingServiceTask.CheckApplicableConfigurations());

			Creator.DefaultCompany.GC_IsActive = true;
			Creator.Factory.Save();
			Creator.CreateReportConfigurationWithoutQueuing();
			AssertEquals("Report configuration without queuing but applicable for Old queue purging", string.Empty, ComplianceReportQueuingServiceTask.CheckApplicableConfigurations());

			Creator.CreateReportConfigurationWithQueuing();
			AssertEquals("An applicable for queuing Report configuration exists", string.Empty, ComplianceReportQueuingServiceTask.CheckApplicableConfigurations());
		}

		public void TestHasComplianceReportConfigurationsForGenerating()
		{
			var noConfigsMsg = "There are no active login companies with compliance report configuration.";
			var hasConfigsMsg = string.Empty;

			Creator.DefaultCompany.GC_IsActive = false;
			Creator.NonCurrentCompany.GC_IsActive = false;
			Creator.Factory.Save();
			AssertEquals("Precondition", noConfigsMsg, ComplianceReportQueuingServiceTask.CheckApplicableConfigurations());

			Creator.CreateNewCompany("TW1", Constants.CountryCodes.Taiwan, orgProxy: Creator.ZECTRA);
			AssertEquals("Taiwan has no valid default report configuration", noConfigsMsg, ComplianceReportQueuingServiceTask.CheckApplicableConfigurations());

			var registry = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration;

			var germanCompany = Creator.CreateNewCompany("DE1", Constants.CountryCodes.Germany, orgProxy: Creator.ZECTRA);
			Creator.CreateReportConfiguration(germanCompany, Constants.CountryCodes.Germany, AccComplianceReport.ReportTypes.IDEA, BaseTablePrefixListCodes.AllTransactions, PeriodicityCodes.DateRange);
			AssertEquals(hasConfigsMsg, ComplianceReportQueuingServiceTask.CheckApplicableConfigurations());
			Assert(ComplianceReportQueuingServiceTask.CheckApplicableConfigurations_ForTestOnly());

			registry.SetValue(germanCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ComplianceReportConfigurationCollection(Factory));
			AssertEquals(hasConfigsMsg, ComplianceReportQueuingServiceTask.CheckApplicableConfigurations());
			Assert(!ComplianceReportQueuingServiceTask.CheckApplicableConfigurations_ForTestOnly());

			var frenchCompany = Creator.CreateNewCompany("FR1", Constants.CountryCodes.France, orgProxy: Creator.ZECTRA);
			Creator.CreateReportConfiguration(frenchCompany, Constants.CountryCodes.France, AccComplianceReport.ReportTypes.FEC, BaseTablePrefixListCodes.AllTransactions, PeriodicityCodes.DateRange);
			AssertEquals(hasConfigsMsg, ComplianceReportQueuingServiceTask.CheckApplicableConfigurations());
			Assert(ComplianceReportQueuingServiceTask.CheckApplicableConfigurations_ForTestOnly());

			registry.SetValue(frenchCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ComplianceReportConfigurationCollection(Factory));
			AssertEquals(hasConfigsMsg, ComplianceReportQueuingServiceTask.CheckApplicableConfigurations());
			Assert(!ComplianceReportQueuingServiceTask.CheckApplicableConfigurations_ForTestOnly());

			var polishCompany = Creator.CreateNewCompany("PL1", Constants.CountryCodes.Poland, orgProxy: Creator.ZECTRA);
			Creator.CreateReportConfiguration(polishCompany, Constants.CountryCodes.Poland, AccComplianceReport.ReportTypes.JPKV7M, BaseTablePrefixListCodes.TransactionLine, PeriodicityCodes.CalendarMonth);
			AssertEquals(hasConfigsMsg, ComplianceReportQueuingServiceTask.CheckApplicableConfigurations());
			Assert(ComplianceReportQueuingServiceTask.CheckApplicableConfigurations_ForTestOnly());

			registry.SetValue(polishCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new ComplianceReportConfigurationCollection(Factory));
			AssertEquals(hasConfigsMsg, ComplianceReportQueuingServiceTask.CheckApplicableConfigurations());
			Assert(!ComplianceReportQueuingServiceTask.CheckApplicableConfigurations_ForTestOnly());
		}

		[TestDate(2020, 5, 20)]
		public void TestComplianceReportQueuingServiceTask_TransactionsWithPresentationCategoryIncluded()
		{
			var configurations = Creator.CreateReportConfigurationWithQueuing(GroupingCodes.DayBookWithPresentation, BaseTablePrefixListCodes.AllTransactions);
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);
			var report = CreateReport();

			var header1 = Creator.CreateGLHeader("TEST.30.00").PK;
			var header2 = Creator.CreateGLHeader("TEST.40.00").PK;

			var journal = Creator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2020, 05, 3), new ZDateTime(2020, 05, 3), new ZDateTime(2020, 05, 10));
			journal.AH_TransactionCategory = "PRE";
			var journalTransactions = new TransactionLine[] {
				Creator.CreateGLJournalLine(journal, 350m, DebitCredit.DR, header1),
				Creator.CreateGLJournalLine(journal, 350m, DebitCredit.CR, header2) };

			var journal2 = Creator.CreateGLJournal(TransactionTypes.GLAutoJournal, new ZDateTime(2020, 05, 3), new ZDateTime(2020, 05, 3), new ZDateTime(2020, 05, 10));
			journal2.AH_TransactionCategory = "PRE";
			journal2.AH_DueDate = new ZDateTime(2020, 05, 31, 23, 59, 59);
			var journalTransactions2 = new TransactionLine[] {
				Creator.CreateGLJournalLine(journal2, 350m, DebitCredit.DR, header1),
				Creator.CreateGLJournalLine(journal2, 350m, DebitCredit.CR, header2) };

			Creator.CreateTestPeriodsForEntireYear(2020); // calls Factory.Save(), so no need to call again.

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			report.Reload();

			AssertEquals("Queue records created", 4, GetNoOfQueueRows());
			AssertEquals("Queue record created for journal line", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{journalTransactions[0].PK}'"));
			AssertEquals("Queue record created for journal line", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{journalTransactions[1].PK}'"));
			AssertEquals("Queue record created for journal line", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{journalTransactions2[0].PK}'"));
			AssertEquals("Queue record created for journal line", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{journalTransactions2[1].PK}'"));
			AssertEquals("Report status set to Queued", AccComplianceReport.Status.ReportDataQueued, report.ACR_Status);

			var log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process starting.", log);
			AssertContains("Debug|Started queue building for company: 'EDI', report type 'LIB', dates from 01-May-20 to 31-May-20.", log);
			AssertContains("Debug|Completed queue building for company: 'EDI', report type 'LIB', dates from 01-May-20 to 31-May-20.", log);
			AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);
		}

		[TestDate(2020, 5, 20)]
		public void TestComplianceReportQueuingServiceTask_TransactionsWithPresentationCategoryNotIncluded()
		{
			var configurations = Creator.CreateReportConfigurationWithQueuing(GroupingCodes.DayBook, BaseTablePrefixListCodes.AllTransactions);
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);
			var report = CreateReport();

			var header1 = Creator.CreateGLHeader("TEST.30.00").PK;
			var header2 = Creator.CreateGLHeader("TEST.40.00").PK;

			var journal = Creator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2020, 05, 3), new ZDateTime(2020, 05, 3), new ZDateTime(2020, 05, 10));
			journal.AH_TransactionCategory = "PRE";
			Creator.CreateGLJournalLine(journal, 350m, DebitCredit.DR, header1);
			Creator.CreateGLJournalLine(journal, 350m, DebitCredit.CR, header2);

			var journal2 = Creator.CreateGLJournal(TransactionTypes.GLAutoJournal, new ZDateTime(2020, 05, 3), new ZDateTime(2020, 05, 3), new ZDateTime(2020, 05, 10));
			journal2.AH_TransactionCategory = "PRE";
			journal2.AH_DueDate = new ZDateTime(2020, 05, 31, 23, 59, 59);
			Creator.CreateGLJournalLine(journal2, 350m, DebitCredit.DR, header1);
			Creator.CreateGLJournalLine(journal2, 350m, DebitCredit.CR, header2);

			Creator.CreateTestPeriodsForEntireYear(2020); // calls Factory.Save(), so no need to call again.

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			report.Reload();

			AssertEquals("Queue records created", 0, GetNoOfQueueRows());
			AssertEquals("Report status set to Queued", AccComplianceReport.Status.ReportDataQueued, report.ACR_Status);

			var log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process starting.", log);
			AssertContains("Debug|Started queue building for company: 'EDI', report type 'LIB', dates from 01-May-20 to 31-May-20.", log);
			AssertContains("Debug|Completed queue building for company: 'EDI', report type 'LIB', dates from 01-May-20 to 31-May-20.", log);
			AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);
		}

		[TestDate(2020, 5, 20)]
		public void TestComplianceReportQueuingServiceTask_TransactionsWithPayments()
		{
			// Test queuing for report with TPA grouping option for transactions of ledger and type from report settings having matchlinks, not reversed and Org having OB_APPrintContractorForm  flag ticked for AP ledger
			var configurations = Creator.CreateReportConfigurationWithQueuing(GroupingCodes.TransactionPayments, BaseTablePrefixListCodes.TransactionHeader);
			var reportConfig = configurations[0];
			var setting = reportConfig.Settings.AddNew();
			setting.LedgerType = LedgerTypes.AccountsPayable;
			setting.InvoiceType = TransactionTypes.Invoice;
			setting.TaxInvoiceRule = TaxInvoiceRuleCodes.All;
			setting.OriginalRule = OriginalRuleCodes.OriginalTransactionOnly;
			setting.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);

			var nonApplicableCreditor = Creator.Creditor2;
			nonApplicableCreditor.CompanyData.OB_APPrintContractorForm = false;
			var applicableCreditor = Creator.Creditor1;
			applicableCreditor.CompanyData.OB_APPrintContractorForm = true;

			var report = CreateReport();
			CreateTransactions();

			CreateAPInvoiceForCreditor("APIN0001", nonApplicableCreditor, PaymentOption.FullyPaid);

			var invoice1 = CreateAPInvoiceForCreditor("APIA0001", applicableCreditor, PaymentOption.FullyPaid);
			var invoice2 = CreateAPInvoiceForCreditor("APIA0002", applicableCreditor, PaymentOption.PartiallyPaid);

			CreateAPInvoiceForCreditor("APIA0010", applicableCreditor, PaymentOption.FullyPaid, new ZDateTime(2020, 4, 30)); // Before report period
			CreateAPInvoiceForCreditor("APIA0020", applicableCreditor, PaymentOption.FullyPaid, new ZDateTime(2020, 6, 1)); // After report period

			var invoiceToReverse = CreateAPInvoiceForCreditor("APIA0030", applicableCreditor); // Not paid, to be reversed
			var reversal = (APCreditNote)Creator.ReverseTransaction(invoiceToReverse, out _);
			reversal.AH_TransactionNum = "APCR0001";
			CreateAndPayAPCreditNotaForCreditor("ARCA0001", applicableCreditor);    // Credit note does not match settings

			Factory.Save();

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			report.Reload();

			AssertEquals("Queue records created", 2, GetNoOfQueueRows());
			AssertEquals("Queue record created for invoice1", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{invoice1.PK}'"));
			AssertEquals("Queue record created for invoice2", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{invoice2.PK}'"));
			AssertEquals("Report status set to Queued", AccComplianceReport.Status.ReportDataQueued, report.ACR_Status);

			var log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process starting.", log);
			AssertContains("Debug|Started queue building for company: 'EDI', report type 'LIB', dates from 01-May-20 to 31-May-20.", log);
			AssertContains("Debug|Completed queue building for company: 'EDI', report type 'LIB', dates from 01-May-20 to 31-May-20.", log);
			AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);
		}

		[TestDate(2021, 6, 30)]
		public void TestComplianceReportQueuingServiceTask_FullyPaidPTRS()
		{
			// Test queuing for report with TPA grouping option for transactions of ledger and type from report settings having matchlinks, not reversed and Org having OB_APPrintContractorForm  flag ticked for AP ledger
			var configurations = Creator.CreateReportConfigurationWithQueuing("PTR", BaseTablePrefixListCodes.TransactionHeader, reportCode: "PTR");
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);

			var nonApplicableCreditor = Creator.Creditor2;
			nonApplicableCreditor.CompanyData.OB_IsCreditor = true;
			nonApplicableCreditor.CompanyData.OB_APExcludeFromPaymentReports = true;

			var applicableCreditor = Creator.Creditor1;
			applicableCreditor.CompanyData.OB_IsCreditor = true;
			var rsbDocument = applicableCreditor.RequiredDocuments.AddNew("RSB");
			rsbDocument.EQ_DocCategory = "CTR";
			rsbDocument.EQ_ValidToDate = new ZDateTime(2021, 6, 30);

			var report = CreateReport("PTR");
			CreateTransactions();

			CreateAPInvoiceForCreditor("APIN0001", nonApplicableCreditor, PaymentOption.FullyPaid, new ZDateTime(2021, 6, 11));

			CreateAPInvoiceForCreditor("APIA0010", applicableCreditor, PaymentOption.notPaid);
			CreateAPInvoiceForCreditor("APIA0020", applicableCreditor, PaymentOption.PartiallyPaid, new ZDateTime(2021, 6, 12));

			CreateAPInvoiceForCreditor("APIA0030", applicableCreditor, PaymentOption.FullyPaid, new ZDateTime(2021, 5, 31)); // Before report period
			CreateAPInvoiceForCreditor("APIA0040", applicableCreditor, PaymentOption.FullyPaid, new ZDateTime(2021, 7, 1)); // After report period

			var invoiceToReverse = CreateAPInvoiceForCreditor("APIA0050", applicableCreditor); // Not paid, to be reversed
			var reversal = (APCreditNote)Creator.ReverseTransaction(invoiceToReverse, out _);
			reversal.AH_TransactionNum = "APCR0001";
			CreateAndPayAPCreditNotaForCreditor("ARCA0001", applicableCreditor);    // Credit notes are not used for this type of grouping

			var invoice1 = CreateAPInvoiceForCreditor("APIA0101", applicableCreditor, PaymentOption.FullyPaid, new ZDateTime(2021, 6, 1)); // Start of report period
			var invoice2 = CreateAPInvoiceForCreditor("APIA0102", applicableCreditor, PaymentOption.FullyPaid, new ZDateTime(2021, 6, 30)); // End of report period

			Factory.Save();

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			report.Reload();

			AssertEquals("Queue records created", 2, GetNoOfQueueRows());
			AssertEquals("Queue record created for invoice1", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{invoice1.PK}'"));
			AssertEquals("Queue record created for invoice2", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{invoice2.PK}'"));
			AssertEquals("Report status set to Queued", AccComplianceReport.Status.ReportDataQueued, report.ACR_Status);

			var log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process starting.", log);
			AssertContains("Debug|Started queue building for company: 'EDI', report type 'PTR', dates from 01-Jun-21 to 30-Jun-21.", log);
			AssertContains("Debug|Completed queue building for company: 'EDI', report type 'PTR', dates from 01-Jun-21 to 30-Jun-21.", log);
			AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);
		}

		[TestDate(2021, 6, 30)]
		public void TestComplianceReportQueuingServiceTask_AllPaymentsPTRS()
		{
			// Test queuing for report with TPA grouping option for transactions of ledger and type from report settings having matchlinks, not reversed and Org having OB_APPrintContractorForm  flag ticked for AP ledger
			var configurations = Creator.CreateReportConfigurationWithQueuing("PTA", BaseTablePrefixListCodes.TransactionHeader, reportCode: "PTA");
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);

			var nonApplicableCreditor = Creator.Creditor2;
			nonApplicableCreditor.CompanyData.OB_IsCreditor = true;
			nonApplicableCreditor.CompanyData.OB_APExcludeFromPaymentReports = true;
			var applicableCreditor = Creator.Creditor1;
			applicableCreditor.CompanyData.OB_IsCreditor = true;

			var report = CreateReport("PTA");
			CreateTransactions();

			CreateAPInvoiceForCreditor("APIN0001", nonApplicableCreditor, PaymentOption.FullyPaid, new ZDateTime(2021, 6, 11));

			CreateAPInvoiceForCreditor("APIA0010", applicableCreditor, PaymentOption.notPaid);
			var invoice1 = CreateAPInvoiceForCreditor("APIA0020", applicableCreditor, PaymentOption.PartiallyPaid, new ZDateTime(2021, 6, 12));

			CreateAPInvoiceForCreditor("APIA0030", applicableCreditor, PaymentOption.FullyPaid, new ZDateTime(2021, 5, 31)); // Before report period
			CreateAPInvoiceForCreditor("APIA0040", applicableCreditor, PaymentOption.FullyPaid, new ZDateTime(2021, 7, 1)); // After report period

			var invoiceToReverse = CreateAPInvoiceForCreditor("APIA0050", applicableCreditor); // Not paid, to be reversed
			var reversal = (APCreditNote)Creator.ReverseTransaction(invoiceToReverse, out _);
			reversal.AH_TransactionNum = "APCR0001";
			CreateAndPayAPCreditNotaForCreditor("ARCA0001", applicableCreditor);    // Credit notes are not used for this type of grouping

			var invoice2 = CreateAPInvoiceForCreditor("APIA0101", applicableCreditor, PaymentOption.FullyPaid, new ZDateTime(2021, 6, 1)); // Start of report period

			Factory.Save();

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			report.Reload();

			AssertEquals("Queue records created", 2, GetNoOfQueueRows());
			AssertEquals("Queue record created for invoice1", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{invoice1.PK}'"));
			AssertEquals("Queue record created for invoice2", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{invoice2.PK}'"));
			AssertEquals("Report status set to Queued", AccComplianceReport.Status.ReportDataQueued, report.ACR_Status);

			var log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process starting.", log);
			AssertContains("Debug|Started queue building for company: 'EDI', report type 'PTA', dates from 01-Jun-21 to 30-Jun-21.", log);
			AssertContains("Debug|Completed queue building for company: 'EDI', report type 'PTA', dates from 01-Jun-21 to 30-Jun-21.", log);
			AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						AccComplianceReportSchema.Constants.TableName,
						"Compliance Report Queuing",
						AccComplianceReportSchema.Constants.ACR_Status + "=" + AccComplianceReport.Status.ReportCreated),
					new TaskNudgeInformationForTest(
						AccComplianceReportSchema.Constants.TableName,
						"Compliance Report Re-queuing",
						AccComplianceReportSchema.Constants.ACR_Status + "=" + AccComplianceReport.Status.ReportPendingQueueing),
				};
			}
		}

		[TestDate(2022, 2, 22)]
		public void TestComplianceReportQueuingServiceTaskForJPKV7M()
		{
			// Arrange
			var polandCompany = Creator.CreateCompanyAndBranch("PLWAW");
			Factory.Save();

			using (Creator.SwitchEnvToCompany(polandCompany))
			{
				var configurations = Creator.CreateReportConfigurationWithQueuing("", BaseTablePrefixListCodes.TransactionLine);
				configurations[0].ReportCode = AccComplianceReport.ReportTypes.JPKV7M;
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);

				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.JPKV7M;
				complianceReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				complianceReport.ACR_Description = "Compliance Report JPK";
				complianceReport.ACR_DateFrom = new ZDate(2022, 1, 1);
				complianceReport.ACR_DateTo = new ZDate(2022, 1, 31);
				Factory.Save();

				var serviceTask = new ComplianceReportQueuingServiceTask();

				// Act
				var logger = InitialiseAndRunTaskSchedule(serviceTask);
				complianceReport.Reload();

				// Assert
				AssertEquals("Report status set to Finalized once the generated report has been processed", AccComplianceReport.Status.ReportOutputGenerated, complianceReport.ACR_Status);
				var docManager = complianceReport.DocManagerInfo();
				AssertEquals("One file attached to eDocs", 1, docManager.AllEDocs.Count);
				AssertEquals("FileName", "2022-02-22T000000_JPK_VAT.xml", docManager.AllEDocs[0].FileName);

				var log = logger.ToString();
				AssertContains("Information|Compliance report transaction queuing process starting.", log);
				AssertContains("Debug|Started generating output for company 'CPL', report type 'JPK', dates from 01-Jan-22 to 31-Jan-22", log);
				AssertContains("Debug|Generated JPK_V7M file:", log);
				AssertContains("2022-02-22T000000_JPK_VAT.xml", log);
				AssertContains("Debug|Export finished for Compliance Report JPK and took ", log);
				AssertContains("Debug|Completed generating output for company 'CPL', report type 'JPK', dates from 01-Jan-22 to 31-Jan-22", log);
				AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);
			}
		}

		[TestDate(2022, 2, 22)]
		public void TestComplianceReportQueuingServiceTaskForJPKV7M_NoProcessedReportWhenTablePrefixIs_AllTransactions()
			=> AssertComplianceReportQueuingServiceTaskForJPKV7M_NoProcessedReportWhenTablePrefixIsNot_TransactionLines(BaseTablePrefixListCodes.AllTransactions);

		[TestDate(2022, 2, 22)]
		public void TestComplianceReportQueuingServiceTaskForJPKV7M_NoProcessedReportWhenTablePrefixIs_TransactionHeader()
			=> AssertComplianceReportQueuingServiceTaskForJPKV7M_NoProcessedReportWhenTablePrefixIsNot_TransactionLines(BaseTablePrefixListCodes.TransactionHeader);

		void AssertComplianceReportQueuingServiceTaskForJPKV7M_NoProcessedReportWhenTablePrefixIsNot_TransactionLines(string tablePrefix)
		{
			var polandCompany = Creator.CreateCompanyAndBranch("PLWAW");
			Factory.Save();

			using (Creator.SwitchEnvToCompany(polandCompany))
			{
				var configurations = Creator.CreateReportConfigurationWithQueuing("", tablePrefix);
				configurations[0].ReportCode = AccComplianceReport.ReportTypes.JPKV7M;
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);

				var complianceReport = Factory.NewWithValidTestData<AccComplianceReport>();
				complianceReport.ACR_ReportType = AccComplianceReport.ReportTypes.JPKV7M;
				complianceReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				complianceReport.ACR_Description = "Compliance Report JPK";
				complianceReport.ACR_DateFrom = new ZDate(2022, 1, 1);
				complianceReport.ACR_DateTo = new ZDate(2022, 1, 31);
				Factory.Save();

				var serviceTask = new ComplianceReportQueuingServiceTask();

				var logger = InitialiseAndRunTaskSchedule(serviceTask);
				complianceReport.Reload();

				AssertEquals("Since Compliance Report Configuration is not set properly, the reports status is not changed", AccComplianceReport.Status.ReportGenerated, complianceReport.ACR_Status);
				var docManager = complianceReport.DocManagerInfo();
				AssertEquals("No attached file(s) to eDocs", 0, docManager.AllEDocs.Count);

				var log = logger.ToString();
				AssertContains("Information|Compliance report transaction queuing process starting.", log);
				AssertContains("Debug|Compliance report transaction queuing process iteration 1 started.", log);
				AssertContains("Information|Compliance report transaction queuing process completed after 1 iterations.", log);
			}
		}

		[TestDate(2019, 12, 31, 16, 27, 15)]
		public void TestCanRunInAnyBranch()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.France))
			{
				PrepareTestDataFEC();

				var serviceTask = new ComplianceReportQueuingServiceTask();
				using (EnvProxy.Instance.TemporaryServiceTaskContext(ComplianceReportQueuingServiceTask.Code, canRunInAnyBranch: true))
				{
					serviceTask.ServiceLogger = new LoggerForTest();
					serviceTask.RunTask();
				}
				AssertEquals("No Errors", 0, ErrorReporter.LastMessageReported.Length);
			}
		}

		public void TestAUComplianceReportPTSQueuingServiceTask()
		{
			var reportCode = AccountingConstants.ComplianceReportTypes.PaymentTimesSmallBusiness2024ReportType;
			var configurations = Creator.CreateReportConfigurationWithQueuing(GroupingCodes.PaymentTimesSmallBusinessReportable, BaseTablePrefixListCodes.TransactionHeader, reportCode: reportCode);
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);
			var creditor = CreateCreditorWithABN(true, "CRD1");
			var report = CreateReport(reportCode, "RNG", AUComplianceReportReportingFromDate, AUComplianceReportReportingToDate);
			var invoice1 = CreateAPInvoiceForCreditor("APIA0020", creditor, PaymentOption.FullyPaid, AUComplianceReportReportingFromDate);// Start of report period
			var invoice2 = CreateAPInvoiceForCreditor("APIA0101", creditor, PaymentOption.FullyPaid, AUComplianceReportReportingToDate); // End of report period
			Factory.Save();
			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			report.Reload();

			AssertEquals("Queue records created", 2, GetNoOfQueueRows());
			AssertEquals("Queue record created for invoice1", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{invoice1.PK}'"));
			AssertEquals("Queue record created for invoice2", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{invoice2.PK}'"));
			AssertEquals("Report status set to Queued", AccComplianceReport.Status.ReportDataQueued, report.ACR_Status);
		}

		public void TestAUComplianceReportTCPQueuingServiceTask()
		{
			var reportCode = AccountingConstants.ComplianceReportTypes.PaymentTimesAllPayments2024ReportType;
			var configurations = Creator.CreateReportConfigurationWithQueuing(groupingCode: GroupingCodes.PaymentTimesAll, BaseTablePrefixListCodes.TransactionHeader, reportCode: reportCode);
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);
			var smallBusinessCreditor = CreateCreditorWithABN(true, "CRD1");
			var nonSmallBusinessCreditor = CreateCreditorWithABN(false, "CRD2");
			var report = CreateReport(reportCode, "RNG", AUComplianceReportReportingFromDate, AUComplianceReportReportingToDate);
			var invoice1 = CreateAPInvoiceForCreditor("APIA0001", smallBusinessCreditor, PaymentOption.FullyPaid, AUComplianceReportReportingFromDate);// Start of report period
			var invoice2 = CreateAPInvoiceForCreditor("APIA0002", nonSmallBusinessCreditor, PaymentOption.FullyPaid, AUComplianceReportReportingToDate); // End of report period
			var invoice3 = CreateAPInvoiceForCreditor("APIA0003", smallBusinessCreditor, PaymentOption.PartiallyPaid, AUComplianceReportReportingFromDate);// Start of report period
			var invoice4 = CreateAPInvoiceForCreditor("APIA0004", nonSmallBusinessCreditor, PaymentOption.PartiallyPaid, AUComplianceReportReportingToDate); // End of report period
			Factory.Save();

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			report.Reload();

			AssertEquals("Queue records created", 4, GetNoOfQueueRows());
			AssertEquals("Queue record created for invoice1", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{invoice1.PK}'"));
			AssertEquals("Queue record created for invoice2", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{invoice2.PK}'"));
			AssertEquals("Queue record created for invoice3", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{invoice3.PK}'"));
			AssertEquals("Queue record created for invoice4", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{invoice4.PK}'"));
			AssertEquals("Report status set to Queued", AccComplianceReport.Status.ReportDataQueued, report.ACR_Status);
		}

		OrgHeader CreateCreditorWithABN(bool isSmallBusiness, string orgCode)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = orgCode;
			orgHeader.OH_FullName = orgCode;
			orgHeader.OH_IsCreditor = true;

			var orgCusCode = Factory.NewWithValidTestData<OrgCusCode>();
			orgCusCode.OK_CodeType = "ABN";
			orgCusCode.OK_CountryDefault = false;
			orgCusCode.OK_CustomsRegNo = Guid.NewGuid().ToString();
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_RN_NKCodeCountry = "AU";
			orgCusCode.OK_SystemCreateTimeUtc = DateTime.UtcNow;
			orgCusCode.OK_SystemCreateUser = "E";
			orgCusCode.OK_SystemLastEditTimeUtc = DateTime.UtcNow;
			orgCusCode.OK_SystemLastEditUser = "E";

			if (isSmallBusiness)
			{
				var doc = Factory.NewWithValidTestData<JobRequiredDocument>();
				doc.ParentType = typeof(OrgHeader);
				doc.EQ_DocCategory = "CTR";
				doc.EQ_DocType = "RSB";
				doc.EQ_ParentTableCode = "OH";
				doc.EQ_ParentID = orgHeader.PK;
				doc.EQ_DateReceived = AUComplianceReportReportingFromDate.ToDateTime();
				doc.EQ_ValidToDate = AUComplianceReportReportingToDate;
			}
			return orgHeader;
		}

		#region Generating Tests

		public void TestGenerateForCompany_CompletesInOneRun_WhenUnderTimeLimit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var report = CreateIDEAReportWithTwelvePeriodsAndQueueRecords();

				var serviceTask = new ComplianceReportQueuingServiceTask();
				InitialiseAndRunTaskSchedule(serviceTask);

				// Assert that all generation steps have been executed in one run
				var newFactory = new BusinessObjectFactory();
				var reportLoadedFromDatabase = newFactory.Load<AccComplianceReport>(report.PK);
				AssertEquals("Report status after CRQ run", AccComplianceReport.Status.ReportGenerated, reportLoadedFromDatabase.ACR_Status);
			}
		}

		public void TestGenerateForCompany_ProcessAtLeastOne_WhenOverTimeLimit()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var report = CreateIDEAReportWithTwelvePeriodsAndQueueRecords();

				// set time limit to 0 so only one generation run is done
				using (AccountingMasterFilesRegistry.Instance.MaximumTimeForCRQServiceTaskToRun.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				{
					var serviceTask = new ComplianceReportQueuingServiceTask();
					InitialiseAndRunTaskSchedule(serviceTask);
				}

				// Assert that the report is not yet generated
				var newFactory = new BusinessObjectFactory();
				var reportLoadedFromDatabase = newFactory.Load<AccComplianceReport>(report.PK);
				AssertEquals("Report status after CRQ run", AccComplianceReport.Status.ReportDataQueued, reportLoadedFromDatabase.ACR_Status);
			}
		}

		public void TestGenerateForCompany_CancelGeneration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var report = CreateIDEAReportWithTwelvePeriodsAndQueueRecords();

				var serviceTask = new ComplianceReportQueuingServiceTask();
				AssertExceptionThrown<OperationCanceledException>("Should throw operation cancelled exception", () => { InitialiseAndRunTaskSchedule(serviceTask, new CancellationToken(true)); });

				// Assert that the report is not yet generated
				var newFactory = new BusinessObjectFactory();
				var reportLoadedFromDatabase = newFactory.Load<AccComplianceReport>(report.PK);
				AssertEquals("Report status after CRQ run", AccComplianceReport.Status.ReportDataQueued, reportLoadedFromDatabase.ACR_Status);
			}
		}

		#endregion

		#region Queue Purging Tests

		public void TestNoPurgingAttemptedWithoutReportConfigurationsHavingSettings()
		{
			var activeCompanies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, ZBool.True));
			foreach (var company in activeCompanies)
			{
				Creator.CreateReportConfigurationWithQueuing("DAB", company: company);
			}

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			var log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process starting.", log);
			AssertNotContains("Debug|Started queue building", log);
			AssertNotContains("Debug|Completed queue building", log);
			AssertContains("Information|Compliance report transaction queuing process completed after 1 iterations.", log);
			AssertNotContains("Debug|Started purging old queue records.", log);
			AssertNotContains("Debug|Completed purging old queue records.", log);
		}

		public void TestNoPurgingAttemptedForTPAReportHavingSettingsButQueuedOnlyByServiceTask()
		{
			var activeCompanies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, ZBool.True));
			var auCompany = activeCompanies.FirstOrDefault(x => x.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia);
			AssertNotNull(auCompany);

			foreach (var company in activeCompanies.Except(auCompany))
			{
				Creator.CreateReportConfigurationWithQueuing("DAB", company: company);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.SetTemporaryValue(auCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var reportConfigs = AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.GetValueWithoutFallback(auCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var tpaConfig = reportConfigs.OfType<ComplianceReportConfiguration>().FirstOrDefault(x => x.Country == Core.Constants.CountryCodes.Australia
					&& x.ReportCode == AccountingConstants.ComplianceReportTypes.TaxablePaymentsAnnualReportType);
				AssertNotNull("TPAR Configuration", tpaConfig);
				// TPAR Report is queued only by the CRQ task but is based on TransactionHeader and has Settings, which matches requirements for queue purging
				AssertEquals("TPAR Configuration ReportLineGrouping", GroupingCodes.TransactionPayments, tpaConfig.ReportLineGrouping);
				AssertEquals("TPAR Configuration ReportBaseTablePrefix", BaseTablePrefixListCodes.TransactionHeader, tpaConfig.ReportBaseTablePrefix);
				Assert("TPAR Configuration has Settings", tpaConfig.Settings.Any());

				var newRegistryCOnfig = new ComplianceReportConfigurationCollection(tpaConfig.Factory);
				newRegistryCOnfig.Add(tpaConfig);

				using (AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetTemporaryValue(auCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, newRegistryCOnfig))
				{
					var serviceTask = new ComplianceReportQueuingServiceTask();
					var logger = InitialiseAndRunTaskSchedule(serviceTask);

					var log = logger.ToString();
					AssertContains("Information|Compliance report transaction queuing process starting.", log);
					AssertNotContains("Debug|Started queue building", log);
					AssertNotContains("Debug|Completed queue building", log);
					AssertContains("Information|Compliance report transaction queuing process completed after 1 iterations.", log);
					// Should not be any attempts for purging
					AssertNotContains("Debug|Started purging old queue records.", log);
					AssertNotContains("Debug|Completed purging old queue records.", log);
				}
			}
		}

		[TestDate(2021, 6, 14)]
		public void TestPurgingAttemptedWhenReportIsPendingQueue()
		{
			var activeCompanies = Factory.Load<GlbCompany>(new ZQuery(GlbCompanySchema.GC_IsActive, ZBool.True));
			foreach (var company in activeCompanies.Where(x => x.PK != GlbCompany.CurrentCompany.PK))
			{
				Creator.CreateReportConfigurationWithoutQueuing(company);
			}

			Creator.CreateReportConfigurationWithQueuing("DAB");
			var report = CreateReport(); // Report pending queuing
			Factory.Save();

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			var log = logger.ToString();
			AssertContains("Information|Compliance report transaction queuing process starting.", log);
			AssertContains("Debug|Started queue building for company: 'EDI', report type 'LIB', dates from 01-Jun-21 to 30-Jun-21.", log);
			AssertContains("Debug|Completed queue building for company: 'EDI', report type 'LIB', dates from 01-Jun-21 to 30-Jun-21.", log);
			AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);
			AssertContains("Debug|Started purging old queue records.", log);

			report.Reload();
			AssertEquals(AccComplianceReport.Status.ReportDataQueued, report.ACR_Status);

			serviceTask = new ComplianceReportQueuingServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);
			AssertLoggerForQueuePurgingAttempted(logger);
		}

		[TestDate(2021, 12, 15)]
		public void TestNoQueueRecordsPurgedWhenTheyAreNotOldEnough()
		{
			Creator.CreateReportConfigurationWithoutQueuing();
			var queueDate = ZDateTime.Today.AddMonths(-24);

			var report = CreateReport("IBL");
			var invoices = CreateARInvoices();
			Factory.Save();
			var queuePKs = Creator.CreateComplianceReportQueueEntry(report, "", queueDate, invoices.SelectMany(x => x.Lines.OfType<InvoicingLineBase>().ToArray()).ToArray());

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			AssertLoggerForQueuePurgingAttempted(logger);

			var loadedQueuePKs = LoadQueueRecordPKs();
			AssertContainsExactElementsInAnyOrder("No Queue records were purged", queuePKs, loadedQueuePKs);
		}

		[TestDate(2021, 12, 15)]
		public void TestNoQueueRecordsPurgedWithRelatedReport()
		{
			Creator.CreateReportConfigurationWithoutQueuing();
			var queueDate = ZDateTime.Today.AddMonths(-25);

			var report = CreateReport("IBL");
			report.ACR_DateFrom = queueDate.Date;
			report.ACR_DateTo = queueDate.AddMonths(1).Date;

			var invoices = CreateARInvoices();
			Factory.Save();
			var queuePKs = Creator.CreateComplianceReportQueueEntry(report, "", queueDate, invoices.SelectMany(x => x.Lines.OfType<InvoicingLineBase>().ToArray()).ToArray());

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			AssertLoggerForQueuePurgingAttempted(logger);

			var loadedQueuePKs = LoadQueueRecordPKs();
			AssertContainsExactElementsInAnyOrder("No Queue records were purged", queuePKs, loadedQueuePKs);
		}

		public void TestPurgeQueueRecords()
		{
			var configurations = Creator.CreateReportConfigurationWithoutQueuing();
			Creator.AddNewConfigurationWithoutQueuing(configurations, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IBH", BaseTablePrefixListCodes.TransactionHeader); // Second configuration with different Table Prefix
			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);

			var queueDateToPurge = ZDateTime.Today.AddMonths(-25);
			var queueDateToKeep = ZDateTime.Today.AddMonths(-24);

			var reportAL = CreateReport("IBL");
			reportAL.ACR_DateFrom = queueDateToPurge.AddDays(1).Date;
			reportAL.ACR_DateTo = reportAL.ACR_DateFrom.AddDays(1);

			var reportAH = CreateReport("IBH");
			reportAH.ACR_DateFrom = queueDateToPurge.AddDays(1).Date;
			reportAH.ACR_DateTo = reportAL.ACR_DateFrom.AddDays(1);
			Factory.Save();

			var invoices = CreateARInvoices();
			Factory.Save();
			var queueToPurgeALPKs = Creator.CreateComplianceReportQueueEntry(reportAL, "", queueDateToPurge, invoices.SelectMany(x => x.Lines.OfType<InvoicingLineBase>().ToArray()).ToArray());
			var queueToPurgeAHPKs = Creator.CreateComplianceReportQueueEntry(reportAH, invoices, (_) => queueDateToPurge.Date, (_) => string.Empty);

			invoices = CreateARInvoices();
			Factory.Save();
			var queueToKeepByReportALPKs = Creator.CreateComplianceReportQueueEntry(reportAL, "", reportAL.ACR_DateFrom, invoices.SelectMany(x => x.Lines.OfType<InvoicingLineBase>().ToArray()).ToArray());
			var queueToKeepByReportAHPKs = Creator.CreateComplianceReportQueueEntry(reportAH, invoices, (_) => reportAH.ACR_DateFrom, (_) => string.Empty);

			invoices = CreateARInvoices();
			Factory.Save();
			var queueToKeepAsNotOldALPKs = Creator.CreateComplianceReportQueueEntry(reportAL, "", queueDateToKeep, invoices.SelectMany(x => x.Lines.OfType<InvoicingLineBase>().ToArray()).ToArray());
			var queueToKeepAsNotOldAHPKs = Creator.CreateComplianceReportQueueEntry(reportAH, invoices, (_) => queueDateToKeep.Date, (_) => string.Empty);

			var allQueueRecords = queueToPurgeALPKs.Union(queueToKeepByReportALPKs).Union(queueToKeepAsNotOldALPKs)
				.Union(queueToPurgeAHPKs).Union(queueToKeepByReportAHPKs).Union(queueToKeepAsNotOldAHPKs).ToArray();
			var loadedQueuePKs = LoadQueueRecordPKs();
			AssertContainsExactElementsInAnyOrder("All Queue records before purging", allQueueRecords, loadedQueuePKs);

			var serviceTask = new ComplianceReportQueuingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);
			AssertLoggerForQueuePurged(logger, 10, "'IBH'", reportAH.Company.PK.ToString());
			AssertLoggerForQueuePurged(logger, 10, "'IBL'", reportAL.Company.PK.ToString());

			var queueRecordsToKeep = queueToKeepByReportALPKs.Union(queueToKeepAsNotOldALPKs).Union(queueToKeepByReportAHPKs).Union(queueToKeepAsNotOldAHPKs).ToArray();
			loadedQueuePKs = LoadQueueRecordPKs();
			AssertContainsExactElementsInAnyOrder("Old Queue records without Report were purged", queueRecordsToKeep, loadedQueuePKs);
		}

		public void TestMaxQueueRecordsToPurgeAtOnce()
		{
			using (AccountingMasterFilesRegistry.Instance.MaximumTimeForCRQServiceTaskToRun.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			{
				var configurations = Creator.CreateReportConfigurationWithoutQueuing();
				Creator.AddNewConfigurationWithoutQueuing(configurations, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, "IB2",
					BaseTablePrefixListCodes.TransactionLine); // Second configuration with same Table Prefix
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);

				var companySIN = GlbCompany.GetActiveCompanies(countryCode: "SG", Factory).FirstOrDefault();
				AssertNotNull(companySIN);
				Creator.CreateReportConfigurationWithoutQueuing(companySIN);

				var queueDate = ZDateTime.Today.AddMonths(-25);

				var report = CreateReport("IBL");

				var invoices = CreateARInvoices(30);
				var linePKs = invoices.SelectMany(x => x.Lines.OfType<InvoicingLineBase>()).ToArray();
				AssertEquals(30, linePKs.Length);
				Factory.Save();
				Creator.CreateComplianceReportQueueEntry(report, "", queueDate, linePKs);

				var queueSINPKs = Array.Empty<ZGuid>();
				var reportSIN = CreateReport("IBL");
				using (Env.Instance.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, companySIN.FirstActiveBranch.PK.ToGuid(), Env.Instance.CurrentDepartment.PK))
				{
					invoices = CreateARInvoices(3);
					linePKs = invoices.SelectMany(x => x.Lines.OfType<InvoicingLineBase>()).ToArray();
					AssertEquals(3, linePKs.Length);
					Factory.Save();
					queueSINPKs = Creator.CreateComplianceReportQueueEntry(reportSIN, "", queueDate, linePKs);
				}

				var loadedQueuePKs = LoadQueueRecordPKs();
				AssertEquals("Pre-condition: Number of queue records", 33, loadedQueuePKs.Length);

				var serviceTask = new ComplianceReportQueuingServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);
				AssertLoggerForQueuePurged(logger, 25, "'IB2','IBL'", reportSIN.Company.PK.ToString());

				loadedQueuePKs = LoadQueueRecordPKs();
				AssertEquals("Maximum 25 Queue records can be purged per a test run", 8, loadedQueuePKs.Length);

				serviceTask = new ComplianceReportQueuingServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);
				AssertLoggerForQueuePurged(logger, 5, "'IB2','IBL'", reportSIN.Company.PK.ToString());

				loadedQueuePKs = LoadQueueRecordPKs();
				AssertEquals("The rest of EDI Queue records were purged", 3, loadedQueuePKs.Length);
				AssertContainsExactElementsInAnyOrder("Only SIN queue records are left", queueSINPKs, loadedQueuePKs);

				serviceTask = new ComplianceReportQueuingServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);
				AssertLoggerForQueuePurged(logger, 3, "'IBL'", companySIN.PK.ToString());

				loadedQueuePKs = LoadQueueRecordPKs();
				AssertEquals("The SIN Queue records were purged", 0, loadedQueuePKs.Length);
			}
		}

		void AssertLoggerForQueuePurgingAttempted(TestServiceLogger logger)
		{
			var log = logger.ToString();
			CombineAssertions(() =>
			{
				AssertContains("Information|Compliance report transaction queuing process starting.", log);
				AssertNotContains("Debug|Started queue building", log);
				AssertNotContains("Debug|Completed queue building", log);
				AssertContains("Debug|Started purging old queue records.", log);
			});
		}

		void AssertLoggerForQueuePurged(TestServiceLogger logger, int purgedRecords, string purgedReportTypes, string companyPK)
		{
			var log = logger.ToString();
			CombineAssertions(() =>
			{
				AssertContains("Information|Compliance report transaction queuing process starting.", log);
				AssertNotContains("Debug|Started queue building", log);
				AssertNotContains("Debug|Completed queue building", log);
				AssertContains("Debug|Started purging old queue records.", log);
				AssertContains($"Debug|Purged {purgedRecords} old queue records for company: '{companyPK}', report types {purgedReportTypes}, purge date 01-", log);
			});
		}

		[TestDate(2021, 1, 15)]
		public void TestPurgeGeneratedRecordsImmediately()
		{
			var germanCompany = Creator.CreateNewCompany("DE1", Core.Constants.CountryCodes.Germany, orgProxy: Creator.ZECTRA);
			Creator.CreateReportConfiguration(germanCompany, Core.Constants.CountryCodes.Germany, AccComplianceReport.ReportTypes.IDEA, BaseTablePrefixListCodes.AllTransactions, PeriodicityCodes.DateRange);
			var frenchCompany = Creator.CreateNewCompany("FR1", Core.Constants.CountryCodes.France, orgProxy: Creator.ZECTRA);
			Creator.CreateReportConfiguration(frenchCompany, Core.Constants.CountryCodes.France, AccComplianceReport.ReportTypes.FEC, BaseTablePrefixListCodes.AllTransactions, PeriodicityCodes.DateRange);
			var serviceTask = new ComplianceReportQueuingServiceTask();
			var numberOfQueueRecordsToCreate = ComplianceReportQueuingServiceTask.MaxRecordsToPurge * 3 + 1;

			// Create a compliance report of type "IDE" and status "GEN" and insert more queue records for it than a single purge action will delete
			var ideaReport = Factory.NewWithValidTestData<AccComplianceReport>();
			ideaReport.ACR_GC_Company = germanCompany.PK;
			ideaReport.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;
			ideaReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			ideaReport.ACR_Periodicity = PeriodicityCodes.DateRange;
			ideaReport.ACR_DateFrom = new ZDate(2021, 1, 1);
			ideaReport.ACR_DateTo = new ZDate(2021, 1, 31);
			var invoice1 = CreateARInvoiceForDebtor("AR0001", TransactionTypes.Invoice, Creator.Debtor, Creator.EUR, 300, Creator.REV);
			Factory.Save();
			for (var i = 0; i < numberOfQueueRecordsToCreate; i++)
			{
				Creator.CreateComplianceReportQueueEntryForCompany(germanCompany, ideaReport, invoice1);
			}

			// Create a compliance report of type "FEC" and status "GEN" and insert more queue records for it than a single purge action will delete
			var fecReport = Factory.NewWithValidTestData<AccComplianceReport>();
			fecReport.ACR_GC_Company = frenchCompany.PK;
			fecReport.ACR_ReportType = AccComplianceReport.ReportTypes.FEC;
			fecReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			fecReport.ACR_Periodicity = PeriodicityCodes.DateRange;
			fecReport.ACR_DateFrom = new ZDate(2021, 1, 1);
			fecReport.ACR_DateTo = new ZDate(2021, 1, 31);
			var invoice2 = CreateARInvoiceForDebtor("AR0002", TransactionTypes.Invoice, Creator.Debtor1, Creator.EUR, 555, Creator.REV);
			Factory.Save();
			for (var i = 0; i < numberOfQueueRecordsToCreate; i++)
			{
				Creator.CreateComplianceReportQueueEntryForCompany(frenchCompany, fecReport, invoice2);
			}

			AssertEquals("Queue records before purge", numberOfQueueRecordsToCreate * 2, GetNoOfQueueRows());

			InitialiseAndRunTaskSchedule(serviceTask);
			AssertEquals("Queue records after purge", 0, GetNoOfQueueRows());
		}

		[TestDate(2023, 11, 23)]
		public void TestJPKNotPurgeQueueImmediatelyAfterOutputGenerated()
		{
			var polishCompany = Creator.CreateNewCompany("PL1", Core.Constants.CountryCodes.Poland, orgProxy: Creator.ZECTRA);
			Creator.CreateReportConfiguration(polishCompany, Core.Constants.CountryCodes.Poland, AccComplianceReport.ReportTypes.JPKV7M, BaseTablePrefixListCodes.TransactionLine, PeriodicityCodes.CalendarMonth);

			var serviceTask = new ComplianceReportQueuingServiceTask();

			// Create a compliance report of type "JPK" and status "GEN" and insert queue records for it
			var jpkReport = Factory.NewWithValidTestData<AccComplianceReport>();
			jpkReport.ACR_GC_Company = polishCompany.PK;
			jpkReport.ACR_ReportType = AccComplianceReport.ReportTypes.JPKV7M;
			jpkReport.ACR_Status = AccComplianceReport.Status.ReportGenerated;
			jpkReport.ACR_Periodicity = PeriodicityCodes.DateRange;
			jpkReport.ACR_DateFrom = new ZDate(2023, 10, 1);
			jpkReport.ACR_DateTo = new ZDate(2023, 10, 31);
			var invoice1 = CreateARInvoiceForDebtor("AR0001", TransactionTypes.Invoice, Creator.Debtor, Creator.EUR, 300m, Creator.REV);
			Factory.Save();

			Creator.CreateComplianceReportQueueEntry(jpkReport, invoice1.Lines[0]);

			InitialiseAndRunTaskSchedule(serviceTask);

			jpkReport.Reload();
			AssertEquals("Output Generated", AccComplianceReport.Status.ReportOutputGenerated, jpkReport.ACR_Status);
			AssertEquals("Queue records not purged after generating output", 1, GetNoOfQueueRows());

			InitialiseAndRunTaskSchedule(serviceTask);

			AssertEquals("Queue records not purged after next run", 1, GetNoOfQueueRows());
		}

		[TestDate(2021, 1, 16)]
		public void TestPurgeQueueImmediatelyTimeLimit()
		{
			var germanCompany = Creator.CreateNewCompany("DE1", Core.Constants.CountryCodes.Germany, orgProxy: Creator.ZECTRA);
			Creator.CreateReportConfiguration(germanCompany, Core.Constants.CountryCodes.Germany, AccComplianceReport.ReportTypes.IDEA, BaseTablePrefixListCodes.AllTransactions, PeriodicityCodes.DateRange);

			// Create a compliance report of type "IDE" and status "GEN" because this matches the condition for purging
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_GC_Company = germanCompany.PK;
				report.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;
				report.ACR_Status = AccComplianceReport.Status.ReportGenerated;
				report.ACR_Periodicity = PeriodicityCodes.DateRange;
				report.ACR_DateFrom = new ZDate(2021, 1, 1);
				report.ACR_DateTo = new ZDate(2021, 1, 31);
				var invoice = CreateARInvoiceForDebtor("AR0001", TransactionTypes.Invoice, Creator.Debtor, Creator.EUR, 300, Creator.REV);
				Factory.Save();

				var serviceTask = new ComplianceReportQueuingServiceTask();
				var numberOfQueueRecordsToCreate = ComplianceReportQueuingServiceTask.MaxRecordsToPurge + 1;
				for (var i = 0; i < numberOfQueueRecordsToCreate; i++)
				{
					Creator.CreateComplianceReportQueueEntryForCompany(germanCompany, report, invoice);
				}
				AssertEquals("Queue records before purge", numberOfQueueRecordsToCreate, GetNoOfQueueRows());

				// set time limit to 0 so only one purge run is done
				using (AccountingMasterFilesRegistry.Instance.MaximumTimeForCRQServiceTaskToRun.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
				{
					var logger = InitialiseAndRunTaskSchedule(serviceTask);
				}

				AssertEquals("Queue records after purge", 0, GetNoOfQueueRows());
			}
		}

		#endregion

		#region ZM Report

		[TestDate(2021, 1, 15)]
		public void TestComplianceReportQueuingServiceTask_ZMReport_DebtorWithoutVatId()
		{
			// Test queuing for "Zusammenfassende Meldung" report
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var configurations = CreateZMReportConfiguration();
				AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);

				var report = Factory.NewWithValidTestData<AccComplianceReport>();
				report.ACR_GC_Company = GlbCompany.CurrentCompany.PK;
				report.ACR_ReportType = "ZMD";
				report.ACR_Periodicity = PeriodicityCodes.DateRange;
				report.ACR_DateFrom = new ZDate(2021, 1, 1);
				report.ACR_DateTo = new ZDate(2021, 1, 31);

				var germanDebtor = CreateDebtorWithBusinessRegNo("DEBTORDE1", Constants.CountryCodes.Germany, "UST", "123456789");
				var frenchDebtor = CreateDebtorWithBusinessRegNo("DEBTORFR1", Constants.CountryCodes.France, "TVA", "987654321");
				var frenchDebtorWithoutVatID = CreateDebtorWithBusinessRegNo("DEBTORFR2", Constants.CountryCodes.France, "TVA", null);
				var australianDebtor = CreateDebtorWithBusinessRegNo("DEBTORAU1", Constants.CountryCodes.Australia, "LSC", "111222333");
				var multiCountryDebtor = CreateDebtorWithBusinessRegNo("DEBTORMC1", Constants.CountryCodes.Netherlands, "BTW", "777666555");
				multiCountryDebtor.CustomsCodes.AddNew("TVA", "987987987", Core.Constants.CountryCodes.France);
				var additionalNonMainAddress = multiCountryDebtor.Addresses.AddNew(OrgAddressType.Residential, false);
				additionalNonMainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
				additionalNonMainAddress.Address1 = "Paris";

				Creator.CreateTestPeriods(new ZDateTime(2021, 1, 1));

				var germanInvoice = CreateARInvoiceForDebtor("ARDE0001", TransactionTypes.Invoice, germanDebtor, Creator.EUR, 100, Creator.REV);  // must not be queued because transactions inside Germany are to be ignored
				var frenchInvoice = CreateARInvoiceForDebtor("ARFR0001", TransactionTypes.Invoice, frenchDebtor, Creator.EUR, 200, Creator.REV);  // must be queued
				var frenchInvoice2 = CreateARInvoiceForDebtor("ARFR0002", TransactionTypes.Invoice, frenchDebtorWithoutVatID, Creator.EUR, 200, Creator.REV);  // must be queued
				var australianInvoice = CreateARInvoiceForDebtor("ARAU0001", TransactionTypes.Invoice, australianDebtor, Creator.EUR, 300, Creator.REV);  // must not be queued because Australia is not part of the EU
				var multiCountryInvoice = CreateARInvoiceForDebtor("ARMC0001", TransactionTypes.Invoice, multiCountryDebtor, Creator.EUR, 400, Creator.REV);  // must be queued

				Factory.Save();

				var serviceTask = new ComplianceReportQueuingServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);
				report.Reload();

				AssertEquals("Queue records created", 3, GetNoOfQueueRows());
				AssertEquals("Queue record created for French invoice", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{frenchInvoice.Lines[0].PK}' AND ACQ_ReportSubCode = 'FR987654321'"));
				AssertEquals("Queue record created for French invoice Debtor without VAT ID", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{frenchInvoice2.Lines[0].PK}' AND ACQ_ReportSubCode = 'VAT ID missing'"));
				AssertEquals("Queue record created for MultiCountry invoice", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{multiCountryInvoice.Lines[0].PK}' AND ACQ_ReportSubCode = 'NL777666555'"));
				AssertEquals("Report status set to Error", AccComplianceReport.Status.ReportError, report.ACR_Status);
				AssertEquals("Report status message has been set", "Debtors without VAT ID found. Please check the Notes tab for details.", report.ACR_StatusMessage);
				var notes = report.GetNotes().GetAllNotes();
				AssertEquals("Number of notes attached to report", 1, notes.Count);
				var firstNote = notes.Cast<StmNote>().First();
				var notesText = @"These debtors have Accounts Receivable transactions (INV, CRD, ADJ) with reverse charge Tax ID but do not have a VAT ID assigned.

ZDEBTORFR2 - Test Company Name : 00001002";
				AssertEquals("Note text", notesText, firstNote.ST_NoteDataAsText);

				var log = logger.ToString();
				AssertContains("Information|Compliance report transaction queuing process starting.", log);
				AssertContains("Debug|Started queue building for company: 'EDI', report type 'ZMD', dates from 01-Jan-21 to 31-Jan-21.", log);
				AssertContains("Debug|Completed queue building for company: 'EDI', report type 'ZMD', dates from 01-Jan-21 to 31-Jan-21.", log);
				AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);

				// set the VAT ID for frenchDebtorWithoutVatID and re-run the report
				frenchDebtorWithoutVatID.CustomsCodes.AddNew("TVA", "6543322", Constants.CountryCodes.France);
				Factory.Save();
				report.ReQueue();
				Factory.Save();

				serviceTask = new ComplianceReportQueuingServiceTask();
				logger = InitialiseAndRunTaskSchedule(serviceTask);
				report = new BusinessObjectFactory().Load<AccComplianceReport>(report.PK);

				AssertEquals("Queue records created", 3, GetNoOfQueueRows());
				AssertEquals("Queue record created for French invoice", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{frenchInvoice.Lines[0].PK}' AND ACQ_ReportSubCode = 'FR987654321'"));
				AssertEquals("Queue record created for French invoice Debtor now with VAT ID", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{frenchInvoice2.Lines[0].PK}' AND ACQ_ReportSubCode = 'FR6543322'"));
				AssertEquals("Queue record created for MultiCountry invoice", 1, GetNoOfQueueRows($"Where ACQ_ParentID = '{multiCountryInvoice.Lines[0].PK}' AND ACQ_ReportSubCode = 'NL777666555'"));
				AssertEquals("Report status set to Queued", AccComplianceReport.Status.ReportDataQueued, report.ACR_Status);
				AssertEquals("Report status message has been set", "Report data has been queued. Please click Generate to proceed.", report.ACR_StatusMessage);
				notes = report.GetNotes().GetAllNotes();
				AssertEquals("Number of notes attached to report", 0, notes.Count);

				log = logger.ToString();
				AssertContains("Information|Compliance report transaction queuing process starting.", log);
				AssertContains("Debug|Started queue building for company: 'EDI', report type 'ZMD', dates from 01-Jan-21 to 31-Jan-21.", log);
				AssertContains("Debug|Completed queue building for company: 'EDI', report type 'ZMD', dates from 01-Jan-21 to 31-Jan-21.", log);
				AssertContains("Information|Compliance report transaction queuing process completed after 2 iterations.", log);
			}
		}

		#endregion

		#region Implementation

		AccComplianceReport CreateReport(string reportType = "LIB", string periodicity = PeriodicityCodes.DateRange, ZDate? fromDate = null, ZDate? toDate = null)
		{
			var result = Factory.NewWithValidTestData<AccComplianceReport>();
			result.ACR_GC_Company = GlbCompany.CurrentCompany.PK;
			result.ACR_ReportType = reportType;
			result.ACR_Periodicity = periodicity;
			result.ACR_Description = "Compliance Report " + reportType;

			result.ACR_DateFrom = fromDate ?? ZDate.Today.AddDays(-ZDate.Today.Day + 1);
			result.ACR_DateTo = toDate ?? result.ACR_DateFrom.AddMonths(1).AddDays(-1);

			return result;
		}

		InvoicingBase[] CreateARInvoices(int count = 10)
		{
			Creator.CreateTestPeriods(ZDateTime.Now);

			var result = new List<InvoicingBase>();
			for (var i = 0; i < count; i++)
			{
				var arInvoice = Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
				Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 100m + i);
				result.Add(arInvoice);
			}

			return result.ToArray();
		}

		void CreateTransactions()
		{
			Creator.CreateTestPeriods(ZDateTime.Now);

			var arInvoice = Creator.CreateInvoice(typeof(ARInvoice), Creator.AUD, 1m);
			Creator.CreateInvoiceLine(arInvoice, Creator.AUD, 1m, 100m);

			var apInvoice = Creator.CreateAPInvoice<APInvoice>("AP0001", Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m);
		}

		ZGuid[] LoadQueueRecordPKs()
		{
			var sql = $"SELECT {AccTransactionComplianceReportQueueSchema.Constants.PK} FROM {AccTransactionComplianceReportQueueSchema.Constants.SqlSchemaName}.{AccTransactionComplianceReportQueueSchema.Constants.TableName}";
			var queueRecords = new DynamicBusinessObjectCollection(Factory);
			queueRecords.Load(sql);
			return queueRecords.Select(x => (ZGuid)x[AccTransactionComplianceReportQueueSchema.Constants.PK]).ToArray();
		}

		void CreateAndSaveOpeningAndClosingGLJournal()
		{
			var list = new GLPresentationJournalCategoryCollection();
			var category1 = list.AddNew();
			category1.Code = "CLS";
			category1.Description = (NoResString)"Category Closing";
			category1.Bool = true; // active
			category1.Bool2 = false; // elimination
			category1.Bool3 = true; // closing
			category1.Bool4 = false; // opening
			var category2 = list.AddNew();
			category2.Code = "OPN";
			category2.Description = (NoResString)"Category Opening";
			category2.Bool = true; // active
			category2.Bool2 = false; // elimination
			category2.Bool3 = false; // closing
			category2.Bool4 = true; // opening
			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			var openingJournal = Creator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));
			openingJournal.AH_TransactionCategory = "OPN";
			var line1_1 = Creator.CreateGLJournalLine(openingJournal, 10, DebitCredit.CR, Creator.GLHeader1.PK);
			var line1_2 = Creator.CreateGLJournalLine(openingJournal, 10, DebitCredit.DR, Creator.GLHeader2.PK);

			var closingJournal = Creator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));
			closingJournal.AH_TransactionCategory = "CLS";
			var line2_1 = Creator.CreateGLJournalLine(closingJournal, 30, DebitCredit.CR, Creator.GLHeader1.PK);
			var line2_2 = Creator.CreateGLJournalLine(closingJournal, 30, DebitCredit.DR, Creator.GLHeader2.PK);
		}

		enum PaymentOption { notPaid, PartiallyPaid, FullyPaid }

		APInvoice CreateAPInvoiceForCreditor(string transactionNum, OrgHeader creditor, PaymentOption payment = PaymentOption.notPaid, ZDateTime? paymentDate = null)
		{
			var result = Creator.CreateAPInvoice<APInvoice>(transactionNum, Creator.AUD, 1m, 100m, 10m, 0m, 100m, 10m, 0m);
			result.AH_OH = creditor.PK;

			if (payment == PaymentOption.FullyPaid)
			{
				Creator.CreateAndMatchAPPaymentForAPInvoice(result, paymentDate ?? ZDateTime.Today);
			}
			else if (payment == PaymentOption.PartiallyPaid)
			{
				Creator.CreateAndMatchAPPaymentForAPInvoice(result, paymentDate ?? ZDateTime.Today, -55m);
			}

			return result;
		}

		APCreditNote CreateAndPayAPCreditNotaForCreditor(string transactionNum, OrgHeader creditor)
		{
			var result = Creator.CreateAPCreditNote(transactionNum, creditor, Creator.AUD, 1m, "Credit Note");
			var line = (APCreditNoteLine)result.Lines.AddNew();
			line.AL_AG = Creator.GLHeader1.PK;
			result.AH_OSTotal = line.AL_LineAmount = line.AL_OSAmount = 100m;
			Creator.CreateAndMatchAPPaymentForAPInvoice(result, ZDateTime.Today);
			return result;
		}

		ARInvoice CreateARInvoiceForDebtor(string transactionNum, string transactionType, OrgHeader debtor, RefCurrency currency, decimal netAmount, AccTaxRate taxRate)
		{
			var invoice = Creator.CreateARInvoice<ARInvoice>(transactionNum, currency, 1m, debtor);
			invoice.AH_TransactionType = transactionType;
			var line = Creator.CreateInvoiceLine(invoice, currency, 1m, netAmount, netAmount, 0m, netAmount, netAmount, 0m);
			line.AL_AT = taxRate.PK;

			return invoice;
		}

		int GetNoOfQueueRows(string whereClause = null)
		{
			int result = 0;
			var sql = @"select count(*) from dbo.AccTransactionComplianceReportQueue ";
			if (whereClause != null)
			{
				sql += whereClause;
			}
			var cmd = ((IDbConnected)Factory).Connection.Command(sql);

			object count = cmd.ExecuteScalar();
			if (count != null)
			{
				result = (int)count;
			}

			return result;
		}

		TestObjectCreator Creator => creator ?? (creator = new TestObjectCreator(Factory));
		TestObjectCreator creator;

		ComplianceReportConfigurationCollection CreateZMReportConfiguration()
		{
			var configurations = new ComplianceReportConfigurationCollection(Factory);
			var config = configurations.AddNew();
			config.Country = GlbCompany.CurrentCompany.Country.Code;
			config.ReportCode = "ZMD";
			config.ReportBaseTablePrefix = BaseTablePrefixListCodes.TransactionLine;
			config.ReportLineGrouping = GroupingCodes.TransactionHeaderAndReportSubCode;
			config.ReportTitle = $"Test Report with '{config.ReportBaseTablePrefix}' Base Table Prefix and '{config.ReportLineGrouping}' grouping code supported by the Service Task";
			config.TaxRegistrationType = "GCR";
			config.ReportPeriodicity = PeriodicityCodes.DateRange;

			AccountingMasterFilesRegistry.Instance.ComplianceReportConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurations);
			return configurations;
		}

		OrgHeader CreateDebtorWithBusinessRegNo(string debtorCode, string country, string businessRegType, string businessRegNo)
		{
			var debtor = Creator.CreateOrgHeader(debtorCode, false, true, false, false, true, false, true);
			if (!string.IsNullOrEmpty(businessRegNo))
			{
				debtor.CustomsCodes.AddNew(businessRegType, businessRegNo, country);
			}
			debtor.Addresses.MainAddress.OA_RN_NKCountryCode = country;
			return debtor;
		}

		AccComplianceReport CreateIDEAReportWithTwelvePeriodsAndQueueRecords()
		{
			var germanCompany = Creator.CreateNewCompany("DE1", Core.Constants.CountryCodes.Germany, orgProxy: Creator.ZECTRA);
			Creator.CreateReportConfiguration(germanCompany, Core.Constants.CountryCodes.Germany, AccComplianceReport.ReportTypes.IDEA, BaseTablePrefixListCodes.AllTransactions, PeriodicityCodes.DateRange);
			var report = Factory.NewWithValidTestData<AccComplianceReport>();
			report.ACR_GC_Company = germanCompany.PK;
			report.ACR_ReportType = AccComplianceReport.ReportTypes.IDEA;
			report.ACR_Status = AccComplianceReport.Status.ReportDataQueued;
			report.ACR_Periodicity = PeriodicityCodes.DateRange;
			report.ACR_DateFrom = new ZDate(2021, 1, 1);
			report.ACR_DateTo = new ZDate(2021, 12, 31);

			var invoices = new ARInvoice[12];
			for (var period = 1; period <= invoices.Length; period++)
			{
				invoices[period - 1] = CreateARInvoiceForDebtor($"AR{period:n4}", TransactionTypes.Invoice, Creator.Debtor, Creator.EUR, period * 100, Creator.REV);
				invoices[period - 1].AH_PostDate = new ZDateTime(2021, period, 15);
			}
			Factory.Save();

			for (var period = 1; period <= invoices.Length; period++)
			{
				Creator.CreateComplianceReportQueueEntry(report, invoices[period - 1]);
			}

			return report;
		}

		ZString GetEmbeddedResourceAsZString(string filename)
		{
			var filecontent = ZString.Empty;
			using (var stream = GetType().Assembly.GetManifestResourceStream("Enterprise.Accounting.ServiceTasks.Testing.ComplianceReport.TestFiles." + filename))
			{
				using (var sr = new StreamReader(stream))
				{
					filecontent = sr.ReadToEnd();
				}
			}
			return filecontent;
		}

		#endregion
	}
}

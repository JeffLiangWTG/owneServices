using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(GeneralLedgerDataBacklogProcessServiceTask))]
	class GeneralLedgerDataBacklogProcessServiceTaskTest : ServiceTaskTestCase<GeneralLedgerDataBacklogProcessServiceTask>
	{
		public void TestServiceTaskDisabled()
		{
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			ProcessServiceTask.RunTask(CancellationToken.None);
			var logger = (LoggerForTest)ProcessServiceTask.ServiceLogger;

			AssertNotContains($"Logs should contain disabled message", $@"Related feature is not enabled. The General Ledger Data Backlog Process Service Task will not start.", logger.ToString(), true);
		}

		#region TestServiceTaskIsRegistered

		public void TestServiceTaskIsRegistered()
		{
			var assembly = typeof(GeneralLedgerDataBacklogProcessServiceTask).Assembly;
			var attributes = assembly.GetCustomAttributes(typeof(HostedServiceAttribute), false);
			var result = attributes.Cast<HostedServiceAttribute>().Count(attribute => attribute.Code == "GLP");
			AssertEquals(1, result);
		}

		#endregion

		#region TestHostedServiceAttributes

		public void TestHostedServiceAttribute()
		{
			var attribute = typeof(GeneralLedgerDataBacklogProcessServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>().Single(x => x.Code == GeneralLedgerDataBacklogProcessServiceTask.Code);
			CombineAssertions(() =>
			{
				AssertEquals("IsMandatory", true, attribute.IsMandatory);
				AssertEquals("ActiveByDefault", true, attribute.ActiveByDefault);
			});
		}

		#endregion

		[TestDate(2023, 2, 2)]
		public void TestProcessQueueDataForMultipleCompanies()
		{
			var newCompany = TestObjectCreator.CreateNewCompany("ABC");
			newCompany.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;
			var branch = TestObjectCreator.CreateNewBranch(newCompany, "BRH");

			TestObjectCreator.CreateTestPeriodsForEntireYear(newCompany, 2023);
			TestObjectCreator.CreateJournal<ARJournal>(100m, new ZDateTime(2023, 01, 02), TestObjectCreator.Debtor.PK);
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var newFactory = new BusinessObjectFactory();
				newFactory.NewWithValidTestData<ARJournal>();
				newFactory.Save();
			}

			using (Env.Instance.TemporaryServiceTaskContext(GeneralLedgerDataBacklogQueueServiceTask.Code, canRunInAnyBranch: true))
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				QueueServiceTask.RunTask(CancellationToken.None);
			}
			using (Env.Instance.TemporaryServiceTaskContext(GeneralLedgerDataBacklogProcessServiceTask.Code, canRunInAnyBranch: true))
			{
				ProcessServiceTask.RunTask(CancellationToken.None);
			}

			AssertAccGeneralLedgerDataTableRowsCount(2);

			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(newCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1));
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetValue(newCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 1));

			QueueServiceTask.RunTask(CancellationToken.None);
			var newCompanyServiceTask = new GeneralLedgerDataBacklogProcessServiceTask();
			InitialiseAndRunTaskSchedule(newCompanyServiceTask);
			AssertLogs((TestServiceLogger)newCompanyServiceTask.ServiceLogger
				, "Debug|General Ledger Data Backlog Process Start."
				, "Debug|General ledger data backlog process start: EDI."
				, "Debug|General ledger data backlog process end: EDI."
				, "Debug|General ledger data backlog process start: ABC."
				, "Debug|General ledger data backlog process end: ABC."
				, "Information|General ledger data backlog process completed for 2 companies."
				, "Debug|General Ledger Data Backlog Process End."
			);

			AssertAccGeneralLedgerDataTableRowsCount(4);
			AssertAccTransactionPostingToGLDQueueTableRowsCount(0);
		}

		[TestDate(2023, 2, 2)]
		public void TestRunServiceTaskWithException()
		{
			TestObjectCreator.CreateJournal<ARJournal>(100m, new ZDateTime(2023, 01, 02), TestObjectCreator.Debtor.PK);
			Factory.Save();

			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();
			var mockDataRecover = CreateMockGeneralLedgerDataProcessor();
			mockDataRecover
				.Setup(x => x.ProcessData(It.IsAny<DataRow[]>()))
				.Callback<DataRow[]>(
					(dataRow) => throw new Exception("Dummy Exception!")
				);

			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			using (ObjectFactory.Substitute(mockDataRecover.Object))
			{
				QueueServiceTask.RunTask(CancellationToken.None);

				var serviceTask = new GeneralLedgerDataBacklogProcessServiceTask();
				InitialiseAndRunTaskSchedule(serviceTask);
				AssertLogs((TestServiceLogger)serviceTask.ServiceLogger
					, "Debug|General Ledger Data Backlog Process Start."
					, "Debug|General ledger data backlog process start: EDI."
					, "Error|Dummy Exception!"
					, "Debug|General ledger data backlog process end: EDI."
					, "Information|General ledger data backlog process completed for 1 companies."
					, "Debug|General Ledger Data Backlog Process End."
				);

				AssertEquals("Last processed general entry date should be empty.", DateTime.MinValue, AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

				mockDataRecover.Verify(x => x.ProcessData(It.IsAny<DataRow[]>())
					, Times.Exactly(1));

				mockServiceTaskNudger.Verify(x => x.NudgeServiceTask("GLP", null), Times.Exactly(1));
			}
		}

		[TestDate(2023, 2, 2)]
		[SuspendCriticalValidation]
		public void TestProcessQueueDataForMissingLines()
		{
			var invoiceDate = new DateTime(2023, 03, 01);
			var glStdJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, invoiceDate, new DateTime(2023, 01, 05), invoiceDate);
			TestObjectCreator.CreateGLJournalLine(glStdJournal, 20m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(glStdJournal, 20m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);

			var glNoteJournal = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, invoiceDate, new DateTime(2023, 01, 05), invoiceDate);
			TestObjectCreator.CreateGLJournalLine(glNoteJournal, 10m, DebitCredit.DR, TestObjectCreator.GLHeaderNTE1.PK);
			TestObjectCreator.CreateGLJournalLine(glNoteJournal, 10m, DebitCredit.CR, TestObjectCreator.GLHeaderNTE2.PK);

			Factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				QueueServiceTask.RunTask(CancellationToken.None);
			}

			glStdJournal.Lines[0].Delete();
			glStdJournal.Lines[0].Delete();
			Factory.Save();

			ProcessServiceTask.RunTask(CancellationToken.None);
			AssertAccGeneralLedgerDataTableRowsCount(2);
			AssertAccTransactionPostingToGLDQueueTableRowsCount(0);
		}

		[TestDate(2023, 2, 2)]
		public void TestProcessQueueDataMutipleRuns()
		{
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());

			var creditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD1", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader2.PK, "FIN");
			creditNote.AH_PostDate = new ZDateTime(2022, 02, 01);

			Factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				QueueServiceTask.RunTask(CancellationToken.None);
				ProcessServiceTask.RunTask(CancellationToken.None);

				AssertAccGeneralLedgerDataTableRowsCount(0);
				AssertEquals("Last processed general entry date should be same as generate journal entries start date.", new DateTime(2023, 1, 1), AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

				QueueServiceTask.RunTask(CancellationToken.None);
				AssertAccTransactionPostingToGLDQueueTableRowsCount(2);
				ProcessServiceTask.RunTask(CancellationToken.None);

				AssertAccGeneralLedgerDataTableRowsCount(6);
				AssertAccTransactionPostingToGLDQueueTableRowsCount(0);
				AssertEquals("Last processed general entry date should be same as generate journal entries start date.", new DateTime(2022, 1, 1), AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		[TestDate(2023, 2, 2)]
		public void TestProcessQueueDataWithoutGLQ()
		{
			var companyCode = Env.CurrentCompany.Code;
			var serviceTaskNudgerMock = new Mock<IServiceTaskNudger>();
			var shouldSkipGLQ = false;
			serviceTaskNudgerMock.Setup(x => x.NudgeServiceTask("GLQ", null))
				.Callback(() =>
				{
					if (!shouldSkipGLQ)
					{
						QueueServiceTask.RunTask(CancellationToken.None);
					}
				});

			TestObjectCreator.CreateJournal<ARJournal>(100m, new ZDateTime(2023, 01, 02), TestObjectCreator.Debtor.PK);
			Factory.Save();

			using (ObjectFactory.Substitute(serviceTaskNudgerMock.Object))
			{
				ProcessServiceTask.ServiceLogger = null;
				var logger = InitialiseTaskSchedule(ProcessServiceTask);

				ProcessServiceTask.RunTask(CancellationToken.None);

				AssertAccGeneralLedgerDataTableRowsCount(0);
				serviceTaskNudgerMock.Verify(x => x.NudgeServiceTask("GLQ", null), Times.Exactly(1));

				shouldSkipGLQ = true;
				ProcessServiceTask.RunTask(CancellationToken.None);

				AssertAccGeneralLedgerDataTableRowsCount(2);
				serviceTaskNudgerMock.Verify(x => x.NudgeServiceTask("GLQ", null), Times.Exactly(2));

				shouldSkipGLQ = false;
				ProcessServiceTask.RunTask(CancellationToken.None);

				serviceTaskNudgerMock.Verify(x => x.NudgeServiceTask("GLQ", null), Times.Exactly(3));

				ProcessServiceTask.RunTask(CancellationToken.None);

				serviceTaskNudgerMock.Verify(x => x.NudgeServiceTask("GLQ", null), Times.Exactly(3));
				AssertLogs(logger
					, "Debug|General Ledger Data Backlog Process Start."
					, $"Debug|Skip processing company: {companyCode} because this company has never run GLQ task before."
					, "Debug|Service Task GLQ has been nudged."
					, "Debug|General Ledger Data Backlog Process End."
					, "Debug|General Ledger Data Backlog Process Start."
					, $"Debug|General ledger data backlog process start: {companyCode}."
					, $"Debug|General ledger data backlog process end: {companyCode}."
					, "Information|General ledger data backlog process completed for 1 companies."
					, "Debug|Service Task GLQ has been nudged."
					, "Debug|General Ledger Data Backlog Process End."
					, "Debug|General Ledger Data Backlog Process Start."
					, $"Debug|Company: {companyCode} failed to run because Last Queued Date: 2023-01-01 is not equal to Generate Journal Entries Start Date: 2022-01-01."
					, "Debug|Service Task GLQ has been nudged."
					, "Debug|General Ledger Data Backlog Process End."
					, "Debug|General Ledger Data Backlog Process Start."
					, $"Debug|General ledger data backlog process start: {companyCode}."
					, $"Debug|General ledger data backlog process end: {companyCode}."
					, "Information|General ledger data backlog process completed for 1 companies."
					, "Debug|General Ledger Data Backlog Process End.");
			}
		}

		[TestDate(2023, 5, 11)]
		public void TestServiceTaskReloadCache()
		{
			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();
			var companyCode = Env.CurrentCompany.Code;
			var cachedDate = new DateTime(2022, 01, 01);
			var actualDate = new DateTime(2023, 01, 01);

			AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new DateTime(2023, 01, 01));
			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, cachedDate);
			AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, cachedDate);
			AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, cachedDate);

			UpdateDateRegistryDirectly(AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.Name, actualDate, Env.CurrentCompanyPK);
			UpdateDateRegistryDirectly(AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.Name, actualDate, Env.CurrentCompanyPK);
			UpdateDateRegistryDirectly(AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.Name, DateTime.MinValue, Env.CurrentCompanyPK);

			AssertEquals(cachedDate, AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals(cachedDate, AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals(cachedDate, AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));

			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			{
				var serviceTask = new GeneralLedgerDataBacklogProcessServiceTask();
				InitialiseAndRunTaskSchedule(serviceTask);

				AssertLogs((TestServiceLogger)serviceTask.ServiceLogger
					, "Debug|General Ledger Data Backlog Process Start."
					, $"Debug|General ledger data backlog process start: {companyCode}."
					, $"Debug|General ledger data backlog process end: {companyCode}."
					, "Information|General ledger data backlog process completed for 1 companies."
					, "Debug|Service Task GLQ has been nudged."
					, "Debug|General Ledger Data Backlog Process End."
				);
			}

			AssertEquals(actualDate.AddYears(-1), AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals(actualDate, AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals(actualDate, AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
		}

		void UpdateDateRegistryDirectly(string registryName, DateTime value, Guid companyPK)
		{
			var sql = string.Format(CultureInfo.InvariantCulture,
				"UPDATE dbo.StmData SET SD_BinaryValue = CONVERT(varbinary(max), N'{0}') WHERE SD_Name = '{1}' and SD_Owner = '{2}';",
				value, registryName, companyPK);
			Db.Connection.ExecuteNonQuery(sql);
		}

		#region Test for all type of transactions

		[TestDate(2023, 2, 2)]
		[SuspendCriticalValidation]
		public void TestProcessQueueDataForOnlyHeaders()
		{
			var date = new ZDateTime(2023, 02, 01);

			TestObjectCreator.CreateJournal<ARJournal>(100m, date, TestObjectCreator.Debtor.PK);
			TestObjectCreator.CreateTransfer<ARTransfer>(100m, date, TestObjectCreator.Debtor.PK, TestObjectCreator.Creditor1.PK);
			TestObjectCreator.CreateARReceipt(1m, 100m, date, date, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			TestObjectCreator.CreateARPayment(1m, 100m, date, date, TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			TestObjectCreator.CreateARDiscount(100, date, TestObjectCreator.Debtor.PK);
			TestObjectCreator.CreateOverpayment<AROverpayment>(100m, date, TestObjectCreator.Debtor.PK);
			TestObjectCreator.CreateExchangeDifference<ARExchangeDifference>(100m, date, TestObjectCreator.Debtor.PK);

			TestObjectCreator.CreateJournal<APJournal>(100m, date, TestObjectCreator.Creditor1.PK);
			TestObjectCreator.CreateTransfer<APTransfer>(100m, date, TestObjectCreator.Creditor1.PK, TestObjectCreator.Debtor.PK);
			TestObjectCreator.CreateAPReceipt(1m, 100m, date, date, TestObjectCreator.Creditor1.PK, TestObjectCreator.AUDBankAccount.PK);
			TestObjectCreator.CreateAPPayment(1m, 100m, date, date, TestObjectCreator.Creditor1.PK, TestObjectCreator.AUDBankAccount.PK);
			TestObjectCreator.CreateAPDiscount(100, date, TestObjectCreator.Creditor1.PK);
			TestObjectCreator.CreateOverpayment<APOverpayment>(100m, date, TestObjectCreator.Creditor1.PK);
			TestObjectCreator.CreateExchangeDifference<APExchangeDifference>(100m, date, TestObjectCreator.Creditor1.PK);

			TestObjectCreator.CreateContra(100m, date, TestObjectCreator.Debtor.PK, TestObjectCreator.Creditor1.PK);

			TestObjectCreator.CreateBankTransfer(date, TestObjectCreator.CHNBankAccount.PK, TestObjectCreator.AUDBankAccount.PK, 100m, 2m);
			TestObjectCreator.AUDBankAccount.Factory.Save();
			TestObjectCreator.CreateCashbookExchangeDifference(date, 100m, TestObjectCreator.AUDBankAccount, false);

			Factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				QueueServiceTask.RunTask(CancellationToken.None);
			}

			ProcessServiceTask.RunTask(CancellationToken.None);

			AssertAccGeneralLedgerDataTableRowsCount(34);
			AssertAccTransactionPostingToGLDQueueTableRowsCount(0);
		}

		[TestDate(2023, 2, 2)]
		public void TestProcessQueueDataForWIPACR()
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var date = new ZDateTime(2023, 02, 01);

				var accrual1 = TestObjectCreator.CreateAccrual();
				accrual1.AL_PostDate = date;
				accrual1.AL_LineAmount = accrual1.AL_OverseasTotal = 10m;

				var accrual2 = TestObjectCreator.CreateAccrual();
				accrual2.AL_PostDate = date;
				accrual2.Reverse();
				accrual2.AL_ReverseDate = new ZDateTime(2022, 02, 01);

				var wip1 = TestObjectCreator.CreateWIP();
				wip1.AL_PostDate = ZDateTime.Empty;

				Factory.Save();

				QueueServiceTask.RunTask(CancellationToken.None);
				AssertAccTransactionPostingToGLDQueueTableRowsCount(4);
				ProcessServiceTask.RunTask(CancellationToken.None);

				AssertAccGeneralLedgerDataTableRowsCount(8);
				AssertAccTransactionPostingToGLDQueueTableRowsCount(0);

				var wip2 = TestObjectCreator.CreateWIP();
				wip2.AL_PostDate = date;
				wip2.Reverse();
				wip2.AL_ReverseDate = new ZDateTime(2022, 02, 01);

				Factory.Save();

				QueueServiceTask.RunTask(CancellationToken.None);
				AssertAccTransactionPostingToGLDQueueTableRowsCount(2);
				ProcessServiceTask.RunTask(CancellationToken.None);

				AssertAccGeneralLedgerDataTableRowsCount(12);
				AssertAccTransactionPostingToGLDQueueTableRowsCount(0);
				AssertEquals("Last processed general entry date should be same as generate journal entries start date.", new DateTime(2022, 1, 1), AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		[TestDate(2023, 2, 2)]
		public void TestProcessQueueDataForREVCST()
		{
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());

			var date1 = new ZDateTime(2023, 02, 01);
			var date2 = new ZDateTime(2022, 02, 01);

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader2.PK, "FIN");
			arInvoice.AH_PostDate = date1;

			var creditNote = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD1", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader2.PK, "FIN");
			creditNote.AH_PostDate = date2;

			TestObjectCreator.CreateInvoiceWithLine(typeof(ARAdjustmentNote), "ADJ1", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader2.PK, "FIN");

			var apInvoiceWithoutReverseDate = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV2", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader2.PK, "FIN");
			apInvoiceWithoutReverseDate.AH_PostDate = ZDateTime.Empty;

			TestObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "CRD2", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader2.PK, "FIN");
			TestObjectCreator.CreateInvoiceWithLine(typeof(APAdjustmentNote), "ADJ2", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader2.PK, "FIN");

			var jcJournal1 = TestObjectCreator.CreateJCJournalHeader(date1, 100m);
			jcJournal1.AH_TransactionType = TransactionTypes.Journal;
			TestObjectCreator.CreateJCJournalLine(jcJournal1, TestObjectCreator.CC1, null, date1, 100m);

			var job1 = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0m, TestObjectCreator.Agent, 1.0m);
			var jcJournal2 = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job1, 250m);
			jcJournal2.AH_PostDate = date1;

			Factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				QueueServiceTask.RunTask(CancellationToken.None);
				ProcessServiceTask.RunTask(CancellationToken.None);

				AssertAccGeneralLedgerDataTableRowsCount(36);
				AssertAccTransactionPostingToGLDQueueTableRowsCount(0);
				AssertEquals("Last processed general entry date should be same as generate journal entries start date.", new DateTime(2023, 1, 1), AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

				QueueServiceTask.RunTask(CancellationToken.None);
				AssertAccTransactionPostingToGLDQueueTableRowsCount(2);
				ProcessServiceTask.RunTask(CancellationToken.None);

				AssertAccGeneralLedgerDataTableRowsCount(42);
				AssertAccTransactionPostingToGLDQueueTableRowsCount(0);
				AssertEquals("Last processed general entry date should be same as generate journal entries start date.", new DateTime(2022, 1, 1), AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
			}
		}

		[TestDate(2023, 2, 2)]
		[SuspendCriticalValidation]
		public void TestProcessQueueDataForCashBasisVAT()
		{
			AccountingConfigurationRegistry.Instance.GSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.SetValue(Guid.Empty, Guid.Empty,
				Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			var apInvoice = (APInvoice)TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 100, 10, 100, 10);
			apInvoice.AH_FullyPaidDate = ZDateTime.Now;
			var line = apInvoice.Lines[0];
			line.AL_AT = TestObjectCreator.GST1.PK;
			line.AL_GSTVATBasis = AccountingMasterFilesConstants.TransactionLineTaxBasisTypes.Cash.Code;
			TestObjectCreator.CreateCashBasisVAT(line, -90, -9);
			Factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				QueueServiceTask.RunTask(CancellationToken.None);
			}
			ProcessServiceTask.RunTask(CancellationToken.None);

			AssertAccGeneralLedgerDataTableRowsCount(2, gLDTypeCode: AccountingConstants.GLDTypeCodes.RealizeCashBasisVAT);
			AssertAccTransactionPostingToGLDQueueTableRowsCount(0);
			AssertEquals(new DateTime(2023, 1, 1), AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.Value);
		}

		[TestDate(2023, 2, 2)]
		[SuspendCriticalValidation]
		public void TestProcessQueueDataForTaxGlMovement()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARNV1", TestObjectCreator.AUD, 1m, 100m, 10m, 100m, 10m);
			invoice.Lines[0].AL_AG = TestObjectCreator.GLHeader2.PK;
			var taxRecord = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxRecord.ATT_Ledger = TaxConfigurationLedgers.AccountsReceivable.Code;
			taxRecord.ATT_PostDate = new ZDate(2023, 3, 2);
			taxRecord.ATT_AG_LedgerControlAccount = TestObjectCreator.GLHeader1.PK;
			taxRecord.ATT_AG_TaxExpenseAccount = TestObjectCreator.GLHeader2.PK;
			taxRecord.ATT_AH = invoice.PK;
			taxRecord.ATT_Basis = TaxBasisList.Posting.Code;
			taxRecord.ATT_Rate = 1;

			var pivots1 = Factory.Load<AccTaxRecordTransactionLinePivot>(new ZQuery(AccTaxRecordTransactionLinePivotSchema.ATP_ATT, taxRecord.PK) { FetchOnlyFromLocalCache = true });
			pivots1[0].ATP_IsTaxExpense = true;

			Factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				QueueServiceTask.RunTask(CancellationToken.None);
			}

			ProcessServiceTask.RunTask(CancellationToken.None);

			AssertAccGeneralLedgerDataTableRowsCount(2, gLDTypeCode: AccountingConstants.GLDTypeCodes.RealizeTaxGLMovement);
			AssertAccTransactionPostingToGLDQueueTableRowsCount(0);
			AssertEquals(new DateTime(2023, 1, 1), AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		[TestDate(2023, 2, 2)]
		public void TestProcessQueueDataForDPYDRC()
		{
			var date = new ZDateTime(2023, 02, 01);

			TestObjectCreator.CreateDirectReceipt(date, 100, 10, 50, 5);
			TestObjectCreator.CreateDirectPayment(date, 100, 10, 50, 5);
			Factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				QueueServiceTask.RunTask(CancellationToken.None);
			}

			ProcessServiceTask.RunTask(CancellationToken.None);

			AssertAccGeneralLedgerDataTableRowsCount(16);
		}

		[TestDate(2023, 2, 2)]
		public void TestProcessQueueDataForGJLNJL()
		{
			var invoiceDate = new DateTime(2023, 03, 01);

			var glStdJournal1 = TestObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, invoiceDate, new DateTime(2023, 04, 05));
			AddGLLines(glStdJournal1);

			var glNoteJournal1 = TestObjectCreator.CreateGLJournal(TransactionTypes.GLNoteJournal, invoiceDate, new DateTime(2023, 05, 05));
			AddGLLines(glNoteJournal1);

			Factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				QueueServiceTask.RunTask(CancellationToken.None);
			}

			ProcessServiceTask.RunTask(CancellationToken.None);

			AssertAccGeneralLedgerDataTableRowsCount(4);
			AssertEquals("Last processed general entry date should be same as generate journal entries start date.", new DateTime(2023, 1, 1), AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		[TestDate(2023, 2, 2)]
		public void TestProcessQueueDataForAJLRJL()
		{
				var invoiceDate = new DateTime(2023, 03, 01);
				var glReverseJournal1 = TestObjectCreator.CreateGLJournal(TransactionTypes.GLReversingJournal, invoiceDate, new DateTime(2023, 01, 05), invoiceDate);
				AddGLLines(glReverseJournal1);

				var glAutoJournal1 = TestObjectCreator.CreateGLJournal(TransactionTypes.GLAutoJournal, invoiceDate, new DateTime(2023, 01, 05), invoiceDate);
				AddGLLines(glAutoJournal1);

				Factory.Save();

				using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
				{
					QueueServiceTask.RunTask(CancellationToken.None);
				}

				ProcessServiceTask.RunTask(CancellationToken.None);

				AssertAccGeneralLedgerDataTableRowsCount(10);
				AssertEquals("Last processed general entry date should be same as generate journal entries start date.", new DateTime(2023, 1, 1), AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		[TestDate(2023, 2, 22)]
		public void TestProcessQueueDataForOnlyHeaders_LastQueuedDateShouldEqualToStartDate()
		{
			TestObjectCreator.CreateARReceipt(1m, 100m, new ZDateTime(2023, 03, 01), new ZDateTime(2023, 03, 01), TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);
			TestObjectCreator.CreateARReceipt(1m, 100m, new ZDateTime(2022, 03, 01), new ZDateTime(2022, 03, 01), TestObjectCreator.Debtor.PK, TestObjectCreator.AUDBankAccount.PK);

			Factory.Save();

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				QueueServiceTask.RunTask(CancellationToken.None);
				ProcessServiceTask.RunTask(CancellationToken.None);

				var firstStartDate = new DateTime(2023, 1, 1);

				AssertEquals("Last queued date should be start date.", firstStartDate, AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.Value);
				AssertEquals("Last processed date should be start date.", firstStartDate, AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.Value);
				AssertAccGeneralLedgerDataTableRowsCount(2);

				var secondStartDate = new DateTime(2022, 1, 1);

				ProcessServiceTask.RunTask(CancellationToken.None);

				AssertEquals("Last processed date should be 01-JAN-2023.", firstStartDate, AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.Value);
				AssertAccGeneralLedgerDataTableRowsCount(2);

				QueueServiceTask.RunTask(CancellationToken.None);
				ProcessServiceTask.RunTask(CancellationToken.None);

				AssertEquals("Last queued date should be start date.", secondStartDate, AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.Value);
				AssertEquals("Last processed date should be start date.", secondStartDate, AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.Value);
				AssertAccGeneralLedgerDataTableRowsCount(4);
			}
		}

		void AddGLLines(GLJournal gLJournal)
		{
			TestObjectCreator.CreateGLJournalLine(gLJournal, 20m, DebitCredit.DR, TestObjectCreator.GLHeader1.PK);
			TestObjectCreator.CreateGLJournalLine(gLJournal, 20m, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);
		}

		void AssertAccTransactionPostingToGLDQueueTableRowsCount(int expectedRowCount)
		{
			using (var command = Db.Connection.Command($@"SELECT count(1) FROM dbo.AccTransactionPostingToGLDQueue"))
			{
				AssertEquals($"Data count of AccTransactionPostingToGLDQueue", expectedRowCount, (int)command.ExecuteScalar());
			}
		}

		void AssertAccGeneralLedgerDataTableRowsCount(int expectedRowCount, string gLDTypeCode = null)
		{
			var query = new ZQuery();
			if (gLDTypeCode != null)
			{
				query.AddToFilter(AccGeneralLedgerDataSchema.GLD_Type, gLDTypeCode);
			}
			Factory.ClearQueryCache();
			var result = Factory.Load<AccGeneralLedgerData>(query);
			AssertEquals($"Data count of AccGeneralLedgerDataTable", expectedRowCount, result.Length);
		}

		#endregion

		#region Test GenerateAndStoreJournalEntriesForPostedAccountingTransactions

		[TestDate(2023, 2, 2)]
		public void TestUpgradeGenerateJournalEntriesStartDateToPreviousPeriodAccountYearStartDay()
		{
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.MinValue);
			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.MinValue);
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeaderNTE1.PK.ToGuid());

			var creditNote1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD1", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader2.PK, "FIN");
			creditNote1.AH_PostDate = new ZDateTime(2022, 02, 01);

			var creditNote2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "CRD2", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader2.PK, "FIN");
			creditNote2.AH_PostDate = new ZDateTime(2023, 02, 02);

			TestObjectCreator.DeleteAllPeriodsForCurrentCompany();

			Factory.Save();

			AssertTaskRun(0, 0, DateTime.MinValue);

			TestObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2022);
			TestObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2023);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(new DateTime(2023, 1, 1), AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			var secondDateTime = new DateTime(2022, 1, 1);

			AssertTaskRun(2, 6, secondDateTime);
			AssertTaskRun(2, 12, secondDateTime);
			AssertTaskRun(0, 12, secondDateTime);
		}

		void AssertTaskRun(int queueCount, int gLDCount, DateTime journalEntriesStartDate)
		{
			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				QueueServiceTask.RunTask(CancellationToken.None);
			}
			AssertAccTransactionPostingToGLDQueueTableRowsCount(queueCount);
			ProcessServiceTask.RunTask(CancellationToken.None);
			AssertAccGeneralLedgerDataTableRowsCount(gLDCount);
			AssertEquals(journalEntriesStartDate, AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));
		}

		#endregion

		void AssertLogs(TestServiceLogger logger, params string[] messages)
		{
			var index = 0;
			CombineAssertions(() =>
			{
				foreach (var message in messages)
				{
					AssertEquals(message, logger[index++]);
				}
			});
		}

		Mock<IGeneralLedgerDataProcessor> CreateMockGeneralLedgerDataProcessor()
		{
			var mockDataRecover = new Mock<IGeneralLedgerDataProcessor>();
			mockDataRecover.Setup(x => x.ProcessData(It.IsAny<DataRow[]>()))
				.Callback<DataRow[]>(
					(dataRow) => Assert("'ProcessData' should not be called.", false)
				);

			return mockDataRecover;
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			base.SetUpCore();

			TestObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2022);
			TestObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2023);
			TestObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			Factory.Save();

			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 1, 1));
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 1));

			QueueServiceTask.ServiceLogger = Logger;
			ProcessServiceTask.ServiceLogger = Logger;
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		readonly LoggerForTest Logger = new LoggerForTest();

		readonly GeneralLedgerDataBacklogQueueServiceTask QueueServiceTask = new GeneralLedgerDataBacklogQueueServiceTask();
		readonly GeneralLedgerDataBacklogProcessServiceTask ProcessServiceTask = new GeneralLedgerDataBacklogProcessServiceTask();
	}
}

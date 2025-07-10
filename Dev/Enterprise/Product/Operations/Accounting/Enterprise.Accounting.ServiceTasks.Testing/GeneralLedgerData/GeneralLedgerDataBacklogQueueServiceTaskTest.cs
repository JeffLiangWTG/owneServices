using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(GeneralLedgerDataBacklogQueueServiceTask))]
	class GeneralLedgerDataBacklogQueueServiceTaskTest : ServiceTaskTestCase<GeneralLedgerDataBacklogQueueServiceTask>
	{
		[TestDate(2023, 07, 23)]
		public void TestRunServiceTask()
		{
			var cdcStartDate20230523 = new DateTime(2023, 05, 23);
			var cdcStartDate20230601 = new DateTime(2023, 06, 01);
			var cdcStartDateToday20230723 = new DateTime(2023, 07, 23);
			AssertEquals("PreCondition", cdcStartDateToday20230723, ZDateTime.Today.Date);

			var companyEDI = GlbCompany.CurrentCompany;
			var companyDTR = TestObjectCreator.CreateNewCompany("DTR", CountryCodes.Turkey, orgProxy: TestObjectCreator.CreditorTR);
			var companyDDE = TestObjectCreator.CreateNewCompany("DDE", CountryCodes.Germany, orgProxy: TestObjectCreator.DebtorDE);
			Factory.Save();

			EnableGenerateJournalEntries(companyEDI
				, 2023
				, firstPeriodStartDate: new DateTime(2023, 01, 15)
				, cdcStartDate: cdcStartDate20230523);
			EnableGenerateJournalEntries(companyDTR
				, 2022
				, firstPeriodStartDate: new DateTime(2022, 01, 09)
				, lastQueueDate: new DateTime(2023, 01, 12)
			);
			EnableGenerateJournalEntries(companyDDE
				, 2023
				, firstPeriodStartDate: new DateTime(2023, 01, 18)
				, cdcStartDate: cdcStartDate20230601);

			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();
			var mockDataRecover = CreateMockGeneralLedgerDataQueue();

			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			using (ObjectFactory.Substitute(mockDataRecover.Object))
			{
				var serviceTask = new GeneralLedgerDataBacklogQueueServiceTask();
				InitialiseAndRunTaskSchedule(serviceTask);
				AssertLogs((TestServiceLogger)serviceTask.ServiceLogger
					, "Debug|General ledger data backlog queuing process starting."
					, "Debug|Start to queue transactions of company 'DDE'."
					, "Debug|Start to backlog queue to '2023-05-01'."
					, "Debug|Start to backlog queue to '2023-04-01'."
					, "Debug|Start to backlog queue to '2023-03-01'."
					, "Debug|Start to backlog queue to '2023-02-01'."
					, "Debug|Start to backlog queue to '2023-01-18'."
					, "Debug|Finish to queue transactions of company 'DDE'."
					, "Debug|Start to queue transactions of company 'DTR'."
					, "Debug|Start to backlog queue to '2023-01-01'."
					, "Debug|Start to backlog queue to '2022-12-01'."
					, "Debug|Start to backlog queue to '2022-11-01'."
					, "Debug|Start to backlog queue to '2022-10-01'."
					, "Debug|Start to backlog queue to '2022-09-01'."
					, "Debug|Start to backlog queue to '2022-08-01'."
					, "Debug|Start to backlog queue to '2022-07-01'."
					, "Debug|Start to backlog queue to '2022-06-01'."
					, "Debug|Start to backlog queue to '2022-05-01'."
					, "Debug|Start to backlog queue to '2022-04-01'."
					, "Debug|Start to backlog queue to '2022-03-01'."
					, "Debug|Start to backlog queue to '2022-02-01'."
					, "Debug|Start to backlog queue to '2022-01-09'."
					, "Debug|Finish to queue transactions of company 'DTR'."
					, "Debug|Start to queue transactions of company 'EDI'."
					, "Debug|Start to backlog queue to '2023-05-01'."
					, "Debug|Start to backlog queue to '2023-04-01'."
					, "Debug|Start to backlog queue to '2023-03-01'."
					, "Debug|Start to backlog queue to '2023-02-01'."
					, "Debug|Start to backlog queue to '2023-01-15'."
					, "Debug|Finish to queue transactions of company 'EDI'."
					, "Information|General ledger data backlog queuing process completed for 3 companies."
					, "Debug|Service Task GLP has been nudged."
				);

				AssertEquals("[companyEDI]Last queued general entry date."
					, new DateTime(2023, 01, 15)
					, GetJournalEntriesLastQueuedDate(companyEDI)
				);

				AssertEquals("[companyDTR]Last queued general entry date."
					, new DateTime(2022, 01, 09)
					, GetJournalEntriesLastQueuedDate(companyDTR)
				);

				AssertEquals("[companyDDE]Last queued general entry date."
					, new DateTime(2023, 01, 18)
					, GetJournalEntriesLastQueuedDate(companyDDE)
				);

				AssertRunServiceTask(
					expectedLastQueuedDatesForCompanyEDI: new[] {
						(NewValue: new DateTime(2023, 05, 01), LastValue: DateTime.MinValue)
						, (NewValue: new DateTime(2023, 04, 01), LastValue: new DateTime(2023, 05, 01))
						, (NewValue: new DateTime(2023, 03, 01), LastValue: new DateTime(2023, 04, 01))
						, (NewValue: new DateTime(2023, 02, 01), LastValue: new DateTime(2023, 03, 01))
						, (NewValue: new DateTime(2023, 01, 15), LastValue: new DateTime(2023, 02, 01))
					}
					, expectedLastQueuedDatesForCompanyDTR: new[] {
						 (NewValue: new DateTime(2023, 01, 01), LastValue: new DateTime(2023, 01, 12))
						, (NewValue: new DateTime(2022, 12, 01), LastValue: new DateTime(2023, 01, 01))
						, (NewValue: new DateTime(2022, 11, 01), LastValue: new DateTime(2022, 12, 01))
						, (NewValue: new DateTime(2022, 10, 01), LastValue: new DateTime(2022, 11, 01))
						, (NewValue: new DateTime(2022, 09, 01), LastValue: new DateTime(2022, 10, 01))
						, (NewValue: new DateTime(2022, 08, 01), LastValue: new DateTime(2022, 09, 01))
						, (NewValue: new DateTime(2022, 07, 01), LastValue: new DateTime(2022, 08, 01))
						, (NewValue: new DateTime(2022, 06, 01), LastValue: new DateTime(2022, 07, 01))
						, (NewValue: new DateTime(2022, 05, 01), LastValue: new DateTime(2022, 06, 01))
						, (NewValue: new DateTime(2022, 04, 01), LastValue: new DateTime(2022, 05, 01))
						, (NewValue: new DateTime(2022, 03, 01), LastValue: new DateTime(2022, 04, 01))
						, (NewValue: new DateTime(2022, 02, 01), LastValue: new DateTime(2022, 03, 01))
						, (NewValue: new DateTime(2022, 01, 09), LastValue: new DateTime(2022, 02, 01))
					}
					, expectedLastQueuedDatesForCompanyDDE: new[] {
						(NewValue: new DateTime(2023, 05, 01), LastValue: DateTime.MinValue)
						, (NewValue: new DateTime(2023, 04, 01), LastValue: new DateTime(2023, 05, 01))
						, (NewValue: new DateTime(2023, 03, 01), LastValue: new DateTime(2023, 04, 01))
						, (NewValue: new DateTime(2023, 02, 01), LastValue: new DateTime(2023, 03, 01))
						, (NewValue: new DateTime(2023, 01, 18), LastValue: new DateTime(2023, 02, 01))
					});

				mockServiceTaskNudger.Verify(x => x.NudgeServiceTask("GLP", null), Times.Exactly(1));
				AssertEquals("MustRollBack", false, Db.Connection.MustRollBack);
			}

			void AssertRunServiceTask(IEnumerable<(DateTime NewValue, DateTime LastValue)> expectedLastQueuedDatesForCompanyEDI
				, IEnumerable<(DateTime NewValue, DateTime LastValue)> expectedLastQueuedDatesForCompanyDTR
				, IEnumerable<(DateTime NewValue, DateTime LastValue)> expectedLastQueuedDatesForCompanyDDE)
			{
				var expectedAllQueueTransactionTimes = expectedLastQueuedDatesForCompanyEDI
					.Concat(expectedLastQueuedDatesForCompanyDTR)
					.Concat(expectedLastQueuedDatesForCompanyDDE)
					.Count();

				mockDataRecover.Verify(x => x.QueueTransaction(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>())
					, Times.Exactly(expectedAllQueueTransactionTimes));

				foreach (var expectedLastQueuedDate in expectedLastQueuedDatesForCompanyEDI)
				{
					mockDataRecover.Verify(x => x.QueueTransaction(Db.Connection, companyEDI.PK.ToGuid(), expectedLastQueuedDate.NewValue, expectedLastQueuedDate.LastValue, cdcStartDate20230523, false)
						, Times.Exactly(1)
					);
				}

				foreach (var expectedLastQueuedDate in expectedLastQueuedDatesForCompanyDTR)
				{
					mockDataRecover.Verify(
						x => x.QueueTransaction(Db.Connection, companyDTR.PK.ToGuid(), expectedLastQueuedDate.NewValue, expectedLastQueuedDate.LastValue, cdcStartDateToday20230723, false)
						, Times.Exactly(1)
					);
				}

				foreach (var expectedLastQueuedDate in expectedLastQueuedDatesForCompanyDDE)
				{
					mockDataRecover.Verify(
						x => x.QueueTransaction(Db.Connection, companyDDE.PK.ToGuid(), expectedLastQueuedDate.NewValue, expectedLastQueuedDate.LastValue, cdcStartDate20230601, false)
						, Times.Exactly(1)
					);
				}
			}
		}

		[TestDate(2023, 5, 15)]
		public void TestRunServiceTaskWithoutGenerateJournalEntries()
		{
			AssertEquals("PreCondition", DateTime.MinValue, AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty));

			var serviceTask = new GeneralLedgerDataBacklogQueueServiceTask();

			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();
			var mockDataRecover = CreateMockGeneralLedgerDataQueue();

			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			using (ObjectFactory.Substitute(mockDataRecover.Object))
			{
				InitialiseAndRunTaskSchedule(serviceTask);
				var logger = (TestServiceLogger)serviceTask.ServiceLogger;
				AssertNotContains($"Information|General ledger data backlog queuing process starting.", $@"Information|Related feature is not enabled. The General Ledger Data Backlog Queue Service Task will not start.", logger.ToString(), true);
				AssertEquals("Last queued general entry date should be empty.", DateTime.MinValue, GetJournalEntriesLastQueuedDate(GlbCompany.CurrentCompany));

				mockDataRecover.Verify(x => x.QueueTransaction(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>())
					, Times.Exactly(0));
				mockServiceTaskNudger.Verify(x => x.NudgeServiceTask("GLP", null), Times.Exactly(1));
				AssertEquals("MustRollBack", false, Db.Connection.MustRollBack);
			}
		}

		[TestDate(2023, 5, 11)]
		public void TestRunServiceTaskWithException()
		{
			EnableGenerateJournalEntries(GlbCompany.CurrentCompany, 2023);

			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();
			var mockDataRecover = CreateMockGeneralLedgerDataQueue();
			mockDataRecover
				.Setup(x => x.QueueTransaction(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>()))
				.Callback<DbConnection, Guid, DateTime, DateTime, DateTime, bool>(
					(connection, companyPK, startDate, endDate, endSystemCreateTime, checkDuplicated) => throw new Exception("Dummy Exception!")
				);

			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			using (ObjectFactory.Substitute(mockDataRecover.Object))
			{
				var serviceTask = new GeneralLedgerDataBacklogQueueServiceTask();
				InitialiseAndRunTaskSchedule(serviceTask);
				AssertLogs((TestServiceLogger)serviceTask.ServiceLogger
					, "Debug|General ledger data backlog queuing process starting."
					, "Debug|Start to queue transactions of company 'EDI'."
					, "Debug|Start to backlog queue to '2023-05-01'."
					, "Error|Fail to queue transactions of company 'EDI' - Dummy Exception!"
					, "Debug|Finish to queue transactions of company 'EDI'."
					, "Information|General ledger data backlog queuing process completed for 1 companies."
					, "Debug|Service Task GLP has been nudged."
				);

				AssertEquals("Last queued general entry date should be empty.", DateTime.MinValue, GetJournalEntriesLastQueuedDate(GlbCompany.CurrentCompany));

				mockDataRecover.Verify(x => x.QueueTransaction(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>())
					, Times.Exactly(1));

				mockDataRecover.Verify(x => x.QueueTransaction(Db.Connection, GlbCompany.CurrentCompany.PK.ToGuid(), new DateTime(2023, 05, 01), DateTime.MinValue, new DateTime(2023, 05, 11), false)
					, Times.Exactly(1)
				);

				mockServiceTaskNudger.Verify(x => x.NudgeServiceTask("GLP", null), Times.Exactly(1));
				AssertEquals("MustRollBack", true, Db.Connection.MustRollBack);
			}
		}

		[TestDate(2023, 2, 1)]
		public void TestServiceTaskDisabled()
		{
			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1m, 100m, 5m, 100m, 5m);
			Factory.Save();

			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();
			var mockDataRecover = CreateMockGeneralLedgerDataQueue();

			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			using (ObjectFactory.Substitute(mockDataRecover.Object))
			{
				var serviceTask = new GeneralLedgerDataBacklogQueueServiceTask();
				InitialiseAndRunTaskSchedule(serviceTask);

				var logger = (TestServiceLogger)serviceTask.ServiceLogger;
				AssertNotContains($"Information|General ledger data backlog queuing process starting.", $@"Information|Related feature is not enabled. The General Ledger Data Backlog Queue Service Task will not start.", logger.ToString(), true);

				AssertEquals("Last queued general entry date should be empty.", DateTime.MinValue, GetJournalEntriesLastQueuedDate(GlbCompany.CurrentCompany));

				mockDataRecover.Verify(x => x.QueueTransaction(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>())
					, Times.Exactly(0));
				mockServiceTaskNudger.Verify(x => x.NudgeServiceTask("GLP", null), Times.Exactly(0));
				AssertEquals("MustRollBack", false, Db.Connection.MustRollBack);
			}
		}

		[TestDate(2023, 5, 11)]
		public void TestServiceTaskReloadCache()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(GlbCompany.CurrentCompany, 2022);
			EnableGenerateJournalEntries(GlbCompany.CurrentCompany, 2023);

			var mockServiceTaskNudger = new Mock<IServiceTaskNudger>();
			var companyCode = Env.CurrentCompany.Code;
			var cachedDate = new DateTime(2022, 01, 01);
			var actualDate = new DateTime(2023, 01, 01);

			AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new DateTime(2023, 01, 01));
			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, cachedDate);
			AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, cachedDate);

			UpdateDateRegistryDirectly(AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.Name, actualDate, Env.CurrentCompanyPK);
			UpdateDateRegistryDirectly(AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.Name, DateTime.MinValue, Env.CurrentCompanyPK);

			AssertEquals(cachedDate, AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals(cachedDate, AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));

			using (ObjectFactory.Substitute(mockServiceTaskNudger.Object))
			{
				var serviceTask = new GeneralLedgerDataBacklogQueueServiceTask();
				InitialiseAndRunTaskSchedule(serviceTask);

				AssertLogs((TestServiceLogger)serviceTask.ServiceLogger
					, "Debug|General ledger data backlog queuing process starting."
					, $"Debug|Start to queue transactions of company '{companyCode}'."
					, "Debug|Start to backlog queue to '2023-05-01'."
					, "Debug|Start to backlog queue to '2023-04-01'."
					, "Debug|Start to backlog queue to '2023-03-01'."
					, "Debug|Start to backlog queue to '2023-02-01'."
					, "Debug|Start to backlog queue to '2023-01-01'."
					, $"Debug|Finish to queue transactions of company '{companyCode}'."
					, "Information|General ledger data backlog queuing process completed for 1 companies."
					, "Debug|Service Task GLP has been nudged."
				);
			}

			AssertEquals(actualDate, AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
			AssertEquals(actualDate, AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.GetValueWithoutFallback(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty));
		}

		void UpdateDateRegistryDirectly(string registryName, DateTime value, Guid companyPK)
		{
			var sql = string.Format(CultureInfo.InvariantCulture,
				"UPDATE dbo.StmData SET SD_BinaryValue = CONVERT(varbinary(max), N'{0}') WHERE SD_Name = '{1}' and SD_Owner = '{2}';",
				value, registryName, companyPK);
			Db.Connection.ExecuteNonQuery(sql);
		}

		Mock<IGeneralLedgerDataQueue> CreateMockGeneralLedgerDataQueue()
		{
			var mockDataRecover = new Mock<IGeneralLedgerDataQueue>();
			mockDataRecover.Setup(x => x.RemoveNarrowGLD(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
				.Callback<DbConnection, Guid, DateTime, DateTime>(
					(connection, companyPK, startDate, endDate) => Assert("'RemoveNarrowGLD' should not be called.", false)
				);

			mockDataRecover.Setup(x => x.QueueTransaction(It.IsAny<DbConnection>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<bool>()))
				.Callback<DbConnection, Guid, DateTime, DateTime, DateTime, bool>(
					(connection, companyPK, startDate, endDate, endSystemCreateTime, checkDuplicated) => Assert("IsInTransaction", connection.IsInTransaction)
				);
			return mockDataRecover;
		}

		DateTime GetJournalEntriesLastQueuedDate(GlbCompany company) => AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty);

		void EnableGenerateJournalEntries(GlbCompany company, int finacialYear, DateTime? firstPeriodStartDate = null, DateTime? lastQueueDate = null, DateTime? cdcStartDate = null)
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(company, finacialYear);
			Factory.Save();

			var periodCalculator = new AccountingPeriodCalculator(Factory, company);
			var firstPeriod = periodCalculator.GetFirstPeriodForYear(finacialYear);
			var firstPerioeManagementOfYear = periodCalculator.GetPeriodManagementFromDate(
				periodCalculator.GetFirstDayForPeriod(firstPeriod)
			);

			if (firstPeriodStartDate.HasValue)
			{
				firstPerioeManagementOfYear.AM_StartDate = firstPeriodStartDate.Value;
				Factory.Save();
			}

			AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, firstPerioeManagementOfYear.AM_StartDate.ToDateTime());

			if (cdcStartDate.HasValue)
			{
				AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, cdcStartDate.Value);
			}

			if (lastQueueDate.HasValue)
			{
				AccountingMasterFilesRegistry.Instance.JournalEntriesLastQueuedDate.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, lastQueueDate.Value);
			}
		}

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

		#region TestHostedServiceAttributes

		public void TestHostedServiceAttribute()
		{
			var attribute = typeof(GeneralLedgerDataBacklogQueueServiceTask).Assembly.GetCustomAttributes(true).OfType<HostedServiceAttribute>().Single(x => x.Code == GeneralLedgerDataBacklogQueueServiceTask.Code);
			CombineAssertions(() =>
			{
				AssertEquals("IsMandatory", true, attribute.IsMandatory);
				AssertEquals("ActiveByDefault", true, attribute.ActiveByDefault);
			});
		}

		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			base.SetUpCore();

			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			TestObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();

			Factory.Save();
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}

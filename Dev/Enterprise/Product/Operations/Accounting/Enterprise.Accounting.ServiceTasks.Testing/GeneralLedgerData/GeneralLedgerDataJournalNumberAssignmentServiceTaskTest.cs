using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(GeneralLedgerDataJournalNumberAssignmentServiceTask))]
	public class GeneralLedgerDataJournalNumberAssignmentServiceTaskTest : ServiceTaskTestCase<GeneralLedgerDataJournalNumberAssignmentServiceTask>
	{
		public void TestServiceTaskAssignJournalNumber()
		{
			AccountingConfigurationRegistry.Instance.AllocateJournalEntriesNumberStartDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new DateTime(2024, 11, 1));

			var accGeneralLedgerData = Factory.NewWithValidTestData<AccGeneralLedgerData>();
			accGeneralLedgerData.GLD_GC_Company = Env.CurrentCompanyPK;
			accGeneralLedgerData.GLD_PostDate = new ZDateTime(2024, 11, 1);
			accGeneralLedgerData.GLD_PostPeriod = 202411;
			accGeneralLedgerData.GLD_Type = "PST";
			accGeneralLedgerData.GLD_GLAccountType = "ARC";
			var header = Factory.NewWithValidTestData<AccTransactionHeader>();
			accGeneralLedgerData.GLD_AH_TransactionHeader = header.PK;

			Factory.Save();

			AssertRun(new DateTime(2024, 12, 1), shouldAssignJournalNumber: false, accGeneralLedgerData.PK);
			AssertRun(new DateTime(2024, 10, 1), shouldAssignJournalNumber: true, accGeneralLedgerData.PK);
		}

		void AssertRun(DateTime lastProcesedDate, bool shouldAssignJournalNumber, ZGuid gldPk)
		{
			using (AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, lastProcesedDate))
			{
				var logger = InitialiseAndRunTaskSchedule(ServiceTask);

				Factory.ReloadAll<AccGeneralLedgerData>();
				var gld = Factory.Load<AccGeneralLedgerData>(gldPk);
				AssertEquals("GLD_JournalEntriesNumber:", shouldAssignJournalNumber, !gld.GLD_JournalEntriesNumber.IsEmpty);
				AssertContains("General Ledger Journal Number Assignment Starting.", logger.ToString());

				var logForCompany = "General Ledger Journal Number Assignment Process Start: " + Env.CurrentCompany.Code;
				AssertEquals(shouldAssignJournalNumber, logger.ToString().Contains(logForCompany));
			}
		}

		public void TestRunServiceTaskWithException()
		{
			AccountingConfigurationRegistry.Instance.AllocateJournalEntriesNumberStartDate.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new DateTime(1100, 1, 1));
			AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, new DateTime(1000, 1, 1));

			var logger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertContains("General Ledger Journal Number Assignment Handle Failed", logger.ToString());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			base.SetUpCore();

			TestObjectCreator.SetControlAccountsForGenerateJournalEntriesStartDate();
			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);

			AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesForPostedAccountingTransactions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.GenerateAndStoreJournalEntriesForPostedAccountingTransactions.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			var journalEntriesNumberCustomisationSetting = new JournalEntriesNumberCustomisationSetting(new FallbackLevel(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty))
			{
					AllocationOption = AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN,
					NumberRule = AccountingConstants.JournalEntriesNumberCustomisationNumberRule.ALL
			};
			AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, journalEntriesNumberCustomisationSetting);

			Factory.Save();
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		GeneralLedgerDataJournalNumberAssignmentServiceTask ServiceTask => new();
	}
}

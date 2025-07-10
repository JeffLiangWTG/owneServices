using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	[TestedType(typeof(AccountingCurrencyAdjustmentQueueServiceTask))]
	public class AccountingCurrencyAdjustmentQueueServiceTaskTest : ServiceTaskTestCase<AccountingCurrencyAdjustmentQueueServiceTask>
	{
		[TestDate(2020, 2, 10)]
		public void TestReverseJournalCreated_ARGain()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.USD, 0.9M, 100M, 0M, 111.11M, 0M);
			arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			var serviceTask = new AccountingCurrencyAdjustmentQueueServiceTask();
			InitialiseAndRunTaskSchedule(serviceTask);

			var arAdjustment = AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.Value;
			var exchangeGain = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value;

			var journal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal));

			AssertJournal(journal);

			var lineDescription = "AR Unrealized Gain based on USD 100.00, Original Local AUD 111.11 (Ex.Rate 0.900009), Adjusted Local AUD 125.00 (Ex.Rate 0.800000)";
			AssertJournalLine(journal, DebitCreditDataEntry.DR, arAdjustment, 13.8900M, lineDescription);
			AssertJournalLine(journal, DebitCreditDataEntry.CR, exchangeGain, 13.8900M, lineDescription);

			using (Env.Instance.TemporaryServiceTaskContext(AccountingCurrencyAdjustmentQueueServiceTask.Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}
		}

		[TestDate(2020, 2, 10)]
		public void TestReverseJournalCreated_ARLoss()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.USD, 0.9M, 100M, 0M, 111.11M, 0M);
			arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 1.1M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			InitialiseAndRunTaskSchedule(new AccountingCurrencyAdjustmentQueueServiceTask());

			var arAdjustment = AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.Value;
			var exchangeLoss = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value;

			var journal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal));

			AssertJournal(journal);

			var lineDescription = "AR Unrealized Loss based on USD 100.00, Original Local AUD 111.11 (Ex.Rate 0.900009), Adjusted Local AUD 90.91 (Ex.Rate 1.100000)";
			AssertJournalLine(journal, DebitCreditDataEntry.DR, exchangeLoss, 20.2000M, lineDescription);
			AssertJournalLine(journal, DebitCreditDataEntry.CR, arAdjustment, 20.2000M, lineDescription);
		}

		[TestDate(2020, 2, 10)]
		public void TestReverseJournalCreated_ARGainAndLoss()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.USD, 0.9M, 100M, 0M, 111.11M, 0M);
			arInvoice1.AH_PostDate = new ZDateTime(2020, 1, 10);

			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR002", testObjectCreator.USD, 0.7M, 100M, 0M, 142.86M, 0M);
			arInvoice2.AH_PostDate = new ZDateTime(2020, 1, 10);

			var arInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR003", testObjectCreator.USD, 0.8M, 100M, 0M, 125M, 0M);
			arInvoice3.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			InitialiseAndRunTaskSchedule(new AccountingCurrencyAdjustmentQueueServiceTask());

			var arAdjustment = AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.Value;
			var exchangeGain = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value;
			var exchangeLoss = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value;

			var journal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal));

			AssertJournal(journal, 4);

			var lineDescription = "AR Unrealized Gain based on USD 200.00, Original Local AUD 236.11 (Ex.Rate 0.847063), Adjusted Local AUD 250.00 (Ex.Rate 0.800000)";
			AssertJournalLine(journal, DebitCreditDataEntry.DR, arAdjustment, 13.8900M, lineDescription);
			AssertJournalLine(journal, DebitCreditDataEntry.CR, exchangeGain, 13.8900M, lineDescription);

			lineDescription = "AR Unrealized Loss based on USD 100.00, Original Local AUD 142.86 (Ex.Rate 0.699986), Adjusted Local AUD 125.00 (Ex.Rate 0.800000)";
			AssertJournalLine(journal, DebitCreditDataEntry.DR, exchangeLoss, 17.8600M, lineDescription);
			AssertJournalLine(journal, DebitCreditDataEntry.CR, arAdjustment, 17.8600M, lineDescription);
		}

		[TestDate(2020, 2, 10)]
		public void TestReverseJournalCreated_APLoss()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP001", TestObjectCreator.USD, 0.9M, 100M, 0M, 0M, 111.11m, 0M, 0M, true);
			apInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			InitialiseAndRunTaskSchedule(new AccountingCurrencyAdjustmentQueueServiceTask());

			var apAdjustment = AccountingConfigurationRegistry.Instance.APControlAccountAdjustment.Value;
			var exchangeLoss = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value;

			var journal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal));

			AssertJournal(journal);

			var lineDescription = "AP Unrealized Loss based on USD 100.00, Original Local AUD 111.11 (Ex.Rate 0.900009), Adjusted Local AUD 125.00 (Ex.Rate 0.800000)";
			AssertJournalLine(journal, DebitCreditDataEntry.DR, exchangeLoss, 13.8900M, lineDescription);
			AssertJournalLine(journal, DebitCreditDataEntry.CR, apAdjustment, 13.8900M, lineDescription);
		}

		[TestDate(2020, 2, 10)]
		public void TestReverseJournalCreated_APGain()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP001", TestObjectCreator.USD, 0.9M, 100M, 0M, 0M, 111.11m, 0M, 0M, true);
			apInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 1.1M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			InitialiseAndRunTaskSchedule(new AccountingCurrencyAdjustmentQueueServiceTask());

			var apAdjustment = AccountingConfigurationRegistry.Instance.APControlAccountAdjustment.Value;
			var exchangeGain = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value;

			var journal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal));

			AssertJournal(journal);

			var lineDescription = "AP Unrealized Gain based on USD 100.00, Original Local AUD 111.11 (Ex.Rate 0.900009), Adjusted Local AUD 90.91 (Ex.Rate 1.100000)";
			AssertJournalLine(journal, DebitCreditDataEntry.DR, apAdjustment, 20.2000M, lineDescription);
			AssertJournalLine(journal, DebitCreditDataEntry.CR, exchangeGain, 20.2000M, lineDescription);
		}

		[TestDate(2020, 2, 10)]
		public void TestReverseJournalCreated_APGainAndLoss()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var apInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("AP001", TestObjectCreator.USD, 0.9M, 100M, 0M, 0M, 111.11m, 0M, 0M, true);
			apInvoice1.AH_PostDate = new ZDateTime(2020, 1, 10);
			var apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("AP002", TestObjectCreator.USD, 0.7M, 100M, 0M, 0M, 142.86m, 0M, 0M, true);
			apInvoice2.AH_PostDate = new ZDateTime(2020, 1, 10);
			var apInvoice3 = TestObjectCreator.CreateAPInvoice<APInvoice>("AP003", TestObjectCreator.USD, 0.8M, 100M, 0M, 0M, 125m, 0M, 0M, true);
			apInvoice3.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			InitialiseAndRunTaskSchedule(new AccountingCurrencyAdjustmentQueueServiceTask());

			var apAdjustment = AccountingConfigurationRegistry.Instance.APControlAccountAdjustment.Value;
			var exchangeGain = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value;
			var exchangeLoss = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value;

			var journal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal));

			AssertJournal(journal, 4);

			var lineDescription = "AP Unrealized Gain based on USD 200.00, Original Local AUD 267.86 (Ex.Rate 0.746659), Adjusted Local AUD 250.00 (Ex.Rate 0.800000)";
			AssertJournalLine(journal, DebitCreditDataEntry.DR, apAdjustment, 17.8600M, lineDescription);
			AssertJournalLine(journal, DebitCreditDataEntry.CR, exchangeGain, 17.8600M, lineDescription);

			lineDescription = "AP Unrealized Loss based on USD 100.00, Original Local AUD 111.11 (Ex.Rate 0.900009), Adjusted Local AUD 125.00 (Ex.Rate 0.800000)";
			AssertJournalLine(journal, DebitCreditDataEntry.DR, exchangeLoss, 13.8900M, lineDescription);
			AssertJournalLine(journal, DebitCreditDataEntry.CR, apAdjustment, 13.8900M, lineDescription);
		}

		[TestDate(2020, 2, 10)]
		public void TestReverseJournalCreated_APAPGainAndLoss_LinesOrderby()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var apInvoice1 = TestObjectCreator.CreateAPInvoice<APInvoice>("AP001", TestObjectCreator.USD, 0.9M, 100M, 0M, 0M, 111.11m, 0M, 0M, true);
			apInvoice1.AH_PostDate = new ZDateTime(2020, 1, 10);
			var apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("AP002", TestObjectCreator.USD, 0.7M, 100M, 0M, 0M, 142.86m, 0M, 0M, true);
			apInvoice2.AH_PostDate = new ZDateTime(2020, 1, 10);
			var apInvoice3 = TestObjectCreator.CreateAPInvoice<APInvoice>("AP003", TestObjectCreator.EUR, 0.9M, 100M, 0M, 0M, 111.11m, 0M, 0M, true);
			apInvoice3.AH_PostDate = new ZDateTime(2020, 1, 10);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.USD, 0.9M, 100M, 0M, 111.11M, 0M);
			arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.EUR, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			InitialiseAndRunTaskSchedule(new AccountingCurrencyAdjustmentQueueServiceTask());

			var journal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal));

			AssertJournal(journal, 8);

			string[] descriptionOrderbys = new string[] {
				"AP Unrealized Gain based on USD 100.00, Original Local AUD 142.86 (Ex.Rate 0.699986), Adjusted Local AUD 125.00 (Ex.Rate 0.800000)",
				"AP Unrealized Loss based on EUR 100.00, Original Local AUD 111.11 (Ex.Rate 0.900009), Adjusted Local AUD 125.00 (Ex.Rate 0.800000)",
				"AP Unrealized Loss based on USD 100.00, Original Local AUD 111.11 (Ex.Rate 0.900009), Adjusted Local AUD 125.00 (Ex.Rate 0.800000)",
				"AR Unrealized Gain based on USD 100.00, Original Local AUD 111.11 (Ex.Rate 0.900009), Adjusted Local AUD 125.00 (Ex.Rate 0.800000)"
			};

			for (int i = 1; i < descriptionOrderbys.Length; i++)
			{
				var journalLine1 = journal.Lines.Cast<GLJournalLine>().FirstOrDefault(x => x.AL_Desc == descriptionOrderbys[i - 1]);
				AssertNotNull($"{descriptionOrderbys[i - 1]} is found:", journalLine1);

				var journalLine2 = journal.Lines.Cast<GLJournalLine>().FirstOrDefault(x => x.AL_Desc == descriptionOrderbys[i]);
				AssertNotNull($"{descriptionOrderbys[i]} is found:", journalLine2);

				Assert($"journal line sequence should increase with sequence {journalLine1.AL_Sequence} and {journalLine2.AL_Sequence}", journalLine1.AL_Sequence < journalLine2.AL_Sequence);
			}
		}

		[TestDate(2020, 2, 10)]
		public void TestReverseJournalsCreatedInDifferentCompanies()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.USD, 1M, 100M, 0M, 100M, 0M);
			arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			var sgCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			Guid departmentBRNPK = (Guid)Utilities.GetFieldFromTable(GlbDepartmentSchema.PK.Name, GlbDepartmentSchema.Constants.TableName, GlbDepartmentSchema.GE_Code, "BRN");
			Guid singaporeBranchPK = (Guid)Utilities.GetFieldFromTable(GlbBranchSchema.PK.Name, GlbBranchSchema.Constants.TableName, GlbBranchSchema.GB_Code, "SIN");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeBranchPK, departmentBRNPK))
			{
				var newFactory = new BusinessObjectFactory();
				var newTestObjectCreator = new TestObjectCreator(newFactory);
				var newPeriod = new PeriodManager(newFactory);

				AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				newTestObjectCreator.CreateTestPeriodsForEntireYear(2020);
				arInvoice = newTestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR002", newTestObjectCreator.USD, 1M, 150M, 0M, 150M, 0M);
				arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
				newFactory.Save();
				newPeriod.CloseSubLedgerPeriod();

				var arControlAccount = newTestObjectCreator.CreateAccGLHeader("5300.03.99", "AS", "Test BSH 1", "BSH", Core.Constants.DebitCredit.Debit);
				AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arControlAccount.PK.ToGuid());
				var apControlAccount = newTestObjectCreator.CreateAccGLHeader("5300.03.00", "AS", "Test BSH 2", "BSH", Core.Constants.DebitCredit.Credit);
				AccountingConfigurationRegistry.Instance.APControlAccountAdjustment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apControlAccount.PK.ToGuid());

				newTestObjectCreator.CreateExchangeRate(newTestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
				newFactory.Save();
			}

			InitialiseAndRunTaskSchedule(new AccountingCurrencyAdjustmentQueueServiceTask());

			var auJournal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNotNull(auJournal);

			var sgJournal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal).AddToFilter(AccTransactionHeaderSchema.AH_GC, sgCompany.PK));
			AssertNotNull(sgJournal);
		}

		[TestDate(2020, 2, 10)]
		public void TestFollowingReverseJournalCreatedWhenException()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.USD, 1M, 100M, 0M, 100M, 0M);
			arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			var sgCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			var departmentBRNPK = (Guid)Utilities.GetFieldFromTable(GlbDepartmentSchema.PK.Name, GlbDepartmentSchema.Constants.TableName, GlbDepartmentSchema.GE_Code, "BRN");
			var singaporeBranchPK = (Guid)Utilities.GetFieldFromTable(GlbBranchSchema.PK.Name, GlbBranchSchema.Constants.TableName, GlbBranchSchema.GB_Code, "SIN");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singaporeBranchPK, departmentBRNPK))
			{
				var newFactory = new BusinessObjectFactory();
				var newTestObjectCreator = new TestObjectCreator(newFactory);
				var newPeriod = new PeriodManager(newFactory);

				AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				newTestObjectCreator.CreateTestPeriodsForEntireYear(2020);
				arInvoice = newTestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR002", newTestObjectCreator.USD, 1M, 150M, 0M, 150M, 0M);
				arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
				newFactory.Save();
				newPeriod.CloseSubLedgerPeriod();

				var arControlAccount = newTestObjectCreator.CreateAccGLHeader("5300.03.99", "AS", "Test BSH 1", "BSH", Core.Constants.DebitCredit.Debit);
				AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arControlAccount.PK.ToGuid());
				var apControlAccount = newTestObjectCreator.CreateAccGLHeader("5300.03.00", "AS", "Test BSH 2", "BSH", Core.Constants.DebitCredit.Credit);
				AccountingConfigurationRegistry.Instance.APControlAccountAdjustment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apControlAccount.PK.ToGuid());

				newTestObjectCreator.CreateExchangeRate(newTestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
				newFactory.Save();
			}

			var serviceTask = new AccountingCurrencyAdjustmentQueueServiceTask();
			var loggerMock = new Mock<ILogger>();
			loggerMock
				.Setup(l => l.Log(LogType.Debug, It.Is<string>(s => s.Contains("Calculation of Currency Adjustment Values starting") && s.Contains($"Company: {GlbCompany.CurrentCompany.GC_Code}"))))
				.Throws(new Exception("Fail on first loop"));

			InitialiseTaskSchedule(serviceTask);
			serviceTask.ServiceLogger = loggerMock.Object;
			RunTaskSchedule(serviceTask);

			var auJournal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNull(auJournal);

			var sgJournal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal).AddToFilter(AccTransactionHeaderSchema.AH_GC, sgCompany.PK));
			AssertNotNull(sgJournal);

			var query = new ZQuery(AccPeriodManagementSchema.AM_Period, 202001);
			query.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
			var period = Factory.LoadTop1<AccPeriodManagement>(query);
			AssertQueuedPeriodExists(period.PK.ToGuid(), true);

			loggerMock.Verify(l => l.Log(LogType.Error, $"Company: EDI, Period 202001 Calculation of Currency Adjustment Values finished abnormally. {System.Environment.NewLine}Fail on first loop"), Times.Once);
		}

		[TestDate(2020, 2, 10)]
		public void TestReverseJournalCreated_GainAndLossEqualToZero()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
			arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			var task = new AccountingCurrencyAdjustmentQueueServiceTask();
			var serviceLog = InitialiseAndRunTaskSchedule(task);

			var journal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal));
			AssertNull(journal);
			AssertMultilineASCIIEquals($@"
Debug|AR and AP Outstanding Balance Currency Adjustment automated process starting.
Debug|Company: EDI, Period 202001 Calculation of Currency Adjustment Values starting.
Debug|Automated journal(s) was not created due to Gain and Loss both equal to 0.
Information|AR and AP Outstanding Balance Currency Adjustment automated process completed for 1 periods.
Debug|AR and AP Outstanding Balance Currency Adjustment automated process completed.".Trim(), serviceLog.ToString());

			var query = new ZQuery(AccPeriodManagementSchema.AM_Period, 202001);
			query.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
			var period = Factory.LoadTop1<AccPeriodManagement>(query);
			AssertEquals(true, period.Logs.HasLogWith(StmALogSchema.SL_Reference, "Automated journal(s) was not created."));

			AssertQueuedPeriodExists(period.PK.ToGuid(), false);
		}

		void AssertQueuedPeriodExists(Guid periodPK, bool exists)
		{
			AssertEquals(exists, Db.Connection.Exists(FormattableString.Invariant($"FROM dbo.AccCurrencyAdjustmentQueue WHERE ACA_ParentID = @PeriodPK"), cmd => cmd.AddParameter("@PeriodPK", SqlDbType.UniqueIdentifier, periodPK)));
		}

		[TestDate(2020, 2, 10)]
		public void TestReportsCreated()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.USD, 0.9M, 100M, 0M, 111.11M, 0M);
			arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			InitialiseAndRunTaskSchedule(new AccountingCurrencyAdjustmentQueueServiceTask());

			var journal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal));
			var edocs = journal.DocManagerInfo.AllEDocs;
			AssertEquals(3, edocs.Count);
			Assert(edocs.Cast<StorageFile>().Any(x => x.Name == "AR Outstanding Balances Currency Revaluation at Period End Rates.pdf" && x.IsInDatabase && ((IDeliveryEmailAttachment)x).FileSizeInBytes > 0 && x.SC_DocType == "PDF"));
			Assert(edocs.Cast<StorageFile>().Any(x => x.Name == "AP Outstanding Balances Currency Revaluation at Period End Rates.pdf" && x.IsInDatabase && ((IDeliveryEmailAttachment)x).FileSizeInBytes > 0 && x.SC_DocType == "PDF"));
		}

		[TestDate(2020, 2, 10)]
		public void TestReportsNotCreatedWithErrors()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.USD, 0.9M, 100M, 0M, 111.11M, 0M);
			arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();
			var serviceTask = new AccountingCurrencyAdjustmentQueueServiceTask();
			serviceTask.ShouldEnableInvoicePaymentWebService = true;
			var serviceLog = InitialiseAndRunTaskSchedule(serviceTask);

			var journal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal));
			AssertNull(journal);
			AssertContains(@"Error|Automated journal(s) was not created due to Outstanding Balances Currency Revaluation at Period End Rates report has following validation errors.
Error - SinglePeriod: With the Invoice Payment Web Service enabled it must be only the current period
Please check if the relevant periods have been set up.", serviceLog.ToString());
		}

		[TestDate(2020, 2, 10)]
		public void TestReverseJournalPostedAutomatically()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.USD, 0.9M, 100M, 0M, 111.11M, 0M);
			arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			InitialiseAndRunTaskSchedule(new AccountingCurrencyAdjustmentQueueServiceTask());

			var journal = Factory.LoadTop1<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLReversingJournal));
			AssertNotNull(journal);
			AssertJournalApprovalRequest(journal);
		}

		[TestDate(2020, 2, 10)]
		public void TestGeneralJournalPostedAutomaticallyForChina()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
				var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.USD, 0.9M, 100M, 0M, 111.11M, 0M);
				arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
				Factory.Save();
				PeriodManager.CloseSubLedgerPeriod();

				SetupControlAccount();
				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
				Factory.Save();

				InitialiseAndRunTaskSchedule(new AccountingCurrencyAdjustmentQueueServiceTask());

				var lineDescription = "AR Unrealized Gain based on USD 100.00, Original Local CNY 111.11 (Ex.Rate 0.900009), Adjusted Local CNY 125.00 (Ex.Rate 0.800000)";
				var arAdjustment = AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.Value;
				var exchangeGain = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value;

				var journals = Factory.Load<GLJournal>(new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.GLStandardJournal)).OrderBy(x => x.PostPeriod).ToArray();
				AssertEquals("There are two GLJournals created for China company.", 2, journals.Length);

				var transactionBelongToGroup = journals[0].AH_TransactionBelongsToGroup;
				AssertNotEquals("TransactionBelongsToGroup should be valid.", ZGuid.Empty, transactionBelongToGroup);
				for (int i = 0; i < 2; i++)
				{
					AssertEquals("A/R AND A/P OUTSTANDING BALANCE CURRENCY ADJUSTMENT JOURNAL.", journals[i].AH_Desc);
					AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, journals[i].AH_RX_NKTransactionCurrency);
					AssertEquals(arInvoice.AH_GB, journals[i].AH_GB);
					AssertEquals(DefaultDepartment.PK, journals[i].AH_GE);
					AssertEquals(transactionBelongToGroup, journals[i].AH_TransactionBelongsToGroup);

					AssertJournalApprovalRequest(journals[i]);

					AssertEquals("journal lines count:", 2, journals[i].Lines.Count);
					if (i == 0)
					{
						AssertEquals(202001, journals[i].PostPeriod);
						AssertJournalLine(journals[i], DebitCreditDataEntry.DR, arAdjustment, 13.8900M, lineDescription);
						AssertJournalLine(journals[i], DebitCreditDataEntry.CR, exchangeGain, 13.8900M, lineDescription);
					}
					else
					{
						AssertEquals(202002, journals[i].PostPeriod);
						AssertJournalLine(journals[i], DebitCreditDataEntry.DR, exchangeGain, 13.8900M, lineDescription);
						AssertJournalLine(journals[i], DebitCreditDataEntry.CR, arAdjustment, 13.8900M, lineDescription);
					}
				}
			}
		}

		void AssertJournal(GLJournal journal, int linesCount = 2)
		{
			AssertEquals("A/R AND A/P OUTSTANDING BALANCE CURRENCY ADJUSTMENT JOURNAL.", journal.AH_Desc);
			AssertEquals("PostPeriod:", 202001, journal.PostPeriod);
			AssertEquals("AgePeriod:", 202002, journal.AgePeriod);
			AssertEquals("Currency:", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, journal.AH_RX_NKTransactionCurrency);
			AssertEquals("Branch:", GlbBranch.CurrentBranch.PK, journal.AH_GB);
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "BRN"));
			AssertEquals("Department:", journal.AH_GE, department.PK);

			var journalLines = journal.Lines.Cast<GLJournalLine>();
			AssertEquals("Lines count:", linesCount, journalLines.Count());
		}

		void AssertJournalLine(GLJournal journal, string debitOrCredit, Guid lineAccount, decimal gainLossAmount, string description)
		{
			var journalLine = journal.Lines.Cast<GLJournalLine>().FirstOrDefault(x => x.DebitCreditSign == debitOrCredit && x.AL_Desc == description);
			AssertNotNull(journalLine);
			AssertEquals("GL Account:", lineAccount, journalLine.AL_AG);
			AssertEquals("Amount:", gainLossAmount, journalLine.UnsignedOSLineAmount);
			AssertEquals("Description:", description, journalLine.AL_Desc);
			AssertEquals("Branch:", GlbBranch.CurrentBranch.PK, journalLine.AL_GB);
			AssertEquals("Department:", DefaultDepartment.PK, journalLine.AL_GE);
		}

		void AssertJournalApprovalRequest(GLJournal journal)
		{
			var request = Factory.LoadTop1<GLJournalApprovalRequest>(new ZQuery(GenApprovalRequestSchema.XP_ParentID, journal.PK));
			AssertEquals(journal.AH_GB, request.XP_GB_RequestingBranch);
			AssertEquals(journal.AH_Desc, request.XP_ReasonDescription);
			AssertEquals("PST", request.XP_ApprovalStatus);
			AssertEquals("GLJ", request.XP_ApprovalType);
			AssertEquals(ZDateTime.Now.Date, request.XP_ApprovalDate.Date);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, request.XP_GS_NKApprovingUser1);
		}

		[TestDate(2020, 2, 10)]
		public void TestJournalsCreatedWithAggregation()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.USD, 0.9M, 100M, 0M, 111.11M, 0M);
			arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			InitialiseAndRunTaskSchedule(new AccountingCurrencyAdjustmentQueueServiceTask());

			var arAdjustment = AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.Value;
			var exchangeGain = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value;

			AssertEquals("Should be 13.8900M", 13.8900M, GetSumFromAccGLAggregateForAccount(arAdjustment, 202001));
			AssertEquals("Should be 13.8900M", -13.8900M, GetSumFromAccGLAggregateForAccount(exchangeGain, 202001));
			AssertEquals("Should be 13.8900M", -13.8900M, GetSumFromAccGLAggregateForAccount(arAdjustment, 202002));
			AssertEquals("Should be 13.8900M", 13.8900M, GetSumFromAccGLAggregateForAccount(exchangeGain, 202002));
		}

		decimal GetSumFromAccGLAggregateForAccount(Guid gLAccount, int period)
		{
			string sQL = "SELECT SUM(AA_Amount) as Amount FROM dbo.AccGLAggregate WHERE AA_AG = @AccountNum and AA_Period = @Period";
			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@AccountNum", gLAccount, AccGLAggregateSchema.AA_AG);
			parameters.Add("@Period", period, AccGLAggregateSchema.AA_Period);
			DynamicBusinessObjectCollection dynamicCollection = new DynamicBusinessObjectCollection(Factory);
			dynamicCollection.Load(sQL, parameters);
			return (ZDecimal)dynamicCollection[0]["Amount"];
		}

		[TestDate(2020, 2, 10)]
		public void TestAccountingCurrencyAdjustmentQueueServiceTask_RunTask()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			SetupTransactions();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			var task = new AccountingCurrencyAdjustmentQueueServiceTask();
			var serviceLog = InitialiseAndRunTaskSchedule(task);

			AssertMultilineASCIIEquals($@"
Debug|AR and AP Outstanding Balance Currency Adjustment automated process starting.
Debug|Company: EDI, Period 202001 Calculation of Currency Adjustment Values starting.
Debug|Automated journal(s) was created successfully.
Information|AR and AP Outstanding Balance Currency Adjustment automated process completed for 1 periods.
Debug|AR and AP Outstanding Balance Currency Adjustment automated process completed.".Trim(), serviceLog.ToString());

			var query = new ZQuery(AccPeriodManagementSchema.AM_Period, 202001);
			query.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
			var period = Factory.LoadTop1<AccPeriodManagement>(query);
			AssertEquals(true, period.Logs.HasLogWith(StmALogSchema.SL_Reference, "Automated journal(s) was created."));

			AssertQueuedPeriodExists(period.PK.ToGuid(), false);
		}

		public void TestAccountingCurrencyAdjustmentQueueServiceTask_NoPeriodQueuedWarning()
		{
			var task = new AccountingCurrencyAdjustmentQueueServiceTask();
			var serviceLog = InitialiseAndRunTaskSchedule(task);

			AssertMultilineASCIIEquals($@"
Debug|AR and AP Outstanding Balance Currency Adjustment automated process starting.
Warning|No AR and AP Outstanding Balance Currency Adjustment queue was found.
Debug|AR and AP Outstanding Balance Currency Adjustment automated process completed.".Trim(), serviceLog.ToString());
		}

		[TestDate(2020, 2, 10)]
		public void TestAccountingCurrencyAdjustmentQueueServiceTask_InvoicePaymentWebServiceValidation()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			SetupTransactions();
			PeriodManager.CloseSubLedgerPeriod();

			SetupControlAccount();
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "PER", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
			Factory.Save();

			var sql = string.Format(@"SELECT * FROM dbo.AccCurrencyAdjustmentQueue");
			var results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Count should be 1", 1, results.Rows.Count);

			AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var task = new AccountingCurrencyAdjustmentQueueServiceTask();
			var serviceLog = InitialiseAndRunTaskSchedule(task);
			AssertContains("Warning|The Invoice Payment Web Service is enabled. Queue deleted.", serviceLog.ToString());

			results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Count should be 0", 0, results.Rows.Count);
		}

		[TestDate(2020, 2, 10)]
		public void TestAccountingCurrencyAdjustmentQueueServiceTask_ControlAccountValidation()
		{
			AccountingCurrencyAdjustmentQueueServiceTask task;
			TestServiceLogger serviceLog;
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			PeriodManager.CloseSubLedgerPeriod();

			//When controller Account is all Guid.Empty
			var gLHeader = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, AccountingConstants.RegistryDefaultValues.ForeignCurrencyGLBalanceAdjustmentAccount));
			gLHeader.Delete();
			Factory.Save();
			using (AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
			using (AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.Value);
				AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.APControlAccountAdjustment.Value);
				AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value);
				AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value);

				AssertServiceLog(true, true, true, true);
			}

			//When controller Account contain Guid.Empty && not vaild & valid
			gLHeader = testObjectCreator.CreateAccGLHeader(AccountingConstants.RegistryDefaultValues.ForeignCurrencyGLBalanceAdjustmentAccount, "TS", "Test1", "P&L", Enterprise.Core.Constants.DebitCredit.Credit);
			using (AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, gLHeader.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid()))
			{
				gLHeader.Delete();
				Factory.Save();

				AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.Value);
				AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.APControlAccountAdjustment.Value);
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value);
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value);

				AssertServiceLog(true, true, true, false);
			}

			//When controller Account is all not vaild
			gLHeader = testObjectCreator.CreateAccGLHeader(AccountingConstants.RegistryDefaultValues.ForeignCurrencyGLBalanceAdjustmentAccount, "TS", "Test2", "P&L", Enterprise.Core.Constants.DebitCredit.Credit);
			SetupControlAccount();
			using (AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, gLHeader.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, gLHeader.PK.ToGuid()))
			{
				gLHeader.Delete();
				Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.Value).Delete();
				Factory.Load<AccGLHeader>(AccountingConfigurationRegistry.Instance.APControlAccountAdjustment.Value).Delete();
				Factory.Save();

				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.Value);
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.APControlAccountAdjustment.Value);
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value);
				AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value);

				AssertServiceLog(true, true, true, true);
			}

			//When controller Account is all vaild
			using (AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.APControlAccountAdjustment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid()))
			using (AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader2.PK.ToGuid()))
			{
				AssertServiceLog(false, false, false, false);
			}

			void AssertServiceLog(bool shouldLogARControlAccountAdjustment, bool shouldLogAPControlAccountAdjustment, bool shouldLogGainAccount, bool shouldLogLossAccount)
			{
				task = new AccountingCurrencyAdjustmentQueueServiceTask();
				serviceLog = InitialiseAndRunTaskSchedule(task);

				AssertEquals(shouldLogARControlAccountAdjustment, serviceLog.ToString().Contains("Error|Registry 'Accounting -> General Ledger Defaults -> Link Account -> AR Control Account Adjustment' has not been setup."));
				AssertEquals(shouldLogAPControlAccountAdjustment, serviceLog.ToString().Contains("Error|Registry 'Accounting -> General Ledger Defaults -> Link Account -> AP Control Account Adjustment' has not been setup."));
				AssertEquals(shouldLogGainAccount, serviceLog.ToString().Contains("Error|Registry 'Accounting -> General Ledger Defaults -> Link Account -> Currency Adjustment Exchange Gain Account' has not been setup."));
				AssertEquals(shouldLogLossAccount, serviceLog.ToString().Contains("Error|Registry 'Accounting -> General Ledger Defaults -> Link Account -> Currency Adjustment Exchange Loss Account' has not been setup."));

				AssertEquals(!shouldLogARControlAccountAdjustment && !shouldLogAPControlAccountAdjustment && !shouldLogGainAccount && !shouldLogLossAccount, serviceLog.ToString().Contains("Calculation of Currency Adjustment Values starting"));
			}
		}

		[TestDate(2020, 2, 10)]
		public void TestAccountingCurrencyAdjustmentQueueServiceTask_PeriodClosedValidation()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			SetupTransactions();
			PeriodManager.CloseSubLedgerPeriod();
			PeriodManager.CloseGLPeriod();
			PeriodManager.NextUnClosedGLPeriod.AM_IsGeneralLedgerClosed = true;
			SetupControlAccount();
			Factory.Save();

			var task = new AccountingCurrencyAdjustmentQueueServiceTask();
			var serviceLog = InitialiseAndRunTaskSchedule(task);
			AssertContains("Error|Period 202001 is closed.", serviceLog.ToString());
		}

		[TestDate(2020, 2, 10)]
		public void TestAccountingCurrencyAdjustmentQueueServiceTask_PeriodSetupValidation()
		{
			PeriodManager.CreateOnePeriod(202001, new ZDateTime(2020, 1, 1), new ZDateTime(2020, 1, 31), Factory);
			SetupTransactions();
			PeriodManager.CloseSubLedgerPeriod();
			SetupControlAccount();

			var task = new AccountingCurrencyAdjustmentQueueServiceTask();
			var serviceLog = InitialiseAndRunTaskSchedule(task);
			AssertContains("Error|The Reverse Period of 202001 has not been setup.", serviceLog.ToString());

			PeriodManager.CreateOnePeriod(202002, new ZDateTime(2020, 2, 1), new ZDateTime(2020, 2, 28), Factory);
			Factory.Save();
			task = new AccountingCurrencyAdjustmentQueueServiceTask();
			serviceLog = InitialiseAndRunTaskSchedule(task);
			AssertNotContains("Error|The Reverse Period of 202001 has not been setup.", serviceLog.ToString());
		}

		[TestDate(2020, 2, 10)]
		public void TestAccountingCurrencyAdjustmentQueueServiceTask_PeriodDeletedValidation()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			SetupTransactions();
			PeriodManager.CloseSubLedgerPeriod();
			SetupControlAccount();
			Factory.Save();

			AssertEquals(12, PeriodManager.Periods.Count);
			var query = new ZQuery(AccPeriodManagementSchema.AM_Period, 202001);
			query.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
			var period = Factory.LoadTop1<AccPeriodManagement>(query);
			AssertQueuedPeriodExists(period.PK.ToGuid(), true);

			period.Delete();
			Factory.Save();
			AssertEquals(11, PeriodManager.Periods.Count);
			AssertQueuedPeriodExists(period.PK.ToGuid(), true);

			var task = new AccountingCurrencyAdjustmentQueueServiceTask();
			var serviceLog = InitialiseAndRunTaskSchedule(task);

			AssertContains("Warning|Invalid queue period is found in Company EDI.", serviceLog.ToString());
			AssertQueuedPeriodExists(period.PK.ToGuid(), false);
		}

		[TestDate(2020, 2, 10)]
		public void TestAccountingCurrencyAdjustmentQueueServiceTask_ExchangeRateValidation()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);

			SetupTransactions();

			PeriodManager.CloseSubLedgerPeriod();
			SetupControlAccount();

			using (AccountingConfigurationRegistry.Instance.ARAPOutstandingBalancesCurrencyAdjustmentExchangeRateType.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, "BUY"))
			{
				var task = new AccountingCurrencyAdjustmentQueueServiceTask();
				var serviceLog = InitialiseAndRunTaskSchedule(task);
				var companyCode = GlbCompany.CurrentCompany.GC_Code;

				AssertContains($"Error|The exchange rate for transaction currency USD cannot be found with Rate type BUY in {companyCode} login company.", serviceLog.ToString());

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 0.8M, new ZDateTime(2020, 1, 9), new ZDateTime(2020, 1, 20));
				task = new AccountingCurrencyAdjustmentQueueServiceTask();
				serviceLog = InitialiseAndRunTaskSchedule(task);

				AssertContains("Exchange can't be found with 'BUY' RateType and PeriodEndDate '2020-1-31'", $"Error|The exchange rate for transaction currency USD cannot be found with Rate type BUY in {companyCode} login company.", serviceLog.ToString());

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 0.8M, new ZDateTime(2020, 1, 31), new ZDateTime(2020, 1, 31));
				task = new AccountingCurrencyAdjustmentQueueServiceTask();
				serviceLog = InitialiseAndRunTaskSchedule(task);

				AssertNotContains("Exchange can be found with 'BUY' RateType and PeriodEndDate '2020-1-31'", $"Error|The exchange rate for transaction currency USD cannot be found with Rate type BUY in {companyCode} login company.", serviceLog.ToString());
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		void SetupTransactions()
		{
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP001", TestObjectCreator.USD, 0.9M, 100M, 0M, 0M, 111.11m, 0M, 0M, true);
			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR001", testObjectCreator.AUD, 1M, 200M, 0M, 200M, 0M);
			apInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			arInvoice.AH_PostDate = new ZDateTime(2020, 1, 10);
			Factory.Save();
		}

		void SetupControlAccount()
		{
			var arControlAccount = TestObjectCreator.CreateAccGLHeader("5300.03.97", "AS", "Test BSH 1", "BSH", Core.Constants.DebitCredit.Debit);
			AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, arControlAccount.PK.ToGuid());

			var apControlAccount = TestObjectCreator.CreateAccGLHeader("5300.03.98", "AS", "Test BSH 2", "BSH", Core.Constants.DebitCredit.Credit);
			AccountingConfigurationRegistry.Instance.APControlAccountAdjustment.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, apControlAccount.PK.ToGuid());
		}

		GlbDepartment DefaultDepartment => Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "BRN"));

		PeriodManager PeriodManager => periodManager ?? (periodManager = new PeriodManager(Factory));
		PeriodManager periodManager;

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		protected override void SetUpCore()
		{
			base.SetUpCore();

			AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
		}
	}
}

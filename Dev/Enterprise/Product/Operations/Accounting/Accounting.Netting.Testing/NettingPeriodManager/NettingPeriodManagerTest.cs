using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting
{
	sealed class NettingPeriodManagerTest : TestCaseWithFactory
	{
		[TestDate(2015, 1, 10)]
		public void TestMovingUnmatchedTransactionsToNextPeriod()
		{
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.AALSHI.PK.ToGuid());

			var nettingSystem = Factory.New<NettingSystem>();
			nettingSystem.NS_Code = "NS1";
			nettingSystem.NS_Description = "bla bla";
			nettingSystem.NS_GC = GlbCompany.CurrentCompany.PK;

			SetupParticipant(participant1, "FUL", "GBP", "EUR", "USD");
			SetupParticipant(participant2, "FUL", "USD", "EUR", "USD");

			var pivot1 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "INV1", "AUD", 100M);
			var pivot2 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "INV2", "EUR", 200M);

			NettingReceivableTransaction receivableTransaction;
			NettingPayableTransaction payableTransaction;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "INV3", "AUD", 300M, 300M, out receivableTransaction, out payableTransaction);

			Factory.Save();

			var receivableQuery = new ZQuery(NettingReceivableTransactionSchema.NRT_NSP_Period, nextPeriod.PK);
			var receivableTransactions = Factory.Load<NettingReceivableTransaction>(receivableQuery);

			var payableQuery = new ZQuery(NettingPayableTransactionSchema.NPT_NSP_Period, nextPeriod.PK);
			var payableTransactions = Factory.Load<NettingPayableTransaction>(payableQuery);

			AssertEquals("Precondition", 0, receivableTransactions.Length);
			AssertEquals("Precondition", 0, payableTransactions.Length);

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			var newFactory = new BusinessObjectFactory();
			receivableTransactions = newFactory.Load<NettingReceivableTransaction>(receivableQuery);
			payableTransactions = newFactory.Load<NettingPayableTransaction>(payableQuery);

			AssertEquals(1, receivableTransactions.Length);
			AssertEquals(1, payableTransactions.Length);

			AssertEquals(receivableTransaction.PK, receivableTransactions.First().PK);
			AssertEquals(payableTransaction.PK, payableTransactions.First().PK);
		}

		public void TestUpdateIndicativeExchangeRatesFromExecutionRates()
		{
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.AALSHI.PK.ToGuid());

			var cNY_IND = CreateExchangeRate(nettingSystem, "CNY", "IND", 0.05M, period.PK);
			var nOK_IND = CreateExchangeRate(nettingSystem, "NOK", "IND", 1.15M, period.PK);
			var sEK_IND = CreateExchangeRate(nettingSystem, "SEK", "IND", 1.25M, nextPeriod.PK);

			var aUD_NET = CreateExchangeRate(nettingSystem, "CNY", "NET", 0.05M, period.PK);
			var sGD_NET = CreateExchangeRate(nettingSystem, "SGD", "NET", 0.88M, period.PK);
			var nOK_NET = CreateExchangeRate(nettingSystem, "NOK", "NET", 1.10M, period.PK);
			var sEK_NET = CreateExchangeRate(nettingSystem, "SEK", "NET", 1.35M, period.PK);

			Factory.Save();

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			AssertEquals("CNY indicating rate stays the same", 0.05M, cNY_IND.NER_Rate);

			var newFactory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(NettingSystemExchangeRate));
			query.AddToFilter(NettingSystemExchangeRateSchema.NER_RX_NKCurrency, "SGD");
			query.AddToFilter(NettingSystemExchangeRateSchema.NER_RateType, "IND");
			var sGD_IND = newFactory.LoadTop1<NettingSystemExchangeRate>(query);
			AssertNotNull("SGD indicating rate is created from NET execution rate", sGD_IND);
			AssertEquals("SGD NET execution rate", 0.88M, sGD_IND.NER_Rate);

			query = new ZDBOnlyQuery(typeof(NettingSystemExchangeRate));
			query.AddToFilter(NettingSystemExchangeRateSchema.NER_RX_NKCurrency, "NOK");
			query.AddToFilter(NettingSystemExchangeRateSchema.NER_RateType, "IND");
			nOK_IND = newFactory.LoadTop1<NettingSystemExchangeRate>(query);
			AssertEquals("NOK indicating updated from NET execution rate", 1.10M, nOK_IND.NER_Rate);

			AssertEquals("SEK indicating stays the same as no current NET execution rate is present", 1.25M, sEK_IND.NER_Rate);
		}

		public void TestCopyExecutionRatetoNextPeriodIndicativeRate()
		{
			var query = new ZQuery(NettingSystemExchangeRateSchema.NER_NSP_Period, period.PK);
			query.AddToFilter(NettingSystemExchangeRateSchema.NER_RateType, "NET");

			var indRates = Factory.Load<NettingSystemExchangeRate>(query);
			int numberOfAlreadyExistingRates = indRates.Length;

			var fRK_NET_CurrentPeriod = CreateExchangeRate(nettingSystem, "FRK", "IND", 30M, period.PK);

			var bDT_NET_CurrentPeriod = CreateExchangeRate(nettingSystem, "BDT", "NET", 0.075M, period.PK);
			var nOK_NET_CurrentPeriod = CreateExchangeRate(nettingSystem, "NOK", "NET", 1.10M, period.PK);
			var sEK_NET_CurrentPeriod = CreateExchangeRate(nettingSystem, "SEK", "NET", 1.35M, period.PK);

			var cNY_IND_NextPeriod = CreateExchangeRate(nettingSystem, "CNY", "IND", 0.05M, nextPeriod.PK);
			var nOK_IND_NextPeriod = CreateExchangeRate(nettingSystem, "NOK", "IND", 1.15M, nextPeriod.PK);
			var sEK_IND_NextPeriod = CreateExchangeRate(nettingSystem, "SEK", "IND", 1.25M, nextPeriod.PK);

			var jPY_NET_NextPeriod = CreateExchangeRate(nettingSystem, "JPY", "NET", 0.025M, nextPeriod.PK);

			Factory.Save();

			new NettingPeriodManager(period).CopyExecutionRatesToNextPeriodIndicativeRate();

			var newFactory = new BusinessObjectFactory();
			query = new ZQuery(NettingSystemExchangeRateSchema.NER_NSP_Period, nextPeriod.PK);
			query.AddToFilter(NettingSystemExchangeRateSchema.NER_RateType, "IND");

			indRates = newFactory.Load<NettingSystemExchangeRate>(query);
			AssertEquals("Number of IND exchange rates for next period", numberOfAlreadyExistingRates + 4, indRates.Length);

			var bDT_IND_NextPeriod = indRates.First(x => x.NER_RX_NKCurrency == "BDT");
			AssertEquals("BDT NET rate is copied to next period as IND", bDT_NET_CurrentPeriod.NER_Rate, bDT_IND_NextPeriod.NER_Rate);

			var cNY_IND_NextPeriod_Actual = indRates.First(x => x.NER_RX_NKCurrency == "CNY");
			AssertEquals("CNY rate is unchanged, it was not present in the previous period", cNY_IND_NextPeriod.NER_Rate, cNY_IND_NextPeriod_Actual.NER_Rate);

			var nOK_IND_NextPeriod_Actual = indRates.First(x => x.NER_RX_NKCurrency == "NOK");
			AssertEquals("NOK rate is unchanged since it had some pre-existing value", nOK_IND_NextPeriod.NER_Rate, nOK_IND_NextPeriod_Actual.NER_Rate);

			var sEK_IND_NextPeriod_Actual = indRates.First(x => x.NER_RX_NKCurrency == "SEK");
			AssertEquals("SEK rate is unchanged since it had some pre-existing value", sEK_IND_NextPeriod.NER_Rate, sEK_IND_NextPeriod_Actual.NER_Rate);

			query = new ZQuery(NettingSystemExchangeRateSchema.NER_NSP_Period, nextPeriod.PK);
			query.AddToFilter(NettingSystemExchangeRateSchema.NER_RateType, "NET");

			var netRates = newFactory.Load<NettingSystemExchangeRate>(query);
			AssertEquals("Number of NET exchange rates for next period", 1, netRates.Length);

			AssertEquals("No change has been done to NET rate", jPY_NET_NextPeriod.NER_Rate, netRates.First().NER_Rate);
		}

		public void TestSettlementAtLineLevel()
		{
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.AALSHI.PK.ToGuid());

			SetupParticipant(participant1, "FUL", "USD", "USD", "USD");
			SetupParticipant(participant2, "FUL", "USD", "USD", "USD");

			NettingReceivableTransaction receivableTransaction1;
			NettingPayableTransaction payableTransaction1;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "INV101", "USD", 1200M, 1200M, out receivableTransaction1, out payableTransaction1);

			Factory.Save();

			var receivableLine1 = TestObjectCreator.CreateNettingTransactionLine(receivableTransaction1, "S00123", 700M, "USD");
			var receivableLine2 = TestObjectCreator.CreateNettingTransactionLine(receivableTransaction1, "S00124", 500M, "USD");

			var payableLine1 = TestObjectCreator.CreateNettingTransactionLine(payableTransaction1, "S00123", 700M, "USD");
			var payableLine2 = TestObjectCreator.CreateNettingTransactionLine(payableTransaction1, "S00124", 500M, "USD");

			Factory.Save();

			var pivot1 = Factory.New<NettingMatchPivot>();
			pivot1.NMP_NSP_Period = period.PK;
			pivot1.NMP_NRL_ReceivableLine = receivableLine1.PK;
			pivot1.NMP_NPL_PayableLine = payableLine1.PK;

			var pivot2 = Factory.New<NettingMatchPivot>();
			pivot2.NMP_NSP_Period = period.PK;
			pivot2.NMP_NRL_ReceivableLine = receivableLine2.PK;
			pivot2.NMP_NPL_PayableLine = payableLine2.PK;

			receivableTransaction1.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			payableTransaction1.ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			Factory.Save();

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			var calculations = Factory.Load<NettingCalculation>(new ZQuery());
			AssertEquals("calculation count", 4, calculations.Length);

			AssertCalculation("transaction 1 participant 1", calculations, pivot1, participant1, "USD", 700M, 636.36M, 1.1M);
			AssertCalculation("transaction 2 participant 1", calculations, pivot2, participant1, "USD", 500.00M, 454.55M, 1.1M);

			AssertCalculation("transaction 1 participant 2", calculations, pivot1, participant2, "USD", -700M, -636.36M, 1.1M);
			AssertCalculation("transaction 2 participant 2", calculations, pivot2, participant2, "USD", -500.00m, -454.55m, 1.1M);
		}

		public void TestCreationofJournals_FullNetting()
		{
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.AALSHI.PK.ToGuid());

			SetupParticipant(participant1, "FUL", "GBP", "EUR", "USD");
			SetupParticipant(participant2, "FUL", "USD", "EUR", "USD");

			var transaction1 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "USD 1000", "USD", 1000m);
			var transaction2 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "EUR 875", "EUR", 875m);
			var transaction3 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "USD 400", "USD", 400m);
			var transaction4 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "EUR 350", "EUR", 350m);

			Factory.Save();

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			var calculations = Factory.Load<NettingCalculation>(new ZQuery());
			AssertEquals("calculation count", 8, calculations.Length);

			AssertCalculation("transaction 1 participant 1", calculations, transaction1, participant1, "GBP", 600m, 909.09m, 0.66m);
			AssertCalculation("transaction 2 participant 1", calculations, transaction2, participant1, "GBP", 614.36m, 930.85m, 0.66m);
			AssertCalculation("transaction 3 participant 1", calculations, transaction3, participant1, "GBP", -240m, -363.64m, 0.66m);
			AssertCalculation("transaction 4 participant 1", calculations, transaction4, participant1, "GBP", -245.74m, -372.34m, 0.66m);

			AssertCalculation("transaction 1 participant 2", calculations, transaction1, participant2, "USD", -1000m, -909.09m, 1.1m);
			AssertCalculation("transaction 2 participant 2", calculations, transaction2, participant2, "USD", -1023.94m, -930.85m, 1.1m);
			AssertCalculation("transaction 3 participant 2", calculations, transaction3, participant2, "USD", 400m, 363.64m, 1.1m);
			AssertCalculation("transaction 4 participant 2", calculations, transaction4, participant2, "USD", 409.57m, 372.34m, 1.1m);

			var expectedJournals = new[]
			{
				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AP", "EUR", DebitCredit.DR, 1037.72m, 1103.96m, 0.939998m),
				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AR", "USD", DebitCredit.CR, 1214.370m, 1103.96m, 1.100013m)
			};

			AssertJournals(expectedJournals, Factory.Load<TransactionHeader>(new ZQuery()));
		}

		protected override void TearDown()
		{
			base.TearDown();

			DirectoryInfo di = new DirectoryInfo(Path.Combine(Enterprise.Environment.Env.TempPath, period.NSP_Period));
			try
			{
				di.Delete(true);
			}
			catch { }
		}

		public void TestCreationofJournals_FullNetting_AllOneCurrency()
		{
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.AALSHI.PK.ToGuid());

			SetupParticipant(participant1, "FUL", LocalCurrency, LocalCurrency, LocalCurrency);
			SetupParticipant(participant2, "FUL", LocalCurrency, LocalCurrency, LocalCurrency);

			var transaction1 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "1000", LocalCurrency, 1000m);
			var transaction2 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "875", LocalCurrency, 875m);
			var transaction3 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "400", LocalCurrency, 400m);
			var transaction4 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "350", LocalCurrency, 350m);

			Factory.Save();

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			var calculations = Factory.Load<NettingCalculation>(new ZQuery());
			AssertEquals("calculation count", 8, calculations.Length);

			AssertCalculation("transaction 1 participant 1", calculations, transaction1, participant1, LocalCurrency, 1000m, 1000m, 1m);
			AssertCalculation("transaction 2 participant 1", calculations, transaction2, participant1, LocalCurrency, 875m, 875m, 1m);
			AssertCalculation("transaction 3 participant 1", calculations, transaction3, participant1, LocalCurrency, -400m, -400m, 1m);
			AssertCalculation("transaction 4 participant 1", calculations, transaction4, participant1, LocalCurrency, -350m, -350m, 1m);

			AssertCalculation("transaction 1 participant 2", calculations, transaction1, participant2, LocalCurrency, -1000m, -1000m, 1m);
			AssertCalculation("transaction 2 participant 2", calculations, transaction2, participant2, LocalCurrency, -875m, -875m, 1m);
			AssertCalculation("transaction 3 participant 2", calculations, transaction3, participant2, LocalCurrency, 400m, 400m, 1m);
			AssertCalculation("transaction 4 participant 2", calculations, transaction4, participant2, LocalCurrency, 350m, 350m, 1m);

			var expectedJournals = new[]
			{
				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AP", LocalCurrency, DebitCredit.DR, 1125m, 1125m, 1m),
				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AR", LocalCurrency, DebitCredit.CR, 1125m, 1125m, 1m)
			};

			AssertJournals(expectedJournals, Factory.Load<TransactionHeader>(new ZQuery()));
		}

		public void TestCreationofJournals_HomeNetting()
		{
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.AALSHI.PK.ToGuid());

			SetupParticipant(participant1, "HOM", "GBP", "EUR", "USD");
			SetupParticipant(participant2, "HOM", "USD", "EUR", "USD");

			var transaction1 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "USD 1000", "USD", 1000m);
			var transaction2 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "EUR 875", "EUR", 875m);
			var transaction3 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "USD 400", "USD", 400m);
			var transaction4 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "EUR 350", "EUR", 350m);

			Factory.Save();

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			var calculations = Factory.Load<NettingCalculation>(new ZQuery());
			AssertEquals("calculation count", 8, calculations.Length);

			AssertCalculation("transaction 1 participant 1", calculations, transaction1, participant1, "GBP", 600m, 909.09m, 0.66m);
			AssertCalculation("transaction 2 participant 1", calculations, transaction2, participant1, "GBP", 614.36m, 930.85m, 0.66m);
			AssertCalculation("transaction 3 participant 1", calculations, transaction3, participant1, "GBP", -240m, -363.64m, 0.66m);
			AssertCalculation("transaction 4 participant 1", calculations, transaction4, participant1, "GBP", -245.74m, -372.34m, 0.66m);

			AssertCalculation("transaction 1 participant 2", calculations, transaction1, participant2, "USD", -1000m, -909.09m, 1.1m);
			AssertCalculation("transaction 2 participant 2", calculations, transaction2, participant2, "USD", -1023.94m, -930.85m, 1.1m);
			AssertCalculation("transaction 3 participant 2", calculations, transaction3, participant2, "USD", 400m, 363.64m, 1.1m);
			AssertCalculation("transaction 4 participant 2", calculations, transaction4, participant2, "USD", 409.57m, 372.34m, 1.1m);

			var expectedJournals = new[]
			{
				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AP", "EUR", DebitCredit.DR, 1729.54m, 1839.94m, 0.939998m),
				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AR", "USD", DebitCredit.CR, 809.58m, 735.98m, 1.100003m),

				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AP", "EUR", DebitCredit.DR, 691.82m, 735.98m, 0.939998m),
				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AR", "USD", DebitCredit.CR, 2023.94m, 1839.94m, 1.100003m)
			};

			AssertJournals(expectedJournals, Factory.Load<TransactionHeader>(new ZQuery()));
		}

		public void TestCreationofJournals_CurrencyNetting()
		{
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.AALSHI.PK.ToGuid());

			SetupParticipant(participant1, "CUR", "GBP", "EUR", "USD");
			SetupParticipant(participant2, "CUR", "USD", "EUR", "USD");

			var transaction1 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "USD 1000", "USD", 1000m);
			var transaction2 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "EUR 875", "EUR", 875m);
			var transaction3 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "USD 400", "USD", 400m);
			var transaction4 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "EUR 350", "EUR", 350m);

			Factory.Save();

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			var calculations = Factory.Load<NettingCalculation>(new ZQuery());
			AssertEquals("calculation count", 8, calculations.Length);

			AssertCalculation("transaction 1 participant 1", calculations, transaction1, participant1, "GBP", 600m, 909.09m, 0.66m);
			AssertCalculation("transaction 2 participant 1", calculations, transaction2, participant1, "GBP", 614.36m, 930.85m, 0.66m);
			AssertCalculation("transaction 3 participant 1", calculations, transaction3, participant1, "GBP", -240m, -363.64m, 0.66m);
			AssertCalculation("transaction 4 participant 1", calculations, transaction4, participant1, "GBP", -245.74m, -372.34m, 0.66m);

			AssertCalculation("transaction 1 participant 2", calculations, transaction1, participant2, "USD", -1000m, -909.09m, 1.1m);
			AssertCalculation("transaction 2 participant 2", calculations, transaction2, participant2, "USD", -1023.94m, -930.85m, 1.1m);
			AssertCalculation("transaction 3 participant 2", calculations, transaction3, participant2, "USD", 400m, 363.64m, 1.1m);
			AssertCalculation("transaction 4 participant 2", calculations, transaction4, participant2, "USD", 409.57m, 372.34m, 1.1m);

			var expectedJournals = new[]
			{
				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AP", "USD", DebitCredit.DR, 600m, 545.45m, 1.100009m),
				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AP", "EUR", DebitCredit.DR, 525m, 558.51m, 0.940001m),

				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AR", "USD", DebitCredit.CR, 600m, 545.45m, 1.100009m),
				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AR", "EUR", DebitCredit.CR, 525m, 558.51m, 0.940001m)
			};

			AssertJournals(expectedJournals, Factory.Load<TransactionHeader>(new ZQuery()));
		}

		public void TestCreationofJournals_GrossNetting()
		{
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.AALSHI.PK.ToGuid());

			SetupParticipant(participant1, "GRS", "GBP", "EUR", "USD");
			SetupParticipant(participant2, "GRS", "USD", "EUR", "USD");

			var transaction1 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "USD 1000", "USD", 1000m);
			var transaction2 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "EUR 875", "EUR", 875m);
			var transaction3 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "USD 400", "USD", 400m);
			var transaction4 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "EUR 350", "EUR", 350m);
			Factory.Save();

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			var calculations = Factory.Load<NettingCalculation>(new ZQuery());
			AssertEquals("calculation count", 8, calculations.Length);

			AssertCalculation("transaction 1 participant 1", calculations, transaction1, participant1, "GBP", 600m, 909.09m, 0.66m);
			AssertCalculation("transaction 2 participant 1", calculations, transaction2, participant1, "GBP", 614.36m, 930.85m, 0.66m);
			AssertCalculation("transaction 3 participant 1", calculations, transaction3, participant1, "GBP", -240m, -363.64m, 0.66m);
			AssertCalculation("transaction 4 participant 1", calculations, transaction4, participant1, "GBP", -245.74m, -372.34m, 0.66m);

			AssertCalculation("transaction 1 participant 2", calculations, transaction1, participant2, "USD", -1000m, -909.09m, 1.1m);
			AssertCalculation("transaction 2 participant 2", calculations, transaction2, participant2, "USD", -1023.94m, -930.85m, 1.1m);
			AssertCalculation("transaction 3 participant 2", calculations, transaction3, participant2, "USD", 400m, 363.64m, 1.1m);
			AssertCalculation("transaction 4 participant 2", calculations, transaction4, participant2, "USD", 409.57m, 372.34m, 1.1m);

			var expectedJournals = new[]
			{
				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AP", "USD", DebitCredit.DR, 1000m, 909.09m, 1.100001m),
				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AR", "USD", DebitCredit.CR, 1000m, 909.09m, 1.100001m),

				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AP", "EUR", DebitCredit.DR, 875m, 930.85m, 0.940001m),
				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AR", "EUR", DebitCredit.CR, 875m, 930.85m, 0.940001m),

				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AR", "USD", DebitCredit.CR, 400m, 363.64m, 1.099989m),
				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AP", "USD", DebitCredit.DR, 400m, 363.64m, 1.099989m),

				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AR", "EUR", DebitCredit.CR, 350m, 372.34m, 0.940001m),
				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AP", "EUR", DebitCredit.DR, 350m, 372.34m, 0.940001m)
			};

			AssertJournals(expectedJournals, Factory.Load<TransactionHeader>(new ZQuery()));
		}

		public void TestSimpleCaseWithOneInvoice()
		{
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.AALSHI.PK.ToGuid());

			SetupParticipant(participant1, "FUL", LocalCurrency, LocalCurrency, LocalCurrency);
			SetupParticipant(participant2, "FUL", LocalCurrency, LocalCurrency, LocalCurrency);

			var transaction1 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "reference", LocalCurrency, 1000m);

			Factory.Save();

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			var calculations = Factory.Load<NettingCalculation>(new ZQuery());
			AssertEquals("calculation count", 2, calculations.Length);

			AssertCalculation("transaction 1 participant 1", calculations, transaction1, participant1, LocalCurrency, 1000m, 1000m, 1m);
			AssertCalculation("transaction 1 participant 2", calculations, transaction1, participant2, LocalCurrency, -1000m, -1000m, 1m);

			var expectedJournals = new[]
			{
				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AP", LocalCurrency, DebitCredit.DR, 1000m, 1000m, 1m),	// the netting administrator will pay USD 1000 to participant1 .
				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AR", LocalCurrency, DebitCredit.CR, 1000m, 1000m, 1m)		// the netting administrator will receive USD 1000 from participant2.
			};

			AssertJournals(expectedJournals, Factory.Load<TransactionHeader>(new ZQuery()));
		}

		public void TestGenerateJournalForFXOffers_Case1()
		{
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.AALSHI.PK.ToGuid());

			SetupParticipant(participant1, "FUL", LocalCurrency, LocalCurrency, LocalCurrency);
			SetupParticipant(participant2, "FUL", LocalCurrency, LocalCurrency, LocalCurrency);

			var transaction1 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "reference", LocalCurrency, 1000m);
			var fxOffer = CreateFXOffer(period, participant2, "OFF", "APP", "desc", LocalCurrency, 400m);
			var fxRequest = CreateFXOffer(period, participant1, "REQ", "APP", "desc", LocalCurrency, 300m);
			Factory.Save();

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			var expectedJournals = new[]
			{
				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AP", LocalCurrency, DebitCredit.DR, 700m, 700m, 1m),	// transaction + adjustment of request
				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AP", LocalCurrency, DebitCredit.DR, 300m, 300m, 1m),		// original journal for fx request

				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AR", LocalCurrency, DebitCredit.CR, 600m, 600m, 1m),	// transaction + adjustment of offer
				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AR", LocalCurrency, DebitCredit.CR, 400m, 400m, 1m),		// original journal for fx offer
			};

			AssertJournals(expectedJournals, Factory.Load<TransactionHeader>(new ZQuery()));
		}

		public void TestGenerateJournalForFXOffers_Case2()
		{
			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.AALSHI.PK.ToGuid());

			SetupParticipant(participant1, "FUL", LocalCurrency, LocalCurrency, LocalCurrency);
			SetupParticipant(participant2, "FUL", LocalCurrency, LocalCurrency, LocalCurrency);

			var transaction1 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "reference", LocalCurrency, 1000m);
			var fxOffer = CreateFXOffer(period, participant1, "OFF", "APP", "desc", LocalCurrency, 400m);
			var fxRequest = CreateFXOffer(period, participant2, "REQ", "APP", "desc", LocalCurrency, 300m);
			Factory.Save();

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			var expectedJournals = new[]
			{
				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AP", LocalCurrency, DebitCredit.DR, 1400m, 1400m, 1m),	// transaction + adjustment of offer
				new JournalInfoForTesting(participant1.Organisation.OH_Code, "AR", LocalCurrency, DebitCredit.CR, 400m, 400m, 1m),		// original journal for fx offer

				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AR", LocalCurrency, DebitCredit.CR, 1300m, 1300m, 1m),	// transaction + adjustment of request
				new JournalInfoForTesting(participant2.Organisation.OH_Code, "AP", LocalCurrency, DebitCredit.DR, 300m, 300m, 1m),		// original journal for fx request
			};

			AssertJournals(expectedJournals, Factory.Load<TransactionHeader>(new ZQuery()));
		}

		[TestDate(2015, 04, 20)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateAndSendJournalImportFileToParticipants()
		{
			using (Factory.AddDisposableService())
			{
				SetupMethod();

				var transaction1 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "USD 1000", "USD", 1000m);
				var transaction2 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "EUR 875", "EUR", 875m);
				var transaction3 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "USD 400", "USD", 400m);
				var transaction4 = TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "EUR 350", "EUR", 350m);

				CreateFXOffer(period, participant1, "OFF", "APP", "FX Offer", "AUD", 200M);
				CreateFXOffer(period, participant2, "REQ", "APP", "FX Request", "AUD", 300M);

				Factory.Save();

				AssertEquals("Precondition: EDIMessage table should be empty.", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));

				new NettingPeriodManager(period).FinaliseNettingCycle(notification);

				var tempFileDirectory = Env.TempPath;
				string org1FileName = string.Empty;
				using (TextReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.Netting.Testing\HYEDUSDAT.csv"))
				{
					org1FileName = Path.Combine(tempFileDirectory, period.NSP_Period, string.Format("{0}.csv", org1.OH_Code));
					AssertFileSameAsString(org1FileName, reader.ReadToEnd());
				}

				string org2FileName = string.Empty;
				using (TextReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.Netting.Testing\HYEDAUDAT.csv"))
				{
					org2FileName = Path.Combine(tempFileDirectory, period.NSP_Period, string.Format("{0}.csv", org2.OH_Code));
					AssertFileSameAsString(org2FileName, reader.ReadToEnd());
				}

				AssertEquals("EDIMessage table shoulb have 2 message.", 2, Factory.GetDatabaseCount(typeof(EDIMessage)));
			}
		}

		[TestDate(2016, 09, 28)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateAndSendJournalImportFileToParticipants_OneToOneMatch()
		{
			SetupMethod();

			TestObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "1To1MatchAtHeader_SameAmount", "AUD", 1000M);

			NettingReceivableTransaction arTransaction1 = null;
			NettingPayableTransaction apTransaction1 = null;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "1To1MatchAtHeader_DifferentAmount", "AUD", 500M, 499.99M, out arTransaction1, out apTransaction1);

			var pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRT_ReceivableTransaction = arTransaction1.PK;
			pivot.NMP_NPT_PayableTransaction = apTransaction1.PK;

			arTransaction1.NRT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction1.NPT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			NettingReceivableTransaction arTransaction2 = null;
			NettingPayableTransaction apTransaction2 = null;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "1To1MatchAtLine_SameAmount", "AUD", 100M, 100M, out arTransaction2, out apTransaction2);

			var receivableLine1 = TestObjectCreator.CreateNettingTransactionLine(arTransaction2, "S001001", 100M, "AUD");
			var payableLine1 = TestObjectCreator.CreateNettingTransactionLine(apTransaction2, "S001001", 100M, "AUD");

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivableLine1.PK;
			pivot.NMP_NPL_PayableLine = payableLine1.PK;

			arTransaction2.NRT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction2.NPT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			NettingReceivableTransaction arTransaction3 = null;
			NettingPayableTransaction apTransaction3 = null;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "1To1MatchAtLine_DifferentAmount", "AUD", 50M, 49.99M, out arTransaction3, out apTransaction3);

			var receivableLine2 = TestObjectCreator.CreateNettingTransactionLine(arTransaction3, "S001002", 50M, "AUD");
			var payableLine2 = TestObjectCreator.CreateNettingTransactionLine(apTransaction3, "S001002", 49.99M, "AUD");

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivableLine2.PK;
			pivot.NMP_NPL_PayableLine = payableLine2.PK;

			arTransaction3.NRT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction3.NPT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			Factory.Save();

			AssertEquals("Precondition: EDIMessage table should be empty.", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			var tempFileDirectory = Env.TempPath;
			string org1FileName = string.Empty;
			using (TextReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.Netting.Testing\HYEDUSDAT_OneToOne.csv"))
			{
				org1FileName = Path.Combine(tempFileDirectory, period.NSP_Period, string.Format("{0}.csv", org1.OH_Code));
				AssertFileSameAsString(org1FileName, reader.ReadToEnd());
			}

			string org2FileName = string.Empty;
			using (TextReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.Netting.Testing\HYEDAUDAT_OneToOne.csv"))
			{
				org2FileName = Path.Combine(tempFileDirectory, period.NSP_Period, string.Format("{0}.csv", org2.OH_Code));
				AssertFileSameAsString(org2FileName, reader.ReadToEnd());
			}

			AssertEquals("EDIMessage table shoulb have 2 message.", 2, Factory.GetDatabaseCount(typeof(EDIMessage)));
		}

		[TestDate(2016, 09, 28)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateAndSendJournalImportFileToParticipants_OneToManyMatch()
		{
			SetupMethod();

			NettingReceivableTransaction arTransaction1 = null;
			NettingPayableTransaction apTransaction1 = null;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "1ToMnyMatchAtHeader_SameAmount", "AUD", 1000M, 500M, out arTransaction1, out apTransaction1);
			INettingTransaction apTransaction1_2 = TestObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "1ToMnyMatchAtHeader_SameAmt1", "AUD", 500M);

			var pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRT_ReceivableTransaction = arTransaction1.PK;
			pivot.NMP_NPT_PayableTransaction = apTransaction1.PK;

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRT_ReceivableTransaction = arTransaction1.PK;
			pivot.NMP_NPT_PayableTransaction = apTransaction1_2.PK;

			arTransaction1.NRT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction1.NPT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction1_2.ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			NettingReceivableTransaction arTransaction2 = null;
			NettingPayableTransaction apTransaction2 = null;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "1ToMnyMatchAtHeader_DifferentAmount", "AUD", 500M, 200M, out arTransaction2, out apTransaction2);
			INettingTransaction apTransaction2_2 = TestObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "1ToMnyMatchAtHeader_DifferentAmt1", "AUD", 299.99M);

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRT_ReceivableTransaction = arTransaction2.PK;
			pivot.NMP_NPT_PayableTransaction = apTransaction2.PK;

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRT_ReceivableTransaction = arTransaction2.PK;
			pivot.NMP_NPT_PayableTransaction = apTransaction2_2.PK;

			arTransaction2.NRT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction2.NPT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction2_2.ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			NettingReceivableTransaction arTransaction3 = null;
			NettingPayableTransaction apTransaction3 = null;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "1ToMnyMatchAtLine_SameAmount", "AUD", 100M, 50M, out arTransaction3, out apTransaction3);
			INettingTransaction apTransaction3_2 = TestObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "1ToMnyMatchAtLine_SameAmt1", "AUD", 50M);

			var receivable3Line1 = TestObjectCreator.CreateNettingTransactionLine(arTransaction3, "S001001", 100M, "AUD");
			var payable3Line1 = TestObjectCreator.CreateNettingTransactionLine(apTransaction3, "S001001", 50M, "AUD");
			var payable3_2Line1 = TestObjectCreator.CreateNettingTransactionLine(apTransaction3_2, "S001001", 50M, "AUD");

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivable3Line1.PK;
			pivot.NMP_NPL_PayableLine = payable3Line1.PK;

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivable3Line1.PK;
			pivot.NMP_NPL_PayableLine = payable3_2Line1.PK;

			arTransaction3.NRT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction3.NPT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction3_2.ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			NettingReceivableTransaction arTransaction4 = null;
			NettingPayableTransaction apTransaction4 = null;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "1ToMnyMatchAtLine_DifferentAmount", "AUD", 50M, 20M, out arTransaction4, out apTransaction4);
			INettingTransaction apTransaction4_2 = TestObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "1ToMnyMatchAtLine_DifferentAmt1", "AUD", 29.99M);

			var receivable4Line1 = TestObjectCreator.CreateNettingTransactionLine(arTransaction4, "S001002", 50M, "AUD");
			var payable4Line1 = TestObjectCreator.CreateNettingTransactionLine(apTransaction4, "S001002", 20M, "AUD");
			var payable4_2Line1 = TestObjectCreator.CreateNettingTransactionLine(apTransaction4_2, "S001002", 29.99M, "AUD");

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivable4Line1.PK;
			pivot.NMP_NPL_PayableLine = payable4Line1.PK;

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivable4Line1.PK;
			pivot.NMP_NPL_PayableLine = payable4_2Line1.PK;

			arTransaction4.NRT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction4.NPT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction4_2.ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			NettingReceivableTransaction arTransaction5 = null;
			NettingPayableTransaction apTransaction5 = null;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "1ToMnyMatchAtLine_DiffAmount_C2", "AUD", 20M, 12M, out arTransaction5, out apTransaction5);
			INettingTransaction apTransaction5_2 = TestObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "1ToMnyMatchAtLine_DiffAmt1_C2", "AUD", 8M);

			var receivable5Line1 = TestObjectCreator.CreateNettingTransactionLine(arTransaction5, "S001002", 12M, "AUD");
			var receivable5Line2 = TestObjectCreator.CreateNettingTransactionLine(arTransaction5, "S001002", 8M, "AUD");
			var payable5Line1 = TestObjectCreator.CreateNettingTransactionLine(apTransaction5, "S001002", 12M, "AUD");
			var payable5_2Line1 = TestObjectCreator.CreateNettingTransactionLine(apTransaction5_2, "S001002", 8M, "AUD");

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivable5Line1.PK;
			pivot.NMP_NPL_PayableLine = payable5Line1.PK;

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivable5Line2.PK;
			pivot.NMP_NPL_PayableLine = payable5_2Line1.PK;

			arTransaction5.NRT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction5.NPT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction5_2.ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			Factory.Save();

			AssertEquals("Precondition: EDIMessage table should be empty.", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			var tempFileDirectory = Env.TempPath;
			string org1FileName = string.Empty;
			using (TextReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.Netting.Testing\HYEDUSDAT_OneToMany.csv"))
			{
				org1FileName = Path.Combine(tempFileDirectory, period.NSP_Period, string.Format("{0}.csv", org1.OH_Code));
				AssertFileSameAsString(org1FileName, reader.ReadToEnd());
			}

			string org2FileName = string.Empty;
			using (TextReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.Netting.Testing\HYEDAUDAT_OneToMany.csv"))
			{
				org2FileName = Path.Combine(tempFileDirectory, period.NSP_Period, string.Format("{0}.csv", org2.OH_Code));
				AssertFileSameAsString(org2FileName, reader.ReadToEnd());
			}

			AssertEquals("EDIMessage table shoulb have 2 message.", 2, Factory.GetDatabaseCount(typeof(EDIMessage)));
		}

		[TestDate(2016, 09, 28)]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGenerateAndSendJournalImportFileToParticipants_ManyToOneMatch()
		{
			SetupMethod();

			NettingReceivableTransaction arTransaction1 = null;
			NettingPayableTransaction apTransaction1 = null;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "MnyTo1MatchAtHeader_SameAmount", "AUD", 500M, 1000M, out arTransaction1, out apTransaction1);
			INettingTransaction arTransaction1_2 = TestObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "MnyTo1MatchAtHeader_SameAmt1", "AUD", 500M);

			var pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRT_ReceivableTransaction = arTransaction1.PK;
			pivot.NMP_NPT_PayableTransaction = apTransaction1.PK;

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRT_ReceivableTransaction = arTransaction1_2.PK;
			pivot.NMP_NPT_PayableTransaction = apTransaction1.PK;

			arTransaction1.NRT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			arTransaction1_2.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction1.NPT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			NettingReceivableTransaction arTransaction2 = null;
			NettingPayableTransaction apTransaction2 = null;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "MnyTo1MatchAtHeader_DifferentAmount", "AUD", 200M, 500M, out arTransaction2, out apTransaction2);
			INettingTransaction arTransaction2_2 = TestObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "MnyTo1MatchAtHeader_DifferentAmt1", "AUD", 299.99M);

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRT_ReceivableTransaction = arTransaction2.PK;
			pivot.NMP_NPT_PayableTransaction = apTransaction2.PK;

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRT_ReceivableTransaction = arTransaction2_2.PK;
			pivot.NMP_NPT_PayableTransaction = apTransaction2.PK;

			arTransaction2.NRT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			arTransaction2_2.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction2.NPT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			NettingReceivableTransaction arTransaction3 = null;
			NettingPayableTransaction apTransaction3 = null;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "MnyTo1MatchAtLine_SameAmount", "AUD", 50M, 100M, out arTransaction3, out apTransaction3);
			INettingTransaction arTransaction3_2 = TestObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "MnyTo1MatchAtLine_SameAmt1", "AUD", 50M);

			var receivable3Line1 = TestObjectCreator.CreateNettingTransactionLine(arTransaction3, "S001001", 50M, "AUD");
			var receivable3_2Line1 = TestObjectCreator.CreateNettingTransactionLine(arTransaction3_2, "S001001", 50M, "AUD");
			var payable3Line1 = TestObjectCreator.CreateNettingTransactionLine(apTransaction3, "S001001", 100M, "AUD");

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivable3Line1.PK;
			pivot.NMP_NPL_PayableLine = payable3Line1.PK;

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivable3_2Line1.PK;
			pivot.NMP_NPL_PayableLine = payable3Line1.PK;

			arTransaction3.NRT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			arTransaction3_2.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction3.NPT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			NettingReceivableTransaction arTransaction4 = null;
			NettingPayableTransaction apTransaction4 = null;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "MnyTo1MatchAtLine_DifferentAmount", "AUD", 20M, 50M, out arTransaction4, out apTransaction4);
			INettingTransaction arTransaction4_2 = TestObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "MnyTo1MatchAtLine_DifferentAmt1", "AUD", 29.99M);

			var receivable4Line1 = TestObjectCreator.CreateNettingTransactionLine(arTransaction4, "S001002", 20M, "AUD");
			var receivable4_2Line1 = TestObjectCreator.CreateNettingTransactionLine(arTransaction4_2, "S001002", 29.99M, "AUD");
			var payable4Line1 = TestObjectCreator.CreateNettingTransactionLine(apTransaction4, "S001002", 50M, "AUD");

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivable4Line1.PK;
			pivot.NMP_NPL_PayableLine = payable4Line1.PK;

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivable4_2Line1.PK;
			pivot.NMP_NPL_PayableLine = payable4Line1.PK;

			arTransaction4.NRT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			arTransaction4_2.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction4.NPT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			NettingReceivableTransaction arTransaction4_case2 = null;
			NettingPayableTransaction apTransaction4_case2 = null;
			TestObjectCreator.CreateNettingTransactionPairWithoutMatching(period, participant1, participant2, "MnyTo1MatchAtLine_SameAmount_C2", "AUD", 12M, 20M, out arTransaction4_case2, out apTransaction4_case2);
			INettingTransaction arTransaction4_2_case2 = TestObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "MnyTo1MatchAtLine_SameAmt1_C2", "AUD", 8M);

			var receivable4case2Line1 = TestObjectCreator.CreateNettingTransactionLine(arTransaction4_case2, "S001002", 12M, "AUD");
			var receivable4case2_2Line1 = TestObjectCreator.CreateNettingTransactionLine(arTransaction4_2_case2, "S001002", 8M, "AUD");

			var payable4case2Line1 = TestObjectCreator.CreateNettingTransactionLine(apTransaction4_case2, "S001002", 12M, "AUD");
			var payable4case2Line2 = TestObjectCreator.CreateNettingTransactionLine(apTransaction4_case2, "S001002", 8M, "AUD");

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivable4case2Line1.PK;
			pivot.NMP_NPL_PayableLine = payable4case2Line1.PK;

			pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRL_ReceivableLine = receivable4case2_2Line1.PK;
			pivot.NMP_NPL_PayableLine = payable4case2Line2.PK;

			arTransaction4_case2.NRT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			arTransaction4_2_case2.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction4_case2.NPT_ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			Factory.Save();

			AssertEquals("Precondition: EDIMessage table should be empty.", 0, Factory.GetDatabaseCount(typeof(EDIMessage)));

			new NettingPeriodManager(period).FinaliseNettingCycle(notification);

			var tempFileDirectory = Env.TempPath;
			string org1FileName = string.Empty;
			using (TextReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.Netting.Testing\HYEDUSDAT_ManyToOne.csv"))
			{
				org1FileName = Path.Combine(tempFileDirectory, period.NSP_Period, string.Format("{0}.csv", org1.OH_Code));
				AssertFileSameAsString(org1FileName, reader.ReadToEnd());
			}

			string org2FileName = string.Empty;
			using (TextReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.Netting.Testing\HYEDAUDAT_ManyToOne.csv"))
			{
				org2FileName = Path.Combine(tempFileDirectory, period.NSP_Period, string.Format("{0}.csv", org2.OH_Code));
				AssertFileSameAsString(org2FileName, reader.ReadToEnd());
			}

			AssertEquals("EDIMessage table shoulb have 2 message.", 2, Factory.GetDatabaseCount(typeof(EDIMessage)));
		}

		void SetupMethod()
		{
			string org1eHubID = "HYEDUSDAT";
			org1.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, org1eHubID);
			AddNettingCommunicationMode(org1, org1eHubID);

			string org2eHubID = "HYEDAUDAT";
			org2.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, org2eHubID);
			AddNettingCommunicationMode(org2, org2eHubID);

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = TestObjectCreator.AALSHI.PK;

			AccountingConfigurationRegistry.Instance.NettingSystemOrg.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.AALSHI.PK.ToGuid());
			EDICommunicationsModeDependentCollection modes_Participant1 = new EDICommunicationsModeDependentCollection(TestObjectCreator.AALSHI);
			var participant1Mode = modes_Participant1.AddNew();
			participant1Mode.EK_Module = EDICommunicationsMode.Modules.Netting;
			participant1Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;
			participant1Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			participant1Mode.EK_Destination = "HYEDUSDAT";

			EDICommunicationsModeDependentCollection modes_Participant2 = new EDICommunicationsModeDependentCollection(TestObjectCreator.AALSHI);
			var participant2Mode = modes_Participant1.AddNew();
			participant2Mode.EK_Module = EDICommunicationsMode.Modules.Netting;
			participant2Mode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;
			participant2Mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			participant2Mode.EK_Destination = "HYEDAUDAT";

			SetupParticipant(participant1, "FUL", LocalCurrency, LocalCurrency, LocalCurrency);
			SetupParticipant(participant2, "FUL", LocalCurrency, LocalCurrency, LocalCurrency);
		}

		void AddNettingCommunicationMode(OrgHeader orgHeader, ZString destination)
		{
			EDICommunicationsModeDependentCollection mode_NC = new EDICommunicationsModeDependentCollection(orgHeader);
			var mode1 = mode_NC.AddNew();
			mode1.EK_Module = EDICommunicationsMode.Modules.Netting;
			mode1.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction;
			mode1.EK_Destination = destination;
			mode1.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
		}

		void AssertCalculation(string message, NettingCalculation[] calculations, NettingMatchPivot pivot, NettingOrganisation participant, string currency, decimal organisationAmount, decimal nettingSystemAmount, decimal rate)
		{
			var calculation = calculations.First(x => x.NPC_NMP_Pivot == pivot.PK && x.NPC_NSO_Organisation == participant.PK && x.NPC_NSP_Period == pivot.NMP_NSP_Period);

			CombineAssertions(message, () =>
			{
				AssertEquals("Currency", currency, calculation.NPC_RX_NKNettingCurrency);
				AssertEquals("Participant Amount", organisationAmount, calculation.NPC_NettingOrganisationAmount);
				AssertEquals("Netting System Amount", nettingSystemAmount, calculation.NPC_NettingSystemAmount);
				AssertEquals("Rate", rate, calculation.NPC_NettingRate);

				var newFactory = new BusinessObjectFactory();
				NettingReceivableTransaction receivableTransactionInNewFactory = null;
				NettingPayableTransaction payableTransactionInNewFactory = null;
				if (!calculation.MatchingPivot.NMP_NRT_ReceivableTransaction.IsEmpty && !calculation.MatchingPivot.NMP_NPT_PayableTransaction.IsEmpty)
				{
					receivableTransactionInNewFactory = newFactory.Load<NettingReceivableTransaction>(calculation.MatchingPivot.NMP_NRT_ReceivableTransaction);
					payableTransactionInNewFactory = newFactory.Load<NettingPayableTransaction>(calculation.MatchingPivot.NMP_NPT_PayableTransaction);
				}
				else if (!calculation.MatchingPivot.NMP_NRL_ReceivableLine.IsEmpty && !calculation.MatchingPivot.NMP_NPL_PayableLine.IsEmpty)
				{
					var subQuery = new ZDBOnlySubQuery(typeof(NettingReceivableTransactionLine), NettingReceivableTransactionLineSchema.NRL_NRT_Transaction);
					subQuery.AddToFilter(NettingReceivableTransactionLineSchema.PK, calculation.MatchingPivot.NMP_NRL_ReceivableLine);

					var query = new ZDBOnlyQuery(typeof(NettingReceivableTransaction));
					query.AddSubQuery(subQuery, JoinCondition.And);

					receivableTransactionInNewFactory = newFactory.LoadTop1<NettingReceivableTransaction>(query);

					subQuery = new ZDBOnlySubQuery(typeof(NettingPayableTransactionLine), NettingPayableTransactionLineSchema.NPL_NPT_Transaction);
					subQuery.AddToFilter(NettingPayableTransactionLineSchema.PK, calculation.MatchingPivot.NMP_NPL_PayableLine);

					query = new ZDBOnlyQuery(typeof(NettingPayableTransaction));
					query.AddSubQuery(subQuery, JoinCondition.And);

					payableTransactionInNewFactory = newFactory.LoadTop1<NettingPayableTransaction>(query);
				}

				AssertEquals(NettingTransactionApprovalStatus.Setteled, receivableTransactionInNewFactory.ApprovalStatus);
				AssertEquals(NettingTransactionApprovalStatus.Setteled, payableTransactionInNewFactory.ApprovalStatus);
			});
		}

		void AssertJournals(IEnumerable<JournalInfoForTesting> expected, IEnumerable<TransactionHeader> actual)
		{
			var expectedStrings = GetInfosAsString(expected);
			var actualStrings = GetJournalsAsString(actual);
			AssertContainsExactElementsInAnyOrder(expectedStrings, actualStrings);

			foreach (var journal in actual)
			{
				var query = new ZQuery();
				query.AddToFilter(AccTransactionHeaderNettingLinkSchema.AH2_AH, journal.PK);
				query.AddToFilter(AccTransactionHeaderNettingLinkSchema.AH2_NSP_Period, period.PK);

				AssertNotNull("Link created", Factory.LoadTop1<AccTransactionHeaderNettingLink>(query));
			}
		}

		List<string> GetInfosAsString(IEnumerable<JournalInfoForTesting> expected)
		{
			var result = new List<string>();

			foreach (var info in expected)
			{
				result.Add(string.Format("Organisation: {0} Ledger: {1} Currency: {2}, DebitCredit: {3} OS Amount: {4}, Local Amount: {5}, ExchangeRate: {6}{7}",
					info.OH_Code,
					info.Ledger,
					info.SettlementCurrency,
					info.DRCR.ToString(),
					info.SettlementAmount.ToString(3),
					info.NettingSystemAmount.ToString(3),
					info.ExchangeRate.ToString(6),
					System.Environment.NewLine));
			}

			return result;
		}

		List<string> GetJournalsAsString(IEnumerable<TransactionHeader> journals)
		{
			var result = new List<string>();

			foreach (Journal journal in journals)
			{
				result.Add(string.Format("Organisation: {0} Ledger: {1} Currency: {2}, DebitCredit: {3} OS Amount: {4}, Local Amount: {5}, ExchangeRate: {6}{7}",
					journal.Header.OH_Code,
					journal.AH_Ledger,
					journal.TransactionCurrency.RX_Code,
					journal.DebitCreditSign,
					journal.AH_OSExTaxAmount.ToString(3),
					journal.AH_LocalExTaxAmount.ToString(3),
					journal.AH_ExchangeRate.ToString(6),
					System.Environment.NewLine));
			}

			return result;
		}

		class JournalInfoForTesting
		{
			public JournalInfoForTesting(ZString oH_Code, ZString ledger, ZString settlementCurrency, DebitCredit dRCR, ZDecimal settlementAmount, ZDecimal nettingSystemAmount, ZDecimal exchangeRate)
			{
				this.OH_Code = oH_Code;
				this.Ledger = ledger;
				this.DRCR = dRCR;
				this.SettlementCurrency = settlementCurrency;
				this.SettlementAmount = settlementAmount;
				this.NettingSystemAmount = nettingSystemAmount;
				this.ExchangeRate = exchangeRate;
			}

			public ZString OH_Code;
			public ZString Ledger;
			public DebitCredit DRCR;
			public ZString SettlementCurrency;
			public ZDecimal SettlementAmount;
			public ZDecimal NettingSystemAmount;
			public ZDecimal ExchangeRate;
		}

		protected override void SetUp()
		{
			base.SetUp();

			LocalCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			nettingSystem = CreateNettingSystem("code", "description", GlbCompany.CurrentCompany);
			period = CreatePeriod(nettingSystem);

			org1 = TestObjectCreator.CreateOrgHeader("AAAAA", true, true, "AUSYD");
			org2 = TestObjectCreator.CreateOrgHeader("BBBBB", true, true, "AUSYD");
			org3 = TestObjectCreator.CreateOrgHeader("CCCCC", true, true, "AUSYD");

			TestObjectCreator.CreateNewCompany("CM1", org1);
			TestObjectCreator.CreateNewCompany("CM2", org2);
			TestObjectCreator.CreateNewCompany("CM3", org3);

			participant1 = CreateNettingOrganisation(nettingSystem, org1, "FUL");
			participant2 = CreateNettingOrganisation(nettingSystem, org2, "FUL");
			CreateNettingOrganisation(nettingSystem, org3, "FUL");
			CreateExchangeRate(nettingSystem, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, "NET", 100.0m, period.PK); // should be ignored because it's the netting system's local currency
			CreateExchangeRate(nettingSystem, "USD", "NET", 1.1m, period.PK);
			CreateExchangeRate(nettingSystem, "GBP", "NET", 0.66m, period.PK);
			CreateExchangeRate(nettingSystem, "EUR", "NET", 0.94m, period.PK);

			var participantStatementMenuItem = Factory.New<StmMenuItem>();
			participantStatementMenuItem.SU_MenuName = "Participant Statement";
			participantStatementMenuItem.SU_BusinessContext = "ParticipantStmnt";
			participantStatementMenuItem.SU_IsPublished = true;
			participantStatementMenuItem.SU_MenuPath = "";
			participantStatementMenuItem.SU_ContactType = "NPS";

			var nettingClearingStatementMenuItem = Factory.New<StmMenuItem>();
			nettingClearingStatementMenuItem.SU_MenuName = "Clearing Bank Payment Document";
			nettingClearingStatementMenuItem.SU_BusinessContext = "ParticipantStmnt";
			nettingClearingStatementMenuItem.SU_IsPublished = true;
			nettingClearingStatementMenuItem.SU_MenuPath = "";
			nettingClearingStatementMenuItem.SU_ContactType = "NPS";

			var nettingClearingJournalMenuItem = Factory.New<StmMenuItem>();
			nettingClearingJournalMenuItem.SU_MenuName = "Clearing Journals";
			nettingClearingJournalMenuItem.SU_BusinessContext = "ParticipantStmnt";
			nettingClearingJournalMenuItem.SU_IsPublished = true;
			nettingClearingJournalMenuItem.SU_MenuPath = "";
			nettingClearingJournalMenuItem.SU_ContactType = "NCJ";

			Factory.Save();

			notification = new NotificationBuffer();
		}

		NettingSystem CreateNettingSystem(string code, string description, GlbCompany company)
		{
			var nettingSystem = Factory.New<NettingSystem>();
			nettingSystem.NS_Code = "Code";
			nettingSystem.NS_Description = "Description";
			nettingSystem.NS_GC = company.PK;
			nettingSystem.NS_IsActive = true;
			return nettingSystem;
		}

		NettingSystemPeriod CreatePeriod(NettingSystem nettingSystem)
		{
			period = Factory.New<NettingSystemPeriod>();
			period.NSP_Period = "201504";
			period.NSP_EarliestInvoiceDateUtc = ZDateTime.Now.AddDays(-7);
			period.NSP_LatestInvoiceDateUtc = ZDateTime.Now.AddDays(15);
			period.NSP_NettingExecutionDateUtc = ZDateTime.Now.AddDays(17);

			period.NSP_LatestUploadDateUtc = ZDateTime.Now.AddDays(5);
			period.NSP_LatestFXOfferDateUtc = ZDateTime.Now.AddDays(8);
			period.NSP_LatestApprovalDateUtc = ZDateTime.Now.AddDays(10);
			period.NSP_ValueDate = ZDate.Today.AddDays(12);
			period.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(12);

			period.NSP_NS_NettingSystem = nettingSystem.PK;

			CreateNextPeriod(nettingSystem);

			return period;
		}

		void CreateNextPeriod(NettingSystem nettingSystem)
		{
			nextPeriod = Factory.New<NettingSystemPeriod>();
			nextPeriod.NSP_Period = "201505";
			nextPeriod.NSP_EarliestInvoiceDateUtc = ZDateTime.Now.AddDays(23);
			nextPeriod.NSP_LatestInvoiceDateUtc = ZDateTime.Now.AddDays(45);
			nextPeriod.NSP_NettingExecutionDateUtc = ZDateTime.Now.AddDays(37);

			nextPeriod.NSP_LatestUploadDateUtc = ZDateTime.Now.AddDays(35);
			nextPeriod.NSP_LatestFXOfferDateUtc = ZDateTime.Now.AddDays(38);
			nextPeriod.NSP_LatestApprovalDateUtc = ZDateTime.Now.AddDays(40);
			nextPeriod.NSP_ValueDate = ZDate.Today.AddDays(42);
			nextPeriod.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(42);

			nextPeriod.NSP_NS_NettingSystem = nettingSystem.PK;
		}

		NettingFXOffer CreateFXOffer(NettingSystemPeriod period, NettingOrganisation participant, string type, string status, string description, string currency, decimal value)
		{
			var fxOffer = Factory.New<NettingFXOffer>();
			fxOffer.NFO_NS_System = period.NSP_NS_NettingSystem;
			fxOffer.NFO_NSP_Period = period.PK;
			fxOffer.NFO_NSO_Organisation = participant.PK;
			fxOffer.NFO_Type = type;
			fxOffer.NFO_ApprovalStatus = status;
			fxOffer.NFO_DeclinedReason = description;
			fxOffer.NFO_RX_NKCurrency = currency;
			fxOffer.NFO_Value = value;
			return fxOffer;
		}

		NettingSystemExchangeRate CreateExchangeRate(NettingSystem system, ZString currency, ZString rateType, ZDecimal rate, ZGuid period)
		{
			var exRate = Factory.New<NettingSystemExchangeRate>();
			exRate.NER_NS_NettingSystem = system.PK;
			exRate.NER_RX_NKCurrency = currency;
			exRate.NER_RateType = rateType;
			exRate.NER_Rate = rate;
			exRate.NER_NSP_Period = period;
			return exRate;
		}

		NettingOrganisation CreateNettingOrganisation(NettingSystem nettingSystem, OrgHeader organisation, string nettingType)
		{
			var nettingOrg = Factory.New<NettingOrganisation>();
			nettingOrg.NSO_NS_NettingSystem = nettingSystem.PK;
			nettingOrg.NSO_OH_Organisation = organisation.PK;
			nettingOrg.NSO_NettingType = nettingType;
			return nettingOrg;
		}

		void SetupParticipant(NettingOrganisation participant, ZString nettingType, ZString reportingCurrency, ZString arSettlementCurrency, ZString apSettlementCurrency)
		{
			participant.NSO_NettingType = nettingType;
			participant.NSO_RX_NKReportingCurrency = reportingCurrency;
			participant.NSO_RX_NKARSettlementCurrency = arSettlementCurrency;
			participant.NSO_RX_NKAPSettlementCurrency = apSettlementCurrency;
		}

		ZString LocalCurrency;
		NettingSystem nettingSystem;
		NettingSystemPeriod period;
		NettingSystemPeriod nextPeriod;
		OrgHeader org1, org2, org3;
		NettingOrganisation participant1, participant2;
		NotificationBuffer notification;

		NettingObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new NettingObjectCreator(Factory));
		NettingObjectCreator testObjectCreator;
	}
}

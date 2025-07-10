using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Netting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocNettingStatement))]
	sealed class DocNettingStatementTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocNettingStatement[] { DocNettingStatement.New(new ParticipantStatement(Factory, period.PK), Factory) };
		}

		void AssertDocNettingCurrency(DocNettingCurrency currency, ZString transactionCurrency, ZString nettingCurrency, ZDecimal nettingSystemExchangeRate, ZString participantCurrency, ZDecimal participantExchangeRate)
		{
			CombineAssertions(() =>
			{
				AssertEquals(transactionCurrency, currency.TransactionCurrency);
				AssertEquals(nettingCurrency, currency.NettingCurrency);
				AssertEquals(nettingSystemExchangeRate, currency.NettingSystemExchangeRate.Round(6));
				AssertEquals(participantCurrency, currency.ParticipantCurrency);
				AssertEquals(participantExchangeRate, currency.ParticipantExchangeRate.Round(6));
			});
		}

		public void TestParticipantStatements()
		{
			SetupNettingCompaniesAndParticipants();
			SetupNettingExchangeRates();
			var arTransaction1 = (NettingReceivableTransaction)NettingObjectCreator.CreateNettingTransaction("AR", period, participant1, participant2, "USD 1000", CurrencyCodes.UnitedStates, 1000m);
			var apTransaction1 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", period, participant1, participant2, "USD 500", CurrencyCodes.UnitedStates, 500m);
			var apTransaction2 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", period, participant1, participant2, "USD 500", CurrencyCodes.UnitedStates, 500m);
			NettingObjectCreator.CreateNettingMatchingRecord(period, arTransaction1, apTransaction1);
			NettingObjectCreator.CreateNettingMatchingRecord(period, arTransaction1, apTransaction2);

			NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "EUR 875", CurrencyCodes.EuropeanUnion, 875m);
			NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "USD 400", CurrencyCodes.UnitedStates, 400m);
			NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "EUR 350", CurrencyCodes.EuropeanUnion, 350m);

			CreateFXOffer(period, participant3, "OFF", CurrencyCodes.UnitedKingdom, 450M);
			CreateFXOffer(period, participant3, "REQ", CurrencyCodes.EuropeanUnion, 300M);

			Factory.Save();

			NettingObjectCreator.GenerateCalculationRecords(period.PK.ToGuid());

			var statementType = StatementType.Final;

			var participant1DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company1Code, statementType), Factory);
			AssertEquals(2, participant1DocNettingStatement.ReceivableTransactions.Count);
			AssertEquals(2, participant1DocNettingStatement.PayableTransactions.Count);
			AssertEquals(0, participant1DocNettingStatement.FXOffers.Count);
			AssertEquals(0, participant1DocNettingStatement.FXRequests.Count);

			AssertEquals(CurrencyCodes.UnitedStates, participant1DocNettingStatement.NettingCurrency);
			AssertEquals(1583.33M, participant1DocNettingStatement.TotalReceivables);
			AssertEquals(-633.33M, participant1DocNettingStatement.TotalPayables);
			AssertEquals(950M, participant1DocNettingStatement.NetAmount);

			AssertEquals(CurrencyCodes.UnitedKingdom, participant1DocNettingStatement.PayableTransactions[0].ParticipantCurrency);
			AssertEquals(1900.16M, participant1DocNettingStatement.TotalReceivablesInParticipantCurrency);
			AssertEquals(-760.06M, participant1DocNettingStatement.TotalPayablesInParticipantCurrency);
			AssertEquals(1140.10M, participant1DocNettingStatement.NetAmountInParticipantCurrency);

			AssertEquals(2, participant1DocNettingStatement.Currencies.Count);

			AssertEquals(3, participant1DocNettingStatement.ExchangeRates.Count);
			AssertDocNettingCurrency(participant1DocNettingStatement.ExchangeRates[0], CurrencyCodes.EuropeanUnion, CurrencyCodes.UnitedStates, 1.5M, CurrencyCodes.UnitedKingdom, 1.249896M);
			AssertDocNettingCurrency(participant1DocNettingStatement.ExchangeRates[1], CurrencyCodes.UnitedKingdom, CurrencyCodes.UnitedStates, 1.2001M, CurrencyCodes.UnitedKingdom, 1.0M);
			AssertDocNettingCurrency(participant1DocNettingStatement.ExchangeRates[2], CurrencyCodes.UnitedStates, CurrencyCodes.UnitedStates, 1.0M, CurrencyCodes.UnitedKingdom, 0.833264M);

			var participant2DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company2Code, statementType), Factory);
			AssertEquals(2, participant2DocNettingStatement.ReceivableTransactions.Count);
			AssertEquals(2, participant2DocNettingStatement.PayableTransactions.Count);
			AssertEquals(0, participant2DocNettingStatement.FXOffers.Count);
			AssertEquals(0, participant2DocNettingStatement.FXRequests.Count);

			AssertEquals(CurrencyCodes.UnitedStates, participant2DocNettingStatement.NettingCurrency);
			AssertEquals(633.33M, participant2DocNettingStatement.TotalReceivables);
			AssertEquals(-1583.33M, participant2DocNettingStatement.TotalPayables);
			AssertEquals(-950M, participant2DocNettingStatement.NetAmount);

			AssertEquals(CurrencyCodes.UnitedKingdom, participant2DocNettingStatement.PayableTransactions[0].ParticipantCurrency);
			AssertEquals(760.06M, participant2DocNettingStatement.TotalReceivablesInParticipantCurrency);
			AssertEquals(-1900.16M, participant2DocNettingStatement.TotalPayablesInParticipantCurrency);
			AssertEquals(-1140.10M, participant2DocNettingStatement.NetAmountInParticipantCurrency);

			AssertEquals(2, participant2DocNettingStatement.Currencies.Count);

			AssertEquals(3, participant2DocNettingStatement.ExchangeRates.Count);
			AssertDocNettingCurrency(participant2DocNettingStatement.ExchangeRates[0], CurrencyCodes.EuropeanUnion, CurrencyCodes.UnitedStates, 1.5M, CurrencyCodes.UnitedKingdom, 1.249896M);
			AssertDocNettingCurrency(participant2DocNettingStatement.ExchangeRates[1], CurrencyCodes.UnitedKingdom, CurrencyCodes.UnitedStates, 1.2001M, CurrencyCodes.UnitedKingdom, 1.0M);
			AssertDocNettingCurrency(participant2DocNettingStatement.ExchangeRates[2], CurrencyCodes.UnitedStates, CurrencyCodes.UnitedStates, 1.0M, CurrencyCodes.UnitedKingdom, 0.833264M);

			var participant3DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company3Code, statementType), Factory);
			AssertEquals(0, participant3DocNettingStatement.ReceivableTransactions.Count);
			AssertEquals(0, participant3DocNettingStatement.PayableTransactions.Count);
			AssertEquals(1, participant3DocNettingStatement.FXOffers.Count);
			AssertEquals(1, participant3DocNettingStatement.FXRequests.Count);

			AssertEquals(CurrencyCodes.UnitedStates, participant3DocNettingStatement.NettingCurrency);
			AssertEquals(374.97M, participant3DocNettingStatement.TotalFXOffers);
			AssertEquals(-200M, participant3DocNettingStatement.TotalFXRequests);
			AssertEquals(174.97M, participant3DocNettingStatement.NetAmount);

			AssertEquals(CurrencyCodes.UnitedKingdom, participant3DocNettingStatement.FXOffers[0].ParticipantCurrency);
			AssertEquals(450M, participant3DocNettingStatement.TotalFXOffersInParticipantCurrency);
			AssertEquals(-240.02M, participant3DocNettingStatement.TotalFXRequestsInParticipantCurrency);
			AssertEquals(209.98M, participant3DocNettingStatement.NetAmountInParticipantCurrency);

			AssertEquals(2, participant3DocNettingStatement.ExchangeRates.Count);
			AssertDocNettingCurrency(participant3DocNettingStatement.ExchangeRates[0], CurrencyCodes.EuropeanUnion, CurrencyCodes.UnitedStates, 1.5M, CurrencyCodes.UnitedKingdom, 1.249896M);
			AssertDocNettingCurrency(participant3DocNettingStatement.ExchangeRates[1], CurrencyCodes.UnitedKingdom, CurrencyCodes.UnitedStates, 1.2001M, CurrencyCodes.UnitedKingdom, 1.0M);
		}

		public void TestTrialParticipantStatements()
		{
			SetupNettingCompaniesAndParticipants();
			SetupNettingExchangeRates();
			AssertTrialParticipantStatements();
		}

		public void TestTrialParticipantStatementsWithoutOrgProxyNettingTransaction()
		{
			SetupNettingCompaniesAndParticipants();
			SetupNettingExchangeRates();
			var newOrgProxy = NettingObjectCreator.CreateOrgHeader("GGGGG", true, true, "AUSYD");
			var company1 = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, company1Code));
			var existingOrgProxyPK = company1.GC_OH_OrgProxy;
			company1.GC_OH_OrgProxy = newOrgProxy.PK;

			var branch = TestObjectCreator.CreateNewBranch(company1, "BR1");
			branch.GB_OH_OrgProxy = existingOrgProxyPK;

			Factory.Save();

			AssertExceptionThrown<IncorrectDataSetupException>("Netting Participant not found. The Organization Proxy of company 'CC1' is not a Netting Participant. Kindly add the same as the participant and try again.",
				() => AssertTrialParticipantStatements());
		}

		public void TestTrialParticipantStatementsWithoutOrgProxyNettingTransactionAndNettingModeIsOrganizationLevel()
		{
			AccountingConfigurationRegistry.Instance.NettingModeOption.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.NettingModeOption.OrganizationLevel.Code);

			SetupNettingCompaniesAndParticipants();
			SetupNettingExchangeRates();
			var newOrgProxy = NettingObjectCreator.CreateOrgHeader("GGGGG", true, true, "AUSYD");
			var company1 = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, company1Code));
			var existingOrgProxyPK = company1.GC_OH_OrgProxy;
			company1.GC_OH_OrgProxy = newOrgProxy.PK;

			var branch = TestObjectCreator.CreateNewBranch(company1, "BR1");
			branch.GB_OH_OrgProxy = existingOrgProxyPK;

			Factory.Save();

			var arTransaction1 = (NettingReceivableTransaction)NettingObjectCreator.CreateNettingTransaction("AR", period, participant1, participant2, "USD 1000", CurrencyCodes.UnitedStates, 1000m);
			var apTransaction1 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", period, participant1, participant2, "USD 500", CurrencyCodes.UnitedStates, 500m);
			var apTransaction2 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", period, participant1, participant2, "USD 500", CurrencyCodes.UnitedStates, 500m);
			NettingObjectCreator.CreateNettingMatchingRecord(period, arTransaction1, apTransaction1);
			NettingObjectCreator.CreateNettingMatchingRecord(period, arTransaction1, apTransaction2);

			var transaction2 = NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "EUR 875", CurrencyCodes.EuropeanUnion, 875m);
			var transaction3 = NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "USD 400", CurrencyCodes.UnitedStates, 400m);
			var transaction4 = NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "EUR 350", CurrencyCodes.EuropeanUnion, 350m);

			var arTranaction2 = NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant3, "AUD 1000", CurrencyCodes.Australia, 1000m);

			var fxOffer = CreateFXOffer(period, participant3, "OFF", CurrencyCodes.UnitedKingdom, 450M);
			var fxRequest = CreateFXOffer(period, participant3, "REQ", CurrencyCodes.EuropeanUnion, 300M);

			Factory.Save();

			var statementType = StatementType.Trial;

			var participant1DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company1Code, statementType), Factory);

			AssertEquals(3, participant1DocNettingStatement.ReceivableTransactions.Count);
			AssertEquals(2, participant1DocNettingStatement.PayableTransactions.Count);
			AssertEquals(0, participant1DocNettingStatement.FXOffers.Count);
			AssertEquals(0, participant1DocNettingStatement.FXRequests.Count);

			AssertNoExceptionThrown(() => AssertEquals(4, participant1DocNettingStatement.ExchangeRates.Count));
		}

		void AssertTrialParticipantStatements()
		{
			var arTransaction1 = (NettingReceivableTransaction)NettingObjectCreator.CreateNettingTransaction("AR", period, participant1, participant2, "USD 1000", CurrencyCodes.UnitedStates, 1000m);
			var apTransaction1 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", period, participant1, participant2, "USD 500", CurrencyCodes.UnitedStates, 500m);
			var apTransaction2 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", period, participant1, participant2, "USD 500", CurrencyCodes.UnitedStates, 500m);
			NettingObjectCreator.CreateNettingMatchingRecord(period, arTransaction1, apTransaction1);
			NettingObjectCreator.CreateNettingMatchingRecord(period, arTransaction1, apTransaction2);

			var transaction2 = NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant2, "EUR 875", CurrencyCodes.EuropeanUnion, 875m);
			var transaction3 = NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "USD 400", CurrencyCodes.UnitedStates, 400m);
			var transaction4 = NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant2, participant1, "EUR 350", CurrencyCodes.EuropeanUnion, 350m);

			var arTranaction2 = NettingObjectCreator.CreateFullMatchingNettingTransaction(period, participant1, participant3, "AUD 1000", CurrencyCodes.Australia, 1000m);

			CreateFXOffer(period, participant3, "OFF", CurrencyCodes.UnitedKingdom, 450M);
			CreateFXOffer(period, participant3, "REQ", CurrencyCodes.EuropeanUnion, 300M);

			Factory.Save();

			var statementType = StatementType.Trial;

			var participant1DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company1Code, statementType), Factory);
			AssertEquals(3, participant1DocNettingStatement.ReceivableTransactions.Count);
			AssertEquals(2, participant1DocNettingStatement.PayableTransactions.Count);
			AssertEquals(0, participant1DocNettingStatement.FXOffers.Count);
			AssertEquals(0, participant1DocNettingStatement.FXRequests.Count);

			AssertEquals(CurrencyCodes.UnitedStates, participant1DocNettingStatement.NettingCurrency);
			AssertEquals(2916.13M, participant1DocNettingStatement.TotalReceivables);
			AssertEquals(-633.33M, participant1DocNettingStatement.TotalPayables);
			AssertEquals(2282.80M, participant1DocNettingStatement.NetAmount);

			AssertEquals(CurrencyCodes.UnitedKingdom, participant1DocNettingStatement.PayableTransactions[0].ParticipantCurrency);
			AssertEquals(3499.65M, participant1DocNettingStatement.TotalReceivablesInParticipantCurrency);
			AssertEquals(-760.06M, participant1DocNettingStatement.TotalPayablesInParticipantCurrency);
			AssertEquals(2739.59M, participant1DocNettingStatement.NetAmountInParticipantCurrency);

			AssertEquals(3, participant1DocNettingStatement.Currencies.Count);

			AssertEquals(4, participant1DocNettingStatement.ExchangeRates.Count);
			AssertDocNettingCurrency(participant1DocNettingStatement.ExchangeRates[0], CurrencyCodes.Australia, CurrencyCodes.UnitedStates, 0.750300M, CurrencyCodes.UnitedKingdom, 0.625198M);
			AssertDocNettingCurrency(participant1DocNettingStatement.ExchangeRates[1], CurrencyCodes.EuropeanUnion, CurrencyCodes.UnitedStates, 1.5M, CurrencyCodes.UnitedKingdom, 1.249896M);
			AssertDocNettingCurrency(participant1DocNettingStatement.ExchangeRates[2], CurrencyCodes.UnitedKingdom, CurrencyCodes.UnitedStates, 1.200100M, CurrencyCodes.UnitedKingdom, 1.0M);
			AssertDocNettingCurrency(participant1DocNettingStatement.ExchangeRates[3], CurrencyCodes.UnitedStates, CurrencyCodes.UnitedStates, 1.0M, CurrencyCodes.UnitedKingdom, 0.833264M);

			var participant2DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company2Code, statementType), Factory);
			AssertEquals(2, participant2DocNettingStatement.ReceivableTransactions.Count);
			AssertEquals(2, participant2DocNettingStatement.PayableTransactions.Count);
			AssertEquals(0, participant2DocNettingStatement.FXOffers.Count);
			AssertEquals(0, participant2DocNettingStatement.FXRequests.Count);

			AssertEquals(CurrencyCodes.UnitedStates, participant2DocNettingStatement.NettingCurrency);
			AssertEquals(633.33M, participant2DocNettingStatement.TotalReceivables);
			AssertEquals(-1583.33M, participant2DocNettingStatement.TotalPayables);
			AssertEquals(-950M, participant2DocNettingStatement.NetAmount);

			AssertEquals(CurrencyCodes.UnitedKingdom, participant2DocNettingStatement.PayableTransactions[0].ParticipantCurrency);
			AssertEquals(760.06M, participant2DocNettingStatement.TotalReceivablesInParticipantCurrency);
			AssertEquals(-1900.16M, participant2DocNettingStatement.TotalPayablesInParticipantCurrency);
			AssertEquals(-1140.10M, participant2DocNettingStatement.NetAmountInParticipantCurrency);

			AssertEquals(2, participant2DocNettingStatement.Currencies.Count);

			AssertEquals(3, participant2DocNettingStatement.ExchangeRates.Count);
			AssertDocNettingCurrency(participant2DocNettingStatement.ExchangeRates[0], CurrencyCodes.EuropeanUnion, CurrencyCodes.UnitedStates, 1.5M, CurrencyCodes.UnitedKingdom, 1.249896M);
			AssertDocNettingCurrency(participant2DocNettingStatement.ExchangeRates[1], CurrencyCodes.UnitedKingdom, CurrencyCodes.UnitedStates, 1.200100M, CurrencyCodes.UnitedKingdom, 1.0M);
			AssertDocNettingCurrency(participant2DocNettingStatement.ExchangeRates[2], CurrencyCodes.UnitedStates, CurrencyCodes.UnitedStates, 1.0M, CurrencyCodes.UnitedKingdom, 0.833264M);

			var participant3DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company3Code, statementType), Factory);
			AssertEquals(0, participant3DocNettingStatement.ReceivableTransactions.Count);
			AssertEquals(1, participant3DocNettingStatement.PayableTransactions.Count);
			AssertEquals(1, participant3DocNettingStatement.FXOffers.Count);
			AssertEquals(1, participant3DocNettingStatement.FXRequests.Count);

			AssertEquals(CurrencyCodes.UnitedStates, participant3DocNettingStatement.NettingCurrency);
			AssertEquals(374.97M, participant3DocNettingStatement.TotalFXOffers);
			AssertEquals(-200M, participant3DocNettingStatement.TotalFXRequests);
			AssertEquals(-1332.80M, participant3DocNettingStatement.TotalPayables);
			AssertEquals(-1157.83M, participant3DocNettingStatement.NetAmount);

			AssertEquals(CurrencyCodes.UnitedKingdom, participant3DocNettingStatement.FXOffers[0].ParticipantCurrency);
			AssertEquals(450M, participant3DocNettingStatement.TotalFXOffersInParticipantCurrency);
			AssertEquals(-240.02M, participant3DocNettingStatement.TotalFXRequestsInParticipantCurrency);
			AssertEquals(-1599.49M, participant3DocNettingStatement.TotalPayablesInParticipantCurrency);
			AssertEquals(-1389.51M, participant3DocNettingStatement.NetAmountInParticipantCurrency);

			AssertEquals(3, participant3DocNettingStatement.ExchangeRates.Count);
			AssertDocNettingCurrency(participant3DocNettingStatement.ExchangeRates[0], CurrencyCodes.Australia, CurrencyCodes.UnitedStates, 0.750300M, CurrencyCodes.UnitedKingdom, 0.625198M);
			AssertDocNettingCurrency(participant3DocNettingStatement.ExchangeRates[1], CurrencyCodes.EuropeanUnion, CurrencyCodes.UnitedStates, 1.5M, CurrencyCodes.UnitedKingdom, 1.249896M);
			AssertDocNettingCurrency(participant3DocNettingStatement.ExchangeRates[2], CurrencyCodes.UnitedKingdom, CurrencyCodes.UnitedStates, 1.200100M, CurrencyCodes.UnitedKingdom, 1.0M);
		}

		public void TestReceivableTransactions_PayableTransactions()
		{
			SetupNettingCompaniesAndParticipants();
			var arTransaction1 = (NettingReceivableTransaction)NettingObjectCreator.CreateNettingTransaction("AR", period, participant1, participant2, "Credit note USD 1000", CurrencyCodes.UnitedStates, -1000m);
			var apTransaction1 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", period, participant1, participant2, "Credit note USD 500/1", CurrencyCodes.UnitedStates, -500m);
			var apTransaction2 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", period, participant1, participant2, "Credit note USD 500/2", CurrencyCodes.UnitedStates, -500m);

			NettingObjectCreator.CreateNettingMatchingRecord(period, arTransaction1, apTransaction1);
			NettingObjectCreator.CreateNettingMatchingRecord(period, arTransaction1, apTransaction2);

			Factory.Save();

			var participant1DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company1Code, StatementType.Trial), Factory);
			AssertEquals("1 AR transaction from participant1 to participant2", 1, participant1DocNettingStatement.ReceivableTransactions.Count);
			AssertEquals("No AP transaction from participant1 to participant2", 0, participant1DocNettingStatement.PayableTransactions.Count);
			AssertEquals("1 AR was matched against 2 AP", 2, participant1DocNettingStatement.ReceivableMatchedInvoices.Count);

			var participant2DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company2Code, StatementType.Trial), Factory);
			AssertEquals("No AR transaction from participant1 to participant2", 0, participant2DocNettingStatement.ReceivableTransactions.Count);
			AssertEquals("Because it is a receivable based system, 1 AP transaction from participant1 to participant2", 1, participant2DocNettingStatement.PayableTransactions.Count);
			AssertEquals("1 AR was matched against 2 AP", 2, participant2DocNettingStatement.PayableMatchedInvoices.Count);
		}

		public void TestReceivableTransactions_PayableTransactions_ForDifferentCurrencies()
		{
			// Arrange
			SetupNettingCompaniesAndParticipants();
			var foreignCurrency = CurrencyCodes.Kazakhstan;
			var arTransaction = NettingObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "Credit note 10", foreignCurrency, -10m) as NettingReceivableTransaction;
			var apTransaction = NettingObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "Credit note 10", foreignCurrency, -10m) as NettingPayableTransaction;
			NettingObjectCreator.CreateNettingMatchingRecord(period, arTransaction, apTransaction);
			Factory.Save();

			var docNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company1Code, StatementType.Trial), Factory);
			AssertEquals("Pre-condition: 1 AR transaction from participant1 to participant2", 1, docNettingStatement.ReceivableTransactions.Count);
			AssertEquals("Pre-condition: 0 AP transaction from participant1 to participant2", 0, docNettingStatement.PayableTransactions.Count);

			// Act
			var statement1Currencies = docNettingStatement.Currencies;

			// Assert
			AssertEquals("Expect currencies, not a divide by 0 exception", 1, statement1Currencies.Count);
		}

		public void TestSplitMatchedInvoicesMatchedAtLineLevel_FullyMatched()
		{
			SetupNettingCompaniesAndParticipants();
			SetupNettingExchangeRates();
			var arTransaction1 = (NettingReceivableTransaction)NettingObjectCreator.CreateNettingTransaction("AR", period, participant1, participant2, "Credit note USD 1000", CurrencyCodes.UnitedStates, -1000m);
			var arLine1 = NettingObjectCreator.CreateNettingTransactionLine(arTransaction1, "S001001", -400m, CurrencyCodes.UnitedStates);
			var arLine2 = NettingObjectCreator.CreateNettingTransactionLine(arTransaction1, "S001002", -600m, CurrencyCodes.UnitedStates);

			var apTransaction1 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", period, participant1, participant2, "Credit note USD 400", CurrencyCodes.UnitedStates, -500m);
			var apLine1 = NettingObjectCreator.CreateNettingTransactionLine(apTransaction1, "S001001", -400m, CurrencyCodes.UnitedStates);

			var apTransaction2 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", period, participant1, participant2, "Credit note USD 600", CurrencyCodes.UnitedStates, -500m);
			var apLine2 = NettingObjectCreator.CreateNettingTransactionLine(apTransaction2, "S001002", -600m, CurrencyCodes.UnitedStates);

			Factory.Save();

			NettingHelper.NettingMatchTransactions(Db.Connection, period.PK.ToGuid(), participant1EhubID, participant2EhubID, Env.CurrentCompanyPK, "BP");

			var newFactory = new BusinessObjectFactory();
			var arTransaction1InNewFactory = newFactory.Load<NettingReceivableTransaction>(arTransaction1.PK);
			AssertEquals("Precondition: AR Transaction 1 status changed to MAT", NettingTransactionApprovalStatus.Matched, arTransaction1InNewFactory.ApprovalStatus);

			var apTransaction1InNewFactory = newFactory.Load<NettingPayableTransaction>(apTransaction1.PK);
			var apTransaction2InNewFactory = newFactory.Load<NettingPayableTransaction>(apTransaction2.PK);
			AssertEquals("Precondition: AP Transaction 1 status changed to MAT", NettingTransactionApprovalStatus.Matched, apTransaction1InNewFactory.ApprovalStatus);
			AssertEquals("Precondition: AP Transaction 2 status changed to MAT", NettingTransactionApprovalStatus.Matched, apTransaction2InNewFactory.ApprovalStatus);

			var query = new ZQuery(NettingMatchPivotSchema.NMP_NRT_ReceivableTransaction, arTransaction1.PK);
			query.AddToFilter(NettingMatchPivotSchema.NMP_NPT_PayableTransaction, apTransaction1.PK);
			Assert("No matching record exists at header level", !Factory.ExistsInDatabase(NettingMatchPivotSchema.Constants.TableName, query));

			query = new ZQuery(NettingMatchPivotSchema.NMP_NRL_ReceivableLine, arLine1.PK);
			query.AddToFilter(NettingMatchPivotSchema.NMP_NPL_PayableLine, apLine1.PK);
			Assert("Matching record exists at line level", Factory.ExistsInDatabase(NettingMatchPivotSchema.Constants.TableName, query));

			var statementType = StatementType.Trial;
			AssertSplitMatchedRecords("Trial Statment - Before cycle is finalized");

			NettingObjectCreator.GenerateCalculationRecords(period.PK.ToGuid());
			AssertSplitMatchedRecords("Trial Statment - After cycle is finalized");

			statementType = StatementType.Final;
			AssertSplitMatchedRecords("Final Statment - After cycle is finalized");

			void AssertSplitMatchedRecords(ZString message)
			{
				var participant1DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company1Code, statementType), Factory);
				var participant2DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company2Code, statementType), Factory);

				AssertEquals(message + " 1 AR from participant1 was matched against 2 AP from participant2", 2, participant1DocNettingStatement.ReceivableMatchedInvoices.Count);
				AssertEquals(message + " No AP from participant1 was included in the match", 0, participant1DocNettingStatement.PayableMatchedInvoices.Count);

				AssertEquals(message + " No AR from participant2 was included in the match", 0, participant2DocNettingStatement.ReceivableMatchedInvoices.Count);
				AssertEquals(message + " 1 AR from participant1 was matched against 2 AP from participant2", 2, participant2DocNettingStatement.PayableMatchedInvoices.Count);
			}
		}

		public void TestSplitMatchedInvoicesMatchedAtLineLevel_ARPartiallyMatched()
		{
			SetupNettingCompaniesAndParticipants();
			SetupNettingExchangeRates();
			var arTransaction1 = (NettingReceivableTransaction)NettingObjectCreator.CreateNettingTransaction("AR", period, participant1, participant2, "Credit note USD 1000", CurrencyCodes.UnitedStates, -1000m);
			var arLine1 = NettingObjectCreator.CreateNettingTransactionLine(arTransaction1, "S001001", -400m, CurrencyCodes.UnitedStates);
			var arLine2 = NettingObjectCreator.CreateNettingTransactionLine(arTransaction1, "S001002", -600m, CurrencyCodes.UnitedStates);

			var apTransaction1 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", period, participant1, participant2, "Credit note USD 400", CurrencyCodes.UnitedStates, -500m);
			var apLine1 = NettingObjectCreator.CreateNettingTransactionLine(apTransaction1, "S001001", -400m, CurrencyCodes.UnitedStates);

			Factory.Save();

			NettingHelper.NettingMatchTransactions(Db.Connection, period.PK.ToGuid(), participant1EhubID, participant2EhubID, Env.CurrentCompanyPK, "BP");

			var newFactory = new BusinessObjectFactory();
			var arTransaction1InNewFactory = newFactory.Load<NettingReceivableTransaction>(arTransaction1.PK);
			AssertEquals("Precondition: AR Transaction 1 status remains as APP", NettingTransactionApprovalStatus.Approved, arTransaction1InNewFactory.ApprovalStatus);

			var apTransaction1InNewFactory = newFactory.Load<NettingPayableTransaction>(apTransaction1.PK);
			AssertEquals("Precondition: AP Transaction 1 status changed to MAT", NettingTransactionApprovalStatus.Matched, apTransaction1InNewFactory.ApprovalStatus);

			var query = new ZQuery(NettingMatchPivotSchema.NMP_NRT_ReceivableTransaction, arTransaction1.PK);
			query.AddToFilter(NettingMatchPivotSchema.NMP_NPT_PayableTransaction, apTransaction1.PK);
			Assert("No matching record exists at header level", !Factory.ExistsInDatabase(NettingMatchPivotSchema.Constants.TableName, query));

			query = new ZQuery(NettingMatchPivotSchema.NMP_NRL_ReceivableLine, arLine1.PK);
			query.AddToFilter(NettingMatchPivotSchema.NMP_NPL_PayableLine, apLine1.PK);
			Assert("Matching record exists at line level", Factory.ExistsInDatabase(NettingMatchPivotSchema.Constants.TableName, query));

			var statementType = StatementType.Trial;
			AssertSplitMatchedRecords("Trial Statment - Before cycle is finalized");

			NettingObjectCreator.GenerateCalculationRecords(period.PK.ToGuid());
			AssertSplitMatchedRecords("Trial Statment - After cycle is finalized");

			statementType = StatementType.Final;
			AssertSplitMatchedRecords("Final Statment - After cycle is finalized");

			void AssertSplitMatchedRecords(ZString message)
			{
				var participant1DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company1Code, statementType), Factory);
				var participant2DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company2Code, statementType), Factory);

				AssertEquals(message + " 1 AR from participant1 was matched against 1 AP from participant2, AR status is still APP. Hence, no record should be returned in ReceivableMatchedInvoices", 0, participant1DocNettingStatement.ReceivableMatchedInvoices.Count);
				AssertEquals(message + " No AP from participant1 was included in the match", 0, participant1DocNettingStatement.PayableMatchedInvoices.Count);

				AssertEquals(message + " No AR from participant2 was included in the match", 0, participant2DocNettingStatement.ReceivableMatchedInvoices.Count);
				AssertEquals(message + " 1 AR from participant1 was matched against 1 AP from participant2, AR status is still APP. Hence, no record should be returned in PayableMatchedInvoices", 0, participant2DocNettingStatement.PayableMatchedInvoices.Count);
			}
		}

		public void TestSplitMatchedInvoicesMatchedAtLineLevel_APPartiallyMatched()
		{
			SetupNettingCompaniesAndParticipants();
			var arTransaction1 = (NettingReceivableTransaction)NettingObjectCreator.CreateNettingTransaction("AR", period, participant1, participant2, "Credit note USD 1000", CurrencyCodes.UnitedStates, -1000m);
			var arLine1 = NettingObjectCreator.CreateNettingTransactionLine(arTransaction1, "S001001", -400m, CurrencyCodes.UnitedStates);

			var apTransaction1 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", period, participant1, participant2, "Credit note USD 400", CurrencyCodes.UnitedStates, -500m);
			var apLine1 = NettingObjectCreator.CreateNettingTransactionLine(apTransaction1, "S001001", -400m, CurrencyCodes.UnitedStates);
			var apLine2 = NettingObjectCreator.CreateNettingTransactionLine(apTransaction1, "S001002", -600m, CurrencyCodes.UnitedStates);

			Factory.Save();

			NettingHelper.NettingMatchTransactions(Db.Connection, period.PK.ToGuid(), participant1EhubID, participant2EhubID, Env.CurrentCompanyPK, "BP");

			var newFactory = new BusinessObjectFactory();
			var arTransaction1InNewFactory = newFactory.Load<NettingReceivableTransaction>(arTransaction1.PK);
			AssertEquals("Precondition: AR Transaction 1 status changed to MAT", NettingTransactionApprovalStatus.Matched, arTransaction1InNewFactory.ApprovalStatus);

			var apTransaction1InNewFactory = newFactory.Load<NettingPayableTransaction>(apTransaction1.PK);
			AssertEquals("Precondition: AP Transaction 1 status remains as APP", NettingTransactionApprovalStatus.Approved, apTransaction1InNewFactory.ApprovalStatus);

			var query = new ZQuery(NettingMatchPivotSchema.NMP_NRT_ReceivableTransaction, arTransaction1.PK);
			query.AddToFilter(NettingMatchPivotSchema.NMP_NPT_PayableTransaction, apTransaction1.PK);
			Assert("No matching record exists at header level", !Factory.ExistsInDatabase(NettingMatchPivotSchema.Constants.TableName, query));

			query = new ZQuery(NettingMatchPivotSchema.NMP_NRL_ReceivableLine, arLine1.PK);
			query.AddToFilter(NettingMatchPivotSchema.NMP_NPL_PayableLine, apLine1.PK);
			Assert("Matching record exists at line level", Factory.ExistsInDatabase(NettingMatchPivotSchema.Constants.TableName, query));

			var statementType = StatementType.Trial;
			AssertSplitMatchedRecords("Trial Statment - Before cycle is finalized");

			NettingObjectCreator.GenerateCalculationRecords(period.PK.ToGuid());
			AssertSplitMatchedRecords("Trial Statment - After cycle is finalized");

			statementType = StatementType.Final;
			AssertSplitMatchedRecords("Final Statment - After cycle is finalized");

			void AssertSplitMatchedRecords(ZString message)
			{
				var participant1DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company1Code, statementType), Factory);
				var participant2DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company2Code, statementType), Factory);

				AssertEquals(message + " 1 AR from participant1 was matched against 1 AP from participant2, AP status is still APP. Hence, no record should be returned in ReceivableMatchedInvoices", 0, participant1DocNettingStatement.ReceivableMatchedInvoices.Count);
				AssertEquals(message + " No AP from participant1 was included in the match", 0, participant1DocNettingStatement.PayableMatchedInvoices.Count);

				AssertEquals(message + " No AR from participant2 was included in the match", 0, participant2DocNettingStatement.ReceivableMatchedInvoices.Count);
				AssertEquals(message + " 1 AR from participant1 was matched against 1 AP from participant2, AP status is still APP. Hence, no record should be returned in PayableMatchedInvoices", 0, participant2DocNettingStatement.PayableMatchedInvoices.Count);
			}
		}

		public void TestRecipientNameAddress()
		{
			var nettingSystem = Factory.LoadTop1<NettingSystem>(new ZQuery());
			var orgProxy = NettingObjectCreator.CreateOrgHeader("AAAAA", true, true, "AUSYD");
			var mainAddress = orgProxy.Addresses.MainAddress;
			mainAddress.OA_IsActive = false;
			Assert(!mainAddress.OA_IsActive);
			Assert(mainAddress.IsAddressOfType(OrgAddressType.Office));

			var activeOfficeAddress = orgProxy.Addresses.AddNew(OrgAddressType.Office, false);
			activeOfficeAddress.OA_Address1 = "activeOfficeAddress";
			activeOfficeAddress.OA_Address2 = "555";

			var company = NettingObjectCreator.CreateNewCompany("NEW", orgProxy);
			NettingObjectCreator.CreateNettingOrganisation(nettingSystem, orgProxy, "FUL");
			Factory.Save();

			var statement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company.GC_Code, StatementType.Trial), Factory);

			AssertContains("activeOfficeAddress", statement.RecipientNameAddress);
		}

		public void TestNettingType()
		{
			SetupNettingCompaniesAndParticipants();
			var statementType = StatementType.Final;
			var participant1DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company1Code, statementType), Factory);
			AssertEquals("FULL", participant1DocNettingStatement.NettingType);

			var participant2DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company2Code, statementType), Factory);
			AssertEquals("CURRENCY", participant2DocNettingStatement.NettingType);

			var participant3DocNettingStatement = DocNettingStatement.New(new ParticipantStatement(Factory, period.PK, company3Code, statementType), Factory);
			AssertEquals("HOME", participant3DocNettingStatement.NettingType);
		}

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(TestingCountry);

			var nettingSystem = NettingObjectCreator.CreateNettingSystem("code", "description", GlbCompany.CurrentCompany);
			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			period = NettingObjectCreator.CreateNettingPeriod(nettingSystem, "201504", ZDateTime.Now.AddDays(-7), ZDateTime.Now.AddDays(15), ZDateTime.Now.AddDays(17), ZDateTime.Now.AddDays(10), ZDateTime.Now.AddDays(8), ZDateTime.Now.AddDays(5), ZDate.Today.AddDays(12));

			Factory.Save();

			base.SetUp();
		}

		protected override string TestingCountry => CountryCodes.UnitedStates;

		NettingFXOffer CreateFXOffer(NettingSystemPeriod period, NettingOrganisation participant, string type, string currency, decimal value)
		{
			var fxOffer = Factory.New<NettingFXOffer>();
			fxOffer.NFO_NS_System = period.NSP_NS_NettingSystem;
			fxOffer.NFO_NSP_Period = period.PK;
			fxOffer.NFO_NSO_Organisation = participant.PK;
			fxOffer.NFO_Type = type;
			fxOffer.NFO_ApprovalStatus = "APP";
			fxOffer.NFO_DeclinedReason = ZString.Empty;
			fxOffer.NFO_RX_NKCurrency = currency;
			fxOffer.NFO_Value = value;
			return fxOffer;
		}

		void SetupNettingCompaniesAndParticipants()
		{
			var nettingSystem = Factory.LoadTop1<NettingSystem>(new ZQuery());
			var org1 = NettingObjectCreator.CreateOrgHeader("AAAAA", true, true, "AUSYD");
			var org2 = NettingObjectCreator.CreateOrgHeader("BBBBB", true, true, "AUSYD");
			var org3 = NettingObjectCreator.CreateOrgHeader("CCCCC", true, true, "AUSYD");

			NettingObjectCreator.CreateNewCompany(company1Code, org1);
			NettingObjectCreator.CreateNewCompany(company2Code, org2);
			NettingObjectCreator.CreateNewCompany(company3Code, org3);

			participant1 = SetupParticipant(nettingSystem, org1, "FUL", participant1EhubID);
			participant2 = SetupParticipant(nettingSystem, org2, "CUR", participant2EhubID);
			participant3 = SetupParticipant(nettingSystem, org3, "HOM", "123459876");
			Factory.Save();
		}

		void SetupNettingExchangeRates()
		{
			var nettingSystem = Factory.LoadTop1<NettingSystem>(new ZQuery());
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.Australia, "NET", 0.7503m, period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.UnitedStates, "NET", 1m, period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.UnitedKingdom, "NET", 1.2001m, period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.EuropeanUnion, "NET", 1.5m, period);

			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.Australia, "IND", 0.7503m, period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.UnitedStates, "IND", 1m, period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.UnitedKingdom, "IND", 1.2001m, period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, CurrencyCodes.EuropeanUnion, "IND", 1.5m, period);
		}

		NettingOrganisation SetupParticipant(NettingSystem nettingSystem, OrgHeader orgProxy, ZString nettingType, ZString eHubId)
		{
			var participant = NettingObjectCreator.CreateNettingOrganisation(nettingSystem, orgProxy, nettingType);
			participant.NSO_RX_NKReportingCurrency = CurrencyCodes.UnitedKingdom;
			participant.NSO_RX_NKARSettlementCurrency = CurrencyCodes.EuropeanUnion;
			participant.NSO_RX_NKAPSettlementCurrency = CurrencyCodes.UnitedStates;
			participant.Organisation.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, eHubId);
			return participant;
		}

		NettingObjectCreator NettingObjectCreator => nettingObjectCreator ?? (nettingObjectCreator = new NettingObjectCreator(Factory));
		NettingObjectCreator nettingObjectCreator;

		NettingSystemPeriod period;
		NettingOrganisation participant1, participant2, participant3;
		readonly ZString company1Code = "CM1";
		readonly ZString company2Code = "CM2";
		readonly ZString company3Code = "CM3";
		readonly ZString participant1EhubID = "123456789";
		readonly ZString participant2EhubID = "987654321";
	}
}

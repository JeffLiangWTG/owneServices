using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingTransaction))]
	public class NettingTransactionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNettingTransaction()
		{
			AssertNoExceptionThrown(() => new NettingTransaction());

			AssertExceptionThrown<ArgumentNullException>(() => new NettingTransaction(null));

			AssertNoExceptionThrown(() => new NettingTransaction(new ParticipantStatement(Factory, ZGuid.NewZGuid())));
		}

		public void TestTransactionRef()
		{
			AssertNotNull(Statement);
			AssertNotNull(Statement.Transactions);

			var nettingTransaction = new NettingTransaction(Statement);

			foreach (var transaction in Statement.Transactions)
			{
				AssertNotNull(transaction.TransactionRef);
			}
		}

		public void TestTransactionLineRef()
		{
			AssertNotNull(Statement);
			AssertNotNull(Statement.Transactions);

			var nettingTransaction = new NettingTransaction(Statement);

			foreach (var transaction in Statement.Transactions)
			{
				AssertNotNull(transaction.TransactionLineRef);
			}
		}

		protected NettingTransaction NettingTransaction;
		protected NettingStatement Statement;

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry("US");

			testObjectCreator = new NettingObjectCreator(Factory);

			nettingSystem = testObjectCreator.CreateNettingSystem("code", "description", GlbCompany.CurrentCompany);
			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Period = CreatePeriod(nettingSystem, "Current");

			Statement = new NettingCentreStatement(Factory, Period.PK);
			NettingTransaction = new NettingTransaction(Statement);

			org1 = testObjectCreator.CreateOrgHeader("AAAAA", true, true, "AUSYD");
			org2 = testObjectCreator.CreateOrgHeader("BBBBB", true, true, "AUSYD");
			org3 = testObjectCreator.CreateOrgHeader("CCCCC", true, true, "AUSYD");

			participant1 = testObjectCreator.CreateNettingOrganisation(nettingSystem, org1, "HOM"); //BAAFORSYD
			participant2 = testObjectCreator.CreateNettingOrganisation(nettingSystem, org2, "GRS"); //IMRFORSYD
			participant3 = testObjectCreator.CreateNettingOrganisation(nettingSystem, org3, "FUL"); //JASUNILON

			testObjectCreator.CreateNettingExchangeRate(nettingSystem, "AUD", "IND", 0.7503m, Period);
			testObjectCreator.CreateNettingExchangeRate(nettingSystem, "USD", "IND", 1m, Period);
			testObjectCreator.CreateNettingExchangeRate(nettingSystem, "GBP", "IND", 1.2001m, Period);
			testObjectCreator.CreateNettingExchangeRate(nettingSystem, "EUR", "IND", 1.5m, Period);

			Factory.Save();

			SetupParticipant(participant1, "HOM", "GBP", "EUR", "USD");
			SetupParticipant(participant2, "GRS", "GBP", "EUR", "USD");
			SetupParticipant(participant3, "FUL", "GBP", "EUR", "USD");

			var matchPivot1 = testObjectCreator.CreateFullMatchingNettingTransaction(Period, participant1, participant2, "EUR 875", "EUR", 875m);
			var matchPivot2 = testObjectCreator.CreateFullMatchingNettingTransaction(Period, participant2, participant1, "USD 400", "USD", 400m);
			var matchPivot3 = testObjectCreator.CreateFullMatchingNettingTransaction(Period, participant2, participant1, "EUR 350", "EUR", 350m);

			Factory.Save();

			base.SetUp();
		}

		NettingSystemPeriod CreatePeriod(NettingSystem nettingSystem, ZString nsp_period)
		{
			Period = Factory.New<NettingSystemPeriod>();
			Period.NSP_Period = nsp_period;
			Period.NSP_EarliestInvoiceDateUtc = ZDateTime.Now.AddDays(-7);
			Period.NSP_LatestInvoiceDateUtc = ZDateTime.Now.AddDays(15);
			Period.NSP_NettingExecutionDateUtc = ZDateTime.Now.AddDays(17);

			Period.NSP_LatestUploadDateUtc = ZDateTime.Now.AddDays(5);
			Period.NSP_LatestFXOfferDateUtc = ZDateTime.Now.AddDays(8);
			Period.NSP_LatestApprovalDateUtc = ZDateTime.Now.AddDays(10);
			Period.NSP_ValueDate = ZDate.Today.AddDays(12);
			Period.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(12);

			Period.NSP_NS_NettingSystem = nettingSystem.PK;

			return Period;
		}

		void SetupParticipant(NettingOrganisation participant, ZString nettingType, ZString reportingCurrency, ZString arSettlementCurrency, ZString apSettlementCurrency)
		{
			participant.NSO_NettingType = nettingType;
			participant.NSO_RX_NKReportingCurrency = reportingCurrency;
			participant.NSO_RX_NKARSettlementCurrency = arSettlementCurrency;
			participant.NSO_RX_NKAPSettlementCurrency = apSettlementCurrency;
		}

		NettingObjectCreator testObjectCreator;
		NettingSystem nettingSystem;
		protected NettingSystemPeriod Period { get; set; }
		OrgHeader org1, org2, org3;
		NettingOrganisation participant1, participant2, participant3;
	}
}

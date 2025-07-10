using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingStatement))]
	public abstract class NettingStatementTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetNetMovements()
		{
			var nettingCentreStatement = new NettingCentreStatement(Factory, Period.PK);
			var nettingcentreReceivables = nettingCentreStatement.GetReceivableNettingMovements();
			var nettingcentrePayables = nettingCentreStatement.GetPayableNettingMovements();

			var participant3Statement = new ParticipantStatement(Factory, Period.PK, "CM1", StatementType.Trial);
			var participant3Receivables = participant3Statement.GetReceivableNettingMovements();
			var participant3Payables = participant3Statement.GetPayableNettingMovements();

			CombineAssertions(() =>
			{
				foreach (var item in participant3Receivables)
				{
					AssertCollectionContains(item, nettingcentrePayables);
				}

				foreach (var item in participant3Payables)
				{
					AssertCollectionContains(item, nettingcentreReceivables);
				}
			});

			var participant2Statement = new ParticipantStatement(Factory, Period.PK, "CM2", StatementType.Trial);
			var participant2Receivables = participant2Statement.GetReceivableNettingMovements();
			var participant2Payables = participant2Statement.GetPayableNettingMovements();

			CombineAssertions(() =>
			{
				foreach (var item in participant2Receivables)
				{
					AssertCollectionContains(item, nettingcentrePayables);
				}

				foreach (var item in participant2Payables)
				{
					AssertCollectionContains(item, nettingcentreReceivables);
				}
			});

			var participant1Statement = new ParticipantStatement(Factory, Period.PK, "CM3", StatementType.Trial);
			var participant1Receivables = participant1Statement.GetReceivableNettingMovements();
			var participant1Payables = participant1Statement.GetPayableNettingMovements();

			CombineAssertions(() =>
			{
				foreach (var item in participant3Receivables)
				{
					AssertCollectionContains(item, nettingcentrePayables);
				}

				foreach (var item in participant3Payables)
				{
					AssertCollectionContains(item, nettingcentreReceivables);
				}
			});

			AssertSignsForNetMovements();
		}

		protected virtual void AssertSignsForNetMovements()
		{
		}

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany.CurrentCompany.SetCountry("US");

			var nettingSystem = NettingObjectCreator.CreateNettingSystem("code", "description", GlbCompany.CurrentCompany);
			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Period = CreatePeriod(nettingSystem, "Current");

			var org1 = NettingObjectCreator.CreateOrgHeader("AAAAA", true, true, "AUSYD");
			var org2 = NettingObjectCreator.CreateOrgHeader("BBBBB", true, true, "AUSYD");
			var org3 = NettingObjectCreator.CreateOrgHeader("CCCCC", true, true, "AUSYD");

			NettingObjectCreator.CreateNewCompany("CM1", org1);
			NettingObjectCreator.CreateNewCompany("CM2", org2);
			NettingObjectCreator.CreateNewCompany("CM3", org3);

			participant1 = NettingObjectCreator.CreateNettingOrganisation(nettingSystem, org1, "HOM");
			participant2 = NettingObjectCreator.CreateNettingOrganisation(nettingSystem, org2, "GRS");
			participant3 = NettingObjectCreator.CreateNettingOrganisation(nettingSystem, org3, "FUL");

			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, "AUD", NettingExchangeRateType.Execution, 0.7503m, Period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, "USD", NettingExchangeRateType.Execution, 1m, Period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, "GBP", NettingExchangeRateType.Execution, 1.2001m, Period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, "EUR", NettingExchangeRateType.Execution, 1.5m, Period);

			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, "AUD", NettingExchangeRateType.Indicative, 0.7503m, Period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, "USD", NettingExchangeRateType.Indicative, 1m, Period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, "GBP", NettingExchangeRateType.Indicative, 1.2001m, Period);
			NettingObjectCreator.CreateNettingExchangeRate(nettingSystem, "EUR", NettingExchangeRateType.Indicative, 1.5m, Period);

			Factory.Save();

			SetupParticipant(participant1, "HOM", "GBP", "EUR", "USD");
			SetupParticipant(participant2, "GRS", "GBP", "EUR", "USD");
			SetupParticipant(participant3, "FUL", "GBP", "EUR", "USD");

			var arTransaction1 = (NettingReceivableTransaction)NettingObjectCreator.CreateNettingTransaction("AR", Period, participant1, participant2, "USD 1000", "USD", 1000m);
			var apTransaction1 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", Period, participant1, participant2, "USD 500", "USD", 500m);
			var apTransaction2 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", Period, participant1, participant2, "USD 500", "USD", 500m);
			NettingObjectCreator.CreateNettingMatchingRecord(Period, arTransaction1, apTransaction1);
			NettingObjectCreator.CreateNettingMatchingRecord(Period, arTransaction1, apTransaction2);

			NettingObjectCreator.CreateFullMatchingNettingTransaction(Period, participant1, participant2, "EUR 875", "EUR", 875m);
			NettingObjectCreator.CreateFullMatchingNettingTransaction(Period, participant2, participant1, "USD 400", "USD", 400m);
			NettingObjectCreator.CreateFullMatchingNettingTransaction(Period, participant2, participant1, "EUR 350", "EUR", 350m);

			var arTransaction2 = (NettingReceivableTransaction)NettingObjectCreator.CreateNettingTransaction("AR", Period, participant3, participant1, "AUD 10000", "AUD", -10000m);
			var apTransaction3 = (NettingPayableTransaction)NettingObjectCreator.CreateNettingTransaction("AP", Period, participant3, participant1, "AUD 10000", "AUD", -10000m);
			NettingObjectCreator.CreateNettingMatchingRecord(Period, arTransaction2, apTransaction3);

			CreateFXOffer(Period, participant3, "OFF", "APP", "", "GBP", 450M);
			CreateFXOffer(Period, participant3, "REQ", "APP", "", "EUR", 300M);

			Factory.Save();

			NettingObjectCreator.GenerateCalculationRecords(Period.PK.ToGuid());
		}

		NettingSystemPeriod CreatePeriod(NettingSystem nettingSystem, ZString nsp_period)
		{
			var period = Factory.New<NettingSystemPeriod>();
			period.NSP_Period = nsp_period;
			period.NSP_EarliestInvoiceDateUtc = ZDateTime.Now.AddDays(-7);
			period.NSP_LatestInvoiceDateUtc = ZDateTime.Now.AddDays(15);
			period.NSP_NettingExecutionDateUtc = ZDateTime.Now.AddDays(17);

			period.NSP_LatestUploadDateUtc = ZDateTime.Now.AddDays(5);
			period.NSP_LatestFXOfferDateUtc = ZDateTime.Now.AddDays(8);
			period.NSP_LatestApprovalDateUtc = ZDateTime.Now.AddDays(10);
			period.NSP_ValueDate = ZDate.Today.AddDays(12);
			period.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(12);

			period.NSP_NS_NettingSystem = nettingSystem.PK;

			return period;
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

		protected void SetupParticipant(NettingOrganisation participant, ZString nettingType, ZString reportingCurrency, ZString arSettlementCurrency, ZString apSettlementCurrency)
		{
			participant.NSO_NettingType = nettingType;
			participant.NSO_RX_NKReportingCurrency = reportingCurrency;
			participant.NSO_RX_NKARSettlementCurrency = arSettlementCurrency;
			participant.NSO_RX_NKAPSettlementCurrency = apSettlementCurrency;
		}

		protected NettingObjectCreator NettingObjectCreator => nettingObjectCreator ?? (nettingObjectCreator = new NettingObjectCreator(Factory));
		NettingObjectCreator nettingObjectCreator;

		protected NettingSystemPeriod Period { get; set; }

		protected NettingOrganisation participant1, participant2, participant3;
	}
}

using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	internal class ChargeToExRateLinkerTest : TestCaseWithFactory
	{
		public void TestIsService()
		{
			Assert(Linker is IService);
		}

		public void TestAddLinks()
		{
			AssertNotNull(Charge.CostExchangeRate);
			AssertNotNull(Charge.RevenueExchangeRate);
			AssertNotNull(Charge.SellInvoiceExchangeRate);

			var costExRate = Factory.Load<ExchangeRate>(Charge.CostExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Creditor1.PK, costExRate.JF_OH_Org);
			var sellExRate = Factory.Load<ExchangeRate>(Charge.RevenueExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Debtor.PK, sellExRate.JF_OH_Org);
			var sellInvExRate = Factory.Load<ExchangeRate>(Charge.SellInvoiceExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Debtor.PK, sellInvExRate.JF_OH_Org);

			// Explicitely add links for testing purposes
			Linker.AddLinks(Charge);

			AssertHasOneRelatedCharge(costExRate, Charge, "Charge related to Cost Ex Rate");
			AssertHasOneRelatedCharge(sellExRate, Charge, "Charge related to Sell Ex Rate");
			AssertHasOneRelatedCharge(sellInvExRate, Charge, "Charge related to Sell Invoice Ex Rate");

			var genericCreditorUSDExRate = Job.ExchangeRates.AddRate(Creator.USD, 0.75m, ZGuid.Empty, ExchangeRateOrgTypeEnum.Creditor);
			var genericDebtorUSDExRate = Job.ExchangeRates.AddRate(Creator.USD, 0.76m, ZGuid.Empty, ExchangeRateOrgTypeEnum.Debtor);
			var genericDebtorEURExRate = Job.ExchangeRates.AddRate(Creator.EUR, 0.8m, ZGuid.Empty, ExchangeRateOrgTypeEnum.Debtor);

			AssertHasOneRelatedCharge(genericCreditorUSDExRate, Charge, "Charge related to generic Cost Ex Rate");
			AssertHasOneRelatedCharge(genericDebtorUSDExRate, Charge, "Charge related to generic Sell Ex Rate");
			AssertHasOneRelatedCharge(genericDebtorEURExRate, Charge, "Charge related to generic Sell Invoice Ex Rate");

			var genericUSDExRate = Job.ExchangeRates.AddRate(Creator.USD, 0.76m, ZGuid.Empty, ExchangeRateOrgTypeEnum.None);
			var genericEURExRate = Job.ExchangeRates.AddRate(Creator.EUR, 0.81m, ZGuid.Empty, ExchangeRateOrgTypeEnum.None);

			AssertHasOneRelatedCharge(genericUSDExRate, Charge, "Charge related to generic USD Ex Rate");
			AssertHasOneRelatedCharge(genericEURExRate, Charge, "Charge related to generic EUR Ex Rate");
		}

		public void TestBeginInitialisingAndIsInitialising()
		{
			foreach (ExchangeRateKind rateKind in Enum.GetValues(typeof(ExchangeRateKind)))
			{
				Assert("Not initialising yet. Rate kind: " + rateKind.ToString(), !Linker.IsInitialising(Charge, rateKind));
				using (Linker.BeginInitialising(Charge, rateKind))
				{
					Assert("Initialising. Rate kind: " + rateKind.ToString(), Linker.IsInitialising(Charge, rateKind));
					using (Linker.BeginInitialising(Charge, rateKind))
					{
						Assert("Initialising again. Rate kind: " + rateKind.ToString(), Linker.IsInitialising(Charge, rateKind));
					}
					Assert("Still initialising. Rate kind: " + rateKind.ToString(), Linker.IsInitialising(Charge, rateKind));
				}
				Assert("Not initialising anymore. Rate kind: " + rateKind.ToString(), !Linker.IsInitialising(Charge, rateKind));
			}
		}

		public void TestGetRelatedChargesPKs()
		{
			AssertNotNull(Charge.CostExchangeRate);
			AssertNotNull(Charge.RevenueExchangeRate);
			AssertNotNull(Charge.SellInvoiceExchangeRate);

			var costExRate = Factory.Load<ExchangeRate>(Charge.CostExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Creditor1.PK, costExRate.JF_OH_Org);
			var sellExRate = Factory.Load<ExchangeRate>(Charge.RevenueExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Debtor.PK, sellExRate.JF_OH_Org);
			var sellInvExRate = Factory.Load<ExchangeRate>(Charge.SellInvoiceExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Debtor.PK, sellInvExRate.JF_OH_Org);

			var creditorUSDRate = Job.AddCurrency(Creator.USD, ExchangeRateValidLedgerEnum.AP);
			var debtorUSDRate = Job.AddCurrency(Creator.USD, ExchangeRateValidLedgerEnum.AR);
			var genericUSDRate = Job.AddCurrency(Creator.USD, ExchangeRateValidLedgerEnum.None);
			var debtorEURRate = Job.AddCurrency(Creator.EUR, ExchangeRateValidLedgerEnum.AR);
			var genericEURRate = Job.AddCurrency(Creator.EUR, ExchangeRateValidLedgerEnum.None);

			AssertChargeIsNotRelated(costExRate, Charge, "Cost Ex Rate is not related yet");
			AssertChargeIsNotRelated(sellExRate, Charge, "Sell Ex Rate is not related yet");
			AssertChargeIsNotRelated(sellInvExRate, Charge, "Sell Invoice Ex Rate is not related yet");
			AssertChargeIsNotRelated(creditorUSDRate, Charge, "Creditor USD Ex Rate is not related yet");
			AssertChargeIsNotRelated(debtorUSDRate, Charge, "Debtor USD Ex Rate is not related yet");
			AssertChargeIsNotRelated(genericUSDRate, Charge, "Generic USD Ex Rate is not related yet");
			AssertChargeIsNotRelated(debtorEURRate, Charge, "Debtor EUR Ex Rate is not related yet");
			AssertChargeIsNotRelated(genericEURRate, Charge, "Generic EUR Ex Rate is not related yet");

			// Explicitely add links for testing purposes
			Linker.AddLinks(Charge);

			AssertHasOneRelatedCharge(costExRate, Charge, "Cost Ex Rate is related now");
			AssertHasOneRelatedCharge(sellExRate, Charge, "Sell Ex Rate is related now");
			AssertHasOneRelatedCharge(sellInvExRate, Charge, "Sell Invoice Ex Rate is related now");
			AssertHasOneRelatedCharge(creditorUSDRate, Charge, "Creditor USD Ex Rate is related now");
			AssertHasOneRelatedCharge(debtorUSDRate, Charge, "Debtor USD Ex Rate is related now");
			AssertHasOneRelatedCharge(genericUSDRate, Charge, "Generic USD Ex Rate is related now");
			AssertHasOneRelatedCharge(debtorEURRate, Charge, "Debtor EUR Ex Rate is related now");
			AssertHasOneRelatedCharge(genericEURRate, Charge, "Generic EUR Ex Rate is related now");
		}

		public void TestGetRelatedChargesPKs_MultipleCharges()
		{
			AssertNotNull(Charge.CostExchangeRate);
			AssertNotNull(Charge.RevenueExchangeRate);
			AssertNotNull(Charge.SellInvoiceExchangeRate);

			var costExRate = Factory.Load<ExchangeRate>(Charge.CostExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Creditor1.PK, costExRate.JF_OH_Org);
			var sellExRate = Factory.Load<ExchangeRate>(Charge.RevenueExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Debtor.PK, sellExRate.JF_OH_Org);
			var sellInvExRate = Factory.Load<ExchangeRate>(Charge.SellInvoiceExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Debtor.PK, sellInvExRate.JF_OH_Org);

			var charge2 = Job.Charges.AddNew();
			charge2.JR_OH_CostAccount = Charge.JR_OH_CostAccount;
			charge2.JR_RX_NKCostCurrency = Charge.JR_RX_NKCostCurrency;
			charge2.JR_OH_SellAccount = Charge.JR_OH_SellAccount;
			charge2.JR_RX_NKSellCurrency = Charge.JR_RX_NKSellCurrency;
			charge2.JR_RX_NKSellInvoiceCurrency = Charge.JR_RX_NKSellInvoiceCurrency;
			Assert(charge2.BillInInvoiceCurrency);

			// Explicitely add links for testing purposes
			Linker.AddLinks(Charge);
			Linker.AddLinks(charge2);

			AssertHasRelatedCharges(costExRate, "Cost Rate has related changes", Charge, charge2);
			AssertHasRelatedCharges(sellExRate, "Sell Rate has related changes", Charge, charge2);
			AssertHasRelatedCharges(sellInvExRate, "Sell Invoice Rate has related changes", Charge, charge2);

			var charge3 = Job.Charges.AddNew();
			charge3.JR_OH_CostAccount = Charge.JR_OH_CostAccount;
			charge3.JR_RX_NKCostCurrency = Charge.JR_RX_NKCostCurrency;

			var charge4 = Job.Charges.AddNew();
			charge4.JR_OH_SellAccount = Charge.JR_OH_SellAccount;
			charge4.JR_RX_NKSellCurrency = Charge.JR_RX_NKSellCurrency;

			var charge5 = Job.Charges.AddNew();
			charge5.JR_OH_SellAccount = Charge.JR_OH_SellAccount;
			charge5.JR_RX_NKSellInvoiceCurrency = Charge.JR_RX_NKSellInvoiceCurrency;

			Linker.AddLinks(charge3);
			Linker.AddLinks(charge4);
			Linker.AddLinks(charge5);

			AssertHasRelatedCharges(costExRate, "Cost Rate has related changes", Charge, charge2, charge3);
			AssertHasRelatedCharges(sellExRate, "Sell Rate has related changes", Charge, charge2, charge4);
			AssertHasRelatedCharges(sellInvExRate, "Sell Invoice Rate has related changes", Charge, charge2, charge5);
		}

		public void TestGetRelatedChargesPKs_DifferentJob()
		{
			var job2 = Creator.CreateJob("J00002", Creator.LocalClient, 0m, Creator.Agent, 0m);
			var charge2 = job2.Charges.AddNew();
			charge2.JR_OH_CostAccount = Charge.JR_OH_CostAccount;
			charge2.JR_RX_NKCostCurrency = Charge.JR_RX_NKCostCurrency;
			charge2.JR_OH_SellAccount = Charge.JR_OH_SellAccount;
			charge2.JR_RX_NKSellCurrency = Charge.JR_RX_NKSellCurrency;
			charge2.JR_RX_NKSellInvoiceCurrency = Charge.JR_RX_NKSellInvoiceCurrency;

			AssertNotNull(Charge.CostExchangeRate);
			AssertNotNull(Charge.RevenueExchangeRate);
			AssertNotNull(Charge.SellInvoiceExchangeRate);

			AssertNotNull(charge2.CostExchangeRate);
			AssertNotNull(charge2.RevenueExchangeRate);
			AssertNotNull(charge2.SellInvoiceExchangeRate);

			var costExRate = Factory.Load<ExchangeRate>(Charge.CostExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Creditor1.PK, costExRate.JF_OH_Org);
			var sellExRate = Factory.Load<ExchangeRate>(Charge.RevenueExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Debtor.PK, sellExRate.JF_OH_Org);
			var sellInvExRate = Factory.Load<ExchangeRate>(Charge.SellInvoiceExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Debtor.PK, sellInvExRate.JF_OH_Org);

			AssertChargeIsNotRelated(costExRate, Charge, "Cost Ex Rate is not related yet");
			AssertChargeIsNotRelated(sellExRate, Charge, "Sell Ex Rate is not related yet");
			AssertChargeIsNotRelated(sellInvExRate, Charge, "Sell Invoice Ex Rate is not related yet");

			var costExRate2 = Factory.Load<ExchangeRate>(charge2.CostExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Creditor1.PK, costExRate2.JF_OH_Org);
			var sellExRate2 = Factory.Load<ExchangeRate>(charge2.RevenueExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Debtor.PK, sellExRate2.JF_OH_Org);
			var sellInvExRate2 = Factory.Load<ExchangeRate>(charge2.SellInvoiceExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Debtor.PK, sellInvExRate2.JF_OH_Org);

			AssertChargeIsNotRelated(costExRate2, charge2, "Cost Ex Rate is not related yet");
			AssertChargeIsNotRelated(sellExRate2, charge2, "Sell Ex Rate is not related yet");
			AssertChargeIsNotRelated(sellInvExRate2, charge2, "Sell Invoice Ex Rate is not related yet");

			// Explicitely add links for testing purposes
			Linker.AddLinks(Charge);
			Linker.AddLinks(charge2);

			AssertHasOneRelatedCharge(costExRate, Charge, "Cost Ex Rate is related one charge now");
			AssertHasOneRelatedCharge(sellExRate, Charge, "Sell Ex Rate is related one charge now");
			AssertHasOneRelatedCharge(sellInvExRate, Charge, "Sell Invoice Ex Rate is related one charge now");
			AssertChargeIsNotRelated(costExRate, charge2, "Cost Rate has no related change from different job");
			AssertChargeIsNotRelated(sellExRate, charge2, "Sell Rate has no related change from different job");
			AssertChargeIsNotRelated(sellInvExRate, charge2, "Sell Invoice Rate has no related change from different job");

			AssertHasOneRelatedCharge(costExRate2, charge2, "Cost Ex Rate is related one charge now");
			AssertHasOneRelatedCharge(sellExRate2, charge2, "Sell Ex Rate is related one charge now");
			AssertHasOneRelatedCharge(sellInvExRate2, charge2, "Sell Invoice Ex Rate is related one charge now");
			AssertChargeIsNotRelated(costExRate2, Charge, "Cost Rate has no related change from different job");
			AssertChargeIsNotRelated(sellExRate2, Charge, "Sell Rate has no related change from different job");
			AssertChargeIsNotRelated(sellInvExRate2, Charge, "Sell Invoice Rate has no related change from different job");
		}

		public void TestRemoveLinksAndIsInitialised()
		{
			AssertNotNull(Charge.CostExchangeRate);
			AssertNotNull(Charge.RevenueExchangeRate);
			AssertNotNull(Charge.SellInvoiceExchangeRate);

			var costExRate = Factory.Load<ExchangeRate>(Charge.CostExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Creditor1.PK, costExRate.JF_OH_Org);
			var sellExRate = Factory.Load<ExchangeRate>(Charge.RevenueExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Debtor.PK, sellExRate.JF_OH_Org);
			var sellInvExRate = Factory.Load<ExchangeRate>(Charge.SellInvoiceExchangeRate.ExchangeRatePk);
			AssertEquals(Creator.Debtor.PK, sellInvExRate.JF_OH_Org);

			AssertChargeIsNotRelated(costExRate, Charge, "No links from Charge to Cost Ex Rate yet");
			AssertChargeIsNotRelated(sellExRate, Charge, "No links from Charge to Sell Ex Rate yet");
			AssertChargeIsNotRelated(sellInvExRate, Charge, "No links from Charge to Sell Invoice Ex Rate yet");

			foreach (ExchangeRateKind rateKind in Enum.GetValues(typeof(ExchangeRateKind)))
			{
				Assert("No links means initialised. Rate kind: " + rateKind.ToString(), Linker.IsInitialised(Charge, rateKind));
			}

			// Explicitely add links for testing purposes
			Linker.AddLinks(Charge);

			AssertHasOneRelatedCharge(costExRate, Charge, "Charge is related to Cost Ex Rate");
			AssertHasOneRelatedCharge(sellExRate, Charge, "Charge is related to Sell Ex Rate");
			AssertHasOneRelatedCharge(sellInvExRate, Charge, "Charge is related to Sell Invoice Ex Rate");

			foreach (ExchangeRateKind rateKind in Enum.GetValues(typeof(ExchangeRateKind)))
			{
				Assert("Not initialised yet. Rate kind: " + rateKind.ToString(), !Linker.IsInitialised(Charge, rateKind));
			}

			Linker.RemoveLinks(Charge, ExchangeRateKind.CostRate);

			AssertChargeIsNotRelated(costExRate, Charge, "Charge is not related to Cost Ex Rate anymore");
			AssertHasOneRelatedCharge(sellExRate, Charge, "Charge related to Sell Ex Rate");
			AssertHasOneRelatedCharge(sellInvExRate, Charge, "Charge related to Sell Invoice Ex Rate");

			Assert("Initialised. Cost Ex Rate", Linker.IsInitialised(Charge, ExchangeRateKind.CostRate));
			Assert("Not initialised yet. Sell Ex Rate", !Linker.IsInitialised(Charge, ExchangeRateKind.SellRate));
			Assert("Not initialised yet. Sell Invoice Ex Rate", !Linker.IsInitialised(Charge, ExchangeRateKind.SellInvoiceRate));

			Linker.RemoveLinks(Charge, ExchangeRateKind.SellRate);

			AssertChargeIsNotRelated(costExRate, Charge, "Charge is not related to Cost Ex Rate anymore");
			AssertChargeIsNotRelated(sellExRate, Charge, "Charge is not related to Sell Ex Rate anymore");
			AssertHasOneRelatedCharge(sellInvExRate, Charge, "Charge related to Sell Invoice Ex Rate");

			Assert("Initialised. Cost Ex Rate", Linker.IsInitialised(Charge, ExchangeRateKind.CostRate));
			Assert("Initialised. Sell Ex Rate", Linker.IsInitialised(Charge, ExchangeRateKind.SellRate));
			Assert("Not initialised yet. Sell Invoice Ex Rate", !Linker.IsInitialised(Charge, ExchangeRateKind.SellInvoiceRate));

			Linker.RemoveLinks(Charge, ExchangeRateKind.SellInvoiceRate);

			AssertChargeIsNotRelated(costExRate, Charge, "Charge is not related to Cost Ex Rate anymore");
			AssertChargeIsNotRelated(sellExRate, Charge, "Charge is not related to Sell Ex Rate anymore");
			AssertChargeIsNotRelated(sellInvExRate, Charge, "Charge is not related to Sell Invoice Ex Rate anymore");

			Assert("Initialised. Cost Ex Rate", Linker.IsInitialised(Charge, ExchangeRateKind.CostRate));
			Assert("Initialised. Sell Ex Rate", Linker.IsInitialised(Charge, ExchangeRateKind.SellRate));
			Assert("Initialised. Sell Invoice Ex Rate", Linker.IsInitialised(Charge, ExchangeRateKind.SellInvoiceRate));
		}

		#region Implementation

		void AssertHasOneRelatedCharge(ExchangeRate exRate, BaseCharge charge, string msg = "Charge is related to Ex Rate")
		{
			var relatedChargePKs = Linker.GetRelatedChargesPKs(exRate);
			AssertEquals(1, relatedChargePKs.Length);
			AssertEquals(msg, charge.PK, relatedChargePKs[0]);
		}

		void AssertHasRelatedCharges(ExchangeRate exRate, string msg, params BaseCharge[] charges)
		{
			var relatedChargePKs = Linker.GetRelatedChargesPKs(exRate);
			AssertEquals(charges.Length, relatedChargePKs.Length);
			int i = 0;
			foreach (var charge in charges)
			{
				var chargePK = relatedChargePKs.FirstOrDefault(x => x == charge.PK);
				Assert(msg + " - Charge" + i.ToString(), !chargePK.IsEmpty);
				i++;
			}
		}

		void AssertChargeIsNotRelated(ExchangeRate exRate, BaseCharge charge, string msg = "Charge is not related to Ex Rate")
		{
			var relatedChargePKs = Linker.GetRelatedChargesPKs(exRate);
			var chargePK = relatedChargePKs.FirstOrDefault(x => x == charge.PK);
			Assert(msg, chargePK.IsEmpty);
		}

		TestObjectCreator Creator;
		ChargeToExRateLinker Linker;
		Job Job;
		Charge Charge;

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);
			Linker = new ChargeToExRateLinker();
			Job = Creator.CreateJob("JOB001", Creator.LocalClient, 0m, Creator.Agent, 0m);
			Charge = Job.Charges.AddNew();
			Charge.JR_OH_CostAccount = Creator.Creditor1.PK;
			Charge.JR_RX_NKCostCurrency = Creator.USD.Code;
			Charge.JR_OH_SellAccount = Creator.Debtor.PK;
			Charge.JR_RX_NKSellCurrency = Creator.USD.Code;
			Charge.JR_RX_NKSellInvoiceCurrency = Creator.EUR.Code;
			Assert(Charge.BillInInvoiceCurrency);
		}

		#endregion
	}
}
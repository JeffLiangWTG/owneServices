using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ChargeReloaderTest : TestCaseWithFactory
	{
		//protected override Type GetExpectedBusinessObjectType()
		//{
		//    return typeof(Charge);
		//}

		public void TestSkipDataRefreshBusUpdateBusinessContextsAreRemovedFromReloadedCharges()
		{
			var charge1 = GetNewChargeWithValidTestData();
			var charge2 = GetNewChargeWithValidTestData();
			var charge3 = GetNewChargeWithValidTestData();
			var cost1 = GetNewCostWithValidTestData();
			charge1.JR_E6 = cost1.PK;
			charge2.JR_E6 = cost1.PK;
			charge3.JR_E6 = cost1.PK;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var cost1Copy = factory2.Load<JobConsolCost>(cost1.PK);
			var charge1Copy = factory2.Load<Charge>(charge1.PK);
			var charge2Copy = factory2.Load<Charge>(charge2.PK);
			var charge3Copy = factory2.Load<Charge>(charge3.PK);

			charge1Copy.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange);
			charge3Copy.SetContext(BusinessContext.SkipDataRefreshBusUpdateDueToSubscriberIsDeleted);

			Assert("Pre condition: Charge1 has SkipDataRefreshBusUpdateDueToAnyChange context.", charge1Copy.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
			Assert("Pre condition: Charge3 has SkipDataRefreshBusUpdateDueToSubscriberIsDeleted context.", charge3Copy.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToSubscriberIsDeleted));

			ChargeReloader.ReloadChargesInFactoryPreservingDisplayOrder(factory2);

			Assert("Post condition: Charge1 does not have SkipDataRefreshBusUpdateDueToAnyChange context.", !charge1Copy.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToAnyChange));
			Assert("Post condition: Charge3 does not have SkipDataRefreshBusUpdateDueToSubscriberIsDeleted context.", !charge3Copy.HasContext(BusinessContext.SkipDataRefreshBusUpdateDueToSubscriberIsDeleted));
		}

		public void TestSettingHasChangesIsSuspendedWhenSettingDisplaySequence()
		{
			var charge1 = GetNewChargeWithValidTestData();
			var charge2 = GetNewChargeWithValidTestData();
			charge1.JR_DisplaySequence = 1;
			charge2.JR_DisplaySequence = 2;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var charge1Copy = factory2.Load<Charge>(charge1.PK);
			var charge2Copy = factory2.Load<Charge>(charge2.PK);

			charge1Copy.JR_DisplaySequence = 2;
			charge2Copy.JR_DisplaySequence = 1;
			charge1Copy.HasChanges = false;
			charge2Copy.HasChanges = false;

			Assert("Pre condition: Charge1 has no changes.", !charge1Copy.HasChanges);
			Assert("Pre condition: Charge2 has no changes.", !charge2Copy.HasChanges);

			ChargeReloader.ReloadChargesInFactoryPreservingDisplayOrder(factory2);

			AssertEquals((ZShort)2, charge1Copy.JR_DisplaySequence);
			AssertEquals((ZShort)1, charge2Copy.JR_DisplaySequence);
			Assert("Post condition: Charge1 has no changes.", !charge1Copy.HasChanges);
			Assert("Post condition: Charge2 has no changes.", !charge2Copy.HasChanges);
		}

		public void TestChargeReloader1Charge()
		{
			AssertChargeReloaderDbHits(1, 1);
		}

		public void TestChargeReloader5Charges()
		{
			AssertChargeReloaderDbHits(5, 1);
		}

		public void TestChargeReloader6Charges()
		{
			AssertChargeReloaderDbHits(6, 2);
		}

		public void TestChargeReloader10Charges()
		{
			AssertChargeReloaderDbHits(10, 2);
		}

		public void TestChargeReloader11Charges()
		{
			AssertChargeReloaderDbHits(11, 3);
		}

		void AssertChargeReloaderDbHits(int numberOfCharges, int expectedDbHits)
		{
			ChargeWithCost[] originalCharges = new ChargeWithCost[numberOfCharges];
			AssertNull("Pre-condition: No Charge Reloader on the Factory", Factory.ServiceContainer.GetService<ChargeReloader>());
			originalCharges[0] = GetNewChargeWithValidTestData();

			for (int i = 1; i < originalCharges.Length; i++)
			{
				originalCharges[i] = GetNewChargeWithValidTestData();
				originalCharges[i].FillWithValidTestData();
			}
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory
			{
				RefreshEnabled = false
			};

			ChargeWithCost[] copyCharges = new ChargeWithCost[originalCharges.Length];
			for (int i = 0; i < originalCharges.Length; i++)
			{
				copyCharges[i] = newFactory.Load<Charge>(originalCharges[i].PK);
				AssertNotNull(copyCharges[i]);
				AssertEquals("Job Charge Description", originalCharges[i].JR_Desc, copyCharges[i].JR_Desc);

				originalCharges[i].JR_Desc = "Test Charge";
			}
			Factory.Save();

			for (int i = 0; i < originalCharges.Length; i++)
			{
				AssertNotEquals("Job Charge Description", originalCharges[i].JR_Desc, copyCharges[i].JR_Desc);
			}

			int hitsBefore = newFactory.GetTableHitCount(JobChargeSchema.Constants.TableName);
			AssertNull("No Charge Reloader on the new Factory", newFactory.ServiceContainer.GetService<ChargeReloader>());
			new ChargeReloader(newFactory).Reload();
			int hitsAfter = newFactory.GetTableHitCount(JobChargeSchema.Constants.TableName);
			AssertEquals(string.Format("Expected {0} hit(s) with test batch size 5", expectedDbHits), expectedDbHits, hitsAfter - hitsBefore);
			AssertNull("There is no Charge Reloader on the new Factory even we called its constructor", newFactory.ServiceContainer.GetService<ChargeReloader>());

			for (int i = 0; i < copyCharges.Length; i++)
			{
				AssertEquals("Job Charge Description", originalCharges[i].JR_Desc, copyCharges[i].JR_Desc);
				Assert("Charge is In Database and does not have changes - it is a reloading candidate on next Factory Save", copyCharges[i].IsInDatabase && !copyCharges[i].HasChanges);
			}

			newFactory.Save();
			AssertNull("No Charge Reloader on the new Factory after saving. It should be added on Factory Saving and removed On Factory Saved by ChargeWithCost", newFactory.ServiceContainer.GetService<ChargeReloader>());
		}

		public void TestChargeReloaderLoadsJobExRates()
		{
			var charge = GetNewChargeWithValidTestData();
			AssertEquals("No Exchange Rates on Job", 0, charge.InvoicingJob.ExchangeRates.Count);
			AssertNull("Pre-condition: No Charge Reloader on the Factory", Factory.ServiceContainer.GetService<ChargeReloader>());

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var copyCharge = newFactory.Load<Charge>(charge.PK);
			AssertEquals("No Exchange Rates on Job in other session", 0, copyCharge.InvoicingJob.ExchangeRates.Count);

			charge.JR_RX_NKSellCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals("New Exchange Rate on Job", 1, charge.InvoicingJob.ExchangeRates.Count);

			Factory.Save();

			AssertNotEquals("Should not be refreshed", Core.Constants.CurrencyCodes.UnitedStates, copyCharge.JR_RX_NKSellCurrency);
			copyCharge.InvoicingJob.JH_ARInvoiceReference = "000";

			new ChargeReloader(newFactory).Reload(); // Have to call it explicitly as RefreshEnabled = false blocks its call
			newFactory.Save();

			AssertEquals("Should be refreshed", Core.Constants.CurrencyCodes.UnitedStates, copyCharge.JR_RX_NKSellCurrency);
			AssertEquals("New Exchange Rate should be on Job in other session", 1, copyCharge.InvoicingJob.ExchangeRates.Count);
			AssertEquals("The same Ex Rate", charge.InvoicingJob.ExchangeRates[0].PK, copyCharge.InvoicingJob.ExchangeRates[0].PK);

			charge.JR_OH_SellAccount = Factory.NewWithValidTestData<OrgHeader>().PK;
			charge.JR_RX_NKSellCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals("New Exchange Rate on Job", 1, charge.InvoicingJob.ExchangeRates.Count);
			var usdDebExRate = charge.InvoicingJob.ExchangeRates[0];
			AssertEquals(charge.JR_OH_SellAccount, usdDebExRate.JF_OH_Org);
			Assert("New Ex Rate", !usdDebExRate.IsInDatabase);

			charge.JR_RX_NKCostCurrency = Enterprise.Core.Constants.CurrencyCodes.EuropeanUnion;
			AssertEquals("Second Exchange Rate on Job", 2, charge.InvoicingJob.ExchangeRates.Count);
			var eurCrdExRate = charge.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(x => x != usdDebExRate);
			AssertNotNull("EUR CRD Ex Rate", eurCrdExRate);
			AssertEquals(ExchangeRateOrgTypeEnum.Creditor.ToCode(), eurCrdExRate.JF_OrgType);
			Assert("New Ex Rate", !eurCrdExRate.IsInDatabase);

			Factory.Save();

			AssertEquals("Should not be refreshed", 1, copyCharge.InvoicingJob.ExchangeRates.Count);
			AssertNotEquals("Should not be refreshed", charge.InvoicingJob.ExchangeRates[0].JF_OH_Org, copyCharge.InvoicingJob.ExchangeRates[0].JF_OH_Org);
			copyCharge.InvoicingJob.JH_ARInvoiceReference = "001";

			new ChargeReloader(newFactory).Reload();
			newFactory.Save();

			AssertEquals("Updated Exchange Rates should be on Job in other session", 2, copyCharge.InvoicingJob.ExchangeRates.Count);
			var copyUsdDebExRate = copyCharge.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(x => x.PK == usdDebExRate.PK);
			AssertNotNull("The same USD DEB Ex Rate", copyUsdDebExRate);
			AssertEquals("Should be refreshed", usdDebExRate.JF_OH_Org, copyUsdDebExRate.JF_OH_Org);
			var copyEurCrdExRate = copyCharge.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(x => x.PK == eurCrdExRate.PK);
			AssertNotNull("The same EUR CRD Ex Rate", copyEurCrdExRate);

			charge.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals("USD DEB Exchange Rate should be gone", 1, charge.InvoicingJob.ExchangeRates.Count);
			AssertEquals("No USD DEB Exchange Rate on Job", eurCrdExRate.PK, charge.InvoicingJob.ExchangeRates[0].PK);

			Factory.Save();

			AssertEquals("Should still have both Exchange Rates on Job in other session", 2, copyCharge.InvoicingJob.ExchangeRates.Count);
			copyEurCrdExRate.JF_IsTransformed = true;
			Assert(copyEurCrdExRate.HasChanges);
			var newUsdCharge = copyCharge.InvoicingJob.Charges.AddNew();
			newUsdCharge.JR_AC = charge.JR_AC;
			newUsdCharge.JR_OH_SellAccount = charge.JR_OH_SellAccount;
			newUsdCharge.JR_RX_NKSellCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals("No extra Exchange Rate added", 2, copyCharge.InvoicingJob.ExchangeRates.Count);
			AssertEquals("New Charge is linked to existing USD DEB Ex Rate", copyUsdDebExRate.PK, newUsdCharge.RevenueExchangeRate.ExchangeRatePk);
			copyCharge.InvoicingJob.JH_ARInvoiceReference = "002";

			new ChargeReloader(newFactory).Reload();
			newFactory.Save();

			AssertEquals("Still have 2 Ex Rates in other session", 2, copyCharge.InvoicingJob.ExchangeRates.Count);
			copyEurCrdExRate = copyCharge.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(x => x.PK == copyEurCrdExRate.PK);
			AssertNotNull("EUR CRD Exchange Rate should stay on Job", copyEurCrdExRate);
			AssertEquals("Modified Base Rate is still there", true, copyEurCrdExRate.JF_IsTransformed);
			var newUsdCrdExRate = copyCharge.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(x => x.PK != copyEurCrdExRate.PK);
			AssertNotEquals(copyUsdDebExRate.PK, newUsdCrdExRate);
			AssertEquals("New USD DEB Ex Rate is linked to new Charge", newUsdCrdExRate.PK, newUsdCharge.RevenueExchangeRate.ExchangeRatePk);
		}

		public void TestChargeReloaderUpdatesExRatesOnModifiedCharges()
		{
			var charge1 = GetNewChargeWithValidTestData();
			AssertEquals("No Exchange Rates on Job", 0, charge1.InvoicingJob.ExchangeRates.Count);
			AssertNull("Pre-condition: No Charge Reloader on the Factory", Factory.ServiceContainer.GetService<ChargeReloader>());

			Factory.Save();

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var copyCharge1 = newFactory.Load<Charge>(charge1.PK);
			AssertEquals("No Exchange Rates on Job in other session", 0, copyCharge1.InvoicingJob.ExchangeRates.Count);

			charge1.JR_OH_SellAccount = Factory.NewWithValidTestData<OrgHeader>().PK;
			charge1.JR_RX_NKSellCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals("New Exchange Rates on Job", 1, charge1.InvoicingJob.ExchangeRates.Count);
			charge1.InvoicingJob.ExchangeRates[0].JF_BaseRate = 2m;

			AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			charge1.JR_OSSellAmt = 100m;

			Factory.Save();

			Assert("Charge1 JR_OH_SellAccount should not be refreshed", copyCharge1.JR_OH_SellAccount.IsEmpty);
			AssertNotEquals("Charge1 JR_RX_NKSellCurrency should not be refreshed", Core.Constants.CurrencyCodes.UnitedStates, copyCharge1.JR_RX_NKSellCurrency);

			copyCharge1.InvoicingJob.JH_ARInvoiceReference = "000";

			new ChargeReloader(newFactory).Reload(); // Have to call it explicitly as RefreshEnabled = false blocks its call
			newFactory.Save();

			AssertEquals("Charge1 JR_OH_SellAccount should be refreshed", charge1.JR_OH_SellAccount, copyCharge1.JR_OH_SellAccount);
			AssertEquals("Charge1 JR_RX_NKSellCurrency should be refreshed", Core.Constants.CurrencyCodes.UnitedStates, copyCharge1.JR_RX_NKSellCurrency);
			AssertEquals("New Exchange Rate should be on Job in other session", 1, copyCharge1.InvoicingJob.ExchangeRates.Count);
			AssertEquals("The same Ex Rate", charge1.InvoicingJob.ExchangeRates[0].PK, copyCharge1.InvoicingJob.ExchangeRates[0].PK);

			AssertEquals("Charge1 JR_LocalSellAmt should be refreshed", 50m, copyCharge1.JR_LocalSellAmt);

			charge1.InvoicingJob.ExchangeRates[0].JF_BaseRate = 1m;

			Factory.Save();

			AssertEquals("Ex Rate is not updated yet", 2m, copyCharge1.InvoicingJob.ExchangeRates[0].JF_BaseRate);
			AssertEquals("Charge1 JR_LocalSellAmt should be refreshed", 50m, copyCharge1.JR_LocalSellAmt);

			var newCharge2 = copyCharge1.InvoicingJob.Charges.AddNew();
			newCharge2.JR_AC = copyCharge1.JR_AC;
			newCharge2.JR_OH_SellAccount = charge1.JR_OH_SellAccount;
			newCharge2.JR_RX_NKSellCurrency = charge1.JR_RX_NKSellCurrency;
			newCharge2.JR_OSSellAmt = 200m;

			AssertEquals("One Exchange Rate should be on Job in other session", 1, copyCharge1.InvoicingJob.ExchangeRates.Count);
			AssertEquals("Charge2 JR_LocalSellAmt", 100m, newCharge2.JR_LocalSellAmt);

			Assert(!copyCharge1.HasChanges);

			new ChargeReloader(newFactory).Reload(); // Have to call it explicitly as RefreshEnabled = false blocks its call
			newFactory.Save();

			AssertEquals("One Exchange Rate should be on Job in other session", 1, copyCharge1.InvoicingJob.ExchangeRates.Count);
			AssertEquals("The same Ex Rate", charge1.InvoicingJob.ExchangeRates[0].PK, copyCharge1.InvoicingJob.ExchangeRates[0].PK);
			AssertEquals("Ex Rate should be updated", 1m, copyCharge1.InvoicingJob.ExchangeRates[0].JF_BaseRate);

			AssertEquals("Charge1 JR_OSSellAmt should not be changed", 100m, copyCharge1.JR_OSSellAmt);
			AssertEquals("Charge2 JR_OSSellAmt should not be changed", 200m, newCharge2.JR_OSSellAmt);
			AssertEquals("Charge1 JR_LocalSellAmt should be refreshed", 100m, copyCharge1.JR_LocalSellAmt);
			AssertEquals("Charge2 JR_LocalSellAmt should be updated", 200m, newCharge2.JR_LocalSellAmt);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestChargeReloaderReloadsChargeAndExRatesOnlyCurrentCompanyCharges()
		{
			var charge1 = GetNewChargeWithValidTestData();
			charge1.JR_OH_SellAccount = Factory.NewWithValidTestData<OrgHeader>().PK;
			charge1.JR_RX_NKSellCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			AssertEquals("New Exchange Rates on Job", 1, charge1.InvoicingJob.ExchangeRates.Count);
			var sellExRate1 = charge1.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().First();
			sellExRate1.JF_BaseRate = 2m;
			charge1.JR_OSSellAmt = 1200m;
			charge1.JR_OH_CostAccount = Factory.NewWithValidTestData<OrgHeader>().PK;
			charge1.JR_RX_NKCostCurrency = Enterprise.Core.Constants.CurrencyCodes.EuropeanUnion;
			AssertEquals("New Exchange Rates on Job", 2, charge1.InvoicingJob.ExchangeRates.Count);
			var costExRate1 = charge1.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().First(x => x.PK != sellExRate1.PK);
			costExRate1.JF_BaseRate = 3m;
			charge1.JR_OSCostAmt = 1300m;

			var creator = new TestObjectCreator(Factory);
			Charge charge2;
			ExchangeRate sellExRate2, costExRate2;
			using (Environment.Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), creator.NonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				charge2 = GetNewChargeWithValidTestData();
				charge2.JR_OH_SellAccount = Factory.NewWithValidTestData<OrgHeader>().PK;
				charge2.JR_RX_NKSellCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
				AssertEquals("New Exchange Rates on Job", 1, charge2.InvoicingJob.ExchangeRates.Count);
				sellExRate2 = charge2.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().First();
				sellExRate2.JF_BaseRate = 0.2m;
				charge2.JR_OSSellAmt = 1000m;
				charge2.JR_OH_CostAccount = Factory.NewWithValidTestData<OrgHeader>().PK;
				charge2.JR_RX_NKCostCurrency = Enterprise.Core.Constants.CurrencyCodes.EuropeanUnion;
				AssertEquals("New Exchange Rates on Job", 2, charge2.InvoicingJob.ExchangeRates.Count);
				costExRate2 = charge2.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().First(x => x.PK != sellExRate2.PK);
				costExRate2.JF_BaseRate = 0.3m;
				charge2.JR_OSCostAmt = 2000m;
			}

			AssertNull("Pre-condition: No Charge Reloader on the Factory", Factory.ServiceContainer.GetService<ChargeReloader>());
			Factory.Save();

			var localSellAmt1 = charge1.JR_LocalSellAmt;
			var localCostAmt1 = charge1.JR_LocalCostAmt;

			var localSellAmt2 = charge2.JR_LocalSellAmt;
			var localCostAmt2 = charge2.JR_LocalCostAmt;

			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };

			var copyCharge1 = newFactory.Load<Charge>(charge1.PK);
			var copyCharge2 = newFactory.Load<Charge>(charge2.PK);

			copyCharge1.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().First(x => x.PK == sellExRate1.PK).JF_BaseRate = 2.5m;
			copyCharge1.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().First(x => x.PK == costExRate1.PK).JF_BaseRate = 3.5m;

			using (Environment.Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), creator.NonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				copyCharge2.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().First(x => x.PK == sellExRate2.PK).JF_BaseRate = 0.25m;
				copyCharge2.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().First(x => x.PK == costExRate2.PK).JF_BaseRate = 0.35m;
			}

			newFactory.Save();

			AssertEquals("Charge1 local Sell Amt not reloaded", localSellAmt1, charge1.JR_LocalSellAmt);
			AssertEquals("Charge1 local Cost Amt not reloaded", localCostAmt1, charge1.JR_LocalCostAmt);
			AssertEquals("Charge2 local Sell Amt not reloaded", localSellAmt2, charge2.JR_LocalSellAmt);
			AssertEquals("Charge2 local Cost Amt not reloaded", localCostAmt2, charge2.JR_LocalCostAmt);

			AssertNull("Pre-condition: No Charge Reloader on the Factory", Factory.ServiceContainer.GetService<ChargeReloader>());
			var reloader = new ChargeReloader(Factory);
			reloader.Reload();

			AssertNotEquals("Charge1 local Sell Amt was reloaded", localSellAmt1, charge1.JR_LocalSellAmt);
			AssertNotEquals("Charge1 local Cost Amt was reloaded", localCostAmt1, charge1.JR_LocalCostAmt);
			AssertEquals("Charge2 local Sell Amt was not reloaded as it belongs to other Company", localSellAmt2, charge2.JR_LocalSellAmt);
			AssertEquals("Charge2 local Cost Amt was not reloaded as it belongs to other Company", localCostAmt2, charge2.JR_LocalCostAmt);

			// Change Charge1 again for the second attempt to reload
			copyCharge1.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().First(x => x.PK == sellExRate1.PK).JF_BaseRate = 5.5m;
			copyCharge1.InvoicingJob.ExchangeRates.Cast<ExchangeRate>().First(x => x.PK == costExRate1.PK).JF_BaseRate = 6.5m;
			// Updated values from Charge1
			localSellAmt1 = copyCharge1.JR_LocalSellAmt;
			localCostAmt1 = copyCharge1.JR_LocalCostAmt;

			newFactory.Save();

			reloader.Reload();

			AssertNotEquals("Charge1 local Sell Amt was not reloaded by second call in the same Company", localSellAmt1, charge1.JR_LocalSellAmt);
			AssertNotEquals("Charge1 local Cost Amt was not reloaded by second call in the same Company", localCostAmt1, charge1.JR_LocalCostAmt);
			AssertEquals("Charge2 local Sell Amt was not reloaded", localSellAmt2, charge2.JR_LocalSellAmt);
			AssertEquals("Charge2 local Cost Amt was not reloaded", localCostAmt2, charge2.JR_LocalCostAmt);

			using (Environment.Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), creator.NonCurrentCompanyBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				reloader.Reload();
			}

			AssertNotEquals("Charge1 local Sell Amt was not reloaded", localSellAmt1, charge1.JR_LocalSellAmt);
			AssertNotEquals("Charge1 local Cost Amt was not reloaded", localCostAmt1, charge1.JR_LocalCostAmt);
			AssertNotEquals("Charge2 local Sell Amt was reloaded", localSellAmt2, charge2.JR_LocalSellAmt);
			AssertNotEquals("Charge2 local Cost Amt was reloaded", localCostAmt2, charge2.JR_LocalCostAmt);
		}

		public void TestChargeWithModifiedDatRowIsNotReloaded()
		{
			var charge1 = GetNewChargeWithValidTestData();
			var charge2 = GetNewChargeWithValidTestData();
			AssertNull("Pre-condition: No Charge Reloader on the Factory", Factory.ServiceContainer.GetService<ChargeReloader>());

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			var copyCharge1 = newFactory.Load<Charge>(charge1.PK);
			var copyCharge2 = newFactory.Load<Charge>(charge2.PK);

			charge1.JR_Desc = "Test Charge 1";
			charge2.JR_Desc = "Test Charge 2";
			Factory.Save();

			var anotherCopyCharge1 = newFactory.Load<JobCharge>(charge1.PK);
			anotherCopyCharge1.JR_CostReference = "Reference";
			Assert(anotherCopyCharge1.HasChanges);
			Assert(copyCharge1.HasChanges);
			Assert("No error should be reported", string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
			AssertEquals("No error should be reported", 0, ExceptionReporterTestListener.Instance.Count);

			// Reset HasChages in code
			copyCharge1.HasChanges = false;

			AssertEquals("JR_CostReference should still be changed", "Reference", copyCharge1.JR_CostReference);
			AssertEquals("JR_CostReference should still be changed", "Reference", anotherCopyCharge1.JR_CostReference);
			AssertEquals("HasChages should be reset on all BizO around the Row", false, copyCharge1.HasChanges);
			AssertEquals("HasChages should be reset on all BizO around the Row", false, anotherCopyCharge1.HasChanges);

			AssertNull("No Charge Reloader on the new Factory", newFactory.ServiceContainer.GetService<ChargeReloader>());
			new ChargeReloader(newFactory).Reload();
			AssertNull("There is no Charge Reloader on the new Factory even we called its constructor", newFactory.ServiceContainer.GetService<ChargeReloader>());
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);
			AssertEquals("No Exception was reported", "", ExceptionReporterTestListener.Instance[0].Message);
			AssertContains("Charge 1 is in Db, HasChanges reset", "Is In DB = Yes, Has Changes = No", ErrorReporter.LastMessageReported);
			AssertContains("Charge 1 actually has changes", "Fields with changes: JR_CostReference (, Reference).", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			AssertEquals("JR_CostReference changes should not be discarded by reloading", "Reference", copyCharge1.JR_CostReference);

			copyCharge1.JR_DisplaySequence = 100;
			Assert(copyCharge1.HasChanges);

			newFactory.Save();
			AssertNull("No Charge Reloader on the new Factory after saving. It should be added on Factory Saving and removed On Factory Saved by ChargeWithCost", newFactory.ServiceContainer.GetService<ChargeReloader>());

			AssertEquals("JR_CostReference changes should be saved to DB", "Reference", copyCharge1.JR_CostReference);
		}

		public void TestDisplaySequenceWhenChargeReloaderRuns()
		{
			Charge charge = GetNewChargeWithValidTestData();
			Charge charge2 = GetNewChargeWithValidTestData();
			Charge charge3 = GetNewChargeWithValidTestData();
			charge.JR_DisplaySequence = 3;
			charge2.JR_DisplaySequence = 2;
			charge3.JR_DisplaySequence = 1;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			Charge chargeCopy = factory2.Load<Charge>(charge.PK);
			Charge charge2Copy = factory2.Load<Charge>(charge2.PK);
			Charge charge3Copy = factory2.Load<Charge>(charge3.PK);

			chargeCopy.JR_DisplaySequence = 1;
			charge2Copy.JR_DisplaySequence = 2;
			charge3Copy.JR_DisplaySequence = 3;
			chargeCopy.HasChanges = false;
			charge2Copy.HasChanges = false;
			charge3Copy.HasChanges = false;

			factory2.Save();

			AssertEquals("Job Charge Display Sequence", (ZShort)1, chargeCopy.JR_DisplaySequence);
			AssertEquals("Job Charge Display Sequence", (ZShort)2, charge2Copy.JR_DisplaySequence);
			AssertEquals("Job Charge Display Sequence", (ZShort)3, charge3Copy.JR_DisplaySequence);

			AssertEquals("DeveloperException should not been reported", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestChargeDeletedInDifferentSession()
		{
			var charge1 = GetNewChargeWithValidTestData();
			var charge2 = GetNewChargeWithValidTestData();
			Factory.Save();

			ChargeReloader.ReloadChargesInFactoryPreservingDisplayOrder(Factory);
			AssertEquals(false, charge1.IsDeleted);
			AssertEquals(false, charge2.IsDeleted);

			BusinessObjectFactory newFactory = new BusinessObjectFactory
			{
				RefreshEnabled = false
			};
			var newCharge = newFactory.Load<Charge>(charge1.PK);
			newCharge.Delete();
			newFactory.Save();

			ChargeReloader.ReloadChargesInFactoryPreservingDisplayOrder(Factory);
			AssertEquals(true, charge1.IsDeleted);
			AssertEquals(false, charge2.IsDeleted);
		}

		public void TestReloadAllChargesAndCostsWithoutChanges()
		{
			var charge1 = GetNewChargeWithValidTestData();
			var charge2 = GetNewChargeWithValidTestData();
			var cost1 = GetNewCostWithValidTestData();
			charge1.JR_E6 = cost1.PK;
			charge2.JR_E6 = cost1.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory
			{
				RefreshEnabled = false
			};
			var charge1Copy = newFactory.Load<Charge>(charge1.PK);
			var charge2Copy = newFactory.Load<Charge>(charge2.PK);
			var cost1Copy = newFactory.Load<JobConsolCost>(cost1.PK);
			charge1Copy.JR_Desc = "charge1 desc";
			charge2Copy.JR_Desc = "charge2 desc";
			cost1Copy.E6_Description = "cost1 desc";
			newFactory.Save();

			AssertNotEquals((ZString)"charge1 desc", charge1.JR_Desc);
			AssertNotEquals((ZString)"charge2 desc", charge2.JR_Desc);
			AssertNotEquals((ZString)"cost1 desc", cost1.E6_Description);

			var charge3 = GetNewChargeWithValidTestData();
			charge3.JR_Desc = "charge3 desc before";
			ChargeReloader.ReloadChargesInFactoryPreservingDisplayOrder(Factory);

			AssertEquals("charge1 should be reloaded", "charge1 desc", charge1.JR_Desc);
			AssertEquals("charge2 should be reloaded", "charge2 desc", charge2.JR_Desc);
			AssertEquals("cost1 should be reloaded", "cost1 desc", cost1.E6_Description);

			var charge3Copy = newFactory.Load<Charge>(charge3.PK);
			charge3.JR_Desc = "charge3 desc after";
			newFactory.Save();

			ChargeReloader.ReloadChargesInFactoryPreservingDisplayOrder(Factory);
			AssertEquals("charge3 should be reloaded", "charge3 desc after", charge3.JR_Desc);
		}

		public void TestNotToReloadWhenChargeOrCostWithChangesOrNotInDB()
		{
			var charge1 = GetNewChargeWithValidTestData();
			var charge2 = GetNewChargeWithValidTestData();
			var cost1 = GetNewCostWithValidTestData();
			charge1.JR_Desc = "charge1 desc before";
			charge2.JR_Desc = "charge2 desc before";
			cost1.E6_Description = "cost1 desc before";
			charge1.JR_E6 = cost1.PK;
			charge2.JR_E6 = cost1.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory
			{
				RefreshEnabled = false
			};
			var charge1Copy = newFactory.Load<Charge>(charge1.PK);
			var charge2Copy = newFactory.Load<Charge>(charge2.PK);
			var cost1Copy = newFactory.Load<JobConsolCost>(cost1.PK);
			charge1Copy.JR_Desc = "charge1 desc after";
			charge2Copy.JR_Desc = "charge2 desc after";
			cost1Copy.E6_Description = "cost1 desc after";
			newFactory.Save();

			cost1.E6_Description = "cost1 desc change";

			AssertEquals(true, cost1.HasChanges);

			ChargeReloader.ReloadChargesInFactoryPreservingDisplayOrder(Factory);

			AssertEquals("charge1 shouldn't be reloaded", "charge1 desc before", charge1.JR_Desc);
			AssertEquals("charge2 shouldn't be reloaded", "charge2 desc before", charge2.JR_Desc);
			AssertEquals("cost1 shouldn't be reloaded", "cost1 desc change", cost1.E6_Description);

			cost1.E6_Description = "cost1 desc before";
			cost1.HasChanges = false;
			AssertEquals(false, cost1.HasChanges);

			var charge3 = GetNewChargeWithValidTestData();
			charge3.JR_E6 = cost1.PK;
			AssertEquals(false, charge3.IsInDatabase);

			ChargeReloader.ReloadChargesInFactoryPreservingDisplayOrder(Factory);

			AssertEquals("charge1 shouldn't be reloaded", "charge1 desc before", charge1.JR_Desc);
			AssertEquals("charge2 shouldn't be reloaded", "charge2 desc before", charge2.JR_Desc);
			AssertEquals("cost1 shouldn't be reloaded", "cost1 desc before", cost1.E6_Description);
		}

		public void TestReloadChargesPreservingDisplayOrder_ReloadRelatedChargesAndCostsOnly()
		{
			var charge1 = GetNewChargeWithValidTestData();
			var charge2 = GetNewChargeWithValidTestData();
			var cost1 = GetNewCostWithValidTestData();

			charge1.JR_Desc = "charge1 desc before";
			charge2.JR_Desc = "charge2 desc before";
			cost1.E6_Description = "cost1 desc before";
			charge1.JR_E6 = cost1.PK;
			charge2.JR_E6 = cost1.PK;

			var charge3 = GetNewChargeWithValidTestData();
			var cost2 = GetNewCostWithValidTestData();

			charge3.JR_Desc = "charge3 desc before";
			cost2.E6_Description = "cost2 desc before";
			charge3.JR_E6 = cost2.PK;

			var charge4 = GetNewChargeWithValidTestData();
			var cost3 = GetNewCostWithValidTestData();

			charge4.JR_Desc = "charge4 desc before";
			cost3.E6_Description = "cost3 desc before";
			charge4.JR_E6 = cost3.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory
			{
				RefreshEnabled = false
			};
			var charge1Copy = newFactory.Load<Charge>(charge1.PK);
			var charge2Copy = newFactory.Load<Charge>(charge2.PK);
			var charge3Copy = newFactory.Load<Charge>(charge3.PK);
			var charge4Copy = newFactory.Load<Charge>(charge4.PK);
			var cost1Copy = newFactory.Load<JobConsolCost>(cost1.PK);
			var cost2Copy = newFactory.Load<JobConsolCost>(cost2.PK);
			var cost3Copy = newFactory.Load<JobConsolCost>(cost3.PK);

			charge1Copy.JR_Desc = "charge1 desc after";
			charge2Copy.JR_Desc = "charge2 desc after";
			charge3Copy.JR_Desc = "charge3 desc after";
			charge4Copy.JR_Desc = "charge4 desc after";
			cost1Copy.E6_Description = "cost1 desc after";
			cost2Copy.E6_Description = "cost2 desc after";
			cost3Copy.E6_Description = "cost3 desc after";

			newFactory.Save();

			Charge[] charges = { charge1 };
			ChargeReloader.ReloadChargesPreservingDisplayOrder(charges);

			AssertEquals("charge1 should be reloaded", "charge1 desc after", charge1.JR_Desc);
			AssertEquals("charge2 should be reloaded", "charge2 desc after", charge2.JR_Desc);
			AssertEquals("cost1 should be reloaded", "cost1 desc after", cost1.E6_Description);

			AssertEquals("charge3 shouldn't be reloaded", "charge3 desc before", charge3.JR_Desc);
			AssertEquals("cost2 shouldn't be reloaded", "cost2 desc before", cost2.E6_Description);
			AssertEquals("charge4 shouldn't be reloaded", "charge4 desc before", charge4.JR_Desc);
			AssertEquals("cost3 shouldn't be reloaded", "cost3 desc before", cost3.E6_Description);

			JobConsolCost[] costs = { cost2 };
			ChargeReloader.ReloadChargesPreservingDisplayOrder(charges, costs);

			AssertEquals("charge1 should be reloaded", "charge1 desc after", charge1.JR_Desc);
			AssertEquals("charge2 should be reloaded", "charge2 desc after", charge2.JR_Desc);
			AssertEquals("cost1 should be reloaded", "cost1 desc after", cost1.E6_Description);

			AssertEquals("charge3 should be reloaded", "charge3 desc after", charge3.JR_Desc);
			AssertEquals("cost2 should be reloaded", "cost2 desc after", cost2.E6_Description);

			AssertEquals("charge4 shouldn't be reloaded", "charge4 desc before", charge4.JR_Desc);
			AssertEquals("cost3 shouldn't be reloaded", "cost3 desc before", cost3.E6_Description);
		}

		Charge GetNewChargeWithValidTestData()
		{
			return Factory.NewWithValidTestData<Charge>();
		}

		JobConsolCost GetNewCostWithValidTestData()
		{
			return Factory.NewWithValidTestData<JobConsolCost>();
		}
	}
}

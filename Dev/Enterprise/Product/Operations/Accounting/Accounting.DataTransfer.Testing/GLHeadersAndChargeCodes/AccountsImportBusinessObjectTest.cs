using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes.Testing
{
	[TestedType(typeof(AccountsImportBusinessObject))]
	public class AccountsImportBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestDeleteAllChargeCodes_NoGlobal()
		{
			ZQuery chargeCodesQuery = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			Assert(Factory.GetDatabaseCount(typeof(AccChargeCode), chargeCodesQuery) > 0);
			AccountsImportBusinessObjectForTest.DeleteAllChargeCodes();
			try
			{
				Factory.Save();
			}
			catch
			{
				Fail("There should be no errors during delete of all Charge Codes.");
			}
			Assert(Factory.GetDatabaseCount(typeof(AccChargeCode), chargeCodesQuery) == 0);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestDeleteAllChargeCodes_WithGlobal()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			ZQuery chargeCodesQuery = new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			Assert(Factory.GetDatabaseCount(typeof(AccChargeCode), chargeCodesQuery) > 0);
			ZQuery globalChargeCodesQuery = new ZQuery(AccChargeCodeSchema.AC_GC, null);
			Assert(Factory.GetDatabaseCount(typeof(AccChargeCode), globalChargeCodesQuery) > 0);

			AccountsImportBusinessObjectForTest.DeleteAllChargeCodes();
			try
			{
				Factory.Save();
			}
			catch
			{
				Fail("There should be no errors during delete of all Charge Codes.");
			}
			Assert(Factory.GetDatabaseCount(typeof(AccChargeCode), chargeCodesQuery) == 0);
			Assert(Factory.GetDatabaseCount(typeof(AccChargeCode), globalChargeCodesQuery) == 0);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1044:FactoryGetDatabaseCountCollectionCountRule", Justification = "Testing")]
		public void TestDeleteAllGLHeaders()
		{
			Assert(Factory.GetDatabaseCount(typeof(AccGLHeader)) > 0);
			Assert("Should have some Control Accounts set in Registry", AccountingConfigurationRegistry.Instance.AreControlAccountRegistryItemsSet());

			AccountsImportBusinessObjectForTest.DeleteAllChargeCodes();
			AccountsImportBusinessObjectForTest.DeleteAllGLHeaders();
			try
			{
				Factory.Save();
			}
			catch
			{
				Fail("There Should be no errors during delete of all GLHeaders");
			}
			Assert(Factory.GetDatabaseCount(typeof(AccGLHeader)) == 0);
			AssertEquals("Should have no Control Accounts set in Registry", false, AccountingConfigurationRegistry.Instance.AreControlAccountRegistryItemsSet());
		}

		public void TestRevertAllControlAccounts()
		{
			Assert("Should have some Control Accounts set in Registry", AccountingConfigurationRegistry.Instance.AreControlAccountRegistryItemsSet());
			Guid jobRevenueJournalControlAccount = AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Value;
			Guid accruedCostControlAccount = AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value;
			Guid accruedRevenueControlAccount = AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value;
			Guid aPControlAccount = AccountingConfigurationRegistry.Instance.APControlAccount.Value;
			Guid aPSuspenseControlAccount = AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.Value;
			Guid aRControlAccount = AccountingConfigurationRegistry.Instance.ARControlAccount.Value;
			Guid aRSuspenseControlAccount = AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.Value;
			Guid aRDiscountAccount = AccountingConfigurationRegistry.Instance.ARDiscountAccount.Value;
			Guid aPDiscountAccount = AccountingConfigurationRegistry.Instance.APDiscountAccount.Value;
			Guid exchangeGainAccount = AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.Value;
			Guid exchangeLossAccount = AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount.Value;
			Guid gSTInputControlAccount = AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value;
			Guid gSTOutputControlAccount = AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value;
			Guid pendingGSTInputControlAccount = AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.Value;
			Guid pendingGSTOutputControlAccount = AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.Value;
			Guid overpaymentsControlAccount = AccountingConfigurationRegistry.Instance.OverpaymentsAccount.Value;
			Guid wHTInputControlAccount = AccountingConfigurationRegistry.Instance.WHTInputControlAccount.Value;
			Guid wHTOutputControlAccount = AccountingConfigurationRegistry.Instance.WHTOutputControlAccount.Value;
			Guid foreignCurrencyGLBalanceAdjustmentAccount = AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.Value;
			Guid currencyAdjustmentExchangeGainAccount = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value;
			Guid currencyAdjustmentExchangeLossAccount = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value;

			AccountsImportBusinessObjectForTest.ClearAllControlAccounts();
			AssertEquals("Should have no Control Accounts set in Registry", false, AccountingConfigurationRegistry.Instance.AreControlAccountRegistryItemsSet());

			AccountsImportBusinessObjectForTest.RevertAllControlAccounts();
			Assert("Should have some Control Accounts set in Registry", AccountingConfigurationRegistry.Instance.AreControlAccountRegistryItemsSet());

			AssertEquals("JobRevenueJournalControlAccount did not revert", jobRevenueJournalControlAccount, AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.Value);
			AssertEquals("AccruedCostControlAccount did not revert", accruedCostControlAccount, AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value);
			AssertEquals("AccruedRevenueControlAccount did not revert", accruedRevenueControlAccount, AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value);
			AssertEquals("APControlAccount did not revert", aPControlAccount, AccountingConfigurationRegistry.Instance.APControlAccount.Value);
			AssertEquals("APSuspenseControlAccount did not revert", aPSuspenseControlAccount, AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.Value);
			AssertEquals("ARControlAccount did not revert", aRControlAccount, AccountingConfigurationRegistry.Instance.ARControlAccount.Value);
			AssertEquals("ARSuspenseControlAccount did not revert", aRSuspenseControlAccount, AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.Value);
			AssertEquals("AR DiscountAccount did not revert", aRDiscountAccount, AccountingConfigurationRegistry.Instance.ARDiscountAccount.Value);
			AssertEquals("AP DiscountAccount did not revert", aPDiscountAccount, AccountingConfigurationRegistry.Instance.APDiscountAccount.Value);
			AssertEquals("ExchangeGainAccount did not revert", exchangeGainAccount, AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.Value);
			AssertEquals("ExchangeLossAccount did not revert", exchangeLossAccount, AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount.Value);
			AssertEquals("GSTInputControlAccount did not revert", gSTInputControlAccount, AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value);
			AssertEquals("GSTOutputControlAccount did not revert", gSTOutputControlAccount, AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value);
			AssertEquals("PendingGSTInputControlAccount did not revert", pendingGSTInputControlAccount, AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.Value);
			AssertEquals("PendingGSTOutputControlAccount did not revert", pendingGSTOutputControlAccount, AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.Value);
			AssertEquals("OverpaymentsControlAccount did not revert", overpaymentsControlAccount, AccountingConfigurationRegistry.Instance.OverpaymentsAccount.Value);
			AssertEquals("WHTInputControlAccount did not revert", wHTInputControlAccount, AccountingConfigurationRegistry.Instance.WHTInputControlAccount.Value);
			AssertEquals("WHTOutputControlAccount did not revert", wHTOutputControlAccount, AccountingConfigurationRegistry.Instance.WHTOutputControlAccount.Value);
			AssertEquals("UnrealizedExchangeDifferenceAccount did not revert", foreignCurrencyGLBalanceAdjustmentAccount, AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.Value);
			AssertEquals("CurrencyAdjustmentExchangeGainAccount did not revert", currencyAdjustmentExchangeGainAccount, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value);
			AssertEquals("CurrencyAdjustmentExchangeLossAccount did not revert", currencyAdjustmentExchangeLossAccount, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value);
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccountsImportBusinessObjectForTest = new AccountsImportBusinessObject(Factory);
		}

		#region Implementation

		AccountsImportBusinessObject AccountsImportBusinessObjectForTest;

		#endregion
	}
}

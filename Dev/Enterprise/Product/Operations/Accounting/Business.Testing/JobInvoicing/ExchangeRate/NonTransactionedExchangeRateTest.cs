using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[UseSnapshotProtection]
	public class NonTransactionedExchangeRateTest : TestCase
	{
		public void TestResettingHasChangesWhenBaseRateWasModified()
		{
			AssertChargeReloaderReloadsExRatesWhenBaseRateWasModified(true);
		}

		public void TestChargereloaaderPreventsMergeConflictsWhenBaseRateWasModified()
		{
			AssertChargeReloaderReloadsExRatesWhenBaseRateWasModified(false);
		}

		void AssertChargeReloaderReloadsExRatesWhenBaseRateWasModified(bool noChargeToReload)
		{
			// Prepare records in CW1 instance A.
			var factory = new BusinessObjectFactory();
			var objectCreator = new TestObjectCreator(factory);
			var shipment = objectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			var job = objectCreator.CreateJob(shipment, objectCreator.LocalClient, 0, objectCreator.Agent, 0);
			var exRate = job.ExchangeRates.AddNew();
			exRate.JF_RX_NKRateCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			exRate.JF_BaseRate = 1.23m;
			factory.Save();

			var charge = objectCreator.CreateCharge(job, objectCreator.CC1, 0m, 0m);
			charge.JR_OH_SellAccount = objectCreator.Debtor.PK;
			charge.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge.JR_OSSellAmt = 100m;
			AssertEquals(1.23m, charge.JR_OSSellExRate);
			factory.Save();

			// Adjust job exchange rate in CW1 instance B.
			var factoryInAnotherInstance = new BusinessObjectFactory() { RefreshEnabled = false };
			var exRateInAnotherInstance = factoryInAnotherInstance.Load<ExchangeRate>(exRate.PK);
			exRateInAnotherInstance.JF_BaseRate = 1.5m;
			factoryInAnotherInstance.Save();

			// Behavior back in CW1 instance A.
			var charge2 = objectCreator.CreateCharge(job, objectCreator.CC1, 0m, 0m);
			charge2.JR_OH_SellAccount = objectCreator.Debtor.PK;
			charge2.JR_RX_NKSellCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			charge2.JR_OSSellAmt = 100m;
			if (noChargeToReload)
			{
				charge.JR_OSSellAmt = 105m;
			}
			AssertEquals("When we modify charge that leaves no charges to reload and Ex Rate would not be reloaded", noChargeToReload, charge.HasChanges);
			factory.Save();

			AssertEquals("if we have at least one reloadable charge related to Ex Rate the Ex Rate will be reloaded", noChargeToReload ? 1.23m : 1.5m, exRate.JF_BaseRate);
			AssertEquals("if Ex Rate is reloaded then charge Rate will be updated", noChargeToReload ? 1.23m : 1.5m, charge.JR_OSSellExRate);
			AssertEquals("if Ex Rate is reloaded then charge2 Rate will be updated", noChargeToReload ? 1.23m : 1.5m, charge2.JR_OSSellExRate);

			exRate.JF_BaseRate = 1.4m;
			if (noChargeToReload)
			{
				var ex = AssertExceptionThrown<ZSaveConcurrencyException>("Save cause auto merge concurrency warning.", () => factory.Save());
				// Simulate auto merge.
				ZExceptionReporting.HandleSaveException(ex);
				factory.Save();

				AssertNullOrEmpty("Should not report any error.", ErrorReporter.LastKeyReported);

				AssertEquals(1.5m, charge2.JR_OSSellExRate);
				AssertEquals("Parent object has no changes,", false, charge2.HasChanges);
				AssertEquals("JR_OSSellExRateInfo.HasChanges should be false", false, charge2.JR_OSSellExRateInfo.HasChanges);

				foreach (var bizO in ex.Factory.GetChanges().GetChangedObjects())
				{
					AssertEquals("No critical property changes in concurreny exception. So that auto merge is valid.", bizO.CanMerge());
				}

				ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			}
			else
			{
				AssertNoExceptionThrown("Should not have any merge conflicts", () => factory.Save());
			}
		}
	}
}
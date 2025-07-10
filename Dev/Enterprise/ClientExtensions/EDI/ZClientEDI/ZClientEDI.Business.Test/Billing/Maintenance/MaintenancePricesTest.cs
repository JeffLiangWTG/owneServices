using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Environment;

namespace Enterprise.Client.EDI.Billing.Business.Maintenance.Test
{
	internal class MaintenancePricesTest : TestCaseWithFactory
	{
		public void TestUpdateRenewal()
		{
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "EEE");
			MaintenanceBillRecipient recipient = new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", ZDateTime.Today);
			licHeader.Billing.L0_NextMaintenancePercent = 30m;
			licHeader.Billing.L0_NextNewSeatMaintenancePercent = 30m;
			var priceList = BillingTestHelper.CreateMaintenancePriceList(licHeader);
			licHeader.GetCoreModule().LM_UserCount = 11;
			priceList.Items.FindByCode("COR").L7_Price = 1000m;

			MaintenancePrices prices = new MaintenancePrices(Factory, licHeader, priceList, new ZDateTime(2010, 1, 1));

			AssertEquals("pre", 0, licHeader.GetCoreModule().LM_RenewalUserCount);
			AssertEquals("pre", (short)0, licHeader.GetCoreModule().LM_PartPurchasedCount);
			AssertEquals(0m, prices.PriceOldSeats);
			AssertEquals(11000m, prices.PriceNewSeats);
			AssertPerModuleAmount(prices);

			prices.UpdateRenewal(Factory);
			AssertEquals("renewal count", 11, licHeader.GetCoreModule().LM_RenewalUserCount);
			AssertEquals("previous renewal count", (short)0, licHeader.GetCoreModule().LM_PartPurchasedCount);
			AssertEquals(new ZDateTime(2010, 1, 1), licHeader.LA_ContractRenewalIssued);
			AssertPerModuleAmount(prices);

			prices = new MaintenancePrices(Factory, licHeader, priceList, new ZDateTime(2010, 1, 1));
			AssertEquals("price old seats unchanged", 0m, prices.PriceOldSeats);
			AssertEquals("price new seats unchanged", 11000m, prices.PriceNewSeats);
			AssertPerModuleAmount(prices);

			prices = new MaintenancePrices(Factory, licHeader, priceList, new ZDateTime(2011, 1, 1));
			AssertEquals("price old seats updated", 11000m, prices.PriceOldSeats);
			AssertEquals("price new seats updated", 0m, prices.PriceNewSeats);

			prices.UpdateRenewal(Factory);
			AssertEquals("renewal count", 11, licHeader.GetCoreModule().LM_RenewalUserCount);
			AssertEquals("previous renewal count", (short)11, licHeader.GetCoreModule().LM_PartPurchasedCount);
			AssertPerModuleAmount(prices);
		}

		public void TestUpdateRenewal_MaintenancePercent()
		{
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "EEE");
			MaintenanceBillRecipient recipient = new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", ZDateTime.Today);
			licHeader.Billing.L0_NextMaintenancePercent = 5m;
			licHeader.Billing.L0_NextNewSeatMaintenancePercent = 20m;
			var priceList = BillingTestHelper.CreateMaintenancePriceList(licHeader);
			licHeader.GetCoreModule().LM_UserCount = 20;
			priceList.Items.FindByCode("COR").L7_Price = 1000m;

			// 20 new seats at $1000 and 20% maintenance
			MaintenancePrices prices = new MaintenancePrices(Factory, licHeader, priceList, new ZDateTime(2010, 1, 1));
			AssertEquals(0m, licHeader.Billing.L0_CurrentMaintenancePercent);

			prices.UpdateRenewal(Factory);
			AssertEquals(5m, licHeader.Billing.L0_CurrentMaintenancePercent);
			AssertEquals(20m, licHeader.Billing.L0_NextMaintenancePercent);
			AssertPerModuleAmount(prices);

			// Reinvoice
			prices.UpdateRenewal(Factory);
			AssertEquals(5m, licHeader.Billing.L0_CurrentMaintenancePercent);
			AssertEquals(20m, licHeader.Billing.L0_NextMaintenancePercent);
			AssertPerModuleAmount(prices);

			// Reinvoice at 25%
			licHeader.Billing.L0_NextNewSeatMaintenancePercent = 25m;
			prices.UpdateRenewal(Factory);
			AssertEquals(5m, licHeader.Billing.L0_CurrentMaintenancePercent);
			AssertEquals(25m, licHeader.Billing.L0_NextMaintenancePercent);
			AssertPerModuleAmount(prices);

			// Next period, 5 new seats at 40%
			licHeader.Billing.L0_NextNewSeatMaintenancePercent = 40m;
			licHeader.GetCoreModule().LM_UserCount = 25;
			prices = new MaintenancePrices(Factory, licHeader, priceList, new ZDateTime(2011, 1, 1));

			AssertEquals(28m, prices.CombinedMaintenancePercent);
			prices.UpdateRenewal(Factory);
			AssertEquals("percent for this period", 25m, licHeader.Billing.L0_CurrentMaintenancePercent);
			AssertEquals("percent for next period", 28m, licHeader.Billing.L0_NextMaintenancePercent);
			AssertPerModuleAmount(prices);

			// Reinvoice with historical seats at 20%
			prices.NewPercent = 20m;
			prices.UpdateRenewal(Factory);
			AssertEquals("percent for this period", 20m, licHeader.Billing.L0_CurrentMaintenancePercent);
			AssertEquals("percent for this period", 20m, prices.NewPercent);
			AssertEquals("percent for next period", (20 * 20m + 5 * 40m) / 25, licHeader.Billing.L0_NextMaintenancePercent);
			AssertPerModuleAmount(prices);

			// Reinvoice
			prices.UpdateRenewal(Factory);
			AssertEquals("percent for this period", 20m, licHeader.Billing.L0_CurrentMaintenancePercent);
			AssertEquals("percent for this period", 20m, prices.NewPercent);
			AssertEquals("percent for next period", (20 * 20m + 5 * 40m) / 25, licHeader.Billing.L0_NextMaintenancePercent);
			AssertPerModuleAmount(prices);
		}

		void AssertPerModuleAmount(MaintenancePrices prices)
		{
			var moduleTotal = prices.Modules.Cast<MaintenanceModule>().Sum(x => x.TotalMaintenance);
			var pricesTotal = prices.Maintenance;
			Assert("Amounts equal within rounding error: " + moduleTotal + " ~= " + pricesTotal, Math.Abs(pricesTotal - moduleTotal) < 1);
		}

		public void TestNewPercent()
		{
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "EEE");
			MaintenanceBillRecipient recipient = new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", ZDateTime.Today);
			licHeader.Billing.L0_NextMaintenancePercent = 5m;
			licHeader.Billing.L0_NextNewSeatMaintenancePercent = 20m;
			var priceList = BillingTestHelper.CreateMaintenancePriceList(licHeader);
			licHeader.GetCoreModule().LM_UserCount = 20;
			priceList.Items.FindByCode("COR").L7_Price = 1000m;

			MaintenancePrices prices = new MaintenancePrices(Factory, licHeader, priceList, new ZDateTime(2010, 1, 1));
			AssertEquals("before renewal", licHeader.Billing.L0_NextMaintenancePercent, prices.NewPercent);
			licHeader.Billing.L0_NextMaintenancePercent = 10m;
			AssertEquals("before renewal", licHeader.Billing.L0_NextMaintenancePercent, prices.NewPercent);

			prices.UpdateRenewal(Factory);
			AssertEquals("after renewal", licHeader.Billing.L0_CurrentMaintenancePercent, prices.NewPercent);
			licHeader.Billing.L0_CurrentMaintenancePercent = 88m;
			AssertEquals("after renewal", licHeader.Billing.L0_CurrentMaintenancePercent, prices.NewPercent);
		}

		public void TestCombinedMaintenancePercent()
		{
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "EEE");
			licHeader.Billing.L0_RenewalMonths = 12;
			licHeader.Billing.L0_NextMaintenancePercent = 5m;
			licHeader.Billing.L0_NextNewSeatMaintenancePercent = 20m;
			var priceList = BillingTestHelper.CreateMaintenancePriceList(licHeader);

			licHeader.GetCoreModule().LM_UserCount = 20;
			licHeader.GetCoreModule().LM_RenewalUserCount = 5;
			priceList.Items.FindByCode("COR").L7_Price = 1000m;

			MaintenancePrices prices = new MaintenancePrices(Factory, licHeader, priceList, new ZDateTime(2010, 1, 1));
			var oldUserTotalPrice = 5 * 1000m;
			var newUserTotalPrice = 15 * 1000m;

			AssertEquals(prices.Maintenance / (oldUserTotalPrice + newUserTotalPrice) * 100m, prices.CombinedMaintenancePercent);

			licHeader.Billing.L0_RenewalMonths = 6;
			AssertEquals(prices.Maintenance * (12 / 6) / (oldUserTotalPrice + newUserTotalPrice) * 100m, prices.CombinedMaintenancePercent);
		}

		public void TestMaintenanceAtLastPercentages()
		{
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "EEE");
			MaintenanceBillRecipient recipient = new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", ZDateTime.Today);
			licHeader.Billing.L0_LastMaintenancePercent = 10m;
			licHeader.Billing.L0_LastNewSeatMaintenancePercent = 15m;
			licHeader.Billing.L0_NextMaintenancePercent = 20m;
			licHeader.Billing.L0_NextNewSeatMaintenancePercent = 30m;
			var priceList = BillingTestHelper.CreateMaintenancePriceList(licHeader);
			licHeader.GetCoreModule().LM_UserCount = 11;
			licHeader.GetCoreModule().LM_RenewalUserCount = 7;
			priceList.Items.FindByCode("COR").L7_Price = 1000m;

			MaintenancePrices prices = new MaintenancePrices(Factory, licHeader, priceList, new ZDateTime(2010, 1, 1));

			AssertEquals(7 * 1000 * 0.1m + 4 * 1000 * 0.15m, prices.MaintenanceAtLastPercentages);
		}

		public void TestMaintenanceSurcharge()
		{
			LicenceHeader licHeader = BillingTestHelper.CreateMaintenanceLicence(Factory, "EEE");
			MaintenanceBillRecipient recipient = new MaintenanceBillRecipient(Factory, Env.CurrentBranch.PK, licHeader.Company.LC_OH, "AUD", ZDateTime.Today);
			licHeader.Billing.L0_FixedMaintenanceAmount = 1232;
			licHeader.Billing.L0_Surcharge = 1.03m;
			licHeader.Billing.L0_SurchargeDescription = "Surcharge description test";
			var priceList = BillingTestHelper.CreateMaintenancePriceList(licHeader);
			licHeader.GetCoreModule().LM_UserCount = 11;
			priceList.Items.FindByCode("COR").L7_Price = 1000m;

			MaintenancePrices prices = new MaintenancePrices(Factory, licHeader, priceList, new ZDateTime(2010, 1, 1));

			AssertEquals(1232m, prices.Maintenance);
			AssertEquals("1232m * 1.03m / 100", 12.69m, prices.SurchargeAmount);
			AssertEquals("Surcharge description test", prices.SurchangeDescription);
			AssertEquals("1232m * 101.03m / 100", 1244.69m, prices.FinalAmount);
		}
	}
}
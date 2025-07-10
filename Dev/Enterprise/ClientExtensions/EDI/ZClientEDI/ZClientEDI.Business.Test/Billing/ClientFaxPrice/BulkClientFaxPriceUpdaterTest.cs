using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(BulkClientFaxPriceUpdater))]
	public class BulkClientFaxPriceUpdaterTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2013, 7, 26)]
		public void TestDefaultValues()
		{
			BulkClientFaxPriceUpdater updater = new BulkClientFaxPriceUpdater(Factory);
			AssertEquals("MonthAndYearPeriod is set to the first day of the current month", new ZDateTime(2013, 7, 1), updater.MonthAndYearPeriod);
		}

		public void TestMonthAndYearPeriod()
		{
			// Fax Prices for 2 monthly periods
			ClientFaxPrice priceAUD1 = Factory.New<ClientFaxPrice>();
			priceAUD1.CFP_RX_NKCurrencyCode = "AUD";
			priceAUD1.CFP_PageRate = 0.08m;
			priceAUD1.CFP_Month = 6;
			priceAUD1.CFP_Year = 2012;
			ClientFaxPrice priceNZD1 = Factory.New<ClientFaxPrice>();
			priceNZD1.CFP_RX_NKCurrencyCode = "NZD";
			priceNZD1.CFP_PageRate = 0.12m;
			priceNZD1.CFP_Month = 6;
			priceNZD1.CFP_Year = 2012;

			ClientFaxPrice priceAUD2 = Factory.New<ClientFaxPrice>();
			priceAUD2.CFP_RX_NKCurrencyCode = "AUD";
			priceAUD2.CFP_PageRate = 0.11m;
			priceAUD2.CFP_Month = 7;
			priceAUD2.CFP_Year = 2012;
			ClientFaxPrice priceNZD2 = Factory.New<ClientFaxPrice>();
			priceNZD2.CFP_RX_NKCurrencyCode = "NZD";
			priceNZD2.CFP_PageRate = 0.15m;
			priceNZD2.CFP_Month = 7;
			priceNZD2.CFP_Year = 2012;

			Factory.Save();

			BulkClientFaxPriceUpdater updater = new BulkClientFaxPriceUpdater(Factory);
			updater.MonthAndYearPeriod = new ZDateTime(2012, 6, 1);
			AssertCollectionContains("2012/06 AUD rate should be in the list", priceAUD1, updater.ClientFaxPriceList);
			AssertCollectionContains("2012/06 NZD rate should be in the list", priceNZD1, updater.ClientFaxPriceList);

			updater.MonthAndYearPeriod = new ZDateTime(2012, 7, 1);
			AssertCollectionContains("2012/07 AUD rate should be in the list", priceAUD2, updater.ClientFaxPriceList);
			AssertCollectionContains("2012/07 NZD rate should be in the list", priceNZD2, updater.ClientFaxPriceList);

			updater.MonthAndYearPeriod = new ZDateTime(2013, 7, 1);
			AssertEquals("No rates for 2013/07", 0, updater.ClientFaxPriceList.Count);
		}

		public void TestValidateMonthAndYearPeriod()
		{
			BulkClientFaxPriceUpdater updater = new BulkClientFaxPriceUpdater(Factory);
			updater.ValidateMonthAndYearPeriod();
			Assert("MonthAndYearPeriod is not empty due to default values, not expecting error", !updater.MonthAndYearPeriodInfo.HasNotifications());

			updater.MonthAndYearPeriod = ZDateTime.Empty;
			Assert("MonthAndYearPeriod is empty , expecting error", updater.MonthAndYearPeriodInfo.HasErrors());

			updater.MonthAndYearPeriod = ZDateTime.Invalid;
			Assert("MonthAndYearPeriod is not valid, expecting error", updater.MonthAndYearPeriodInfo.HasErrors());

			updater.MonthAndYearPeriod = new ZDateTime(2013, 7, 31);
			Assert("MonthAndYearPeriod is not set to first day of the month, expecting error", updater.MonthAndYearPeriodInfo.HasErrors());

			updater.MonthAndYearPeriod = new ZDateTime(2013, 8, 1);
			Assert("MonthAndYearPeriod is valid, not expecting error", !updater.MonthAndYearPeriodInfo.HasNotifications());
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BulkClientFaxPriceUpdater(Factory);
		}

		#endregion
	}
}

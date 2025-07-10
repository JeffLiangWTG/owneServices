using System;
using System.Drawing;
using Enterprise.Core;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RatingRegistryTest : TransactionedTestCase
	{
		public void TestFreightSearchPriorities()
		{
			Registry.Rating.FreightSearchPriorities = "Test";
			AssertEquals("FreightSearchPriorities", "Test", Registry.Rating.FreightSearchPriorities);
		}
		public void TestRateValidityPeriod()
		{
			AssertEquals("Default RateValidityPeriod", 6, Registry.Rating.RateValidityPeriod);
			Registry.RawRegistry.RateValidityPeriod.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 7);
			AssertEquals("RateValidityPeriod", 7, Registry.Rating.RateValidityPeriod);
		}

		public void TestCostRateValidityPeriod()
		{
			AssertEquals("Default CostRateValidityPeriod", 6, Registry.Rating.CostRateValidityPeriod);
			Registry.RawRegistry.CostRateValidityPeriod.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 7);
			AssertEquals("CostRateValidityPeriod", 7, Registry.Rating.CostRateValidityPeriod);
		}

		public void TestGlobalTariffValidityPeriod()
		{
			AssertEquals("Default GlobalTariffValidityPeriod", 6, Registry.Rating.GlobalTariffValidityPeriod);
			Registry.RawRegistry.GlobalTariffValidityPeriod.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 7);
			AssertEquals("GlobalTariffValidityPeriod", 7, Registry.Rating.GlobalTariffValidityPeriod);
		}

		public void TestQuoteValidityPeriod()
		{
			AssertEquals("Default QuoteValidityPeriod", 1, Registry.RawRegistry.QuoteValidityPeriod.Value);
			Registry.RawRegistry.QuoteValidityPeriod.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 7);
			AssertEquals("QuoteValidityPeriod", 7, Registry.RawRegistry.QuoteValidityPeriod.Value);
		}

		public void TestWebRateValidityPeriod()
		{
			IntRegistryDataType dataType = (IntRegistryDataType)Registry.RawRegistry.WebRateValidityPeriod.DataType;
			AssertEquals("LowerBound", 1, (int)dataType.LowerBound);
			AssertEquals("UpperBound", int.MaxValue, (int)dataType.UpperBound);

			AssertEquals("Default WebRateValidityPeriod", 6, Registry.Rating.WebRateValidityPeriod);
			Registry.RawRegistry.WebRateValidityPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 7);
			AssertEquals("WebRateValidityPeriod", 7, Registry.Rating.WebRateValidityPeriod);
		}

		public void TestRateLineSpacing()
		{
			AssertEquals("Default RateLineSpacing", 0, Registry.Rating.RateLineSpacing);
			Registry.RawRegistry.RateLineSpacing.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 1);
			AssertEquals("RateLineSpacing", 1, Registry.Rating.RateLineSpacing);
		}

		public void TestExpiredRateNotificationPeriod()
		{
			AssertEquals("Default ExpiredRateNotificationPeriod", 7, Registry.Rating.ExpiredRateNotificationPeriod);
			Registry.RawRegistry.ExpiredRateNotificationPeriod.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 5);
			AssertEquals("ExpiredRateNotificationPeriod", 5, Registry.Rating.ExpiredRateNotificationPeriod);
		}

		public void TestExpiringRateNotificationPeriod()
		{
			AssertEquals("Default ExpiringRateNotificationPeriod", 90, Registry.Rating.ExpiringRateNotificationPeriod);
			Registry.RawRegistry.ExpiringRateNotificationPeriod.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, 30);
			AssertEquals("ExpiringRateNotificationPeriod", 30, Registry.Rating.ExpiringRateNotificationPeriod);
		}

		public void TestDeleteRatesExpiredPeriod()
		{
			AssertEquals("Default PermanentlyDeleteRatesExpiredPeriod", 0, Registry.Rating.PermanentlyDeleteRatesExpiredPeriod.ExpiredRatesPeriodInYears);
			AssertEquals("Default PermanentlyDeleteRatesExpiredPeriod", 100, Registry.Rating.PermanentlyDeleteRatesExpiredPeriod.BatchSize);
			Registry.RawRegistry.PermanentlyDeleteRatesExpiredPeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DeleteExpiredRates { BatchSize = 5, ExpiredRatesPeriodInYears = 1 });
			AssertEquals("PermanentlyDeleteRatesExpiredPeriod", 5, Registry.Rating.PermanentlyDeleteRatesExpiredPeriod.BatchSize);
			AssertEquals("PermanentlyDeleteRatesExpiredPeriod", 1, Registry.Rating.PermanentlyDeleteRatesExpiredPeriod.ExpiredRatesPeriodInYears);
		}

		public void TestQuoteHeaderText()
		{
			Registry.RawRegistry.QuoteHeaderText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "abc");
			AssertEquals("QuoteHeaderText", "abc", Registry.Rating.QuoteHeaderText);
		}

		public void TestQuoteCoverPageFooterText()
		{
			Registry.RawRegistry.QuoteCoverPageFooterText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Zubin 123");
			AssertEquals("QuoteCoverPageFooterText", "Zubin 123", Registry.Rating.QuoteCoverPageFooterText);
		}

		public void TestFreightRatedCodes()
		{
			string[] codes = Registry.Rating.FreightRatedCodes;
			AssertEquals("Length", 6, codes.Length);
		}

		public void TestBrokerageRatedCodes()
		{
			string[] codes = Registry.Rating.BrokerageRatedCodes;
			AssertEquals("Length", 3, codes.Length);

			codes = Registry.Rating.OriginBrokerageRatedCodes;
			AssertEquals("Length", 2, codes.Length);
		}

		public void TestBuyersConsolApportionedCodes()
		{
			string[] codes = Registry.Rating.BuyersConsolApportionedCodes;
			AssertEquals("Length", 5, codes.Length);
			AssertCollectionNotContains("ORG", codes);
			AssertCollectionNotContains("BRK", codes);
			AssertCollectionNotContains("CFS", codes);
		}

		public void TestShippersConsolApportionedCodes()
		{
			string[] codes = Registry.Rating.ShippersConsolApportionedCodes;
			AssertEquals("Length", 5, codes.Length);
			AssertCollectionNotContains("BRK", codes);
			AssertCollectionNotContains("CFS", codes);
		}

		public void TestPublishedAgentsHeadingText()
		{
			AssertEquals("Default PublishedAgentsHeadingText", "Recommended Agents", Registry.Rating.PublishedAgentsHeadingText);
			Registry.RawRegistry.PublishedAgentsHeadingText.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "Overseas Offices");
			AssertEquals("PublishedAgentsHeadingText", "Overseas Offices", Registry.Rating.PublishedAgentsHeadingText);
		}

		public void TestIncludeCFXrOnQuote()
		{
			AssertEquals("Default IncludeCFXOnQuote", false, Registry.Rating.IncludeCFXOnQuote);
			Registry.RawRegistry.IncludeCFXOnQuotation.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("IncludeCFXOnQuote", true, Registry.Rating.IncludeCFXOnQuote);
		}

		public void TestMarkUpPercentages()
		{
			AssertEquals("Default MarkUpPercentages", "", Registry.Rating.MarkUpPercentages);
			Registry.RawRegistry.MarkUpPercentages.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "AUSYD|1.4;AUMEL|2");
			AssertEquals("MarkUpPercentages", "AUSYD|1.4;AUMEL|2", Registry.Rating.MarkUpPercentages);
		}

		public void TestMinimumMarkUpPercentages()
		{
			AssertEquals("Default MinimumMarkUpPercentages", "", Registry.Rating.MinimumMarkUpPercentages);
			Registry.RawRegistry.MinimumMarkUpPercentages.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, "AUSYD|1.4;AUMEL|2");
			AssertEquals("MinimumMarkUpPercentages", "AUSYD|1.4;AUMEL|2", Registry.Rating.MinimumMarkUpPercentages);
		}

		public void TestQuoteTermsAndConditionsPages()
		{
			EnvProxy.Instance.Registry.RawRegistry.QuoteTCPage1.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, new Bitmap(100, 100));
			AssertEquals(100, EnvProxy.Instance.Registry.Rating.GetQuoteTermsAndConditionsPage(0).Width);

			EnvProxy.Instance.Registry.RawRegistry.QuoteTCPage5.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, new Bitmap(101, 101));
			AssertEquals(101, EnvProxy.Instance.Registry.Rating.GetQuoteTermsAndConditionsPage(4).Width);

			EnvProxy.Instance.Registry.RawRegistry.QuoteOtherPage1.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, new Bitmap(102, 102));
			AssertEquals(102, EnvProxy.Instance.Registry.Rating.GetQuoteTermsAndConditionsPage(5).Width);

			EnvProxy.Instance.Registry.RawRegistry.QuoteOtherPage5.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, new Bitmap(103, 103));
			AssertEquals(103, EnvProxy.Instance.Registry.Rating.GetQuoteTermsAndConditionsPage(9).Width);
		}

		public void TestIncludeCFSFreeStorageDaysInCalculation()
		{
			AssertEquals("Default IncludeCFSFreeStorageDaysInCalculation", false, Registry.Rating.IncludeCFSFreeStorageDaysInCalculation);
			Registry.RawRegistry.IncludeCFSFreeStorageDaysInCalculation.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("IncludeCFSFreeStorageDaysInCalculation", true, Registry.Rating.IncludeCFSFreeStorageDaysInCalculation);
		}

		public void TestStorageCalculationPeriod()
		{
			AssertEquals("Default StorageCalculationPeriod", Constants.StorageCalculationPeriods.Weekly, Registry.Rating.StorageCalculationPeriod);
			Registry.RawRegistry.StorageCalculationPeriod.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.StorageCalculationPeriods.Daily);
			AssertEquals("StorageCalculationPeriod", Constants.StorageCalculationPeriods.Daily, Registry.Rating.StorageCalculationPeriod);
		}

		public void TestDefaultCTOPostCodeAir()
		{
			AssertEquals("Default value is empty", "", Registry.Rating.DefaultCTOAddressAir);
			Registry.RawRegistry.DefaultCTOPostCodeAir.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2000");
			AssertEquals("Value set to True", "2000", Registry.Rating.DefaultCTOAddressAir);
		}

		public void TestDefaultCTOAddressSea()
		{
			AssertEquals("Default value is empty", "", Registry.Rating.DefaultCTOAddressSea);
			Registry.RawRegistry.DefaultCTOPostCodeSea.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2000");
			AssertEquals("Value set to True", "2000", Registry.Rating.DefaultCTOAddressSea);
		}

		public void TestDefaultCTOAddressRail()
		{
			AssertEquals("Default value is empty", "", Registry.Rating.DefaultCTOAddressRail);
			Registry.RawRegistry.DefaultCTOPostCodeRail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2000");
			AssertEquals("Value set to True", "2000", Registry.Rating.DefaultCTOAddressRail);
		}

		public void TestDefaultCFSAddressRoad()
		{
			AssertEquals("Default value is empty", "", Registry.Rating.DefaultCFSAddressRoad);
			Registry.RawRegistry.DefaultCFSPostCodeRoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "2000");
			AssertEquals("Value set to True", "2000", Registry.Rating.DefaultCFSAddressRoad);
		}

		public void TestUseDistanceCalculationService()
		{
			AssertEquals("Default value is False", false, Registry.Rating.UseDistanceCalculationService);
			Registry.RawRegistry.UseDistanceCalculcationService.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("Value set to True", true, Registry.Rating.UseDistanceCalculationService);
		}

		public void TestTACTRateImportPartitionSize()
		{
			AssertEquals("Default value is 50000", 50000, Registry.Rating.TACTRateImportPartitionSize);
			Registry.RawRegistry.TACTRateImportPartitionSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 40000);
			AssertEquals("Value set to 40000", 40000, Registry.Rating.TACTRateImportPartitionSize);
		}

		#region Is Quote Terms and Conditions Pages Value Set

		public void TestIsQuoteTermsAndConditionsPagesValueSet()
		{
			AssertIsQuoteTermsAndConditionsPagesValueSet(Registry.RawRegistry.QuoteTCPage1, 0);
			AssertIsQuoteTermsAndConditionsPagesValueSet(Registry.RawRegistry.QuoteTCPage2, 1);
			AssertIsQuoteTermsAndConditionsPagesValueSet(Registry.RawRegistry.QuoteTCPage3, 2);
			AssertIsQuoteTermsAndConditionsPagesValueSet(Registry.RawRegistry.QuoteTCPage4, 3);
			AssertIsQuoteTermsAndConditionsPagesValueSet(Registry.RawRegistry.QuoteTCPage5, 4);
			AssertIsQuoteTermsAndConditionsPagesValueSet(Registry.RawRegistry.QuoteOtherPage1, 5);
			AssertIsQuoteTermsAndConditionsPagesValueSet(Registry.RawRegistry.QuoteOtherPage2, 6);
			AssertIsQuoteTermsAndConditionsPagesValueSet(Registry.RawRegistry.QuoteOtherPage3, 7);
			AssertIsQuoteTermsAndConditionsPagesValueSet(Registry.RawRegistry.QuoteOtherPage4, 8);
			AssertIsQuoteTermsAndConditionsPagesValueSet(Registry.RawRegistry.QuoteOtherPage5, 9);
			Assert(!Registry.Rating.IsQuoteTermsAndConditionsPagesValueSet(10));
		}

		void AssertIsQuoteTermsAndConditionsPagesValueSet(IRegistryItem item, int index)
		{
			AssertEquals(string.Format("IsQuoteTermsAndConditionsPagesValueSet({0})", index), false, Registry.Rating.IsQuoteTermsAndConditionsPagesValueSet(index));
			item.SetValue(Guid.Empty, EnvProxy.Instance.CurrentBranch.PK, Guid.Empty, null);
			AssertEquals(string.Format("IsQuoteTermsAndConditionsPagesValueSet({0})", index), true, Registry.Rating.IsQuoteTermsAndConditionsPagesValueSet(index));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Registry = new DataRegistry();
		}

		DataRegistry Registry;

		#endregion
	}
}

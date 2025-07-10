using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	sealed class DocRateEntryOutputTest : RatingTestCase
	{
		public void TestSEAFCLInheritance()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			InsertClientChargeCodesForAutoRaterTests(Factory);
			TestBAF.AC_AT_GSTRate = ZGuid.Empty;

			Factory.Save();

			Quote quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = NewClient.PK;
			RateEntry quoteEntry = quote.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "40RE");
			quoteEntry.RateLines.RemoveAndDeleteAll();

			RateLine quoteLine1 = quoteEntry.AddRateLine(TestBAF.AC_Code, UnitCalculator.Code, QuantityUnit.CN);
			quoteLine1.RateLineItems.RemoveAndDeleteAll();
			((UnitCalculator)quoteLine1.Calculator).PerUnit = 1300m;

			PricingPageCollection pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);
			AssertEquals(1, pages.Count);
			DocTableQuotation testTabeleQuotation = DocTableQuotation.New(pages[0], Factory);
			Factory.Save();

			AssertEquals(1, testTabeleQuotation.Entries.Count);
			AssertEquals("", testTabeleQuotation.Entries[0].Column4);
			AssertEquals("40RE", testTabeleQuotation.Entries[0].Header4);
			AssertEquals("Test BAF: USD 1300.00 per 40RE Container", testTabeleQuotation.Entries[0].OtherCharges);

			CompanyTariff tariff = Factory.NewWithValidTestData<CompanyTariff>();
			NewClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			RateEntry tariffEntry = tariff.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "40RE");
			tariffEntry.RateLines.RemoveAndDeleteAll();

			RateLine tariffLine1 = tariffEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			tariffLine1.RateLineItems.RemoveAndDeleteAll();
			((UnitCalculator)tariffLine1.Calculator).PerUnit = 1100m;

			pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);
			AssertEquals(1, pages.Count);
			testTabeleQuotation = DocTableQuotation.New(pages[0], Factory);

			Factory.ClearCachedValue<RelatedRateEntries>(quoteEntry.KeyForQuotationPricingPage());
			Factory.Save();

			AssertEquals(1, testTabeleQuotation.Entries.Count);
			AssertEquals("1100.00", testTabeleQuotation.Entries[0].Column4);
			AssertEquals("40RE", testTabeleQuotation.Entries[0].Header4);
			AssertEquals("Test BAF: USD 1300.00 per 40RE Container", testTabeleQuotation.Entries[0].OtherCharges);

			ClientRate rate = Factory.NewWithValidTestData<ClientRate>();
			rate.TH_OH = NewClient.PK;
			RateEntry rateEntry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "GBLON", "STD", "40RE");
			rateEntry.RateLines.RemoveAndDeleteAll();

			RateLine rateLine1 = rateEntry.AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.CN);
			rateLine1.RateLineItems.RemoveAndDeleteAll();
			((UnitCalculator)rateLine1.Calculator).PerUnit = 1200m;

			pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.LandscapeComplexStyle);
			AssertEquals(1, pages.Count);
			testTabeleQuotation = DocTableQuotation.New(pages[0], Factory);

			Factory.ClearCachedValue<RelatedRateEntries>(quoteEntry.KeyForQuotationPricingPage());
			Factory.Save();

			AssertEquals(1, testTabeleQuotation.Entries.Count);
			AssertEquals("1200.00", testTabeleQuotation.Entries[0].Column4);
			AssertEquals("40RE", testTabeleQuotation.Entries[0].Header4);
			AssertEquals("Test BAF: USD 1300.00 per 40RE Container", testTabeleQuotation.Entries[0].OtherCharges);
		}
	}
}

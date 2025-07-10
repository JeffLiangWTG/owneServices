using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class RateEntryExtensionsTest : RatingTestCase
	{
		public void TestRateLinesIncludingRelated()
		{
			InsertClientChargeCodesForAutoRaterTests(Factory);

			var costing = Factory.NewWithValidTestData<Costing>();
			costing.TH_QuoteNumber = "";
			costing.TH_OH = NewClient2.PK;

			var costEntry = costing.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			costEntry.TI_RateStartDate = ZDate.Today.AddMonths(-6);
			costEntry.RateLines.RemoveAndDeleteAll();

			var costLine1 = costEntry.AddRateLine(TestFRT.AC_Code, FlatCalculator.Code);
			costLine1.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)costLine1.Calculator).BaseRate = 10000m;

			var costLine2 = costEntry.AddRateLine(TestBAF.AC_Code, FlatCalculator.Code);
			costLine2.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)costLine2.Calculator).BaseRate = 9999m;

			var costLine3 = costEntry.AddRateLine(TestCAF.AC_Code, FlatCalculator.Code);
			costLine3.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)costLine3.Calculator).BaseRate = 9998m;

			var costLine4 = costEntry.AddRateLine(TestLOL.AC_Code, FlatCalculator.Code);
			costLine4.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)costLine4.Calculator).BaseRate = 9997m;

			var tariff = Factory.NewWithValidTestData<CompanyTariff>();
			tariff.TH_QuoteNumber = "";
			NewClient.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

			var tariffEntry = tariff.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			tariffEntry.TI_RateStartDate = ZDate.Today.AddMonths(-6);
			tariffEntry.RateLines.RemoveAndDeleteAll();

			var tariffLine1 = tariffEntry.AddRateLine(TestBAF.AC_Code, FlatCalculator.Code);
			tariffLine1.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)tariffLine1.Calculator).BaseRate = 1000m;

			var tariffLine2 = tariffEntry.AddRateLine(TestCAF.AC_Code, FlatCalculator.Code);
			tariffLine2.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)tariffLine2.Calculator).BaseRate = 999m;

			var tariffLine3 = tariffEntry.AddRateLine(TestLOL.AC_Code, FlatCalculator.Code);
			tariffLine3.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)tariffLine3.Calculator).BaseRate = 998m;

			var rate = Factory.NewWithValidTestData<ClientRate>();
			rate.TH_QuoteNumber = "";
			rate.TH_OH = NewClient.PK;

			var rateEntry = rate.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			rateEntry.TI_RateStartDate = ZDate.Today.AddMonths(-6);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine(TestCAF.AC_Code, FlatCalculator.Code);
			rateLine1.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)rateLine1.Calculator).BaseRate = 100m;

			var rateLine2 = rateEntry.AddRateLine(TestLOL.AC_Code, FlatCalculator.Code);
			rateLine2.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)rateLine2.Calculator).BaseRate = 99m;

			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OH = NewClient.PK;
			quote.TH_QuoteDate = ZDate.Today.AddMonths(-6);

			var quoteEntry = quote.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			quoteEntry.TI_RateStartDate = ZDate.Today.AddMonths(-6);
			quoteEntry.RateLines.RemoveAndDeleteAll();

			var quoteLine1 = quoteEntry.AddRateLine(TestLOL.AC_Code, FlatCalculator.Code);
			quoteLine1.RateLineItems.RemoveAndDeleteAll();
			((FlatCalculator)quoteLine1.Calculator).BaseRate = 10m;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { costLine1, costLine2, costLine3, costLine4 }, costEntry.RateLines);
			AssertContainsExactElementsInAnyOrder(new[] { costLine1, costLine2, costLine3, costLine4 }, costEntry.RateLinesIncludingRelated(new PricingPage(costEntry, costEntry.Factory, PricingPageStyle.Landscape)));

			AssertContainsExactElementsInAnyOrder(new[] { tariffLine1, tariffLine2, tariffLine3 }, tariffEntry.RateLines);
			AssertContainsExactElementsInAnyOrder(new[] { tariffLine1, tariffLine2, tariffLine3 }, tariffEntry.RateLinesIncludingRelated(new PricingPage(tariffEntry, tariffEntry.Factory, PricingPageStyle.Landscape)));

			AssertContainsExactElementsInAnyOrder(new[] { rateLine1, rateLine2 }, rateEntry.RateLines);
			AssertContainsExactElementsInAnyOrder(new[] { rateLine1.PK, rateLine2.PK, tariffLine1.PK }, rateEntry.RateLinesIncludingRelated(new PricingPage(rateEntry, rateEntry.Factory, PricingPageStyle.Landscape)).Select(line => line.PK));

			AssertContainsExactElementsInAnyOrder(new[] { quoteLine1 }, quoteEntry.RateLines);
			AssertContainsExactElementsInAnyOrder(new[] { quoteLine1.PK, rateLine1.PK, tariffLine1.PK }, quoteEntry.RateLinesIncludingRelated(new PricingPage(quoteEntry, quoteEntry.Factory, PricingPageStyle.Landscape)).Select(line => line.PK));
		}
	}
}

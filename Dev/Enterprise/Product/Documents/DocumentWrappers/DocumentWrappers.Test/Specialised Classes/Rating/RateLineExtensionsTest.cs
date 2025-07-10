using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class RateLineExtensionsTest : RatingTestCase
	{
		public void TestIsVisibleOnDocuments_ClientRate()
		{
			AssertIsVisibleOnDocuments(Helper.NewClientRate(Helper.NewOrgHeader()));
		}

		public void TestIsVisibleOnDocuments_Costing()
		{
			AssertIsVisibleOnDocuments(Helper.NewCosting(null));
		}

		public void TestIsVisibleOnDocuments_Quotation()
		{
			AssertIsVisibleOnDocuments(Helper.NewQuote(Helper.NewOrgHeader()));
		}

		public void IsHasValueForDocumentPrinting()
		{
			var testQuote = Helper.NewQuote(Helper.NewOrgHeader());
			var entry = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON");
			entry.RateLines.RemoveAndDeleteAll();

			var line1 = entry.AddRateLine("BAF", CombinedCalculator.Code, "M3");
			line1.GetCalculator<CombinedCalculator>().Minimum = 100m;
			line1.Calculator.AddRateLineItem("-", 45m, 5m, 2m);
			line1.Calculator.AddRateLineItem("+", 45m, 4m, 1m);

			var line2 = entry.AddRateLine("CAF", UnitCalculator.Code, "KG");
			line2.GetCalculator<UnitCalculator>().PerUnit = 0m;

			Assert(!line1.HasValueForDocumentPrinting());
			Assert(line2.HasValueForDocumentPrinting());
		}

		public void TestItemOrigin()
		{
			RefUNLOCO aubne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
			RefUNLOCO nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");
			RefUNLOCO nlams = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NLAMS");

			RefCountry australia = aubne.Country;
			RefCountry newzealand = nzakl.Country;
			RefCountry netherlands = nlams.Country;

			RefZoneHeader zone1 = Factory.New<RefZoneHeader>();
			zone1.FZ_Code = "ZNE1";
			zone1.FZ_Description = "Zone 1";
			zone1.Countries.Add(australia);
			zone1.Countries.Add(newzealand);

			RefZoneHeader zone2 = Factory.New<RefZoneHeader>();
			zone2.FZ_Code = "ZNE2";
			zone2.FZ_Description = "Zone 2";
			zone2.Countries.Add(netherlands);

			Quote quote = Factory.New<Quote>();

			RateEntry fclentry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AU", "NZ");
			fclentry.TI_ViaLRC = "NLAMS";

			RateEntry orgentry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AU", "NZ");
			orgentry.TI_ViaLRC = "NLAMS";

			RateEntry dstentry = quote.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "NZ", "NZ");
			dstentry.TI_ViaLRC = "NLAMS";

			RateLine nullline = null;
			RateLine fclline = fclentry.AddRateLine("FRT", FlatCalculator.Code);
			RateLine orgline = orgentry.AddRateLine("ODOC", FlatCalculator.Code);
			RateLine dstline = dstentry.AddRateLine("DDOC", FlatCalculator.Code);

			CombineAssertions(delegate
			{
				AssertEquals("null port", "Brisbane", nullline.ItemOrigin(aubne));
				AssertEquals("FCL port", "Brisbane", fclline.ItemOrigin(aubne));
				AssertEquals("ORG port", "Brisbane", orgline.ItemOrigin(aubne));
				AssertEquals("DST port", "Brisbane", dstline.ItemOrigin(aubne));

				AssertEquals("null country", "Australia", nullline.ItemOrigin(australia));
				AssertEquals("FCL country", "Australia", fclline.ItemOrigin(australia));
				AssertEquals("ORG country", "Australia", orgline.ItemOrigin(australia));
				AssertEquals("DST country", "Australia", dstline.ItemOrigin(australia));

				AssertEquals("null zone1", "Zone 1", nullline.ItemOrigin(zone1));
				AssertEquals("FCL zone1", "Zone 1", fclline.ItemOrigin(zone1));
				AssertEquals("ORG zone1", "Australia", orgline.ItemOrigin(zone1));
				AssertEquals("DST zone1", "Zone 1", dstline.ItemOrigin(zone1));

				AssertEquals("null zone2", "Zone 2", nullline.ItemOrigin(zone2));
				AssertEquals("FCL zone2", "Zone 2", fclline.ItemOrigin(zone2));
				AssertEquals("ORG zone2", "Zone 2", orgline.ItemOrigin(zone2));
				AssertEquals("DST zone2", "Zone 2", dstline.ItemOrigin(zone2));

				AssertEquals("null null", "", nullline.ItemOrigin(null));
				AssertEquals("FCL null", "", fclline.ItemOrigin(null));
				AssertEquals("ORG null", "", orgline.ItemOrigin(null));
				AssertEquals("DST null", "", dstline.ItemOrigin(null));
			});
		}

		public void TestItemDestination()
		{
			RefUNLOCO aubne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
			RefUNLOCO nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");
			RefUNLOCO nlams = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NLAMS");

			RefCountry australia = aubne.Country;
			RefCountry newzealand = nzakl.Country;
			RefCountry netherlands = nlams.Country;

			RefZoneHeader zone1 = Factory.New<RefZoneHeader>();
			zone1.FZ_Code = "ZNE1";
			zone1.FZ_Description = "Zone 1";
			zone1.Countries.Add(australia);
			zone1.Countries.Add(newzealand);

			RefZoneHeader zone2 = Factory.New<RefZoneHeader>();
			zone2.FZ_Code = "ZNE2";
			zone2.FZ_Description = "Zone 2";
			zone2.Countries.Add(netherlands);

			Quote quote = Factory.New<Quote>();

			RateEntry fclentry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AU", "NZ");
			fclentry.TI_ViaLRC = "NLAMS";

			RateEntry orgentry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AU", "NZ");
			orgentry.TI_ViaLRC = "NLAMS";

			RateEntry dstentry = quote.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "NZ", "NZ");
			dstentry.TI_ViaLRC = "NLAMS";

			RateLine nullline = null;
			RateLine fclline = fclentry.AddRateLine("FRT", FlatCalculator.Code);
			RateLine orgline = orgentry.AddRateLine("ODOC", FlatCalculator.Code);
			RateLine dstline = dstentry.AddRateLine("DDOC", FlatCalculator.Code);

			CombineAssertions(delegate
			{
				AssertEquals("null port", "Auckland", nullline.ItemDestination(nzakl));
				AssertEquals("FCL port", "Auckland", fclline.ItemDestination(nzakl));
				AssertEquals("ORG port", "Auckland", orgline.ItemDestination(nzakl));
				AssertEquals("DST port", "Auckland", dstline.ItemDestination(nzakl));

				AssertEquals("null country", "New Zealand", nullline.ItemDestination(newzealand));
				AssertEquals("FCL country", "New Zealand", fclline.ItemDestination(newzealand));
				AssertEquals("ORG country", "New Zealand", orgline.ItemDestination(newzealand));
				AssertEquals("DST country", "New Zealand", dstline.ItemDestination(newzealand));

				AssertEquals("null zone1", "Zone 1", nullline.ItemDestination(zone1));
				AssertEquals("FCL zone1", "Zone 1", fclline.ItemDestination(zone1));
				AssertEquals("ORG zone1", "Zone 1", orgline.ItemDestination(zone1));
				AssertEquals("DST zone1", "New Zealand", dstline.ItemDestination(zone1));

				AssertEquals("null zone2", "Zone 2", nullline.ItemDestination(zone2));
				AssertEquals("FCL zone2", "Zone 2", fclline.ItemDestination(zone2));
				AssertEquals("ORG zone2", "Zone 2", orgline.ItemDestination(zone2));
				AssertEquals("DST zone2", "Zone 2", dstline.ItemDestination(zone2));

				AssertEquals("null null", "", nullline.ItemDestination(null));
				AssertEquals("FCL null", "", fclline.ItemDestination(null));
				AssertEquals("ORG null", "", orgline.ItemDestination(null));
				AssertEquals("DST null", "", dstline.ItemDestination(null));
			});
		}

		public void TestItemVia()
		{
			RefUNLOCO aubne = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUBNE");
			RefUNLOCO nzakl = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NZAKL");
			RefUNLOCO nlams = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "NLAMS");

			RefCountry australia = aubne.Country;
			RefCountry newzealand = nzakl.Country;
			RefCountry netherlands = nlams.Country;

			RefZoneHeader zone1 = Factory.New<RefZoneHeader>();
			zone1.FZ_Code = "ZNE1";
			zone1.FZ_Description = "Zone 1";
			zone1.Countries.Add(australia);
			zone1.Countries.Add(newzealand);

			RefZoneHeader zone2 = Factory.New<RefZoneHeader>();
			zone2.FZ_Code = "ZNE2";
			zone2.FZ_Description = "Zone 2";
			zone2.Countries.Add(netherlands);

			Quote quote = Factory.New<Quote>();

			RateEntry fclentry = quote.AddRateEntry(RatingConstants.RateCategory.FCL, "ALL", "AU", "NZ");
			fclentry.TI_ViaLRC = "NLAMS";

			RateEntry orgentry = quote.AddRateEntry(RatingConstants.RateCategory.ORG, "ALL", "AU", "NZ");
			orgentry.TI_ViaLRC = "NLAMS";

			RateEntry dstentry = quote.AddRateEntry(RatingConstants.RateCategory.DST, "ALL", "NZ", "NZ");
			dstentry.TI_ViaLRC = "NLAMS";

			RateLine nullline = null;
			RateLine fclline = fclentry.AddRateLine("FRT", FlatCalculator.Code);
			RateLine orgline = orgentry.AddRateLine("ODOC", FlatCalculator.Code);
			RateLine dstline = dstentry.AddRateLine("DDOC", FlatCalculator.Code);

			CombineAssertions(delegate
			{
				AssertEquals("null port", "Amsterdam", nullline.ItemVia(nlams));
				AssertEquals("FCL port", "Amsterdam", fclline.ItemVia(nlams));
				AssertEquals("ORG port", "Amsterdam", orgline.ItemVia(nlams));
				AssertEquals("DST port", "Amsterdam", dstline.ItemVia(nlams));

				AssertEquals("null country", "Netherlands", nullline.ItemOrigin(netherlands));
				AssertEquals("FCL country", "Amsterdam", fclline.ItemVia(netherlands));
				AssertEquals("ORG country", "Amsterdam", orgline.ItemVia(netherlands));
				AssertEquals("DST country", "Amsterdam", dstline.ItemVia(netherlands));

				AssertEquals("null zone1", "Zone 1", nullline.ItemVia(zone1));
				AssertEquals("FCL zone1", "Zone 1", fclline.ItemVia(zone1));
				AssertEquals("ORG zone1", "Zone 1", orgline.ItemVia(zone1));
				AssertEquals("DST zone1", "Zone 1", dstline.ItemVia(zone1));

				AssertEquals("null zone2", "Zone 2", nullline.ItemOrigin(zone2));
				AssertEquals("FCL zone2", "Amsterdam", fclline.ItemVia(zone2));
				AssertEquals("ORG zone2", "Amsterdam", orgline.ItemVia(zone2));
				AssertEquals("DST zone2", "Amsterdam", dstline.ItemVia(zone2));

				AssertEquals("null null", "", nullline.ItemVia(null));
				AssertEquals("FCL null", "", fclline.ItemVia(null));
				AssertEquals("ORG null", "", orgline.ItemVia(null));
				AssertEquals("DST null", "", dstline.ItemVia(null));
			});
		}

		#region Implementation

		void AssertIsVisibleOnDocuments(RatingHeader header)
		{
			var bafChargeCode = Helper.ChargeCodes["BAF"];
			bafChargeCode.AC_ShowOnQuotation = true;
			bafChargeCode.AC_SuppressOnQuoteIfZero = false;

			var cafChargeCode = Helper.ChargeCodes["CAF"];
			cafChargeCode.AC_ShowOnQuotation = true;
			cafChargeCode.AC_SuppressOnQuoteIfZero = false;
			Factory.Save();

			var entry = header.AddRateEntry("AIR", "LSE", "AUSYD", "GBLON", "STD", "");
			entry.RateLines.RemoveAndDeleteAll();

			var line1 = entry.AddRateLine(bafChargeCode, CombinedCalculator.Code, "M3");
			var calculator1 = line1.GetCalculator<CombinedCalculator>();
			calculator1.Minimum = 100m;
			calculator1.AddRateLineItem("-", 45m, 5m, 2m);
			calculator1.AddRateLineItem("+", 45m, 4m, 1m);

			var line2 = entry.AddRateLine(cafChargeCode, UnitCalculator.Code, "KG");

			var line3 = entry.AddRateLine(cafChargeCode, CompanyTariffOrCostBasedCalculator.CostBasedCode);
			line3.RateLineItems.RemoveAndDeleteAll();
			line3.GetCalculator<CompanyTariffOrCostBasedCalculator>().PerUnit = 0m;

			Assert(line1.IsVisibleOnDocuments());
			Assert(line2.IsVisibleOnDocuments());
			Assert(line3.IsVisibleOnDocuments());

			Assert(line1.IsVisibleOnDocuments());
			Assert(line2.IsVisibleOnDocuments());
			Assert(line3.IsVisibleOnDocuments());

			bafChargeCode.AC_ShowOnQuotation = false;

			Assert(!line1.IsVisibleOnDocuments());
			Assert(line2.IsVisibleOnDocuments());
			Assert(line3.IsVisibleOnDocuments());

			cafChargeCode.AC_SuppressOnQuoteIfZero = true;

			Assert(!line1.IsVisibleOnDocuments());
			Assert(!line2.IsVisibleOnDocuments());
			Assert(line3.IsVisibleOnDocuments());

			bafChargeCode.AC_ShowOnQuotation = true;
			cafChargeCode.AC_SuppressOnQuoteIfZero = false;
		}

		#endregion
	}
}

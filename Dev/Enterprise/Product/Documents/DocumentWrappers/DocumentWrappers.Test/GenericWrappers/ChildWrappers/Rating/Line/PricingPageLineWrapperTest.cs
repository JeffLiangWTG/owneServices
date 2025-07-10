using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageLineWrapper))]
	sealed class PricingPageLineWrapperTest : GenericWrapperTest
	{
		public void TestCompanyTariff_Description()
		{
			var companyTariff1 = TestHelper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.FCL, "SEA", "", "", "BAF", 10m);
			var rateEntry1 = companyTariff1.AllEntries.Single();

			var companyTariff2 = TestHelper.NewNonLevel1CompanyTariff(level: 2, discountType: RatingConstants.RateCategory.FCL, discount: 10m);
			// companyTariff Level-2 Entry cannot be retrieved using AllEntries
			var rateEntry2 = companyTariff2.EntryCollectionsExcludingSummary[RatingConstants.RateCategory.FCL].LoadedCollection.Cast<RateEntry>().Single();

			var page = new PricingPage(rateEntry1, Factory, PricingPageStyle.Standard);
			var pricingPageLineWrapper = new PricingPageLineWrapper(page, QuotationLine.Header((RateLine)rateEntry2.RateLines.Single(), QuotationLineType.Mandatory | QuotationLineType.UseChargeDescription), 1, 2, Factory);
			AssertEquals("CompanyTariff Description", "Bunker Adjustment Factor", pricingPageLineWrapper.Description);
		}

		TestHelper TestHelper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;

		[TestDate(2010, 10, 21)]
		public override void TestWrapperMappingsEmpty()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry = tariff.AddRateEntry("FCL");
			RateLine line = entry.AddRateLine("FRT", FlatCalculator.Code);
			PricingPage page = new PricingPage(entry, Factory, PricingPageStyle.Standard);

			PricingPageLineWrapper wrapper = new PricingPageLineWrapper(page, QuotationLine.Header(line, QuotationLineType.Mandatory | QuotationLineType.UseChargeDescription), 1, 2, Factory);

			CombineAssertions(delegate
			{
				AssertEquals("SetIndexNum", 1, wrapper.SetIndexNum);
				AssertEquals("SetIndex", "00001", wrapper.SetIndex);
				AssertEquals("LineIndexNum", 2, wrapper.LineIndexNum);
				AssertEquals("LineIndex", "00002", wrapper.LineIndex);
				AssertEquals("Description", "International Freight", wrapper.Description);
				AssertEquals("Currency", "", wrapper.Currency);
				AssertEquals("Amount", "", wrapper.Amount);
				AssertEquals("Units", "", wrapper.Units);
				AssertEquals("Validity", "21/10/2010-21/04/2011", wrapper.Validity);
				AssertEquals("Origin", "", wrapper.Origin.Code);
				AssertEquals("Destination", "", wrapper.Destination.Code);
				AssertEquals("Via", "", wrapper.Via.Code);
			});
		}

		public void TestPopulated()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry = tariff.AddRateEntry("FCL", "ALL", "AU", "NL");
			RateLine line = entry.AddRateLine("FRT", UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line.TL_RateDesc = "Freight";
			((UnitCalculator)line.Calculator).PerUnit = 500m;

			PricingPage page = new PricingPage(entry, Factory, PricingPageStyle.Standard);
			QuotationLine qline = line.Calculator.GetQuotationLines(Calculator.GetQuotationLinesParam.None, entry)[0];

			PricingPageLineWrapper wrapper = new PricingPageLineWrapper(page, qline, 1, 2, Factory);

			CombineAssertions(delegate
			{
				AssertEquals("SetIndexNum", 1, wrapper.SetIndexNum);
				AssertEquals("SetIndex", "00001", wrapper.SetIndex);
				AssertEquals("LineIndexNum", 2, wrapper.LineIndexNum);
				AssertEquals("LineIndex", "00002", wrapper.LineIndex);
				AssertEquals("Description", "Freight", wrapper.Description);
				AssertEquals("Currency", "AUD", wrapper.Currency);
				AssertEquals("Ammount", "500.00", wrapper.Amount);
				AssertEquals("Units", "per Container", wrapper.Units);
			});
		}

		public void TestPorts()
		{
			RefZoneHeader euZone = Factory.New<RefZoneHeader>();
			euZone.FZ_Code = "EUZO";
			euZone.Countries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "NL"));
			euZone.Countries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB"));

			RefZoneHeader alZone = Factory.New<RefZoneHeader>();
			alZone.FZ_Code = "ALZO";
			alZone.Countries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU"));
			alZone.Countries.Add(Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "NZ"));

			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry parentEntry1 = tariff.AddRateEntry("FCL", "ALL", "AU", "NL");
			parentEntry1.TI_ViaLRC = "SGSIN";

			RateEntry parentEntry2 = tariff.AddRateEntry("FCL");

			RateEntry lineEntry1 = tariff.AddRateEntry("FCL", "ALL", "AUBNE", "NLAMS");
			lineEntry1.TI_ViaLRC = "SGSIN";

			RateEntry lineEntry2 = tariff.AddRateEntry("FCL", "ALL", alZone.Code, euZone.Code);
			lineEntry2.TI_ViaLRC = "SGSIN";

			RateEntry lineEntry3 = tariff.AddRateEntry("FCL", "ALL", "NL", "AU");
			lineEntry3.TI_ViaLRC = "SGSIN";

			RateLine line1 = lineEntry1.AddRateLine("FRT", UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line1.TL_RateDesc = "Freight";
			((UnitCalculator)line1.Calculator).PerUnit = 500m;

			RateLine line2 = lineEntry2.AddRateLine("FRT", UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line2.TL_RateDesc = "Freight";
			((UnitCalculator)line2.Calculator).PerUnit = 600m;

			RateLine line3 = lineEntry3.AddRateLine("FRT", UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line3.TL_RateDesc = "Freight";
			((UnitCalculator)line3.Calculator).PerUnit = 600m;

			RateLine line4 = parentEntry2.AddRateLine("FRT", UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line4.TL_RateDesc = "Freight";
			((UnitCalculator)line4.Calculator).PerUnit = 700;

			PricingPage page = new PricingPage(parentEntry1, Factory, PricingPageStyle.Standard);
			page.AddRateEntry(parentEntry2);

			CombineAssertions(delegate
			{
				QuotationLine qline = line1.Calculator.GetQuotationLines(Calculator.GetQuotationLinesParam.None, parentEntry1)[0];
				PricingPageLineWrapper wrapper = new PricingPageLineWrapper(page, qline, 1, 2, Factory);

				AssertEquals("Origin", "AUBNE", wrapper.Origin.Code);
				AssertEquals("Destination", "NLAMS", wrapper.Destination.Code);
				AssertEquals("Via", "SGSIN", wrapper.Via.Code);
			});

			CombineAssertions(delegate
			{
				QuotationLine qline = line2.Calculator.GetQuotationLines(Calculator.GetQuotationLinesParam.None, parentEntry1)[0];
				PricingPageLineWrapper wrapper = new PricingPageLineWrapper(page, qline, 1, 2, Factory);

				AssertEquals("Origin", "AU", wrapper.Origin.Code);
				AssertEquals("Destination", "NL", wrapper.Destination.Code);
				AssertEquals("Via", "SGSIN", wrapper.Via.Code);
			});

			CombineAssertions(delegate
			{
				QuotationLine qline = line3.Calculator.GetQuotationLines(Calculator.GetQuotationLinesParam.None, parentEntry1)[0];
				PricingPageLineWrapper wrapper = new PricingPageLineWrapper(page, qline, 1, 2, Factory);

				AssertEquals("Origin", "NL", wrapper.Origin.Code);
				AssertEquals("Destination", "AU", wrapper.Destination.Code);
				AssertEquals("Via", "SGSIN", wrapper.Via.Code);
			});
		}

		public void TestIsGSTApplicable()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry fclentry = tariff.AddRateEntry("FCL", "ALL", "AU", "NL");
			RateLine fclline = fclentry.AddRateLine("FRT", UnitCalculator.Code, RatingConstants.Units.CN, "AUD"); // no GST on FRT
			fclline.TL_RateDesc = "Freight";
			((UnitCalculator)fclline.Calculator).PerUnit = 500m;

			RateEntry orgentry = tariff.AddRateEntry("ORG", "ALL", "AU", "NL");
			RateLine orgline = fclentry.AddRateLine("ODOC", UnitCalculator.Code, RatingConstants.Units.CN, "AUD"); // GST on ODOC
			orgline.TL_RateDesc = "Freight";
			((UnitCalculator)orgline.Calculator).PerUnit = 500m;

			PricingPage page = new PricingPage(fclentry, Factory, PricingPageStyle.Landscape);

			QuotationLine fclqline = fclline.Calculator.GetQuotationLines(Calculator.GetQuotationLinesParam.None, fclentry)[0];
			QuotationLine orgqline = orgline.Calculator.GetQuotationLines(Calculator.GetQuotationLinesParam.None, orgentry)[0];

			PricingPageLineWrapper fclwrapper = new PricingPageLineWrapper(page, fclqline, 1, 2, Factory);
			PricingPageLineWrapper orgwrapper = new PricingPageLineWrapper(page, orgqline, 1, 2, Factory);

			CombineAssertions(delegate
			{
				AssertEquals("FCL", false, fclwrapper.IsGSTApplicable);
				AssertEquals("ORG", true, orgwrapper.IsGSTApplicable);
			});
		}

		public void TestClosingTaxText()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var tariff = Factory.New<CompanyTariff>();
				var orgentry = tariff.AddRateEntry("ORG", "ALL", "AU", "NL");
				var orgline = orgentry.AddRateLine("ODOC", UnitCalculator.Code, RatingConstants.Units.CN, "AUD"); // GST on ODOC
				orgline.GetCalculator<UnitCalculator>().PerUnit = 500m;

				var page = new PricingPage(orgentry, Factory, PricingPageStyle.Landscape);
				var orgqline = orgline.Calculator.GetQuotationLines(Calculator.GetQuotationLinesParam.None, orgentry)[0];

				var orgwrapper = new PricingPageLineWrapper(page, orgqline, 1, 2, Factory);

				CombineAssertions(delegate
				{
					AssertEquals("ORG", true, orgwrapper.IsGSTApplicable);
					AssertEquals("ORG", "A local Value Added Tax charge (equivalent to GST) may apply to all items marked with an asterisk (*).", orgwrapper.ClosingTaxText);
				});
			}
		}

		public void TestIncoTerm()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry entry1 = tariff.AddRateEntry("FCL", "ALL", "AU", "NL", "", "20GP");
			entry1.TI_QuotePageIncoTerm = "";

			RateLine line1 = entry1.AddRateLine("FRT", UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line1.TL_RateDesc = "Freight";
			((UnitCalculator)line1.Calculator).PerUnit = 500m;

			RateEntry entry2 = tariff.AddRateEntry("FCL", "ALL", "AU", "US", "", "20RE");
			entry2.TI_QuotePageIncoTerm = "";

			RateLine line2 = entry2.AddRateLine("FRT", UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line2.TL_RateDesc = "Freight";
			((UnitCalculator)line2.Calculator).PerUnit = 500m;

			PricingPage page = new PricingPage(entry1, Factory, PricingPageStyle.Landscape);
			page.AddRateEntry(entry2);

			QuotationLine qline1 = line1.Calculator.GetQuotationLines(Calculator.GetQuotationLinesParam.None, entry1)[0];

			CombineAssertions(delegate
			{
				var wrapper = new PricingPageLineWrapper(page, qline1, 1, 2, Factory);
				AssertEquals("All empty", "", wrapper.IncoTerm.Code);

				entry1.TI_QuotePageIncoTerm = "EXW";
				wrapper = new PricingPageLineWrapper(page, qline1, 1, 2, Factory);
				AssertEquals("One set", "EXW", wrapper.IncoTerm.Code);

				entry2.TI_QuotePageIncoTerm = "EXW";
				wrapper = new PricingPageLineWrapper(page, qline1, 1, 2, Factory);
				AssertEquals("Two set in agreement", "EXW", wrapper.IncoTerm.Code);

				entry2.TI_QuotePageIncoTerm = "DDP";
				wrapper = new PricingPageLineWrapper(page, qline1, 1, 2, Factory);
				AssertEquals("Two set in conflict", "", wrapper.IncoTerm.Code);
			});

			entry1.TI_QuotePageIncoTerm = "";
			CombineAssertions("GIVEN FC? WHEN printing THEN should show 'Free Carrier'", () =>
			{
				var wrapper = new PricingPageLineWrapper(page, qline1, 1, 2, Factory);

				entry2.TI_QuotePageIncoTerm = "FCA";
				wrapper = new PricingPageLineWrapper(page, qline1, 1, 2, Factory);
				AssertEquals("FC terms FCA", "Free Carrier", wrapper.IncoTerm.Code);

				entry2.TI_QuotePageIncoTerm = "FC1";
				wrapper = new PricingPageLineWrapper(page, qline1, 1, 2, Factory);
				AssertEquals("FC terms FC1", "Free Carrier", wrapper.IncoTerm.Code);

				entry2.TI_QuotePageIncoTerm = "FC2";
				wrapper = new PricingPageLineWrapper(page, qline1, 1, 2, Factory);
				AssertEquals("FC terms FC2", "Free Carrier", wrapper.IncoTerm.Code);
			});
		}

		public void TestParentEntry()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry parentEntry = tariff.AddRateEntry("FCL", "ALL", "AU", "NL");
			RateEntry lineEntry = tariff.AddRateEntry("FCL", "ALL", "AUBNE", "NLAMS");

			RateLine line = lineEntry.AddRateLine("FRT", UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line.TL_RateDesc = "Freight";
			((UnitCalculator)line.Calculator).PerUnit = 500m;

			PricingPage page = new PricingPage(parentEntry, Factory, PricingPageStyle.Standard);

			QuotationLine qline = line.Calculator.GetQuotationLines(Calculator.GetQuotationLinesParam.None, parentEntry)[0];
			PricingPageLineWrapper wrapper = new PricingPageLineWrapper(page, qline, 1, 2, Factory);

			AssertEquals("Parent Entry", lineEntry.PK, wrapper.ParentEntry.WrappedObjectPK);
		}

		public void TestValidity()
		{
			var today = ZDate.Today;

			var tariff = Factory.New<CompanyTariff>();

			var entry = tariff.AddRateEntry("FCL", "ALL", "AU", "NL");
			entry.TI_RateStartDate = today;
			entry.TI_RateEndDate = today.AddDays(5);

			var line = entry.AddRateLine("FRT", MinimumOrPerUnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line.TL_RateDesc = "Freight";
			((MinimumOrPerUnitCalculator)line.Calculator).PerUnit = 100m;
			((MinimumOrPerUnitCalculator)line.Calculator).Decimal1 = 500m;

			var page = new PricingPage(entry, Factory, PricingPageStyle.Standard);

			var builder = new StringBuilder();
			builder.AppendLine();

			foreach (QuotationLine qline in line.Calculator.GetQuotationLines(Calculator.GetQuotationLinesParam.None, entry))
			{
				var wrapper = new PricingPageLineWrapper(page, qline, 1, 2, Factory);
				builder.Append(wrapper.Description);
				builder.Append('|');
				builder.AppendLine(wrapper.Validity);
			}

			const string expected = @"
Freight|{0:d}-{1:d}
Minimum|
Per Unit|
";

			AssertMultilineASCIIEquals("Validity", string.Format(expected, today, today.AddDays(5)), builder.ToString());
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Destination : 
IncoTerm : 
Origin : 
ParentEntry : (No Default Field Value Available on Rate Entry)
Registry : (No Default Field Value Available on Registry)
Via :
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry = tariff.AddRateEntry("FCL");
			RateLine line = entry.AddRateLine("FRT", FlatCalculator.Code);
			line.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;

			PricingPage page = new PricingPage(entry, Factory, PricingPageStyle.Standard);

			return new PricingPageLineWrapper(page, QuotationLine.Header(line, QuotationLineType.Mandatory | QuotationLineType.UseChargeDescription), 1, 2, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Pricing Page Line
======================================================================
Name                                    Type
----------------------------------------------------------------------
IncoTerm                                CodeAndDescription
ParentEntry                             Rate Entry
Destination                             Rating Area
Origin                                  Rating Area
Via                                     Rating Area
Amount                                  MultilingualString
ClosingTaxText                          String
Currency                                String
DecimalPlaces                           Int
Description                             String
IsGSTApplicable                         Bool
LineIndex                               String
LineIndexNum                            Int
NumOfIndents                            Int
SetIndex                                String
SetIndexNum                             Int
Units                                   MultilingualString
Validity                                String
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry = tariff.AddRateEntry("FCL");
			RateLine line = entry.AddRateLine("FRT", FlatCalculator.Code);
			line.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;

			PricingPage page = new PricingPage(entry, Factory, PricingPageStyle.Standard);

			return new PricingPageLineWrapper(page, QuotationLine.Header(line, QuotationLineType.Mandatory | QuotationLineType.UseChargeDescription), 1, 2, Factory);
		}

		#endregion

		#region override
		protected override void SetUp()
		{
			base.SetUp();

			var gst = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST").AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Australia));
			if (gst != null)
			{
				gst.SetRate_ForTestOnly(10, 1);

				Factory.Save();
			}
		}

		#endregion
	}
}

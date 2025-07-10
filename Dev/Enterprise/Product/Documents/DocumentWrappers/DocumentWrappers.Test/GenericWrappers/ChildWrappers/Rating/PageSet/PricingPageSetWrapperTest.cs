using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Rating.Business.RatingConstants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PricingPageSetWrapper))]
	sealed class PricingPageSetWrapperTest : GenericWrapperTest
	{
		public void TestOriginDestinationAndFreightRates_RollUpSortRates_QuotationDocumentsChargeGroupingSequencingAndRollupRegistry_Enable()
			=> TestOriginDestinationAndFreightRates_RollUpSortRates
			(
				enableQuotationDocumentsChargeGroupingSequencingAndRollupRegistry: true,
				expectedOriginDestinationAndFreightRates: @"
[0,1-FreightRates,-0]
  [Line 0,0] All charges except Customs Duty and Tax|AUD|3.00|
"
			);

		public void TestOriginDestinationAndFreightRates_RollUpSortRates_QuotationDocumentsChargeGroupingSequencingAndRollupRegistry_Disable()
			=> TestOriginDestinationAndFreightRates_RollUpSortRates
			(
				enableQuotationDocumentsChargeGroupingSequencingAndRollupRegistry: false,
				expectedOriginDestinationAndFreightRates: @"
[0,2-FreightRates,-0]
  [Line 1,1] Bunker Adjustment Factor|AUD|1.00|
[0,2-FreightRates,-0]
  [Line 2,1] International Freight|AUD|2.00|
"
			);

		void TestOriginDestinationAndFreightRates_RollUpSortRates(bool enableQuotationDocumentsChargeGroupingSequencingAndRollupRegistry, string expectedOriginDestinationAndFreightRates)
		{
			var client = TestHelper.NewOrgHeader(1);
			QuotationRunDocForwardingStandardPricingPageRollUpSortTest.SetupOrganisation(client, DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Display, DocRollupOrSortDisplayList.Codes.RollUpCharges);
			QuotationRunDocForwardingStandardPricingPageRollUpSortTest.SetupOrganisation(client, DocRollupOrSortJobTypeList.Codes.Forwarding, RatingDocumentsChargeGroupingOrRollup.Schema.RCG_Style, DocRollupOrSortStyleList.Codes.All);

			Factory.Save();

			var quote = TestHelper.NewQuote(client);
			var rateEntry = quote.AddRateEntry(RateCategory.AIR, Core.Constants.RateMode.LSE, "AUSYD", "USLAX");
			rateEntry.AddFlatRateLine("BAF", 1m);
			rateEntry.AddFlatRateLine("FRT", 2m);

			Factory.Save();

			var wrapper = new PricingPageSetWrapperCollection(quote, "Pricing Page", Factory)["ForwardingStandard"];

			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableQuotationDocumentsChargeGroupingSequencingAndRollupRegistry))
			{
				AssertMultilineASCIIEquals("OriginDestinationAndFreightRates", expectedOriginDestinationAndFreightRates, Render(wrapper.OriginDestinationAndFreightRates));
			}
		}

		#region Local/Multilinqual Description - Quote with Related Company Tariff

		public void TestMultilingualDescription_QuoteWithRelatedCompanyTariff()
		{
			var chargeCode = TestHelper.ChargeCodes["FRT"];
			chargeCode.AC_LocalLanguageDescription = "International Freight Local Description 地方";

			TestHelper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.ORG, "ALL", "AUSYD", "USLAX", "FRT", 10m);

			var nonLocalClient = TestHelper.NewOrgHeader(companyTariffDefault: 1);
			var quote = TestHelper.NewQuote(nonLocalClient);
			quote.TH_PrintInheritedOriginCharges = true;
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "USLAX", "FRT", 20m, currency: "AUD");

			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var mockRes = Res.UseMockData())
			{
				TestHelper.SetChargeCodeMultilingualDescription(chargeCode, mockRes, "Mein Testgebührencode");

				var wrapper = new PricingPageSetWrapperCollection(quote, "Pricing Page", Factory)["ForwardingStandard"];
				var actual = Render(wrapper.OriginDestinationAndFreightRates);
				AssertMultilineASCIIEquals
				(
					"WHEN printing Quote with related Company-Tariff THEN should show multilinqual-description",
					expected: @"
[0,1-OriginRates,-0]
  [Line 1,1] Mein Testgebührencode|AUD|10.00|
[0,2-FreightRates,-0]
  [Line 1,1] Mein Testgebührencode|AUD|20.00|",
					actual
				);
			}
		}

		public void TestLocalDescription_QuoteWithRelatedCompanyTariff_NonLocalClient()
		{
			TestHelper.ChargeCodes["FRT"].AC_LocalLanguageDescription = "International Freight Local Description 地方";

			TestHelper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.ORG, "ALL", "AUSYD", "USLAX", "FRT", 10m);

			var nonLocalClient = TestHelper.NewOrgHeader(companyTariffDefault: 1);
			var quote = TestHelper.NewQuote(nonLocalClient);
			quote.TH_PrintInheritedOriginCharges = true;
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "USLAX", "FRT", 20m, currency: "AUD");

			AssertLocalDescription_QuoteWithRelatedCompanyTariff
			(
				"GIVEN non-local-client WHEN printing Quote with related Company-Tariff THEN should show non-local-description",
				quote,
				expected: @"
[0,1-OriginRates,-0]
  [Line 1,1] International Freight|AUD|10.00|
[0,2-FreightRates,-0]
  [Line 1,1] International Freight|AUD|20.00|");
		}

		public void TestLocalDescription_QuoteWithRelatedCompanyTariff_LocalClient()
		{
			TestHelper.ChargeCodes["FRT"].AC_LocalLanguageDescription = "International Freight Local Description 地方";

			TestHelper.NewLevel1CompanyTariffWithSingleRateLine(RatingConstants.RateCategory.ORG, "ALL", "AUSYD", "USLAX", "FRT", 10m);

			var localClient = TestHelper.NewOrgHeader(companyTariffDefault: 1);
			localClient.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;

			var quote = TestHelper.NewQuote(localClient);
			quote.TH_PrintInheritedOriginCharges = true;
			quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, "SEA", "AUSYD", "USLAX", "FRT", 20m, currency: "AUD");

			AssertLocalDescription_QuoteWithRelatedCompanyTariff
			(
				"GIVEN local-client WHEN printing Quote with related Company-Tariff THEN should show local-description",
				quote,
				expected: @"
[0,1-OriginRates,-0]
  [Line 1,1] International Freight Local Description 地方|AUD|10.00|
[0,2-FreightRates,-0]
  [Line 1,1] International Freight Local Description 地方|AUD|20.00|");
		}

		void AssertLocalDescription_QuoteWithRelatedCompanyTariff(string message, Quote quote, string expected)
		{
			var enableLocalChargeCodeDescriptionDefault = (BooleanRegistryItem)TestHelper.FindRegistryItemByName("RegistryItemSet_AccountingConfigurationRegistry", "ENABLE_LOCAL_CHARGE_CODE_DESCRIPTION_DEFAULT");
			using (enableLocalChargeCodeDescriptionDefault.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var wrapper = new PricingPageSetWrapperCollection(quote, "Pricing Page", Factory)["ForwardingStandard"];
				var actual = Render(wrapper.OriginDestinationAndFreightRates);
				AssertMultilineASCIIEquals(message, expected, actual);
			}
		}

		TestHelper TestHelper => testHelper ?? (testHelper = new TestHelper(Factory));
		TestHelper testHelper;

		#endregion

		public override void TestWrapperMappingsEmpty()
		{
			var tariff = Factory.New<CompanyTariff>();
			var wrapper = new PricingPageSetWrapper(tariff, PricingPaginationStrategy.ShippingCategoryFilter | PricingPaginationStrategy.StandardStyle, Factory);

			AssertEquals("no pricing pages", 0, wrapper.PricingPages.Count);
		}

		// Test for CS00678565 incident fix
		public void TestOriginDestinationAndFreightRates_NewlyCopiedQuoteWithFreightCharges_ShouldIncludeFreightCharges()
		{
			var quote = Factory.New<Quote>();

			AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "UAIEV", "AUSYD", "20GP", "FRT", 13m, Factory);
			AddUnitCN(quote, RatingConstants.RateCategory.FCL, "SEA", "UAIEV", "AUSYD", "20RE", "FRT", 66m, Factory);

			Factory.Save();

			var copiedQuote = quote.CopyIncludingChildren();
			copiedQuote.SelectedFilterCategory = RatingConstants.RateCategory.AIR;  // Imitate the first tab selected which is AIR rates

			Factory.Save();

			var wrapper = new PricingPageSetWrapperCollection(copiedQuote, "Pricing Page", Factory)["ForwardingStandard"];

			const string expected = @"
[0,2-FreightRates,-0]
  [Line 1,1] FRT charge|AUD|13.00|per 20GP Container
[0,2-FreightRates,-0]
  [Line 1,2] FRT charge|AUD|66.00|per 20RE Container
";
			AssertMultilineASCIIEquals("", expected, Render(wrapper.OriginDestinationAndFreightRates));
		}

		public void TestOriginDestinationAndFreightRates()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ShippingStandard"];

			const string expected = @"
[0,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[0,2-FreightRates,-0]
  [Line 1,1] FRT charge|||
[0,2-FreightRates,-0]
  [Line 1,2] Minimum|AUD|200.00|
[0,2-FreightRates,-0]
  [Line 1,3] Per Unit|AUD|111.00|per M3 / 1000 KG
[0,2-FreightRates,-0]
  [Line 2,1] BAF charge||2.50|% of freight charges
[0,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[1,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[1,2-FreightRates,-0]
  [Line 1,1] FRT charge|||
[1,2-FreightRates,-0]
  [Line 1,2] Minimum|AUD|200.00|
[1,2-FreightRates,-0]
  [Line 1,3] Per Unit|AUD|111.00|per M3 / 1000 KG
[1,2-FreightRates,-0]
  [Line 2,1] BAF charge||2.50|% of freight charges
[1,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[2,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[2,2-FreightRates,-0]
  [Line 1,1] FRT charge|||
[2,2-FreightRates,-0]
  [Line 1,2] Minimum|AUD|200.00|
[2,2-FreightRates,-0]
  [Line 1,3] Per Unit|AUD|111.00|per M3 / 1000 KG
[2,2-FreightRates,-0]
  [Line 2,1] BAF charge||2.50|% of freight charges
[2,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[3,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[3,2-FreightRates,-0]
  [Line 1,1] FRT charge|||
[3,2-FreightRates,-0]
  [Line 1,2] Minimum|AUD|200.00|
[3,2-FreightRates,-0]
  [Line 1,3] Per Unit|AUD|111.00|per M3 / 1000 KG
[3,2-FreightRates,-0]
  [Line 2,1] BAF charge||2.50|% of freight charges
[3,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[4,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[4,2-FreightRates,-0]
  [Line 1,1] FRT charge|AUD|101.00|per 20GP Container
[4,2-FreightRates,-0]
  [Line 1,2] FRT charge|AUD|102.00|per 20RE Container
[4,2-FreightRates,-0]
  [Line 1,3] FRT charge|AUD|103.00|per 40GP Container
[4,2-FreightRates,-0]
  [Line 1,4] FRT charge|AUD|104.00|per 40RE Container
[4,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[5,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[5,2-FreightRates,-0]
  [Line 1,1] FRT charge|AUD|101.00|per 20GP Container
[5,2-FreightRates,-0]
  [Line 1,2] FRT charge|AUD|102.00|per 20RE Container
[5,2-FreightRates,-0]
  [Line 1,3] FRT charge|AUD|103.00|per 40GP Container
[5,2-FreightRates,-0]
  [Line 1,4] FRT charge|AUD|104.00|per 40RE Container
[5,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[6,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[6,2-FreightRates,-0]
  [Line 1,1] FRT charge|AUD|101.00|per 20GP Container
[6,2-FreightRates,-0]
  [Line 1,2] FRT charge|AUD|102.00|per 20RE Container
[6,2-FreightRates,-0]
  [Line 1,3] FRT charge|AUD|103.00|per 40GP Container
[6,2-FreightRates,-0]
  [Line 1,4] FRT charge|AUD|104.00|per 40RE Container
[6,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[7,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[7,2-FreightRates,-0]
  [Line 1,1] FRT charge|AUD|101.00|per 20GP Container
[7,2-FreightRates,-0]
  [Line 1,2] FRT charge|AUD|102.00|per 20RE Container
[7,2-FreightRates,-0]
  [Line 1,3] FRT charge|AUD|103.00|per 40GP Container
[7,2-FreightRates,-0]
  [Line 1,4] FRT charge|AUD|104.00|per 40RE Container
[7,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper.OriginDestinationAndFreightRates));
		}

		public void TestOriginAndDestinationRates()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ShippingStandard"];

			const string expected = @"
[0,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[0,2-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[1,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[1,2-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[2,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[2,2-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[3,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[3,2-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[4,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[4,2-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[5,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[5,2-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[6,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[6,2-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
[7,1-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|11.00|
[7,2-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|12.00|
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper.OriginAndDestinationRates));
		}

		public void TestOriginDestinationAndContainerTableRows()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ForwardingLandscapeComplex"];

			const string expected = @"
[0,1-TableRows,SubRow-1]
  [Row 0, AU => NL]
  [SubRow 0, AUD, FRT]
[0,1-TableRows,RowBody-2]
  [Row 0, AU => NL]
[0,1-TableRows,SubRow-1]
  [Row 1, AU => SG]
  [SubRow 0, AUD, FRT]
[0,1-TableRows,RowBody-2]
  [Row 1, AU => SG]
[0,2-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|21.00|
[0,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|22.00|
[0,3-DestinationRates,-0]
  [Line 2,1] DDOC charge *|AUD|22.00|
[1,1-TableRows,SubRow-1]
  [Row 0, AU => NL]
  [SubRow 0, AUD, FRT]
[1,1-TableRows,RowBody-2]
  [Row 0, AU => NL]
[1,1-TableRows,SubRow-1]
  [Row 1, AU => SG]
  [SubRow 0, AUD, FRT]
[1,1-TableRows,RowBody-2]
  [Row 1, AU => SG]
[1,2-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|21.00|
[1,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|22.00|
[1,3-DestinationRates,-0]
  [Line 2,1] DDOC charge *|AUD|22.00|
[2,1-TableRows,SubRow-1]
  [Row 0, NL => AU]
  [SubRow 0, AUD, FRT]
[2,1-TableRows,RowBody-2]
  [Row 0, NL => AU]
[2,1-TableRows,SubRow-1]
  [Row 1, SG => AU]
  [SubRow 0, AUD, FRT]
[2,1-TableRows,RowBody-2]
  [Row 1, SG => AU]
[2,2-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|21.00|
[2,2-OriginRates,-0]
  [Line 2,1] ODOC charge *|AUD|21.00|
[2,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|22.00|
[3,1-TableRows,SubRow-1]
  [Row 0, NL => AU]
  [SubRow 0, AUD, FRT]
[3,1-TableRows,RowBody-2]
  [Row 0, NL => AU]
[3,1-TableRows,SubRow-1]
  [Row 1, SG => AU]
  [SubRow 0, AUD, FRT]
[3,1-TableRows,RowBody-2]
  [Row 1, SG => AU]
[3,2-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|21.00|
[3,2-OriginRates,-0]
  [Line 2,1] ODOC charge *|AUD|21.00|
[3,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|22.00|
[4,1-TableRows,SubRow-1]
  [Row 0, AU => NL]
  [SubRow 0, AUD, FRT]
[4,1-TableRows,RowBody-2]
  [Row 0, AU => NL]
[4,1-TableRows,OtherCharges-3]
  [Row 0, AU => NL]
  [Line 1,1] BAF charge||2.50|% of freight charges
[4,1-TableRows,SubRow-1]
  [Row 1, AU => SG]
  [SubRow 0, AUD, FRT]
[4,1-TableRows,RowBody-2]
  [Row 1, AU => SG]
[4,1-TableRows,OtherCharges-3]
  [Row 1, AU => SG]
  [Line 1,1] BAF charge||2.50|% of freight charges
[4,2-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|21.00|
[4,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|22.00|
[4,3-DestinationRates,-0]
  [Line 2,1] DDOC charge *|AUD|22.00|
[5,1-TableRows,SubRow-1]
  [Row 0, NL => AU]
  [SubRow 0, AUD, FRT]
[5,1-TableRows,RowBody-2]
  [Row 0, NL => AU]
[5,1-TableRows,OtherCharges-3]
  [Row 0, NL => AU]
  [Line 1,1] BAF charge||2.50|% of freight charges
[5,1-TableRows,SubRow-1]
  [Row 1, SG => AU]
  [SubRow 0, AUD, FRT]
[5,1-TableRows,RowBody-2]
  [Row 1, SG => AU]
[5,1-TableRows,OtherCharges-3]
  [Row 1, SG => AU]
  [Line 1,1] BAF charge||2.50|% of freight charges
[5,2-OriginRates,-0]
  [Line 1,1] ODOC charge *|AUD|21.00|
[5,2-OriginRates,-0]
  [Line 2,1] ODOC charge *|AUD|21.00|
[5,3-DestinationRates,-0]
  [Line 1,1] DDOC charge *|AUD|22.00|
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper.OriginDestinationAndContainerTableRows));
		}

		public void TestPopulate_ShippingStandard()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ShippingStandard"];

			const string expected = @"
[PricingPage]
Entry:
  Australia -> Netherlands: FCL
Origin:
  ODOC charge *|AUD|11.00||Australia -> Netherlands
Freight:
  FRT charge|AUD|101.00|per 20GP Container|Australia -> Netherlands
  FRT charge|AUD|102.00|per 20RE Container|Australia -> Netherlands
  FRT charge|AUD|103.00|per 40GP Container|Australia -> Netherlands
  FRT charge|AUD|104.00|per 40RE Container|Australia -> Netherlands
Destination:
  DDOC charge *|AUD|12.00||Australia -> Netherlands

[PricingPage]
Entry:
  Australia -> Netherlands: LCL
Origin:
  ODOC charge *|AUD|11.00||Australia -> Netherlands
Freight:
  BAF charge||2.50|% of freight charges|Australia -> Netherlands
  FRT charge||||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Netherlands
  Per Unit|AUD|111.00|per M3 / 1000 KG|Australia -> Netherlands
Destination:
  DDOC charge *|AUD|12.00||Australia -> Netherlands

[PricingPage]
Entry:
  Australia -> Singapore: FCL
Origin:
  ODOC charge *|AUD|11.00||Australia -> Singapore
Freight:
  FRT charge|AUD|101.00|per 20GP Container|Australia -> Singapore
  FRT charge|AUD|102.00|per 20RE Container|Australia -> Singapore
  FRT charge|AUD|103.00|per 40GP Container|Australia -> Singapore
  FRT charge|AUD|104.00|per 40RE Container|Australia -> Singapore
Destination:
  DDOC charge *|AUD|12.00||Australia -> Singapore

[PricingPage]
Entry:
  Australia -> Singapore: LCL
Origin:
  ODOC charge *|AUD|11.00||Australia -> Singapore
Freight:
  BAF charge||2.50|% of freight charges|Australia -> Singapore
  FRT charge||||Australia -> Singapore
  Minimum|AUD|200.00||Australia -> Singapore
  Per Unit|AUD|111.00|per M3 / 1000 KG|Australia -> Singapore
Destination:
  DDOC charge *|AUD|12.00||Australia -> Singapore

[PricingPage]
Entry:
  Netherlands -> Australia: FCL
Origin:
  ODOC charge *|AUD|11.00||Netherlands -> Australia
Freight:
  FRT charge|AUD|101.00|per 20GP Container|Netherlands -> Australia
  FRT charge|AUD|102.00|per 20RE Container|Netherlands -> Australia
  FRT charge|AUD|103.00|per 40GP Container|Netherlands -> Australia
  FRT charge|AUD|104.00|per 40RE Container|Netherlands -> Australia
Destination:
  DDOC charge *|AUD|12.00||Netherlands -> Australia

[PricingPage]
Entry:
  Netherlands -> Australia: LCL
Origin:
  ODOC charge *|AUD|11.00||Netherlands -> Australia
Freight:
  BAF charge||2.50|% of freight charges|Netherlands -> Australia
  FRT charge||||Netherlands -> Australia
  Minimum|AUD|200.00||Netherlands -> Australia
  Per Unit|AUD|111.00|per M3 / 1000 KG|Netherlands -> Australia
Destination:
  DDOC charge *|AUD|12.00||Netherlands -> Australia

[PricingPage]
Entry:
  Singapore -> Australia: FCL
Origin:
  ODOC charge *|AUD|11.00||Singapore -> Australia
Freight:
  FRT charge|AUD|101.00|per 20GP Container|Singapore -> Australia
  FRT charge|AUD|102.00|per 20RE Container|Singapore -> Australia
  FRT charge|AUD|103.00|per 40GP Container|Singapore -> Australia
  FRT charge|AUD|104.00|per 40RE Container|Singapore -> Australia
Destination:
  DDOC charge *|AUD|12.00||Singapore -> Australia

[PricingPage]
Entry:
  Singapore -> Australia: LCL
Origin:
  ODOC charge *|AUD|11.00||Singapore -> Australia
Freight:
  BAF charge||2.50|% of freight charges|Singapore -> Australia
  FRT charge||||Singapore -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|111.00|per M3 / 1000 KG|Singapore -> Australia
Destination:
  DDOC charge *|AUD|12.00||Singapore -> Australia
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		public void TestPopulate_ShippingLandscapeSimple()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ShippingLandscapeSimple"];

			const string expected = @"
[PricingPage]
Entry:
  Australia -> Netherlands: FCL
  Australia -> Netherlands: LCL
  Australia -> Singapore: FCL
  Australia -> Singapore: LCL
  Netherlands -> Australia: FCL
  Netherlands -> Australia: LCL
  Singapore -> Australia: FCL
  Singapore -> Australia: LCL
Origin:
  ODOC charge *|AUD|11.00||Australia -> 
  ODOC charge *|AUD|11.00||Netherlands -> 
  ODOC charge *|AUD|11.00||Singapore -> 
Freight:
  BAF charge||2.50|% of freight charges|Australia -> Netherlands
  BAF charge||2.50|% of freight charges|Australia -> Singapore
  BAF charge||2.50|% of freight charges|Netherlands -> Australia
  BAF charge||2.50|% of freight charges|Singapore -> Australia
  FRT charge||||Australia -> Netherlands
  FRT charge||||Australia -> Singapore
  FRT charge||||Netherlands -> Australia
  FRT charge||||Singapore -> Australia
  FRT charge|AUD|101.00|per 20GP Container|Australia -> Netherlands
  FRT charge|AUD|101.00|per 20GP Container|Australia -> Singapore
  FRT charge|AUD|101.00|per 20GP Container|Netherlands -> Australia
  FRT charge|AUD|101.00|per 20GP Container|Singapore -> Australia
  FRT charge|AUD|102.00|per 20RE Container|Australia -> Netherlands
  FRT charge|AUD|102.00|per 20RE Container|Australia -> Singapore
  FRT charge|AUD|102.00|per 20RE Container|Netherlands -> Australia
  FRT charge|AUD|102.00|per 20RE Container|Singapore -> Australia
  FRT charge|AUD|103.00|per 40GP Container|Australia -> Netherlands
  FRT charge|AUD|103.00|per 40GP Container|Australia -> Singapore
  FRT charge|AUD|103.00|per 40GP Container|Netherlands -> Australia
  FRT charge|AUD|103.00|per 40GP Container|Singapore -> Australia
  FRT charge|AUD|104.00|per 40RE Container|Australia -> Netherlands
  FRT charge|AUD|104.00|per 40RE Container|Australia -> Singapore
  FRT charge|AUD|104.00|per 40RE Container|Netherlands -> Australia
  FRT charge|AUD|104.00|per 40RE Container|Singapore -> Australia
  Minimum|AUD|200.00||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Singapore
  Minimum|AUD|200.00||Netherlands -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|111.00|per M3 / 1000 KG|Australia -> Netherlands
  Per Unit|AUD|111.00|per M3 / 1000 KG|Australia -> Singapore
  Per Unit|AUD|111.00|per M3 / 1000 KG|Netherlands -> Australia
  Per Unit|AUD|111.00|per M3 / 1000 KG|Singapore -> Australia
Destination:
  DDOC charge *|AUD|12.00|| -> Australia
  DDOC charge *|AUD|12.00|| -> Netherlands
  DDOC charge *|AUD|12.00|| -> Singapore
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		public void TestPopulate_ShippingLandscapeComplex()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ShippingLandscapeComplex"];

			const string expected = @"
[PricingPage]
Entry:
  Australia -> Netherlands: FCL
  Australia -> Netherlands: LCL
  Australia -> Singapore: FCL
  Australia -> Singapore: LCL
Origin:
  ODOC charge *|AUD|11.00||Australia -> 
Freight:
  BAF charge||2.50|% of freight charges|Australia -> Netherlands
  BAF charge||2.50|% of freight charges|Australia -> Singapore
  FRT charge||||Australia -> Netherlands
  FRT charge||||Australia -> Singapore
  FRT charge|AUD|101.00|per 20GP Container|Australia -> Netherlands
  FRT charge|AUD|101.00|per 20GP Container|Australia -> Singapore
  FRT charge|AUD|102.00|per 20RE Container|Australia -> Netherlands
  FRT charge|AUD|102.00|per 20RE Container|Australia -> Singapore
  FRT charge|AUD|103.00|per 40GP Container|Australia -> Netherlands
  FRT charge|AUD|103.00|per 40GP Container|Australia -> Singapore
  FRT charge|AUD|104.00|per 40RE Container|Australia -> Netherlands
  FRT charge|AUD|104.00|per 40RE Container|Australia -> Singapore
  Minimum|AUD|200.00||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Singapore
  Per Unit|AUD|111.00|per M3 / 1000 KG|Australia -> Netherlands
  Per Unit|AUD|111.00|per M3 / 1000 KG|Australia -> Singapore
Destination:
  DDOC charge *|AUD|12.00||Australia -> Netherlands
  DDOC charge *|AUD|12.00||Australia -> Singapore

[PricingPage]
Entry:
  Netherlands -> Australia: FCL
  Netherlands -> Australia: LCL
  Singapore -> Australia: FCL
  Singapore -> Australia: LCL
Origin:
  ODOC charge *|AUD|11.00||Netherlands -> Australia
  ODOC charge *|AUD|11.00||Singapore -> Australia
Freight:
  BAF charge||2.50|% of freight charges|Netherlands -> Australia
  BAF charge||2.50|% of freight charges|Singapore -> Australia
  FRT charge||||Netherlands -> Australia
  FRT charge||||Singapore -> Australia
  FRT charge|AUD|101.00|per 20GP Container|Netherlands -> Australia
  FRT charge|AUD|101.00|per 20GP Container|Singapore -> Australia
  FRT charge|AUD|102.00|per 20RE Container|Netherlands -> Australia
  FRT charge|AUD|102.00|per 20RE Container|Singapore -> Australia
  FRT charge|AUD|103.00|per 40GP Container|Netherlands -> Australia
  FRT charge|AUD|103.00|per 40GP Container|Singapore -> Australia
  FRT charge|AUD|104.00|per 40RE Container|Netherlands -> Australia
  FRT charge|AUD|104.00|per 40RE Container|Singapore -> Australia
  Minimum|AUD|200.00||Netherlands -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|111.00|per M3 / 1000 KG|Netherlands -> Australia
  Per Unit|AUD|111.00|per M3 / 1000 KG|Singapore -> Australia
Destination:
  DDOC charge *|AUD|12.00|| -> Australia
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		public void TestPopulate_ShippingCompact()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ShippingCompact"];

			const string expected = @"
[PricingPage]
Entry:
  Australia -> Netherlands: FCL
  Australia -> Netherlands: LCL
  Australia -> Singapore: FCL
  Australia -> Singapore: LCL
  Netherlands -> Australia: FCL
  Netherlands -> Australia: LCL
  Singapore -> Australia: FCL
  Singapore -> Australia: LCL
Origin:
  ODOC charge *|AUD|11.00||Australia -> 
  ODOC charge *|AUD|11.00||Netherlands -> 
  ODOC charge *|AUD|11.00||Singapore -> 
Freight:
  BAF charge||2.50|% of freight charges|Australia -> Netherlands
  BAF charge||2.50|% of freight charges|Australia -> Singapore
  BAF charge||2.50|% of freight charges|Netherlands -> Australia
  BAF charge||2.50|% of freight charges|Singapore -> Australia
  FRT charge||||Australia -> Netherlands
  FRT charge||||Australia -> Singapore
  FRT charge||||Netherlands -> Australia
  FRT charge||||Singapore -> Australia
  FRT charge|AUD|101.00|per 20GP Container|Australia -> Netherlands
  FRT charge|AUD|101.00|per 20GP Container|Australia -> Singapore
  FRT charge|AUD|101.00|per 20GP Container|Netherlands -> Australia
  FRT charge|AUD|101.00|per 20GP Container|Singapore -> Australia
  FRT charge|AUD|102.00|per 20RE Container|Australia -> Netherlands
  FRT charge|AUD|102.00|per 20RE Container|Australia -> Singapore
  FRT charge|AUD|102.00|per 20RE Container|Netherlands -> Australia
  FRT charge|AUD|102.00|per 20RE Container|Singapore -> Australia
  FRT charge|AUD|103.00|per 40GP Container|Australia -> Netherlands
  FRT charge|AUD|103.00|per 40GP Container|Australia -> Singapore
  FRT charge|AUD|103.00|per 40GP Container|Netherlands -> Australia
  FRT charge|AUD|103.00|per 40GP Container|Singapore -> Australia
  FRT charge|AUD|104.00|per 40RE Container|Australia -> Netherlands
  FRT charge|AUD|104.00|per 40RE Container|Australia -> Singapore
  FRT charge|AUD|104.00|per 40RE Container|Netherlands -> Australia
  FRT charge|AUD|104.00|per 40RE Container|Singapore -> Australia
  Minimum|AUD|200.00||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Singapore
  Minimum|AUD|200.00||Netherlands -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|111.00|per M3 / 1000 KG|Australia -> Netherlands
  Per Unit|AUD|111.00|per M3 / 1000 KG|Australia -> Singapore
  Per Unit|AUD|111.00|per M3 / 1000 KG|Netherlands -> Australia
  Per Unit|AUD|111.00|per M3 / 1000 KG|Singapore -> Australia
Destination:
  DDOC charge *|AUD|12.00|| -> Australia
  DDOC charge *|AUD|12.00|| -> Netherlands
  DDOC charge *|AUD|12.00|| -> Singapore
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		public void TestPopulate_ForwardingStandard()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ForwardingStandard"];

			const string expected = @"
[PricingPage]
Entry:
  Australia -> Netherlands: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> Netherlands
Freight:
  FRT charge||||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Netherlands
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Australia -> Netherlands
Destination:
  DDOC charge *|AUD|22.00||Australia -> Netherlands

[PricingPage]
Entry:
  Australia -> Netherlands: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> Netherlands
Freight:
  FRT charge|AUD|161.00|per LD-3 Container|Australia -> Netherlands
  FRT charge|AUD|162.00|per LD-6 Container|Australia -> Netherlands
  FRT charge|AUD|163.00|per LD-9 Container|Australia -> Netherlands
Destination:
  DDOC charge *|AUD|22.00||Australia -> Netherlands

[PricingPage]
Entry:
  Australia -> Netherlands: FCL
Origin:
  ODOC charge *|AUD|21.00||Australia -> Netherlands
Freight:
  FRT charge|AUD|121.00|per 20GP Container|Australia -> Netherlands
  FRT charge|AUD|122.00|per 20RE Container|Australia -> Netherlands
  FRT charge|AUD|123.00|per 40GP Container|Australia -> Netherlands
  FRT charge|AUD|124.00|per 40RE Container|Australia -> Netherlands
Destination:
  DDOC charge *|AUD|22.00||Australia -> Netherlands

[PricingPage]
Entry:
  Australia -> Netherlands: LCL
Origin:
  ODOC charge *|AUD|21.00||Australia -> Netherlands
Freight:
  BAF charge||2.50|% of freight charges|Australia -> Netherlands
  FRT charge||||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Netherlands
  Per Unit|AUD|131.00|per M3 / 1000 KG|Australia -> Netherlands
Destination:
  DDOC charge *|AUD|22.00||Australia -> Netherlands

[PricingPage]
Entry:
  Australia -> Singapore: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> Singapore
Freight:
  FRT charge||||Australia -> Singapore
  Minimum|AUD|200.00||Australia -> Singapore
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Australia -> Singapore
Destination:
  DDOC charge *|AUD|22.00||Australia -> Singapore

[PricingPage]
Entry:
  Australia -> Singapore: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> Singapore
Freight:
  FRT charge|AUD|161.00|per LD-3 Container|Australia -> Singapore
  FRT charge|AUD|162.00|per LD-6 Container|Australia -> Singapore
  FRT charge|AUD|163.00|per LD-9 Container|Australia -> Singapore
Destination:
  DDOC charge *|AUD|22.00||Australia -> Singapore

[PricingPage]
Entry:
  Australia -> Singapore: FCL
Origin:
  ODOC charge *|AUD|21.00||Australia -> Singapore
Freight:
  FRT charge|AUD|121.00|per 20GP Container|Australia -> Singapore
  FRT charge|AUD|122.00|per 20RE Container|Australia -> Singapore
  FRT charge|AUD|123.00|per 40GP Container|Australia -> Singapore
  FRT charge|AUD|124.00|per 40RE Container|Australia -> Singapore
Destination:
  DDOC charge *|AUD|22.00||Australia -> Singapore

[PricingPage]
Entry:
  Australia -> Singapore: LCL
Origin:
  ODOC charge *|AUD|21.00||Australia -> Singapore
Freight:
  BAF charge||2.50|% of freight charges|Australia -> Singapore
  FRT charge||||Australia -> Singapore
  Minimum|AUD|200.00||Australia -> Singapore
  Per Unit|AUD|131.00|per M3 / 1000 KG|Australia -> Singapore
Destination:
  DDOC charge *|AUD|22.00||Australia -> Singapore

[PricingPage]
Entry:
  Netherlands -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Netherlands -> Australia
Freight:
  FRT charge||||Netherlands -> Australia
  Minimum|AUD|200.00||Netherlands -> Australia
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Netherlands -> Australia
Destination:
  DDOC charge *|AUD|22.00||Netherlands -> Australia

[PricingPage]
Entry:
  Netherlands -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Netherlands -> Australia
Freight:
  FRT charge|AUD|161.00|per LD-3 Container|Netherlands -> Australia
  FRT charge|AUD|162.00|per LD-6 Container|Netherlands -> Australia
  FRT charge|AUD|163.00|per LD-9 Container|Netherlands -> Australia
Destination:
  DDOC charge *|AUD|22.00||Netherlands -> Australia

[PricingPage]
Entry:
  Netherlands -> Australia: FCL
Origin:
  ODOC charge *|AUD|21.00||Netherlands -> Australia
Freight:
  FRT charge|AUD|121.00|per 20GP Container|Netherlands -> Australia
  FRT charge|AUD|122.00|per 20RE Container|Netherlands -> Australia
  FRT charge|AUD|123.00|per 40GP Container|Netherlands -> Australia
  FRT charge|AUD|124.00|per 40RE Container|Netherlands -> Australia
Destination:
  DDOC charge *|AUD|22.00||Netherlands -> Australia

[PricingPage]
Entry:
  Netherlands -> Australia: LCL
Origin:
  ODOC charge *|AUD|21.00||Netherlands -> Australia
Freight:
  BAF charge||2.50|% of freight charges|Netherlands -> Australia
  FRT charge||||Netherlands -> Australia
  Minimum|AUD|200.00||Netherlands -> Australia
  Per Unit|AUD|131.00|per M3 / 1000 KG|Netherlands -> Australia
Destination:
  DDOC charge *|AUD|22.00||Netherlands -> Australia

[PricingPage]
Entry:
  Singapore -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Singapore -> Australia
Freight:
  FRT charge||||Singapore -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00||Singapore -> Australia

[PricingPage]
Entry:
  Singapore -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Singapore -> Australia
Freight:
  FRT charge|AUD|161.00|per LD-3 Container|Singapore -> Australia
  FRT charge|AUD|162.00|per LD-6 Container|Singapore -> Australia
  FRT charge|AUD|163.00|per LD-9 Container|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00||Singapore -> Australia

[PricingPage]
Entry:
  Singapore -> Australia: FCL
Origin:
  ODOC charge *|AUD|21.00||Singapore -> Australia
Freight:
  FRT charge|AUD|121.00|per 20GP Container|Singapore -> Australia
  FRT charge|AUD|122.00|per 20RE Container|Singapore -> Australia
  FRT charge|AUD|123.00|per 40GP Container|Singapore -> Australia
  FRT charge|AUD|124.00|per 40RE Container|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00||Singapore -> Australia

[PricingPage]
Entry:
  Singapore -> Australia: LCL
Origin:
  ODOC charge *|AUD|21.00||Singapore -> Australia
Freight:
  BAF charge||2.50|% of freight charges|Singapore -> Australia
  FRT charge||||Singapore -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|131.00|per M3 / 1000 KG|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00||Singapore -> Australia
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		public void TestPopulate_ForwardingLandscapeSimple()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ForwardingLandscapeSimple"];

			const string expected = @"
[PricingPage]
Entry:
  Australia -> Netherlands: Air
  Australia -> Singapore: Air
  Netherlands -> Australia: Air
  Singapore -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
  ODOC charge *|AUD|21.00||Netherlands -> 
  ODOC charge *|AUD|21.00||Singapore -> 
Freight:
  FRT charge||||Australia -> Netherlands
  FRT charge||||Australia -> Singapore
  FRT charge||||Netherlands -> Australia
  FRT charge||||Singapore -> Australia
  Minimum|AUD|200.00||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Singapore
  Minimum|AUD|200.00||Netherlands -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Australia -> Netherlands
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Australia -> Singapore
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Netherlands -> Australia
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia
  DDOC charge *|AUD|22.00|| -> Netherlands
  DDOC charge *|AUD|22.00|| -> Singapore

[PricingPage]
Entry:
  Australia -> Netherlands: Air
  Australia -> Singapore: Air
  Netherlands -> Australia: Air
  Singapore -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
  ODOC charge *|AUD|21.00||Netherlands -> 
  ODOC charge *|AUD|21.00||Singapore -> 
Freight:
  FRT charge|AUD|161.00|per LD-3 Container|Australia -> Netherlands
  FRT charge|AUD|161.00|per LD-3 Container|Australia -> Singapore
  FRT charge|AUD|161.00|per LD-3 Container|Netherlands -> Australia
  FRT charge|AUD|161.00|per LD-3 Container|Singapore -> Australia
  FRT charge|AUD|162.00|per LD-6 Container|Australia -> Netherlands
  FRT charge|AUD|162.00|per LD-6 Container|Australia -> Singapore
  FRT charge|AUD|162.00|per LD-6 Container|Netherlands -> Australia
  FRT charge|AUD|162.00|per LD-6 Container|Singapore -> Australia
  FRT charge|AUD|163.00|per LD-9 Container|Australia -> Netherlands
  FRT charge|AUD|163.00|per LD-9 Container|Australia -> Singapore
  FRT charge|AUD|163.00|per LD-9 Container|Netherlands -> Australia
  FRT charge|AUD|163.00|per LD-9 Container|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia
  DDOC charge *|AUD|22.00|| -> Netherlands
  DDOC charge *|AUD|22.00|| -> Singapore

[PricingPage]
Entry:
  Australia -> Netherlands: FCL
  Australia -> Netherlands: LCL
  Australia -> Singapore: FCL
  Australia -> Singapore: LCL
  Netherlands -> Australia: FCL
  Netherlands -> Australia: LCL
  Singapore -> Australia: FCL
  Singapore -> Australia: LCL
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
  ODOC charge *|AUD|21.00||Netherlands -> 
  ODOC charge *|AUD|21.00||Singapore -> 
Freight:
  BAF charge||2.50|% of freight charges|Australia -> Netherlands
  BAF charge||2.50|% of freight charges|Australia -> Singapore
  BAF charge||2.50|% of freight charges|Netherlands -> Australia
  BAF charge||2.50|% of freight charges|Singapore -> Australia
  FRT charge||||Australia -> Netherlands
  FRT charge||||Australia -> Singapore
  FRT charge||||Netherlands -> Australia
  FRT charge||||Singapore -> Australia
  FRT charge|AUD|121.00|per 20GP Container|Australia -> Netherlands
  FRT charge|AUD|121.00|per 20GP Container|Australia -> Singapore
  FRT charge|AUD|121.00|per 20GP Container|Netherlands -> Australia
  FRT charge|AUD|121.00|per 20GP Container|Singapore -> Australia
  FRT charge|AUD|122.00|per 20RE Container|Australia -> Netherlands
  FRT charge|AUD|122.00|per 20RE Container|Australia -> Singapore
  FRT charge|AUD|122.00|per 20RE Container|Netherlands -> Australia
  FRT charge|AUD|122.00|per 20RE Container|Singapore -> Australia
  FRT charge|AUD|123.00|per 40GP Container|Australia -> Netherlands
  FRT charge|AUD|123.00|per 40GP Container|Australia -> Singapore
  FRT charge|AUD|123.00|per 40GP Container|Netherlands -> Australia
  FRT charge|AUD|123.00|per 40GP Container|Singapore -> Australia
  FRT charge|AUD|124.00|per 40RE Container|Australia -> Netherlands
  FRT charge|AUD|124.00|per 40RE Container|Australia -> Singapore
  FRT charge|AUD|124.00|per 40RE Container|Netherlands -> Australia
  FRT charge|AUD|124.00|per 40RE Container|Singapore -> Australia
  Minimum|AUD|200.00||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Singapore
  Minimum|AUD|200.00||Netherlands -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|131.00|per M3 / 1000 KG|Australia -> Netherlands
  Per Unit|AUD|131.00|per M3 / 1000 KG|Australia -> Singapore
  Per Unit|AUD|131.00|per M3 / 1000 KG|Netherlands -> Australia
  Per Unit|AUD|131.00|per M3 / 1000 KG|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia
  DDOC charge *|AUD|22.00|| -> Netherlands
  DDOC charge *|AUD|22.00|| -> Singapore
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		public void TestPopulate_ForwardingLandscapeSimpleLoose()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ForwardingLandscapeSimpleLoose"];

			const string expected = @"
[PricingPage]
Entry:
  Australia -> Netherlands: Air
  Australia -> Singapore: Air
  Netherlands -> Australia: Air
  Singapore -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
  ODOC charge *|AUD|21.00||Netherlands -> 
  ODOC charge *|AUD|21.00||Singapore -> 
Freight:
  FRT charge||||Australia -> Netherlands
  FRT charge||||Australia -> Singapore
  FRT charge||||Netherlands -> Australia
  FRT charge||||Singapore -> Australia
  Minimum|AUD|200.00||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Singapore
  Minimum|AUD|200.00||Netherlands -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Australia -> Netherlands
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Australia -> Singapore
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Netherlands -> Australia
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia
  DDOC charge *|AUD|22.00|| -> Netherlands
  DDOC charge *|AUD|22.00|| -> Singapore
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		public void TestPopulate_ForwardingLandscapeSimpleNonLoose()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ForwardingLandscapeSimpleNonLoose"];

			const string expected = @"
[PricingPage]
Entry:
  Australia -> Netherlands: Air
  Australia -> Singapore: Air
  Netherlands -> Australia: Air
  Singapore -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
  ODOC charge *|AUD|21.00||Netherlands -> 
  ODOC charge *|AUD|21.00||Singapore -> 
Freight:
  FRT charge|AUD|161.00|per LD-3 Container|Australia -> Netherlands
  FRT charge|AUD|161.00|per LD-3 Container|Australia -> Singapore
  FRT charge|AUD|161.00|per LD-3 Container|Netherlands -> Australia
  FRT charge|AUD|161.00|per LD-3 Container|Singapore -> Australia
  FRT charge|AUD|162.00|per LD-6 Container|Australia -> Netherlands
  FRT charge|AUD|162.00|per LD-6 Container|Australia -> Singapore
  FRT charge|AUD|162.00|per LD-6 Container|Netherlands -> Australia
  FRT charge|AUD|162.00|per LD-6 Container|Singapore -> Australia
  FRT charge|AUD|163.00|per LD-9 Container|Australia -> Netherlands
  FRT charge|AUD|163.00|per LD-9 Container|Australia -> Singapore
  FRT charge|AUD|163.00|per LD-9 Container|Netherlands -> Australia
  FRT charge|AUD|163.00|per LD-9 Container|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia
  DDOC charge *|AUD|22.00|| -> Netherlands
  DDOC charge *|AUD|22.00|| -> Singapore

[PricingPage]
Entry:
  Australia -> Netherlands: FCL
  Australia -> Netherlands: LCL
  Australia -> Singapore: FCL
  Australia -> Singapore: LCL
  Netherlands -> Australia: FCL
  Netherlands -> Australia: LCL
  Singapore -> Australia: FCL
  Singapore -> Australia: LCL
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
  ODOC charge *|AUD|21.00||Netherlands -> 
  ODOC charge *|AUD|21.00||Singapore -> 
Freight:
  BAF charge||2.50|% of freight charges|Australia -> Netherlands
  BAF charge||2.50|% of freight charges|Australia -> Singapore
  BAF charge||2.50|% of freight charges|Netherlands -> Australia
  BAF charge||2.50|% of freight charges|Singapore -> Australia
  FRT charge||||Australia -> Netherlands
  FRT charge||||Australia -> Singapore
  FRT charge||||Netherlands -> Australia
  FRT charge||||Singapore -> Australia
  FRT charge|AUD|121.00|per 20GP Container|Australia -> Netherlands
  FRT charge|AUD|121.00|per 20GP Container|Australia -> Singapore
  FRT charge|AUD|121.00|per 20GP Container|Netherlands -> Australia
  FRT charge|AUD|121.00|per 20GP Container|Singapore -> Australia
  FRT charge|AUD|122.00|per 20RE Container|Australia -> Netherlands
  FRT charge|AUD|122.00|per 20RE Container|Australia -> Singapore
  FRT charge|AUD|122.00|per 20RE Container|Netherlands -> Australia
  FRT charge|AUD|122.00|per 20RE Container|Singapore -> Australia
  FRT charge|AUD|123.00|per 40GP Container|Australia -> Netherlands
  FRT charge|AUD|123.00|per 40GP Container|Australia -> Singapore
  FRT charge|AUD|123.00|per 40GP Container|Netherlands -> Australia
  FRT charge|AUD|123.00|per 40GP Container|Singapore -> Australia
  FRT charge|AUD|124.00|per 40RE Container|Australia -> Netherlands
  FRT charge|AUD|124.00|per 40RE Container|Australia -> Singapore
  FRT charge|AUD|124.00|per 40RE Container|Netherlands -> Australia
  FRT charge|AUD|124.00|per 40RE Container|Singapore -> Australia
  Minimum|AUD|200.00||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Singapore
  Minimum|AUD|200.00||Netherlands -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|131.00|per M3 / 1000 KG|Australia -> Netherlands
  Per Unit|AUD|131.00|per M3 / 1000 KG|Australia -> Singapore
  Per Unit|AUD|131.00|per M3 / 1000 KG|Netherlands -> Australia
  Per Unit|AUD|131.00|per M3 / 1000 KG|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia
  DDOC charge *|AUD|22.00|| -> Netherlands
  DDOC charge *|AUD|22.00|| -> Singapore
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		public void TestPopulate_ForwardingLandscapeComplex()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ForwardingLandscapeComplex"];

			const string expected = @"
[PricingPage]
Entry:
  Australia -> Netherlands: Air
  Australia -> Singapore: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
Freight:
  FRT charge||||Australia -> Netherlands
  FRT charge||||Australia -> Singapore
  Minimum|AUD|200.00||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Singapore
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Australia -> Netherlands
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Australia -> Singapore
Destination:
  DDOC charge *|AUD|22.00||Australia -> Netherlands
  DDOC charge *|AUD|22.00||Australia -> Singapore

[PricingPage]
Entry:
  Australia -> Netherlands: Air
  Australia -> Singapore: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
Freight:
  FRT charge|AUD|161.00|per LD-3 Container|Australia -> Netherlands
  FRT charge|AUD|161.00|per LD-3 Container|Australia -> Singapore
  FRT charge|AUD|162.00|per LD-6 Container|Australia -> Netherlands
  FRT charge|AUD|162.00|per LD-6 Container|Australia -> Singapore
  FRT charge|AUD|163.00|per LD-9 Container|Australia -> Netherlands
  FRT charge|AUD|163.00|per LD-9 Container|Australia -> Singapore
Destination:
  DDOC charge *|AUD|22.00||Australia -> Netherlands
  DDOC charge *|AUD|22.00||Australia -> Singapore

[PricingPage]
Entry:
  Australia -> Netherlands: FCL
  Australia -> Netherlands: LCL
  Australia -> Singapore: FCL
  Australia -> Singapore: LCL
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
Freight:
  BAF charge||2.50|% of freight charges|Australia -> Netherlands
  BAF charge||2.50|% of freight charges|Australia -> Singapore
  FRT charge||||Australia -> Netherlands
  FRT charge||||Australia -> Singapore
  FRT charge|AUD|121.00|per 20GP Container|Australia -> Netherlands
  FRT charge|AUD|121.00|per 20GP Container|Australia -> Singapore
  FRT charge|AUD|122.00|per 20RE Container|Australia -> Netherlands
  FRT charge|AUD|122.00|per 20RE Container|Australia -> Singapore
  FRT charge|AUD|123.00|per 40GP Container|Australia -> Netherlands
  FRT charge|AUD|123.00|per 40GP Container|Australia -> Singapore
  FRT charge|AUD|124.00|per 40RE Container|Australia -> Netherlands
  FRT charge|AUD|124.00|per 40RE Container|Australia -> Singapore
  Minimum|AUD|200.00||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Singapore
  Per Unit|AUD|131.00|per M3 / 1000 KG|Australia -> Netherlands
  Per Unit|AUD|131.00|per M3 / 1000 KG|Australia -> Singapore
Destination:
  DDOC charge *|AUD|22.00||Australia -> Netherlands
  DDOC charge *|AUD|22.00||Australia -> Singapore

[PricingPage]
Entry:
  Netherlands -> Australia: Air
  Singapore -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Netherlands -> Australia
  ODOC charge *|AUD|21.00||Singapore -> Australia
Freight:
  FRT charge||||Netherlands -> Australia
  FRT charge||||Singapore -> Australia
  Minimum|AUD|200.00||Netherlands -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Netherlands -> Australia
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia

[PricingPage]
Entry:
  Netherlands -> Australia: Air
  Singapore -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Netherlands -> Australia
  ODOC charge *|AUD|21.00||Singapore -> Australia
Freight:
  FRT charge|AUD|161.00|per LD-3 Container|Netherlands -> Australia
  FRT charge|AUD|161.00|per LD-3 Container|Singapore -> Australia
  FRT charge|AUD|162.00|per LD-6 Container|Netherlands -> Australia
  FRT charge|AUD|162.00|per LD-6 Container|Singapore -> Australia
  FRT charge|AUD|163.00|per LD-9 Container|Netherlands -> Australia
  FRT charge|AUD|163.00|per LD-9 Container|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia

[PricingPage]
Entry:
  Netherlands -> Australia: FCL
  Netherlands -> Australia: LCL
  Singapore -> Australia: FCL
  Singapore -> Australia: LCL
Origin:
  ODOC charge *|AUD|21.00||Netherlands -> Australia
  ODOC charge *|AUD|21.00||Singapore -> Australia
Freight:
  BAF charge||2.50|% of freight charges|Netherlands -> Australia
  BAF charge||2.50|% of freight charges|Singapore -> Australia
  FRT charge||||Netherlands -> Australia
  FRT charge||||Singapore -> Australia
  FRT charge|AUD|121.00|per 20GP Container|Netherlands -> Australia
  FRT charge|AUD|121.00|per 20GP Container|Singapore -> Australia
  FRT charge|AUD|122.00|per 20RE Container|Netherlands -> Australia
  FRT charge|AUD|122.00|per 20RE Container|Singapore -> Australia
  FRT charge|AUD|123.00|per 40GP Container|Netherlands -> Australia
  FRT charge|AUD|123.00|per 40GP Container|Singapore -> Australia
  FRT charge|AUD|124.00|per 40RE Container|Netherlands -> Australia
  FRT charge|AUD|124.00|per 40RE Container|Singapore -> Australia
  Minimum|AUD|200.00||Netherlands -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|131.00|per M3 / 1000 KG|Netherlands -> Australia
  Per Unit|AUD|131.00|per M3 / 1000 KG|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		public void TestPopulate_ForwardingLandscapeComplexLoose()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ForwardingLandscapeComplexLoose"];

			const string expected = @"
[PricingPage]
Entry:
  Australia -> Netherlands: Air
  Australia -> Singapore: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
Freight:
  FRT charge||||Australia -> Netherlands
  FRT charge||||Australia -> Singapore
  Minimum|AUD|200.00||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Singapore
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Australia -> Netherlands
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Australia -> Singapore
Destination:
  DDOC charge *|AUD|22.00||Australia -> Netherlands
  DDOC charge *|AUD|22.00||Australia -> Singapore

[PricingPage]
Entry:
  Netherlands -> Australia: Air
  Singapore -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Netherlands -> Australia
  ODOC charge *|AUD|21.00||Singapore -> Australia
Freight:
  FRT charge||||Netherlands -> Australia
  FRT charge||||Singapore -> Australia
  Minimum|AUD|200.00||Netherlands -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Netherlands -> Australia
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		public void TestPopulate_ForwardingLandscapeComplexNonLoose()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ForwardingLandscapeComplexNonLoose"];

			const string expected = @"
[PricingPage]
Entry:
  Australia -> Netherlands: Air
  Australia -> Singapore: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
Freight:
  FRT charge|AUD|161.00|per LD-3 Container|Australia -> Netherlands
  FRT charge|AUD|161.00|per LD-3 Container|Australia -> Singapore
  FRT charge|AUD|162.00|per LD-6 Container|Australia -> Netherlands
  FRT charge|AUD|162.00|per LD-6 Container|Australia -> Singapore
  FRT charge|AUD|163.00|per LD-9 Container|Australia -> Netherlands
  FRT charge|AUD|163.00|per LD-9 Container|Australia -> Singapore
Destination:
  DDOC charge *|AUD|22.00||Australia -> Netherlands
  DDOC charge *|AUD|22.00||Australia -> Singapore

[PricingPage]
Entry:
  Australia -> Netherlands: FCL
  Australia -> Netherlands: LCL
  Australia -> Singapore: FCL
  Australia -> Singapore: LCL
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
Freight:
  BAF charge||2.50|% of freight charges|Australia -> Netherlands
  BAF charge||2.50|% of freight charges|Australia -> Singapore
  FRT charge||||Australia -> Netherlands
  FRT charge||||Australia -> Singapore
  FRT charge|AUD|121.00|per 20GP Container|Australia -> Netherlands
  FRT charge|AUD|121.00|per 20GP Container|Australia -> Singapore
  FRT charge|AUD|122.00|per 20RE Container|Australia -> Netherlands
  FRT charge|AUD|122.00|per 20RE Container|Australia -> Singapore
  FRT charge|AUD|123.00|per 40GP Container|Australia -> Netherlands
  FRT charge|AUD|123.00|per 40GP Container|Australia -> Singapore
  FRT charge|AUD|124.00|per 40RE Container|Australia -> Netherlands
  FRT charge|AUD|124.00|per 40RE Container|Australia -> Singapore
  Minimum|AUD|200.00||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Singapore
  Per Unit|AUD|131.00|per M3 / 1000 KG|Australia -> Netherlands
  Per Unit|AUD|131.00|per M3 / 1000 KG|Australia -> Singapore
Destination:
  DDOC charge *|AUD|22.00||Australia -> Netherlands
  DDOC charge *|AUD|22.00||Australia -> Singapore

[PricingPage]
Entry:
  Netherlands -> Australia: Air
  Singapore -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Netherlands -> Australia
  ODOC charge *|AUD|21.00||Singapore -> Australia
Freight:
  FRT charge|AUD|161.00|per LD-3 Container|Netherlands -> Australia
  FRT charge|AUD|161.00|per LD-3 Container|Singapore -> Australia
  FRT charge|AUD|162.00|per LD-6 Container|Netherlands -> Australia
  FRT charge|AUD|162.00|per LD-6 Container|Singapore -> Australia
  FRT charge|AUD|163.00|per LD-9 Container|Netherlands -> Australia
  FRT charge|AUD|163.00|per LD-9 Container|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia

[PricingPage]
Entry:
  Netherlands -> Australia: FCL
  Netherlands -> Australia: LCL
  Singapore -> Australia: FCL
  Singapore -> Australia: LCL
Origin:
  ODOC charge *|AUD|21.00||Netherlands -> Australia
  ODOC charge *|AUD|21.00||Singapore -> Australia
Freight:
  BAF charge||2.50|% of freight charges|Netherlands -> Australia
  BAF charge||2.50|% of freight charges|Singapore -> Australia
  FRT charge||||Netherlands -> Australia
  FRT charge||||Singapore -> Australia
  FRT charge|AUD|121.00|per 20GP Container|Netherlands -> Australia
  FRT charge|AUD|121.00|per 20GP Container|Singapore -> Australia
  FRT charge|AUD|122.00|per 20RE Container|Netherlands -> Australia
  FRT charge|AUD|122.00|per 20RE Container|Singapore -> Australia
  FRT charge|AUD|123.00|per 40GP Container|Netherlands -> Australia
  FRT charge|AUD|123.00|per 40GP Container|Singapore -> Australia
  FRT charge|AUD|124.00|per 40RE Container|Netherlands -> Australia
  FRT charge|AUD|124.00|per 40RE Container|Singapore -> Australia
  Minimum|AUD|200.00||Netherlands -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|131.00|per M3 / 1000 KG|Netherlands -> Australia
  Per Unit|AUD|131.00|per M3 / 1000 KG|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		public void TestPopulate_ForwardingCompact()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ForwardingCompact"];

			const string expected = @"
[PricingPage]
Entry:
  Australia -> Netherlands: Air
  Australia -> Singapore: Air
  Netherlands -> Australia: Air
  Singapore -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
  ODOC charge *|AUD|21.00||Netherlands -> 
  ODOC charge *|AUD|21.00||Singapore -> 
Freight:
  FRT charge||||Australia -> Netherlands
  FRT charge||||Australia -> Singapore
  FRT charge||||Netherlands -> Australia
  FRT charge||||Singapore -> Australia
  Minimum|AUD|200.00||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Singapore
  Minimum|AUD|200.00||Netherlands -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Australia -> Netherlands
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Australia -> Singapore
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Netherlands -> Australia
  Per Unit|AUD|141.00|per M3 (1 KG = 6000 CC)|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia
  DDOC charge *|AUD|22.00|| -> Netherlands
  DDOC charge *|AUD|22.00|| -> Singapore

[PricingPage]
Entry:
  Australia -> Netherlands: Air
  Australia -> Singapore: Air
  Netherlands -> Australia: Air
  Singapore -> Australia: Air
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
  ODOC charge *|AUD|21.00||Netherlands -> 
  ODOC charge *|AUD|21.00||Singapore -> 
Freight:
  FRT charge|AUD|161.00|per LD-3 Container|Australia -> Netherlands
  FRT charge|AUD|161.00|per LD-3 Container|Australia -> Singapore
  FRT charge|AUD|161.00|per LD-3 Container|Netherlands -> Australia
  FRT charge|AUD|161.00|per LD-3 Container|Singapore -> Australia
  FRT charge|AUD|162.00|per LD-6 Container|Australia -> Netherlands
  FRT charge|AUD|162.00|per LD-6 Container|Australia -> Singapore
  FRT charge|AUD|162.00|per LD-6 Container|Netherlands -> Australia
  FRT charge|AUD|162.00|per LD-6 Container|Singapore -> Australia
  FRT charge|AUD|163.00|per LD-9 Container|Australia -> Netherlands
  FRT charge|AUD|163.00|per LD-9 Container|Australia -> Singapore
  FRT charge|AUD|163.00|per LD-9 Container|Netherlands -> Australia
  FRT charge|AUD|163.00|per LD-9 Container|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia
  DDOC charge *|AUD|22.00|| -> Netherlands
  DDOC charge *|AUD|22.00|| -> Singapore

[PricingPage]
Entry:
  Australia -> Netherlands: FCL
  Australia -> Netherlands: LCL
  Australia -> Singapore: FCL
  Australia -> Singapore: LCL
  Netherlands -> Australia: FCL
  Netherlands -> Australia: LCL
  Singapore -> Australia: FCL
  Singapore -> Australia: LCL
Origin:
  ODOC charge *|AUD|21.00||Australia -> 
  ODOC charge *|AUD|21.00||Netherlands -> 
  ODOC charge *|AUD|21.00||Singapore -> 
Freight:
  BAF charge||2.50|% of freight charges|Australia -> Netherlands
  BAF charge||2.50|% of freight charges|Australia -> Singapore
  BAF charge||2.50|% of freight charges|Netherlands -> Australia
  BAF charge||2.50|% of freight charges|Singapore -> Australia
  FRT charge||||Australia -> Netherlands
  FRT charge||||Australia -> Singapore
  FRT charge||||Netherlands -> Australia
  FRT charge||||Singapore -> Australia
  FRT charge|AUD|121.00|per 20GP Container|Australia -> Netherlands
  FRT charge|AUD|121.00|per 20GP Container|Australia -> Singapore
  FRT charge|AUD|121.00|per 20GP Container|Netherlands -> Australia
  FRT charge|AUD|121.00|per 20GP Container|Singapore -> Australia
  FRT charge|AUD|122.00|per 20RE Container|Australia -> Netherlands
  FRT charge|AUD|122.00|per 20RE Container|Australia -> Singapore
  FRT charge|AUD|122.00|per 20RE Container|Netherlands -> Australia
  FRT charge|AUD|122.00|per 20RE Container|Singapore -> Australia
  FRT charge|AUD|123.00|per 40GP Container|Australia -> Netherlands
  FRT charge|AUD|123.00|per 40GP Container|Australia -> Singapore
  FRT charge|AUD|123.00|per 40GP Container|Netherlands -> Australia
  FRT charge|AUD|123.00|per 40GP Container|Singapore -> Australia
  FRT charge|AUD|124.00|per 40RE Container|Australia -> Netherlands
  FRT charge|AUD|124.00|per 40RE Container|Australia -> Singapore
  FRT charge|AUD|124.00|per 40RE Container|Netherlands -> Australia
  FRT charge|AUD|124.00|per 40RE Container|Singapore -> Australia
  Minimum|AUD|200.00||Australia -> Netherlands
  Minimum|AUD|200.00||Australia -> Singapore
  Minimum|AUD|200.00||Netherlands -> Australia
  Minimum|AUD|200.00||Singapore -> Australia
  Per Unit|AUD|131.00|per M3 / 1000 KG|Australia -> Netherlands
  Per Unit|AUD|131.00|per M3 / 1000 KG|Australia -> Singapore
  Per Unit|AUD|131.00|per M3 / 1000 KG|Netherlands -> Australia
  Per Unit|AUD|131.00|per M3 / 1000 KG|Singapore -> Australia
Destination:
  DDOC charge *|AUD|22.00|| -> Australia
  DDOC charge *|AUD|22.00|| -> Netherlands
  DDOC charge *|AUD|22.00|| -> Singapore
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		public void TestPopulate_ShippingDetention()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["ShippingDetention"];

			const string expected = @"
[PricingPage]
Entry:
   -> Australia: FCL
   -> Netherlands: FCL
   -> Singapore: FCL
Origin:
  Origin Cartage Detention *|AUD|61.00|per 20GP Container| -> Australia
  Origin Cartage Detention *|AUD|61.00|per 20GP Container| -> Netherlands
  Origin Cartage Detention *|AUD|61.00|per 20GP Container| -> Singapore
  Origin Cartage Detention *|AUD|62.00|per 20RE Container| -> Australia
  Origin Cartage Detention *|AUD|62.00|per 20RE Container| -> Netherlands
  Origin Cartage Detention *|AUD|62.00|per 20RE Container| -> Singapore
  Origin Cartage Detention *|AUD|63.00|per 40GP Container| -> Australia
  Origin Cartage Detention *|AUD|63.00|per 40GP Container| -> Netherlands
  Origin Cartage Detention *|AUD|63.00|per 40GP Container| -> Singapore
  Origin Cartage Detention *|AUD|64.00|per 40RE Container| -> Australia
  Origin Cartage Detention *|AUD|64.00|per 40RE Container| -> Netherlands
  Origin Cartage Detention *|AUD|64.00|per 40RE Container| -> Singapore
Freight:
Destination:

[PricingPage]
Entry:
  Australia -> : FCL
  Netherlands -> : FCL
  Singapore -> : FCL
Origin:
Freight:
Destination:
  Destination Cartage Detention *|AUD|51.00|per 20GP Container|Australia -> 
  Destination Cartage Detention *|AUD|51.00|per 20GP Container|Netherlands -> 
  Destination Cartage Detention *|AUD|51.00|per 20GP Container|Singapore -> 
  Destination Cartage Detention *|AUD|52.00|per 20RE Container|Australia -> 
  Destination Cartage Detention *|AUD|52.00|per 20RE Container|Netherlands -> 
  Destination Cartage Detention *|AUD|52.00|per 20RE Container|Singapore -> 
  Destination Cartage Detention *|AUD|53.00|per 40GP Container|Australia -> 
  Destination Cartage Detention *|AUD|53.00|per 40GP Container|Netherlands -> 
  Destination Cartage Detention *|AUD|53.00|per 40GP Container|Singapore -> 
  Destination Cartage Detention *|AUD|54.00|per 40RE Container|Australia -> 
  Destination Cartage Detention *|AUD|54.00|per 40RE Container|Netherlands -> 
  Destination Cartage Detention *|AUD|54.00|per 40RE Container|Singapore -> 
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		public void TestPopulate_CFS()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");
			CompanyTariff header = GenerateMegaTariff(Factory);
			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(header, "Pricing Page", Factory)["CFS"];

			const string expected = @"
[PricingPage]
Entry:
   -> Australia: ALL
   -> Netherlands: ALL
   -> Singapore: ALL
  Australia -> : ALL
  Netherlands -> : ALL
  Singapore -> : ALL
Origin:
  CFSPACK charge *|AUD|31.00|per 20GP Container|Australia -> 
  CFSPACK charge *|AUD|31.00|per 20GP Container|Netherlands -> 
  CFSPACK charge *|AUD|31.00|per 20GP Container|Singapore -> 
  CFSPACK charge *|AUD|32.00|per 20RE Container|Australia -> 
  CFSPACK charge *|AUD|32.00|per 20RE Container|Netherlands -> 
  CFSPACK charge *|AUD|32.00|per 20RE Container|Singapore -> 
  CFSPACK charge *|AUD|33.00|per 40GP Container|Australia -> 
  CFSPACK charge *|AUD|33.00|per 40GP Container|Netherlands -> 
  CFSPACK charge *|AUD|33.00|per 40GP Container|Singapore -> 
  CFSPACK charge *|AUD|34.00|per 40RE Container|Australia -> 
  CFSPACK charge *|AUD|34.00|per 40RE Container|Netherlands -> 
  CFSPACK charge *|AUD|34.00|per 40RE Container|Singapore -> 
Freight:
Destination:
  CFSUNPA charge *|AUD|41.00|per 20GP Container| -> Australia
  CFSUNPA charge *|AUD|41.00|per 20GP Container| -> Netherlands
  CFSUNPA charge *|AUD|41.00|per 20GP Container| -> Singapore
  CFSUNPA charge *|AUD|42.00|per 20RE Container| -> Australia
  CFSUNPA charge *|AUD|42.00|per 20RE Container| -> Netherlands
  CFSUNPA charge *|AUD|42.00|per 20RE Container| -> Singapore
  CFSUNPA charge *|AUD|43.00|per 40GP Container| -> Australia
  CFSUNPA charge *|AUD|43.00|per 40GP Container| -> Netherlands
  CFSUNPA charge *|AUD|43.00|per 40GP Container| -> Singapore
  CFSUNPA charge *|AUD|44.00|per 40RE Container| -> Australia
  CFSUNPA charge *|AUD|44.00|per 40RE Container| -> Netherlands
  CFSUNPA charge *|AUD|44.00|per 40RE Container| -> Singapore
";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		RateLine AddFlatCharge(RateEntry entry, ZString chargecode, ZString currency, ZDecimal amount)
		{
			RateLine line = entry.AddRateLine(chargecode, FlatCalculator.Code, "", currency);
			line.TL_RateDesc = chargecode + " charge";
			((FlatCalculator)line.Calculator).BaseRate = amount;

			return line;
		}

		public void TestPopulateContainerClass()
		{
			ClientRate rate = Factory.New<ClientRate>();
			rate.TH_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			RateEntry entry1 = rate.AddRateEntry(RatingConstants.RateCategory.PAC, "ALL", "AUMEL", "NZAKL", "STD", "");
			entry1.RateLines.RemoveAndDeleteAll();
			AddFlatCharge(entry1, "CFSPACK", "AUD", 10);
			AddFlatCharge(entry1, "CFSPACK", "AUD", 20);

			RateEntry entry2 = rate.AddRateEntry(RatingConstants.RateCategory.PAC, "FCL", "AUMEL", "NZAKL", "STD", "20GP");
			entry2.RateLines.RemoveAndDeleteAll();
			AddFlatCharge(entry2, "CFSPACK", "AUD", 30);

			RateEntry entry3 = rate.AddRateEntry(RatingConstants.RateCategory.PAC, "FCL", "AUMEL", "NZAKL", "STD", "40GP");
			entry3.RateLines.RemoveAndDeleteAll();
			AddFlatCharge(entry3, "CFSPACK", "AUD", 40);

			Factory.Save();

			PricingPageSetWrapper wrapper = new PricingPageSetWrapperCollection(rate, "Pricing Page", Factory)["CFS"];

			const string expected = @"
[PricingPage]
Entry:
  Melbourne -> Auckland: ALL
  Melbourne -> Auckland: FCL
Origin:
  |AUD|10.00||Melbourne -> Auckland
  |AUD|20.00||Melbourne -> Auckland
  20GP|AUD|30.00||Melbourne -> Auckland
  40GP|AUD|40.00||Melbourne -> Auckland
  CFSPACK charge *||||Melbourne -> Auckland
  CFSPACK charge *||||Melbourne -> Auckland
Freight:
Destination:

";

			AssertMultilineASCIIEquals("", expected, Render(wrapper));
		}

		#region CreatePricingPageWrapperCollection Entries By Category

		#region Combined Supplimentary Entries in Freight Entries' Pricing Pages

		public void TestCreatePricingPageWrapperCollection_CombinesEntriesByCategory_LandscapeCompactStyle()
		{
			AssertCreatePricingPageWrapperCollection_CombinesEntriesByCategory(PricingPaginationStrategy.LandscapeCompactStyle);
		}

		public void TestCreatePricingPageWrapperCollection_CombinesEntriesByCategory_LandscapeComplexStyle()
		{
			AssertCreatePricingPageWrapperCollection_CombinesEntriesByCategory(PricingPaginationStrategy.LandscapeComplexStyle);
		}

		public void TestCreatePricingPageWrapperCollection_CombinesEntriesByCategory_LandscapeSimpleStyle()
		{
			AssertCreatePricingPageWrapperCollection_CombinesEntriesByCategory(PricingPaginationStrategy.LandscapeSimpleStyle);
		}

		public void TestCreatePricingPageWrapperCollection_CombinesEntriesByCategory_StandardStyle()
		{
			AssertCreatePricingPageWrapperCollection_CombinesEntriesByCategory(PricingPaginationStrategy.StandardStyle);
		}

		void AssertCreatePricingPageWrapperCollection_CombinesEntriesByCategory(PricingPaginationStrategy style)
		{
			var helper = new TestHelper(Factory);
			var quote = helper.NewQuote(helper.NewOrgHeader());
			quote.TH_PrintInheritedOriginCharges = true;
			quote.TH_PrintInheritedDestinationCharges = true;
			var refContainerPK = helper.Containers["20GP"].PK;

			foreach (var category in RatingConstants.RateCategory.RateCategories)
			{
				var entry = quote.AddRateEntry(category);
				entry.PageHeader = category;
				entry.RateLines.RemoveAndDeleteAll();

				var chargeCodeGroup = GetValidChargeGroupForCategory(category);
				var chargeCode = helper.ChargeCodes.New($"NEW{category}CHG", category, FlatCalculator.Code, chargeCodeGroup);
				var rateLine = entry.AddRateLine(chargeCode, currencyCode: Core.Constants.CurrencyCodes.Australia);
				rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

				if (category != RatingConstants.RateCategory.WHS && category != RatingConstants.RateCategory.CYD)
				{
					entry.TI_OriginLRC = "AU";
					entry.TI_DestinationLRC = "UA";
				}

				if (entry.IsSupplementaryEntry())
				{
					entry.TI_Mode = "ALL";
				}

				if (entry.IsFCL())
				{
					entry.TI_RC = refContainerPK;
				}
			}

			const string pagePerCategoryMessage = " Supplimentary Rate Entries should be combined.";
			var message = "Filter should only allow Forwarding Rate Entries." + pagePerCategoryMessage;
			var expected = new[]
				{
					RatingConstants.RateCategory.AIR,
					RatingConstants.RateCategory.LCL,
					RatingConstants.RateCategory.FCL,
				};

			AssertPricingPagesContains(message, expected, quote, style | PricingPaginationStrategy.ForwardingCategoryFilter);

			message = "Filter should only allow Shipping Rate Entries." + pagePerCategoryMessage;
			expected = new[]
				{
					RatingConstants.RateCategory.SCO,
					RatingConstants.RateCategory.SNC,
				};

			AssertPricingPagesContains(message, expected, quote, style | PricingPaginationStrategy.ShippingCategoryFilter);

			message = "Filter should only allow Shipping Detention Rate Entries." + pagePerCategoryMessage;
			expected = new[]
				{
					RatingConstants.RateCategory.SED,
					RatingConstants.RateCategory.SID,
				};

			AssertPricingPagesContains(message, expected, quote, style | PricingPaginationStrategy.ShippingDetentionCategoryFilter);

			message = "Filter should only allow CFS Rate Entries to be included." + pagePerCategoryMessage;
			expected = new[]
				{
					RatingConstants.RateCategory.PAC,
					RatingConstants.RateCategory.UNP,
					RatingConstants.RateCategory.CST,
				};

			AssertPricingPagesContains(message, expected, quote, style | PricingPaginationStrategy.CFSCategoryFilter);
		}

		#endregion

		#region No Combined Pricing Pages (Rate Entries cannot be mergeable due to unique Origin and Destinations)

		public void TestCreatePricingPageWrapperCollection_FiltersEntriesByCategory_LandscapeCompactStyle()
		{
			AssertCreatePricingPageWrapperCollection_FiltersEntriesByCategory(PricingPaginationStrategy.LandscapeCompactStyle);
		}

		public void TestCreatePricingPageWrapperCollection_FiltersEntriesByCategory_LandscapeComplexStyle()
		{
			AssertCreatePricingPageWrapperCollection_FiltersEntriesByCategory(PricingPaginationStrategy.LandscapeComplexStyle);
		}

		public void TestCreatePricingPageWrapperCollection_FiltersEntriesByCategory_LandscapeSimpleStyle()
		{
			AssertCreatePricingPageWrapperCollection_FiltersEntriesByCategory(PricingPaginationStrategy.LandscapeSimpleStyle);
		}

		public void TestCreatePricingPageWrapperCollection_FiltersEntriesByCategory_StandardStyle()
		{
			AssertCreatePricingPageWrapperCollection_FiltersEntriesByCategory(PricingPaginationStrategy.StandardStyle);
		}

		void AssertCreatePricingPageWrapperCollection_FiltersEntriesByCategory(PricingPaginationStrategy style)
		{
			var helper = new TestHelper(Factory);
			var quote = helper.NewQuote(helper.NewOrgHeader());
			quote.TH_PrintInheritedOriginCharges = true;
			quote.TH_PrintInheritedDestinationCharges = true;

			foreach (var category in RatingConstants.RateCategory.RateCategories)
			{
				var entry = quote.AddRateEntry(category);
				entry.PageHeader = category;
				entry.RateLines.RemoveAndDeleteAll();

				var chargeCodeGroup = GetValidChargeGroupForCategory(category);
				var chargeCode = helper.ChargeCodes.New($"NEW{category}CHG", category, FlatCalculator.Code, chargeCodeGroup);
				var rateLine = entry.AddRateLine(chargeCode, currencyCode: Core.Constants.CurrencyCodes.Australia);
				rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;

				if (category != RatingConstants.RateCategory.WHS && category != RatingConstants.RateCategory.CYD)
				{
					entry.TI_OriginLRC = LoadOrCreateUNLOCOForCode(category, true);
					entry.TI_DestinationLRC = LoadOrCreateUNLOCOForCode(category, false);
				}
			}

			var message = "Without a category filter no rates should be loaded from PricingPageSetWrapper.";
			var expected = Array.Empty<string>();

			AssertPricingPagesContains(message, expected, quote, style);

			const string pagePerCategoryMessage = " Because each Rate Entry on this Quote has a different Origin and Destination, none of the Supplimentary Rate Entries should be combined.";
			message = "Filter should only allow Forwarding Rate Entries." + pagePerCategoryMessage;
			expected = new[]
				{
					RatingConstants.RateCategory.AIR,
					RatingConstants.RateCategory.LCL,
					RatingConstants.RateCategory.FCL,
					RatingConstants.RateCategory.ORG,
					RatingConstants.RateCategory.DST,
				};

			AssertPricingPagesContains(message, expected, quote, style | PricingPaginationStrategy.ForwardingCategoryFilter);

			message = "Filter should only allow Shipping Rate Entries." + pagePerCategoryMessage;
			expected = new[]
				{
					RatingConstants.RateCategory.SCO,
					RatingConstants.RateCategory.SNC,
					RatingConstants.RateCategory.SOR,
					RatingConstants.RateCategory.SDE,
				};

			AssertPricingPagesContains(message, expected, quote, style | PricingPaginationStrategy.ShippingCategoryFilter);

			message = "Filter should only allow Shipping Detention Rate Entries." + pagePerCategoryMessage;
			expected = new[]
				{
					RatingConstants.RateCategory.SED,
					RatingConstants.RateCategory.SID,
				};

			AssertPricingPagesContains(message, expected, quote, style | PricingPaginationStrategy.ShippingDetentionCategoryFilter);

			message = "Filter should only allow CFS Rate Entries to be included." + pagePerCategoryMessage;
			expected = new[]
				{
					RatingConstants.RateCategory.PAC,
					RatingConstants.RateCategory.UNP,
					RatingConstants.RateCategory.CST,
				};

			AssertPricingPagesContains(message, expected, quote, style | PricingPaginationStrategy.CFSCategoryFilter);
		}

		#endregion

		void AssertPricingPagesContains(string message, string[] expected, RatingHeader quote, PricingPaginationStrategy strategy)
		{
			var wrapper = new PricingPageSetWrapper(quote, strategy, Factory);
			var entries = wrapper.PricingPages.Cast<PricingPageWrapper>().SelectMany(x => x.Entries);
			var actual = entries.OfType<RatingEntryWrapper>().Select(e => e.PageHeader).ToArray();

			AssertContainsExactElementsInAnyOrder(message, expected, actual);
		}

		string LoadOrCreateUNLOCOForCode(string category, bool isLocal)
		{
			var country = isLocal
				? Core.Constants.CountryCodes.Australia
				: Core.Constants.CountryCodes.Tuvalu;

			var code = country + category;
			var result = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, code);
			if (result == null)
			{
				result = Factory.NewWithValidTestData<RefUNLOCO>();
				result.RL_Code = code;
			}

			return code;
		}

		ZString GetValidChargeGroupForCategory(string category)
		{
			switch (category)
			{
				case RatingConstants.RateCategory.AIR:
				case RatingConstants.RateCategory.FCL:
				case RatingConstants.RateCategory.LCL:
				case RatingConstants.RateCategory.CAI:
				case RatingConstants.RateCategory.CFC:
				case RatingConstants.RateCategory.CLC:
				case RatingConstants.RateCategory.SCO:
				case RatingConstants.RateCategory.SNC:
					return ChargeCodeGroupList.Codes.Freight;

				case RatingConstants.RateCategory.ORG:
				case RatingConstants.RateCategory.COR:
				case RatingConstants.RateCategory.SOR:
				case RatingConstants.RateCategory.SID:
					return ChargeCodeGroupList.Codes.Origin;

				case RatingConstants.RateCategory.DST:
				case RatingConstants.RateCategory.CDS:
				case RatingConstants.RateCategory.SDE:
				case RatingConstants.RateCategory.SED:
					return ChargeCodeGroupList.Codes.Destination;

				case RatingConstants.RateCategory.CST:
					return ChargeCodeGroupList.Codes.ContainerStorage;

				case RatingConstants.RateCategory.PAC:
				case RatingConstants.RateCategory.UNP:
					return ChargeCodeGroupList.Codes.CFSShipment;

				case RatingConstants.RateCategory.TRN:
					return ChargeCodeGroupList.Codes.Transport;

				case RatingConstants.RateCategory.TBC:
					return ChargeCodeGroupList.Codes.TransportBooking;

				case RatingConstants.RateCategory.WHS:
					return ChargeCodeGroupList.Codes.WHSStorage;

				case RatingConstants.RateCategory.TRW:
					return ChargeCodeGroupList.Codes.TRWReceive;

				case RatingConstants.RateCategory.TWU:
					return ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit;

				case RatingConstants.RateCategory.CYD:
					return ChargeCodeGroupList.Codes.YardStorage;

				case RatingConstants.RateCategory.CYU:
					return ChargeCodeGroupList.Codes.YardTransportationUnitGateIn;

				case RatingConstants.RateCategory.CYM:
					return ChargeCodeGroupList.Codes.MNRWorkOrderHeader;

				default:
					throw new ArgumentException("Please check PricingPageCollection.Load expects this new Rate Category.");
			}
		}

		#endregion

		#region Implementation

		static string Render(PricingPageSetWrapper wrapper)
		{
			if (wrapper == null)
			{
				return "<NULL>";
			}
			else
			{
				List<string> lines = new List<string>();

				foreach (PricingPageWrapper page in wrapper.PricingPages)
				{
					lines.Add(Render(page));
				}

				lines.Sort();

				StringBuilder builder = new StringBuilder();

				foreach (string line in lines)
				{
					builder.Append(line);
				}

				return builder.ToString();
			}
		}

		static string Render(PricingPageWrapper wrapper)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine();
			builder.AppendLine("[PricingPage]");

			builder.AppendLine("Entry:");
			AppendLines(builder, wrapper.Entries);

			builder.AppendLine("Origin:");
			AppendLines(builder, wrapper.OriginRates);

			builder.AppendLine("Freight:");
			AppendLines(builder, wrapper.FreightRates);

			builder.AppendLine("Destination:");
			AppendLines(builder, wrapper.DestinationRates);

			return builder.ToString();
		}

		static string Render(PricingPageCompoundLineWrapperCollection collection)
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendLine();

			foreach (PricingPageCompoundLineWrapper wrapper in collection)
			{
				builder.Append('[');
				builder.Append(wrapper.Page.IndexNum);
				builder.Append(',');
				builder.Append(wrapper.LabelOrdinal);
				builder.Append('-');
				builder.Append(wrapper.Label);
				builder.Append(',');
				builder.Append(wrapper.SubLabel);
				builder.Append('-');
				builder.Append(wrapper.SubLabelOrdinal);
				builder.AppendLine("]");

				if (wrapper.Row != null)
				{
					builder.Append("  [Row ");
					builder.Append(wrapper.Row.Index);
					builder.Append(", ");
					builder.Append(wrapper.Row.Entries[0].Origin.Code);
					builder.Append(" => ");
					builder.Append(wrapper.Row.Entries[0].Destination.Code);
					builder.AppendLine("]");
				}

				if (wrapper.SubRow != null)
				{
					builder.Append("  [SubRow ");
					builder.Append(wrapper.SubRow.Index);
					builder.Append(", ");
					builder.Append(wrapper.SubRow.Currency == null ? "<null>" : wrapper.SubRow.Currency.Code.ToString());
					builder.Append(", ");
					builder.Append(wrapper.SubRow.Charge == null ? "<null>" : wrapper.SubRow.Charge.Code.ToString());
					builder.AppendLine("]");
				}

				if (wrapper.RateLine != null)
				{
					builder.Append("  [Line ");
					builder.Append(wrapper.RateLine.SetIndexNum);
					builder.Append(',');
					builder.Append(wrapper.RateLine.LineIndexNum);
					builder.Append("] ");
					builder.Append(wrapper.RateLine.Description);
					builder.Append('|');
					builder.Append(wrapper.RateLine.Currency);
					builder.Append('|');
					builder.Append(wrapper.RateLine.Amount);
					builder.Append('|');
					builder.AppendLine(wrapper.RateLine.Units);
				}
			}

			return builder.ToString();
		}

		static void AppendLines(StringBuilder builder, RatingEntryWrapperCollection collection)
		{
			List<string> lines = new List<string>();

			foreach (RatingEntryWrapper entry in collection)
			{
				lines.Add(string.Format("  {0} -> {1}: {2}\r\n", entry.Origin.Name, entry.Destination.Name, entry.Mode));
			}

			lines.Sort();

			AppendLines(builder, lines);
		}

		static void AppendLines(StringBuilder builder, PricingPageLineWrapperCollection collection)
		{
			List<string> lines = new List<string>();

			foreach (PricingPageLineWrapper line in collection)
			{
				lines.Add(string.Format("  {0}|{1}|{2}|{3}|{4} -> {5}\r\n", line.Description, line.Currency, line.Amount, line.Units, line.Origin.Name, line.Destination.Name));
			}

			lines.Sort();

			AppendLines(builder, lines);
		}

		static void AppendLines(StringBuilder builder, IEnumerable<string> lines)
		{
			foreach (string line in lines)
			{
				builder.Append(line);
			}
		}

		static CompanyTariff GenerateMegaTariff(BusinessObjectFactory factory)
		{
			string local = "AU";
			string[] allPorts = new string[] { "NL", "SG", "AU" };

			CompanyTariff header = factory.New<CompanyTariff>();

			foreach (string port in allPorts)
			{
				if (port != local)
				{
					AddFreightEntries(header, local, port, factory);
					AddFreightEntries(header, port, local, factory);
				}

				AddPortEntries(header, port, factory);
			}

			return header;
		}

		static void AddFreightEntries(RatingHeader header, string origin, string destination, BusinessObjectFactory factory)
		{
			RateEntry entry;

			AddUnitCN(header, RatingConstants.RateCategory.SCO, "SEA", origin, destination, "20GP", "FRT", 101m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.SCO, "SEA", origin, destination, "20RE", "FRT", 102m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.SCO, "SEA", origin, destination, "40GP", "FRT", 103m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.SCO, "SEA", origin, destination, "40RE", "FRT", 104m, factory);

			entry = AddUnitM3(header, RatingConstants.RateCategory.SNC, "LCL", origin, destination, "FRT", 111m, 200);
			AddPercentage(entry, "BAF", 2.5m, "FRT");

			AddUnitCN(header, RatingConstants.RateCategory.FCL, "SEA", origin, destination, "20GP", "FRT", 121m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.FCL, "SEA", origin, destination, "20RE", "FRT", 122m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.FCL, "SEA", origin, destination, "40GP", "FRT", 123m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.FCL, "SEA", origin, destination, "40RE", "FRT", 124m, factory);

			entry = AddUnitM3(header, RatingConstants.RateCategory.LCL, "LCL", origin, destination, "FRT", 131m, 200);
			AddPercentage(entry, "BAF", 2.5m, "FRT");

			AddUnitM3(header, RatingConstants.RateCategory.AIR, "LSE", origin, destination, "FRT", 141m, 200);

			AddUnitCN(header, RatingConstants.RateCategory.AIR, "ULD", origin, destination, "LD-3", "FRT", 161m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.AIR, "ULD", origin, destination, "LD-6", "FRT", 162m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.AIR, "ULD", origin, destination, "LD-9", "FRT", 163m, factory);
		}

		static void AddPortEntries(RatingHeader header, string port, BusinessObjectFactory factory)
		{
			AddFlat(header, RatingConstants.RateCategory.SOR, "ALL", port, "", "ODOC", 11m);
			AddFlat(header, RatingConstants.RateCategory.SDE, "ALL", "", port, "DDOC", 12m);

			AddFlat(header, RatingConstants.RateCategory.ORG, "ALL", port, "", "ODOC", 21m);
			AddFlat(header, RatingConstants.RateCategory.DST, "ALL", "", port, "DDOC", 22m);

			AddUnitCN(header, RatingConstants.RateCategory.PAC, "ALL", port, "", "20GP", "CFSPACK", 31m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.PAC, "ALL", port, "", "20RE", "CFSPACK", 32m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.PAC, "ALL", port, "", "40GP", "CFSPACK", 33m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.PAC, "ALL", port, "", "40RE", "CFSPACK", 34m, factory);

			AddUnitCN(header, RatingConstants.RateCategory.UNP, "ALL", "", port, "20GP", "CFSUNPA", 41m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.UNP, "ALL", "", port, "20RE", "CFSUNPA", 42m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.UNP, "ALL", "", port, "40GP", "CFSUNPA", 43m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.UNP, "ALL", "", port, "40RE", "CFSUNPA", 44m, factory);

			AddUnitCN(header, RatingConstants.RateCategory.SED, "SEA", "", port, "20GP", "OCDET", 61m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.SED, "SEA", "", port, "20RE", "OCDET", 62m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.SED, "SEA", "", port, "40GP", "OCDET", 63m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.SED, "SEA", "", port, "40RE", "OCDET", 64m, factory);

			AddUnitCN(header, RatingConstants.RateCategory.SID, "SEA", port, "", "20GP", "DCDET", 51m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.SID, "SEA", port, "", "20RE", "DCDET", 52m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.SID, "SEA", port, "", "40GP", "DCDET", 53m, factory);
			AddUnitCN(header, RatingConstants.RateCategory.SID, "SEA", port, "", "40RE", "DCDET", 54m, factory);
		}

		static RateEntry AddUnitCN(RatingHeader header, string category, string mode, string origin, string destination, string containerType, string charge, decimal amount, BusinessObjectFactory factory)
		{
			var containerPK = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerType).PK;

			RateEntry entry = header.ChildRateEntries
							.Cast<RateEntry>()
							.SingleOrDefault(x => x.TI_RateCategory == category
									 && x.TI_Mode == mode
									 && x.TI_OriginLRC == origin
									 && x.TI_DestinationLRC == destination
									 && x.TI_RC == containerPK);
			if (entry == null)
			{
				entry = header.AddRateEntry(category, mode, origin, destination, "", containerType);
				entry.RateLines.RemoveAndDeleteAll();
			}

			RateLine line = entry.AddRateLine(charge, UnitCalculator.Code, RatingConstants.Units.CN, "AUD");
			line.TL_RateDesc = charge + " charge";
			((UnitCalculator)line.Calculator).PerUnit = amount;

			return entry;
		}

		static RateEntry AddUnitM3(RatingHeader header, string category, string mode, string origin, string destination, string charge, decimal amount, decimal min)
		{
			RateEntry entry = header.ChildRateEntries
							.Cast<RateEntry>()
							.SingleOrDefault(x => x.TI_RateCategory == category
									 && x.TI_Mode == mode
									 && x.TI_OriginLRC == origin
									 && x.TI_DestinationLRC == destination);
			if (entry == null)
			{
				entry = header.AddRateEntry(category, mode, origin, destination, "", "");
				entry.RateLines.RemoveAndDeleteAll();
			}

			RateLine line = entry.AddRateLine(charge, MinimumOrPerUnitCalculator.Code, RatingConstants.Units.M3, "AUD");
			line.TL_RateDesc = charge + " charge";
			((MinimumOrPerUnitCalculator)line.Calculator).PerUnit = amount;
			((MinimumOrPerUnitCalculator)line.Calculator).Minimum = min;

			return entry;
		}

		static RateEntry AddFlat(RatingHeader header, string category, string mode, string origin, string destination, string charge, decimal amount)
		{
			RateEntry entry = header.ChildRateEntries
							.Cast<RateEntry>()
							.SingleOrDefault(x => x.TI_RateCategory == category
									 && x.TI_Mode == mode
									 && x.TI_OriginLRC == origin
									 && x.TI_DestinationLRC == destination);
			if (entry == null)
			{
				entry = header.AddRateEntry(category, mode, origin, destination, "", "");
				entry.RateLines.RemoveAndDeleteAll();
			}

			RateLine line = entry.AddRateLine(charge, FlatCalculator.Code, "", "AUD");
			line.TL_RateDesc = charge + " charge";
			((FlatCalculator)line.Calculator).BaseRate = amount;

			return entry;
		}

		static RateEntry AddPercentage(RateEntry entry, string charge, decimal percentage, string percentageOf)
		{
			RateLine line = entry.AddRateLine(charge, PercentageCalculator.Code, "", "AUD");
			line.TL_RateDesc = charge + " charge";

			PercentageCalculator calc = (PercentageCalculator)line.Calculator;
			calc.Percent = percentage;
			calc.AddApplyToItem(percentageOf);

			return entry;
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			return new PricingPageSetWrapper(tariff, PricingPaginationStrategy.ForwardingCategoryFilter | PricingPaginationStrategy.StandardStyle, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Pricing Page Set
======================================================================
Name                                    Type
----------------------------------------------------------------------

OriginAndDestinationRates               Compound Line Collection
OriginDestinationAndChargeableTableRows  Compound Line Collection
OriginDestinationAndContainerTableRows  Compound Line Collection
OriginDestinationAndFreightRates        Compound Line Collection
OriginDestinationAndFreightTableRows    Compound Line Collection
PricingPages                            Pricing Page Collection
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			PricingPage page = new PricingPage(tariff.AddRateEntry("LCL", "ALL", "AUBNE", "NLAMS"), Factory, PricingPageStyle.Standard);

			return new PricingPageSetWrapper(tariff, PricingPaginationStrategy.ForwardingCategoryFilter | PricingPaginationStrategy.StandardStyle, Factory);
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

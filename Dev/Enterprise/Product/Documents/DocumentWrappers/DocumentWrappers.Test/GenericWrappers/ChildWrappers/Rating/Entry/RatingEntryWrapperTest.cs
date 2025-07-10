using System;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[TestedType(typeof(RatingEntryWrapper))]
	sealed class RatingEntryWrapperTest : GenericWrapperTest
	{
		[TestDate(2010, 10, 11)]
		public override void TestWrapperMappingsEmpty()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry = tariff.AddRateEntry("SCO");
			entry.TI_RH_NKCommodityCode = "";

			RatingEntryWrapper wrapper = new RatingEntryWrapper(entry, Factory);

			CombineAssertions(delegate
			{
				AssertEquals("wrapper.PageHeader", "", wrapper.PageHeader);
				AssertEquals("wrapper.Mode", "FCL", wrapper.Mode);
				AssertEquals("wrapper.DiscountDescription", "FCL", wrapper.Mode);
				AssertEquals("wrapper.ValidFrom", new ZDateTime(2010, 10, 11, 0, 0, 0), wrapper.ValidFrom);
				AssertEquals("wrapper.ValidUntil", new ZDateTime(2011, 4, 11, 0, 0, 0), wrapper.ValidUntil);
				AssertEquals("wrapper.Origin.Code", "", wrapper.Origin.Code);
				AssertEquals("wrapper.Destination.Code", "", wrapper.Destination.Code);
				AssertEquals("wrapper.Via.Code", "", wrapper.Via.Code);
				AssertEquals("wrapper.Provider.CompanyName", "", wrapper.Provider.CompanyName);
				AssertEquals("wrapper.ServiceLevel.Code", "", wrapper.ServiceLevel.Code);
				AssertEquals("wrapper.CommodityCode", "", wrapper.CommodityCode.Code);
				AssertEquals("wrapper.Frequency.FrientlyText", "", wrapper.Frequency.FriendlyText);
				AssertEquals("wrapper.TransitTime.FriendlyText", "", wrapper.TransitTime.FriendlyText);
			});
		}

		public void TestIsSupplementary()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry freight = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "NLAMS", "AUBNE");
			RateEntry supplementary = tariff.AddRateEntry(RatingConstants.RateCategory.SOR, "ALL", "NLAMS", "AUBNE");

			AssertEquals("Freight", false, new RatingEntryWrapper(freight, Factory).IsSupplementary);
			AssertEquals("Supplementary", true, new RatingEntryWrapper(supplementary, Factory).IsSupplementary);
		}

		public void TestPostCodes()
		{
			ClientRate rate = Factory.New<ClientRate>();

			RateEntry entry = rate.AddRateEntry(RatingConstants.RateCategory.LCL);
			entry.TI_CartageDeliveryAddressPostCode = "1234";
			entry.TI_CartagePickupAddressPostCode = "4321";

			AssertEquals("DeliveryAddressPostCode", "1234", new RatingEntryWrapper(entry, Factory).DeliveryAddressPostCode);
			AssertEquals("PickUpAddressPostCode", "4321", new RatingEntryWrapper(entry, Factory).PickUpAddressPostCode);
		}

		public void TestTransportMode()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry airentry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "AIR", "NLAMS", "AUBNE");
			RateEntry seaentry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "SEA", "AUBNE", "AUSYD");
			RateEntry roadentry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "ROA", "AUSYD", "GBLON");
			RateEntry railentry = tariff.AddRateEntry(RatingConstants.RateCategory.ORG, "RAI", "GBLON", "NLAMS");

			AssertEquals("air - code", "AIR", new RatingEntryWrapper(airentry, Factory).TransportMode.Code);
			AssertEquals("sea - code", "SEA", new RatingEntryWrapper(seaentry, Factory).TransportMode.Code);
			AssertEquals("road - code", "ROA", new RatingEntryWrapper(roadentry, Factory).TransportMode.Code);
			AssertEquals("rai - code", "RAI", new RatingEntryWrapper(railentry, Factory).TransportMode.Code);

			AssertEquals("air - description", "Air", new RatingEntryWrapper(airentry, Factory).TransportMode.Description);
			AssertEquals("sea - description", "Sea", new RatingEntryWrapper(seaentry, Factory).TransportMode.Description);
			AssertEquals("road - description", "Road", new RatingEntryWrapper(roadentry, Factory).TransportMode.Description);
			AssertEquals("rai - descriptoion", "Rail", new RatingEntryWrapper(railentry, Factory).TransportMode.Description);
		}

		public void TestOverseasCountries()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			CombineAssertions(delegate
			{
				CompanyTariff tariff = Factory.New<CompanyTariff>();

				RateEntry impentry = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "NLAMS", "AUBNE");
				RateEntry domentry = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "AUSYD");
				RateEntry expentry = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUSYD", "GBLON");
				RateEntry crxentry = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "GBLON", "NLAMS");

				AssertEquals("imp", "Netherlands", new RatingEntryWrapper(impentry, Factory).OverseasCountries);
				AssertEquals("dom", "", new RatingEntryWrapper(domentry, Factory).OverseasCountries);
				AssertEquals("exp", "United Kingdom", new RatingEntryWrapper(expentry, Factory).OverseasCountries);
				AssertEquals("crx", "United Kingdom - Netherlands", new RatingEntryWrapper(crxentry, Factory).OverseasCountries);
			});
		}

		public void TestConsignorConsignee()
		{
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_Code = "CONSIGNOR";

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "CONSIGNEE";

			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry entry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL);
			entry.TI_OH_Consignor = consignor.PK;
			entry.TI_OH_Consignee = consignee.PK;

			RatingEntryWrapper wrapper = new RatingEntryWrapper(entry, Factory);

			AssertEquals("Consignor", "CONSIGNOR", wrapper.Consignor.CompanyCode);
			AssertEquals("Consignee", "CONSIGNEE", wrapper.Consignee.CompanyCode);
		}

		public void TestContractNumber()
		{
			var quote = Factory.New<Quote>();

			RateEntry entry = quote.AddRateEntry("SCO", "ALL", "AUBNE", "NLAMS");
			entry.TI_ContractNumber = "Contract1";

			var wrapper = new RatingEntryWrapper(entry, Factory);

			DocumentsDataRegistry.Instance.ShowContractNumbersOnShippingQuotationDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("Contract1", wrapper.ContractNumber);

			DocumentsDataRegistry.Instance.ShowContractNumbersOnShippingQuotationDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("", wrapper.ContractNumber);
		}

		public void TestDirection()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			CombineAssertions(delegate
			{
				CompanyTariff tariff = Factory.New<CompanyTariff>();

				RateEntry impentry = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "NLAMS", "AUBNE");
				RateEntry domentry = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUBNE", "AUSYD");
				RateEntry expentry = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "AUSYD", "GBLON");
				RateEntry crxentry = tariff.AddRateEntry(RatingConstants.RateCategory.SCO, "ALL", "GBLON", "NLAMS");

				AssertEquals("imp", "Import", new RatingEntryWrapper(impentry, Factory).Direction.Description);
				AssertEquals("dom", "Domestic", new RatingEntryWrapper(domentry, Factory).Direction.Description);
				AssertEquals("exp", "Export", new RatingEntryWrapper(expentry, Factory).Direction.Description);
				AssertEquals("crx", "Cross Trade", new RatingEntryWrapper(crxentry, Factory).Direction.Description);
			});
		}

		[SetOrgAllowMixedCase(true)]
		public void TestPopulated()
		{
			OrgHeader provider = Factory.New<OrgHeader>();
			provider.OH_Code = "PROVIDER";
			provider.OH_FullName = "Shipping Line";

			CompanyTariff tariff = Factory.New<CompanyTariff>();

			RateEntry entry = tariff.AddRateEntry("SCO", "ALL", "AUBNE", "NLAMS");
			entry.TI_ViaLRC = "SGSIN";
			entry.TI_QuotePageIncoTerm = "EXW";
			entry.TI_RS_NKServiceLevel_NI = "STD";
			entry.TI_RH_NKCommodityCode = "GEN";
			entry.TI_OH_TransportProvider = provider.PK;
			entry.TI_Frequency = 3;
			entry.TI_FrequencyUnit = FrequencyList.Codes.Days;
			entry.TI_TransitTime = "2";
			entry.TI_ContractNumber = "contract";

			RatingEntryWrapper wrapper = new RatingEntryWrapper(entry, Factory);

			CombineAssertions(delegate
			{
				AssertEquals("wrapper.PageHeader", "", wrapper.PageHeader);
				AssertEquals("wrapper.Mode", "FCL", wrapper.Mode);
				AssertEquals("wrapper.DiscountDescription", "FCL", wrapper.Mode);
				AssertEquals("wrapper.Origin.Code", "AUBNE", wrapper.Origin.Code);
				AssertEquals("wrapper.Destination.Code", "NLAMS", wrapper.Destination.Code);
				AssertEquals("wrapper.Via.Code", "SGSIN", wrapper.Via.Code);
				AssertEquals("wrapper.Provider.CompanyName", "Shipping Line", wrapper.Provider.CompanyName);
				AssertEquals("wrapper.ServiceLevel.Code", "STD", wrapper.ServiceLevel.Code);
				AssertEquals("wrapper.CommodityCode", "GEN", wrapper.CommodityCode.Code);
				AssertEquals("wrapper.Frequency.FrientlyText", "Every 3 Days", wrapper.Frequency.FriendlyText);
				AssertEquals("wrapper.TransitTime.FriendlyText", "2 Days", wrapper.TransitTime.FriendlyText);
				AssertEquals("wrapper.ContractNumber", "contract", wrapper.ContractNumber);
			});
		}

		public void TestQuoteValidDateRange()
		{
			var today = ZDate.Today;

			var header = Factory.New<Quote>();
			header.TH_QuoteDate = today.AddDays(1);
			header.TH_QuoteEndDate = today.AddDays(61);

			var entry = header.AddRateEntry("FCL", "ALL", "AUBNE", "NLAMS");
			entry.TI_RateStartDate = today.AddDays(2);
			entry.TI_RateEndDate = today.AddDays(62);

			var wrapper = new RatingEntryWrapper(entry, Factory);
			AssertEquals("Wrapper.ValidFrom", today.AddDays(1), wrapper.ValidFrom);
			AssertEquals("Wrapper.ValidUntil", today.AddDays(61), wrapper.ValidUntil);
		}

		public void TestNonQuoteValidDateRange()
		{
			var today = ZDate.Today;

			var header = Factory.New<CompanyTariff>();
			header.TH_QuoteDate = today.AddDays(1);
			header.TH_QuoteEndDate = today.AddDays(61);

			var entry = header.AddRateEntry("FCL", "ALL", "AUBNE", "NLAMS");
			entry.TI_RateStartDate = today.AddDays(2);
			entry.TI_RateEndDate = today.AddDays(62);

			var wrapper = new RatingEntryWrapper(entry, Factory);
			AssertEquals("Wrapper.ValidFrom", today.AddDays(2), wrapper.ValidFrom);
			AssertEquals("Wrapper.ValidUntil", today.AddDays(62), wrapper.ValidUntil);
		}

		public void TestPageHeader()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry = tariff.AddRateEntry(RatingConstants.RateCategory.FCL);
			RatingEntryWrapper wrapper = new RatingEntryWrapper(entry, Factory);
			Env.Registry.Rating.QuoteHeaderText = "Test Heading";

			AssertEquals(string.Empty, wrapper.PageHeader);

			entry.TI_PageHeading = "Heading";
			AssertEquals("Heading", wrapper.PageHeader);

			Quote newQuote = Factory.NewWithValidTestData<Quote>();
			QuoteEntry newEntry = (QuoteEntry)newQuote.AddRateEntry(RatingConstants.RateCategory.SCO);
			wrapper = new RatingEntryWrapper(newEntry, Factory);

			AssertEquals("Test Heading", wrapper.PageHeader);

			newEntry.TI_PageHeading = "Heading";
			AssertEquals("Heading", wrapper.PageHeader);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestLegacyDocumentDisplayCarrierNameRegistrySetting()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderRatingAndQuotationDocuments.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			OrgHeader airProvider = Factory.New<OrgHeader>();
			airProvider.OH_Code = "AIRPVD";
			airProvider.OH_FullName = "Air Line";

			OrgHeader seaProvider = Factory.New<OrgHeader>();
			seaProvider.OH_Code = "SEAPVD";
			seaProvider.OH_FullName = "Shipping Line";

			Quote newQuote = Factory.NewWithValidTestData<Quote>();
			QuoteEntry airEntry = (QuoteEntry)newQuote.AddRateEntry(RatingConstants.RateCategory.AIR);
			airEntry.TI_OH_TransportProvider = airProvider.PK;
			QuoteEntry seaEntry = (QuoteEntry)newQuote.AddRateEntry(RatingConstants.RateCategory.SCO);
			seaEntry.TI_OH_TransportProvider = seaProvider.PK;

			DocumentsDataRegistry.Instance.AirFreightIncludeTransportProviderOnQuotation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			DocumentsDataRegistry.Instance.SeaFreightIncludeTransportProviderOnQuotation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

			RatingEntryWrapper wrapper1 = new RatingEntryWrapper(airEntry, Factory);
			RatingEntryWrapper wrapper2 = new RatingEntryWrapper(seaEntry, Factory);

			AssertNull(wrapper1.Provider);
			AssertNull(wrapper2.Provider);

			DocumentsDataRegistry.Instance.AirFreightIncludeTransportProviderOnQuotation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			DocumentsDataRegistry.Instance.SeaFreightIncludeTransportProviderOnQuotation.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			wrapper1 = new RatingEntryWrapper(airEntry, Factory);
			wrapper2 = new RatingEntryWrapper(seaEntry, Factory);

			AssertEquals("Air Line", wrapper1.Provider.CompanyName);
			AssertEquals("Shipping Line", wrapper2.Provider.CompanyName);
		}

		public void TestMayGSTBeApplicable()
		{
			var creator = new TestObjectCreator(Factory);
			var tariff = Factory.New<CompanyTariff>();
			var freightEntry = tariff.AddRateEntry(RatingConstants.RateCategory.SNC, Core.Constants.RateMode.ALL, "NLAMS", "AUBNE");
			freightEntry.AddRateLine(CreateChargeCode(creator, false, ChargeCodeGroupList.Codes.Freight));

			AssertEquals(false, new RatingEntryWrapper(freightEntry, Factory).MayGSTBeApplicable);

			freightEntry.AddRateLine(CreateChargeCode(creator, true, ChargeCodeGroupList.Codes.Freight));

			AssertEquals(true, new RatingEntryWrapper(freightEntry, Factory).MayGSTBeApplicable);

			var originEntry = tariff.AddRateEntry(RatingConstants.RateCategory.SOR, Core.Constants.RateMode.ALL, "NLAMS", "AUBNE");
			originEntry.AddRateLine(CreateChargeCode(creator, false, ChargeCodeGroupList.Codes.Origin));

			AssertEquals(false, new RatingEntryWrapper(originEntry, Factory).MayGSTBeApplicable);

			originEntry.AddRateLine(CreateChargeCode(creator, true, ChargeCodeGroupList.Codes.Origin));

			AssertEquals(true, new RatingEntryWrapper(originEntry, Factory).MayGSTBeApplicable);
		}

		AccChargeCode CreateChargeCode(TestObjectCreator creator, bool isGstTaxable, string chargeGroup)
		{
			var taxRate = isGstTaxable ? creator.GST1 : creator.FREECAPGST;
			var code = isGstTaxable ? chargeGroup + "TAX" : chargeGroup + "NOT";
			var chargeCode = creator.CreateChargeCode(code, code, Constants.ChargeType.Margin, 100m, taxRate, null);
			chargeCode.AC_ChargeGroup = chargeGroup;
			chargeCode.AC_RateCalculator = FlatCalculator.Code;

			return chargeCode;
		}

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CommodityCode : GEN - General
Consignee : 
Consignor : 
Destination : 
Direction : Cross Trade
Frequency : 
Origin : 
Provider : 
Registry : (No Default Field Value Available on Registry)
ServiceLevel : 
TransitTime : 
TransportMode : SEA - Sea
Via :
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry = tariff.AddRateEntry("FCL");

			return new RatingEntryWrapper(entry, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
Rate Entry
======================================================================
Name                                    Type
----------------------------------------------------------------------
CommodityCode                           CodeAndDescription
ServiceLevel                            CodeAndDescription
TransportMode                           CodeAndDescription
Consignee                               Organisation
Consignor                               Organisation
Provider                                Organisation
Destination                             Rating Area
Origin                                  Rating Area
Via                                     Rating Area
Direction                               Rating Direction
Frequency                               Rating Frequency
TransitTime                             Rating Transit Time
ContractNumber                          String
DeliveryAddressPostCode                 String
DiscountDescription                     String
IsSupplementary                         Bool
Mode                                    String
OverseasCountries                       String
PageHeader                              String
PickUpAddressPostCode                   String
ValidFrom                               DateTime
ValidUntil                              DateTime
";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			CompanyTariff tariff = Factory.New<CompanyTariff>();
			RateEntry entry = tariff.AddRateEntry("FCL");

			return new RatingEntryWrapper(entry, Factory);
		}

		#endregion
	}
}

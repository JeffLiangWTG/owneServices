using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	sealed class DocEntryQuotationTest2 : TestCaseWithFactory
	{
		public void TestQuotationWithOriginDestinationChargesOnly()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			var org = SetupOrgHeader();

			Quote testQuote = Helper.NewQuote(org);

			RateEntry entry1 = testQuote.AddRateEntry("ORG", "FCL", "AUSYD", "");
			RateLine line1 = entry1.AddRateLine("OPCH", UnitCalculator.Code, QuantityUnit.CN);
			line1.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)90m;

			RateEntry entry2 = testQuote.AddRateEntry("DST", "LCL", "", "AUSYD");
			RateLine line2 = entry2.AddRateLine("DPCH", UnitCalculator.Code, QuantityUnit.M3);
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)10m;

			QuoteFormatEntryCollection formatEntries = testQuote.QuoteFormatEntries;
			formatEntries.LoadEntries();
			Factory.Save();

			formatEntries.Sort<QuoteEntry>((t1, t2) => -StringComparer.OrdinalIgnoreCase.Compare(t1.TI_RateCategory, t2.TI_RateCategory));

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(new PricingPage(formatEntries[0], Factory, PricingPageStyle.Standard), Factory);
			AssertEquals("FCL Freight from Sydney", testDocQuotation.PageHeading);
			AssertEquals(1, testDocQuotation.OriginDocRateLineItems.Count);
			AssertEquals("Origin Port Charges *|AUD|90.00|per Container", testDocQuotation.OriginDocRateLineItems[0].QuotationLine.ToString());
			AssertEquals(0, testDocQuotation.FreightDocRateLineItems.Count);
			AssertEquals(0, testDocQuotation.DestinationDocRateLineItems.Count);

			testDocQuotation = DocEntryQuotation.New(new PricingPage(formatEntries[1], Factory, PricingPageStyle.Standard), Factory);
			AssertEquals("LCL Freight to Sydney", testDocQuotation.PageHeading);
			AssertEquals(0, testDocQuotation.OriginDocRateLineItems.Count);
			AssertEquals(0, testDocQuotation.FreightDocRateLineItems.Count);
			AssertEquals(1, testDocQuotation.DestinationDocRateLineItems.Count);
			AssertEquals("Destination Port Charges *|AUD|10.00|per M3 / 1000 KG", testDocQuotation.DestinationDocRateLineItems[0].QuotationLine.ToString());
		}

		public void TestOverridenFCLFreightCharges()
		{
			RateEntry entry1 = Helper.NewCompanyTariff().AddRateEntry("FCL", "SEA", "AU", "US", "", "20GP");
			entry1.RateLines.RemoveAndDeleteAll();
			RateLine line1 = entry1.AddRateLine("FSC", FlatCalculator.Code);
			line1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;

			Quote quote = Helper.NewQuote(Helper.NewOrgHeader(1));
			RateEntry entry2 = quote.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX", "", "20GP");
			RateLine line2 = entry2.RateLines[0];
			line2.TL_RateCalculator = UnitCalculator.Code;
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1000m;
			RateLine line3 = entry2.AddRateLine("FSC", FlatCalculator.Code);
			line3.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)35m;

			Factory.Save();

			PricingPageCollection pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.StandardStyle);
			AssertEquals(1, pages.Count);

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(pages[0], Factory);
			AssertEquals(4, testDocQuotation.FreightDocRateLineItems.Count);
			AssertEquals("International Freight|||", testDocQuotation.FreightDocRateLineItems.Find(line2)[0].QuotationLine.ToString());
			AssertEquals("20GP|USD|1000.00|per Container", testDocQuotation.FreightDocRateLineItems.Find(line2)[1].QuotationLine.ToString());
			AssertEquals("Fuel Surcharge|||", testDocQuotation.FreightDocRateLineItems.Find(line3)[0].QuotationLine.ToString());
			AssertEquals("20GP|USD|35.00|", testDocQuotation.FreightDocRateLineItems.Find(line3)[1].QuotationLine.ToString());
		}

		public void TestOverridenULDFreightCharges()
		{
			RefContainer aIR1 = RefContainer.New(Factory);
			aIR1.RC_ShippingMode = "AIR";
			aIR1.RC_Code = "AIR1";

			RefContainer aIR2 = RefContainer.New(Factory);
			aIR2.RC_ShippingMode = "AIR";
			aIR2.RC_Code = "AIR2";

			Factory.Save();

			RateEntry entry1 = Helper.NewCompanyTariff().AddRateEntry("AIR", "ULD", "AU", "US", "", "AIR1");
			entry1.RateLines.RemoveAndDeleteAll();
			RateLine line1 = entry1.AddRateLine("FSC", FlatCalculator.Code);
			line1.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)50m;

			Quote quote = Helper.NewQuote(Helper.NewOrgHeader(1));

			RateEntry entry2 = quote.AddRateEntry("AIR", "ULD", "AUSYD", "USLAX", "", "AIR1");
			RateLine line2 = entry2.RateLines[0];
			line2.TL_RateCalculator = UnitCalculator.Code;
			line2.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)1000m;
			RateLine line3 = entry2.AddRateLine("FSC", FlatCalculator.Code);
			line3.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)35m;

			RateEntry entry3 = quote.AddRateEntry("AIR", "ULD", "AUSYD", "USLAX", "", "AIR2");
			RateLine line4 = entry3.RateLines[0];
			line4.TL_RateCalculator = UnitCalculator.Code;
			line4.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)740m;

			RateEntry entry4 = quote.AddRateEntry("ORG", "ULD", "AUSYD", "", "", "AIR1");
			RateLine line5 = entry4.AddRateLine("ODOC", FlatCalculator.Code);
			line5.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)100m;

			RateEntry entry5 = quote.AddRateEntry("ORG", "AIR", "AUSYD", "", "", "");
			RateLine line6 = entry5.AddRateLine("OCART", UnitCalculator.Code, QuantityUnit.CN);
			line6.Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)300m;

			Factory.Save();

			PricingPageCollection pages = new PricingPageCollection(quote);
			pages.Load(PricingPaginationStrategy.StandardStyle);
			AssertEquals(1, pages.Count);

			DocEntryQuotation testDocQuotation = DocEntryQuotation.New(pages[0], Factory);
			AssertEquals(5, testDocQuotation.FreightDocRateLineItems.Count);
			AssertEquals("International Freight|||", testDocQuotation.FreightDocRateLineItems.Find(line2)[0].QuotationLine.ToString());
			AssertEquals("AIR1|AUD|1000.00|per Container", testDocQuotation.FreightDocRateLineItems.Find(line2)[1].QuotationLine.ToString());
			AssertEquals("AIR2|AUD|740.00|per Container", testDocQuotation.FreightDocRateLineItems.Find(line4)[0].QuotationLine.ToString());
			AssertEquals("Fuel Surcharge|||", testDocQuotation.FreightDocRateLineItems.Find(line3)[0].QuotationLine.ToString());
			AssertEquals("AIR1|AUD|35.00|", testDocQuotation.FreightDocRateLineItems.Find(line3)[1].QuotationLine.ToString());

			AssertEquals(3, testDocQuotation.OriginDocRateLineItems.Count);
			AssertEquals("Origin Documentation Fee *|||", testDocQuotation.OriginDocRateLineItems[0].QuotationLine.ToString());
			AssertEquals("AIR1|AUD|100.00|", testDocQuotation.OriginDocRateLineItems[1].QuotationLine.ToString());
			AssertEquals("Pick Up Cartage *|AUD|300.00|per Container", testDocQuotation.OriginDocRateLineItems[2].QuotationLine.ToString());
		}

		public void TestOriginDestination()
		{
			var org = SetupOrgHeader();
			var testQuote = Helper.NewQuote(org);
			var entry = testQuote.AddRateEntryWithFlatRateLine("AIR", "LSE", "", "", "FRT", 10m);
			entry.TI_ViaLRC = "SGSIN";
			var formatEntries = testQuote.QuoteFormatEntries;
			formatEntries.LoadEntries();

			var testDocQuotation = DocEntryQuotation.New(new PricingPage(formatEntries[0], Factory, PricingPageStyle.Standard), Factory);

			AssertEquals("", testDocQuotation.OriginDestination);

			entry.TI_OriginLRC = "AUSYD";
			AssertEquals("from Sydney", testDocQuotation.OriginDestination);

			entry.TI_DestinationLRC = "USLAX";
			AssertEquals("Sydney to Los Angeles", testDocQuotation.OriginDestination);

			entry.TI_OriginLRC = "";
			AssertEquals("to Los Angeles", testDocQuotation.OriginDestination);
		}

		public OrgHeader SetupOrgHeader()
		{
			var org = Helper.NewOrgHeader();
			org.OH_IsConsignee = true;
			Factory.Save();
			return org;
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			originalAlternateRateFormatValue = DocumentsDataRegistry.Instance.AlternativeRateFormat.Value;
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var gst = Factory.LoadTop1<AccTaxRate>(new ZQuery(AccTaxRateSchema.AT_Code, "GST").AddToFilter(AccTaxRateSchema.AT_RN_NKCountry, Core.Constants.CountryCodes.Australia));
			if (gst != null)
			{
				gst.SetRate_ForTestOnly(10, 1);

				Factory.Save();
			}
		}

		protected override void TearDown()
		{
			DocumentsDataRegistry.Instance.AlternativeRateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalAlternateRateFormatValue);
			base.TearDown();
		}

		TestHelper Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new TestHelper(Factory);
				}

				return helper;
			}
		}
		TestHelper helper;

		bool originalAlternateRateFormatValue;

		#endregion
	}
}

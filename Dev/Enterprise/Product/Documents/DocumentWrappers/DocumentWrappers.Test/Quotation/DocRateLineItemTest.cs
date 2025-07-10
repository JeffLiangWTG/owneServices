using System;
using System.Text;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	[TestedType(typeof(DocRateLineItem))]
	public class DocRateLineItemTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
				{
					DocRateLineItem.New(QuotationLine.Spacing(Line), Quotation, Factory)
				};
		}

		public void TestDescription()
		{
			AccChargeCode code = Factory.New<AccChargeCode>();
			code.AC_Code = "BOB";
			code.AC_Desc = "charge description";

			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());

			var categories = new[]
			{
				new { Code = RatingConstants.RateCategory.SID, Expected = "charge description", Load = "", Discharge = "AU" },
				new { Code = RatingConstants.RateCategory.SED, Expected = "charge description", Load = "AU", Discharge = "" },
				new { Code = RatingConstants.RateCategory.SCO, Expected = "rate description" , Load = "AU", Discharge = "NZ" },
			};

			StringBuilder builder = new StringBuilder();

			var collections = new[]
			{
				new { Name = "Origin", Getter = new Converter<DocEntryQuotation, DocQuotationLineCollection>((q) => q.OriginDocRateLineItems) },
				new { Name = "Destination", Getter = new Converter<DocEntryQuotation, DocQuotationLineCollection>((q) => q.DestinationDocRateLineItems) },
				new { Name = "Freight", Getter = new Converter<DocEntryQuotation, DocQuotationLineCollection>((q) => q.FreightDocRateLineItems) },
			};

			for (int i = 0; i < categories.Length; i++)
			{
				RateEntry entry = quote.AddRateEntry(categories[i].Code, "SEA", categories[i].Load, categories[i].Discharge);

				RateLine line = entry.RateLines.AddNew();
				line.TL_AC = code.PK;
				line.TL_RateDesc = "rate description";
				line.TL_WeightVolume = "D";
				line.TL_RateCalculator = FlatCalculator.Code;
				line.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)200m;

				PricingPage page = new PricingPage(entry, entry.Factory, PricingPageStyle.Standard);

				DocEntryQuotation quotation = DocEntryQuotation.New(page, Factory);

				for (int j = 0; j < collections.Length; j++)
				{
					DocQuotationLineCollection collection = collections[j].Getter(quotation);

					if (collection.Count > 0)
					{
						builder.AppendLine();
						builder.Append('[');
						builder.Append(categories[i].Code);
						builder.Append(" - ");
						builder.Append(collections[j].Name);
						builder.AppendLine("]");

						foreach (DocRateLineItem item in collection)
						{
							builder.Append('|');
							builder.Append(item.Description);
							builder.AppendLine("|");
						}
					}
				}
			}

			const string expected = @"
[SID - Destination]
|charge description|

[SED - Origin]
|charge description|

[SCO - Freight]
|rate description|
";
			AssertMultilineASCIIEquals("", expected, builder.ToString());
		}

		public void TestLocalOverseasChargesHeadings()
		{
			ZString storedCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry("AU");
				GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUMEL";

				OrgHeader testOrg = Helper.NewOrgHeader();
				Quote testQuote = Helper.NewQuote(testOrg);

				RateEntry entry1 = testQuote.AddRateEntry("ORG", "AIR", "AUSYD", "");
				RateLine line1 = entry1.AddRateLine("ODOC");
				line1.GetCalculator<FlatCalculator>().BaseRate = 15m;

				RateEntry entry2 = testQuote.AddRateEntry("ORG", "AIR", "USLAX", "");
				RateLine line2 = entry2.AddRateLine("ODOC");
				line2.GetCalculator<FlatCalculator>().BaseRate = 25m;

				RateEntry entry3 = testQuote.AddRateEntry("DST", "AIR", "", "AUSYD");
				RateLine line3 = entry3.AddRateLine("DDOC");
				line3.GetCalculator<FlatCalculator>().BaseRate = 35m;

				RateEntry entry4 = testQuote.AddRateEntry("DST", "AIR", "", "USLAX");
				RateLine line4 = entry4.AddRateLine("DDOC");
				line4.GetCalculator<FlatCalculator>().BaseRate = 45m;

				RateEntry entry5 = testQuote.AddRateEntry("DST", "AIR", "", "USSFO");
				RateLine line5 = entry5.AddRateLine("DDOC");
				line5.GetCalculator<FlatCalculator>().BaseRate = 55m;
				entry5.TI_OH_TransportProvider = testOrg.PK;

				DocEntryQuotation quotation = DocEntryQuotation.New(new PricingPage(entry1, entry1.Factory, PricingPageStyle.Standard), Factory);
				AssertEquals("Origin Charges - Sydney", quotation.OriginDocRateLineItems[0].LocalChargesHeading);

				quotation = DocEntryQuotation.New(new PricingPage(entry2, entry2.Factory, PricingPageStyle.Standard), Factory);
				AssertEquals("Origin Charges - Los Angeles", quotation.OriginDocRateLineItems[0].OverseasChargesHeading);

				quotation = DocEntryQuotation.New(new PricingPage(entry3, entry3.Factory, PricingPageStyle.Standard), Factory);
				AssertEquals("Destination Charges - Sydney", quotation.DestinationDocRateLineItems[0].LocalChargesHeading);

				quotation = DocEntryQuotation.New(new PricingPage(entry4, entry4.Factory, PricingPageStyle.Standard), Factory);
				AssertEquals("Destination Charges - Los Angeles", quotation.DestinationDocRateLineItems[0].OverseasChargesHeading);

				quotation = DocEntryQuotation.New(new PricingPage(entry5, entry5.Factory, PricingPageStyle.Standard), Factory);
				AssertEquals(string.Format("Destination Charges - San Francisco ({0})", testOrg.OH_FullName), quotation.DestinationDocRateLineItems[0].OverseasChargesHeading);

				entry1.Delete();
				entry2.Delete();
				entry3.Delete();
				entry4.Delete();
				entry5.Delete();

				RateEntry entry6 = testQuote.AddRateEntry("DST", "AIR", "", "");
				entry6.TI_IsCrossTrade = true;
				RateLine line6 = entry6.AddRateLine("DDOC");
				line6.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)65m;

				RateEntry entry7 = testQuote.AddRateEntry("DST", "AIR", "", "");
				entry7.TI_IsCrossTrade = true;
				RateLine line7 = entry7.AddRateLine("DDOC");
				line7.Calculator[Calculator.Items.Operator.BAS] = (ZDecimal)75m;

				OrgHeader testOrg2 = Helper.NewOrgHeader();
				entry7.TI_OH_TransportProvider = testOrg2.PK;

				quotation = DocEntryQuotation.New(new PricingPage(entry6, entry6.Factory, PricingPageStyle.Standard), Factory);
				AssertEquals("Destination Charges - Cross Trade", quotation.DestinationDocRateLineItems[0].OverseasChargesHeading);

				quotation = DocEntryQuotation.New(new PricingPage(entry7, entry7.Factory, PricingPageStyle.Standard), Factory);
				AssertEquals(string.Format("Destination Charges - Cross Trade ({0})", testOrg2.OH_FullName), quotation.DestinationDocRateLineItems[0].OverseasChargesHeading);
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(storedCountry);
			}
		}

		public void TestWarehouse()
		{
			DocRateLineItem item = DocRateLineItem.New(QuotationLine.Spacing(Line), Quotation, Factory);
			AssertEquals("All Warehouses", item.Warehouse);

			Line.Parent.TI_WW_Warehouse = Helper.NewWarehouse().PK;
			AssertEquals("WHS1", item.Warehouse);
		}

		public void TestWarehouse_Translatable()
		{
			var docRateLineItem = DocRateLineItem.New(QuotationLine.Spacing(Line), Quotation, Factory);
			AssertEquals("All Warehouses", docRateLineItem.Warehouse);

			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_WarehouseName = "Test Warehouse";
			Line.Parent.TI_WW_Warehouse = warehouse.PK;
			AssertEquals("Warehouse in English.", "Test Warehouse", docRateLineItem.Warehouse);

			var resKey = warehouse.WW_WarehouseNameInfo.CustomizableDataResourceStrings.GetMultilingualString(warehouse, "Test Warehouse").ResourceKey;
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "测试"));
				AssertEquals("Warehouse in Chinese.", "测试", docRateLineItem.Warehouse);
			}
		}

		public void TestProduct()
		{
			DocRateLineItem item = DocRateLineItem.New(QuotationLine.Spacing(Line), Quotation, Factory);
			AssertEquals("", item.Product);

			Line.TL_OP_ProductNumber = Helper.NewOrgSupplierPart(Line.Parent.Parent.Header).PK;
			AssertEquals("PROD1 (###1)", item.Product);
		}

		public void TestContractNumber()
		{
			RateEntry entry = Helper.NewQuote(Helper.NewOrgHeader()).AddRateEntry("SCO", "SEA", "AUSYD", "USLAX");
			DocRateEntry docEntry = DocRateEntry.New(entry, Factory);

			AssertEquals(ZString.Empty, docEntry.ContractNumber);

			entry.TI_ContractNumber = null;
			AssertEquals(docEntry.ContractNumber, "");

			entry.TI_ContractNumber = "12345";
			AssertEquals("Wrong contract number", "12345", docEntry.ContractNumber);

			entry.TI_ContractNumber = "TestString";
			AssertEquals("TestString", docEntry.ContractNumber);
		}

		#region Implementation

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocRateLineItem.New(QuotationLine.Spacing(Line), Quotation, Factory);
		}

		DocEntryQuotation Quotation
		{
			get
			{
				if (fQuotation == null)
				{
					PricingPage page = new PricingPage(Line.Parent, Line.Factory, PricingPageStyle.Standard);
					fQuotation = DocEntryQuotation.New(page, Factory);
				}

				return fQuotation;
			}
		}

		DocEntryQuotation fQuotation;

		RateLine Line
		{
			get
			{
				if (fLine == null)
				{
					Quote testQuote = Helper.NewQuote(Helper.NewOrgHeader());
					fLine = testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").RateLines[0];
				}

				return fLine;
			}
		}

		RateLine fLine;

		TestHelper Helper
		{
			get
			{
				if (fHelper == null)
				{
					fHelper = new TestHelper(Factory);
				}

				return fHelper;
			}
		}

		TestHelper fHelper;

		#endregion
	}
}

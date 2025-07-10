using System;
using System.Collections.Generic;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Quotation.Testing
{
	sealed class DocQuotationTest : TestCaseWithFactory
	{
		public void TestTaxMessageInClosingText()
		{
			var britishBranch = Factory.NewWithValidTestData<GlbBranch>();
			britishBranch.GB_RL_NKHomePort = "GBLON";
			britishBranch.GB_GC = GlbCompany.CurrentCompany.PK;

			var vatTaxRate = Factory.NewWithValidTestData<AccTaxRate>();
			vatTaxRate.AT_Code = "VAT";
			vatTaxRate.AT_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			vatTaxRate.SetRateNumerator_ForTestOnly(20);

			var vatChargeCode = Helper.ChargeCodes.New("DVAT", "DST Charge with VAT", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			vatChargeCode.AC_AT_GSTRate = vatTaxRate.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.GS_LoginName, britishBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var client = Helper.NewOrgHeader();
				var quote = Helper.NewQuote(client);
				var notTaxQuoteEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "", "CN", "FRT", 90);
				var notTaxDocQuotation = DocQuotation.New(new PricingPage(notTaxQuoteEntry, Factory, PricingPageStyle.Standard), Factory);

				Assert("FRT charge is not linked to a tax charge so no message is expected", notTaxDocQuotation.PageClosingText.IsEmpty);

				var taxableQuoteEntry = quote.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, Core.Constants.RateMode.LSE, "", "AUSYD", "DSEC", 25);
				var taxableDocQuotation = DocQuotation.New(new PricingPage(taxableQuoteEntry, Factory, PricingPageStyle.Standard), Factory);

				AssertEquals("A local Value Added Tax charge (equivalent to GST) may apply to all items marked with an asterisk (*).", taxableDocQuotation.PageClosingText);
			}
		}

		public void TestClientNameAndAddress()
		{
			OrgHeader testClient = Factory.New<OrgHeader>();
			testClient.OH_FullName = "Some really random client";
			testClient.MainAddress.OA_Address1 = "123 Fake Street";
			testClient.MainAddress.OA_City = "Sydney";
			testClient.MainAddress.OA_State = "NSW";
			testClient.MainAddress.OA_PostCode = "2000";
			testClient.OH_RL_NKClosestPort = "AUMEL";
			testClient.OH_IsForwarder = true;

			OrgAddress orgAddress = testClient.Addresses.AddNew();
			orgAddress.OA_Address1 = "SECOND ADDRESS";
			orgAddress.OA_City = "City";
			orgAddress.OA_State = "WSN";
			orgAddress.OA_PostCode = "3000";

			Factory.Save();

			Quote quote = Factory.New<Quote>();
			quote.TH_OH = testClient.PK;
			RateEntry testEntry = quote.AddRateEntry("AIR", "LSE", "INBOM", GlbBranch.CurrentBranch.GB_RL_NKHomePort == "AUMEL" ? "AUSYD" : "AUMEL");
			DocQuotation docQuotation = DocQuotation.New(new PricingPage(testEntry, testEntry.Factory, PricingPageStyle.Standard), Factory);

			AssertEquals("SOME REALLY RANDOM CLIENT\n123 FAKE STREET\nSYDNEY VIC 2000\nAUSTRALIA", docQuotation.Client.PostalAddress);
		}

		public void TestInvoiceTermsText()
		{
			OrgHeader client = Factory.New<OrgHeader>();
			Quote quote = Factory.New<Quote>();
			quote.TH_OH = client.PK;
			RateEntry entry = quote.AddRateEntry("AIR");
			DocQuotation docQuotation = DocQuotation.New(new PricingPage(entry, Factory, PricingPageStyle.Standard), Factory);
			AssertEquals("Please be advised that our payment terms are as follows:\r\n1. CASH ON DELIVERY\r\nDisbursement item(s): \r\n1. CASH ON DELIVERY", docQuotation.InvoiceTermsText);
		}

		public void TestClientNameAndAddress_Override()
		{
			Quote quote = Factory.New<Quote>();
			quote.QuotationClientAddress.E2_AddressOverride = true;
			quote.QuotationClientAddress.E2_CompanyName = "Some Company";
			quote.QuotationClientAddress.E2_Address1 = "Address 1";
			quote.QuotationClientAddress.E2_City = "Prospect";
			quote.QuotationClientAddress.E2_State = "VIC";
			quote.QuotationClientAddress.E2_Postcode = "3149";
			quote.QuotationClientAddress.E2_RN_NKCountryCode = "AU";
			quote.QuotationClientAddress.E2_Contact = "Mary Mary";

			RateEntry entry = quote.AddRateEntry("AIR", "LSE", "INBOM", GlbBranch.CurrentBranch.GB_RL_NKHomePort == "AUMEL" ? "AUSYD" : "AUMEL");
			PricingPage page = new PricingPage(entry, entry.Factory, PricingPageStyle.Standard);
			DocQuotation docQuotation = DocQuotation.New(page, Factory);

			AssertEquals("SOME COMPANY\nADDRESS 1\nPROSPECT VIC 3149\nAUSTRALIA", docQuotation.Client.PostalAddress);
		}

		public void TestDocTypeExists()
		{
			Quote quote = Factory.New<Quote>();
			RateEntry entry = quote.AddRateEntry("AIR", "LSE", "", "");
			PricingPage page = new PricingPage(entry, entry.Factory, PricingPageStyle.Standard);
			var testDocQuotation = (IDocTypeCode)DocQuotation.New(page, Factory);
			var docType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, testDocQuotation.DocTypeCode));

			AssertNotNull("Quotation Doc Type for eDocs is valid and exists in table RefDocType", docType);
		}

		public void TestLogo_ForQuotations()
		{
			var client = Helper.NewOrgHeader();
			var quote = Helper.NewQuote(client);

			TestLogo_WithRate(quote);
		}

		public void TestLogo_ForClientRates()
		{
			var client = Helper.NewOrgHeader();
			var clientRate = Helper.NewClientRate(client);

			TestLogo_WithRate(clientRate);
		}

		public void TestLogo_ForGlobalClientRates()
		{
			var client = Helper.NewOrgHeader();
			var globalClientRate = Helper.NewGlobalClientRate(client);

			TestLogo_WithRate(globalClientRate);
		}

		void TestLogo_WithRate(RatingHeader header)
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			var entry = header.AddRateEntry("AIR", "LSE", "", "");
			var page = new PricingPage(entry, entry.Factory, PricingPageStyle.Standard);
			var quote = DocQuotation.New(page, Factory);

			Dictionary<string, object> constants = new Dictionary<string, object>();
			constants.Add(DocumentEngineIntegration.Constants.TemplateDefined.ContactType, "CNE");
			quote.SetTemplateConstants(constants);

			SystemDataRegistry.Instance.CompanyLogo.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new Bitmap(10, 10));
			Env.Registry.QuotationDocumentLogo = new Bitmap(20, 20);

			DocumentsDataRegistry.Instance.EnableClientBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);
			var collection = new ClientTariffAndLevelCollection();
			var element0 = collection.AddNew();
			element0.Code = "0";
			element0.Description = (NoResString)"Desc 0";
			element0.Image = new Bitmap(01, 01);
			element0.CodeList.AddPair("1", "Desc 1");
			element0.CodeList.AddPair("2", "Desc 2");
			element0.BrandName = "blah";
			element0.BrandEmailAddress = "blah@blah.com";

			var element1 = collection.AddNew();
			element1.Code = "1";
			element1.Description = (NoResString)"Desc 1";
			element1.Image = new Bitmap(11, 11);
			element1.CodeList.AddPair("1", "Desc 1");
			element1.CodeList.AddPair("2", "Desc 2");
			element1.BrandName = "blah";
			element1.BrandEmailAddress = "blah@blah.com";

			var element2 = collection.AddNew();
			element2.Code = "2";
			element2.Description = (NoResString)"Desc 2";
			element2.Image = new Bitmap(22, 22);
			element2.CodeList.AddPair("1", "Desc 1");
			element2.CodeList.AddPair("2", "Desc 2");
			element2.BrandName = "blah";
			element2.BrandEmailAddress = "blah@blah.com";

			DocumentsDataRegistry.Instance.ClientTariffAndLevels.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, collection);

			Env.Registry.GlobalTariffDefault = 2;
			AssertEquals("Precondition: Current Organisation should have no RateTariffLevels default described.", 0, header.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).RateTariffLevels.Count);
			AssertEquals("Precondition: Logo should match the image with code matching registry default.", 22, quote.Logo.Size.Height);
			AssertEquals("Precondition: Logo should match the image with code matching registry default.", 22, quote.Logo.Size.Width);

			header.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);
			AssertEquals("Logo should match the image with code '0' from ClientTariffAndLevel Collection.", 1, quote.Logo.Size.Height);
			AssertEquals("Logo should match the image with code '0' from ClientTariffAndLevel Collection.", 1, quote.Logo.Size.Width);

			header.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);
			AssertEquals("Logo should match the image with code '1' from ClientTariffAndLevel Collection.", 11, quote.Logo.Size.Height);
			AssertEquals("Logo should match the image with code '0' from ClientTariffAndLevel Collection.", 11, quote.Logo.Size.Width);

			header.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 2);
			AssertEquals("Logo should match the image with code '2' from ClientTariffAndLevel Collection.", 22, quote.Logo.Size.Height);
			AssertEquals("Logo should match the image with code '0' from ClientTariffAndLevel Collection.", 22, quote.Logo.Size.Width);

			header.Header.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany).RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 3);
			AssertEquals("Logo should load the default image from registry when no code matches Rate Tariff Levels value.", 20, quote.Logo.Size.Height);
			AssertEquals("Logo should match the image with code '0' from ClientTariffAndLevel Collection.", 20, quote.Logo.Size.Width);
		}

		public void TestCFX()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			Quote quote = Helper.NewQuote(Helper.NewOrgHeader());
			quote.TH_AirCFX = 1m;
			quote.TH_SeaCFX = 2m;
			quote.TH_ExportAirCFX = 3m;
			quote.TH_ExportSeaCFX = 4m;

			DocQuotation quotation = DocQuotation.New(new PricingPage(quote.AddRateEntry("AIR", "LSE", "USLAX", "AUSYD"), Factory, PricingPageStyle.Standard), Factory);

			AssertEquals(0, quotation.CFX.Count);

			Env.Registry.Rating.IncludeCFXOnQuote = true;
			AssertEquals(1, quotation.CFX.Count);
			AssertCFS(quotation, 0, "Import - Air", 1);

			quotation = DocQuotation.New(new PricingPage(quote.AddRateEntry("LCL", "LCL", "USLAX", "AUMEL"), Factory, PricingPageStyle.Standard), Factory);
			AssertEquals(1, quotation.CFX.Count);
			AssertCFS(quotation, 0, "Import - Sea", 2);

			quotation = DocQuotation.New(new PricingPage(quote.AddRateEntry("ORG", "AIR", "AUSYD", "USLAX"), Factory, PricingPageStyle.Standard), Factory);
			AssertEquals(1, quotation.CFX.Count);
			AssertCFS(quotation, 0, "Export - Air", 3);

			quotation = DocQuotation.New(new PricingPage(quote.AddRateEntry("DST", "SEA", "AUSYD", "USLAX"), Factory, PricingPageStyle.Standard), Factory);
			AssertEquals(1, quotation.CFX.Count);
			AssertCFS(quotation, 0, "Export - Sea", 4);
		}

		public void TestSignatures_Quote()
		{
			OrgHeader org1 = Helper.NewOrgHeader();
			var rep = Factory.New<GlbStaff>();
			rep.GS_FullName = "Test Rep";
			rep.GS_Code = "TR";
			org1.StaffAssignments.OverallSalesRep = rep.GS_Code;

			Quote testQuote = Helper.NewQuote(org1);
			DocQuotation quotation = DocQuotation.New(new PricingPage(testQuote.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX"), Factory, PricingPageStyle.Standard), Factory);

			AssertEquals(rep.GS_FullName, quotation.SalesRep.FullName);

			testQuote.TH_GS_NKSecondSignatory = GlbStaff.CurrentUser.GS_Code;

			testQuote.TH_GS_NKFirstSignatory = ZString.Empty;
			AssertEquals("Takes default rep from Staff assignments", rep.GS_FullName, quotation.SalesRep.FullName);
		}

		public void TestSignatures_NonQuote()
		{
			OrgHeader org1 = Helper.NewOrgHeader();
			var rep = Factory.New<GlbStaff>();
			rep.GS_FullName = "Test Rep";
			rep.GS_Code = "TR";

			ClientRate testRate = Helper.NewClientRate(org1);

			DocQuotation quotation = DocQuotation.New(new PricingPage(testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX"), Factory, PricingPageStyle.Standard), Factory);

			AssertNull(quotation.SalesRep);

			org1.StaffAssignments.OverallSalesRep = rep.GS_Code;
			quotation = DocQuotation.New(new PricingPage(testRate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX"), Factory, PricingPageStyle.Standard), Factory);
			AssertEquals(rep.GS_FullName, quotation.SalesRep.FullName);
		}

		#region Implementation

		void AssertCFS(DocQuotation quotation, int index, ZString name, ZDecimal value)
		{
			int i = 0;

			foreach (BusinessObject bo in quotation.CFX)
			{
				if (i++ == index)
				{
					AssertEquals(name, bo["Name"]);
					AssertEquals(value, bo["Value"]);
					return;
				}
			}

			Assert(false);
		}

		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;

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

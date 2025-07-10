using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
	{
		public void TestInvoice()
		{
			var parent = Factory.New<JobComInvoiceHeader>();
			AssertEquals(parent.Lookups.Invoice, parent);
		}

		public void TestValuationCodeList()
		{
			var valuationCodeList = (CodeDescriptionPairList)invoice.Lookups.ValuationCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(7, valuationCodeList.Count);
				AssertEquals("10, 20, 30, 4A, 4B, 50, 60", valuationCodeList.CodesAsString);
			});
		}
		public void TestJZ_IncoTerm_List()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var incoTermsCodeList = invoice.Lookups.JZ_IncoTerm_List;
			CombineAssertions(() =>
			{
				AssertEquals(17, incoTermsCodeList.Count);
				AssertEquals("CFR, CIF, CIN, CIP, CPT, DAF, DAP, DAT, DDP, DDU, DEQ, DES, DPU, EXW, FAS, FCA, FOB", incoTermsCodeList.CodesAsString);
			});

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			incoTermsCodeList = invoice.Lookups.JZ_IncoTerm_List;
			CombineAssertions(() =>
			{
				AssertEquals(17, incoTermsCodeList.Count);
				AssertEquals("CFR, CIF, CIN, CIP, CPT, DAF, DAP, DAT, DDP, DDU, DEQ, DES, DPU, EXW, FAS, FCA, FOB", incoTermsCodeList.CodesAsString);
			});

			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			incoTermsCodeList = invoice.Lookups.JZ_IncoTerm_List;
			CombineAssertions(() =>
			{
				AssertEquals(4, incoTermsCodeList.Count);
				AssertEquals("EXW, FAS, FCA, FOB", incoTermsCodeList.CodesAsString);
			});
		}

		public void TestInvoicePaymentTermCodeList()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var paymentTermCodeList = invoice.Lookups.InvoicePaymentTermCodeList;
			CombineAssertions(() =>
			{
				AssertEquals(11, paymentTermCodeList.Count);
				AssertEquals("CD, DA, DP, GN, GO, LH, LS, LU, PT, TT, WK", paymentTermCodeList.CodesAsString);
			});
		}

		public void TestJZ_WeightUQ_List()
		{
			var weightUnitList = invoice.Lookups.JZ_WeightUQ_List;

			CombineAssertions(() =>
			{
				AssertEquals(14, weightUnitList.Count);
				AssertEquals("DT, G, HG, KG, KT, LB, LT, MC, MG, OT, OZ, T, TL, TN", weightUnitList.CodesAsString);
			});
		}

		public void TestSupportingDocumentCodeList()
		{
			var documentList = invoice.Lookups.LocalExportDocumentTypeList;

			CombineAssertions(() =>
			{
				AssertEquals(9, documentList.Count);
				AssertEquals("01, 02, 03, 04, 05, 06, 07, 08, 99", documentList.CodesAsString);
			});
		}

		public void TestOnlineTradeTypeList()
		{
			var onlineTradeTypeList = invoice.Lookups.OnlineTradeTypeList;

			AssertEquals(4, onlineTradeTypeList.Count);
			AssertEquals("A, B, C, Z", onlineTradeTypeList.CodesAsString);
		}

		public void TestAllListAboutCertificateOfOrigin()
		{
			var issuedCodeList = invoice.Lookups.CertificateOfOriginIssuedCodeList;
			var determinationRuleCodeList = invoice.Lookups.CountryOfOriginDeterminationRuleCodeList;
			var splitCodeList = invoice.Lookups.CertificateOfOriginSplitCodeList;

			CombineAssertions(() =>
			{
				AssertEquals(2, issuedCodeList.Count);
				AssertEquals("N, Y", issuedCodeList.CodesAsString);
				AssertEquals(CertificateOfOriginIssuedCodeList.Descriptions.N, issuedCodeList[0].Description);

				AssertEquals(12, determinationRuleCodeList.Count);
				AssertEquals("2, 4, 6, 8, A, B, C, D, E, F, G, H", determinationRuleCodeList.CodesAsString);

				AssertEquals(2, splitCodeList.Count);
				AssertEquals("N, Y", splitCodeList.CodesAsString);
				AssertEquals(CertificateOfOriginSplitCodeList.Descriptions.N, splitCodeList[0].Description);
			});
		}

		public void TestYesNoCodeList()
		{
			var ynCodeList = invoice.Lookups.YesNoCodeList;

			CombineAssertions(() =>
			{
				AssertEquals(2, ynCodeList.Count);
				AssertEquals("N, Y", ynCodeList.CodesAsString);
			});
		}

		public void TestCostRateCodeList()
		{
			var costRateCodeList = invoice.Lookups.CostRateCodeList;

			AssertEquals(2, costRateCodeList.Count);
			AssertEquals("1, 2", costRateCodeList.CodesAsString);
		}

		public void TestSpecialRelationshipCodeList()
		{
			var specialRelationshipCodeList = invoice.Lookups.SpecialRelationshipCodeList;
			AssertEquals(8, specialRelationshipCodeList.Count);
			AssertEquals("01, 02, 03, 04, 05, 06, 07, 08", specialRelationshipCodeList.CodesAsString);
		}

		public void TestValuationDeclarationPricingMethodsList()
		{
			var valuationDeclarationPricingMethodsList = invoice.Lookups.ValuationDeclarationPricingMethodsList;
			AssertEquals(8, valuationDeclarationPricingMethodsList.Count);
			AssertEquals("01, 02, 03, 04, 05, 06, 07, 99", valuationDeclarationPricingMethodsList.CodesAsString);
		}

		public void TestSpecificUseProductTypeList()
		{
			var specificUseProductTypeList = invoice.Lookups.SpecificUseProductTypeList;
			AssertEquals(3, specificUseProductTypeList.Count);
			AssertEquals("10, 11, 99", specificUseProductTypeList.CodesAsString);
		}

		public void TestCustomsOfficeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(ZZ.NKCodeType.CustomsOffice, "Customs Office");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.KoreaSouth, "South Korea");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsOffice, "010", "서울세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsOffice, "012", "성남세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			helper.CreateCusCodeList(Core.Constants.CountryCodes.KoreaSouth, ZZ.NKCodeType.CustomsOffice, "013", "인천공항세관", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var customsOfficeList = invoice.Lookups.CustomsOfficeList;
			customsOfficeList.Load();
			AssertEquals(3, customsOfficeList.Count);

			Assert(customsOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "010"));
			Assert(customsOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "서울세관"));
			Assert(customsOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "012"));
			Assert(customsOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "성남세관"));
			Assert(customsOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Code == "013"));
			Assert(customsOfficeList.Cast<ZZRefCusCodeListCombined>().Any(x => x.ZZD_Description == "인천공항세관"));
		}

		public void TestGoodsDestinationType()
		{
			AssertType<RefCountryCollection>("GoodsDestination", invoice.Lookups.GoodsDestination);
		}

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
		}
		JobComInvoiceHeader invoice;
	}
}

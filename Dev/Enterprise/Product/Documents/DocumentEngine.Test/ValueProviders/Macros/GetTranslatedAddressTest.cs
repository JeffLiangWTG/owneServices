using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(GetTranslatedAddress))]
	sealed class GetTranslatedAddressTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("GetTranslatedAddress", Passes.FirstPass));
			Assert("Should not pass", !ValueProviderToTest.IsResponsibleForReplacing("<GetTranslatedAddress meh>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetTranslatedAddress(MainAddress.PK,ENG)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetTranslatedAddress(ABC,ABC)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetTranslatedAddress(ABC,ABC,ABC)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("<GetTranslatedAddress(ABC, AB)>", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< GETTranslatedAddress ( ABC , ABC ) >", Passes.FirstPass));
			Assert("Should pass", ValueProviderToTest.IsResponsibleForReplacing("< GETTranslatedAddress ( ABC , ABC , ABC ) >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var factory = new BusinessObjectFactory();
			var testOrgHeader = factory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;
			var testAddress1 = testOrgHeader.Addresses.AddNew();
			testAddress1.OA_Language = Core.Constants.Languages.English;
			testAddress1.OA_Address1 = "5th road";
			testAddress1.OA_Address2 = "Mascot";
			testAddress1.OA_City = "Sydney";
			testAddress1.OA_PostCode = "2100";
			testAddress1.OA_State = "NSW";
			testAddress1.OA_RN_NKCountryCode = "AU";
			testAddress1.OA_RL_NKRelatedPortCode = "AUSYD";
			factory.Save();

			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Address", testAddress1));
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;

			AssertEquals("5TH ROAD\nMASCOT\nSYDNEY NSW 2100\nAUSTRALIA", ValueProviderToTest.GetReplacement($"<GetTranslatedAddress({testAddress1.PK},{Core.Constants.Languages.English})>", Report));
			AssertEquals("5TH ROAD\nMASCOT\nSYDNEY NSW 2100\nAUSTRALIA", ValueProviderToTest.GetReplacement($"<GetTranslatedAddress({testAddress1.PK},{Core.Constants.Languages.ChineseSimplified})>", Report));

			var translatedAddress = testAddress1.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Language = Core.Constants.Languages.ChineseSimplified;
			translatedAddress.OTA_CompanyName = "慧咨科技";
			translatedAddress.OTA_Address1 = "第五大道";
			translatedAddress.OTA_Address2 = "麦斯考特";
			translatedAddress.OTA_City = "悉尼";
			translatedAddress.OTA_State = "新南威尔士";
			translatedAddress.OTA_PostCode = "2100";
			factory.Save();
			AssertEquals("5TH ROAD\nMASCOT\nSYDNEY NSW 2100\nAUSTRALIA", ValueProviderToTest.GetReplacement($"<GetTranslatedAddress({testAddress1.PK},{Core.Constants.Languages.English})>", Report));
			AssertEquals("慧咨科技\n第五大道\n麦斯考特\n悉尼 新南威尔士 2100\n澳大利亚", ValueProviderToTest.GetReplacement($"<GetTranslatedAddress({testAddress1.PK},{Core.Constants.Languages.ChineseSimplified})>", Report));

			var country = factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));
			country.RN_AddressFormattingRule = CountryAddressFormattingRuleList.Codes.CityStatePostcodeNoCountry;
			factory.Save();
			AssertEquals("慧咨科技\n第五大道\n麦斯考特\n悉尼\n新南威尔士\n2100", ValueProviderToTest.GetReplacement($"<GetTranslatedAddress({testAddress1.PK},{Core.Constants.Languages.ChineseSimplified})>", Report));

			Pack.Language = Core.Constants.Languages.ChineseSimplified;
			AssertEquals("慧咨科技\n第五大道\n麦斯考特\n悉尼\n新南威尔士\n2100", ValueProviderToTest.GetReplacement($"<GetTranslatedAddress({testAddress1.PK}, DocumentLanguage)>", Report));

			AssertEquals("慧咨科技", ValueProviderToTest.GetReplacement($"<GetTranslatedAddress( {testAddress1.PK} , {Core.Constants.Languages.ChineseSimplified} , CompanyName )>", Report));
			AssertEquals("第五大道", ValueProviderToTest.GetReplacement($"<GetTranslatedAddress( {testAddress1.PK} , {Core.Constants.Languages.ChineseSimplified} , AddressLine1 )>", Report));
			AssertEquals("麦斯考特", ValueProviderToTest.GetReplacement($"<GetTranslatedAddress( {testAddress1.PK} , {Core.Constants.Languages.ChineseSimplified} , AddressLine2 )>", Report));
			AssertEquals("悉尼", ValueProviderToTest.GetReplacement($"<GetTranslatedAddress( {testAddress1.PK} , {Core.Constants.Languages.ChineseSimplified} , City )>", Report));
			AssertEquals("新南威尔士", ValueProviderToTest.GetReplacement($"<GetTranslatedAddress( {testAddress1.PK} , {Core.Constants.Languages.ChineseSimplified} , State )>", Report));
			AssertEquals("2100", ValueProviderToTest.GetReplacement($"<GetTranslatedAddress( {testAddress1.PK} , {Core.Constants.Languages.ChineseSimplified} , PostCode )>", Report));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new GetTranslatedAddress();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			var factory = new BusinessObjectFactory();
			var testOrgHeader = factory.NewWithValidTestData(typeof(OrgHeader)) as OrgHeader;
			var testAddress1 = testOrgHeader.Addresses.AddNew();
			testAddress1.OA_Language = Core.Constants.Languages.English;
			testAddress1.OA_Address1 = "5th road";
			testAddress1.OA_Address2 = "Mascot";
			testAddress1.OA_City = "Sydney";
			testAddress1.OA_PostCode = "2100";
			testAddress1.OA_State = "NSW";
			testAddress1.OA_RN_NKCountryCode = "AU";
			testAddress1.OA_RL_NKRelatedPortCode = "AUSYD";

			var translatedAddress = testAddress1.TranslatedAddresses.AddNew();
			translatedAddress.OTA_Language = Core.Constants.Languages.ChineseSimplified;
			translatedAddress.OTA_Address1 = "第五大道";
			translatedAddress.OTA_Address2 = "麦斯考特";
			translatedAddress.OTA_City = "悉尼";
			translatedAddress.OTA_State = "新南威尔士";
			translatedAddress.OTA_PostCode = "2100";
			factory.Save();

			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("AddressPK", testAddress1.PK));
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
		}
	}
}

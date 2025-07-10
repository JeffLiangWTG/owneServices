using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocumentWrapperHelper))]
sealed class DocumentWrapperHelperTest : TestCaseWithFactory
{
	public void TestEmptyIfNull() => CombineAssertions(() =>
	{
		var collection = new[] { "1", "2", "3" };
		AssertSame("Not empty", collection, collection.EmptyIfNull());
		collection = Array.Empty<string>();
		AssertSame("Empty", collection, collection.EmptyIfNull());
		collection = null;
		AssertEquals("Null", 0, collection.EmptyIfNull().Count());
	});

	public void TestTaxesAndFeesLookup() => CombineAssertions(() =>
	{
		RefCusCodeTestHelper.CreateRateCodeList(Factory, RateTypes.AdditionalTaxes, new[] { "200", "299" }, includeTranslations: true);
		RefCusCodeTestHelper.CreateRateCodeList(Factory, RateTypes.AdditionalFees, new[] { "300", "399" }, includeTranslations: true);

		using var resCacheFR = Res.GetLanguageInstance(Factory.GetDocumentLanguage(SwissCustomsLanguageList.Codes.French)).UseMockData();
		resCacheFR.PutString("FAAC5187-EF80-4ABA-92F2-56962AA805CA", "DutiesFR");

		var lookup = Factory.GetTaxesAndFeesCodeList(Factory.GetDocumentLanguage("EN"));
		AssertContainsExactElementsInAnyOrder(new[] { "110", "200", "299", "300", "399" }, lookup.GetAllCodes());
		AssertEquals("Description Duty", "Duties", lookup.GetDescriptionFromCode(AdditionalTaxesTypes.Duty));
		AssertEquals("Description ADT", "ADT200", lookup.GetDescriptionFromCode("200"));
		AssertEquals("Description FEE", "FEE300", lookup.GetDescriptionFromCode("300"));

		lookup = Factory.GetTaxesAndFeesCodeList(Factory.GetDocumentLanguage(SwissCustomsLanguageList.Codes.French));
		AssertEquals("Description FR Duty", "DutiesFR", lookup.GetDescriptionFromCode(AdditionalTaxesTypes.Duty));
		AssertEquals("Description FR ADT", "ADT200FR", lookup.GetDescriptionFromCode("200"));
		AssertEquals("Description FR FEE", "FEE300FR", lookup.GetDescriptionFromCode("300"));
	});

	public void TestGetVATSuffixText() => CombineAssertions(() =>
	{
		AssertEquals("true", "VAT", DocumentWrapperHelper.GetVATSuffixText(true));
		AssertEquals("false", ZString.Empty, DocumentWrapperHelper.GetVATSuffixText(false));
	});

	public void TestGetDocumentLanguage() => CombineAssertions(() =>
	{
		AssertLanguage("de", "DE-DE");
		AssertLanguage("fr", "FR-FR");
		AssertLanguage("it", "IT-IT");
		AssertLanguage("en", "EN-US");
		AssertLanguage("de-CH", "DE-DE");
		AssertLanguage("fr-CH", "FR-FR");
		AssertLanguage("it-CH", "IT-IT");
		AssertLanguage("FR", "FR-FR");
		AssertLanguage("it-ch-ti", "IT-IT");
		AssertLanguage(string.Empty, ZString.Empty);
		AssertLanguage(null, ZString.Empty);

		void AssertLanguage(string providedCustomsLanguage, ZString expectedCargoWiseLanguage)
		{
			AssertEquals($"customsLanguage={providedCustomsLanguage}", expectedCargoWiseLanguage, Factory.GetDocumentLanguage(providedCustomsLanguage));
		}
	});

	public void TestGetDocumentLanguageLocal()
	{
		AssertDocumentLanguage("DE", "DE-CH");
		AssertDocumentLanguage("FR", "FR-CH");
		AssertDocumentLanguage("IT", "IT-CH");

		void AssertDocumentLanguage(string customsLanguage, ZString expectedLanguage)
		{
			var localLanguage = Factory.New<IRefLocalLanguage>();
			localLanguage.RA_Code = customsLanguage;
			localLanguage.RA_RN_NKCountryCode = Core.Constants.CountryCodes.Switzerland;
			localLanguage.RA_Description = "Swiss " + customsLanguage;
			localLanguage.RA_IsActive = true;
			localLanguage.RA_RA_ParentLanguage = Factory.LoadTop1<IRefLocalLanguage>(
				new ZQuery(RefLocalLanguageSchema.RA_Code, customsLanguage).AddToFilter(RefLocalLanguageSchema.RA_RN_NKCountryCode, customsLanguage)).PK;
			Factory.Save();
			AssertEquals($"customsLanguage={customsLanguage}", expectedLanguage, Factory.GetDocumentLanguage(customsLanguage));
		}
	}

	public void TestGetLanguageCode() => CombineAssertions(() =>
	{
		AssertLanguageCode("DE-CH", "DE");
		AssertLanguageCode("DE", "DE");
		AssertLanguageCode("X", "X");
		AssertLanguageCode(ZString.Empty, ZString.Empty);

		void AssertLanguageCode(ZString documentLanguage, string expectedLanguageCode)
		{
			AssertEquals($"DocumentLanguager={documentLanguage}", expectedLanguageCode, DocumentWrapperHelper.GetLanguageCode(documentLanguage));
		}
	});
}

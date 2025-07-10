using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BE.Business.Testing;

sealed class LookupsHelperTest : TestCaseWithFactory
{
	public void TestGetBELanguageList()
	{
		var languageList = LookupsHelper.GetBELanguageList(Factory);
		CombineAssertions(() =>
		{
			AssertSame("Cached", languageList, LookupsHelper.GetBELanguageList(Factory));

			AssertEquals("NL, FR, DE, EN", languageList.CodesAsString);

			AssertEquals("Dutch", languageList.GetDescriptionFromCode("NL"));
			AssertEquals("French", languageList.GetDescriptionFromCode("FR"));
			AssertEquals("German", languageList.GetDescriptionFromCode("DE"));
			AssertEquals("English", languageList.GetDescriptionFromCode("EN"));
		});
	}
}

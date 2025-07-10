using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.BE.NCTS.Business.Testing;

sealed class NctsHeaderValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckBH_CommunicationLanguage()
	{
		ValidationTestHelper.AssertErrorIfInvalidCode(Factory.New<NctsHeader>().BH_CommunicationLanguageInfo, "XX", "NL");
	}
}

using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.Testing
{
	class EURefCusMapperTest : TestCaseWithFactory
	{
		public void TestMapCW1AuthorisationCodeToCustomsCode()
		{
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
			helper.CreateCusMap("EUNAU", "SAS", "C515", startDate, endDate, "EUN");
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("When CW1 Authorization Code has a RefCusCodeMap, CustomsCode", "C515", EURefCusMapper.MapCW1AuthorisationCodeToCustomsCode(Factory, "SAS"));
				AssertEquals("When CW1 Authorization Code is empty, CustomsCode", "", EURefCusMapper.MapCW1AuthorisationCodeToCustomsCode(Factory, ""));
				AssertEquals("When CW1 Authorization Code does not have a RefCusCodeMap, CustomsCode", "XXX", EURefCusMapper.MapCW1AuthorisationCodeToCustomsCode(Factory, "XXX"));
			});
		}
	}
}

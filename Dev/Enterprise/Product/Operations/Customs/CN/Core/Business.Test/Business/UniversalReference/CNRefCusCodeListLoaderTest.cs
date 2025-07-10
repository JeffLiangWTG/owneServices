using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNRefCusCodeListLoaderTest : TestCaseWithFactory
	{
		public void TestGetCNCodeByType()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CURR", "Currencies");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "CURR", "USD", "USD currency", new ZDateTime(2018, 1, 1), new ZDateTime(2018, 12, 31));
			Factory.Save();
			AssertEquals("USD currency", CNRefCusCodeListLoader.GetCNCodeByType(Factory, "USD", "CURR", new ZDateTime(2018, 10, 1)).ZZD_Description);
			AssertNull("return null for CNY", CNRefCusCodeListLoader.GetCNCodeByType(Factory, "CNY", "CURR", new ZDateTime(2018, 10, 1)));
			AssertNull("return null for empty code", CNRefCusCodeListLoader.GetCNCodeByType(Factory, "", "CURR", new ZDateTime(2018, 10, 1)));
		}
	}
}

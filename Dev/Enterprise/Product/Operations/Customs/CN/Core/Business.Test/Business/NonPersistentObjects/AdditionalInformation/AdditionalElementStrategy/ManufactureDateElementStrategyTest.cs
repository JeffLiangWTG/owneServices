using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class ManufactureDateElementStrategyTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2203000000", "ef374a392b7934885636cee2f907884a");
			helper.CreateAdditionalElement("ef374a392b7934885636cee2f907884a", "生产日期");
			Factory.Save();
			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "ef374a392b7934885636cee2f907884a", ZDateTime.Today);
			var addInfo = new AdditionalElementWrapper(additionalElement, "2022-02-11;2022-02-13", new DummyAdditionalInformationWrapperParent(Factory), EnteringOrExiting.Entering);
			addInfo.RunPreSaveValidation();
			AssertHasWarningContaining(addInfo.ElementValueInfo, "The date should in format 'yyyyMMdd'");
			addInfo.ElementValue = "20220211;20220213";
			AssertNoWarningContaining(addInfo.ElementValueInfo, "The date should in format 'yyyyMMdd'");
		}

		public void TestIsValidManufactureDateString()
		{
			AssertEquals(false, ManufactureDateElementStrategy.IsValidManufactureDateString("2022-02-11;2022-02-13"));
			AssertEquals(true, ManufactureDateElementStrategy.IsValidManufactureDateString("20220211;20220213"));
			AssertEquals(false, ManufactureDateElementStrategy.IsValidManufactureDateString("2022-02-11"));
			AssertEquals(true, ManufactureDateElementStrategy.IsValidManufactureDateString("20220211"));
			AssertEquals(false, ManufactureDateElementStrategy.IsValidManufactureDateString("20220231"));
			AssertEquals(true, ManufactureDateElementStrategy.IsValidManufactureDateString("20220228"));
		}
	}
}

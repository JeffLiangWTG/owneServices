using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class SpecModelElementStrategyTest : TestCaseWithFactory
	{
		public void TestGetDefaultValue()
		{
			AssertEquals("GetDefaultValue", "规格：、型号：", new SpecModelElementStrategy().GetDefaultValue(EnteringOrExiting.Both));
		}

		public void TestValidate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "99998");
			helper.CreateAdditionalElement("99998", "规格型号");
			Factory.Save();
			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "99998", ZDateTime.Today);
			var addInfo = new AdditionalElementWrapper(additionalElement, "AAAAA", new DummyAdditionalInformationWrapperParent(Factory), EnteringOrExiting.Entering);
			addInfo.RunPreSaveValidation();
			AssertHasWarningContaining(addInfo.ElementValueInfo, "The value is in incorrect format");
			addInfo.ElementValue = "规格：XXX、型号：YYY";
			AssertNoWarningContaining(addInfo.ElementValueInfo, "The value is in incorrect format");
		}
	}
}

using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class PackSpecElementStrategyTest : TestCaseWithFactory
	{
		public void TestValidate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "99997");
			helper.CreateAdditionalElement("99997", "包装规格");
			Factory.Save();
			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "99997", ZDateTime.Today);
			var addInfo = new AdditionalElementWrapper(additionalElement, "", new DummyAdditionalInformationWrapperParent(Factory), EnteringOrExiting.Both);
			addInfo.RunPreSaveValidation();
			AssertHasMessageErrorContaining(addInfo.ElementValueInfo, "entered");
			addInfo.ElementValue = "AAAAA";
			AssertHasWarningContaining(addInfo.ElementValueInfo, "The value is in incorrect format");
			addInfo.ElementValue = "10干克*6罐/箱";
			AssertNoWarningContaining(addInfo.ElementValueInfo, "The value is in incorrect format");
		}
	}
}

using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CommonAdditionalElementStrategyTest : TestCaseWithFactory
	{
		public void TestMaxLength()
		{
			AssertEquals("MaxLength", 512, new CommonAdditionalElementStrategy().MaxLength);
		}

		public void TestIsMandatory()
		{
			AssertEquals("IsMandatory", true, new CommonAdditionalElementStrategy().IsMandatory);
		}

		public void TestIsMergeKey()
		{
			AssertEquals("IsMergeKey", false, new CommonAdditionalElementStrategy().IsMergeKey);
		}

		public void TestIsApplicableForEnteringOrExiting()
		{
			var strategy = new CommonAdditionalElementStrategy();
			Assert("Applicable for Both", strategy.IsApplicable(EnteringOrExiting.Both));
			Assert("Applicable for Entering", strategy.IsApplicable(EnteringOrExiting.Entering));
			Assert("Applicable for Exiting", strategy.IsApplicable(EnteringOrExiting.Exiting));
		}

		public void TestList()
		{
			var strategy = new CommonAdditionalElementStrategy();
			AssertEquals("ProvideList", false, strategy.ProvideList);
			AssertEquals("GetList", null, strategy.GetList(Factory, EnteringOrExiting.Both));
		}

		public void TestGetDefaultValue()
		{
			AssertEquals("GetDefaultValue", ZString.Empty, new CommonAdditionalElementStrategy().GetDefaultValue(EnteringOrExiting.Both));
		}

		public void TestValidate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00000");
			helper.CreateAdditionalElement("00000", "品名");
			Factory.Save();

			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "00000", ZDateTime.Today);
			var addInfo = new AdditionalElementWrapper(additionalElement, "", new DummyAdditionalInformationWrapperParent(Factory), EnteringOrExiting.Both);
			addInfo.RunPreSaveValidation();
			AssertHasMessageErrorContaining(addInfo.ElementValueInfo, "entered");
		}
	}
}

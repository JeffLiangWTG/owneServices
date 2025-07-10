using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CountervailingDutyRateStrategyTest : TestCaseWithFactory
	{
		public void TestADDCVDElementStrategy()
		{
			var strategy = new CountervailingDutyRateStrategy();
			Assert("IsMandatory", !strategy.IsMandatory);
			Assert("IsMergeKey", strategy.IsMergeKey);
			Assert("Applicable for Both", strategy.IsApplicable(EnteringOrExiting.Both));
			Assert("Applicable for Entering", strategy.IsApplicable(EnteringOrExiting.Entering));
			Assert("NOT Applicable for Exiting", !strategy.IsApplicable(EnteringOrExiting.Exiting));
		}

		public void TestValidateAdditionalElement()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", CountervailingDutyRateStrategy.AdditionalElementCode);
			helper.CreateAdditionalElement(CountervailingDutyRateStrategy.AdditionalElementCode, "反补贴税率");
			Factory.Save();

			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, CountervailingDutyRateStrategy.AdditionalElementCode, ZDateTime.Today);
			var parent = new DummyAdditionalInformationWrapperParent(Factory);
			var addInfo = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Both);
			addInfo.RunPreSaveValidation();

			var message = "The value should be a number and greater than 0.";
			AssertHasMessageErrorContaining(addInfo.ElementValueInfo, message);

			addInfo.ElementValue = "0.069";
			addInfo.RunPreSaveValidation();
			AssertNoMessageErrorContaining(addInfo.ElementValueInfo, message);

			addInfo.ElementValue = "abc";
			addInfo.RunPreSaveValidation();
			AssertHasMessageErrorContaining(addInfo.ElementValueInfo, message);

			addInfo.ElementValue = "0";
			addInfo.RunPreSaveValidation();
			AssertNoMessageErrorContaining(addInfo.ElementValueInfo, message);

			parent.JobDeclaration.ValidationMode = ValidationModes.Preliminary;
			addInfo.ElementValue = "abc";
			addInfo.RunPreSaveValidation();
			AssertHasWarningContaining(addInfo.ElementValueInfo, message);
		}
	}
}

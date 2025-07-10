using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business.Testing
{
	class PreferentialTariffTreatmentOnDestinationStrategyTest : TestCaseWithFactory
	{
		public void TestisUntranslatable()
		{
			Assert("Pref. tariff treatment on dest. should not be translatable", new PreferentialTariffTreatmentOnDestinationTypeList() is UntranslatableCodeDescriptionPairList);
		}

		public void TestGetDefaultValue()
		{
			var strategy = new PreferentialTariffTreatmentOnDestinationStrategy();
			AssertEquals("GetDefaultValue", "3", strategy.GetDefaultValue(EnteringOrExiting.Entering));
			AssertEquals("GetDefaultValue", "", strategy.GetDefaultValue(EnteringOrExiting.Exiting));
		}

		public void TestList()
		{
			var strategy = new PreferentialTariffTreatmentOnDestinationStrategy();
			AssertEquals("ProvideList", true, strategy.ProvideList);
			AssertEquals("GetList for Both", 4, strategy.GetList(Factory, EnteringOrExiting.Both).Count);
			AssertEquals("GetList for Entering", 1, strategy.GetList(Factory, EnteringOrExiting.Entering).Count);
			Assert("GetList for Entering", strategy.GetList(Factory, EnteringOrExiting.Entering).ContainsCode("3"));
			AssertEquals("GetList for Exiting", 3, strategy.GetList(Factory, EnteringOrExiting.Exiting).Count);
			Assert("GetList for Exiting", !strategy.GetList(Factory, EnteringOrExiting.Exiting).ContainsCode("3"));
		}

		public void TestValidate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00069");
			helper.CreateAdditionalElement("00069", "出口享惠情况");
			Factory.Save();
			var parent = new DummyAdditionalInformationWrapperParent(Factory);
			var additionalElement = CNRefCusCodeListLoader.GetAdditionalElement(Factory, "00069", ZDateTime.Today);
			var addInfo = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Entering);
			addInfo.RunPreSaveValidation();
			addInfo.ElementValue = "1";
			AssertHasMessageErrorContaining(addInfo.ElementValueInfo, "The code you have selected is not in the list");
			addInfo.ElementValue = "3";
			AssertNoMessageErrorContaining(addInfo.ElementValueInfo, "The code you have selected is not in the list");
			addInfo = new AdditionalElementWrapper(additionalElement, "", parent, EnteringOrExiting.Exiting);
			addInfo.RunPreSaveValidation();
			AssertHasMessageErrorContaining(addInfo.ElementValueInfo, "entered");
			addInfo.ElementValue = "3";
			AssertNoMessageErrorContaining(addInfo.ElementValueInfo, "entered");
			AssertHasMessageErrorContaining(addInfo.ElementValueInfo, "The code you have selected is not in the list");
			AssertNoMessageErrorContaining(addInfo.ElementValueInfo, "is only applicable for Goods originating in China.");
			addInfo.ElementValue = "1";
			AssertNoMessageErrorContaining(addInfo.ElementValueInfo, "The code you have selected is not in the list");
			AssertNoMessageErrorContaining(addInfo.ElementValueInfo, "is only applicable for Goods originating in China.");
			parent.CountryOfOrigin = "KR";
			addInfo.RunPreSaveValidation();
			AssertHasMessageErrorContaining(addInfo.ElementValueInfo, "is only applicable for Goods originating in China.");
			parent.CountryOfOrigin = "CN";
			addInfo.RunPreSaveValidation();
			AssertNoMessageErrorContaining(addInfo.ElementValueInfo, "is only applicable for Goods originating in China.");

			parent.JobDeclaration.ValidationMode = ValidationModes.Preliminary;
			parent.CountryOfOrigin = "KR";
			addInfo.RunPreSaveValidation();
			AssertHasWarningContaining(addInfo.ElementValueInfo, "is only applicable for Goods originating in China.");
		}

		public void TestIsMergeKey()
		{
			AssertEquals("IsMergeKey", true, new PreferentialTariffTreatmentOnDestinationStrategy().IsMergeKey);
		}
	}
}

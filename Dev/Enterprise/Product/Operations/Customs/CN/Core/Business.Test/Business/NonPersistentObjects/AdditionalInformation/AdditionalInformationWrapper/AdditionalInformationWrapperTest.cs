using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(AdditionalInformationWrapper))]
	class AdditionalInformationWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBuildAdditionalElementValues()
		{
			SetupTariffAndAdditionalElements(Factory);
			var pivot = Factory.New<CusClassPartPivot>();
			var additionalInformationCodes = new AdditionalInformationCollection(pivot);
			additionalInformationCodes.AddNew("99998", "X");
			pivot.CI_TariffNum = "2713200000";
			var wrapper = new AdditionalInformationWrapper(pivot, "A", "B|C|D", EnteringOrExiting.Both);
			AssertEquals(4, wrapper.AdditionalElementValues.Count);
			AssertAdditionalElementValue(wrapper, 0, "00000", "A", true);
			AssertAdditionalElementValue(wrapper, 1, "00069", "B", true);
			AssertAdditionalElementValue(wrapper, 2, "00422", "C", true);
			AssertAdditionalElementValue(wrapper, 3, "99999", "D", true);
			AssertEquals(0, pivot.AdditionalInformationCodes.Count);
		}

		void AssertAdditionalElementValue(AdditionalInformationWrapper parent, int index, ZString code, ZString value, bool isRequired)
		{
			var additionalElementValue = parent.AdditionalElementValues[index];
			AssertEquals("ElementCode for Element " + index, code, additionalElementValue.ElementCode);
			AssertEquals("ElementValue for Element " + index, value, additionalElementValue.ElementValue);
			AssertEquals("IsRequired for Element " + index, isRequired, additionalElementValue.IsRequired);
		}

		public void TestProperties()
		{
			SetupTariffAndAdditionalElements(Factory);
			var parent = new DummyAdditionalInformationWrapperParent(Factory);
			parent.UniversalTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem, "2713200000", ZDateTime.Today);
			var wrapper = new AdditionalInformationWrapper(parent, "", "", EnteringOrExiting.Both);
			wrapper.AdditionalElementValues["00000"].ElementValue = "A";
			wrapper.AdditionalElementValues["00069"].ElementValue = "B";
			wrapper.AdditionalElementValues["00422"].ElementValue = "C";
			wrapper.AdditionalElementValues["99999"].ElementValue = "D";
			AssertEquals("NameOfGoods", "A", wrapper.NameOfGoods);
			AssertEquals("GoodsSpecModel", "B|C|D", wrapper.GoodsSpecModel);
		}

		public void TestValidation()
		{
			var maxLength = AdditionalInformationHelper.GoodsSpecModelMaxLength;
			var message = $"exceeds the maximum allowed ({maxLength})";
			var charCount = maxLength / 4;

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2713200000", "00069", "00422", "99998", "99999");
			helper.CreateAdditionalElement("00069", "出口享惠情况");
			helper.CreateAdditionalElement("00422", "品牌类型");
			helper.CreateAdditionalElement("99998", "规格型号");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();

			var parent = new DummyAdditionalInformationWrapperParent(Factory)
			{
				UniversalTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem, "2713200000", ZDateTime.Today)
			};
			var wrapper = new AdditionalInformationWrapper(parent, "", "", EnteringOrExiting.Both);
			wrapper.AdditionalElementValues["00069"].ElementValue = new string('X', charCount);
			wrapper.AdditionalElementValues["00422"].ElementValue = new string('X', charCount);
			wrapper.AdditionalElementValues["99998"].ElementValue = new string('X', charCount);
			wrapper.AdditionalElementValues["99999"].ElementValue = new string('X', charCount + 4);
			wrapper.ValidateGoodsSpecModel();
			AssertHasErrorContaining(wrapper.GoodsSpecModelInfo, message);
			wrapper.AdditionalElementValues["99999"].ElementValue = new string('X', charCount - 1);
			wrapper.ValidateGoodsSpecModel();
			AssertNoErrorContaining(wrapper.GoodsSpecModelInfo, message);
		}

		void SetupTariffAndAdditionalElements(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCustomsTariff("2713200000", "00000", "00069", "00422", "99999");
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("00069", "出口享惠情况");
			helper.CreateAdditionalElement("00422", "品牌类型");
			helper.CreateAdditionalElement("99998", "规格型号");
			helper.CreateAdditionalElement("99999", "其他");
			factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AdditionalInformationWrapper(new DummyAdditionalInformationWrapperParent(Factory), "", "", EnteringOrExiting.Both);
		}
	}
}

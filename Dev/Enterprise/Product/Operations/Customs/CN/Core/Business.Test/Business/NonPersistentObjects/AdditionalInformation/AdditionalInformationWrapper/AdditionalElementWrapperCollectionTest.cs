using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(AdditionalElementWrapperCollection))]
	class AdditionalElementWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AdditionalElementWrapperCollection>
	{
		protected override AdditionalElementWrapperCollection GetCollectionToTest()
		{
			CreateTariffForTesting(Factory);
			var parent = new DummyAdditionalInformationWrapperParent(Factory);
			parent.UniversalTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem, "2713200000", ZDateTime.Today);
			return new AdditionalElementWrapperCollection(new AdditionalInformationWrapper(parent, ZString.Empty, ZString.Empty, EnteringOrExiting.Both));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CreateTariffForTesting(Factory);
			return new AdditionalElementWrapper(CNRefCusCodeListLoader.GetAdditionalElement(Factory, "00000", ZDateTime.Today), ZString.Empty, new DummyAdditionalInformationWrapperParent(Factory), EnteringOrExiting.Both);
		}

		void CreateTariffForTesting(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			helper.CreateCustomsTariff("2713200000", "00000", "99999");
			helper.CreateAdditionalElement("00000", "品名");
			helper.CreateAdditionalElement("99999", "其他");
			Factory.Save();
		}

		public void TestGetAndSetValue()
		{
			CreateTariffForTesting(Factory);
			var parent = new DummyAdditionalInformationWrapperParent(Factory);
			parent.UniversalTariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem, "2713200000", ZDateTime.Today);
			var collection = new AdditionalInformationWrapper(parent, ZString.Empty, ZString.Empty, EnteringOrExiting.Both).AdditionalElementValues;
			collection.SetValue("00000", "X");
			AssertEquals("X", collection.GetValue("00000"));
			collection.SetValue("99999", "Y");
			AssertEquals("Y", collection.GetValue("99999"));
			collection.SetValue("11111", "Z");
			AssertEquals("", collection.GetValue("11111"));
		}
	}
}

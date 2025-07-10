using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNAdditionalAttributeInformationProviderTest : TestCaseWithFactory
	{
		public void TestAdditionalDescription()
		{
			var tariffTypeForCN = Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, "222");
			Factory.Save();
			Helper.CreateNewOrGetExistingCusCodeType("ADIEL", "CN AdditionalElements");
			Helper.CreateNewOrGetExistingCusCodeType("OTHER", "CN Others");
			Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "ADIEL", "AAA", "AAA Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.China, "OTHER", "BBB", "BBB Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var tariffViewForCN = Helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypeForCN.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "Tariff Description");
			Helper.CreateTariffAttribute("AdditionalInfo", "AAA", tariffViewForCN);
			Factory.Save();
			var providerForCN = new CNAdditionalAttributeInformationProvider(Factory);
			AssertEquals("AAA Description", providerForCN.AdditionalDescription("AdditionalInfo01", "AAA"));
			AssertEquals("", providerForCN.AdditionalDescription("AdditionalInfo01", "BBB"));
			AssertEquals("", providerForCN.AdditionalDescription("TEST", "DDD"));
		}

		public void TestAdditionalDescriptionVisible()
		{
			AssertEquals(true, new CNAdditionalAttributeInformationProvider(Factory).AdditionalDescriptionVisible);
		}

		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
	}
}

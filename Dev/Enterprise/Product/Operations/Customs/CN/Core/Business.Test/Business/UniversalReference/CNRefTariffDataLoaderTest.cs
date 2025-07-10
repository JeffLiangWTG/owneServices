using System.Collections.Generic;
using System.Collections.Specialized;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNRefTariffDataLoaderTest : TestCaseWithFactory
	{
		public void TestGetCustomsTariffRequiredAdditionalElements()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("8508110000", "00000", "01736", "00003", "99999");
			Factory.Save();
			var tariffAttributes = CNRefTariffDataLoader.GetCustomsTariffAdditionalInfoAttributes(Factory, "8508110000", ZDateTime.Today);
			AssertEquals("ZZ3_Name", "AdditionalInfo01", tariffAttributes[0].ZZ3_Name);
			AssertEquals("ZZ3_Value", "00000", tariffAttributes[0].ZZ3_Value);
			AssertEquals("ZZ3_Name", "AdditionalInfo02", tariffAttributes[1].ZZ3_Name);
			AssertEquals("ZZ3_Value", "01736", tariffAttributes[1].ZZ3_Value);
			AssertEquals("ZZ3_Name", "AdditionalInfo03", tariffAttributes[2].ZZ3_Name);
			AssertEquals("ZZ3_Value", "00003", tariffAttributes[2].ZZ3_Value);
			AssertEquals("ZZ3_Name", "AdditionalInfo04", tariffAttributes[3].ZZ3_Name);
			AssertEquals("ZZ3_Value", "99999", tariffAttributes[3].ZZ3_Value);
		}

		public void TestGetAdditionalElementDescription()
		{
			TariffView tariff = null;
			AssertEquals("when tariff is null", ZString.Empty, tariff.GetAdditionalElementDescription("99998"));

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("6202131000", "99997", "99998", "01746");
			helper.CreateAdditionalElement("99997", "包装规格");
			helper.CreateAdditionalElement("99998", "规格型号");
			helper.CreateAdditionalElement("01746", "规格");
			Factory.Save();

			tariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem, "6202131000", ZDateTime.Today);
			AssertEquals("When code is 99998", "规格型号", tariff.GetAdditionalElementDescription("99998"));

			var cachedAdditionalElements = tariff.Factory.GetCachedValue("CN.AdditionalElements", () => new StringDictionary());
			AssertEquals("AdditionalElements should be cached", 3, cachedAdditionalElements.Count);
		}

		public void TestCacheTariffRequiredAdditionalElements()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("6202131000", "99997", "99998", "01746");
			helper.CreateCustomsTariff("6202131001", "99997", "01746");
			helper.CreateAdditionalElement("99997", "包装规格");
			helper.CreateAdditionalElement("99998", "规格型号");
			helper.CreateAdditionalElement("01746", "规格");
			Factory.Save();

			var tariff1 = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem, "6202131000", ZDateTime.Today);
			var tariff2 = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem, "6202131001", ZDateTime.Today);
			CNRefTariffDataLoader.CacheTariffRequiredAdditionalElements(Factory, new TariffView[] { tariff1, tariff2 });

			var cachedTariffRequiredAdditionalElements = Factory.GetCachedValue("CN.TariffRequiredAdditionalElements", () => new Dictionary<ZGuid, StringDictionary>());
			CombineAssertions("TariffRequiredAdditionalElements should be cached", () =>
			{
				AssertEquals("Entry Count", 2, cachedTariffRequiredAdditionalElements.Count);
				AssertEquals("6202131000 AdditionalElement Count", 3, cachedTariffRequiredAdditionalElements[tariff1.PK].Count);
				AssertEquals("6202131001 AdditionalElement Count", 2, cachedTariffRequiredAdditionalElements[tariff2.PK].Count);
				AssertEquals("6202131000 99998 Description", "规格型号", cachedTariffRequiredAdditionalElements[tariff1.PK]["99998"]);
			});
		}

		public void TestCacheTariffRequiredAdditionalElements_NotFound()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("6202131000", "99998");
			Factory.Save();

			var tariff = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.China, Universal.Constants.TariffTypes.HarmonizedSystem, "6202131000", ZDateTime.Today);

			AssertEquals(ZString.Empty, CNRefTariffDataLoader.GetAdditionalElementDescription(tariff, "99998"));
			AssertEquals("ExceptionReporter should have caught the exceptions", "CNTariffRequiredAdditionalElements_NotFound", ErrorReporter.LastKeyReported);
			AssertEquals("ExceptionReporter should have caught the exceptions", "Cannot find Required Additional Elements For Tariff '6202131000'", ErrorReporter.LastMessageReported);
			ExceptionReporterTestListener.Instance.Clear();
		}
	}
}

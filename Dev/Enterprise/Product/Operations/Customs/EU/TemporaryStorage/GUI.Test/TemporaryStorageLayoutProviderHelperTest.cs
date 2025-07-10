using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TemporaryStorageLayoutProviderHelper))]
	sealed class TemporaryStorageLayoutProviderHelperTest : TestCaseWithFactory
	{
		public void TestStaticGetLayoutProvider()
		{
			var tempStorage = Factory.New<TemporaryStorageHeader>();
			tempStorage.AMA_JobReference = "JobReference";

			tempStorage.AMA_RN_NKCountry = ZString.Empty;
			AssertType<UCC6TemporaryStorageLayoutProvider>("Should return EU LayoutProvider for empty AMA_RN_NKCountry", TemporaryStorageLayoutProviderHelper.GetLayoutProvider(tempStorage));

			tempStorage.AMA_RN_NKCountry = "XX";
			AssertType<UCC6TemporaryStorageLayoutProvider>("Should return EU LayoutProvider for invalid AMA_RN_NKCountry.", TemporaryStorageLayoutProviderHelper.GetLayoutProvider(tempStorage));

			tempStorage.AMA_RN_NKCountry = Core.Constants.CountryCodes.EuropeanUnion;
			AssertType<UCC6TemporaryStorageLayoutProvider>("Should return EU LayoutProvider for AMA_RN_NKCountry EU.", TemporaryStorageLayoutProviderHelper.GetLayoutProvider(tempStorage));

			var ieTempStorage = (TemporaryStorageHeader)Factory.New<Integration.Customs.IE.ITemporaryStorageHeader>();
			ieTempStorage.AMA_RN_NKCountry = Core.Constants.CountryCodes.Ireland;
			AssertEquals(
				message: "Should return IE LayoutProvider for AMA_RN_NKCountry IE.",
				expected: "Enterprise.Customs.IE.GUI.UCC5TemporaryStorageLayoutProvider",
				actual: TemporaryStorageLayoutProviderHelper.GetLayoutProvider(ieTempStorage).GetType().FullName
			);

			tempStorage.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;
			AssertEquals(
				message: "Should return ES LayoutProvider for AMA_RN_NKCountry ES.",
				expected: "Enterprise.Customs.ES.TemporaryStorage.GUI.G5V1TemporaryStorageLayoutProvider",
				actual: TemporaryStorageLayoutProviderHelper.GetLayoutProvider(tempStorage).GetType().FullName
			);

			tempStorage.AMA_RN_NKCountry = Core.Constants.CountryCodes.Italy;
			AssertEquals(
				message: "Should return IT LayoutProvider for AMA_RN_NKCountry IT.",
				expected: "Enterprise.Customs.IT.TemporaryStorage.GUI.UCC6TemporaryStorageLayoutProvider",
				actual: TemporaryStorageLayoutProviderHelper.GetLayoutProvider(tempStorage).GetType().FullName
			);
		}
	}
}

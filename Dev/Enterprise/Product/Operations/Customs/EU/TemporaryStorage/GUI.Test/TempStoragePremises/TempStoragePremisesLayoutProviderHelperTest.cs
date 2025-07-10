using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.TemporaryStorage.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	[TestedType(typeof(TempStoragePremisesLayoutProviderHelper))]
	sealed class TempStoragePremisesLayoutProviderHelperTest : TestCaseWithFactory
	{
		public void TestStaticGetLayoutProvider()
		{
			CombineAssertions(() =>
			{
				var tempStorage = Factory.New<CusTempStorageRegPremises>();

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.EuropeanUnion;
				var layoutProvider = TempStoragePremisesLayoutProviderHelper.GetLayoutProvider(tempStorage);
				AssertType<TempStoragePremisesLayoutProvider>("Should return EU LayoutProvider for Company Country EU.", layoutProvider);
				AssertEquals("Should return EU FullName for Company Country EU.", "Enterprise.Customs.EU.TemporaryStorage.GUI.TempStoragePremisesLayoutProvider", layoutProvider.GetType().FullName);

				GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
				layoutProvider = TempStoragePremisesLayoutProviderHelper.GetLayoutProvider(tempStorage);
				AssertEquals("Should return Default FullName for Company Country ES.", "Enterprise.Customs.EU.TemporaryStorage.GUI.TempStoragePremisesLayoutProvider", layoutProvider.GetType().FullName);
			});
		}
	}
}

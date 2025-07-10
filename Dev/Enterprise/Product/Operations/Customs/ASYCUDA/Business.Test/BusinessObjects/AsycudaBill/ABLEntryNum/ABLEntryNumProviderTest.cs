using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class ABLEntryNumProviderTest : TestCaseWithFactory
	{
		public void TestABLEntryNumProvider()
		{
			var ablEntryNumber = Factory.New<ABLEntryNum>();
			ablEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			var provider = ablEntryNumber.Provider;
			AssertNotNull(provider);
			AssertEquals("Enterprise.Customs.ASYCUDA.Business.ABLEntryNumValidation", provider.GetNewValidation(ablEntryNumber).GetType().FullName);
		}
	}
}

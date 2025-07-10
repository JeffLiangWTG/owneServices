using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class OrgCusAccountGUIProviderTest : TestCaseWithFactory
	{
		public void TestGUIProviderGermany()
		{
			AssertType<OrgCusAccountGUIProvider>(MasterFiles.GUI.OrgCusAccountGUIProvider.GetByCountryCode(Core.Constants.CountryCodes.Germany));
		}
	}
}

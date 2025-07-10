using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.GUI.Testing
{
	class OrgCusAccountGUIProviderTest : TestCaseWithFactory
	{
		public void TestGUIProviderFrance()
		{
			AssertType<OrgCusAccountGUIProvider>(provider);
		}

		public void TestColumnRepresentativeID()
		{
			AssertNotNull(provider.CZ_RepresentativeID);
		}

		protected override void SetUp()
		{
			base.SetUp();

			provider = MasterFiles.GUI.OrgCusAccountGUIProvider.GetByCountryCode(Core.Constants.CountryCodes.France);
		}

		MasterFiles.GUI.OrgCusAccountGUIProvider provider;
	}
}

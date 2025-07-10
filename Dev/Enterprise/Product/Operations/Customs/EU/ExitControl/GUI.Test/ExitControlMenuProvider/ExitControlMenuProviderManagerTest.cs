using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing
{
	class ExitControlMenuProviderManagerTest : TestCaseWithFactory
	{
		public void TestGetMenuProvider()
		{
			CombineAssertions(() =>
			{
				AssertNull("blank", ExitControlMenuProviderManager.GetMenuProvider(string.Empty));
				AssertNull("no match", ExitControlMenuProviderManager.GetMenuProvider("!@#"));
				AssertNotNull("match IE", ExitControlMenuProviderManager.GetMenuProvider(Core.Constants.CountryCodes.Ireland));
				AssertNotNull("match DE", ExitControlMenuProviderManager.GetMenuProvider(Core.Constants.CountryCodes.Germany));
				AssertNotNull("match ES", ExitControlMenuProviderManager.GetMenuProvider(Core.Constants.CountryCodes.Spain));
			});
		}
	}
}

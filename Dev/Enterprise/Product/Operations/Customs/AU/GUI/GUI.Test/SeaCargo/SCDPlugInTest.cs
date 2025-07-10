using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	abstract class SCDPlugInTest : SeaCargoDepotTestCase
	{
		public void TestBusinessEntityNotLoadedWhenTestingCMR()
		{
			using (SCDPlugIn testPlugIn = GetPlugIn())
			{
				Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				AssertEquals("Is CMR", true, testPlugIn.IsCMR);
				Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
				AssertEquals("Is CMR", false, testPlugIn.IsCMR);
				AssertEquals(ContinueWithSave.Yes, testPlugIn.ShowPreSaveDialogs());
				AssertNoExceptionThrown(() => Factory.Save());
			}
		}

		protected abstract SCDPlugIn GetPlugIn();
	}
}

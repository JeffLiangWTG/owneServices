using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	abstract class BaseSCDPlugInTest : TestCaseWithFactory
	{
		public void TestManagerInLegacyMode()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (SCDPlugIn testPlugIn = GetPlugIn())
			{
				AssertEquals("Is CMR", false, testPlugIn.IsCMR);
				AssertNull("Plug Manager is should be null in Legacy Mode", testPlugIn.GetManager());
			}
		}

		public void TestShowPreSaveDialogCore()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			using (SCDPlugIn testPlugIn = GetPlugIn())
			{
				AssertNotNull(testPlugIn.ShowPreSaveDialogsCore());
			}
		}

		protected abstract SCDPlugIn GetPlugIn();
	}
}

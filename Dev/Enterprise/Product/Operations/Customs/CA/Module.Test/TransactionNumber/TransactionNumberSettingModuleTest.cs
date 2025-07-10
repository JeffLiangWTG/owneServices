using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(TransactionNumberSettingModule))]
	sealed class TransactionNumberSettingModuleTest : ZPopupModuleBasherTest
	{
		public void TestController()
		{
			using (var module = GetModule())
			{
				module.Show();
				ZController lastController = ((IPopupModuleInternalsForTesting)module).LastController;
				AssertEquals(typeof(TransactionNumberSettingController), lastController.GetType());
				if (lastController.LastShownForm != null)
				{
					lastController.LastShownForm.Dispose();
				}
			}
		}

		public void TestLicenceCheckPoint()
		{
			using (var module = GetModule())
			{
				module.Show();
				AssertEquals("LicenceCheckPoint", Env.Licence.Broker, module.LicenceCheckPoint);
				module.CloseFormForTestingOnly();
			}
		}

		ZPopupModule GetModule() => (ZPopupModule)ZModuleFactory.Instance.Create(ModuleIDs.Customs.CA.CATransactionNumberSetting);

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.CATransactionNumberSetting;
	}
}

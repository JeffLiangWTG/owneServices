using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.AU.Module.Testing
{
	abstract class CMRSearchOnlyModuleTest : ZModuleBasherTest
	{
		public void TestModule()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()) as CMRSearchOnlyModule)
			{
				AssertEquals("Has no actions", false, module.HasActions);
				AssertEquals("Licence Check Point", Env.Licence.Broker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.CustomsDeclarationEnquiry, module.SecurityCheckpoint);
				Assert("AllowDelete", !module.AllowDelete);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;
	}
}

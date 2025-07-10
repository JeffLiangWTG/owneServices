using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Module.Testing
{
	[TestedType(typeof(ExitControlModule))]
	sealed class ExitControlModuleTest : ZModuleBasherTest
	{
		public void TestFilterBusinessObject()
		{
			AssertType<ExitControlFilterBusinessObject>("FilterBusinessObject Type", module.FilterBusinessObject);
		}

		protected override void SetUp()
		{
			base.SetUp();
			module = new ExitControlModule();
		}

		protected override void TearDown()
		{
			module.Dispose();
			base.TearDown();
		}

		ExitControlModule module;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.ExitControl;

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;
	}
}

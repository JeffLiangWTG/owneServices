using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(TempStoragePremisesModule))]
	class TemporaryStoragePremisesModuleTest : ZModuleBasherTest
	{
		public void TestGetNewController() => AssertType<TempStoragePremisesController>(module.GetNewController());

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.TempStoragePremises;

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;

		protected override void SetUp()
		{
			base.SetUp();
			module = GetModule();
		}

		protected override void TearDown()
		{
			base.TearDown();
			module.Dispose();
		}

		ZFilterModule module;
	}
}

using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.TemporaryStorage.Module.Testing
{
	[TestedType(typeof(TemporaryStorageRegisterLinesModule))]
	class TemporaryStorageRegisterLinesModuleTest : ZModuleBasherTest
	{
		public void TestModuleType()
		{
			using var module = GetModule();
			AssertType<TemporaryStorageRegisterLinesModule>(module);
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = (TemporaryStorageRegisterLinesModule)GetModule())
			{
				AssertType<TemporaryStorageRegisterLinesFilterBusinessObject>(module.FilterBusinessObject);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.TempStorageRegisterLines;

		protected override string CountryCode => Core.Constants.CountryCodes.Spain;

		protected override bool HasController() => false;
	}
}

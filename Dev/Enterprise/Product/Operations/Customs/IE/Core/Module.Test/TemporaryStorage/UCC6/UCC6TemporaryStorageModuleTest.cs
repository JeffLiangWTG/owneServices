using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(UCC6TemporaryStorageModule))]
	sealed class UCC6TemporaryStorageModuleTest : ZModuleBasherTest
	{
		public void TestFilterBusinessObject()
		{
			using (var module = new UCC6TemporaryStorageModule())
			{
				AssertType<UCC6TemporaryStorageFilterStripBusinessObject>(module.FilterBusinessObject);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.UCC6TemporaryStorage;

		protected override string CountryCode => Core.Constants.CountryCodes.Ireland;

		protected override bool HasController() => true;
	}
}

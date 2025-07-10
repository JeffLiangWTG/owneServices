using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CACusPermitModule))]
	sealed class CACusPermitModuleTest : ZModuleBasherTest
	{
		public void TestFilterControl()
		{
			using (var module = new CACusPermitModule())
			{
				AssertType<CACusPermitFilterControl>(module.EmbeddedControl);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.Permits;

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}

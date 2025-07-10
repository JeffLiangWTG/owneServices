using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(CACusRulingModule))]
	sealed class CACusRulingModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.CACusRuling;

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}

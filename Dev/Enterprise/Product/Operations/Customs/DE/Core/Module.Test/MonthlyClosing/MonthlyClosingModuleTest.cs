using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Module.Testing
{
	[TestedType(typeof(MonthlyClosingModule))]
	class MonthlyClosingModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.EU.DE.MonthlyClosing;

		protected override string CountryCode => Core.Constants.CountryCodes.Germany;
	}
}

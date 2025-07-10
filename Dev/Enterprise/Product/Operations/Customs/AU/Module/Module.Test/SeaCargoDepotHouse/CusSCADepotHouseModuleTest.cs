using Enterprise.Customs.AU.Module.SeaCargo;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CusSCADepotHouseModule))]
	sealed class CusSCADepotHouseModuleTest : ZModuleBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.CusSCADepotHouse;
	}
}

using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(AirCTOExportModule))]
	sealed class AirCTOExportModuleTest : ZModuleBasherTest
	{
		public void TestLicenceCheckPoint()
		{
			using (var module = new AirCTOExportModule())
			{
				AssertEquals(Env.Licence.ExportAirCTOReport, module.LicenceCheckPoint);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.AirCTOExport;

		protected override string CountryCode => Core.Constants.CountryCodes.Australia;
	}
}

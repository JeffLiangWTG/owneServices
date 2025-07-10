using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(ExportCustomsManifestModule))]
	public class ExportCustomsManifestTest : ZModuleBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.Australia;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.AU.ExportCustomsManifest;
	}
}

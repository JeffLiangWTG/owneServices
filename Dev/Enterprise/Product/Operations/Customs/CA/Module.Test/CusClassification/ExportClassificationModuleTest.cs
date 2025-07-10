using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(ExportClassificationModule))]
	sealed class ExportClassificationModuleTest : Customs.Module.Testing.ImportClassificationModuleTest
	{
		public override void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.ExportClassification, testImportClassificationModule.SecurityCheckpoint);
		}

		public new void TestModuleID()
		{
			AssertEquals(ModuleIDs.Customs.CA.CAExportClassification, testImportClassificationModule.ID);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.CAExportClassification;

		protected override Customs.Module.ImportClassificationModule GetNewImportClassificationModule() => new ExportClassificationModule();

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}

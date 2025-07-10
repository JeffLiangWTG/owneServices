using Enterprise.Customs.Module;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(HTSClassificationModule))]
	sealed class HTSClassificationModuleTest : Customs.Module.Testing.ImportClassificationModuleTest
	{
		public void TestGetCreateImportFromCSVForm()
		{
			using (var module = new HTSClassificationModule())
			using (var csv = module.CreateImportFromCSVForm())
			{
				AssertType<GUI.ImportClassificationsFromCSVForm>(csv);
			}
		}

		public new void TestModuleID()
		{
			AssertEquals(ModuleIDs.Customs.CA.HTSClassification, testImportClassificationModule.ID);
		}

		public override void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.HTSClassification, testImportClassificationModule.SecurityCheckpoint);
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.CA.HTSClassification;

		protected override ImportClassificationModule GetNewImportClassificationModule() => new HTSClassificationModule();

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}

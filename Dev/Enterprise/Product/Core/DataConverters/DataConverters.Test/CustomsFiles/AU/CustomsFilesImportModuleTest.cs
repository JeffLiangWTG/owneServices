using Enterprise.DataConverters.CustomsFiles;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.CustomsFiles.AU
{
	[TestedType(typeof(CustomsFilesImportModule))]
	sealed internal class CustomsFilesImportModuleTest : ZPopupModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.ImportCustomsFilesData;
		}
	}
}

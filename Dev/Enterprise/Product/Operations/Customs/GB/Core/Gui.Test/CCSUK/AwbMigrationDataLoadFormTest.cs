using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	[TestedType(typeof(AwbMigrationDataLoadForm))]
	class AwbMigrationDataLoadFormTest : DataLoaderFormTestCase
	{
		protected override ImportFromCSVForm GetNewImportFromCSVFormCore()
		{
			return new AwbMigrationDataLoadForm();
		}
	}
}

using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(ImportClassificationsFromCSVForm))]
	sealed class ImportClassificationsFromCSVFormBasher : DataLoaderFormTestCase
	{
		protected override ImportFromCSVForm GetNewImportFromCSVFormCore() => new ImportClassificationsFromCSVForm();
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.ExportManifest.GUI;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI.DataImport
{
	public class ConsolFormCreatorTest : TestCaseWithFactory
	{
		public void TestInitialise()
		{
			ManifestDataImporter dataImporter = new ManifestDataImporter();
			ConsolFormCreator formCreator = new ConsolFormCreator(dataImporter);
			AssertEquals(dataImporter, formCreator.DataImporter);
		}

		[ExpectNoExceptions()]
		public void TestShowForm()
		{
			ManifestDataImporter dataImporter = new ManifestDataImporter();
			ConsolFormCreator formCreator = new ConsolFormCreator(dataImporter);
			formCreator.ShowForms();
		}
	}
}

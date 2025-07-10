using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.LocalCartage.DataTransfer;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class LocalCartageStatusImporterTest : TestCaseWithFactory
	{
		public void TestAdapter()
		{
			LocalCartageStatusImporter importer = new LocalCartageStatusImporter();
			AssertEquals("Adapter", typeof(CommonCartageStatusValueObjectDataAdapter), importer.Adapter.GetType());
		}
	}
}

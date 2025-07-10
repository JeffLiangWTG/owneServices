using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Billing.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.OSP.Data_Import.Testing
{
	public class OSPCombilineImporterTest : TestCaseWithFactory
	{
		public void TestImportDataToFactoryCore()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				string pathToFile = resourceRetriever.SaveResourceToFile("80618BIR.TXT");
				OSPCombilineImporter importer = new OSPCombilineImporter(Factory);
				AssertEquals("PRECANDITION: should be 0 consols", 0, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
				AssertEquals(true, importer.ImportData(pathToFile, new NotificationBuffer(), SourceInfo.EmptySourceInfo));
				AssertEquals(1, Factory.GetDatabaseCount(typeof(ForwardingConsol)));
				ForwardingConsol consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery());
				AssertNotNull(consol);
				AssertEquals(2, consol.Shipments.Count);
				AssertEquals("MI08105981", consol.Shipments[0].JS_HouseBill);
				AssertEquals("MI08106533", consol.Shipments[1].JS_HouseBill);
			}
		}
	}
}

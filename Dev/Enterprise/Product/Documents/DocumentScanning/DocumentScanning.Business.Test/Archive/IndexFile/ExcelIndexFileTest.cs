using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentScanning.Business.Test
{
	sealed class ExcelIndexFileTest : TestCaseWithFactory
	{
		public void TestGetOwner()
		{
			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingConsol)));
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingShipment)));

			Factory.Save();

			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var smallGifPath = resourceRetriever.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.small.gif", "small.gif");
				var consolDoc = (StorageDocsBase)((IDocManagerSupport)consol).DocManagerInfo.AddFileOrDocument(smallGifPath, Core.Constants.RefDocTypes.MiscellaneousDocument);
				var shipmentDoc = (StorageDocsBase)((IDocManagerSupport)shipment).DocManagerInfo.AddFileOrDocument(smallGifPath, Core.Constants.RefDocTypes.MiscellaneousDocument);

				using (var indexFile = new ExcelIndexFile(null))
				{
					var consolOwner = indexFile.GetOwner(consolDoc);
					AssertEquals("GetOwner()", consol.PK, consolOwner.PK);
					var shipmentOwner = indexFile.GetOwner(shipmentDoc);
					AssertEquals("GetOwner()", shipment.PK, shipmentOwner.PK);
				}
			}
		}
	}
}

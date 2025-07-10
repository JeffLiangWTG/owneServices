using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class ProductXmlValueObjectSerializerTest : XmlValueObjectSerializerTestCase
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEndToEndForBulkData()
		{
			var adapter = new ProductValueObjectDataAdapter();
			var importer = new ProductXmlDataImporter(adapter);
			var buffer = new NotificationBuffer();

			using (StreamReader reader = new StreamReader(BaseSourcePath + @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\BulkProducts.xml"))
			{
				importer.ImportData(reader, "", buffer, SourceInfo.EmptySourceInfo);
			}
			Factory.Save();

			BusinessObjectFactory newFactoryForLoad = new BusinessObjectFactory();
			AssertEquals("592 Products should be imported", 592, newFactoryForLoad.GetDatabaseCount(typeof(OrgSupplierPart)));
			AssertEquals("There were errors: " + buffer.AsString, false, buffer.HasErrors);
		}
	}
}

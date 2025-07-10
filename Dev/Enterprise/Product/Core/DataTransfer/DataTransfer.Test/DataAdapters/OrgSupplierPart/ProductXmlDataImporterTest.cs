using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	sealed class ProductXmlDataImporterTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_ValidDocument()
		{
			var importer = new ProductXmlDataImporter(ProductValueObjectDataAdapter.New());

			AssertEquals("Precondition: Database should not contain any Products", 0, Factory.GetDatabaseCount(typeof(OrgSupplierPart)));

			ImportProductXmlData(importer, BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\ValidProduct.xml", new NotificationBuffer());

			var parts = Factory.Load<OrgSupplierPart>(new ZQuery());
			AssertEquals("2 Products should be imported - BOM PRODUCT and PART", 2, parts.Length);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImport_InvalidDocument()
		{
			var importer = new ProductXmlDataImporter(ProductValueObjectDataAdapter.New());
			var notificationBuffer = new NotificationBuffer();

			ImportProductXmlData(importer, BaseSourcePath + @"\Enterprise\Product\Core\DataTransfer\DataTransfer.Test\DataAdapters\OrgSupplierPart\Testing\InvalidXmlDocument.xml", notificationBuffer);
			AssertEquals("The user should be notified of an error", true, notificationBuffer.AsString.IndexOf("There is an error in the XML document") != -1);
		}

		public void TestSerializer()
		{
			var importer = new TestProductXmlDataImporter(ProductValueObjectDataAdapter.New());
			AssertEquals("Serializer", typeof(ProductXmlValueObjectSerializer), importer.GetSerializer().GetType());
		}

		void ImportProductXmlData(ProductXmlDataImporter importer, string fileName, NotificationBuffer notify)
		{
			using (var reader = new StreamReader(fileName))
			{
				importer.ImportData(reader, "", notify, SourceInfo.EmptySourceInfo);
			}
			Factory.Save();
		}

		sealed class TestProductXmlDataImporter : ProductXmlDataImporter
		{
			public TestProductXmlDataImporter(ProductValueObjectDataAdapter adapter)
				: base(adapter)
			{
			}

			internal new Xml.XmlValueObjectSerializer GetSerializer() => base.GetSerializer();
		}
	}
}

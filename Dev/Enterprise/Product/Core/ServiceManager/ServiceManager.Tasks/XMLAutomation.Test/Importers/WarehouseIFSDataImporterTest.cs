using Enterprise.DataTransfer.Business.Testing;
using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Transactions.DataTransfer;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class WarehouseIFSDataImporterTest : XmlDataImporterTest
	{
		#region TestAdapterType

		public void TestAdapterType()
		{
			AssertEquals(typeof(WhsOrderCartageValueObjectDataAdapterIFS), Importer.Adapter.GetType());
		}

		#endregion

		#region TestSerializerType

		public void TestSerializerType()
		{
			AssertEquals(typeof(XmlValueObjectSerializerIFS), Importer.GetSerializer().GetType());
		}

		#endregion

		#region Implementation

		#region WarehouseIFSDataImporterForTest

		class WarehouseIFSDataImporterForTest : WarehouseIFSDataImporter
		{
			public new XmlValueObjectSerializer GetSerializer()
			{
				return base.GetSerializer();
			}
		}

		#endregion

		WarehouseIFSDataImporterForTest Importer
		{
			get { return importer ?? (importer = new WarehouseIFSDataImporterForTest()); }
		}

		WarehouseIFSDataImporterForTest importer;

		#endregion

	}
}

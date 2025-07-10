using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Transactions.DataTransfer;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class WarehouseIFSDataImporter : XmlDataImporter
	{
		public WarehouseIFSDataImporter()
			: base(new WhsOrderCartageValueObjectDataAdapterIFS())
		{
		}

		#region GetSerializer

		protected override XmlValueObjectSerializer GetSerializer()
		{
			return new XmlValueObjectSerializerIFS();
		}

		#endregion
	}
}

using Enterprise.DataTransfer.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class WarehouseCartageDataImporter : XmlDataImporter
	{
		public WarehouseCartageDataImporter()
			: base(new WhsOrderCartageValueObjectDataAdapterClippership())
		{
		}
	}
}
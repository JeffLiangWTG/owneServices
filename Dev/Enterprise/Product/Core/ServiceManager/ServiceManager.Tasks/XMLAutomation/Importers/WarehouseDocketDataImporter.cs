using Enterprise.DataTransfer.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class WarehouseDocketDataImporter : XmlDataImporter
	{
		public WarehouseDocketDataImporter()
			: base(new WhsDocketValueObjectDataUniversalAdapter())
		{
		}
	}
}

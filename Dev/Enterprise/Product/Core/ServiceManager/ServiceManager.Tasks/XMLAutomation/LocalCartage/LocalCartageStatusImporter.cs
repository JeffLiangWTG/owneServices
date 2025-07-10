using Enterprise.DataTransfer.Business;
using Enterprise.Freight.LocalCartage.DataTransfer;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class LocalCartageStatusImporter : XmlDataImporter
	{
		public LocalCartageStatusImporter()
			: base(new CommonCartageStatusValueObjectDataAdapter())
		{
		}
	}
}

using Enterprise.Freight.LocalCartage.DataTransfer;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class LocalCartageBookingImporter : AdditionalActionsXmlDataImporter
	{
		public LocalCartageBookingImporter()
			: base(new CommonCartageBookingValueObjectDataAdapter())
		{
		}
	}
}

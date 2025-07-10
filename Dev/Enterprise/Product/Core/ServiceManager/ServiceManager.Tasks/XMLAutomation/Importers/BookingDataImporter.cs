using Enterprise.DataTransfer.Business;
using Enterprise.Freight.QuotedBookings.DataTransfer;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation
{
	public class BookingDataImporter : XmlDataImporter
	{
		public BookingDataImporter()
			: base(new QuotedBookingValueObjectDataAdapter())
		{
		}
	}
}

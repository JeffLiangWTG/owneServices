using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Billing.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	sealed class BookingDataImporterTest : TestCaseWithFactory
	{
		public void TestEndToEnd()
		{
			var noOfBookingsBefore = NoOfBookings;

			var importer = new BookingDataImporter();
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			using (var stream = resourceRetriever.GetStream("Enterprise.ServiceManager.Tasks.XMLAutomation.Test.Testing.Booking.xml"))
			using (var reader = new StreamReader(stream))
			{
				importer.ImportData(reader, "Booking.Xml", new NotificationBuffer(), SourceInfo.EmptySourceInfo);
			}

			AssertEquals(noOfBookingsBefore + 1, NoOfBookings);
		}

		int NoOfBookings
		{
			get { return Factory.GetDatabaseCount(typeof(ViewQuotedBooking)); }
		}
	}
}

using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.OrderManager
{
	[TestedType(typeof(FreightWrapperFromJobSupplierBooking))]
	public class FreightWrapperFromJobSupplierBookingTest : FreightWrapperTest
	{
		public void TestWrapper()
		{
			var booking = Factory.New<JobSupplierBooking>();
			booking.JSB_BookingId = "JSB001";
			booking.SupplierBookingLines.AddNew().JSL_BookingLineId = "JSL001";
			booking.SupplierBookingLines.AddNew().JSL_BookingLineId = "JSL002";
			var wrapper = new FreightWrapperFromJobSupplierBooking(booking, Factory);
			AssertEquals("JSB001", wrapper.JobNumber);
			AssertEquals("JSB001", wrapper.JobSupplierBooking.BookingId);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "JSL001", "JSL002" }, wrapper.JobSupplierBooking.BookingLines.Cast<DocJobSupplierBookingLine>().Select(x => x.BookingLineId));
		}
	}
}

using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocJobSupplierBookingLineCollection))]
	internal class DocJobSupplierBookingLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocJobSupplierBookingLineCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var booking = Factory.New<JobSupplierBooking>();
			return DocJobSupplierBookingLine.New(booking.SupplierBookingLines.AddNew(), Factory);
		}

		protected override DocJobSupplierBookingLineCollection GetCollectionToTest()
		{
			return new DocJobSupplierBookingLineCollection(Factory);
		}
	}
}

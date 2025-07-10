using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocConsolidatedTransportBooking))]
	sealed class DocConsolidatedTransportBookingTest : DocumentWrapperTestCase
	{
		public void TestEmailSubjectNumber()
		{
			Booking.D1_UniqueConsignRef = "t1";
			AssertEquals(Booking.D1_UniqueConsignRef, BookingWrapper.EmailSubjectNumber);
		}

		#region Implementaion

		CommonConsolidatedTransportBooking Booking;
		DocConsolidatedTransportBooking BookingWrapper;

		protected override void SetUp()
		{
			Booking = Factory.New<CommonConsolidatedTransportBooking>();
			BookingWrapper = DocConsolidatedTransportBooking.New(Booking, Factory);
			AssertNotNull("Created wrapper should not be null", BookingWrapper);

			base.SetUp();
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { BookingWrapper };
		}

		#endregion
	}
}

using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(InstructionWrapperCollection))]
	sealed class InstructionWrapperCollectionTest : GenericWrapperCollectionTest<InstructionWrapperCollection>
	{
		#region TestIndex

		public void TestIndex()
		{
			var consoldiation = BookingHelper.CreateConsolidation();
			var booking = consoldiation.Bookings.AddNew();

			// pickup
			var pickupInstruction = booking.Instructions.AddNew();
			pickupInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			var pickupConfirmation = pickupInstruction.Confirmations.AddNew();
			pickupConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			pickupConfirmation.KK_ReceivedBy = "Bob";

			var bookingWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			var collection = bookingWrapper.BookingInstructions;
			AssertEquals("Bob", collection["FirstPickup"].ReceivedBy);
			AssertNull(collection["LastDelivery"]);

			// multi - with Pickup and Delivery Confirmations
			var multiInstruction = booking.Instructions.AddNew();
			multiInstruction.KN_InstructionType = InstructionTypes.Codes.Multi;
			var deliveryMultiConfirmation = multiInstruction.Confirmations.AddNew();
			deliveryMultiConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.Delivery;
			deliveryMultiConfirmation.KK_ReceivedBy = "Jason";
			var pickupMultiConfirmation = multiInstruction.Confirmations.AddNew();
			pickupMultiConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.PickUp;
			pickupMultiConfirmation.KK_ReceivedBy = "Tim";

			bookingWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			collection = bookingWrapper.BookingInstructions;
			AssertEquals("Bob", collection["FirstPickup"].ReceivedBy);
			AssertNull(collection["LastDelivery"]);

			// delivery
			var deliveryInstruction = booking.Instructions.AddNew();
			deliveryInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			var deliveryConfirmation = deliveryInstruction.Confirmations.AddNew();
			deliveryConfirmation.KK_ConfirmationType = ConfirmationTypes.Codes.Delivery;
			deliveryConfirmation.KK_ReceivedBy = "Bill";

			bookingWrapper = new FreightWrapperFromDtbBooking(booking, Factory);
			collection = bookingWrapper.BookingInstructions;
			AssertEquals("Bob", collection["FirstPickup"].ReceivedBy);
			AssertEquals("Bill", collection["LastDelivery"].ReceivedBy);
		}

		#endregion

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			return new BookingInstructionWrapper(Factory.NewWithValidTestData<DtbBookingInstruction>(), Factory);
		}

		protected override InstructionWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new InstructionWrapperCollection(Factory);
		}

		#region BookingHelper

		TransportBookingTestHelper BookingHelper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;

		#endregion
	}
}

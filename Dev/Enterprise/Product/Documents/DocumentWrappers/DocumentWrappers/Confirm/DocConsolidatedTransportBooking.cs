using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocConsolidatedTransportBooking : DocBaseWrapper
	{
		protected DocConsolidatedTransportBooking(CommonConsolidatedTransportBooking booking, BusinessObjectFactory factoryToWrap)
			: base(booking, factoryToWrap)
		{
		}

		public static DocConsolidatedTransportBooking New(CommonConsolidatedTransportBooking booking, BusinessObjectFactory factoryToWrap)
		{
			return new DocConsolidatedTransportBooking(booking, factoryToWrap);
		}

		protected CommonConsolidatedTransportBooking Booking
		{
			get { return (CommonConsolidatedTransportBooking)WrappedObject; }
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		public ZString Context
		{
			get { return "TRANSPORTBOOKING"; }
		}

		public DocPickupDeliveryConfirmCollection Confirms
		{
			get { return new DocPickupDeliveryConfirmCollection(Booking.Confirms); }
		}

		public ZString EmailSubjectNumber
		{
			get { return BookingID; }
		}

		#region BookingID

		public ZString BookingID
		{
			get { return Booking.D1_UniqueConsignRef; }
		}

		#endregion

		#region Customer

		public DocOrganisation Customer
		{
			get { return CustomerAddress != null ? CustomerAddress.Organisation : null; }
		}

		public DocAddress CustomerAddress
		{
			get { return DocAddress.New(Booking.Customer, Factory); }
		}

		#endregion
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromJobSupplierBooking : FreightWrapper
	{
		public FreightWrapperFromJobSupplierBooking(JobSupplierBooking jobSupplierBooking, BusinessObjectFactory factory) : base(jobSupplierBooking, factory)
		{
			this.jobSupplierBooking = jobSupplierBooking;
		}
		readonly JobSupplierBooking jobSupplierBooking;

		protected override ZGuid GetTrackingBusinessObjectPK()
		{
			return jobSupplierBooking.PK;
		}

		protected override DocJobSupplierBooking GetJobSupplierBookingWrapper()
		{
			return DocJobSupplierBooking.New(jobSupplierBooking, Factory);
		}

		protected override ZString GetJobNumber()
		{
			return jobSupplierBooking?.JSB_BookingId ?? ZString.Empty;
		}
	}
}

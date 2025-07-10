using CargoWise.Types;
using Enterprise.Integration.TransportCommon;

namespace Enterprise.Integration.TransportBooking
{
	public interface IDtbBookingConsolidation : IDtbTransportConsolidation
	{
		ZString KB_JobType { get; set; }

		IDtbBooking[] Bookings { get; }

		ZString KB_JobDirection { get; set; }
		ZGuid KB_KB_MasterBookingConsolidation { get; set; }
	}
}

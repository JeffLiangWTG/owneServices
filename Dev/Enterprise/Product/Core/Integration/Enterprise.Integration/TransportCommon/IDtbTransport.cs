using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.TransportCommon
{
	public interface IDtbTransport : IBusiness
	{
		ZGuid PK { get; }

		ZGuid KM_KB_Booking { get; set; }
		ZDateTime KM_BookingOfTransportRequestedDate { get; set; }
		ZGuid KM_KB_BookingConsolidationMultiJob { get; set; }
		ZString KM_TransportReference { get; set; }
		ZString KM_RatingFreightMode { get; set; }
		ZString KM_KT_NKBookingTemplate { get; set; }
		ZString KM_JobID { get; set; }

		ZGuid JobHeaderPK { get; }
	}
}

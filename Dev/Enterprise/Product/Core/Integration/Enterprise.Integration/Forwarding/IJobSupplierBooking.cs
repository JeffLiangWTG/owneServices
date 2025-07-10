using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Forwarding
	{
		public interface IJobSupplierBooking
		{
			ZDate JSB_BookedOnDate { get; set; }
			ZString JSB_BookingId { get; set; }
			ZDate JSB_CargoAvailableDate { get; set; }
			ZString JSB_GoodsDescription { get; set; }
			ZString JSB_IncoTerm { get; set; }
			ZString JSB_LoadMode { get; set; }
			ZString JSB_MarksAndNumbers { get; set; }
			ZGuid JSB_OH_BookingParty { get; set; }
			ZString JSB_Status { get; set; }
			ZString JSB_TransportMode { get; set; }
		}
	}
}

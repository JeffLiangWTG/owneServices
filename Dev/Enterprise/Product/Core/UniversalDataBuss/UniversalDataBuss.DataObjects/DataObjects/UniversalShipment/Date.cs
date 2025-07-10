using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class Date : IDataObject
	{
		public static Date New(DateType dateType, ZBool isEstimate, UXmlDateTime value)
		{
			return new Date()
			{
				Type = dateType,
				IsEstimate = isEstimate,
				Value = value,
			};
		}

		[Mandatory, CandidateKey]
		public DateType? Type { get; set; }
		[CandidateKey]
		public ZBool? IsEstimate { get; set; }
		[Mandatory]
		public UXmlDateTime? Value { get; set; }
	}

	public enum DateType
	{
		AvailableExFactory,
		Pickup,
		Pack,
		Departure,
		FirstArrivalInCountry,
		Arrival,
		Unpack,
		Delivery,
		BillIssued,
		ShippedOnBoard,
		WarehouseRelease,
		BookingConfirmed,
		DeliveryRequiredBy,
		DepartureVesselCutoffDate,
		ExWorksRequiredBy,
		FollowUp,
		ClientRequestedETA,
		Received,
		LoadingDate,
		DischargeDate,
		EntrySubmitted,
		EntryAuthorisation,
		LocalTransportPickup,
		LocalTransportDelivery,
		LocalTransportCompleted,
		OrderDate,
		CutOffDate,
		FirstForeignArrival,
		LastForeignDeparture,
		EntryDate,
		TransferOfLiability,
		ISFLastAccepted,
		BillRequiredBy,
		DateAtOffice,
		JobCreated,
		TransportBookingRequested,
		EarliestDeparture,
		LatestDelivery,
		CargoReceiptDate,
		VesselStayStartDate,
		VesselStayEndDate,
		HandlingDate,
		PickupReceiptRequested,
		DeliveryReceiptRequested,
		DepartureReceiptRequested,
		ArrivalReceiptRequested,
		PickupDispatchRequested,
		DeliveryDispatchRequested,
		DepartureDispatchRequested,
		ArrivalDispatchRequested,
		BookedOnDate,
		CargoAvailableDate,
		Start,
		End,
		Accepted,
		ClientAccepted,
		DeliveryDueDate,
		RevisedDeliveryDueDate,
		DateLimit,
		ShipmentWindowStart,
		ShipmentWindowEnd,
		ActualArrival,
		Presentation,
		Acceptance,
		Release,
	}
}

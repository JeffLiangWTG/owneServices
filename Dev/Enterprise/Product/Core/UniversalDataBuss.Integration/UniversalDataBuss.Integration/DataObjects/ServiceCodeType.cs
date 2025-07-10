namespace Enterprise.UniversalDataBuss.Integration
{
	/// <summary>
	/// List of all possible Service Codes available to send Universal Data to.
	/// MUST keep this in sync with Enterprise.UniversalDataBuss.Integration.DataObjects.ServiceCodesList
	/// ALSO: Please keep it in alphabetical order. Thanks!
	/// </summary>
	public enum ServiceCodeType
	{
		AIR, // Air Freight
		AMD, // Amend
		BRQ, // BookingRequest
		CBA, // InvoiceCostsFromCarrierBookingAgent
		CPA, // ContainerYardPreArrival
		CRO, // ContainerYardReleaseOrder
		GTB, // GateBooking
		HLD, // Hold
		SEA, // Sea Freight
		SIN, // ShippingInstruction
		TWD, // TransitWarehouseDispatch
		TWR, // TransitWarehouseReceive
		TWX, // TransitWarehouseReceiveAndDispatch
		UCK, // Unlock
		VGM, // VerifiedGrossContainerWeight
		TWP, // TransitWarehousePrepareDispatch
	}

	public static class ServiceCodeTypeExtension
	{
		public static string GetDescription(this ServiceCodeType serviceCodeType)
		{
			return new DataObjects.ServiceCodesList().GetDescriptionFromCode(serviceCodeType.ToString());
		}
	}
}

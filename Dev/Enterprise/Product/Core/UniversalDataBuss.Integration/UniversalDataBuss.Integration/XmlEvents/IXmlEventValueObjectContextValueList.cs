using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IXmlEventValueObjectContextValueList
	{
		ZString SourceEventCode { get; }
		ZString MAWBNumber { get; }
		ZString? MBOLNumber { get; }
		ZString HAWBNumber { get; }
		ZString? HBOLNumber { get; }
		ZString MAWBOriginIATAAirportCode { get; }
		ZString MAWBDestinationIATAAirportCode { get; }
		ZInt MAWBNumberOfPieces { get; }
		ZString OtherServiceInformation { get; }
		ZBool MessageIsPartial { get; }
		ZInt MessageNumberOfPieces { get; }
		ZString MessageWeightOfGoods { get; }
		ZString IATACarrierCode { get; }
		ZString IATAAirportCode { get; }
		ZInt NumberOfPieces { get; }
		ZString PackageType { get; }
		ZString WeightOfGoods { get; }
		ZString ReceivedFromName { get; }
		ZBool IsPartial { get; }
		ZString VolumeOfGoods { get; }
		ZString DensityGroup { get; }
		ZString? FlightNumber { get; }
		ZDateTime FlightDate { get; }
		ZDateTime? TimeOfDeparture { get; }
		ZDateTime? TimeOfArrival { get; }
		ZDateTime? CustomsReleaseDate { get; }
		ZString OriginIATAAirportCode { get; }
		ZString DestinationIATAAirportCode { get; }
		ZString LegOriginUNLOCO { get; }
		ZString LegDestinationUNLOCO { get; }
		List<ZString> ContainerNumbers { get; }
		ZString ContainerISOCode { get; }
		ZString ContainerReleaseNumber { get; }
		ZString ContainerAcceptanceNumber { get; }
		ZString ContainerOwnershipType { get; }
		ZBool IsEmptyContainer { get; }
		ZString ContainerGrossWeight { get; }
		ZString ContainerWeightUnit { get; }
		ZDateTime? GateInTime { get; }
		ZDateTime? GateOutTime { get; }
		ZString PortUNLOCO { get; }
		ZString ContainerSealNo { get; }
		ZString ContainerSealNo2 { get; }
		ZString ContainerSealNo3 { get; }
		ZString ContainerMovementType { get; }
		ZString EventActionUNLOCO { get; }
		ZString RecipientName { get; }
		ZString DiscrepancyCode { get; }
		List<ZString> ULDIdentifications { get; }
		ZString? VoyageNumber { get; }
		ZString? VesselName { get; }
		ZString? LloydsNumber { get; }
		ZString HAWBOriginIATAAirportCode { get; }
		ZString HAWBDestinationIATAAirportCode { get; }
		ZString MBOLOriginUNLOCO { get; }
		ZString MBOLDestinationUNLOCO { get; }
		ZString? MBOLPortOfLoadingScheduleK { get; }
		ZString? MBOLPortOfDischargeScheduleD { get; }
		ZString? MBOLPortOfEntryScheduleD { get; }
		ZString HBOLOriginUNLOCO { get; }
		ZString HBOLDestinationUNLOCO { get; }
		ZString AgentsReference { get; }
		ZString CarriersBookingReference { get; }
		ZString ShippersReference { get; }
		ZString OrderNumber { get; }
		ZByte OrderNumberSplit { get; }
		ZString InterimReceipt { get; }
		ZString CFSReference { get; }
		ZString DeclarationReference { get; }
		ZString CommercialInvoiceNumber { get; }
		ZString TransportBookingJobID { get; }
		ZString TransportBookingInstructionID { get; }
		ZString TransportBookingPackageID { get; }
		ZString ClientReference { get; }
		ZString TransportReference { get; }
		ZString ReceiveReference { get; }
		ZByte ReceiveReferenceSplit { get; }
		ZString AdjustmentReference { get; }
		ZString EntryNumber { get; }
		ZString EntryNumberType { get; }
		ZString EntryNumberCountryOfIssue { get; }

		ZString EntryType { get; }
		ZString EntryCountry { get; }

		ZString CoLoadBillNumber { get; }
		ZString CoLoadBookingReference { get; }
		ZString CoLoadWithC1CCode { get; }
		ZString CoLoadWithName { get; }

		ZString SubscriptionType { get; }

		ZString WaybillNumber { get; }
		ZString QuoteNumber { get; }
		ZString ConsignmentNoteNumber { get; }
		ZString DepotCode { get; }
		ZString GoodsDeclarationNumber { get; }
		ZString PositioningDateTime { get; }
		ZString ContainerConditionCode { get; }
		ZString ContainerConsigneeName { get; }
		ZString ContainerConsignorName { get; }
		ZString ContainerDamageCode { get; }
		ZString ContainerDestinationUNLOCO { get; }
		ZString ContainerFinalDestinationUNLOCO { get; }
		ZString ContainerGoodsDescription { get; }
		ZString ContainerOwnerName { get; }
		ZString ContainerTemperatureSetting { get; }
		ZString ContainerLeaseNumber { get; }
		ZString EstimatedTimeOfArrival { get; }
		ZString EstimatedTimeOfDeparture { get; }
		ZString GoodsDescription { get; }
		ZString VesselCallSign { get; }
		ZString TransportMode { get; }
		ZBool IsCharter { get; }
		ZString GoodsItemID { get; }
		ZString? CarrierCode { get; }
		ZString? CarrierC1CCode { get; }
		ZString? PortOfLoadingUNLOCO { get; }
		ZString? PortOfLoadingSuffix { get; }
		ZString? NotificationDetails { get; }
		ZString OrderTrackingNumber { get; }
		ZString FileName { get; }
		ZString? MessageStatus { get; }
		ZString? RexNumber { get; }
		ZString? ComplianceStatus { get; }
		ZString? ECNNumber { get; }
		ZString? RexResponseType { get; }
		ZString? NotificationTitle { get; }
		ZString? NotificationText { get; }
		ZString? NotificationType { get; }
		ZString ProcessingLog { get; }
		ZString ProcessingStatusCode { get; }
		ZString WarehouseCode { get; }
		ZString ClientCode { get; }
		ZInt? PackageSequence { get; }
		ZString? PickNumber { get; }
		ZString? PackageTypeUOM { get; }
		ZDecimal? Length { get; }
		ZDecimal? Width { get; }
		ZDecimal? Height { get; }
		ZString? DimensionUnit { get; }

		ZString FailureReason { get; }

		ZString InternalTransactionNumber { get; }

		ZString? MasterHouseBill { get; }
		ZString? MessageType { get; }

		ZString? EHubTrackingID { get; }
		ZString? ConversationID { get; }
		ZString? NotificationMessageID { get; }
		ZString? CSPEntryTrackingID { get; }
		ZString? CorrelationID { get; }
		ZString? InterchangeNumber { get; }

		ZString? USLowValueEntriesMatchingKey { get; }
		ZString? USLowValueEntriesUseCode { get; }

		ZString? WarehouseReleaseStatus { get; }

		ZString? ResponseText { get; }

		ZString? ErrorSummary { get; }
		ZString? ErrorDescription { get; }

		ZString? StatusCode { get; }
		ZString? EntryReference { get; }

		ZInt? OrderLineNumber { get; }
		ZInt? OrderLineSubLineNumber { get; }

		ZString? ClearanceReferenceNumber { get; }

		ZInt? OuterPackQty { get; }
		ZInt? InnerPackQty { get; }

		ZString? LegDestinationTerminalCode { get; }

		ZString? ServiceId { get; }
		ZString? ExternalServiceId { get; }
		ZString? ServiceType { get; }
		ZDecimal? ServiceCount { get; }
		ZString? ServiceContractor { get; }
		ZString? ServiceNotes { get; }
		ZDecimal? ServiceRate { get; }
		ZString? ServiceRateCurrency { get; }
		ZString? ServiceMeasurementBasis { get; }
		ZString? ServiceSubLocation { get; }
		ZString? ServiceLocation { get; }
		ZString? ServiceLocationAddress { get; }
		ZDateTime? ServiceDuration { get; }
		ZString? ServiceReference { get; }
		ZString? ShipmentNumber { get; }

		ZString? GateBookingNumber { get;  }
		ZString? MovementBookingNumber { get; }
		ZString? VBSNotificationID { get; }
		ZString? Direction { get; }
		ZString? VehicleRegistration { get; }

		ZString? CarrierShipmentReference { get; }

		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> Values { get; }

		void ClearInvalidMBOL();
		ZString GetErrorLog();
	}
}

using System.Collections.Generic;
using System.Reflection;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	public partial class Event : IXmlEventValueObject
	{
		ZDateTimeOffset IXmlEventValueObject.CreatedTime
		{
			get { return CreatedTime.GetValueOrDefault(); }
		}

		ZString IXmlEventValueObject.EventReference
		{
			get { return EventReference.GetValueOrDefault(); }
		}

		ZDateTimeOffset IXmlEventValueObject.EventTime
		{
			get { return EventTime.GetValueOrDefault(); }
		}

		ZString IXmlEventValueObject.EventType
		{
			get { return EventType.GetValueOrDefault(); }
		}

		ZBool IXmlEventValueObject.IsEstimate
		{
			get { return IsEstimate.GetValueOrDefault(); }
		}

		ZBool IXmlEventValueObject.IsCancelled
		{
			get { return IsCancelled.GetValueOrDefault(); }
		}

		IXmlEventValueObjectContextValueList IXmlEventValueObject.Context
		{
			get { return context ?? (context = new ContextValueList(ContextCollection)); }
		}
		IXmlEventValueObjectContextValueList context;

		public void ValidateContextValues(ISimpleLogger logger)
		{
			new ContextValueList(ContextCollection, logger);
		}

		class ContextValueList : IXmlEventValueObjectContextValueList
		{
			internal ContextValueList(IEnumerable<Context> contexts, ISimpleLogger logger = null)
			{
				logger = logger ?? new DummyLogger();
				if (contexts != null)
				{
					foreach (var context in contexts)
					{
						var contextType = context.Type;
						if (contextType != null && contextType.Type.HasValue && context.Value.HasValue)
						{
							AddValue(contextType, context.Value.Value, logger);

							if (contextType.Type.Value == nameof(ContextTypes.FailureReason))
							{
								errorLog = context.GetErrorLog();
							}
						}
					}
				}
			}

			#region Values Implmentation

			void AddValue(ContextType contextType, ZString valueAsString, ISimpleLogger logger)
			{
				IZType value = valueAsString;

				var interfacePropertyName = GetInterfacePropertyName(contextType);
				var typeCode = contextType.Type.Value;
				var typeDescription = contextType.Description.GetValueOrDefault();

				var propertyInfo = typeof(ContextValueList).GetProperty(interfacePropertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
				if (propertyInfo != null)
				{
					if (propertyInfo.PropertyType.IsAssignableFrom(typeof(List<ZString>)))
					{
						var propertyValue = propertyInfo.GetValue(this);
						if (propertyValue == null)
						{
							propertyInfo.SetValue(this, new List<ZString>());
						}

						((List<ZString>)propertyInfo.GetValue(this)).Add(valueAsString);
					}
					else
					{
						IZType typedValue = propertyInfo.TryConvertValueToPropertyType(valueAsString, logger, Res.GetString("aec51ef3-b320-45d9-b272-0fbe5befb0b8", "Context Collection Value from Key [{0}]", interfacePropertyName));
						if (typedValue != null && typedValue.IsValid)
						{
							propertyInfo.SetValue(this, typedValue, null);
							value = typedValue;
						}
					}
				}

				values.Add(new KeyValuePair<TypeWithDescription, IZType>(new TypeWithDescription(typeCode, typeDescription), value));
			}

			static string GetInterfacePropertyName(ContextType contextType)
			{
				return contextType.Type.Value == nameof(ContextTypes.ContainerNumber)
					? nameof(ContextTypes.ContainerNumber) + "s"
					: contextType.Type.Value == nameof(ContextTypes.ULDIdentification)
						? nameof(ContextTypes.ULDIdentification) + "s"
						: contextType.Type.Value.ToString();
			}

			readonly List<KeyValuePair<TypeWithDescription, IZType>> values = new List<KeyValuePair<TypeWithDescription, IZType>>();

			public IEnumerable<KeyValuePair<TypeWithDescription, IZType>> Values
			{
				get { return values; }
			}

			#endregion

			public ZString SourceEventCode { get; private set; }

			public ZString MAWBNumber { get; private set; }
			public ZString MAWBOriginIATAAirportCode { get; private set; }
			public ZString MAWBDestinationIATAAirportCode { get; private set; }
			public ZInt MAWBNumberOfPieces { get; private set; }
			public ZString OtherServiceInformation { get; private set; }

			public ZBool MessageIsPartial { get; private set; }
			public ZInt MessageNumberOfPieces { get; private set; }
			public ZString MessageWeightOfGoods { get; private set; }

			public ZString IATACarrierCode { get; private set; }
			public ZString IATAAirportCode { get; private set; }
			public ZInt NumberOfPieces { get; private set; }
			public ZString PackageType { get; private set; }
			public ZString WeightOfGoods { get; private set; }
			public ZString ReceivedFromName { get; private set; }
			public ZBool IsPartial { get; private set; }
			public ZString VolumeOfGoods { get; private set; }
			public ZString DensityGroup { get; private set; }
			public ZString? FlightNumber { get; private set; }
			public ZDateTime FlightDate { get; private set; }
			public ZDateTime? TimeOfDeparture { get; private set; }
			public ZDateTime? TimeOfArrival { get; private set; }
			public ZDateTime? CustomsReleaseDate { get; private set; }
			public ZString OriginIATAAirportCode { get; private set; }
			public ZString DestinationIATAAirportCode { get; private set; }
			public ZString LegOriginUNLOCO { get; private set; }
			public ZString LegDestinationUNLOCO { get; private set; }
			public ZString RecipientName { get; private set; }
			public ZString DiscrepancyCode { get; private set; }
			public List<ZString> ULDIdentifications { get; private set; }
			public ZString HAWBNumber { get; private set; }
			public ZString? HBOLNumber { get; private set; }
			public ZString? MBOLNumber { get; private set; }
			public ZString GoodsDescription { get; private set; }
			public List<ZString> ContainerNumbers { get; private set; }
			public ZString ContainerISOCode { get; private set; }
			public ZString ContainerReleaseNumber { get; private set; }
			public ZString ContainerAcceptanceNumber { get; private set; }
			public ZString ContainerOwnershipType { get; private set; }
			public ZString ContainerOwnerName { get; private set; }
			public ZString ContainerType { get; private set; }
			public ZBool IsEmptyContainer { get; private set; }
			public ZString ContainerGrossWeight { get; private set; }
			public ZString ContainerWeightUnit { get; private set; }
			public ZDateTime? GateInTime { get; private set; }
			public ZDateTime? GateOutTime { get; private set; }
			public ZString PortUNLOCO { get; private set; }
			public ZString ContainerSealNo { get; private set; }
			public ZString ContainerSealNo2 { get; private set; }
			public ZString ContainerSealNo3 { get; private set; }
			public ZString ContainerMovementType { get; private set; }
			public ZString ContainerDamageCode { get; private set; }
			public ZString ContainerConditionCode { get; private set; }
			public ZString ContainerDestinationUNLOCO { get; private set; }
			public ZString ContainerFinalDestinationUNLOCO { get; private set; }
			public ZString ContainerConsigneeName { get; private set; }
			public ZString ContainerConsignorName { get; private set; }
			public ZString ContainerTemperatureSetting { get; private set; }
			public ZString ContainerGoodsDescription { get; private set; }
			public ZString ContainerLeaseNumber { get; private set; }
			public ZString EventActionUNLOCO { get; private set; }
			public ZString? VoyageNumber { get; private set; }
			public ZString? VesselName { get; private set; }
			public ZString VesselCallSign { get; private set; }
			public ZString? LloydsNumber { get; private set; }
			public ZString HAWBOriginIATAAirportCode { get; private set; }
			public ZString HAWBDestinationIATAAirportCode { get; private set; }
			public ZString MBOLOriginUNLOCO { get; private set; }
			public ZString MBOLDestinationUNLOCO { get; private set; }
			public ZString? MBOLPortOfLoadingScheduleK { get; private set; }
			public ZString? MBOLPortOfDischargeScheduleD { get; private set; }
			public ZString? MBOLPortOfEntryScheduleD { get; private set; }
			public ZString HBOLOriginUNLOCO { get; private set; }
			public ZString HBOLDestinationUNLOCO { get; private set; }
			public ZString EstimatedTimeOfArrival { get; private set; }
			public ZString EstimatedTimeOfDeparture { get; private set; }
			public ZString AgentsReference { get; private set; }
			public ZString CarriersBookingReference { get; private set; }
			public ZString ShippersReference { get; private set; }
			public ZString CFSReference { get; private set; }
			public ZString DeclarationReference { get; private set; }
			public ZString InterimReceipt { get; private set; }
			public ZString OrderNumber { get; private set; }
			public ZByte OrderNumberSplit { get; private set; }
			public ZString CommercialInvoiceNumber { get; private set; }

			public ZString TransportBookingJobID { get; private set; }
			public ZString TransportBookingInstructionID { get; private set; }
			public ZString TransportBookingPackageID { get; private set; }

			public ZString ClientReference { get; private set; }
			public ZString TransportReference { get; private set; }
			public ZString ReceiveReference { get; private set; }
			public ZByte ReceiveReferenceSplit { get; private set; }
			public ZString AdjustmentReference { get; private set; }

			public ZString EntryNumber { get; private set; }
			public ZString EntryNumberType { get; private set; }
			public ZString EntryNumberCountryOfIssue { get; private set; }

			public ZString QuoteNumber { get; private set; }
			public ZString WaybillNumber { get; private set; }
			public ZString ConsignmentNoteNumber { get; private set; }

			public ZString DepotCode { get; private set; }
			public ZString GoodsDeclarationNumber { get; private set; }
			public ZString PositioningDateTime { get; private set; }
			public ZString TransportMode { get; private set; }
			public ZBool IsCharter { get; private set; }
			public ZString GoodsItemID { get; private set; }

			public ZString FailureReason { get; private set; }

			public ZString InternalTransactionNumber { get; private set; }

			public ZString? MasterHouseBill { get; private set; }
			public ZString? MessageType { get; private set; }

			public ZString? CarrierCode { get; private set; }
			public ZString? CarrierC1CCode { get; private set; }
			public ZString? PortOfLoadingUNLOCO { get; private set; }
			public ZString? PortOfLoadingSuffix { get; private set; }
			public ZString? NotificationDetails { get; private set; }

			public ZString OrderTrackingNumber { get; private set; }
			public ZString FileName { get; private set; }

			public ZString? MessageStatus { get; private set; }
			public ZString? RexNumber { get; private set; }
			public ZString? ComplianceStatus { get; private set; }
			public ZString? ECNNumber { get; private set; }
			public ZString? RexResponseType { get; private set; }
			public ZString? NotificationTitle { get; private set; }
			public ZString? NotificationText { get; private set; }
			public ZString? NotificationType { get; private set; }
			public ZString ProcessingLog { get; private set; }
			public ZString ProcessingStatusCode { get; private set; }
			public ZString WarehouseCode { get; private set; }
			public ZString ClientCode { get; private set; }
			public ZInt? PackageSequence { get; private set; }
			public ZString? PickNumber { get; private set; }
			public ZString? PackageTypeUOM { get; private set; }
			public ZDecimal? Length { get; private set; }
			public ZDecimal? Width { get; private set; }
			public ZDecimal? Height { get; private set; }
			public ZString? DimensionUnit { get; private set; }

			public ZString? EHubTrackingID { get; private set; }
			public ZString? ConversationID { get; private set; }
			public ZString? NotificationMessageID { get; private set; }
			public ZString? ResponseText { get; private set; }
			public ZString? ErrorSummary { get; private set; }
			public ZString? ErrorDescription { get; private set; }
			public ZString? CSPEntryTrackingID { get; private set; }
			public ZString? CorrelationID { get; private set; }
			public ZString? InterchangeNumber { get; private set; }

			public ZString? USLowValueEntriesMatchingKey { get; private set; }
			public ZString? USLowValueEntriesUseCode { get; private set; }

			public ZString? WarehouseReleaseStatus { get; private set; }

			public ZString? StatusCode { get; private set; }
			public ZString? EntryReference { get; private set; }

			public ZInt? OrderLineNumber { get; private set; }
			public ZInt? OrderLineSubLineNumber { get; private set; }

			public ZString EntryType { get; private set; }

			public ZString EntryCountry { get; private set; }

			public ZString CoLoadBillNumber { get; private set; }
			public ZString CoLoadBookingReference { get; private set; }
			public ZString CoLoadWithC1CCode { get; private set; }
			public ZString CoLoadWithName { get; private set; }

			public ZString SubscriptionType { get; private set; }

			public ZString? ClearanceReferenceNumber { get; private set; }

			public ZInt? OuterPackQty { get; private set; }

			public ZInt? InnerPackQty { get; private set; }

			public ZString? LegDestinationTerminalCode { get; private set; }

			public ZString? ServiceId { get; private set; }
			public ZString? ExternalServiceId { get; private set; }
			public ZString? ServiceType { get; private set; }
			public ZDecimal? ServiceCount { get; private set; }
			public ZString? ServiceContractor { get; private set; }
			public ZString? ServiceNotes { get; private set; }
			public ZDecimal? ServiceRate { get; private set; }
			public ZString? ServiceRateCurrency { get; private set; }
			public ZString? ServiceMeasurementBasis { get; private set; }
			public ZString? ServiceSubLocation { get; private set; }
			public ZString? ServiceLocation { get; private set; }
			public ZString? ServiceLocationAddress { get; private set; }
			public ZDateTime? ServiceDuration { get; private set; }
			public ZString? ServiceReference { get; private set; }
			public ZString? ShipmentNumber { get; private set; }

			public ZString? GateBookingNumber { get; private set; }
			public ZString? MovementBookingNumber { get; private set; }
			public ZString? VBSNotificationID { get; private set; }
			public ZString? Direction { get; private set; }
			public ZString? VehicleRegistration { get; private set; }

			public ZString? CarrierShipmentReference { get; private set; }

			public ZString GetErrorLog()
			{
				return errorLog.IsEmpty ? FailureReason : errorLog;
			}
			readonly ZString errorLog;

			public void ClearInvalidMBOL()
			{
				MBOLNumber = null;
			}
		}

		public enum ContextTypes
		{
			SourceEventCode,
			MAWBNumber,
			MAWBOriginIATAAirportCode,
			MAWBDestinationIATAAirportCode,
			MAWBNumberOfPieces,
			OtherServiceInformation,
			MessageIsPartial,
			MessageNumberOfPieces,
			MessageWeightOfGoods,
			IATACarrierCode,
			IATAAirportCode,
			NumberOfPieces,
			PackageType,
			WeightOfGoods,
			ReceivedFromName,
			IsPartial,
			VolumeOfGoods,
			DensityGroup,
			FlightNumber,
			FlightDate,
			TimeOfDeparture,
			TimeOfArrival,
			CustomsReleaseDate,
			OriginIATAAirportCode,
			DestinationIATAAirportCode,
			LegOriginUNLOCO,
			LegDestinationUNLOCO,
			RecipientName,
			DiscrepancyCode,
			ULDIdentification,
			HAWBNumber,
			HBOLNumber,
			MBOLNumber,
			GoodsDescription,
			ContainerNumber,
			ContainerISOCode,
			ContainerReleaseNumber,
			ContainerAcceptanceNumber,
			ContainerOwnershipType,
			ContainerOwnerName,
			IsEmptyContainer,
			ContainerGrossWeight,
			ContainerWeightUnit,
			GateInTime,
			GateOutTime,
			PortUNLOCO,
			ContainerSealNo,
			ContainerSealNo2,
			ContainerSealNo3,
			ContainerMovementType,
			ContainerDamageCode,
			ContainerConditionCode,
			ContainerDestinationUNLOCO,
			ContainerFinalDestinationUNLOCO,
			ContainerConsigneeName,
			ContainerConsignorName,
			ContainerTemperatureSetting,
			ContainerGoodsDescription,
			ContainerLeaseNumber,
			EventActionUNLOCO,
			VoyageNumber,
			VesselName,
			VesselCallSign,
			LloydsNumber,
			HAWBOriginIATAAirportCode,
			HAWBDestinationIATAAirportCode,
			MBOLOriginUNLOCO,
			MBOLDestinationUNLOCO,
			MBOLPortOfLoadingScheduleK,
			MBOLPortOfDischargeScheduleD,
			MBOLPortOfEntryScheduleD,
			HBOLOriginUNLOCO,
			HBOLDestinationUNLOCO,
			EstimatedTimeOfArrival,
			EstimatedTimeOfDeparture,
			AgentsReference,
			CarriersBookingReference,
			ShippersReference,
			CFSReference,
			DeclarationReference,
			InterimReceipt,
			OrderNumber,
			OrderNumberSplit,
			CommercialInvoiceNumber,
			TransportBookingJobID,
			TransportBookingInstructionID,
			TransportBookingPackageID,
			ClientReference,
			TransportReference,
			ReceiveReference,
			ReceiveReferenceSplit,
			AdjustmentReference,
			EntryNumber,
			EntryNumberType,
			EntryNumberCountryOfIssue,
			EntryReference,
			EntryType,
			EntryCountry,
			CoLoadBillNumber,
			CoLoadBookingReference,
			CoLoadWithC1CCode,
			CoLoadWithName,
			SubscriptionType,
			QuoteNumber,
			WaybillNumber,
			ConsignmentNoteNumber,
			DepotCode,
			GoodsDeclarationNumber,
			PositioningDateTime,
			TransportMode,
			IsCharter,
			GoodsItemID,
			FailureReason,
			InternalTransactionNumber,
			MasterHouseBill,
			MessageType,
			CarrierCode,
			CarrierC1CCode,
			PortOfLoadingUNLOCO,
			PortOfLoadingSuffix,
			NotificationDetails,
			OrderTrackingNumber,
			FileName,
			MessageStatus,
			RexNumber,
			ComplianceStatus,
			ECNNumber,
			RexResponseType,
			NotificationTitle,
			NotificationText,
			ProcessingLog,
			ProcessingStatusCode,
			WarehouseCode,
			ClientCode,
			PackageSequence,
			PickNumber,
			PackageTypeUOM,
			Length,
			Width,
			Height,
			DimensionUnit,
			EHubTrackingID,
			ConversationID,
			NotificationMessageID,
			CSPEntryTrackingID,
			CorrelationID,
			USLowValueEntriesMatchingKey,
			USLowValueEntriesUseCode,
			WarehouseReleaseStatus,
			NotificationType,
			OrderLineNumber,
			OrderLineSubLineNumber,
			ClearanceReferenceNumber,
			OuterPackQty,
			InnerPackQty,
			LegDestinationTerminalCode,
			ServiceId,
			ExternalServiceId,
			ServiceType,
			ServiceCount,
			ServiceContractor,
			ServiceNotes,
			ServiceRate,
			ServiceRateCurrency,
			ServiceMeasurementBasis,
			ServiceSubLocation,
			ServiceLocation,
			ServiceLocationAddress,
			ServiceDuration,
			ServiceReference,
			ShipmentNumber,
			VBSNotificationID,
			MovementBookingNumber,
			GateBookingNumber,
			Direction,
			VehicleRegistration,
			CarrierShipmentReference
		}
	}
}


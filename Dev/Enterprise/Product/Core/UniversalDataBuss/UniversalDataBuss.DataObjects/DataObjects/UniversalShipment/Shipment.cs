using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema("UniversalShipment.xsd"), RootElement("UniversalShipment")]
	[DataObjectAttributes(nameof(AllowUpdateOfCustomsDeclarationAfterCommencement))]
	public partial class Shipment : TopLevelDataObject,
		IJobCostingData,
		IConsolCostsData,
		ICustomizedFieldContainer,
		IOrganizationAddressCollectionParent,
		IAddInfoCollectionParent,
		IAddInfoGroupCollectionParent,
		ICustomsReferenceCollectionParent,
		ICustomsSupportingInformationCollectionParent,
		IValidationRuleCollectionParent,
		IPackageParentDataObject,
		IMilestoneCollectionParent,
		IExceptionCollectionParent,
		IAttachedDocumentContainer,
		IDataObjectParseSupporter,
		IDisposable
	{
		public Shipment()
		{
		}

		public Shipment(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[NamespaceDependent(UniversalXmlInfo.Namespace_2011_11, typeof(_2011_11.DataContext))]
		[NamespaceDependent(UniversalXmlInfo.Namespace_2012_11, typeof(_2012_11.DataContext))]
		[ReferenceProperty]
		public override IDataContextDataObject DataContext { get; set; }

		#region Properties as per the XSD

		[RestrictedProperty(nameof(HideRestrictedProperty))]
		public ZBool? AllowUpdateOfCustomsDeclarationAfterCommencement { get; set; }
		public static bool HideRestrictedProperty => true;
		public ZDecimal? ActualChargeable { get; set; }
		[MaxLength(50)]
		public ZString? AdditionalTerms { get; set; }
		[MaxLength(35)]
		public ZString? AgentsReference { get; set; }
		[MaxLength(5)]
		public ZString? AgreedPlaceCode { get; set; }
		public CodeDescriptionPair AWBServiceLevel { get; set; }
		[MaxLength(35)]
		public ZString? BookingConfirmationReference { get; set; }
		public CodeDescriptionPair BillType { get; set; }
		public CodeDescriptionPair BillTerms { get; set; }
		public CodeDescriptionPair ElectronicBillOfLadingStatus { get; set; }
		public ZShort? ElectronicBillOfLadingVersion { get; set; }
		[MaxLength(50)]
		public ZString? CarrierContractNumber { get; set; }
		public Branch Branch { get; set; }
		public Staff CustomsBroker { get; set; }
		[MaxLength(20)]
		public ZString? CartageWaybillNumber { get; set; }
		[MaxLength(35)]
		public ZString? CFSReference { get; set; }
		public CodeDescriptionPair ConsolidatedCargoStatus { get; set; }
		public ZInt? ContainerCount { get; set; }
		public ContainerMode ContainerMode { get; set; }
		public LoadMode LoadMode { get; set; }
		public Country CountryOfSupply { get; set; }
		public ContainerMode CustomsContainerMode { get; set; }
		public CodeDescriptionPair10Char CustomsOffice { get; set; }
		public CodeDescriptionPair DeliveryMode { get; set; }
		public ZDecimal? DestinationExchangeRate { get; set; }
		public ZDecimal? DestinationGoodsValue { get; set; }
		public Currency DestinationGoodsValueCurrency { get; set; }
		public ZDecimal? DocumentedChargeable { get; set; }
		public ZDecimal? DocumentedVolume { get; set; }
		public ZDecimal? DocumentedWeight { get; set; }
		public CodeDescriptionPair EFTMode { get; set; }
		[MaxLength(255)]
		public ZString? ElectronicBillOfLadingReference { get; set; }
		public EntryStatus EntryStatus { get; set; }
		public CodeDescriptionPair ExportGoodsType { get; set; }
		public UNLOCO EventBranchHomePort { get; set; }
		[MaxLength(35)]
		public ZString? FirstBuyerContact { get; set; }
		[MaxLength(20)]
		public ZString? Folio { get; set; }
		public ZDecimal? FreightRate { get; set; }
		public Currency FreightRateCurrency { get; set; }
		[MaxLength(2560), AllowLineControlWhiteSpace]
		public ZString? GoodsDescription { get; set; }
		[MaxLength(UniversalXmlInfo.MaxStringLength)]
		public ZString? MarksAndNumbers { get; set; }
		public CodeDescriptionPair GoodsOrigin { get; set; }
		public ZDecimal? GoodsValue { get; set; }
		public Currency GoodsValueCurrency { get; set; }
		public ZBool? IsLastMileDeliverySelfBooked { get; set; }
		public ZBool? HasProhibitedPackaging { get; set; }
		public CodeDescriptionPair HBLAWBChargesDisplay { get; set; }
		[MaxLength(9)]
		public ZString? HBLContainerPackModeOverride { get; set; }
		public CodeDescriptionPair HouseBillOfLadingType { get; set; }
		public CodeDescriptionPair AviationSecurityInspectionType { get; set; }
		public CodeDescriptionPair AviationSecurityAdditionalInspectionType { get; set; }
		public ZDecimal? InsuranceValue { get; set; }
		public ZDecimal? TransportValue { get; set; }
		public Currency InsuranceValueCurrency { get; set; }
		[MaxLength(35)]
		public ZString? InterimReceiptNumber { get; set; }
		public ZBool? IsBooking { get; set; }
		public ZBool? IsBuyersConsol { get; set; }
		public ZBool? IsCancelled { get; set; }
		public ZBool? IsCFSRegistered { get; set; }
		public ZBool? IsDirectBooking { get; set; }
		public ZBool? IsFinalManifest { get; set; }
		public ZBool? IsForwardRegistered { get; set; }
		public ZBool? IsHazardous { get; set; }
		public ZBool? IsHighRisk { get; set; }
		public ZBool? IsMasterHouse { get; set; }
		public IsNeutralMaster IsNeutralMaster { get; set; }
		public ZBool? IsOutOfGauge { get; set; }
		public ZBool? IsPersonalEffects { get; set; }
		public ZBool? IsShipping { get; set; }
		public ZBool? IsSplitShipment { get; set; }
		public ZBool? IsTracked { get; set; }
		public JobCosting JobCosting { get; set; }
		public ConsolCosts ConsolCosts { get; set; }
		public Commodity ConsolCommodity { get; set; }

		/// <summary>
		/// The `JS_RH_NKRateCommodity` of a shipment, or for quotes, from the
		/// TT_RH_NKCommodity. Where quotes is one of booking-with-quotes,
		/// one-off-quotes or quick-booking.
		/// </summary>
		public Commodity RateCommodity { get; set; }

		[MaxLength(4)]
		public ZString? FMCTariffID { get; set; }
		public CodeDescriptionPair LocalTransportEquipmentNeeded { get; set; }
		public CodeDescriptionPair4Char LocalTransportJobType { get; set; }
		[MaxLength(7)]
		public ZString? LloydsIMO { get; set; }
		public CodeDescriptionPair35Char LocationAtClearance { get; set; }
		public CodeDescriptionPair35Char SubLocationAtClearance { get; set; }
		public ZDecimal? ManifestedChargeable { get; set; }
		public ZDecimal? ManifestedVolume { get; set; }
		public ZDecimal? ManifestedWeight { get; set; }
		public CodeDescriptionPair MergeBy { get; set; }
		public CodeDescriptionPair MessageStatus { get; set; }
		public CodeDescriptionPair MessageSubType { get; set; }
		public CodeDescriptionPair MessageType { get; set; }
		public CodeDescriptionPair MessagingApplicationCode { get; set; }
		public ZByte? NoCopyBills { get; set; }
		public ZByte? NoOriginalBills { get; set; }
		public ZByte? CompanyTariffLevelOverride { get; set; }
		public CodeDescriptionPair OperationalStatus { get; set; }
		public ZInt? OuterPacks { get; set; }
		public PackageType OuterPacksPackageType { get; set; }
		[MaxLength(35)]
		public ZString? OwnerRef { get; set; }
		public ZInt? PackingOrder { get; set; }
		public CodeDescriptionPair PaymentMethod { get; set; }
		public CodeDescriptionPair PaidBy { get; set; }

		[MaxLength(35)]
		public ZString? DefermentAccountNumber { get; set; }
		public CodeDescriptionPair PickupMode { get; set; }
		[MaxLength(35)]
		public ZString? QuoteNumber { get; set; }
		public ZBool? IsDomesticFreight { get; set; }
		public ZBool? IsQuoteApprovedByManager { get; set; }
		public CodeDescriptionPair TransitTime { get; set; }
		public ZInt? Frequency { get; set; }
		public CodeDescriptionPair FrequencyUnit { get; set; }
		public ZShort? QuoteNumberOfEntries { get; set; }
		public ZShort? QuoteNumberOfEntryLines { get; set; }

		public CodeDescriptionPair QuoteKPI { get; set; }
		public CodeDescriptionPair QuoteSource { get; set; }
		public CodeDescriptionPair QuoteRevisionReason { get; set; }

		public CodeDescriptionPair RatingTransportMode { get; set; }
		public CodeDescriptionPair ReceivingForwarderHandlingType { get; set; }
		public CodeDescriptionPair ReleaseType { get; set; }
		public ZBool? RequiresRefrigeration { get; set; }
		public CodeDescriptionPair ScreeningStatus { get; set; }
		[MaxLength(35)]
		public ZString? SecondBuyerContact { get; set; }
		public ServiceLevel CarrierServiceLevel { get; set; }
		public CodeDescriptionPair SendingForwarderHandlingType { get; set; }
		public ServiceLevel ServiceLevel { get; set; }
		public ServiceLevel GatewayServiceLevel { get; set; }
		public IncoTerm ShipmentIncoTerm { get; set; }
		public CodeDescriptionPair ShipmentStatus { get; set; }
		public CodeDescriptionPair ShipmentType { get; set; }
		public CodeDescriptionPair ShipmentSubType { get; set; }
		public CodeDescriptionPair ShippedOnBoard { get; set; }
		public ZDecimal? ShipperCODAmount { get; set; }
		public CodeDescriptionPair ShipperCODPayMethod { get; set; }
		public ZInt? TotalNoOfPacks { get; set; }
		public ZDecimal? TotalNoOfPacksDecimal { get; set; }
		public PackageType TotalNoOfPacksPackageType { get; set; }
		public ZInt? TotalNoOfPieces { get; set; }
		public ZInt? TotalNoOfPiecesLanded { get; set; }
		public ZDecimal? TotalVolume { get; set; }
		public UnitOfVolume TotalVolumeUnit { get; set; }
		public ZDecimal? TotalWeight { get; set; }
		public UnitOfWeight TotalWeightUnit { get; set; }
		public ZBool? TranshipToOtherCFS { get; set; }
		public CodeDescriptionPair10Char CommunityTransitStatus { get; set; }
		public Country TransportNationality { get; set; }
		public CodeDescriptionPair TransportMode { get; set; }
		public CodeDescriptionPair BookingTransportMode { get; set; }
		public UNLOCO PortFirstForeign { get; set; }
		public UNLOCO PortLastForeign { get; set; }
		public UNLOCO PortOfOrigin { get; set; }
		public UNLOCO PortOfLoading { get; set; }
		public UNLOCO PortOfFirstArrival { get; set; }
		public UNLOCO PortOfDischarge { get; set; }
		public UNLOCO PortOfDestination { get; set; }
		[MaxLength(2)]
		public ZString? GoodsDestination { get; set; }
		public UNLOCO PlaceOfReceipt { get; set; }
		public UNLOCO PlaceOfDelivery { get; set; }
		public UNLOCO PlaceOfIssue { get; set; }
		public UNLOCO CarrierBookingOffice { get; set; }
		public CodeDescriptionPair CarrierBookingLatestStatus { get; set; }
		public ZDateTime? CarrierBookingLatestDate { get; set; }
		public CodeDescriptionPair10Char CustomsLoadPort { get; set; }
		public CodeDescriptionPair10Char CustomsDischargePort { get; set; }
		[MaxLength(35)]
		public ZString? VesselName { get; set; }
		public Country VesselCountryOfRegistration { get; set; }
		[MaxLength(10)]
		public ZString? VoyageFlightNo { get; set; }
		[MaxLength(43)]
		public ZString? WarehouseLocation { get; set; }
		public CodeDescriptionPair WarehouseReleaseStatus { get; set; }
		[MaxLength(35)]
		public ZString? WayBillNumber { get; set; }
		public WayBillType WayBillType { get; set; }
		public TransportBookingDirection TransportBookingDirection { get; set; }
		public PortMessaging PortMessaging { get; set; }
		public ZDecimal? TotalLoadingMeters { get; set; }
		public ZDecimal? TotalPreallocatedWeight { get; set; }
		public UnitOfWeight TotalPreallocatedWeightUnit { get; set; }
		public ZDecimal? TotalPreallocatedVolume { get; set; }
		public UnitOfVolume TotalPreallocatedVolumeUnit { get; set; }
		public ZDecimal? TotalPreallocatedChargeable { get; set; }
		public ZDecimal? CarrierCorrectedWeight { get; set; }
		public UnitOfWeight CarrierCorrectedWeightUnit { get; set; }
		public ZDecimal? CarrierCorrectedVolume { get; set; }
		public UnitOfVolume CarrierCorrectedVolumeUnit { get; set; }
		public ZDecimal? CarrierCorrectedChargeable { get; set; }
		public ZDecimal? ChargeableRate { get; set; }
		public ZBool? IsSignatureRequired { get; set; }
		public ZBool? IsAuthorizedToLeave { get; set; }
		[MaxLength(35)]
		public ZString? CoLoadMasterBillNumber { get; set; }
		[MaxLength(35)]
		public ZString? CoLoadBookingConfirmationReference { get; set; }
		[MaxLength(15)]
		public ZString? ConsigneeBussinessNumber { get; set; }
		[MaxLength(15)]
		public ZString? ConsigneeIdentifier { get; set; }
		[MaxLength(15)]
		public ZString? ConsignorIdentifier { get; set; }
		[MaxLength(15)]
		public ZString? VendorIdentifier { get; set; }
		public CodeDescriptionPair DeclarantType { get; set; }
		public CodeDescriptionPair CustomsValuationPort { get; set; }
		public ValueTypePair CustomsProfileIdentifier { get; set; }
		public ZBool? RequiresTemperatureControl { get; set; }
		public ZDecimal? RequiredTemperatureMinimum { get; set; }
		public ZDecimal? RequiredTemperatureMaximum { get; set; }
		public CodeDescriptionPair1Char RequiredTemperatureUnit { get; set; }

		public ZDecimal? MaximumAllowablePackageLength { get; set; }
		public ZDecimal? MaximumAllowablePackageWidth { get; set; }
		public ZDecimal? MaximumAllowablePackageHeight { get; set; }
		public UnitOfLength MaximumAllowablePackageLengthUnit { get; set; }
		public ZDateTime? SlotDateTime { get; set; }
		[MaxLength(35)]
		public ZString? SlotReference { get; set; }
		public CodeDescriptionPair FacilityJobType { get; set; }
		public VehicleRun VehicleRun { get; set; }

		[MaxLength(35)]
		public ZString? ManifestNumber { get; set; }

		public SealInfo SealInfo { get; set; }
		[MaxLength(35)]
		public ZString? UniqueConsignmentReference { get; set; }
		public GreenhouseGasEmission GreenhouseGasEmission { get; set; }
		public TEU TEU { get; set; }
		[MaxLength(3)]
		public ZString? Direction { get; set; }
		[MaxLength(35)]
		public ZString? ConsignmentNote { get; set; }
		public CodeDescriptionPair MNRStartEquipmentGrade { get; set; }
		public CodeDescriptionPair MNREndEquipmentGrade { get; set; }
		public ZDateTimeOffset? MNRWorkOrderApprovedTime { get; set; }
		public CodeDescriptionPair MNRType { get; set; }
		public ZShort? MNRRevision { get; set; }

		#endregion

		[VerticalPartition]
		public LocalProcessing LocalProcessing { get; set; }
		[VerticalPartition]
		public Order Order { get; set; }
		[VerticalPartition]
		public CarrierDocumentsOverride CarrierDocumentsOverride { get; set; }
		[VerticalPartition]
		public CarrierAccount CarrierAccount { get; set; }
		[MaxLength(3)]
		public ZString? CarrierAccountBillingType { get; set; }

		#region Collections

		public List<Shipment> RelatedShipmentCollection { get; private set; }
		public DataObjectList<Shipment> SubShipmentCollection { get; private set; }
		public List<Shipment> ParentShipmentCollection { get; private set; }
		public List<Shipment> PreCarriageShipmentCollection { get; private set; }
		public List<Shipment> PostCarriageShipmentCollection { get; private set; }
		public List<AdditionalBill> AdditionalBillCollection { get; private set; }
		public List<AdditionalAddressInfo> AdditionalAddressInfoCollection { get; private set; }
		public DataObjectList<AdditionalReference> AdditionalReferenceCollection { get; private set; }
		public CommercialInfo CommercialInfo { get; set; }
		public DataObjectList<TransportLeg> TransportLegCollection { get; private set; }
		public DataObjectList<Container> ContainerCollection { get; private set; }
		public DataObjectList<WeightData> EmptyContainerCollection { get; private set; }
		public DataObjectList<PackingLine> PackingLineCollection { get; private set; }
		public DataObjectList<PackingLine> ParentPackingLineCollection { get; private set; }
		public List<EntryNumber> EntryNumberCollection { get; private set; }
		public List<CodeDescriptionPair> SpecialHandlingCollection { get; private set; }
		public List<EntryHeader> EntryHeaderCollection { get; private set; }
		public List<EntryInstruction> EntryInstructionCollection { get; private set; }
		public List<OrganizationAddress> OrganizationAddressCollection { get; private set; }
		public DataObjectList<Note> NoteCollection { get; private set; }
		public List<Date> DateCollection { get; private set; }
		public List<AddInfo> AddInfoCollection { get; private set; }
		public DataObjectList<Instruction> InstructionCollection { get; private set; }
		public List<CustomizedField> CustomizedFieldCollection { get; private set; }
		public List<AddInfoGroup> AddInfoGroupCollection { get; private set; }
		public List<CustomsReference> CustomsReferenceCollection { get; private set; }
		public List<CustomsSupportingInformation> CustomsSupportingInformationCollection { get; private set; }
		public List<InBondMoveHeader> InBondMoveHeaderCollection { get; private set; }
		public List<Milestone> MilestoneCollection { get; private set; }
		public List<WorkflowException> ExceptionCollection { get; private set; }
		public List<PaymentHandlingInstruction> PaymentHandlingInstructionCollection { get; private set; }
		public List<BillOfLadingClause> BillOfLadingClauseCollection { get; private set; }
		public List<Guarantee> GuaranteeCollection { get; private set; }
		public List<AttachedDocument> AttachedDocumentCollection { get; private set; }
		public List<GatewayInfo> GatewayInfoCollection { get; private set; }
		public DataObjectList<UNDG> PreallocatedUNDGCollection { get; private set; }
		public List<PortReference> PortReferenceCollection { get; private set; }
		public List<CarrierAccount> AdditionalCarrierAccountCollection { get; private set; }
		public List<PotentialCarrier> PotentialCarrierCollection { get; private set; }
		public List<TransportMeans> TransportMeansCollection { get; private set; }
		public List<LocationOfGoods> LocationOfGoodsCollection { get; private set; }
		public List<Equipment> TransportEquipmentCollection { get; private set; }
		public List<BillScreening> BillScreeningCollection { get; private set; }
		public List<CustomsValueInformation> CustomsValueInformationCollection { get; private set; }
		public DataObjectList<MNRWorkOrderLine> MNRWorkOrderLineCollection { get; private set; }

		#endregion

		#region DocData

		[RestrictedProperty(nameof(IsDocDataRestricted))]
		public DocumentData DocData { get; set; }

		public static bool IsDocDataRestricted => !eAdaptorRegistry.Instance.SupportDocDataInUniversalShipmentXML.Value;

		#endregion

		#region GetSourceDataObject

		public static Shipment GetSourceDataObject(Shipment topLevelDataObject)
		{
			Shipment result = null;
			var dataContext = topLevelDataObject.DataContext;
			if (dataContext != null)
			{
				var dataSources = dataContext.DataSourceCollection;
				if (dataSources != null && dataSources.Count() > 1)
				{
					result = FindRelatedShipment(topLevelDataObject.SubShipmentCollection, dataSources.Last());
				}
			}

			return result ?? topLevelDataObject;
		}

		#endregion

		#region FindRelatedShipment

		static Shipment FindRelatedShipment(IEnumerable<Shipment> subShipments, IDataSourceDataObject sourceDataSource)
		{
			Shipment result = null;

			if (subShipments != null)
			{
				foreach (var subShipment in subShipments)
				{
					if (subShipment.DataContext != null)
					{
						if (subShipment.DataContext.DataSourceCollection.Any(o => o.Type == sourceDataSource.Type && o.Key == sourceDataSource.Key))
						{
							result = subShipment;
						}
						else
						{
							result = FindRelatedShipment(subShipment.SubShipmentCollection, sourceDataSource);
						}

						if (result != null)
						{
							break;
						}
					}
				}
			}

			return result;
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (AttachedDocumentCollection != null)
				{
					foreach (var attachedDocument in AttachedDocumentCollection)
					{
						attachedDocument?.Dispose();
					}
				}
			}

			base.Dispose(disposing);
		}

		#region IAttachedDocumentContainer

		bool IAttachedDocumentContainer.SetAttachedDocumentCollection(Func<IEnumerable<IAttachedDocument>> value)
		{
			return SetAttachedDocumentCollection(() =>
			{
				var result = value.Invoke();
				return result.Cast<AttachedDocument>().ToList();
			});
		}

		IEnumerable<IAttachedDocument> IAttachedDocumentContainer.AttachedDocumentCollection => AttachedDocumentCollection;

		#endregion

		#region IDataObjectParseSupporter Members

		ZString[] SupportedElementsAfterCustomsDeclarationCommenced => new ZString[] { nameof(DataContext), nameof(AdditionalReferenceCollection), nameof(NoteCollection) };

		bool IsTargetsContainsCustomsDeclaration => (DataContext?.DataTargetCollection?.Any(target => target.Type?.Equals(nameof(DataContextType.CustomsDeclaration)) ?? false)) ?? false;

		ZBool IDataObjectParseSupporter.IsElementSupported(string elementName)
		{
			return !AllowUpdateOfCustomsDeclarationAfterCommencement.GetValueOrDefault()
				|| !IsTargetsContainsCustomsDeclaration
				|| SupportedElementsAfterCustomsDeclarationCommenced.Contains(elementName);
		}

		ZString IDataObjectParseSupporter.GetErrorTextWhenNonSupportedElementsFound()
		{
			var supportedElementsBuilder = new ZStringBuilder();
			supportedElementsBuilder.AppendLine();
			SupportedElementsAfterCustomsDeclarationCommenced.ForEach(e => supportedElementsBuilder.AppendLine("· <" + e + ">"));

			return $@"Only the following elements are supported when 'AllowUpdateOfCustomsDeclarationAfterCommencement' is flagged as true.{supportedElementsBuilder}";
		}

		#endregion

		#region IValidationRuleCollectionParentCollection

		public List<ValidationRule> ValidationRuleCollection { get; private set; }

		#endregion
	}
}

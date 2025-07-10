using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public partial class PackingLine : IDataObject, ICustomizedFieldContainer, IContainerLinkParent, IOrganizationAddressCollectionParent
	{
		public PackingLine()
		{
		}

		public PackingLine(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		[MaxLength(50)]
		public ZString? Barcode { get; set; }
		[MaxLength(20)]
		public ZString? ContainerNumber { get; set; }
		public Commodity Commodity { get; set; }
		public ZInt? ContainerLink { get; set; }
		public ZInt? ContainerPackingOrder { get; set; }
		[MaxLength(260), AllowLineControlWhiteSpace]
		public ZString? GoodsDescription { get; set; }
		[MaxLength(15)]
		public ZString? HarmonisedCode { get; set; }
		public ZDecimal? Height { get; set; }
		public ZShort? ItemNo { get; set; }
		public ZDecimal? Length { get; set; }
		public ZInt? Link { get; set; }
		public ZInt? ParentPackingLineLink { get; set; }
		public ZDecimal? ManifestedVolume { get; set; }
		public ZDecimal? ManifestedWeight { get; set; }
		[MaxLength(UniversalXmlInfo.MaxStringLength), AllowLineControlWhiteSpace]
		public ZString? MarksAndNos { get; set; }
		public Country CountryOfOrigin { get; set; }
		public ZInt? OutturnQty { get; set; }
		public ZInt? OutturnDamagedQty { get; set; }
		public CodeDescriptionPair OutturnDamagedReason { get; set; }
		public ZInt? OutturnPillagedQty { get; set; }
		[MaxLength(250), AllowLineControlWhiteSpace]
		public ZString? OutturnComment { get; set; }
		public ZDecimal? OutturnedHeight { get; set; }
		public ZDecimal? OutturnedLength { get; set; }
		public ZDecimal? OutturnedVolume { get; set; }
		public ZDecimal? OutturnedWeight { get; set; }
		public ZDecimal? OutturnedWidth { get; set; }
		public ZLong? PackQty { get; set; }
		public PackageType PackType { get; set; }
		[MaxLength(20)]
		public ZString? PackingLineID { get; set; }
		[MaxLength(20)]
		public ZString? PreviousPackingLineID { get; set; }
		public ZBool? IsUnknownQty { get; set; }
		public ZBool? Fumigated { get; set; }
		public ZBool? NonStackable { get; set; }
		public ZBool? TopLoadOnly { get; set; }
		public ZBool? HeatTreated { get; set; }
		public ZBool? ISPMPallet { get; set; }
		public ZBool? Pillaged { get; set; }
		public PackageType CustomsPackType { get; set; }
		[MaxLength(46)]
		public ZString? ReferenceNumber { get; set; }
		[MaxLength(35)]
		public ZString? ExportReferenceNumber { get; set; }
		[MaxLength(35)]
		public ZString? ImportReferenceNumber { get; set; }
		public UnitOfLength LengthUnit { get; set; }
		public ZDecimal? Volume { get; set; }
		public UnitOfVolume VolumeUnit { get; set; }
		public ZDecimal? Weight { get; set; }
		public UnitOfWeight WeightUnit { get; set; }
		public ZDecimal? TareWeight { get; set; }
		public ZDecimal? DunnageWeight { get; set; }
		public ZDecimal? Width { get; set; }
		[MaxLength(35)]
		public ZString? TransportReference { get; set; }
		public ZDecimal? LoadingMeters { get; set; }
		public ZShort? EndItemNo { get; set; }
		public ZDecimal? LinePrice { get; set; }
		public Currency LinePriceCurrency { get; set; }
		[MaxLength(UniversalXmlInfo.MaxStringLength), AllowLineControlWhiteSpace]
		public ZString? DetailedDescription { get; set; }
		[MaxLength(35)]
		public ZString? BillNumber { get; set; }
		public WayBillType BillType { get; set; }
		public ZInt? InBondPackQty { get; set; }
		public ZInt? CustomsOuterPacks { get; set; }
		[MaxLength(UniversalXmlInfo.MaxStringLength), AllowLineControlWhiteSpace]
		public ZString? ShippingSymbol { get; set; }
		public Vehicle Vehicle { get; set; }
		public PortMessaging PortMessaging { get; set; }
		[MaxLength(35)]
		public ZString? OrderReference { get; set; }
		public NMFC NMFC { get; set; }
		[MaxLength(3)]
		public ZString? Status { get; set; }
		public ZDecimal? ReceivedQuantity { get; set; }
		public PackageType ReceivedQuantityType { get; set; }
		public ZInt? ReceivedPacks { get; set; }
		public PackageType ReceivedPacksType { get; set; }
		public ZDecimal? ReceivedWeight { get; set; }
		public UnitOfWeight ReceivedWeightUnit { get; set; }
		public ZDecimal? ReceivedVolume { get; set; }
		public UnitOfVolume ReceivedVolumeUnit { get; set; }
		[MaxLength(3)]
		public ZString? ScreeningMethod { get; set; }
		public ZDecimal? InnerQty { get; set; }
		public PackageType InnerPackType { get; set; }
		public ZDateTime? FirstCFSReceiptDate { get; set; }
		public ZDateTime? LastCFSReceiptDate { get; set; }
		public ZDateTime? UnloadDate { get; set; }
		public ZDateTime? LoadDate { get; set; }

		// temperatures
		public ZBool? RequiresTemperatureControl { get; set; }
		public ZDecimal? RequiredTemperatureMinimum { get; set; }
		public ZDecimal? RequiredTemperatureMaximum { get; set; }
		public CodeDescriptionPair1Char RequiredTemperatureUnit { get; set; }

		// Legacy DAKOSY fields, left for backward compatibility, should be deleted A.S.A.P.
		[MaxLength(35)]
		public ZString? MRN { get; set; }
		[MaxLength(1)]
		public ZString? MRNComplete { get; set; }
		[MaxLength(3)]
		public ZString? EntryType { get; set; }
		[MaxLength(1)]
		public ZString? ExemptionReason { get; set; }
		[MaxLength(35)]
		public ZString? LRN { get; set; }
		[MaxLength(1)]
		public ZString? LRNComplete { get; set; }
		[MaxLength(3)]
		// Legacy DAKOSY end

		public CodeDescriptionPair AviationSecurityInspectionType { get; set; }
		public CodeDescriptionPair AviationSecurityAdditionalInspectionType { get; set; }
		public ZBool? IsHighRisk { get; set; }
		public ZBool? RequiresFumigationCertificate { get; set; }
		public ZBool? IsPersonalEffects { get; set; }
		public ZBool? IsTimber { get; set; }
		public ZBool? IsPerishable { get; set; }
		public ZBool? IsHVLVClearance { get; set; }
		public ZBool? IsFlammable { get; set; }
		public ZBool? IsSignatureRequired { get; set; }
		public ZDateTime? LastKnownCFSStatusDate { get; set; }
		public CodeDescriptionPair LastKnownCFSStatus { get; set; }
		public ZBool? IsDamaged { get; set; }
		public ZBool? IsCheckedWeighedCubed { get; set; }
		public List<OrganizationAddress> OrganizationAddressCollection { get; private set; }

		public DataObjectList<Classification> ClassificationCollection { get; private set; }
		public List<CustomizedField> CustomizedFieldCollection { get; private set; }
		public List<AddInfoGroup> AddInfoGroupCollection { get; private set; }
		public List<PackedItem> PackedItemCollection { get; private set; }
		public List<PackingLine> PackingLineCollection { get; private set; }
		public List<UNDG> UNDGCollection { get; private set; }
		public List<AdditionalService> AdditionalServiceCollection { get; private set; }
		public List<Reference> ReferenceNumberCollection { get; private set; }
		public List<AddInfo> AddInfoCollection { get; private set; }
		public List<PortReference> PortReferenceCollection { get; private set; }
		public List<AdditionalReference> AdditionalReferenceCollection { get; private set; }
		public List<Entity> RelatedEntityCollection { get; private set; }

		public ZString? GetCleanSingleLineGoodsDescription()
		{
			if (!GoodsDescription.HasValue)
			{
				return null;
			}

			if (!cleanSingleLineGoodsDescription.HasValue)
			{
				cleanSingleLineGoodsDescription = Regex.Replace(GoodsDescription, " ?[\\t\\r\\n]+ ?", " ").Trim();
			}

			return cleanSingleLineGoodsDescription.Value;
		}
		ZString? cleanSingleLineGoodsDescription;
	}
}

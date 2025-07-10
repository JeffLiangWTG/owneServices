using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public partial class OrderLine : IDataObject, ICustomizedFieldContainer, IOrganizationAddressCollectionParent
	{
		public OrderLine()
		{
		}

		public OrderLine(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public Product Product { get; set; }
		public CodeDescriptionPair AdjustmentReason { get; set; }
		public CustomsEntryInfo CustomsData { get; set; }
		public ZInt? Link { get; set; }
		public Commodity Commodity { get; set; }
		public ZDateTimeOffset? ArrivalDate { get; set; }
		public OrganizationAddress Consignee { get; set; }
		public ZDecimal? InnerPacksQty { get; set; }
		public PackageType InnerPacksQtyUnit { get; set; }
		public CodeDescriptionPair InventoryStatus { get; set; }
		public ZDecimal? PackageQty { get; set; }
		public PackageType PackageQtyUnit { get; set; }
		public ZDecimal? PackageLength { get; set; }
		public ZDecimal? PackageWidth { get; set; }
		public ZDecimal? PackageHeight { get; set; }
		public UnitOfLength PackageLengthUnit { get; set; }
		[MaxLength(25)]
		public ZString? PutAwayArea { get; set; }
		public ZDecimal? OrderedQty { get; set; }
		public CodeDescriptionPair OrderedQtyUnit { get; set; }
		public Location Location { get; set; }
		[MaxLength(35)]
		public ZString? PackageGroupId { get; set; }
		[MaxLength(30)]
		public ZString? PalletID { get; set; }
		public ZDecimal? PerPackageQty { get; set; }
		public ZDecimal? ExpectedQuantity { get; set; }
		public ZDecimal? SplitQuantity { get; set; }
		public ZDecimal? QuantityMet { get; set; }
		public ZDecimal? ReservedQuantity { get; set; }
		public ZDecimal? ShortfallQuantity { get; set; }
		public ZDecimal? QtyBooked { get; set; }
		public ZDecimal? QtyPacked { get; set; }
		public ZDateTime? ExpiryDate { get; set; }
		[MaxLength(30)]
		public ZString? PartAttribute1 { get; set; }
		[MaxLength(30)]
		public ZString? PartAttribute2 { get; set; }
		[MaxLength(30)]
		public ZString? PartAttribute3 { get; set; }
		[MaxLength(50)]
		public ZString? SerialNumber { get; set; }
		public ZDateTime? PackingDate { get; set; }
		public ZDateTimeOffset? RequiredBy { get; set; }
		public ZDateTime? SupplierConfirmedAcceptance { get; set; }
		public ZDateTime? RequiredExWorks { get; set; }
		public ZDateTime? RequiredInStore { get; set; }
		[MaxLength(35)]
		public ZString? ConfirmationNumber { get; set; }
		public IncoTerm IncoTerm { get; set; }
		[MaxLength(50)]
		public ZString? AdditionalTerms { get; set; }
		[MaxLength(250), AllowLineControlWhiteSpace]
		public ZString? LineComment { get; set; }
		[MaxLength(UniversalXmlInfo.MaxStringLength), AllowLineControlWhiteSpace]
		public ZString? AdditionalInformation { get; set; }
		[MaxLength(UniversalXmlInfo.MaxStringLength), AllowLineControlWhiteSpace]
		public ZString? SpecialInstructions { get; set; }
		[MaxLength(35)]
		public ZString? CrossDockOrderNumber { get; set; }
		[MaxLength(20)]
		public ZString? CommercialInvoiceNumber { get; set; }
		[MaxLength(20)]
		public ZString? ContainerNumber { get; set; }
		public ZInt? ContainerPackingOrder { get; set; }
		public ZInt? LineNumber { get; set; }
		public ZInt? SubLineNumber { get; set; }
		public ZShort? LineSplitNumber { get; set; }
		[MaxLength(35)]
		public ZString? LineReference { get; set; }
		public CodeDescriptionPair Status { get; set; }
		public CodeDescriptionPair9Char OriginalHoldCode { get; set; }
		public CodeDescriptionPair9Char CurrentHoldCode { get; set; }
		[MaxLength(50)]
		public ZString? CurrentHoldReason { get; set; }
		public ZDecimal? UnitPriceRecommended { get; set; }
		public Currency UnitPriceCurrency { get; set; }
		public ZDecimal? UnitPriceDiscountPercent { get; set; }
		public ZDecimal? UnitPriceDiscountAmount { get; set; }
		public ZDecimal? UnitPriceAfterDiscount { get; set; }
		public ZDecimal? ExtendedLinePrice { get; set; }
		public ZDecimal? Weight { get; set; }
		[MaxLength(15)]
		public ZString? HarmonisedCode { get; set; }
		public UnitOfWeight WeightUnit { get; set; }
		public ZDecimal? Volume { get; set; }
		public UnitOfVolume VolumeUnit { get; set; }
		public ZDecimal? OverQuantityPercentageLimit { get; set; }
		public ZDecimal? UnderQuantityPercentageLimit { get; set; }
		public ZByte? LateShipmentLimitDays { get; set; }
		public ZByte? EarlyShipmentLimitDays { get; set; }
		public List<UNDG> UNDGCollection { get; private set; }
		public List<CustomizedField> CustomizedFieldCollection { get; private set; }
		public List<OrganizationAddress> OrganizationAddressCollection { get; private set; }
		public ZDate? ShipmentWindowStart { get; set; }
		public ZDate? ShipmentWindowEnd { get; set; }
		public List<OrderLine> OrderLineCollection { get; private set; }
		public List<Entity> RelatedEntityCollection { get; private set; }
		[MaxLength(50)]
		public ZString? BatchNumber { get; set; }
	}
}

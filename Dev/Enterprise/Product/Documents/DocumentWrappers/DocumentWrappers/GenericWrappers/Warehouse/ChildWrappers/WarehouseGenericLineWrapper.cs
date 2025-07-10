using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[WrapperTypeName("WarehouseJobLine")]
	public abstract class WarehouseGenericLineWrapper : GenericWrapper
	{
		public WarehouseGenericLineWrapper(BusinessObject whsLineBO, BusinessObjectFactory factory)
			: base(whsLineBO, factory)
		{
		}

		#region Product

		public ProductWrapper Product
		{
			get { return product ?? (product = ProductCore); }
		}
		ProductWrapper product;

		protected virtual ProductWrapper ProductCore
		{
			get { return new ProductWrapper(null, Factory); }
		}

		#endregion

		#region CrossDockConsigneeAddress

		public AddressWrapper CrossDockConsigneeAddress
		{
			get { return crossDockConsigneeAddress ?? (crossDockConsigneeAddress = CrossDockConsigneeAddressCore); }
		}

		AddressWrapper crossDockConsigneeAddress;

		protected virtual AddressWrapper CrossDockConsigneeAddressCore
		{
			get { return AddressWrapper.Empty(Factory); }
		}

		#endregion

		#region CrossDockOrderNumber 

		public ZString CrossDockOrderNumber
		{
			get { return CrossDockOrderNumberCore; }
		}

		protected virtual ZString CrossDockOrderNumberCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region OrgPartRelation

		public OrgPartRelation OrgPartRelation
		{
			get { return orgPartRelation ?? (orgPartRelation = OrgPartRelationCore); }
		}
		OrgPartRelation orgPartRelation;

		protected virtual OrgPartRelation OrgPartRelationCore
		{
			get { return null; }
		}

		#endregion

		#region SupplierProductDesc

		public ZString SupplierProductDesc
		{
			get { return SupplierProductDescCore; }
		}

		protected virtual ZString SupplierProductDescCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ProductBrandName

		public ZString ProductBrandName
		{
			get { return ProductBrandNameCore; }
		}

		protected virtual ZString ProductBrandNameCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ProductModel

		public ZString ProductModel
		{
			get { return ProductModelCore; }
		}

		protected virtual ZString ProductModelCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ProductCode

		public ZString ProductCode
		{
			get { return ProductCodeCore; }
		}

		protected virtual ZString ProductCodeCore
		{
			get { return ""; }
		}

		#endregion

		#region ProductCodeBarcode

		public ZString ProductCodeBarcode
		{
			get { return ProductCodeBarcodeCore; }
		}

		protected virtual ZString ProductCodeBarcodeCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ProductCodeBarcodeNumber

		public ZString ProductCodeBarcodeNumber
		{
			get { return ProductCodeBarcodeNumberCore; }
		}

		protected virtual ZString ProductCodeBarcodeNumberCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ProductDescription

		public ZString ProductDescription
		{
			get { return ProductDescriptionCore; }
		}

		protected virtual ZString ProductDescriptionCore
		{
			get { return ""; }
		}

		#endregion

		#region NMFC

		public virtual DocRefNMFC NMFC
		{
			get { return null; }
		}

		#endregion

		#region Custom Attributes

		public ZString CustomAttrib1
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomAttrib1Core; }
		}

		protected virtual ZString CustomAttrib1Core
		{
			get { return ""; }
		}

		public ZString CustomAttrib2
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomAttrib2Core; }
		}

		protected virtual ZString CustomAttrib2Core
		{
			get { return ""; }
		}

		public ZString CustomAttrib3
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomAttrib3Core; }
		}

		protected virtual ZString CustomAttrib3Core
		{
			get { return ""; }
		}

		public ZString CustomAttrib4
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomAttrib4Core; }
		}

		protected virtual ZString CustomAttrib4Core
		{
			get { return ""; }
		}

		public ZString CustomAttrib5
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomAttrib5Core; }
		}

		protected virtual ZString CustomAttrib5Core
		{
			get { return ""; }
		}

		public ZString CustomAttrib6
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomAttrib6Core; }
		}

		protected virtual ZString CustomAttrib6Core
		{
			get { return ""; }
		}

		public ZDateTime CustomDate1
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomDate1Core; }
		}

		protected virtual ZDateTime CustomDate1Core
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime CustomDate2
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomDate2Core; }
		}

		protected virtual ZDateTime CustomDate2Core
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime CustomDate3
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomDate3Core; }
		}

		protected virtual ZDateTime CustomDate3Core
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime CustomDate4
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomDate4Core; }
		}

		protected virtual ZDateTime CustomDate4Core
		{
			get { return ZDateTime.Empty; }
		}

		public ZDateTime CustomDate5
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomDate5Core; }
		}

		protected virtual ZDateTime CustomDate5Core
		{
			get { return ZDateTime.Empty; }
		}

		public ZDecimal CustomDecimal1
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomDecimal1Core; }
		}

		protected virtual ZDecimal CustomDecimal1Core
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal CustomDecimal2
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomDecimal2Core; }
		}

		protected virtual ZDecimal CustomDecimal2Core
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal CustomDecimal3
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomDecimal3Core; }
		}

		protected virtual ZDecimal CustomDecimal3Core
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal CustomDecimal4
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomDecimal4Core; }
		}

		protected virtual ZDecimal CustomDecimal4Core
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal CustomDecimal5
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomDecimal5Core; }
		}

		protected virtual ZDecimal CustomDecimal5Core
		{
			get { return ZDecimal.Zero; }
		}

		public ZBool CustomFlag1
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomFlag1Core; }
		}

		protected virtual ZBool CustomFlag1Core
		{
			get { return ZBool.False; }
		}

		public ZBool CustomFlag2
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomFlag2Core; }
		}

		protected virtual ZBool CustomFlag2Core
		{
			get { return ZBool.False; }
		}

		public ZBool CustomFlag3
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomFlag3Core; }
		}

		protected virtual ZBool CustomFlag3Core
		{
			get { return ZBool.False; }
		}

		public ZBool CustomFlag4
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomFlag4Core; }
		}

		protected virtual ZBool CustomFlag4Core
		{
			get { return ZBool.False; }
		}

		public ZBool CustomFlag5
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomFlag5Core; }
		}

		protected virtual ZBool CustomFlag5Core
		{
			get { return ZBool.False; }
		}

		public ZString CustomTextBlob1
		{
			[DocumentEngineObsoleteField("Use CustomFields instead")]
			get { return CustomTextBlob1Core; }
		}

		protected virtual ZString CustomTextBlob1Core
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region AttributeUnits

		public ZString AttributeUnits
		{
			get { return AttributeUnitsCore; }
		}

		protected virtual ZString AttributeUnitsCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ExpiryDate

		public ZDateTime ExpiryDate
		{
			get { return ExpiryDateCore; }
		}

		protected virtual ZDateTime ExpiryDateCore
		{
			get { return ZDateTime.Empty; }
		}

		public ZString ExpiryDateFormatted
		{
			get { return ExpiryDateFormattedCore; }
		}

		protected virtual ZString ExpiryDateFormattedCore
		{
			get { return ExpiryDate.ToShortDateString(); }
		}

		#endregion

		#region PackingDate

		public ZDateTime PackingDate
		{
			get { return PackingDateCore; }
		}

		protected virtual ZDateTime PackingDateCore
		{
			get { return ZDateTime.Empty; }
		}

		public ZString PackingDateFormatted
		{
			get { return PackingDateFormattedCore; }
		}

		protected virtual ZString PackingDateFormattedCore
		{
			get { return PackingDate.ToShortDateString(); }
		}

		#endregion

		#region PartAttribute1

		public ZString PartAttribute1
		{
			get { return PartAttribute1Core; }
		}

		protected virtual ZString PartAttribute1Core
		{
			get { return ""; }
		}

		#endregion

		#region PartAttribute2

		public ZString PartAttribute2
		{
			get { return PartAttribute2Core; }
		}

		protected virtual ZString PartAttribute2Core
		{
			get { return ""; }
		}

		#endregion

		#region PartAttribute3

		public ZString PartAttribute3
		{
			get { return PartAttribute3Core; }
		}

		protected virtual ZString PartAttribute3Core
		{
			get { return ""; }
		}

		#endregion

		#region TrackedSerialNumber

		public ZString TrackedSerialNumber
		{
			get { return TrackedSerialNumberCore; }
		}

		protected virtual ZString TrackedSerialNumberCore
		{
			get { return ""; }
		}

		#endregion

		#region ExpiryDateBarcode

		public ZString ExpiryDateBarcode
		{
			get { return ExpiryDateBarcodeCore; }
		}

		protected virtual ZString ExpiryDateBarcodeCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PackingDateBarcode

		public ZString PackingDateBarcode
		{
			get { return PackingDateBarcodeCore; }
		}

		protected virtual ZString PackingDateBarcodeCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PartAttribute1Barcode

		public ZString PartAttribute1Barcode
		{
			get { return PartAttribute1BarcodeCore; }
		}

		protected virtual ZString PartAttribute1BarcodeCore
		{
			get { return ""; }
		}

		#endregion

		#region PartAttribute2Barcode

		public ZString PartAttribute2Barcode
		{
			get { return PartAttribute2BarcodeCore; }
		}

		protected virtual ZString PartAttribute2BarcodeCore
		{
			get { return ""; }
		}

		#endregion

		#region PartAttribute3Barcode

		public ZString PartAttribute3Barcode
		{
			get { return PartAttribute3BarcodeCore; }
		}

		protected virtual ZString PartAttribute3BarcodeCore
		{
			get { return ""; }
		}

		#endregion

		#region TrackedSerialBarcode

		public ZString TrackedSerialBarcode
		{
			get { return TrackedSerialBarcodeCore; }
		}

		protected virtual ZString TrackedSerialBarcodeCore
		{
			get { return ""; }
		}

		#endregion

		#region RFConfirmName

		public ZString RFConfirmName
		{
			get { return RFConfirmNameCore; }
		}

		protected virtual ZString RFConfirmNameCore
		{
			get { return ""; }
		}

		#endregion

		#region RFConfirm

		public ZString RFConfirm
		{
			get { return RFConfirmCore; }
		}

		protected virtual ZString RFConfirmCore
		{
			get { return ""; }
		}

		#endregion

		#region RFConfirmBarcode

		public ZString RFConfirmBarcode
		{
			get { return RFConfirmBarcodeCore; }
		}

		protected virtual ZString RFConfirmBarcodeCore
		{
			get { return ""; }
		}

		#endregion

		#region DangerousGoodsSubstance

		public virtual UNDGSubstanceWrapper DangerousGoodsSubstance
		{
			get { return DangerousGoodsSubstanceCore; }
		}

		protected virtual UNDGSubstanceWrapper DangerousGoodsSubstanceCore
		{
			get { return null; }
		}

		#endregion

		#region ExtendedLinePrice

		public MoneyWrapper ExtendedLinePrice
		{
			get { return ExtendedLinePriceCore; }
		}

		protected virtual MoneyWrapper ExtendedLinePriceCore
		{
			get { return null; }
		}

		#endregion

		#region ExtendedLinePriceForTotal

		internal ZDecimal ExtendedLinePriceForTotal
		{
			get { return ExtendedLinePriceForTotalCore; }
		}

		protected virtual ZDecimal ExtendedLinePriceForTotalCore
		{
			get { return ExtendedLinePrice?.Amount ?? ZDecimal.Zero; }
		}

		#endregion

		#region RecommendedUnitPrice

		public LabelValuePairWrapper RecommendedUnitPrice
		{
			get { return RecommendedUnitPriceCore; }
		}

		protected virtual LabelValuePairWrapper RecommendedUnitPriceCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region UnitDiscountAmount

		public LabelValuePairWrapper UnitDiscountAmount
		{
			get { return UnitDiscountAmountCore; }
		}

		protected virtual LabelValuePairWrapper UnitDiscountAmountCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region UnitDiscountPercent

		public LabelValuePairWrapper UnitDiscountPercent
		{
			get { return UnitDiscountPercentCore; }
		}

		protected virtual LabelValuePairWrapper UnitDiscountPercentCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region UnitPriceAfterDiscount

		public virtual LabelValuePairWrapper UnitPriceAfterDiscount
		{
			get { return UnitPriceAfterDiscountCore; }
		}

		protected virtual LabelValuePairWrapper UnitPriceAfterDiscountCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region AdditionalMoneys

		public ZString AdditionalMoneys
		{
			get { return AdditionalMoneysCore; }
		}

		protected virtual ZString AdditionalMoneysCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region UnitsMet

		public LabelValuePairWrapper UnitsMet
		{
			get { return UnitsMetCore; }
		}

		protected virtual LabelValuePairWrapper UnitsMetCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region UnitsOrdered

		public LabelValuePairWrapper UnitsOrdered
		{
			get { return UnitsOrderedCore; }
		}

		protected virtual LabelValuePairWrapper UnitsOrderedCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region UnitsPicked

		public LabelValuePairWrapper UnitsPicked
		{
			get { return UnitsPickedCore; }
		}

		protected virtual LabelValuePairWrapper UnitsPickedCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region UnitsShort

		public LabelValuePairWrapper UnitsShort
		{
			get { return UnitsShortCore; }
		}

		protected virtual LabelValuePairWrapper UnitsShortCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region ArrivalDate

		public ZDateTime ArrivalDate
		{
			get { return ArrivalDateCore; }
		}

		protected virtual ZDateTime ArrivalDateCore
		{
			get { return ZDateTime.Empty; }
		}

		#endregion

		#region UnitsUQ

		public ZString UnitsUQ
		{
			get { return UnitsUQCore; }
		}

		protected virtual ZString UnitsUQCore
		{
			get { return ""; }
		}

		#endregion

		#region PacksUQ

		public ZString PacksUQ
		{
			get { return PacksUQCore; }
		}

		protected virtual ZString PacksUQCore
		{
			get { return ""; }
		}

		#endregion

		#region LocationString

		public ZString LocationString
		{
			get { return LocationStringCore; }
		}

		protected virtual ZString LocationStringCore
		{
			get { return ""; }
		}

		#endregion

		#region PartAttribute1WithLabel

		public MultilingualString PartAttribute1WithLabel
		{
			get { return PartAttribute1WithLabelCore; }
		}

		protected virtual MultilingualString PartAttribute1WithLabelCore
		{
			get { return (NoResString)""; }
		}

		#endregion

		#region PartAttribute2WithLabel

		public MultilingualString PartAttribute2WithLabel
		{
			get { return PartAttribute2WithLabelCore; }
		}

		protected virtual MultilingualString PartAttribute2WithLabelCore
		{
			get { return (NoResString)""; }
		}

		#endregion

		#region PartAttribute3WithLabel

		public MultilingualString PartAttribute3WithLabel
		{
			get { return PartAttribute3WithLabelCore; }
		}

		protected virtual MultilingualString PartAttribute3WithLabelCore
		{
			get { return (NoResString)""; }
		}

		#endregion

		#region TrackedSerialWithLabel

		public ZString TrackedSerialWithLabel
		{
			get { return TrackedSerialWithLabelCore; }
		}

		protected virtual ZString TrackedSerialWithLabelCore
		{
			get { return string.Empty; }
		}

		#endregion

		#region PackingDateWithLabel

		public ZString PackingDateWithLabel
		{
			get { return PackingDateWithLabelCore; }
		}

		protected virtual ZString PackingDateWithLabelCore
		{
			get { return ""; }
		}

		#endregion

		#region ExpiryDateWithLabel

		public ZString ExpiryDateWithLabel
		{
			get { return ExpiryDateWithLabelCore; }
		}

		protected virtual ZString ExpiryDateWithLabelCore
		{
			get { return ""; }
		}

		#endregion

		#region ExtraDetails

		public ZString ExtraDetails
		{
			get { return ExtraDetailsCore; }
		}

		protected virtual ZString ExtraDetailsCore
		{
			get { return ""; }
		}

		#endregion

		#region LineComment

		public ZString LineComment
		{
			get { return LineCommentCore; }
		}

		protected virtual ZString LineCommentCore
		{
			get { return ""; }
		}

		#endregion

		#region ReasonCode

		public ZString ReasonCode
		{
			get { return ReasonCodeCore; }
		}

		protected virtual ZString ReasonCodeCore
		{
			get { return ""; }
		}

		#endregion

		#region ReasonDescription

		public ZString ReasonDescription
		{
			get { return ReasonDescriptionCore; }
		}

		protected virtual ZString ReasonDescriptionCore
		{
			get { return ""; }
		}

		#endregion

		#region Packs

		public ZDecimal Packs
		{
			get { return PacksCore; }
		}

		protected virtual ZDecimal PacksCore
		{
			get { return 0m; }
		}

		#endregion

		#region Units

		public ZDecimal Units
		{
			get { return UnitsCore; }
			set { UnitsCore = value; }
		}

		protected virtual ZDecimal UnitsCore { get; set; }

		#endregion

		#region Fields for WarehouseGroupedPickingSlipLineWrapper

		#region UnitsGroupedUOMType

		public ZString UnitsGroupedUOMType => UnitsGroupedUOMTypeCore;

		protected virtual ZString UnitsGroupedUOMTypeCore => string.Empty;

		#endregion

		#region UnitsGroupedPackQty

		public ZString UnitsGroupedPackQty
		{
			get { return UnitsGroupedPackQtyCore; }
		}

		protected virtual ZString UnitsGroupedPackQtyCore
		{
			get { return string.Empty; }
		}

		#endregion

		#region UnitsGroupedQty

		public ZString UnitsGroupedQty
		{
			get { return UnitsGroupedQtyCore; }
		}

		protected virtual ZString UnitsGroupedQtyCore
		{
			get { return string.Empty; }
		}

		#endregion

		#region UnitsGroupedPackType

		public ZString UnitsGroupedPackType
		{
			get { return UnitsGroupedPackTypeCore; }
		}

		protected virtual ZString UnitsGroupedPackTypeCore
		{
			get { return string.Empty; }
		}

		#endregion

		#region UnitsGroupedStockKeepingUnit

		public ZString UnitsGroupedStockKeepingUnit
		{
			get { return UnitsGroupedStockKeepingUnitCore; }
		}

		protected virtual ZString UnitsGroupedStockKeepingUnitCore
		{
			get { return string.Empty; }
		}

		#endregion

		#endregion

		#region ReleaseUnitsAndUQ

		public ZString ReleaseUnitsAndUQ
		{
			get { return ReleaseUnitsAndUQCore; }
		}

		protected virtual ZString ReleaseUnitsAndUQCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ClientName

		public ZString ClientName
		{
			get { return ClientNameCore; }
		}

		protected virtual ZString ClientNameCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ClientCode

		public ZString ClientCode
		{
			get { return ClientCodeCore; }
		}

		protected virtual ZString ClientCodeCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PickArea

		public ZString PickArea => PickAreaCore;

		protected virtual ZString PickAreaCore => ZString.Empty;

		#endregion

		#region PickMethod

		public ZString PickMethod
		{
			get { return PickMethodCore; }
		}

		protected virtual ZString PickMethodCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PickGroup

		public ZString PickGroup
		{
			get { return PickGroupCore; }
		}

		protected virtual ZString PickGroupCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PickGroupNumber

		public ZShort PickGroupNumber
		{
			get { return PickGroupNumberCore; }
		}

		protected virtual ZShort PickGroupNumberCore
		{
			get { return ZShort.Zero; }
		}

		#endregion

		#region RowPathSequence

		public ZShort RowPathSequence
		{
			get { return RowPathSequenceCore; }
		}

		protected virtual ZShort RowPathSequenceCore
		{
			get { return ZShort.Zero; }
		}

		#endregion

		#region PickPathSequence

		public ZInt PickPathSequence
		{
			get { return PickPathSequenceCore; }
		}

		protected virtual ZInt PickPathSequenceCore
		{
			get { return ZInt.Zero; }
		}

		#endregion

		#region WeightUQ

		public ZString WeightUQ
		{
			get { return WeightUQCore; }
		}

		protected virtual ZString WeightUQCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region VolumeUQ

		public ZString VolumeUQ
		{
			get { return VolumeUQCore; }
		}

		protected virtual ZString VolumeUQCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PalletID

		public ZString PalletID
		{
			get { return PalletIDCore; }
		}

		protected virtual ZString PalletIDCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PalletIDBarcode

		public ZString PalletIDBarcode
		{
			get { return PalletIDBarcodeCore; }
		}

		protected virtual ZString PalletIDBarcodeCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PartAttribute1Name

		public MultilingualString PartAttribute1Name
		{
			get { return PartAttribute1NameCore; }
		}

		protected virtual MultilingualString PartAttribute1NameCore
		{
			get { return (NoResString)ZString.Empty; }
		}

		#endregion

		#region PartAttribute2Name

		public MultilingualString PartAttribute2Name
		{
			get { return PartAttribute2NameCore; }
		}

		protected virtual MultilingualString PartAttribute2NameCore
		{
			get { return (NoResString)ZString.Empty; }
		}

		#endregion

		#region PartAttribute3Name

		public MultilingualString PartAttribute3Name
		{
			get { return PartAttribute3NameCore; }
		}

		protected virtual MultilingualString PartAttribute3NameCore
		{
			get { return (NoResString)ZString.Empty; }
		}

		#endregion

		#region LeftOverAttributes

		public MultilingualString LeftOverAttributes
		{
			get { return LeftOverAttributesCore; }
		}

		protected virtual MultilingualString LeftOverAttributesCore
		{
			get { return (NoResString)ZString.Empty; }
		}

		#endregion

		#region FirstDate

		public ZString FirstDate
		{
			get { return FirstDateCore; }
		}

		protected virtual ZString FirstDateCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SecondDate

		public ZString SecondDate
		{
			get { return SecondDateCore; }
		}

		protected virtual ZString SecondDateCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ReceievedQty

		public ZString ReceivedQty
		{
			get { return ReceivedQtyCore; }
		}

		protected virtual ZString ReceivedQtyCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region InventoryQty

		public ZString InventoryQty
		{
			get { return InventoryQtyCore; }
		}

		protected virtual ZString InventoryQtyCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region GroupedReceivedWeight

		public ZString GroupedReceivedWeight
		{
			get { return GroupedReceivedWeightCore; }
		}

		protected virtual ZString GroupedReceivedWeightCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Pallets

		public ZDecimal Pallets
		{
			get { return PalletsCore; }
		}

		protected virtual ZDecimal PalletsCore
		{
			get { return ZDecimal.Zero; }
		}

		#endregion

		#region Weight

		public ZDecimal Weight
		{
			get { return WeightCore; }
		}

		protected virtual ZDecimal WeightCore
		{
			get { return ZDecimal.Zero; }
		}

		#endregion

		#region Volume

		public ZDecimal Volume
		{
			get { return VolumeCore; }
		}

		protected virtual ZDecimal VolumeCore
		{
			get { return ZDecimal.Zero; }
		}

		#endregion

		#region PackQty

		public ZDecimal PackQty
		{
			get { return PackQtyCore; }
		}

		protected virtual ZDecimal PackQtyCore
		{
			get { return ZDecimal.Zero; }
		}

		#endregion

		#region ExpectedReceiptQuantity

		public ZDecimal ExpectedReceiptQuantity
		{
			get { return ExpectedReceiptQuantityCore; }
		}

		protected virtual ZDecimal ExpectedReceiptQuantityCore
		{
			get { return ZDecimal.Zero; }
		}

		#endregion

		#region Status

		public ZString Status
		{
			get { return StatusCore; }
		}

		protected virtual ZString StatusCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region LocationColumn

		public ZShort LocationColumn
		{
			get { return LocationColumnCore; }
		}

		protected virtual ZShort LocationColumnCore
		{
			get { return ZShort.Zero; }
		}

		#endregion

		#region LocationLevel

		public ZShort LocationLevel
		{
			get { return LocationLevelCore; }
		}

		protected virtual ZShort LocationLevelCore
		{
			get { return ZShort.Zero; }
		}

		#endregion

		#region IsTopLevelOrOddIndex

		public ZString IsTopLevelOrOddIndex
		{
			get { return IsTopLevelOrOddIndexCore; }
		}

		protected virtual ZString IsTopLevelOrOddIndexCore
		{
			get { return Index % 2 == 1 ? "Y" : "N"; }
		}

		#endregion

		#region IsTopLevelOrEvenIndex

		public ZString IsTopLevelOrEvenIndex
		{
			get { return IsTopLevelOrEvenIndexCore; }
		}

		protected virtual ZString IsTopLevelOrEvenIndexCore
		{
			get { return Index % 2 == 0 ? "Y" : "N"; }
		}
		#endregion

		#region Customs Fields

		#region CustomsEntryKey

		public ZString CustomsEntryKey
		{
			get { return CustomsEntryKeyCore; }
		}

		protected virtual ZString CustomsEntryKeyCore
		{
			get { return ""; }
		}

		#endregion

		#region CustomsEntryNo

		public ZString CustomsEntryNo
		{
			get { return CustomsEntryNoCore; }
		}

		protected virtual ZString CustomsEntryNoCore
		{
			get { return ""; }
		}

		#endregion

		#region CustomsEntryLineNo

		public ZShort CustomsEntryLineNo
		{
			get { return CustomsEntryLineNoCore; }
		}

		protected virtual ZShort CustomsEntryLineNoCore
		{
			get { return ZShort.Zero; }
		}

		#endregion

		#region CustomsTariffItem

		public ZString CustomsTariffItem
		{
			get { return CustomsTariffItemCore; }
		}

		protected virtual ZString CustomsTariffItemCore
		{
			get { return ""; }
		}

		#endregion

		#region CustomsEntryDate

		public ZDateTime CustomsEntryDate
		{
			get { return CustomsEntryDateCore; }
		}

		protected virtual ZDateTime CustomsEntryDateCore
		{
			get { return ZDateTime.Empty; }
		}

		#endregion

		#region CustomsQuantity

		public ZDecimal CustomsQuantity
		{
			get { return CustomsQuantityCore; }
		}

		protected virtual ZDecimal CustomsQuantityCore
		{
			get { return 0m; }
		}

		#endregion

		#region CustomsQuantityUQ

		public ZString CustomsQuantityUQ
		{
			get { return CustomsQuantityUQCore; }
		}

		protected virtual ZString CustomsQuantityUQCore
		{
			get { return ""; }
		}

		#endregion

		#region CustomsVFD

		public ZDecimal CustomsVFD
		{
			get { return CustomsVFDCore; }
		}

		protected virtual ZDecimal CustomsVFDCore
		{
			get { return 0m; }
		}

		#endregion

		#region CustomsTILV

		public ZDecimal CustomsTILV
		{
			get { return CustomsTILVCore; }
		}

		protected virtual ZDecimal CustomsTILVCore
		{
			get { return 0m; }
		}

		#endregion

		#region CustomsCtryOfOrigin

		public ZString CustomsCtryOfOrigin
		{
			get { return CustomsCtryOfOriginCore; }
		}

		protected virtual ZString CustomsCtryOfOriginCore
		{
			get { return ""; }
		}

		#endregion

		#region CustomsAddInfo

		public ZString CustomsAddInfo
		{
			get { return CustomsAddInfoCore; }
		}

		protected virtual ZString CustomsAddInfoCore
		{
			get { return ""; }
		}

		#endregion

		#region CustomsSecondQuantity

		public ZDecimal CustomsSecondQuantity
		{
			get { return CustomsSecondQuantityCore; }
		}

		protected virtual ZDecimal CustomsSecondQuantityCore
		{
			get { return 0; }
		}

		#endregion

		#region CustomsSecondUnitQty

		public ZString CustomsSecondUnitQty
		{
			get { return CustomsSecondUnitQtyCore; }
		}

		protected virtual ZString CustomsSecondUnitQtyCore
		{
			get { return ""; }
		}

		#endregion

		#region CustomsThirdQuantity

		public ZDecimal CustomsThirdQuantity
		{
			get { return CustomsThirdQuantityCore; }
		}

		protected virtual ZDecimal CustomsThirdQuantityCore
		{
			get { return 0; }
		}

		#endregion

		#region CustomsThirdUnitQty

		public ZString CustomsThirdUnitQty
		{
			get { return CustomsThirdUnitQtyCore; }
		}

		protected virtual ZString CustomsThirdUnitQtyCore
		{
			get { return ""; }
		}

		#endregion

		#region ManufacturerAddress

		public AddressWrapper ManufacturerAddress
		{
			get { return manufacturerAddress ?? (manufacturerAddress = ManufacturerAddressCore); }
		}

		AddressWrapper manufacturerAddress;

		protected virtual AddressWrapper ManufacturerAddressCore
		{
			get { return AddressWrapper.Empty(Factory); }
		}

		#endregion

		#region Tariff

		public ZString Tariff
		{
			get { return TariffCore; }
		}

		protected virtual ZString TariffCore
		{
			get { return ""; }
		}

		#endregion

		#region PrimaryPreference

		public ZString PrimaryPreference
		{
			get { return PrimaryPreferenceCore; }
		}

		protected virtual ZString PrimaryPreferenceCore
		{
			get { return ""; }
		}

		#endregion

		#endregion

		#region Bill Of Materials (BOM) and Work Order

		internal virtual ZBool IsWorkOrder { get { return ZBool.False; } }

		public virtual ZInt BOMLevel { get { return ZInt.Zero; } }

		public virtual MultilingualString Attributes
		{
			get { return (NoResString)ZString.Empty; }
		}

		public ZString BOMIndentation
		{
			get { return new ZString(' ', (int)(BOMLevel * indentation * (Convert.ToInt32(IsWorkOrder) * .5))); }  // This calc is temporary.  It should be halved for portrait styles.
		}
		internal const int indentation = 9;

		internal static ZString BuildAttribute(ZString attributeValue, ZString attributeName, ZString alternateAttributeName)
		{
			if (attributeName.IsEmpty)
			{
				attributeName = alternateAttributeName;
			}

			return attributeValue.IsEmpty ? ZString.Empty : new ZString(attributeName.ToString() + ": " + attributeValue.ToString());
		}

		internal static MultilingualString BuildAttribute(ZString attributeValue, MultilingualString attributeName, MultilingualString alternateAttributeName)
		{
			if (attributeName.IsEmpty)
			{
				attributeName = alternateAttributeName;
			}

			return attributeValue.IsEmpty ? (NoResString)ZString.Empty : MultilingualString.Join(": ", attributeName, (NoResString)attributeValue);
		}

		#endregion

		#region LineNo

		public ZString LineNo
		{
			get { return LineNoCore.ToString("00000"); }
		}

		protected virtual ZShort LineNoCore
		{
			get { return 0; }
		}

		#endregion

		#region GroupedInventoryUnits

		public ZDecimal GroupedInventoryUnits
		{
			get;
			protected set;
		}

		#endregion

		#region GroupedReceiveUnits

		public ZDecimal GroupedReceiveUnits
		{
			get;
			protected set;
		}

		#endregion

		#region DestLocationCaption

		public ZString DestLocationCaption
		{
			get { return DestLocationCaptionCore; }
		}

		protected virtual ZString DestLocationCaptionCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region DestPalletIDCaption

		public ZString DestPalletIDCaption
		{
			get { return DestPalletIDCaptionCore; }
		}

		protected virtual ZString DestPalletIDCaptionCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region DestWarehouseCaption

		public ZString DestWarehouseCaption
		{
			get { return DestWarehouseCaptionCore; }
		}

		protected virtual ZString DestWarehouseCaptionCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region FromLocationCaption

		public ZString FromLocationCaption
		{
			get { return FromLocationCaptionCore; }
		}

		protected virtual ZString FromLocationCaptionCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region FromPalletIDCaption

		public ZString FromPalletIDCaption
		{
			get { return FromPalletIDCaptionCore; }
		}

		protected virtual ZString FromPalletIDCaptionCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region FromWarehouseCaption

		public ZString FromWarehouseCaption
		{
			get { return FromWarehouseCaptionCore; }
		}

		protected virtual ZString FromWarehouseCaptionCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PalletID2

		public ZString PalletID2
		{
			get { return PalletID2Core; }
		}

		protected virtual ZString PalletID2Core
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region LocationString2

		public ZString LocationString2
		{
			get { return LocationString2Core; }
		}

		protected virtual ZString LocationString2Core
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region TransferFromWarehouseName

		public MultilingualString TransferFromWarehouseName
		{
			get { return TransferFromWarehouseNameCore; }
		}

		protected virtual MultilingualString TransferFromWarehouseNameCore
		{
			get { return (NoResString)ZString.Empty; }
		}

		#endregion

		#region TransferToWarehouseName

		public MultilingualString TransferToWarehouseName
		{
			get { return TransferToWarehouseNameCore; }
		}

		protected virtual MultilingualString TransferToWarehouseNameCore
		{
			get { return (NoResString)ZString.Empty; }
		}

		#endregion

		#region StagingAreaName

		public ZString StagingAreaName
		{
			// For Staging Location BOM, but name has remained unchanged due to compatibility with clients existing document wrappers
			get { return StagingAreaNameCore; }
		}

		protected virtual ZString StagingAreaNameCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region InventoryStatus

		public ZString InventoryStatus
		{
			get { return InventoryStatusCore; }
		}

		protected virtual ZString InventoryStatusCore
		{
			get { return ""; }
		}

		public ZString InventoryStatusDescription
		{
			get { return InventoryStatusDescriptionCore; }
		}

		protected virtual ZString InventoryStatusDescriptionCore
		{
			get { return ""; }
		}

		#endregion

		#region LastCount

		public ZDecimal LastCount
		{
			get { return LastCountCore; }
		}

		protected virtual ZDecimal LastCountCore
		{
			get { return ZDecimal.Zero; }
		}

		#endregion

		#region SystemUnits

		public ZDecimal SystemUnits
		{
			get { return SystemUnitsCore; }
		}

		protected virtual ZDecimal SystemUnitsCore
		{
			get { return ZDecimal.Zero; }
		}

		#endregion

		#region Variance

		public ZDecimal Variance
		{
			get { return VarianceCore; }
		}

		protected virtual ZDecimal VarianceCore
		{
			get { return ZDecimal.Zero; }
		}

		#endregion

		#region IsEmptyLocation

		public ZBool IsEmptyLocation
		{
			get { return IsEmptyLocationCore; }
		}

		protected virtual ZBool IsEmptyLocationCore
		{
			get { return ZBool.False; }
		}

		#endregion

		#region IsLocationEmptyAfterFinalisingPick

		public ZBool IsLocationEmptyAfterPickFinalisation
		{
			get { return IsLocationEmptyAfterPickFinalisationCore; }
		}

		protected virtual ZBool IsLocationEmptyAfterPickFinalisationCore
		{
			get { return ZBool.False; }
		}

		#endregion

		#region Flags

		public ZBool HasDG
		{
			get { return Product != null && Product.UNDGSubstances != null && Product.UNDGSubstances.Count > 0; }
		}

		public ZInt NoOfAttribs
		{
			get
			{
				var result = PartAttribute1.IsEmpty ? 0 : 1;
				result += PartAttribute2.IsEmpty ? 0 : 1;
				result += PartAttribute3.IsEmpty ? 0 : 1;
				result += TrackedSerialNumber.IsEmpty ? 0 : 1;
				result += ExpiryDate.IsEmpty ? 0 : 1;
				result += PackingDate.IsEmpty ? 0 : 1;
				return result;
			}
		}

		public ZInt AttributePrintSizeFactor => maxNoOfAttribsAndDG - NoOfAttribs - (HasDG ? 1 : 0);
		const int maxNoOfAttribsAndDG = 7;

		#endregion

		#region HoldCode

		public ZString HoldCode => HoldCodeCore;

		protected virtual ZString HoldCodeCore => ZString.Empty;

		#endregion

		#region HoldReason

		public ZString HoldReason => HoldReasonCore;

		protected virtual ZString HoldReasonCore => ZString.Empty;

		#endregion

		#region GroupedLineUnitsMet

		public ZDecimal GroupedLineUnitsMet
		{
			get => GroupedLineUnitsMetCore;
			set => GroupedLineUnitsMetCore = value;
		}

		protected virtual ZDecimal GroupedLineUnitsMetCore { get; set; }

		#endregion

		#region CurrencySymbol

		public ZString CurrencySymbol => CurrencySymbolCore;

		protected virtual ZString CurrencySymbolCore => ZString.Empty;

		#endregion

		#region CustomFields

		public CustomFieldWrapperCollection CustomFields
		{
			get { return customFields ?? (customFields = GetCustomFields()); }
		}

		CustomFieldWrapperCollection GetCustomFields()
		{
			var collection = new CustomFieldWrapperCollection(Factory);

			var customLabelsProvider = GetCustomLabelsProvider();
			if (customLabelsProvider != null)
			{
				var configOrg = customLabelsProvider.CustomLabelsProvider.ConfigOrgProvider.ConfigOrg;
				if (configOrg != null)
				{
					var customLabelInfos = customLabelsProvider.CustomLabelsProvider.GetCustomFields(configOrg, Factory).OfType<CustomLabelInfo>().Where(c => c.IsEnabled);
					foreach (var customLabelInfo in customLabelInfos)
					{
						collection.Add(new CustomFieldWrapper(customLabelsProvider.BizOWithCustomFields, customLabelInfo, Factory));
					}
				}
			}

			return collection;
		}

		protected virtual CustomLabelsProviderAndBizO GetCustomLabelsProvider()
		{
			return null;
		}

		CustomFieldWrapperCollection customFields;

		#endregion

		public virtual ZDecimal GroupedUnits { get; set; }
		public ZDecimal SubTotalUnits { get; set; }

		public ZString PositionAfterSorting { get; set; }
		public ZInt Index;
		public ZGuid LinePK { get; set; }
	}
}

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("Packages"), WrapperTypeName("Package")]
	public abstract class PackageWrapper : GenericWrapper
	{
		protected PackageWrapper(BusinessObject objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		#region Identifier

		public ZGuid Identifier
		{
			get { return WrappedBO != null ? WrappedBO.PK : ZGuid.NewZGuid(); }
		}

		#endregion

		#region Additional References

		public CustomsEntryWrapperCollection CustomsEntries
		{
			get { return fCustomsEntries ?? (fCustomsEntries = GetCustomsEntries()); }
		}
		CustomsEntryWrapperCollection fCustomsEntries;
		protected virtual CustomsEntryWrapperCollection GetCustomsEntries() => new CustomsEntryWrapperCollection(Factory);

		#endregion

		#region Bills

		#region HouseBill

		public ZString HouseBill
		{
			get { return GetHouseBill(); }
		}
		protected abstract ZString GetHouseBill();

		#endregion

		#region MasterBill

		public ZString MasterBill
		{
			get { return GetMasterBill(); }
		}
		protected abstract ZString GetMasterBill();

		#endregion

		#endregion

		#region CartonGroupAndSize

		public ZString CartonGroupAndSize
		{
			get { return GetCartonGroupAndSize(); }
		}

		protected abstract ZString GetCartonGroupAndSize();

		#endregion

		#region Commodity

		public CodeAndDescriptionWrapper Commodity
		{
			get { return fCommodity ?? (fCommodity = GetCommodity()); }
		}
		CodeAndDescriptionWrapper fCommodity;
		protected abstract CodeAndDescriptionWrapper GetCommodity();

		#endregion

		#region DamagedReason

		public virtual CodeAndDescriptionWrapper DamagedReason
		{
			get { return fDamagedReason ?? (fDamagedReason = GetDamagedReason()); }
		}
		CodeAndDescriptionWrapper fDamagedReason;
		protected abstract CodeAndDescriptionWrapper GetDamagedReason();

		#endregion

		#region ContainerDetails

		#region Container

		public ContainerWrapper Container
		{
			get { return container ?? (container = GetContainer()); }
		}
		ContainerWrapper container;
		protected abstract ContainerWrapper GetContainer();

		#endregion

		#region ContainerNo

		public ZString ContainerNo
		{
			get { return GetContainerNo(); }
		}
		protected abstract ZString GetContainerNo();

		#endregion

		#region ContainerJobID

		public ZString ContainerJobID
		{
			get { return GetContainerJobID(); }
		}
		protected abstract ZString GetContainerJobID();

		#endregion

		#region Custom Attributes

		public ZString CustomAttribute1
		{
			get { return GetCustomAttribute1(); }
		}

		public ZString CustomAttribute2
		{
			get { return GetCustomAttribute2(); }
		}

		public ZString CustomAttribute3
		{
			get { return GetCustomAttribute3(); }
		}

		public ZString CustomAttribute4
		{
			get { return GetCustomAttribute4(); }
		}

		protected abstract ZString GetCustomAttribute1();
		protected abstract ZString GetCustomAttribute2();
		protected abstract ZString GetCustomAttribute3();
		protected abstract ZString GetCustomAttribute4();

		#endregion

		#region Custom Dates / Decimals / Flags

		public ZDateTime CustomDate1
		{
			get { return GetCustomDate1(); }
		}

		public ZDateTime CustomDate2
		{
			get { return GetCustomDate2(); }
		}

		public ZDecimal CustomDecimal1
		{
			get { return GetCustomDecimal1(); }
		}

		public ZDecimal CustomDecimal2
		{
			get { return GetCustomDecimal2(); }
		}

		public ZBool CustomFlag1
		{
			get { return GetCustomFlag1(); }
		}

		public ZBool CustomFlag2
		{
			get { return GetCustomFlag2(); }
		}

		protected abstract ZDateTime GetCustomDate1();
		protected abstract ZDateTime GetCustomDate2();
		protected abstract ZDecimal GetCustomDecimal1();
		protected abstract ZDecimal GetCustomDecimal2();
		protected abstract ZBool GetCustomFlag1();
		protected abstract ZBool GetCustomFlag2();

		#endregion

		#endregion

		#region Description

		public ZString Description
		{
			get { return GetDescription(); }
		}
		protected abstract ZString GetDescription();

		#endregion

		#region DisplayOrder

		public ZString DisplayOrder
		{
			get { return GetDisplayOrder(); }
		}

		protected abstract ZString GetDisplayOrder();

		#endregion

		#region FreightPackLine

		public PackLine FreightPackLine
		{
			get { return GetFreightPackLine(); }
		}
		protected abstract PackLine GetFreightPackLine();

		#endregion

		#region HarmonizedCodes

		public ZString HarmonizedCode
		{
			get { return GetHarmonizedCode(); }
		}
		protected abstract ZString GetHarmonizedCode();

		public HarmonisedCodeWrapperCollection HarmonizedCodes
		{
			get { return harmonizedCodes ?? (harmonizedCodes = GetHarmonizedCodes()); }
		}
		HarmonisedCodeWrapperCollection harmonizedCodes;
		protected abstract HarmonisedCodeWrapperCollection GetHarmonizedCodes();

		#endregion

		#region Indent

		public ZString Indent
		{
			get { return GetIndent(); }
		}

		protected abstract ZString GetIndent();

		#endregion

		#region Inners

		public ZInt Inners
		{
			get { return GetInners(); }
		}

		protected abstract ZInt GetInners();

		#endregion

		#region InnersDetail

		public ZString InnersDetail
		{
			get { return GetInnersDetail(); }
		}

		protected abstract ZString GetInnersDetail();

		#endregion

		#region ItmNumber

		public ZShort ItmNumber
		{
			get { return GetItemNumber(); }
		}
		protected abstract ZShort GetItemNumber();

		#endregion

		#region LinePrice

		public ZDecimal LinePrice
		{
			get { return GetLinePrice(); }
		}
		protected abstract ZDecimal GetLinePrice();

		#endregion

		#region CommonCurrency

		public CurrencyWrapper CommonCurrency => GetCommonCurrency();

		protected virtual CurrencyWrapper GetCommonCurrency()
		{
			return new CurrencyWrapper(null, Factory);
		}

		#endregion

		#region MarksAndNumbers

		public ZString MarksAndNumbers
		{
			get { return GetMarksAndNumbers(); }
		}
		protected abstract ZString GetMarksAndNumbers();

		#endregion

		#region MostRecentAudit

		public PackageAuditWrapper MostRecentAudit
		{
			get { return GetMostRecentAudit(); }
		}
		protected abstract PackageAuditWrapper GetMostRecentAudit();

		#endregion

		#region NMFC

		public ZString NMFC
		{
			get
			{
				if (nMFC.IsEmpty)
				{
					nMFC = GetNMFC();
				}
				return nMFC;
			}
		}
		ZString nMFC = "";

		protected virtual ZString GetNMFC()
		{
			return "";
		}

		#endregion

		#region OutturnComment

		public ZString OutturnComment
		{
			get { return GetOutturnComment(); }
		}
		protected abstract ZString GetOutturnComment();

		#endregion

		#region Origin

		public LocationWrapper Origin
		{
			get { return origin ?? (origin = GetOrigin()); }
		}
		LocationWrapper origin;
		protected abstract LocationWrapper GetOrigin();

		#endregion

		#region Packages

		#region DamagedPackages

		public PackQTYWrapper DamagedPackages
		{
			get { return fDamagedPackages ?? (fDamagedPackages = GetDamagedPackages()); }
		}
		PackQTYWrapper fDamagedPackages;
		protected abstract PackQTYWrapper GetDamagedPackages();

		#endregion

		#region OutturnedPackages

		public PackQTYWrapper OutturnedPackages
		{
			get { return fOutturnedPackages ?? (fOutturnedPackages = GetOutturnedPackages()); }
		}
		PackQTYWrapper fOutturnedPackages;
		protected abstract PackQTYWrapper GetOutturnedPackages();

		#endregion

		#region Packages

		public PackQTYWrapper Packages
		{
			get { return fPackages ?? (fPackages = GetPackages()); }
		}
		PackQTYWrapper fPackages;
		protected abstract PackQTYWrapper GetPackages();

		#endregion

		#region PillagedPackages

		public PackQTYWrapper PillagedPackages
		{
			get { return fPillagedPackages ?? (fPillagedPackages = GetPillagedPackages()); }
		}
		PackQTYWrapper fPillagedPackages;
		protected abstract PackQTYWrapper GetPillagedPackages();

		#endregion

		#region FumigatedPackages

		public PackQTYWrapper FumigatedPackages
		{
			get { return fFumigatedPackages ?? (fFumigatedPackages = GetFumigatedPackages()); }
		}
		PackQTYWrapper fFumigatedPackages;
		protected virtual PackQTYWrapper GetFumigatedPackages() => PackQTYWrapper.Empty;

		#endregion

		#region NonStackablePackages

		public PackQTYWrapper NonStackablePackages
		{
			get { return fNonStackablePackages ?? (fNonStackablePackages = GetNonStackablePackages()); }
		}
		PackQTYWrapper fNonStackablePackages;
		protected virtual PackQTYWrapper GetNonStackablePackages() => PackQTYWrapper.Empty;

		#endregion

		#region TopLoadOnlyPackages

		public PackQTYWrapper TopLoadOnlyPackages
		{
			get { return fTopLoadOnlyPackages ?? (fTopLoadOnlyPackages = GetTopLoadOnlyPackages()); }
		}
		PackQTYWrapper fTopLoadOnlyPackages;
		protected virtual PackQTYWrapper GetTopLoadOnlyPackages() => PackQTYWrapper.Empty;

		#endregion

		#region HeatTreatedPackages

		public PackQTYWrapper HeatTreatedPackages
		{
			get { return fHeatTreatedPackages ?? (fHeatTreatedPackages = GetHeatTreatedPackages()); }
		}
		PackQTYWrapper fHeatTreatedPackages;
		protected virtual PackQTYWrapper GetHeatTreatedPackages() => PackQTYWrapper.Empty;

		#endregion

		#endregion

		#region ISPMPalletPackages

		public PackQTYWrapper ISPMPalletPackages
		{
			get { return fISPMPalletPackages ?? (fISPMPalletPackages = GetISPMPalletPackages()); }
		}
		PackQTYWrapper fISPMPalletPackages;
		protected virtual PackQTYWrapper GetISPMPalletPackages() => PackQTYWrapper.Empty;

		#endregion

		#region PackageOrderReference

		public PackageOrderReferenceWrapper PackageOrderReference => packageOrderReference ??= GetPackageOrderReference();
		PackageOrderReferenceWrapper packageOrderReference;

		protected virtual PackageOrderReferenceWrapper GetPackageOrderReference() => new PackageOrderReferenceWrapper(null, Factory);

		#endregion

		#region PackedItem

		public PackedItemWrapper PackedItem
		{
			get { return packedItem ?? (packedItem = GetPackedItem()); }
		}

		PackedItemWrapper packedItem;

		protected abstract PackedItemWrapper GetPackedItem();

		#endregion

		#region PackedItemCount

		public ZInt PackedItemCount
		{
			get { return GetPackedItemCount(); }
		}

		protected abstract ZInt GetPackedItemCount();

		#endregion

		#region PackedItems

		public PackedItemWrapperCollection PackedItems
		{
			get { return packedItems ?? (packedItems = GetPackedItems()); }
		}
		PackedItemWrapperCollection packedItems;
		protected abstract PackedItemWrapperCollection GetPackedItems();

		#endregion

		#region PackedPackages

		public PackageWrapperCollection PackedPackages
		{
			get { return fPackedPackages ?? (fPackedPackages = GetPackedPackages()); }
		}
		PackageWrapperCollection fPackedPackages;
		protected virtual PackageWrapperCollection GetPackedPackages() => new PackageWrapperCollection(Factory);

		#endregion

		#region FirstLevelPackedPackages

		public PackageWrapperCollection FirstLevelPackedPackages
		{
			get { return firstLevelPackedPackages ?? (firstLevelPackedPackages = GetFirstLevelPackedPackages()); }
		}
		PackageWrapperCollection firstLevelPackedPackages;
		protected virtual PackageWrapperCollection GetFirstLevelPackedPackages() => new PackageWrapperCollection(Factory);

		#endregion

		#region PackingOrder

		public ZInt PackingOrder
		{
			get { return GetPackingOrder(); }
		}
		protected abstract ZInt GetPackingOrder();

		#endregion

		#region OutterPackageSequence

		public ZShort OutterPackageSequence
		{
			get { return GetOutterPackageSequence(); }
		}

		protected abstract ZShort GetOutterPackageSequence();

		#endregion

		#region OutterPackagesCount

		public ZShort OutterPackagesCount
		{
			get { return GetOutterPackagesCount(); }
		}

		protected abstract ZShort GetOutterPackagesCount();

		#endregion

		#region InnerPackagesCount

		public ZShort InnerPackagesCount
		{
			get { return GetInnerPackagesCount(); }
		}

		protected abstract ZShort GetInnerPackagesCount();

		#endregion

		#region Parent

		public FreightWrapper Parent
		{
			get { return GetParent(); }
		}
		protected abstract FreightWrapper GetParent();

		#endregion

		#region Products

		public PackProductWrapperCollection Products
		{
			get { return products ?? (products = GetProducts()); }
		}
		PackProductWrapperCollection products;
		protected abstract PackProductWrapperCollection GetProducts();

		#endregion

		#region RefNumber / BarCode

		#region IsPackageIdValidSSCCBarCode

		public ZBool IsPackageIdValidSSCCBarCode
		{
			get { return GetIsPackageIdValidSSCCBarCode(); }
		}

		protected virtual ZBool GetIsPackageIdValidSSCCBarCode()
		{
			return false;
		}

		#endregion

		#region RefNumber

		public ZString RefNumber
		{
			get { return GetRefNumber(); }
		}

		protected abstract ZString GetRefNumber();

		#endregion

		#region ExportRefNumber

		public ZString ExportRefNumber
		{
			get { return GetExportRefNumber(); }
		}

		protected abstract ZString GetExportRefNumber();

		#endregion

		#region ImportRefNumber

		public ZString ImportRefNumber
		{
			get { return GetImportRefNumber(); }
		}

		protected abstract ZString GetImportRefNumber();

		#endregion

		#region PackageReferenceNumberHeaderText

		public ZString PackageReferenceHeaderText
		{
			get { return GetPackageReferenceHeaderText(); }
		}

		protected abstract ZString GetPackageReferenceHeaderText();

		#endregion

		#region RefNumberWithSSCCPrefix

		public ZString RefNumberWithSSCCPrefix
		{
			get { return GetRefNumberWithSSCCPrefix(); }
		}

		protected virtual ZString GetRefNumberWithSSCCPrefix()
		{
			return "(00)" + RefNumber;
		}

		#endregion

		#region PackageBarcode

		public ZString PackageBarcode
		{
			get { return GetPackageBarcode(); }
		}

		protected ZString GetPackageBarcode()
		{
			return new TextBarcode(RefNumber).TextAs128sFontString;
		}

		#endregion

		#region PackageBarcodeWithOptimisedEncoding

		public ZString PackageBarcodeWithOptimisedEncoding
		{
			get { return GetPackageBarcodeWithOptimisedEncoding(); }
		}

		protected ZString GetPackageBarcodeWithOptimisedEncoding()
		{
			const bool USE_OPTIMISED_ENCODING = true;
			return new TextBarcode(RefNumber, USE_OPTIMISED_ENCODING).TextAs128sFontString;
		}

		#endregion

		#region PackageBarcodeWithSSCCPrefix

		public ZString PackageBarcodeWithSSCCPrefix
		{
			get { return GetPackageBarcodeWithSSCCPrefixCore(); }
		}

		public ZString PackageBarcodeWithSSCCPrefixNonOptimisedEncoding
		{
			get
			{
				const bool USE_OPTIMISED_ENCODING = false;
				return GetPackageBarcodeWithSSCCPrefix(USE_OPTIMISED_ENCODING);
			}
		}

		protected virtual ZString GetPackageBarcodeWithSSCCPrefixCore()
		{
			const bool USE_OPTIMISED_ENCODING = true;
			return GetPackageBarcodeWithSSCCPrefix(USE_OPTIMISED_ENCODING);
		}

		ZString GetPackageBarcodeWithSSCCPrefix(bool shouldOptimiseEncoding)
		{
			const bool IS_GS1_128_BarCode = true;
			return new TextBarcode("00" + RefNumber, shouldOptimiseEncoding, IS_GS1_128_BarCode).TextAs128sFontString;
		}

		#endregion

		#region PostcodeBarcodeNumber

		public ZString PostcodeBarcodeNumber
		{
			get { return GetPostcodeBarcodeNumber(); }
		}

		protected abstract ZString GetPostcodeBarcodeNumber();

		protected ZString ConsigneeCountryISONumericCode
		{
			get { return GetConsigneeCountryISONumericCodeCore(); }
		}

		protected virtual ZString GetConsigneeCountryISONumericCodeCore()
		{
			var parent = Parent;
			var consigneeAddress = parent != null ? parent.DeliveryAddress : null;
			var country = consigneeAddress != null ? consigneeAddress.Country : null;
			return country != null ? country.ISONumericCode : ZString.Empty;
		}

		protected ZString ConsigneePostCode
		{
			get { return GetConsigneePostCodeCore(); }
		}

		protected virtual ZString GetConsigneePostCodeCore()
		{
			var parent = Parent;
			var consigneeAddress = parent != null ? parent.DeliveryAddress : null;
			return consigneeAddress != null ? consigneeAddress.PostCode : ZString.Empty;
		}

		#endregion

		#region PostcodeBarcode

		public ZString PostcodeBarcode
		{
			get { return GetPostcodeBarcode(); }
		}

		protected ZString GetPostcodeBarcode()
		{
			const bool USE_OPTIMISED_ENCODING = true;
			const bool IS_GS1_128_BarCode = true;

			var result = ZString.Empty;

			if (!PostcodeBarcodeNumber.IsEmpty)
			{
				result = new TextBarcode(string.Format("{0}{1}", PostCodePrefix, PostcodeBarcodeNumber), USE_OPTIMISED_ENCODING, IS_GS1_128_BarCode).TextAs128sFontString;
			}

			return result;
		}

		protected ZString PostCodePrefix
		{
			get
			{
				var parent = Parent;
				return parent != null && parent.IsDomestic ? "420" : "421";
			}
		}

		#endregion

		#region PostcodeBarcodeNumberWithPrefix

		public ZString PostcodeBarcodeNumberWithPrefix
		{
			get
			{
				var postcodeBarcodeNumber = PostcodeBarcodeNumber;
				return !postcodeBarcodeNumber.IsEmpty ? string.Format("({0}) {1}", PostCodePrefix, PostcodeBarcodeNumber) : "";
			}
		}

		public ZString PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix
		{
			get
			{
				// This property is used only for Retailers label. Clients always want to print 421 on the label regardless of post code is local or not.
				ZString postcodeBarcodeNumberAndISONumericCountryCode = ConsigneeCountryISONumericCode + ConsigneePostCode;
				return !postcodeBarcodeNumberAndISONumericCountryCode.IsEmpty ? string.Format("(421) {0}", postcodeBarcodeNumberAndISONumericCountryCode) : "";
			}
		}

		public ZString PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText
		{
			get
			{
				const bool USE_OPTIMISED_ENCODING = false;
				const bool IS_GS1_128_BarCode = true;
				return new TextBarcode(PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix, USE_OPTIMISED_ENCODING, IS_GS1_128_BarCode).TextAs128sFontString;
			}
		}

		public ZString PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText
		{
			get
			{
				const bool USE_OPTIMISED_ENCODING = false;
				const bool IS_GS1_128_BarCode = true;
				var consigneeAddressShortCode = Parent != null && Parent.WarehouseJob != null && Parent.WarehouseJob.ConsigneeAddress != null
												? Parent.WarehouseJob.ConsigneeAddress.ShortCode
												: ZString.Empty;

				return new TextBarcode(PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix +
					(!consigneeAddressShortCode.IsEmpty ? (ZString)("(90)" + consigneeAddressShortCode) : ZString.Empty), USE_OPTIMISED_ENCODING, IS_GS1_128_BarCode).TextAs128sFontString;
			}
		}

		#endregion

		#endregion

		#region Seals

		public ZString Seals
		{
			get { return GetSeals(); }
		}

		protected virtual ZString GetSeals()
		{
			return ZString.Empty;
		}

		#endregion

		#region PackageTemperatures

		public ZString PackageTemperatures
		{
			get { return GetPackageTemperatures(); }
		}

		protected virtual ZString GetPackageTemperatures()
		{
			return ZString.Empty;
		}

		#endregion

		#region UNDGSubstances

		public UNDGSubstanceWrapperCollection UNDGSubstances
		{
			get { return GetUNDGSubstances(); }
		}
		protected abstract UNDGSubstanceWrapperCollection GetUNDGSubstances();

		#endregion

		#region Volume / Weight / Dimensions

		#region Volume

		public VolumeWrapper Volume
		{
			get { return fVolume ?? (fVolume = GetVolume()); }
		}
		VolumeWrapper fVolume;
		protected abstract VolumeWrapper GetVolume();

		#endregion

		#region OutturnedVolume

		public VolumeWrapper OutturnedVolume
		{
			get { return fOutturnedVolume ?? (fOutturnedVolume = GetOutturnedVolume()); }
		}
		VolumeWrapper fOutturnedVolume;
		protected abstract VolumeWrapper GetOutturnedVolume();

		#endregion

		#region Weight

		public WeightWrapper Weight
		{
			get { return fWeight ?? (fWeight = GetWeight()); }
		}
		WeightWrapper fWeight;
		protected abstract WeightWrapper GetWeight();

		#endregion

		#region OutturnedWeight

		public WeightWrapper OutturnedWeight
		{
			get { return fOutturnedWeight ?? (fOutturnedWeight = GetOutturnedWeight()); }
		}
		WeightWrapper fOutturnedWeight;
		protected abstract WeightWrapper GetOutturnedWeight();

		#endregion

		#region Dimensions

		public DimensionsWrapper Dimensions
		{
			get { return dimensions ?? (dimensions = GetDimensions()); }
		}
		DimensionsWrapper dimensions;
		protected abstract DimensionsWrapper GetDimensions();

		#endregion

		#endregion

		#region PickLocation

		public ZString PickLocation
		{
			get { return GetPickLocation(); }
		}

		protected virtual ZString GetPickLocation()
		{
			return "";
		}

		#endregion

		#region PickMethod

		public ZString PickMethod
		{
			get { return GetPickMethod(); }
		}

		protected virtual ZString GetPickMethod()
		{
			return "";
		}

		#endregion

		#region PickGroup

		public ZString PickGroup
		{
			get { return GetPickGroup(); }
		}

		protected virtual ZString GetPickGroup()
		{
			return ZString.Empty;
		}

		#endregion

		#region AreaName

		public ZString AreaName
		{
			get { return GetAreaName(); }
		}

		protected virtual ZString GetAreaName()
		{
			return ZString.Empty;
		}

		#endregion

		#region Flags

		#region HasPackedItem

		public ZBool HasPackedItem
		{
			get { return GetHasPackedItem(); }
		}

		protected abstract ZBool GetHasPackedItem();

		#endregion

		#region HasSingleProduct

		public ZBool HasSingleProduct
		{
			get { return GetHasSingleProduct(); }
		}

		protected abstract ZBool GetHasSingleProduct();

		#endregion

		#region HasSingleAttribute

		public ZBool HasSingleAttribute1 => GetHasSingleAttribute1();
		protected virtual ZBool GetHasSingleAttribute1() => ZBool.False;

		public ZBool HasSingleAttribute2 => GetHasSingleAttribute2();
		protected virtual ZBool GetHasSingleAttribute2() => ZBool.False;

		public ZBool HasSingleAttribute3 => GetHasSingleAttribute3();
		protected virtual ZBool GetHasSingleAttribute3() => ZBool.False;

		public ZBool HasSingleExpiryDate => GetHasSingleExpiryDate();
		protected virtual ZBool GetHasSingleExpiryDate() => ZBool.False;

		public ZBool HasSinglePackingDate => GetHasSinglePackingDate();
		protected virtual ZBool GetHasSinglePackingDate() => ZBool.False;

		#endregion

		#region IsExclusive

		public ZBool IsExclusive
		{
			get { return GetIsExclusive(); }
		}

		protected abstract ZBool GetIsExclusive();

		#endregion

		#region IsExpiryUsed

		public ZBool IsExpiryUsed
		{
			get { return GetIsExpiryUsed(); }
		}

		protected abstract ZBool GetIsExpiryUsed();

		#endregion

		#region IsPackingDateUsed

		public ZBool IsPackingDateUsed
		{
			get { return GetIsPackingDateUsed(); }
		}

		protected abstract ZBool GetIsPackingDateUsed();

		#endregion

		#region IsPartAttrib1Used

		public ZBool IsPartAttrib1Used
		{
			get { return GetIsPartAttrib1Used(); }
		}

		protected abstract ZBool GetIsPartAttrib1Used();

		#endregion

		#region IsPartAttrib2Used

		public ZBool IsPartAttrib2Used
		{
			get { return GetIsPartAttrib2Used(); }
		}

		protected abstract ZBool GetIsPartAttrib2Used();

		#endregion

		#region IsPartAttrib3Used

		public ZBool IsPartAttrib3Used
		{
			get { return GetIsPartAttrib3Used(); }
		}

		protected abstract ZBool GetIsPartAttrib3Used();

		#endregion

		#region IsTrackedSerialUsed

		public ZBool IsTrackedSerialUsed
		{
			get { return GetIsTrackedSerialUsed(); }
		}

		protected abstract ZBool GetIsTrackedSerialUsed();

		#endregion

		#region IsTopLevelNonContainerisedPackage

		public ZBool IsTopLevelNonContainerisedPackage
		{
			get { return GetIsTopLevelNonContainerisedPackage(); }
		}

		protected abstract ZBool GetIsTopLevelNonContainerisedPackage();

		#endregion

		#region IsTopLevelPackage

		public ZBool IsTopLevelPackage
		{
			get { return GetIsTopLevelPackage(); }
		}

		protected abstract ZBool GetIsTopLevelPackage();

		#endregion

		#region IsOuterPackage

		public ZBool IsOuterPackage
		{
			get { return GetIsOuterPackage(); }
		}

		protected abstract ZBool GetIsOuterPackage();

		#endregion

		#region PackageState

		public PackageStateWrapper PackageState
		{
			get { return GetPackageState(); }
		}

		protected abstract PackageStateWrapper GetPackageState();

		#endregion

		#region HandlingUnit

		public PackageWrapper HandlingUnit
		{
			get { return handlingUnit ?? (handlingUnit = GetHandlingUnit()); }
		}
		PackageWrapper handlingUnit;

		protected virtual PackageWrapper GetHandlingUnit()
		{
			return null;
		}

		#endregion

		#region TopLevelHandlingUnit

		public PackageWrapper TopLevelHandlingUnit
		{
			get { return topLevelHandlingUnit ?? (topLevelHandlingUnit = GetTopLevelHandlingUnit()); }
		}
		PackageWrapper topLevelHandlingUnit;

		protected virtual PackageWrapper GetTopLevelHandlingUnit()
		{
			return null;
		}

		#endregion

		#endregion

		#region StarTrack_QRCodeText

		public ZString StarTrack_QRCodeText => GetStarTrack_QRCodeText();
		protected abstract ZString GetStarTrack_QRCodeText();

		#endregion

		#region InnerPackages

		public PackageWrapperCollection InnerPackages
		{
			get { return innerPackages ?? (innerPackages = GetNewInnerPackages()); }
		}
		PackageWrapperCollection innerPackages;
		protected virtual PackageWrapperCollection GetNewInnerPackages() => new PackageWrapperCollection(Factory);

		#endregion

		#region PackLineId

		public ZString PackLineId => GetPackLineId();
		protected virtual ZString GetPackLineId() => ZString.Empty;

		#endregion

		#region UOMType

		public CodeAndDescriptionWrapper UOMType => uomType ?? (uomType = GetUOMType());
		CodeAndDescriptionWrapper uomType;
		protected virtual CodeAndDescriptionWrapper GetUOMType() => CodeAndDescriptionWrapper.Empty;

		#endregion

		#region PackageNumber

		public ZInt PackageNumber { get ; set; }

		#endregion
	}
}

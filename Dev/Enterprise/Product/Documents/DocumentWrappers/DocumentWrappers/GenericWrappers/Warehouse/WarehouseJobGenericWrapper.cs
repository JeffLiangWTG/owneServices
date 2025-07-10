using System;
using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse.ChildWrappers;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	/// <remarks>
	/// THIS CLASS IS A STANDARD INTERFACE FOR DOCBUILDER WAREHOUSE DOCUMENTS ONLY.
	/// </remarks>
	// Each required document should have it's own document wrapper subclassed from here.  Note this does not necessarily need to follow the Warehouse Bizo model.
	[WrapperTypeName("WarehouseJob")]
	public abstract class WarehouseJobGenericWrapper : GenericWrapper, IDocTypeCode
	{
		#region CustomCompanyLogo

		public virtual Image CustomCompanyLogo
		{
			get { return null; }
		}

		#endregion

		#region Constructors

		// todo - refactor out and use the strategy constructor below instead.
		public WarehouseJobGenericWrapper(BusinessObject businessObject, BusinessObjectFactory factory)
			: base(businessObject, factory)
		{
		}

		public WarehouseJobGenericWrapper(WhsPopulateStrategy strategy)
			: base(strategy.WrappedBO, strategy.Factory)
		{
			this.populateStrategy = strategy;
		}

		public static WarehouseJobGenericWrapper New(BusinessObject businessObject, BusinessObjectFactory factory)
		{
			switch (businessObject)
			{
				case WhsOrder order:
					return new WarehouseOrderWrapper(order, factory);

				case WhsWorkOrder workOrder:
					return new WarehouseWorkOrderWrapper(workOrder, factory);

				case WhsDynamicWorkOrder dynamicWorkOrder:
					return new WarehouseDynamicWorkOrderWrapper(dynamicWorkOrder, factory);

				default:
					return null;
			}
		}

		#endregion

		#region Properties
		// EVERY PROPERTY EVER REQUIRED FOR ANY WAREHOUSE DOCUMENT MUST RESIDE HERE.  (Please add in alphabetical order)

		#region Addresses

		#region ConsigneeAddress

		public virtual AddressWrapper ConsigneeAddress
		{
			get { return PopulateStrategy.ConsigneeAddress; }
		}

		#endregion

		#region DropOffAddress

		public virtual AddressWrapper DropOffAddress
		{
			get { return DropOffAddressCore; }
		}

		protected virtual AddressWrapper DropOffAddressCore
		{
			get { return null; }
		}

		#endregion

		#region GoodsBillToAddress

		public virtual AddressWrapper GoodsBillToAddress
		{
			get { return GoodsBillToAddressCore; }
		}

		protected virtual AddressWrapper GoodsBillToAddressCore
		{
			get { return null; }
		}

		#endregion

		#region PickUpAddress

		public virtual AddressWrapper PickUpAddress
		{
			get { return PickUpAddressCore; }
		}

		protected virtual AddressWrapper PickUpAddressCore
		{
			get { return null; }
		}

		#endregion

		#region SupplierDocAddress

		public AddressWrapper SupplierDocAddress
		{
			get { return SupplierDocAddressCore; }
		}

		protected virtual AddressWrapper SupplierDocAddressCore
		{
			get { return null; }
		}

		#endregion

		#region TransportBillToAddress

		public virtual AddressWrapper TransportBillToAddress
		{
			get { return TransportBillToAddressCore; }
		}

		protected virtual AddressWrapper TransportBillToAddressCore
		{
			get { return null; }
		}

		#endregion

		#region DistributionCenterAddress

		public virtual AddressWrapper DistributionCentreAddress
		{
			get { return DistributionCentreAddressCore; }
		}

		protected virtual AddressWrapper DistributionCentreAddressCore
		{
			get { return null; }
		}

		#endregion

		#endregion

		#region AllowPartialLoading

		public ZBool AllowPartialLoading => AllowPartialLoadingCore;

		protected virtual ZBool AllowPartialLoadingCore => ZBool.False;

		#endregion

		#region ArrivalDate

		public ZDateTime ArrivalDate => ArrivalDateCore;

		protected virtual ZDateTime ArrivalDateCore => ZDateTime.Empty;

		#endregion

		#region BookingDate

		public ZDateTime BookingDate => BookingDateCore;

		protected virtual ZDateTime BookingDateCore => ZDateTime.Empty;

		#endregion

		#region BOLNumber

		public virtual ZString BOLNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region CartageAdviceClosingText

		public virtual MultilingualString CartageAdviceClosingText
		{
			get { return (NoResString)ZString.Empty; }
		}

		#endregion

		#region CartageAdviceOpeningText

		public virtual MultilingualString CartageAdviceOpeningText
		{
			get { return (NoResString)ZString.Empty; }
		}

		#endregion

		#region CartageDropMode

		public virtual ZString CartageDropMode
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Client

		public virtual OrganisationWrapper Client
		{
			get { return null; }
		}

		#endregion

		#region ClientRequestedBillToParty

		public OrganisationWrapper ClientRequestedBillToParty => ClientRequestedBillToPartyCore;

		protected virtual OrganisationWrapper ClientRequestedBillToPartyCore => null;

		#endregion

		#region CODAmount

		public virtual MoneyWrapper CODAmount
		{
			get { return null; }
		}

		#endregion

		#region CODType

		public virtual CodeAndDescriptionWrapper CODType
		{
			get { return null; }
		}

		#endregion

		#region ConfirmationInstruction

		public virtual LabelValuePairWrapper ConfirmationInstructions
		{
			get { return ConfirmationInstructionsCore; }
		}

		protected virtual LabelValuePairWrapper ConfirmationInstructionsCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region Consignee

		public virtual OrganisationWrapper Consignee
		{
			get { return PopulateStrategy.Consignee; }
		}

		#endregion

		#region Consignor

		public virtual OrganisationWrapper Consignor
		{
			get { return null; }
		}

		#endregion

		#region SupplierBuyerLink

		public virtual SupplierBuyerLinkWrapper SupplierBuyerLink
		{
			get { return null; }
		}

		#endregion

		#region ConsolidatedInvoiceRef

		public virtual ZString ConsolidatedInvoiceRef
		{
			get { return PopulateStrategy.ConsolidatedInvoiceRef; }
		}

		#endregion

		#region ContainerNumberAndTypeLine

		public virtual ZString ContainerNumberAndTypeLine
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Custom Fields

		#region Attributes

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

		public ZString CustomAttribute5
		{
			get { return GetCustomAttribute5(); }
		}

		protected virtual ZString GetCustomAttribute1()
		{
			return "";
		}

		protected virtual ZString GetCustomAttribute2()
		{
			return "";
		}

		protected virtual ZString GetCustomAttribute3()
		{
			return "";
		}

		protected virtual ZString GetCustomAttribute4()
		{
			return "";
		}

		protected virtual ZString GetCustomAttribute5()
		{
			return "";
		}

		#endregion

		#region Dates

		public ZDateTime UnloadCompleteTime
		{
			get { return GetUnloadCompleteTime(); }
		}

		protected virtual ZDateTime GetUnloadCompleteTime()
		{
			return ZDateTime.Empty;
		}

		public ZDateTime CustomDate1
		{
			get { return GetCustomDate1(); }
		}

		public ZDateTime CustomDate2
		{
			get { return GetCustomDate2(); }
		}

		protected virtual ZDateTime GetCustomDate1()
		{
			return ZDateTime.Empty;
		}

		protected virtual ZDateTime GetCustomDate2()
		{
			return ZDateTime.Empty;
		}

		#endregion

		#region Decimals

		public ZDecimal CustomDecimal1
		{
			get { return GetCustomDecimal1(); }
		}

		public ZDecimal CustomDecimal2
		{
			get { return GetCustomDecimal2(); }
		}

		public ZDecimal CustomDecimal3
		{
			get { return GetCustomDecimal3(); }
		}

		public ZDecimal CustomDecimal4
		{
			get { return GetCustomDecimal4(); }
		}

		public ZDecimal CustomDecimal5
		{
			get { return GetCustomDecimal5(); }
		}

		protected virtual ZDecimal GetCustomDecimal1()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZDecimal GetCustomDecimal2()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZDecimal GetCustomDecimal3()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZDecimal GetCustomDecimal4()
		{
			return ZDecimal.Zero;
		}

		protected virtual ZDecimal GetCustomDecimal5()
		{
			return ZDecimal.Zero;
		}

		#endregion

		#region Flags

		public ZBool CustomFlag1
		{
			get { return GetCustomFlag1(); }
		}

		public ZBool CustomFlag2
		{
			get { return GetCustomFlag2(); }
		}

		public ZBool CustomFlag3
		{
			get { return GetCustomFlag3(); }
		}

		public ZBool CustomFlag4
		{
			get { return GetCustomFlag4(); }
		}

		public ZBool CustomFlag5
		{
			get { return GetCustomFlag5(); }
		}

		protected virtual ZBool GetCustomFlag1()
		{
			return ZBool.False;
		}

		protected virtual ZBool GetCustomFlag2()
		{
			return ZBool.False;
		}

		protected virtual ZBool GetCustomFlag3()
		{
			return ZBool.False;
		}

		protected virtual ZBool GetCustomFlag4()
		{
			return ZBool.False;
		}

		protected virtual ZBool GetCustomFlag5()
		{
			return ZBool.False;
		}

		#endregion

		#endregion

		#region CustomerReference

		public virtual LabelValuePairWrapper CustomerReference
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region CustomsStatus

		public CodeAndDescriptionWrapper CustomsStatus => CustomsStatusCore;

		protected virtual CodeAndDescriptionWrapper CustomsStatusCore => null;

		#endregion

		#region DeliveryRoute

		public ZString DeliveryRoute => DeliveryRouteCore;

		protected virtual ZString DeliveryRouteCore => ZString.Empty;

		#endregion

		#region Destination

		public virtual PlaceAndDateWrapper Destination
		{
			get { return PopulateStrategy.Destination; }
		}

		#endregion

		#region DispatchDriverName

		public ZString DispatchDriverName => DispatchDriverNameCore;

		protected virtual ZString DispatchDriverNameCore => ZString.Empty;

		#endregion

		#region DispatchDriverSignature

		public Image DispatchDriverSignature => DispatchDriverSignatureCore;

		protected virtual Image DispatchDriverSignatureCore => null;

		#endregion

		#region DockDoor

		public ZString DockDoor
		{
			get { return DockDoorCore; }
		}

		protected virtual ZString DockDoorCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		public EquipmentWrapper TransportationUnit
		{
			get { return TransportationUnitCore; }
		}

		protected virtual EquipmentWrapper TransportationUnitCore
		{
			get { return null; }
		}

		#region DocumentTitle

		public ZString DocumentTitle
		{
			get { return DocumentTitleCore; }
		}

		protected virtual ZString DocumentTitleCore
		{
			get { return ""; }
		}

		#endregion

		#region TransportZone

		public ZString TransportZone
		{
			get { return TransportZoneCore; }
		}

		protected virtual ZString TransportZoneCore
		{
			get { return ""; }
		}

		#endregion

		#region DropMode

		public virtual CodeAndDescriptionWrapper DropMode
		{
			get { return null; }
		}

		#endregion

		#region FinalisedDate

		public LabelValuePairWrapper FinalisedDate
		{
			get { return FinalisedDateCore; }
		}

		protected virtual LabelValuePairWrapper FinalisedDateCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region FulfillRule

		public virtual CodeAndDescriptionWrapper FulfillRule
		{
			get { return null; }
		}

		#endregion

		#region HandlingInstructions

		public LabelValuePairWrapper HandlingInstructions
		{
			get { return HandlingInstructionsCore; }
		}

		protected virtual LabelValuePairWrapper HandlingInstructionsCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region HasDispatchDriverSignature

		public ZBool HasDispatchDriverSignature => HasDispatchDriverSignatureCore;

		protected virtual ZBool HasDispatchDriverSignatureCore => DispatchDriverSignature != null;

		#endregion

		#region HasMultipleUnits

		public ZBool HasMultipleStockKeepingUnits
		{
			get { return HasMultipleStockKeepingUnitsCore; }
		}

		protected virtual ZBool HasMultipleStockKeepingUnitsCore
		{
			get { return ZBool.False; }
		}

		public ZBool HasMultipleWeightUnits
		{
			get { return HasMultipleWeightUnitsCore; }
		}

		protected virtual ZBool HasMultipleWeightUnitsCore
		{
			get { return ZBool.False; }
		}

		public ZBool HasMultipleVolumeUnits
		{
			get { return HasMultipleVolumeUnitsCore; }
		}

		protected virtual ZBool HasMultipleVolumeUnitsCore
		{
			get { return false; }
		}

		#endregion

		#region HasNonPickedItems

		public virtual ZBool HasNonPickedItems
		{
			get { return false; }
		}

		#endregion

		#region HasReceiveDriverSignature

		public ZBool HasReceiveDriverSignature => HasReceiveDriverSignatureCore;

		protected virtual ZBool HasReceiveDriverSignatureCore => ReceiveDriverSignature != null;

		#endregion

		#region HasShortfallItems

		public virtual ZBool HasShortfallItems
		{
			get { return false; }
		}

		#endregion

		#region IncoTerm

		public virtual CodeAndDescriptionWrapper IncoTerm
		{
			get { return PopulateStrategy.IncoTerm; }
		}

		#endregion

		#region Insurance

		public virtual MoneyWrapper Insurance
		{
			get { return null; }
		}

		#endregion

		#region InvoiceNumber

		public virtual ZString InvoiceNumber
		{
			get { return PopulateStrategy.InvoiceNumber; }
		}

		#endregion

		#region IsAuthorizedForDispatch

		public ZBool IsAuthorizedForDispatch => IsAuthorizedForDispatchCore;

		protected virtual ZBool IsAuthorizedForDispatchCore => ZBool.False;

		#endregion

		#region IsPickByBiggestEnabled

		public bool IsPickByBiggestEnabled
		{
			get { return IsPickByBiggestEnabledCore; }
		}

		protected virtual bool IsPickByBiggestEnabledCore
		{
			get
			{
				var branchPK = Guid.Empty;
				var companyPK = Guid.Empty;
				var warehouse = Warehouse;
				if (warehouse != null)
				{
					var actualWarehouse = (WhsWarehouse)warehouse.WrappedObject;
					if (actualWarehouse != null && actualWarehouse.WW_GB_RelatedCompanyBranch.IsValid)
					{
						branchPK = actualWarehouse.WW_GB_RelatedCompanyBranch.ToGuid();
						companyPK = actualWarehouse.RelatedCompanyBranch.GB_GC.ToGuid();
					}
				}
				return WarehouseDataRegistry.Instance.PickByBiggestType.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
			}
		}

		#endregion

		#region IsSplit

		public ZBool IsSplit => IsSplitCore;

		protected virtual ZBool IsSplitCore => ZBool.False;

		#endregion

		#region JobType

		public ZString JobType
		{
			get { return JobTypeCore; }
		}

		protected virtual ZString JobTypeCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region JobClient

		public virtual OrganisationWrapper JobClient
		{
			get { return null; }
		}

		#endregion

		#region JobNumber

		public ZString JobNumber
		{
			get { return JobNumberCore; }
		}

		protected virtual ZString JobNumberCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region JobNumberHeading

		public ZString JobNumberHeading
		{
			get { return JobNumberHeadingCore; }
		}

		protected virtual ZString JobNumberHeadingCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PickingInstructions

		public virtual LabelValuePairWrapper PickingInstructions { get { return LabelValuePairWrapper.Empty; } }

		#endregion

		#region PickMethod

		public virtual LabelValuePairWrapper PickMethod
		{
			get { return PopulateStrategy.PickMethod; }
		}

		#endregion

		#region PickNo

		public virtual LabelValuePairWrapper PickNo
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region PickNumberReference

		public virtual LabelValuePairWrapper PickNumberReference
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region PickOption

		public virtual CodeAndDescriptionWrapper PickOption
		{
			get { return null; }
		}

		#endregion

		#region PrimaryBarcode

		public virtual LabelValuePairWrapper PrimaryBarcode
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region PrimaryBarcodeText

		public virtual ZString PrimaryBarcodeText
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region CustomerReferenceBarcode

		public ZString CustomerReferenceBarcode => CustomerReferenceBarcodeCore;

		protected virtual ZString CustomerReferenceBarcodeCore => ZString.Empty;

		#endregion

		#region ProductLinesCount

		public ZInt ProductLinesCount
		{
			get { return ProductLinesCountCore; }
		}

		protected virtual ZInt ProductLinesCountCore
		{
			get { return 0; }
		}

		#endregion

		#region MasterBillHeading

		public ZString MasterBillHeading
		{
			get { return MasterBillHeadingCore; }
		}

		protected virtual ZString MasterBillHeadingCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region MasterBill

		public ZString MasterBill
		{
			get { return MasterBillCore; }
		}

		protected virtual ZString MasterBillCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region HouseBillHeading

		public ZString HouseBillHeading
		{
			get { return HouseBillHeadingCore; }
		}

		protected virtual ZString HouseBillHeadingCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region HouseBill

		public ZString HouseBill
		{
			get { return HouseBillCore; }
		}

		protected virtual ZString HouseBillCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region DepartmentName

		public ZString DepartmentName
		{
			get { return DepartmentNameCore; }
		}

		protected virtual ZString DepartmentNameCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region DepartmentNumber

		public ZString DepartmentNumber
		{
			get { return DepartmentNumberCore; }
		}

		protected virtual ZString DepartmentNumberCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region OrderTypeCodeFirst2Characters

		public ZString OrderTypeCodeFirst2Characters
		{
			get { return OrderTypeCodeFirst2CharactersCore; }
		}

		protected virtual ZString OrderTypeCodeFirst2CharactersCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region OrderTypeCodeLast4Characters

		public ZString OrderTypeCodeLast4Characters
		{
			get { return OrderTypeCodeLast4CharactersCore; }
		}

		protected virtual ZString OrderTypeCodeLast4CharactersCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region EventTypeCode

		public ZString EventTypeCode
		{
			get { return EventTypeCodeCore; }
		}

		protected virtual ZString EventTypeCodeCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region OtherReferences

		public ZString OtherReferences
		{
			get { return OtherReferencesCore; }
		}

		protected virtual ZString OtherReferencesCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region References

		public ZString References
		{
			get { return ReferencesCore; }
		}

		protected virtual ZString ReferencesCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ReferencesExtended

		public ZString ReferencesExtended
		{
			get { return ReferencesExtendedCore; }
		}

		protected virtual ZString ReferencesExtendedCore
		{
			get { return ""; }
		}

		#endregion

		#region RequiredDate

		public virtual LabelValuePairWrapper RequiredDate
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region ReceiveDriverName

		public ZString ReceiveDriverName => ReceiveDriverNameCore;

		protected virtual ZString ReceiveDriverNameCore => ZString.Empty;

		#endregion

		#region ReceiveDriverSignature

		public Image ReceiveDriverSignature => ReceiveDriverSignatureCore;

		protected virtual Image ReceiveDriverSignatureCore => null;

		#endregion

		#region Seal

		public ZString Seal
		{
			get { return SealCore; }
		}

		protected virtual ZString SealCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SecondaryHeading

		public ZString SecondaryHeading
		{
			get { return SecondaryHeadingCore; }
		}

		protected virtual ZString SecondaryHeadingCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SecondaryNumber

		public ZString SecondaryNumber
		{
			get { return SecondaryNumberCore; }
		}

		protected virtual ZString SecondaryNumberCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SecondaryReference

		public LabelValuePairWrapper SecondaryReference
		{
			get { return SecondaryReferenceCore; }
		}

		protected virtual LabelValuePairWrapper SecondaryReferenceCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region CarrierAccount

		public OrgCarrierAccountWrapper CarrierAccount
		{
			get { return CarrierAccountCore; }
		}

		protected virtual OrgCarrierAccountWrapper CarrierAccountCore
		{
			get { return null; }
		}

		#endregion

		#region CarrierServiceLevel

		public CarrierServiceLevelWrapper CarrierServiceLevel
		{
			get { return CarrierServiceLevelCore; }
		}

		protected virtual CarrierServiceLevelWrapper CarrierServiceLevelCore
		{
			get { return null; }
		}

		#endregion

		#region SalesChannel

		public CodeAndDescriptionWrapper SalesChannel => SalesChannelCore;

		protected virtual CodeAndDescriptionWrapper SalesChannelCore => null;

		#endregion

		#region ServiceLevel

		public CodeAndDescriptionWrapper ServiceLevel
		{
			get { return ServiceLevelCore; }
		}

		protected virtual CodeAndDescriptionWrapper ServiceLevelCore
		{
			get { return null; }
		}

		#endregion

		#region SplitNumber

		public LabelValuePairWrapper SplitNumber
		{
			get { return SplitNumberCore; }
		}

		protected virtual LabelValuePairWrapper SplitNumberCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region StagingAreaName

		public LabelValuePairWrapper StagingAreaName
		{
			get { return StagingAreaNameCore; }
		}

		protected virtual LabelValuePairWrapper StagingAreaNameCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region Status

		public virtual LabelValuePairWrapper Status
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region Supplier

		public OrganisationWrapper Supplier => SupplierCore;

		protected virtual OrganisationWrapper SupplierCore => null;

		#endregion

		#region Branch

		public DocBranch Branch => BranchCore;

		protected virtual DocBranch BranchCore => null;

		#endregion

		#region Forwarder

		public OrganisationWrapper Forwarder => ForwarderCore;

		protected virtual OrganisationWrapper ForwarderCore => null;

		#endregion

		#region TotalExtendedLinePrice

		public virtual MoneyWrapper TotalExtendedLinePrice
		{
			get { return null; }
		}

		#endregion

		#region TotalNumberOfLabels

		public virtual ZInt TotalNumberOfLabels
		{
			get { return PopulateStrategy.TotalNumberOfLabels; }
		}

		#endregion

		#region TotalNumberOfPackageLabels

		public virtual ZInt TotalNumberOfPackageLabels
		{
			get { return PopulateStrategy.TotalNumberOfPackageLabels; }
		}

		#endregion

		#region TransportCoAddress

		public virtual AddressWrapper TransportCoAddress
		{
			get { return null; }
		}

		#endregion

		#region TransportCompany

		public virtual OrganisationWrapper TransportCompany
		{
			get { return PopulateStrategy.TransportCompany; }
		}

		#endregion

		#region TransportReference

		public virtual LabelValuePairWrapper TransportReference
		{
			get { return PopulateStrategy.TransportReference; }
		}

		#endregion

		#region UNGDs

		public UNDGSubstanceWrapperCollection UNDGs
		{
			get { return UNDGsCore; }
		}

		protected virtual UNDGSubstanceWrapperCollection UNDGsCore
		{
			get { return null; }
		}

		#endregion

		#region VehicleNumber

		public ZString VehicleNumber
		{
			get { return VehicleNumberCore; }
		}

		protected virtual ZString VehicleNumberCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region VendorID

		public ZString VendorID
		{
			get { return VendorIDCore; }
		}

		protected virtual ZString VendorIDCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Warehouse

		public WarehouseBOWrapper Warehouse
		{
			get { return WarehouseCore; }
		}

		protected virtual WarehouseBOWrapper WarehouseCore
		{
			get { return null; }
		}

		#endregion

		#region WarehouseCartageCoordinatorName

		public virtual ZString WarehouseCartageCoordinatorName
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region WarehouseCartageCoordinatorPhone

		public virtual ZString WarehouseCartageCoordinatorPhone
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region WarehouseCompanyLogo

		public virtual Image WarehouseCompanyLogo
		{
			get { return null; }
		}

		#endregion

		#region WarehouseReference

		public ZString WarehouseReference
		{
			get { return GetWarehouseReference(); }
		}

		protected virtual ZString GetWarehouseReference()
		{
			return "";
		}

		#endregion

		#region WhoCreated

		public LabelValuePairWrapper WhoCreated
		{
			get { return WhoCreatedCore; }
		}

		protected virtual LabelValuePairWrapper WhoCreatedCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region WhoFinalised

		public LabelValuePairWrapper WhoFinalised
		{
			get { return WhoFinalisedCore; }
		}

		protected virtual LabelValuePairWrapper WhoFinalisedCore
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region LoadNumber

		public ZString LoadNumber => LoadNumberCore;

		protected virtual ZString LoadNumberCore => ZString.Empty;

		#endregion

		#region TransitWarehouse Properties

		#region ContainerType

		public ZString ContainerType => ContainerTypeCore;

		protected virtual ZString ContainerTypeCore => ZString.Empty;

		#endregion

		#region TransportMode

		public ZString TransportMode => TransportModeCore;

		protected virtual ZString TransportModeCore => ZString.Empty;

		#endregion

		#region GateInTime

		public ZDateTime GateInTime => GateInTimeCore;

		protected virtual ZDateTime GateInTimeCore => ZDateTime.Empty;

		#endregion

		#region GateOutTime

		public ZDateTime GateOutTime => GateOutTimeCore;

		protected virtual ZDateTime GateOutTimeCore => ZDateTime.Empty;

		#endregion

		#region CutoffTime

		public ZDateTime CutoffTime => CutoffTimeCore;

		protected virtual ZDateTime CutoffTimeCore => ZDateTime.Empty;

		#endregion

		#region ExpectedDispatchTime

		public ZDateTime ExpectedDispatchTime => ExpectedDispatchTimeCore;

		protected virtual ZDateTime ExpectedDispatchTimeCore => ZDateTime.Empty;

		#endregion

		#region StartTime

		public ZDateTime StartTime => StartTimeCore;

		protected virtual ZDateTime StartTimeCore => ZDateTime.Empty;

		#endregion

		#region CompleteTime

		public ZDateTime CompleteTime => CompleteTimeCore;

		protected virtual ZDateTime CompleteTimeCore => ZDateTime.Empty;

		#endregion

		#region FinalizedTime

		public ZDateTime FinalizedTime => FinalizedTimeCore;

		protected virtual ZDateTime FinalizedTimeCore => ZDateTime.Empty;

		#endregion

		#region IsAwaitingForwardingChanges

		public ZBool IsAwaitingForwardingChanges => IsAwaitingForwardingChangesCore;

		protected virtual ZBool IsAwaitingForwardingChangesCore => ZBool.False;

		#endregion

		#region IsReadyToStage

		public ZBool IsReadyToStage => IsReadyToStageCore;

		protected virtual ZBool IsReadyToStageCore => ZBool.False;

		#endregion

		#region IsSecure

		public ZBool IsSecure => IsSecureCore;

		protected virtual ZBool IsSecureCore => ZBool.False;

		#endregion

		#region NextDischargePort

		public ZString NextDischargePort => NextDischargePortCore;

		protected virtual ZString NextDischargePortCore => ZString.Empty;

		#endregion

		#region WarehouseExpectedArrivalTime

		public ZDateTime WarehouseExpectedArrivalTime => GetWarehouseExpectedArrivalTime;

		protected virtual ZDateTime GetWarehouseExpectedArrivalTime => ZDateTime.Empty;

		#endregion

		#endregion

		#endregion

		public virtual ZString PrintDGDetails
		{
			get { return ZString.Empty; }
		}

		public virtual DocDocAddress SOPConsigneeAddress { get { return null; } }

		public virtual ZString SOPConsigneeAddressLabel { get { return ZString.Empty; } }

		public virtual LabelValuePairWrapper SOPStagingAreaName
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		public virtual LabelValuePairWrapper SOPSpecialInstructions
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		public virtual LabelValuePairWrapper SOPTransportCompany { get { return LabelValuePairWrapper.Empty; } }

		public virtual LabelValuePairWrapper SOPCarrierServiceLevel { get { return LabelValuePairWrapper.Empty; } }

		public virtual LabelValuePairWrapper SOPRequiredDate { get { return LabelValuePairWrapper.Empty; } }

		public virtual LabelValuePairWrapper SOPOrderNumber { get { return LabelValuePairWrapper.Empty; } }

		public virtual LabelValuePairWrapper WarehouseName { get { return LabelValuePairWrapper.Empty; } }

		#region WarehouseNameAndAddress

		public MultilingualString WarehouseNameAndAddress => WarehouseNameAndAddressCore;

		protected virtual MultilingualString WarehouseNameAndAddressCore => (NoResString)ZString.Empty;

		#endregion

		#region WarehousePhoneAndFax

		public ZString WarehousePhoneAndFax => WarehousePhoneAndFaxCore;

		protected virtual ZString WarehousePhoneAndFaxCore => ZString.Empty;

		#endregion

		public virtual WarehousePickableDocketWrapperCollection Orders { get { return null; } }

		public virtual WarehouseBOMStagingLocationPartWrapperCollection BOMStagingAreaParts
		{
			get { return new WarehouseBOMStagingLocationPartWrapperCollection(Factory); }
		}

		public virtual LabelValuePairWrapper StagingLocationString { get { return LabelValuePairWrapper.Empty; } }

		public virtual LabelValuePairWrapper VehicleReference { get { return LabelValuePairWrapper.Empty; } }

		#region Flags

		public virtual ZBool EnableDangerousGoodsDetails
		{
			get { return ZBool.False; }
		}

		#endregion

		#region HasOversAndUnders

		public ZBool HasOversAndUnders
		{
			get { return HasOversAndUndersCore; }
		}

		protected virtual ZBool HasOversAndUndersCore
		{
			get { return false; }
		}

		#endregion

		public virtual ZBool IsCustomsTransaction
		{
			get { return ZBool.False; }
		}

		#region Collections

		#region JobLines / Use this collection only, all other collections will be replaced with override for this one.

		public WarehouseGenericWrapperCollection JobLines
		{
			get { return jobLines ?? (jobLines = GetJobLines()); }
		}

		WarehouseGenericWrapperCollection jobLines;

		protected virtual WarehouseGenericWrapperCollection GetJobLines()
		{
			return new WarehouseEmptyWrapperCollection(Factory);
		}

		#endregion

		#region Jobs Wrapper Collections

		#region Jobs

		public WarehouseJobGenericWrapperCollection Jobs
		{
			get { return jobs ?? (jobs = GetJobs()); }
		}
		WarehouseJobGenericWrapperCollection jobs;

		protected virtual WarehouseJobGenericWrapperCollection GetJobs()
		{
			return new WarehouseJobEmptyWrapperCollection(Factory);
		}

		#endregion

		#endregion

		#region Packing Lines Wrapper Collection

		public WarehousePackingSlipLineWrapperCollection PackingLines
		{
			get { return packingLines ?? (packingLines = NewWarehousePackingSlipLineWrapperCollection()); }
		}
		WarehousePackingSlipLineWrapperCollection packingLines;

		protected virtual WarehousePackingSlipLineWrapperCollection NewWarehousePackingSlipLineWrapperCollection()
		{
			return new WarehousePackingSlipLineWrapperCollection(Factory);
		}

		#endregion

		#region Order Lines Wrapper Collection

		public DocWhsOrderLineCollection OrderLines
		{
			get { return fOrderLines ?? (fOrderLines = NewWarehouseOrderLineWrapperCollection()); }
		}
		DocWhsOrderLineCollection fOrderLines;

		protected virtual DocWhsOrderLineCollection NewWarehouseOrderLineWrapperCollection()
		{
			return new DocWhsOrderLineCollection(Factory);
		}

		#endregion

		#region RolledUpLinesForOrderCopy

		public DocWhsOrderLineCollection RolledUpLinesForOrderCopy
		{
			get { return rolledUpLinesForOrderCopy ?? (rolledUpLinesForOrderCopy = NewWarehouseRolledUpLinesForOrderCopyCollection()); }
		}
		DocWhsOrderLineCollection rolledUpLinesForOrderCopy;

		protected virtual DocWhsOrderLineCollection NewWarehouseRolledUpLinesForOrderCopyCollection()
		{
			return new DocWhsOrderLineCollection(Factory);
		}

		#endregion

		#region BillOfLadingPackingLines

		public WarehousePackingSlipLineWrapperCollection BillOfLadingPackingLines => BillOfLadingPackingLinesCore;

		protected virtual WarehousePackingSlipLineWrapperCollection BillOfLadingPackingLinesCore => new WarehousePackingSlipLineWrapperCollection(Factory);

		#endregion

		#region BillOfLadingPackingLinesUS

		public WarehousePackingSlipLineWrapperCollection BillOfLadingPackingLinesUS => BillOfLadingPackingLinesUSCore;

		protected virtual WarehousePackingSlipLineWrapperCollection BillOfLadingPackingLinesUSCore => new WarehousePackingSlipLineWrapperCollection(Factory);

		#endregion

		#region PackageLabels

		public DocWhsPackageLabelCollection PackageLabels => PackageLabelsCore;

		protected virtual DocWhsPackageLabelCollection PackageLabelsCore => new DocWhsPackageLabelCollection(Factory);

		#endregion

		#region PackageLabelsForBOM

		public DocWhsPackageLabelCollection PackageLabelsForBOM => PackageLabelsForBOMCore;

		protected virtual DocWhsPackageLabelCollection PackageLabelsForBOMCore => new DocWhsPackageLabelCollection(Factory);

		#endregion

		#region  Picking Lines Wrapper Collection

		public WarehousePickingSlipLineWrapperCollection PickingLines
		{
			get { return pickingLines ?? (pickingLines = NewWarehousePickingSlipLineWrapperCollection()); }
		}
		WarehousePickingSlipLineWrapperCollection pickingLines;

		protected virtual WarehousePickingSlipLineWrapperCollection NewWarehousePickingSlipLineWrapperCollection()
		{
			return new WarehousePickingSlipLineWrapperCollection(Factory);
		}

		#endregion

		#region Periodic Invoice Lines Wrapper Collection

		public DocWhsJobChargeCollection JobChargeLines => NewWarehouseJobChargeLineWrapperCollection();

		protected virtual DocWhsJobChargeCollection NewWarehouseJobChargeLineWrapperCollection() => DocWhsJobChargeCollection.GetCollection(this, (NoResString)"Empty");

		#endregion

		#region Work Order Lines Wrapper Collection

		public WarehouseWorkOrderLineWrapperCollection WorkOrderLines
		{
			get { return NewWorkOrderLineWrapperCollection(); }
		}

		protected virtual WarehouseWorkOrderLineWrapperCollection NewWorkOrderLineWrapperCollection()
		{
			return new WarehouseWorkOrderLineWrapperCollection(Factory);
		}

		#endregion

		#region Stocktake Variance Lines Wrapper Collection

		public WarehouseStocktakeLineWrapperCollection VarianceLines
		{
			get { return varianceLines ?? (varianceLines = VarianceLinesCore); }
		}
		protected WarehouseStocktakeLineWrapperCollection varianceLines;

		protected virtual WarehouseStocktakeLineWrapperCollection VarianceLinesCore
		{
			get { return new WarehouseStocktakeLineWrapperCollection(Factory); }
		}

		#endregion

		#region DispatchLoadLists

		public WhsItemDispatchLoadListWrapperCollection DispatchLoadLists => dispatchLoadLists ?? (dispatchLoadLists = GetDispatchLoadLists());

		WhsItemDispatchLoadListWrapperCollection dispatchLoadLists;

		protected virtual WhsItemDispatchLoadListWrapperCollection GetDispatchLoadLists()
		{
			return new WhsItemDispatchLoadListWrapperCollection(Factory);
		}

		#endregion

		#region ReceiveTransportationUnits

		public WhsItemReceiveTransportationUnitWrapperCollection ReceiveTransportationUnits => receiveTransportationUnits ?? (receiveTransportationUnits = GetReceiveTransportationUnits());

		WhsItemReceiveTransportationUnitWrapperCollection receiveTransportationUnits;

		protected virtual WhsItemReceiveTransportationUnitWrapperCollection GetReceiveTransportationUnits()
		{
			return new WhsItemReceiveTransportationUnitWrapperCollection(Factory);
		}

		#endregion

		#region DispatchTransportationUnits

		public WhsItemDispatchTransportationUnitWrapperCollection DispatchTransportationUnits => dispatchTransportationUnits ?? (dispatchTransportationUnits = GetDispatchTransportationUnits());

		WhsItemDispatchTransportationUnitWrapperCollection dispatchTransportationUnits;

		protected virtual WhsItemDispatchTransportationUnitWrapperCollection GetDispatchTransportationUnits()
		{
			return new WhsItemDispatchTransportationUnitWrapperCollection(Factory);
		}

		#endregion

		#region PalletizedInventory

		public WarehouseInventoryWrapperCollection PalletizedInventory
		{
			get { return palletizedInventory ?? (palletizedInventory = GetPalletizedInventory()); }
		}

		WarehouseInventoryWrapperCollection palletizedInventory;

		protected virtual WarehouseInventoryWrapperCollection GetPalletizedInventory()
		{
			return new WarehouseInventoryWrapperCollection(Factory);
		}

		#endregion

		#region Containers

		public ContainerWrapperCollection Containers
		{
			get { return containers ?? (containers = NewWarehouseContainerLineWrapperCollection()); }
		}
		protected ContainerWrapperCollection containers;

		protected virtual ContainerWrapperCollection NewWarehouseContainerLineWrapperCollection()
		{
			return new ContainerWrapperCollection(Factory);
		}

		#endregion

		#region Service Lines

		internal ServiceWrapperCollection Services
		{
			get { return services ?? (services = NewServiceLineWrapperCollection()); }
		}
		protected ServiceWrapperCollection services;

		protected virtual ServiceWrapperCollection NewServiceLineWrapperCollection()
		{
			return new ServiceWrapperCollection((GenericWrapper)null, Factory);
		}

		#endregion

		#region Packages

		public PackageWrapperCollection Packages
		{
			get { return packages ?? (packages = GetPackages()); }
		}

		protected virtual PackageWrapperCollection GetPackages()
		{
			return new PackageWrapperCollection((PkgPackageJob)null, Factory);
		}

		protected PackageWrapperCollection packages;

		public PackageWrapperCollection LoadedPackages
		{
			get { return loadedPackages ?? (loadedPackages = GetLoadedPackages()); }
		}

		protected virtual PackageWrapperCollection GetLoadedPackages()
		{
			return new PackageWrapperCollection((PkgPackageJob)null, Factory);
		}

		protected PackageWrapperCollection loadedPackages;

		#endregion

		#region Grouped Job Lines for Variance Wrapper Collection

		public WarehouseGroupedLinesForVarianceWrapperCollection JobLinesVariances => jobLinesVariances ?? (jobLinesVariances = GetJobLinesVariances());

		protected virtual WarehouseGroupedLinesForVarianceWrapperCollection GetJobLinesVariances()
		{
			return new WarehouseGroupedLinesForVarianceWrapperCollection(Factory);
		}

		protected WarehouseGroupedLinesForVarianceWrapperCollection jobLinesVariances;

		#endregion

		#region DeliveryLabels

		public DocWhsLabelCollection DeliveryLabels
		{
			get
			{
				var result = new DocWhsLabelCollection(Factory);
				for (var i = 0; i < TotalNumberOfLabels; i++)
				{
					var label = new WhsLabel();
					label.Number = i + 1;
					result.Add(DocWhsLabel.New(label, Factory));
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region Totals

		#region OuterPackagesContents

		public ZString OuterPackagesContents
		{
			get { return GetOuterPackagesContents(); }
		}

		protected virtual ZString GetOuterPackagesContents()
		{
			return "";
		}

		#endregion

		#region TotalOuterPackagesWeight

		public WeightWrapper TotalOuterPackagesWeight
		{
			get { return totalOuterPackagesWeight ?? (totalOuterPackagesWeight = GetTotalOuterPackagesWeight()); }
		}

		WeightWrapper totalOuterPackagesWeight;

		protected virtual WeightWrapper GetTotalOuterPackagesWeight()
		{
			return WeightWrapper.Empty;
		}

		#endregion

		#region TotalOuterPackagesVolume

		public VolumeWrapper TotalOuterPackagesVolume
		{
			get { return totalOuterPackagesVolume ?? (totalOuterPackagesVolume = GetTotalOuterPackagesVolume()); }
		}

		VolumeWrapper totalOuterPackagesVolume;

		protected virtual VolumeWrapper GetTotalOuterPackagesVolume()
		{
			return VolumeWrapper.Empty;
		}

		#endregion

		#region TotalLoadedWeight

		public WeightWrapper TotalLoadedWeight => TotalLoadedWeightCore;

		protected virtual WeightWrapper TotalLoadedWeightCore => WeightWrapper.Empty;

		#endregion

		#region TotalLoadedVolume

		public VolumeWrapper TotalLoadedVolume => TotalLoadedVolumeCore;

		protected virtual VolumeWrapper TotalLoadedVolumeCore => VolumeWrapper.Empty;

		#endregion

		#region TotalLoadedUnits

		public LabelValuePairWrapper TotalLoadedUnits => TotalLoadedUnitsCore;

		protected virtual LabelValuePairWrapper TotalLoadedUnitsCore => LabelValuePairWrapper.Empty;

		#endregion

		#region TotalLoadedPackages

		public LabelValuePairWrapper TotalLoadedPackages => TotalLoadedPackagesCore;

		protected virtual LabelValuePairWrapper TotalLoadedPackagesCore => LabelValuePairWrapper.Empty;

		#endregion

		#endregion

		#region Flags

		public virtual ZBool EnableExtendedLinePrice
		{
			get { return TotalExtendedLinePrice == null ? ZBool.False : (ZBool)(TotalExtendedLinePrice.Amount > 0); }
		}

		#endregion

		#region Pickable Docket Line Info

		public virtual VolumeWrapper CubicSent
		{
			get { return null; }
		}

		public virtual ValueAndUnitWrapper PackagesSent
		{
			get { return null; }
		}

		public virtual ZShort PalletsSent
		{
			get { return ZShort.Zero; }
		}

		public virtual ZDecimal UnitsSent
		{
			get { return ZDecimal.Zero; }
		}

		public virtual WeightWrapper WeightSent
		{
			get { return null; }
		}

		#endregion

		#region WorkOrderLevels

		public ZString WorkOrderLevels1st
		{
			get { return WorkOrderLevels1stCore; }
		}

		protected virtual ZString WorkOrderLevels1stCore
		{
			get { return ZString.Empty; }
		}

		public ZString WorkOrderLevels2nd
		{
			get { return WorkOrderLevels2ndCore; }
		}

		protected virtual ZString WorkOrderLevels2ndCore
		{
			get { return ZString.Empty; }
		}

		public ZString WorkOrderLevels3rd
		{
			get { return WorkOrderLevels3rdCore; }
		}

		protected virtual ZString WorkOrderLevels3rdCore
		{
			get { return ZString.Empty; }
		}

		public ZString WorkOrderLevels4th
		{
			get { return WorkOrderLevels4thCore; }
		}

		protected virtual ZString WorkOrderLevels4thCore
		{
			get { return ZString.Empty; }
		}

		public ZString WorkOrderLevels5th
		{
			get { return WorkOrderLevels5thCore; }
		}

		protected virtual ZString WorkOrderLevels5thCore
		{
			get { return ZString.Empty; }
		}

		public ZString WorkOrderLevels6th
		{
			get { return WorkOrderLevels6thCore; }
		}

		protected virtual ZString WorkOrderLevels6thCore
		{
			get { return ZString.Empty; }
		}

		public ZString WorkOrderLevels7th
		{
			get { return WorkOrderLevels7thCore; }
		}

		protected virtual ZString WorkOrderLevels7thCore
		{
			get { return ZString.Empty; }
		}

		public ZString WorkOrderLevels8th
		{
			get { return WorkOrderLevels8thCore; }
		}

		protected virtual ZString WorkOrderLevels8thCore
		{
			get { return ZString.Empty; }
		}

		public ZString WorkOrderLevels9th
		{
			get { return WorkOrderLevels9thCore; }
		}

		protected virtual ZString WorkOrderLevels9thCore
		{
			get { return ZString.Empty; }
		}

		public ZString WorkOrderLevels10th
		{
			get { return WorkOrderLevels10thCore; }
		}

		protected virtual ZString WorkOrderLevels10thCore
		{
			get { return ZString.Empty; }
		}

		protected virtual ZString BomLevel
		{
			get { return ZString.Empty; }
		}

		protected virtual ZString Attributes
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SubTypeDesc

		public ZString SubTypeDesc
		{
			get { return SubTypeDescCore; }
		}

		protected virtual ZString SubTypeDescCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		public virtual ZString CP_IssueNo
		{
			get { return ZString.Empty; }
		}

		public virtual ZString ACSEstCode
		{
			get { return ZString.Empty; }
		}

		public virtual ZString ATOEstCode
		{
			get { return ZString.Empty; }
		}

		#region ClientCPC

		public ZString ClientCPC => ClientCPCCore;

		protected virtual ZString ClientCPCCore => ZString.Empty;

		#endregion

		#region CurrencySymbol

		public ZString CurrencySymbol => CurrencySymbolCore;

		protected virtual ZString CurrencySymbolCore => ZString.Empty;

		#endregion

		#region DocketStatus

		public ZString DocketStatus => DocketStatusCore;

		protected virtual ZString DocketStatusCore => ZString.Empty;

		#endregion

		#region DocketSubType

		public ZString DocketSubType => DocketSubTypeCore;

		protected virtual ZString DocketSubTypeCore => ZString.Empty;

		#endregion

		#region DocketType

		public ZString DocketType => DocketTypeCore;

		protected virtual ZString DocketTypeCore => ZString.Empty;

		#endregion

		#region EmergencyContactMessageString

		public ZString EmergencyContactMessageString => EmergencyContactMessageStringCore;

		protected virtual ZString EmergencyContactMessageStringCore => ZString.Empty;

		#endregion

		#region PrintPageWithContainerNumber

		public ZBool PrintPageWithContainerNumber => PrintPageWithContainerNumberCore;

		protected virtual ZBool PrintPageWithContainerNumberCore => ZBool.False;

		#endregion

		#region TotalExtendedLinePriceWithSymbol

		public ZString TotalExtendedLinePriceWithSymbol => TotalExtendedLinePriceWithSymbolCore;

		protected virtual ZString TotalExtendedLinePriceWithSymbolCore => ZString.Empty;

		#endregion

		#region TotalGroupedLineUnitsMet

		public ZDecimal TotalGroupedLineUnitsMet => TotalGroupedLineUnitsMetCore;

		protected virtual ZDecimal TotalGroupedLineUnitsMetCore => ZDecimal.Zero;

		#endregion

		#region TotalCubic

		public ZDecimal TotalCubic => TotalCubicCore;

		protected virtual ZDecimal TotalCubicCore => ZDecimal.Zero;

		#endregion

		#region TotalTopLevelUnitsMet

		public ZDecimal TotalTopLevelUnitsMet => TotalTopLevelUnitsMetCore;

		protected virtual ZDecimal TotalTopLevelUnitsMetCore => ZDecimal.Zero;

		#endregion

		#region TotalTopLevelUnitsOrdered

		public ZDecimal TotalTopLevelUnitsOrdered => TotalTopLevelUnitsOrderedCore;

		protected virtual ZDecimal TotalTopLevelUnitsOrderedCore => ZDecimal.Zero;

		#endregion

		#region TotalUnits

		public ZDecimal TotalUnits => TotalUnitsCore;

		protected virtual ZDecimal TotalUnitsCore => ZDecimal.Zero;

		#endregion

		#region TotalWeight

		public ZDecimal TotalWeight => TotalWeightCore;

		protected virtual ZDecimal TotalWeightCore => ZDecimal.Zero;

		#endregion

		#region TotalPallets

		public ZShort TotalPallets => TotalPalletsCore;

		protected virtual ZShort TotalPalletsCore => ZShort.Zero;

		#endregion

		#region TotalInnerPackLines

		public ZShort TotalInnerPackLines => TotalInnerPackLinesCore;

		protected virtual ZShort TotalInnerPackLinesCore => ZShort.Zero;

		#endregion

		#region TotalInners

		public ZShort TotalInners => TotalInnersCore;

		protected virtual ZShort TotalInnersCore => ZShort.Zero;

		#endregion

		#region TotalInnerPackLines

		public ZShort TotalOverpacks => TotalOverpacksCore;

		protected virtual ZShort TotalOverpacksCore => ZShort.Zero;

		#endregion

		#region WarehouseCCPCode

		public ZString WarehouseCCPCode => WarehouseCCPCodeCore;

		protected virtual ZString WarehouseCCPCodeCore => ZString.Empty;

		#endregion

		#region PackingSlipTitle

		public ZString PackingSlipTitle => PackingSlipTitleCore;

		protected virtual ZString PackingSlipTitleCore => ZString.Empty;

		#endregion

		public virtual ZString ClientGCR
		{
			get { return ZString.Empty; }
		}

		public virtual ZString ABN
		{
			get { return ZString.Empty; }
		}

		#region Stocktake Fields

		#region StocktakeNumber

		public ZString StocktakeNumber
		{
			get { return StocktakeNumberCore; }
		}

		protected virtual ZString StocktakeNumberCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SelectedCycle

		public ZString SelectedCycle
		{
			get { return SelectedCycleCore; }
		}

		protected virtual ZString SelectedCycleCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SelectedClient

		public ZString SelectedClient
		{
			get { return SelectedClientCore; }
		}

		protected virtual ZString SelectedClientCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SelectedSupplierPart

		public ZString SelectedSupplierPart
		{
			get { return SelectedSupplierPartCore; }
		}

		protected virtual ZString SelectedSupplierPartCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SelectedCycle

		public ZString SelectedCommodityCode
		{
			get { return SelectedCommodityCodeCore; }
		}

		protected virtual ZString SelectedCommodityCodeCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SelectedPickMethod

		public ZString SelectedPickMethod
		{
			get { return SelectedPickMethodCore; }
		}

		protected virtual ZString SelectedPickMethodCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SelectedCycle

		public ZString SelectedRow
		{
			get { return SelectedRowCore; }
		}

		protected virtual ZString SelectedRowCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SelectedArea

		public MultilingualString SelectedArea
		{
			get { return SelectedAreaCore; }
		}

		protected virtual MultilingualString SelectedAreaCore
		{
			get { return (NoResString)ZString.Empty; }
		}

		#endregion

		#region SelectedABCCategory

		public ZString SelectedABCCategory
		{
			get { return SelectedABCCategoryCore; }
		}

		protected virtual ZString SelectedABCCategoryCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SelectedLocation

		public ZString SelectedLocation
		{
			get { return SelectedLocationCore; }
		}

		protected virtual ZString SelectedLocationCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SelectedStocktakeType

		public ZString SelectedStocktakeType
		{
			get { return SelectedStocktakeTypeCore; }
		}

		protected virtual ZString SelectedStocktakeTypeCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#endregion

		public virtual ZString DebtorCodeAndName
		{
			get { return ZString.Empty; }
		}

		public virtual ZString AccountCode
		{
			get { return ZString.Empty; }
		}

		public virtual ZString CurrencyCode
		{
			get { return ZString.Empty; }
		}

		public virtual MultilingualString ReportDescription
		{
			get { return (NoResString)ZString.Empty; }
		}

		public virtual ZDateTime FromDate
		{
			get { return ZDateTime.Empty; }
		}

		public virtual ZDateTime ToDate
		{
			get { return ZDateTime.Empty; }
		}

		public virtual ZBool IsWorkOrder
		{
			get { return ZBool.False; }
		}

		public virtual ZBool IsWorkOrderPick
		{
			get { return ZBool.False; }
		}

		public virtual ZBool IsAuthorisedToLeave => ZBool.False;

		public virtual ZString AssignedLoader => ZString.Empty;

		public virtual ZBool AllDCNsAreAuthorized => ZBool.False;

		#region Implementation

		#region IDocTypeCode

		ZString IDocTypeCode.DocTypeCode
		{
			get { return DocTypeCode; }
			set { DocTypeCode = value; }
		}
		protected ZString DocTypeCode;

		#endregion

		// todo - once PopulateStrategy is fully implemented, we can remove the "?? (populateStrategy = WhsPopulateStrategy.NewPopulateStrategy(WrappedBO))" bit.
		internal WhsPopulateStrategy PopulateStrategy
		{
			get { return populateStrategy ?? (populateStrategy = WhsPopulateStrategy.NewPopulateStrategy(WrappedBO)); }
		}
		WhsPopulateStrategy populateStrategy;

		#endregion

		#region IsFinalised

		public ZBool IsFinalised => IsFinalisedCore;

		protected virtual ZBool IsFinalisedCore => ZBool.False;

		#endregion

	}
}

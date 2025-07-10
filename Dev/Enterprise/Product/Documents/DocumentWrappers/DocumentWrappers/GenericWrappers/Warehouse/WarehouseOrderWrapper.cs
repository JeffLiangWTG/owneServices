using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseOrderWrapper : WarehousePickableDocketWrapper
	{
		public WarehouseOrderWrapper(WhsOrder order, BusinessObjectFactory factoryToWrap)
			: base(order, factoryToWrap)
		{
		}

		#region Properties

		#region Addresses

		#region GoodsBilledToAddress

		protected override AddressWrapper GoodsBillToAddressCore => Order != null ? goodsBillToAddress ?? (goodsBillToAddress = new AddressWrapper(Order.GoodsBillToDocAddress, Factory)) : null;
		AddressWrapper goodsBillToAddress;

		#endregion

		#region PickUpAddress

		protected override AddressWrapper PickUpAddressCore => Order != null ? pickUpAddress ?? (pickUpAddress = new AddressWrapper(Order.PickUpDocAddress, Factory)) : null;
		AddressWrapper pickUpAddress;

		#endregion

		#region DropOffAddress

		protected override AddressWrapper DropOffAddressCore => Order != null ? dropOffAddress ?? (dropOffAddress = new AddressWrapper(Order.DropOffDocAddress, Factory)) : null;
		AddressWrapper dropOffAddress;

		#endregion

		#region SupplierDocAddress

		protected override AddressWrapper SupplierDocAddressCore => Order != null ? supplierDocAddress ?? (supplierDocAddress = new AddressWrapper(Order.SupplierDocAddress, Factory)) : null;
		AddressWrapper supplierDocAddress;

		#endregion

		#region TransportBillToAddress

		public override AddressWrapper TransportBillToAddress => Order != null ? transportBillToAddress ?? (transportBillToAddress = new AddressWrapper(Order.TransportBillToDocAddress, Factory)) : null;
		AddressWrapper transportBillToAddress;

		#endregion

		#region DistributionCenterAddress

		public override AddressWrapper DistributionCentreAddress => Order != null ? distributionCentreAddress ?? (distributionCentreAddress = new AddressWrapper(Order.DistributionCentreDocAddress, Factory)) : null;
		AddressWrapper distributionCentreAddress;

		#endregion

		#endregion

		protected override CarrierServiceLevelWrapper CarrierServiceLevelCore => Order == null || Order.CarrierServiceLevel == null ? base.CarrierServiceLevelCore : new CarrierServiceLevelWrapper(Order.CarrierServiceLevel, Factory);

		#region DepartmentName

		protected override ZString DepartmentNameCore => GetReferenceValue(WarehouseAdditionalReferenceTypes.Codes.DepartmentNameCode);

		#endregion

		#region DepartmentNumber

		protected override ZString DepartmentNumberCore => GetReferenceValue(WarehouseAdditionalReferenceTypes.Codes.DepartmentNumberCode);

		#endregion

		#region DockDoor

		protected override ZString DockDoorCore => Order?.Pick?.DockDoorLocation?.WLV_LocationString ?? ZString.Empty;

		#endregion

		#region OrderTypeCodeFirst2CharactersCore

		protected override ZString OrderTypeCodeFirst2CharactersCore => GetReferenceValue(WarehouseAdditionalReferenceTypes.Codes.OrderTypeCode).SubstringSafe(0, 2);

		#endregion

		#region OrderTypeCodeLast4CharactersCore

		protected override ZString OrderTypeCodeLast4CharactersCore => GetReferenceValue(WarehouseAdditionalReferenceTypes.Codes.OrderTypeCode).SubstringSafe(2, 4);

		#endregion

		#region EventTypeCode

		protected override ZString EventTypeCodeCore => GetReferenceValue(WarehouseAdditionalReferenceTypes.Codes.EventTypeCode);

		#endregion

		#region SecondaryReference

		protected override LabelValuePairWrapper SecondaryReferenceCore
		{
			get
			{
				return (Order != null)
				? new LabelValuePairWrapper(Res.GetString("ec200ea3-3156-454f-a902-4b7adc6205c9", "Order Number"), Order.WD_ExternalReference, Factory)
				: LabelValuePairWrapper.Empty;
			}
		}

		#endregion

		#region JobCharge

		protected override DocWhsJobChargeCollection NewWarehouseJobChargeLineWrapperCollection() =>
			Order?.JobHeader is Job orderJob
			? DocWhsJobChargeCollection.GetCollection(this, nameof(NewWarehouseJobChargeLineWrapperCollection), orderJob.Charges.Cast<JobCharge>().ToArray())
			: base.NewWarehouseJobChargeLineWrapperCollection();

		#endregion

		#region JobType

		protected override ZString JobTypeCore
		{
			get
			{
				var jobTypeCode = base.JobTypeCore;
				if (Order.IsDomesticFreight)
				{
					jobTypeCode = "DOM";
				}

				return jobTypeCode;
			}
		}

		#endregion

		#region Totals

		#region TotalOuterPackagesWeight

		protected override WeightWrapper GetTotalOuterPackagesWeight()
		{
			var packageJob = Order?.PackageJob;
			return packageJob != null ? new WeightWrapper(packageJob.Weight, packageJob.WeightUQ, 2, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), Factory) : WeightWrapper.Empty;
		}

		#endregion

		#region TotalOuterPackagesVolume

		protected override VolumeWrapper GetTotalOuterPackagesVolume()
		{
			var packageJob = Order?.PackageJob;
			return packageJob != null ? new VolumeWrapper(packageJob.Volume, packageJob.VolumeUQ, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume), Factory) : VolumeWrapper.Empty;
		}

		#endregion

		#region OuterPackagesContents

		protected override ZString GetOuterPackagesContents()
		{
			return Order?.PackageJob?.Packages.ToStringSummary(10) ?? ZString.Empty;
		}

		#endregion

		#endregion

		#region CarrierAccount

		protected override OrgCarrierAccountWrapper CarrierAccountCore
		{
			get
			{
				OrgCarrierAccountWrapper result = null;

				var client = Order.Client;
				if (client != null)
				{
					var carrierAccount = OrgWhsClientAccountAssociation.GetCarrierAccountNumber(Order.GetTransportCo(), client, Order.Warehouse, Order.SalesChannel);
					if (carrierAccount != null)
					{
						result = new OrgCarrierAccountWrapper(carrierAccount, Factory);
					}
				}

				return result;
			}
		}

		#endregion

		#region SalesChannel

		protected override CodeAndDescriptionWrapper SalesChannelCore
		{
			get
			{
				var salesChannel = Order?.SalesChannel;
				return salesChannel != null ? new CodeAndDescriptionWrapper(salesChannel.WSH_Code, salesChannel.WSH_Description, Factory) : null;
			}
		}

		#endregion

		#region SupplierBuyerLink

		public override SupplierBuyerLinkWrapper SupplierBuyerLink
		{
			get
			{
				var order = Order;
				if (order != null && supplierBuyerLink == null)
				{
					supplierBuyerLink = new SupplierBuyerLinkWrapper(order.SupplierBuyerLink, Factory);
				}
				return supplierBuyerLink;
			}
		}
		SupplierBuyerLinkWrapper supplierBuyerLink;

		#endregion

		#region TransportZone

		protected override ZString TransportZoneCore
		{
			get { return Order?.TransportZone?.TZ_ZoneName ?? ZString.Empty; }
		}

		#endregion

		#region IsAuthorisedToLeave

		public override ZBool IsAuthorisedToLeave => PackingDocketBO?.WD_IsAuthorisedToLeave ?? false;

		#endregion

		#region PickingInstructions

		public override LabelValuePairWrapper PickingInstructions
		{
			get
			{
				ZString result = ZString.Empty;
				if (!Order?.PickingInstructions.IsEmpty ?? false)
				{
					result = Res.GetString("af809e0b-591a-494b-a6aa-56ac5245f66b", "Order {0} - {1}", Order.WD_ExternalReference, Order.PickingInstructions);
				}
				return new LabelValuePairWrapper(Res.GetString("a132975f-ce90-47fb-bc9c-3d28e17423c1", "Picking Inst."), result, Factory);
			}
		}

		#endregion

		protected override ZString PackingSlipTitleCore => Order.Warehouse?.PackingSlipTitle ?? Res.GetString("ba236f03-7f96-4f77-b7a3-1515a7c53a9d", "Packing Slip");

		#region LoadNumber

		protected override ZString LoadNumberCore => GetLoadNumber();

		#endregion

		#endregion

		#region Implementation

		protected WhsOrder Order => (WhsOrder)WrappedObject;

		#endregion
	}
}

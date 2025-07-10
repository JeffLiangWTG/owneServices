using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaTransferHeader : ManifestBase.AsycudaTransferHeader
	{
		public AsycudaTransferHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ResourceStringData("ASYCUDA.Business.AsycudaTransferHeader|ATF_OnwardCarrier", Caption = "Onward Carrier")]
		[List(nameof(Lookups) + "." + nameof(AsycudaTransferHeaderLookups.CarrierCollection))]
		public override ZString ATF_OnwardCarrier
		{
			get => base.ATF_OnwardCarrier;
			set => base.ATF_OnwardCarrier = value;
		}

		[ResourceStringData("ASYCUDA.Business.AsycudaTransferHeader|ATF_RL_NKDestinationPortCode", Caption = "Destination Airport")]
		public override ZString ATF_RL_NKDestinationPortCode
		{
			get => base.ATF_RL_NKDestinationPortCode;
			set => base.ATF_RL_NKDestinationPortCode = value;
		}

		[ResourceStringData("ASYCUDA.Business.AsycudaTransferHeader|ATF_TransferType", Caption = "Transfer Type")]
		public override ZString ATF_TransferType
		{
			get => base.ATF_TransferType;
			set => base.ATF_TransferType = value;
		}

		#region Carrier

		[ResourceStringData("ASYCUDA.Business.AsycudaTransferHeader|ATF_OA_Carrier", Caption = "In-Bond Carrier Address", ShortCaption = "In-Bond Carrier")]
		public override ZGuid ATF_OA_Carrier
		{
			get { return base.ATF_OA_Carrier; }
			set => base.ATF_OA_Carrier = value;
		}

		[ResourceStringData("ASYCUDA.Business.AsycudaTransferHeader|ATF_CarrierID", Caption = "In-Bond Carrier ID")]
		public override ZString ATF_CarrierID
		{
			get => base.ATF_CarrierID;
			set => base.ATF_CarrierID = value;
		}

		[ResourceStringData("ASYCUDA.Business.AsycudaTransferHeader|InBondCarrierOrgPK", Caption = "In-Bond Carrier")]
		[List(nameof(Lookups) + "." + nameof(AsycudaTransferHeaderLookups.ShippingProviders))]
		public ZGuid InBondCarrierOrgPK
		{
			get { return ATF_OA_Carrier_ZAddress.OrgPK; }
			set
			{
				ATF_OA_Carrier_ZAddress.OrgPK = value;
				ATF_OA_CarrierInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo InBondCarrierOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(InBondCarrierOrgPK), x => ATF_OA_Carrier_ZAddress.OrgPKInfo); }
		}

		public OrgHeader InBondCarrierOrg
		{
			get { return (OrgHeader)ATF_OA_Carrier_ZAddress.OrgHeader; }
		}

		#endregion

		#region DestinationWarehouse

		[ResourceStringData("ASYCUDA.Business.AsycudaTransferHeader|ATF_OA_DestinationWarehouse", Caption = "Bonded Premises Address", ShortCaption = "Bonded Premises")]
		public override ZGuid ATF_OA_DestinationWarehouse
		{
			get { return base.ATF_OA_DestinationWarehouse; }
			set => base.ATF_OA_DestinationWarehouse = value;
		}

		[ResourceStringData("ASYCUDA.Business.AsycudaTransferHeader|ATF_DestinationWarehouseID", Caption = "Bonded Premises ID")]
		public override ZString ATF_DestinationWarehouseID
		{
			get => base.ATF_DestinationWarehouseID;
			set => base.ATF_DestinationWarehouseID = value;
		}

		[ResourceStringData("ASYCUDA.Business.AsycudaTransferHeader|DestinationWarehouseOrgPK", Caption = "Bonded Premises")]
		[List(nameof(Lookups) + "." + nameof(AsycudaTransferHeaderLookups.BondedWarehouseCollection))]
		public ZGuid DestinationWarehouseOrgPK
		{
			get { return ATF_OA_DestinationWarehouse_ZAddress.OrgPK; }
			set
			{
				ATF_OA_DestinationWarehouse_ZAddress.OrgPK = value;
				ATF_OA_DestinationWarehouseInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DestinationWarehouseOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(DestinationWarehouseOrgPK), x => ATF_OA_DestinationWarehouse_ZAddress.OrgPKInfo); }
		}

		public OrgHeader DestinationWarehouseOrg
		{
			get { return (OrgHeader)ATF_OA_DestinationWarehouse_ZAddress.OrgHeader; }
		}

		#endregion

		public new AsycudaArrivalHeader ArrivalHeader => (AsycudaArrivalHeader)base.ArrivalHeader;

		public new IBusinessObjectCollection<AsycudaTransferBill> TransferBills => (IBusinessObjectCollection<AsycudaTransferBill>)base.TransferBills;
		protected override ManifestBase.IAsycudaTransferBillCollection<ManifestBase.AsycudaTransferBill> CreateNewAsycudaTransferBillCollection() => new ManifestBase.AsycudaTransferBillCollection<AsycudaTransferBill>(this);

		public new AsycudaTransferHeaderLookups Lookups => (AsycudaTransferHeaderLookups)base.Lookups;
		protected override ManifestBase.AsycudaTransferHeaderLookups GetNewLookups() => new AsycudaTransferHeaderLookups(this);
		public new AsycudaTransferHeaderValidation Validation => (AsycudaTransferHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaTransferHeaderValidation GetNewValidation() => new AsycudaTransferHeaderValidation(this);
	}
}

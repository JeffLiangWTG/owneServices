using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaBillDocWrapper : DocumentWrapper
	{
		public AsycudaBillDocWrapper(AsycudaBill bill)
			: base(bill, bill.Factory)
		{
			this.bill = bill;
		}
		readonly AsycudaBill bill;

		public ZString Origin => bill.ABL_RL_NKOrigin;

		public ZString BillNumber => bill.ABL_BillNumber;

		public ZString GoodsDescription => bill.ABL_GoodsDescription;

		public ZString Destination => bill.ABL_RL_NKFinalDestination;

		public ZString Marks => bill.ABL_MarksAndNumbers;

		public ZString Weight => string.Join("\t", bill.ABL_GrossWeight.ToString(), bill.ABL_GrossWeightUQ);

		public ZString Volume => string.Join("\t", bill.ABL_Volume.ToString(), bill.ABL_VolumeUQ);

		public ZString Quantity => string.Join("\t", bill.ABL_ManifestQty.ToString(), bill.ABL_ManifestUQ);

		public ZGuid ConsignorPK => bill.ABL_OA_Shipper;

		public ZGuid ConsigneePK => bill.ABL_OA_Consignee;

		public ZBool HasBillScreenings => !bill.BillScreenings.IsNullOrEmpty();

		public ZBool HasAdditionalSupplyChainActor => !bill.CusSupplyChainActorReferences.Cast<CusSupplyChainActorReference>().IsNullOrEmpty();

		public AsycudaBillScreeningDocWrapper AsycudaBillScreeningDocWrapper => HasBillScreenings ? new AsycudaBillScreeningDocWrapper(bill.BillScreenings.FirstOrDefault()) : null;
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(AsycudaArrivalHeader), "TransferHeaders")]
	public class AsycudaTransferHeader : AutoAsycudaTransferHeader
		, Integration.Customs.ManifestBase.IAsycudaTransferHeader
		, IClusterKeyWorker
	{
		public AsycudaTransferHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[List("Lookups.TransferTypeList")]
		public override ZString ATF_TransferType { get => base.ATF_TransferType; set => base.ATF_TransferType = value; }

		public AsycudaArrivalHeader ArrivalHeader => Factory.Load<AsycudaArrivalHeader>(ATF_ATH_ArrivalHeader);

		[RelatedBusinessObject(nameof(ArrivalHeader))]
		public override ZGuid ATF_ATH_ArrivalHeader
		{
			get { return base.ATF_ATH_ArrivalHeader; }
			set { base.ATF_ATH_ArrivalHeader = value; }
		}

		public override void Delete()
		{
			base.Delete();
			transferBills?.RemoveAndDeleteAll();
		}

		[ChildEditable]
		public IBusinessObjectCollection<AsycudaTransferBill> TransferBills
		{
			get
			{
				if (transferBills == null)
				{
					transferBills = CreateNewAsycudaTransferBillCollection();
					transferBills.Load();
					RegisterEditableChildObject(transferBills);
				}
				return transferBills;
			}
		}
		IBusinessObjectCollection<AsycudaTransferBill> transferBills;

		protected virtual IAsycudaTransferBillCollection<AsycudaTransferBill> CreateNewAsycudaTransferBillCollection() => new AsycudaTransferBillCollection<AsycudaTransferBill>(this);

		#region IClusterKeyWorker

		Type IClusterKeyWorker.ParentBizObjType => typeof(AsycudaArrivalHeader);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)ATF_ATH_ArrivalHeaderInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(AsycudaTransferBill), AsycudaTransferBillSchema.ATB_ATF_TransferHeader);
			}
		}

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)ATF_ClusterKeyInfo;

		#endregion
	}
}

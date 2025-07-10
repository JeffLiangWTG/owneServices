using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.ManifestBase
{
	[SingleObjectAroundARow]
	[DependentBusinessObject(typeof(AsycudaTransferHeader), "TransferBills")]
	public class AsycudaTransferBill : AutoAsycudaTransferBill
		, Integration.Customs.ManifestBase.IAsycudaTransferBill
		, IClusterKeyWorker
	{
		public AsycudaTransferBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public AsycudaTransferHeader TransferHeader => Factory.Load<AsycudaTransferHeader>(ATB_ATF_TransferHeader);

		[RelatedBusinessObject(nameof(TransferHeader))]
		public override ZGuid ATB_ATF_TransferHeader
		{
			get { return base.ATB_ATF_TransferHeader; }
			set { base.ATB_ATF_TransferHeader = value; }
		}

		#region IClusterKeyWorker

		Type IClusterKeyWorker.ParentBizObjType => typeof(AsycudaTransferHeader);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)ATB_ATF_TransferHeaderInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)ATB_ClusterKeyInfo;

		#endregion
	}
}

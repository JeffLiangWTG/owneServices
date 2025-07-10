using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	[SingleObjectAroundARow]
	public class AsycudaArrivalHeader : AutoAsycudaArrivalHeader
		, Integration.Customs.ManifestBase.IAsycudaArrivalHeader
		, IClusterKeyWorker
	{
		public AsycudaArrivalHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly AsycudaArrivalHeaderTypeDecider TypeDecider = new AsycudaArrivalHeaderTypeDecider();

		public AsycudaManifestHeader Header => Factory.Load<AsycudaManifestHeader>(ATH_AMA_ManifestHeader);

		[RelatedBusinessObject(nameof(Header))]
		public override ZGuid ATH_AMA_ManifestHeader
		{
			get { return base.ATH_AMA_ManifestHeader; }
			set { base.ATH_AMA_ManifestHeader = value; }
		}

		public override void Delete()
		{
			base.Delete();
			transferHeaders?.RemoveAndDeleteAll();
		}

		[ChildEditable]
		public IBusinessObjectCollection<AsycudaTransferHeader> TransferHeaders
		{
			get
			{
				if (transferHeaders == null)
				{
					transferHeaders = CreateNewAsycudaTransferHeaderCollection();
					transferHeaders.Load();
					RegisterEditableChildObject(transferHeaders);
				}
				return transferHeaders;
			}
		}
		IBusinessObjectCollection<AsycudaTransferHeader> transferHeaders;

		protected virtual IAsycudaTransferHeaderCollection<AsycudaTransferHeader> CreateNewAsycudaTransferHeaderCollection() => new AsycudaTransferHeaderCollection<AsycudaTransferHeader>(this);

		#region IClusterKeyWorker

		Type IClusterKeyWorker.ParentBizObjType => typeof(AsycudaManifestHeader);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)ATH_AMA_ManifestHeaderInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(AsycudaArrivalLine), AsycudaArrivalLineSchema.ATL_ATH);
				yield return new ClusterKeyChildInfo(typeof(AsycudaTransferHeader), AsycudaTransferHeaderSchema.ATF_ATH_ArrivalHeader);
			}
		}

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)ATH_ClusterKeyInfo;

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ManifestBase
{
	[SingleObjectAroundARow]
	[RowFetchStrategy(FetchStrategyType = typeof(AsycudaContainerRowFetchStrategy))]
	[DependentBusinessObject(typeof(AsycudaManifestHeader), "Containers")]
	public class AsycudaContainer : AutoAsycudaContainer
		, Integration.Customs.ManifestBase.IAsycudaContainer
		, IClusterKeyWorker
	{
		public AsycudaContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly AsycudaContainerTypeDecider TypeDecider = new AsycudaContainerTypeDecider();

		public AsycudaManifestHeader Header => Factory.Load<AsycudaManifestHeader>(ACN_AMA_Manifest);

		[RelatedBusinessObject("Header")]
		public override ZGuid ACN_AMA_Manifest
		{
			get { return base.ACN_AMA_Manifest; }
			set { base.ACN_AMA_Manifest = value; }
		}

		public override void Delete()
		{
			this.DeleteChildren<AsycudaContainerBillOrPackageLink>(AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container);
			base.Delete();
		}

		public override bool SupportsNotes => false;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaContainerFetchStrategy(this);

		#region IClusterKeyWorker

		Type IClusterKeyWorker.ParentBizObjType => typeof(AsycudaManifestHeader);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)ACN_AMA_ManifestInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
		{
			get
			{
				yield return new ClusterKeyChildInfo(typeof(AsycudaContainerBillOrPackageLink), AsycudaContainerBillOrPackageLinkSchema.APC_ACN_Container);
			}
		}

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)ACN_ClusterKeyInfo;

		#endregion
	}
}


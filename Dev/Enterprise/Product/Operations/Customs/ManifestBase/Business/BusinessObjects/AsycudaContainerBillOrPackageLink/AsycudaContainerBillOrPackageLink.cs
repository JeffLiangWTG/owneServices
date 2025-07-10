using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.ManifestBase
{
	[SingleObjectAroundARow]
	[RowFetchStrategy(FetchStrategyType = typeof(AsycudaContainerBillOrPackageLinkRowFetchStrategy))]
	public class AsycudaContainerBillOrPackageLink : AutoAsycudaContainerBillOrPackageLink
		, Integration.Customs.ManifestBase.IAsycudaContainerBillOrPackageLink
		, IClusterKeyWorker
	{
		public AsycudaContainerBillOrPackageLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly AsycudaContainerBillOrPackageLinkTypeDecider TypeDecider = new AsycudaContainerBillOrPackageLinkTypeDecider();

		[RelatedBusinessObject("Container")]
		public override ZGuid APC_ACN_Container
		{
			get { return base.APC_ACN_Container; }
			set { base.APC_ACN_Container = value; }
		}

		public AsycudaContainer Container => Factory.Load<AsycudaContainer>(APC_ACN_Container);

		[RelatedBusinessObject("Pack")]
		public override ZGuid APC_APA_Pack
		{
			get { return base.APC_APA_Pack; }
			set { base.APC_APA_Pack = value; }
		}

		public AsycudaPack Pack => Factory.Load<AsycudaPack>(APC_APA_Pack);

		[RelatedBusinessObject("Bill")]
		public override ZGuid APC_ABL_Bill
		{
			get { return base.APC_ABL_Bill; }
			set { base.APC_ABL_Bill = value; }
		}

		public AsycudaBill Bill => Factory.Load<AsycudaBill>(APC_ABL_Bill);

		public override bool SupportsNotes => false;

		#region IClusterKeyWorker

		Type IClusterKeyWorker.ParentBizObjType => typeof(AsycudaContainer);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)APC_ACN_ContainerInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)APC_ClusterKeyInfo;

		#endregion
	}
}

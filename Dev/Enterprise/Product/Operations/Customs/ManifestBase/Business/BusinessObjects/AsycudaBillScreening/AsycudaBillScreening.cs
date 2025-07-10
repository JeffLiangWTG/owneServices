using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.ManifestBase
{
	public class AsycudaBillScreening : AutoAsycudaBillScreening, Integration.Customs.ManifestBase.IAsycudaBillScreening, IClusterKeyWorker
	{
		public AsycudaBillScreening(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly AsycudaBillScreeningTypeDecider TypeDecider = new AsycudaBillScreeningTypeDecider();

		public AsycudaBill Bill => Factory.Load<AsycudaBill>(ASR_ABL);

		[RelatedBusinessObject(nameof(Bill))]
		public override ZGuid ASR_ABL
		{
			get { return base.ASR_ABL; }
			set { base.ASR_ABL = value; }
		}

		#region IClusterKeyWorker

		Type IClusterKeyWorker.ParentBizObjType => typeof(AsycudaBill);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)ASR_ABLInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)ASR_ClusterKeyInfo;

		#endregion
	}
}

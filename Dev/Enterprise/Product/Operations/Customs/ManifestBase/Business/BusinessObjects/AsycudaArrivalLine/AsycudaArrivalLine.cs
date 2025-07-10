using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.ManifestBase
{
	[SingleObjectAroundARow]
	public class AsycudaArrivalLine : AutoAsycudaArrivalLine
		, Integration.Customs.ManifestBase.IAsycudaArrivalLine
		, IClusterKeyWorker
	{
		public AsycudaArrivalLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public AsycudaArrivalHeader ArrivalHeader => Factory.Load<AsycudaArrivalHeader>(ATL_ATH);

		[RelatedBusinessObject(nameof(ArrivalHeader))]
		public override ZGuid ATL_ATH
		{
			get { return base.ATL_ATH; }
			set { base.ATL_ATH = value; }
		}

		#region IClusterKeyWorker

		Type IClusterKeyWorker.ParentBizObjType => typeof(AsycudaArrivalHeader);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)ATL_ATHInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)ATL_ClusterKeyInfo;

		#endregion
	}
}

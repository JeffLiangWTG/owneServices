using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.EU.Intrastat.Business
{
	public class CusIntrastatMergedLine : AutoCusIntrastatMergedLine
		, IClusterKeyWorker
		, Integration.Customs.EU.ICusIntrastatMergedLine
	{
		public CusIntrastatMergedLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject(nameof(CusIntrastatMergedLine.Group))]
		public override ZGuid CIM_CIG_Group
		{
			get => base.CIM_CIG_Group;
			set => base.CIM_CIG_Group = value;
		}

		public CusIntrastatGroup Group => Factory.Load<CusIntrastatGroup>(CIM_CIG_Group);

		#region IClusterKeyWorker

		ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CIM_ClusterKeyInfo;

		Type IClusterKeyWorker.ParentBizObjType => typeof(CusIntrastatGroup);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CIM_CIG_GroupInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

		#endregion
	}
}

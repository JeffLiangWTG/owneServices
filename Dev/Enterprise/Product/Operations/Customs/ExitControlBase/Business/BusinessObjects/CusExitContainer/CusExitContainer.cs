using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.ExitControlBase.Business;

[SingleObjectAroundARow]
public class CusExitContainer : AutoCusExitContainer, IClusterKeyWorker
{
	public CusExitContainer(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public CusExitHeader Header => Factory.Load<CusExitHeader>(CXN_CXH_Header);

	[RelatedBusinessObject(nameof(Header))]
	public override ZGuid CXN_CXH_Header
	{
		get => base.CXN_CXH_Header;
		set => base.CXN_CXH_Header = value;
	}

	[ChildEditable(true)]
	public ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> CusExitConsignmentPivots
	{
		get
		{
			if (cusExitConsignmentPivots == null)
			{
				cusExitConsignmentPivots = CreateNewCusExitConsignmentPivotCollection();
				RegisterEditableChildObject(cusExitConsignmentPivots);
			}
			return cusExitConsignmentPivots;
		}
	}
	ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> cusExitConsignmentPivots;

	protected virtual ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> CreateNewCusExitConsignmentPivotCollection() => new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this);

	#region IClusterKeyWorker

	ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CXN_ClusterKeyInfo;

	Type IClusterKeyWorker.ParentBizObjType => typeof(CusExitHeader);

	ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CXN_CXH_HeaderInfo;

	IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

	#endregion
}


using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ExitControlBase.Business;

[SingleObjectAroundARow]
public class CusExitConsignmentPackage : AutoCusExitConsignmentPackage, IClusterKeyWorker
{
	public CusExitConsignmentPackage(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public CusExitHeader Header => Factory.Load<CusExitHeader>(CXP_CXH_Header);

	public CusExitConsignmentPivot ConsignmentPivot => Factory.LoadTop1<CusExitConsignmentPivot>(new ZQuery(CusExitConsignmentPivotSchema.CNP_CXP_Package, PK));

	[RelatedBusinessObject(nameof(Header))]
	public override ZGuid CXP_CXH_Header
	{
		get => base.CXP_CXH_Header;
		set => base.CXP_CXH_Header = value;
	}

	public ICusExitReportItemCollection<CusExitReportItem> CusExitReportItems
	{
		get
		{
			if (cusExitReportItems == null)
			{
				cusExitReportItems = CreateNewCusExitReportItemCollection();
			}
			return cusExitReportItems;
		}
	}
	ICusExitReportItemCollection<CusExitReportItem> cusExitReportItems;

	protected virtual ICusExitReportItemCollection<CusExitReportItem> CreateNewCusExitReportItemCollection() => new CusExitReportItemCollection<CusExitReportItem>(this);

	public override void Delete()
	{
		CusExitReportItems.DeleteAll();
		var consignmentPivot = ConsignmentPivot;
		base.Delete();
		consignmentPivot?.Delete();
	}

	#region IClusterKeyWorker

	ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CXP_ClusterKeyInfo;

	Type IClusterKeyWorker.ParentBizObjType => typeof(CusExitHeader);

	ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CXP_CXH_HeaderInfo;

	IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

	#endregion
}


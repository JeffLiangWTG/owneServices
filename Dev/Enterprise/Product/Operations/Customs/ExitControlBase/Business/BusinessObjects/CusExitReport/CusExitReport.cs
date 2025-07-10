using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ExitControlBase.Business;

[SingleObjectAroundARow]
public class CusExitReport : AutoCusExitReport, IClusterKeyWorker
{
	public CusExitReport(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public CusExitHeader Header => Factory.Load<CusExitHeader>(CER_CXH_Header);

	public CusExitConsignment Consignment => Factory.Load<CusExitConsignment>(CER_CXC_Consignment);

	[RelatedBusinessObject(nameof(Consignment))]
	public override ZGuid CER_CXC_Consignment
	{
		get => base.CER_CXC_Consignment;
		set => base.CER_CXC_Consignment = value;
	}

	[RelatedBusinessObject(nameof(Header))]
	public override ZGuid CER_CXH_Header
	{
		get => base.CER_CXH_Header;
		set => base.CER_CXH_Header = value;
	}

	[ChildEditable(true)]
	public ICusExitReportItemCollection<CusExitReportItem> CusExitReportItems
	{
		get
		{
			if (cusExitReportItems == null)
			{
				cusExitReportItems = CreateNewCusExitReportItemCollection(null);
				RegisterEditableChildObject(cusExitReportItems);
			}
			return cusExitReportItems;
		}
	}
	ICusExitReportItemCollection<CusExitReportItem> cusExitReportItems;

	protected virtual ICusExitReportItemCollection<CusExitReportItem> CreateNewCusExitReportItemCollection(ZQuery filter) => new CusExitReportItemCollection<CusExitReportItem>(this, filter);

	#region IClusterKeyWorker

	ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CER_ClusterKeyInfo;

	Type IClusterKeyWorker.ParentBizObjType => typeof(CusExitHeader);

	ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CER_CXH_HeaderInfo;

	IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
	{
		get
		{
			yield return new ClusterKeyChildInfo(typeof(CusExitReportItem), CusExitReportItemSchema.ERI_CER_Report);
		}
	}

	#endregion
}


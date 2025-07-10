using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.ExitControlBase.Business;

[SingleObjectAroundARow]
public class CusExitReportItem : AutoCusExitReportItem, IClusterKeyWorker
{
	public CusExitReportItem(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public CusExitReport Report => Factory.Load<CusExitReport>(ERI_CER_Report);

	public CusExitConsignmentItem ConsignmentItem => Factory.Load<CusExitConsignmentItem>(ERI_CCI_ConsignmentItem);

	public CusExitConsignmentPackage Package => Factory.Load<CusExitConsignmentPackage>(ERI_CXP_Package);

	[RelatedBusinessObject(nameof(ConsignmentItem))]
	public override ZGuid ERI_CCI_ConsignmentItem
	{
		get => base.ERI_CCI_ConsignmentItem;
		set => base.ERI_CCI_ConsignmentItem = value;
	}

	[RelatedBusinessObject(nameof(Report))]
	public override ZGuid ERI_CER_Report
	{
		get => base.ERI_CER_Report;
		set => base.ERI_CER_Report = value;
	}

	[RelatedBusinessObject(nameof(Package))]
	public override ZGuid ERI_CXP_Package
	{
		get => base.ERI_CXP_Package;
		set => base.ERI_CXP_Package = value;
	}

	#region IClusterKeyWorker

	ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)ERI_ClusterKeyInfo;

	Type IClusterKeyWorker.ParentBizObjType => typeof(CusExitReport);

	ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)ERI_CER_ReportInfo;

	IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

	#endregion
}


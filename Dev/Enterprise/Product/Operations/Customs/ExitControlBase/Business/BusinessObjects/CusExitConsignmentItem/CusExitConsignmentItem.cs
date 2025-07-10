using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ExitControlBase.Business;

[SingleObjectAroundARow]
public class CusExitConsignmentItem : AutoCusExitConsignmentItem, IClusterKeyWorker
{
	public CusExitConsignmentItem(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public CusExitConsignment Consignment => Factory.Load<CusExitConsignment>(CCI_CXC_Consignment);

	public int MaxPivotItemCount => MaxPivotItemCountCore;
	protected virtual int MaxPivotItemCountCore => 9;

	[RelatedBusinessObject(nameof(Consignment))]
	public override ZGuid CCI_CXC_Consignment
	{
		get => base.CCI_CXC_Consignment;
		set
		{
			var oldValue = CCI_CXC_Consignment;
			base.CCI_CXC_Consignment = value;
			if (!IsCopying && oldValue != CCI_CXC_Consignment)
			{
				CusExitConsignmentPivots.MarkAsNeedingValidation();
			}
		}
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

	[ChildEditable(true)]
	public ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> CusExitConsignmentPackagePivots
	{
		get
		{
			if (cusExitConsignmentPackagePivots == null)
			{
				cusExitConsignmentPackagePivots = CreateNewCusExitConsignmentPackagePivotCollection();
				RegisterEditableChildObject(cusExitConsignmentPackagePivots);
			}
			return cusExitConsignmentPackagePivots;
		}
	}
	ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> cusExitConsignmentPackagePivots;

	[ChildEditable(true)]
	public ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> CusExitConsignmentContainerPivots
	{
		get
		{
			if (cusExitConsignmentContainerPivots == null)
			{
				cusExitConsignmentContainerPivots = CreateNewCusExitConsignmentContainerPivotCollection();
				RegisterEditableChildObject(cusExitConsignmentContainerPivots);
			}
			return cusExitConsignmentContainerPivots;
		}
	}
	ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> cusExitConsignmentContainerPivots;

	protected virtual ICusExitReportItemCollection<CusExitReportItem> CreateNewCusExitReportItemCollection() => new CusExitReportItemCollection<CusExitReportItem>(this);

	protected virtual ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> CreateNewCusExitConsignmentPivotCollection() => new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this);

	protected virtual ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> CreateNewCusExitConsignmentPackagePivotCollection() => new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this, ConsignmentItemPivotType.Package);

	protected virtual ICusExitConsignmentPivotCollection<CusExitConsignmentPivot> CreateNewCusExitConsignmentContainerPivotCollection() => new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(this, ConsignmentItemPivotType.Container);

	protected internal ShortSequenceNumberGenerator PackageSequenceGenerator => packageSequenceNumberCalculator ?? (packageSequenceNumberCalculator = new ShortSequenceNumberGenerator(() => CusExitConsignmentPackagePivots));
	ShortSequenceNumberGenerator packageSequenceNumberCalculator;

	public override void Delete()
	{
		if (!IsDeleted)
		{
			FetchForLoadChildEditableObjectsIfNeeded();
			using (PackageSequenceGenerator.GetLineNumberSuspender())
			{
				CusExitConsignmentPivots.DeleteAll();
			}
			this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
		}
		base.Delete();
	}

	#region IClusterKeyWorker

	ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CCI_ClusterKeyInfo;

	Type IClusterKeyWorker.ParentBizObjType => typeof(CusExitConsignment);

	ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CCI_CXC_ConsignmentInfo;

	IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
	{
		get
		{
			yield return new ClusterKeyChildInfo(typeof(CusExitConsignmentPivot), CusExitConsignmentPivotSchema.CNP_CCI_ConsignmentItem);
		}
	}

	#endregion
}


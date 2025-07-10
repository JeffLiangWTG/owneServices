using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ExitControlBase.Business;

[SingleObjectAroundARow]
public class CusExitHeader : AutoCusExitHeader, IClusterKeyMaster, IAuditParent
{
	public CusExitHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();

		var currentBranch = GlbBranch.CurrentBranch;
		CXH_GB_Branch = currentBranch.PK;
		CXH_GC_Company = currentBranch.GB_GC;
	}

#if DEBUG

	protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
	{
		base.FillWithValidTestDataCore(kind, propertyPath);
		CXH_ApplicationCode = Common.CusExitHeaderApplicationCodeList.Codes.ExitControl;
	}

#endif

	[LightValidationTestExempt]
	public override ZInt CXH_ClusterKey
	{
		get => base.CXH_ClusterKey;
		set
		{
			if (CXH_ClusterKey != value)
			{
				this.CheckCanSetMasterClusterKey();
				base.CXH_ClusterKey = value;
				if (!IsCopying)
				{
					CusExitReports.MarkAsNeedingValidation();
					CusExitContainers.MarkAsNeedingValidation();
					CusExitConsignments.MarkAsNeedingValidation();
					CusExitConsignmentPackages.MarkAsNeedingValidation();
				}
			}
		}
	}

	public override ZGuid CXH_GC_Company
	{
		get => base.CXH_GC_Company;
		set
		{
			var oldValue = CXH_GC_Company;
			base.CXH_GC_Company = value;
			if (!IsCopying && oldValue != CXH_GC_Company)
			{
				CusExitReports.MarkAsNeedingValidation();
				CusExitContainers.MarkAsNeedingValidation();
				CusExitConsignmentPackages.MarkAsNeedingValidation();
				CusExitConsignments.SelectMany(consignment => consignment.CusExitConsignmentItems).ForEach(consignmentItem =>
				{
					consignmentItem.MarkAsNeedingValidation();
					consignmentItem.CusExitConsignmentPivots.MarkAsNeedingValidation();
				});
			}
		}
	}

	[ChildEditable(true)]
	public ICusExitReportCollection<CusExitReport> CusExitReports
	{
		get
		{
			if (cusExitReports == null)
			{
				cusExitReports = CreateNewCusExitReportCollection();
				RegisterEditableChildObject(cusExitReports);
			}
			return cusExitReports;
		}
	}
	ICusExitReportCollection<CusExitReport> cusExitReports;

	[ChildEditable(true)]
	public ICusExitContainerCollection<CusExitContainer> CusExitContainers
	{
		get
		{
			if (cusExitContainers == null)
			{
				cusExitContainers = CreateNewCusExitContainerCollection();
				RegisterEditableChildObject(cusExitContainers);
			}
			return cusExitContainers;
		}
	}
	ICusExitContainerCollection<CusExitContainer> cusExitContainers;

	[ChildEditable(true)]
	public ICusExitConsignmentCollection<CusExitConsignment> CusExitConsignments
	{
		get
		{
			if (cusExitConsignments == null)
			{
				cusExitConsignments = CreateNewCusExitConsignmentCollection();
				RegisterEditableChildObject(cusExitConsignments);
			}
			return cusExitConsignments;
		}
	}
	ICusExitConsignmentCollection<CusExitConsignment> cusExitConsignments;

	[ChildEditable(true)]
	public ICusExitConsignmentPackageCollection<CusExitConsignmentPackage> CusExitConsignmentPackages
	{
		get
		{
			if (cusExitConsignmentPackages == null)
			{
				cusExitConsignmentPackages = CreateNewCusExitConsignmentPackageCollection();
				RegisterEditableChildObject(cusExitConsignmentPackages);
			}
			return cusExitConsignmentPackages;
		}
	}
	ICusExitConsignmentPackageCollection<CusExitConsignmentPackage> cusExitConsignmentPackages;

	protected virtual ICusExitReportCollection<CusExitReport> CreateNewCusExitReportCollection() => new CusExitReportCollection<CusExitReport>(this);

	protected virtual ICusExitContainerCollection<CusExitContainer> CreateNewCusExitContainerCollection() => new CusExitContainerCollection<CusExitContainer>(this);

	protected virtual ICusExitConsignmentCollection<CusExitConsignment> CreateNewCusExitConsignmentCollection() => new CusExitConsignmentCollection<CusExitConsignment>(this);

	protected virtual ICusExitConsignmentPackageCollection<CusExitConsignmentPackage> CreateNewCusExitConsignmentPackageCollection() => new CusExitConsignmentPackageCollection<CusExitConsignmentPackage>(this);

	#region IClusterKeyMaster

	ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CXH_ClusterKeyInfo;
	#endregion

	IEnumerable<AuditChildInfo> IAuditParent.RelatedAuditChildren =>
	[
		new (CusExitConsignmentSchema.CXC_ClusterKey, null),
		new (CusExitConsignmentItemSchema.CCI_ClusterKey, null),
		new (CusExitConsignmentPackageSchema.CXP_ClusterKey, null),
		new (CusExitConsignmentPivotSchema.CNP_ClusterKey, null),
		new (CusExitContainerSchema.CXN_ClusterKey, null),
		new (CusExitReportSchema.CER_ClusterKey, null),
		new (CusExitReportItemSchema.ERI_ClusterKey, null),
	];
}

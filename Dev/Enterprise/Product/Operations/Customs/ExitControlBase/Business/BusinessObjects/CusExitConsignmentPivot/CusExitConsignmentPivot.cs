using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;

namespace Enterprise.Customs.ExitControlBase.Business;

[SingleObjectAroundARow]
public class CusExitConsignmentPivot : AutoCusExitConsignmentPivot
	, IClusterKeyWorker
	, IShortSequenceNumberLine
{
	public CusExitConsignmentPivot(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public CusExitConsignmentItem ConsignmentItem => Factory.Load<CusExitConsignmentItem>(CNP_CCI_ConsignmentItem);

	public CusExitContainer Container => Factory.Load<CusExitContainer>(CNP_CXN_Container);

	[RelatedBusinessObject(nameof(ConsignmentItem))]
	public override ZGuid CNP_CCI_ConsignmentItem
	{
		get => base.CNP_CCI_ConsignmentItem;
		set
		{
			var oldValue = CNP_CCI_ConsignmentItem;
			base.CNP_CCI_ConsignmentItem = value;
			if (!IsCopying && oldValue != CNP_CCI_ConsignmentItem)
			{
				Package?.MarkAsNeedingValidation();
			}
		}
	}

	[RelatedBusinessObject(nameof(Container))]
	public override ZGuid CNP_CXN_Container
	{
		get => base.CNP_CXN_Container;
		set => base.CNP_CXN_Container = value;
	}

	[RelatedBusinessObject(nameof(Package))]
	public override ZGuid CNP_CXP_Package
	{
		get => base.CNP_CXP_Package;
		set
		{
			var oldValue = CNP_CXP_Package;
			base.CNP_CXP_Package = value;
			if (!IsCopying && oldValue != CNP_CXP_Package)
			{
				ConsignmentItem?.PackageSequenceGenerator.RecalculateWhenAdded(this);
			}
		}
	}

	public CusExitConsignmentPackage Package
	{
		get
		{
			if (packageCache == null)
			{
				packageCache = GetPackageCore();
				RegisterEditableChildObject(packageCache);
			}
			return packageCache;
		}
	}
	CusExitConsignmentPackage packageCache;

	protected virtual CusExitConsignmentPackage GetPackageCore() => Factory.Load<CusExitConsignmentPackage>(CNP_CXP_Package);

	public ZShort SequenceNumber => ((ISequenceNumberLine<ZShort>)this).SequenceNumber;

	public override void Delete()
	{
		if (!IsDeleted)
		{
			var package = Package;
			if (package != null && !package.IsDeleted)
			{
				ConsignmentItem?.PackageSequenceGenerator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
				package.Delete();
			}
		}

		base.Delete();
	}

	#region  IShortSequenceNumberLine

	ZGuid ISequenceNumberLine.FKToHeader => CNP_CCI_ConsignmentItem;

	ZShort ISequenceNumberLine<ZShort>.SequenceNumber
	{
		get { return Package?.CXP_Sequence ?? ZShort.Zero; }
		set
		{
			if (Package is CusExitConsignmentPackage package)
			{
				package.CXP_Sequence = value;
			}
		}
	}

	#endregion

	#region IClusterKeyWorker

	ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CNP_ClusterKeyInfo;

	Type IClusterKeyWorker.ParentBizObjType => typeof(CusExitConsignmentItem);

	ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CNP_CCI_ConsignmentItemInfo;

	IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;

	#endregion
}


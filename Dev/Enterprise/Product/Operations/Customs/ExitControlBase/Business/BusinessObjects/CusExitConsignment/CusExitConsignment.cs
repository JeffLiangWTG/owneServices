using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ExitControlBase.Business;

[SingleObjectAroundARow]
public class CusExitConsignment : AutoCusExitConsignment, IClusterKeyWorker
{
	public CusExitConsignment(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public CusExitHeader Header => Factory.Load<CusExitHeader>(CXC_CXH_Header);

	public int MaxItemCount => MaxItemCountCore;
	protected virtual int MaxItemCountCore => 99;

	[RelatedBusinessObject(nameof(Header))]
	public override ZGuid CXC_CXH_Header
	{
		get => base.CXC_CXH_Header;
		set
		{
			var oldValue = CXC_CXH_Header;
			base.CXC_CXH_Header = value;
			if (!IsCopying && oldValue != CXC_CXH_Header)
			{
				CusExitConsignmentItems.MarkAsNeedingValidation();
				CusExitConsignmentItems.ForEach(x => x.CusExitConsignmentPivots.MarkAsNeedingValidation());
			}
		}
	}

	[ChildEditable(true)]
	public ICusExitConsignmentItemCollection<CusExitConsignmentItem> CusExitConsignmentItems
	{
		get
		{
			if (cusExitConsignmentItems == null)
			{
				cusExitConsignmentItems = CreateNewCusExitConsignmentItemCollection();
				RegisterEditableChildObject(cusExitConsignmentItems);
			}
			return cusExitConsignmentItems;
		}
	}
	ICusExitConsignmentItemCollection<CusExitConsignmentItem> cusExitConsignmentItems;

	protected virtual ICusExitConsignmentItemCollection<CusExitConsignmentItem> CreateNewCusExitConsignmentItemCollection() => new CusExitConsignmentItemCollection<CusExitConsignmentItem>(this);

	public override void Delete()
	{
		if (!IsDeleted)
		{
			FetchForLoadChildEditableObjectsIfNeeded();
			this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			this.DeleteChildren<CusExitReport>(CusExitReportSchema.CER_CXC_Consignment);
			CusExitConsignmentItems.DeleteAll();
		}
		base.Delete();
	}

	#region IClusterKeyWorker

	ZPropertyInfoInt IClusterKeyEntity.ClusterKeyPty => (ZPropertyInfoInt)CXC_ClusterKeyInfo;

	Type IClusterKeyWorker.ParentBizObjType => typeof(CusExitHeader);

	ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)CXC_CXH_HeaderInfo;

	IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList
	{
		get
		{
			yield return new ClusterKeyChildInfo(typeof(CusExitConsignmentItem), CusExitConsignmentItemSchema.CCI_CXC_Consignment);
		}
	}

	#endregion
}


using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ExitControlBase.Business;

public interface ICusExitConsignmentPivotCollection<out T> : IActiveBusinessObjectCollection<T>
	where T : CusExitConsignmentPivot
{
	new T this[int index] { get; }
	void MarkAsNeedingValidation();
	event CollectionCountChangedEventHandler CollectionCountChange;
	IDisposable SuspendAllowNew();
}

public class CusExitConsignmentPivotCollection<T> : ActiveBusinessObjectCollection<T>, ICusExitConsignmentPivotCollection<T>
	where T : CusExitConsignmentPivot
{
	public CusExitConsignmentPivotCollection(CusExitConsignmentItem master)
		: base(master.Factory, master, new ZQuery(), CusExitConsignmentPivotSchema.CNP_CCI_ConsignmentItem)
	{
	}

	public CusExitConsignmentPivotCollection(CusExitConsignmentItem master, ConsignmentItemPivotType consignmentItemPivotType)
		: this(master)
	{
		cusExitConsignmentItem = master;
		isItemPackagePivot = consignmentItemPivotType == ConsignmentItemPivotType.Package;
		isItemContainerPivot = consignmentItemPivotType == ConsignmentItemPivotType.Container;
		if (isItemPackagePivot)
		{
			this.EnableMaxCountValidation(cusExitConsignmentItem.MaxPivotItemCount, Res.GetString("779E4DCE-E16C-40DA-A922-C91FE3EF4367", "Maximum number of Packing Details is {0}.", cusExitConsignmentItem.MaxPivotItemCount), false);
		}
	}
	readonly CusExitConsignmentItem cusExitConsignmentItem;
	readonly ZBool isItemPackagePivot;
	readonly ZBool isItemContainerPivot;

	public CusExitConsignmentPivotCollection(CusExitContainer master)
		: base(master.Factory, master, new ZQuery(), CusExitConsignmentPivotSchema.CNP_CXN_Container)
	{
	}

	#region AllowNew & Suspender

	protected override bool AllowNew => allowNewSuspender < 1;
	int allowNewSuspender;

	protected override object[] GetCollectionState()
	{
		return new object[] { allowNewSuspender };
	}

	IDisposable ICusExitConsignmentPivotCollection<T>.SuspendAllowNew() => new DisposableAction(() => allowNewSuspender++, () => allowNewSuspender--);

	#endregion

	protected override void SetDefaultsForNewElementCore(T newElement)
	{
		base.SetDefaultsForNewElementCore(newElement);

		var exitHeader = cusExitConsignmentItem?.Consignment?.Header;
		if (exitHeader != null)
		{
			if (isItemPackagePivot)
			{
				var package = exitHeader.CusExitConsignmentPackages.AddNew();
				newElement.CNP_CXP_Package = package.PK;
			}
			else if (isItemContainerPivot)
			{
				var container = exitHeader.CusExitContainers.AddNew();
				newElement.CNP_CXN_Container = container.PK;
			}
		}
	}

	protected override ZQuery CreateRelationshipFilter()
	{
		var filter = base.CreateRelationshipFilter();
		if (isItemPackagePivot)
		{
			filter.AddToFilter(CusExitConsignmentPivotSchema.CNP_CXP_Package, SQLComparisonOperator.NotEqual, ZGuid.Empty);
		}
		else if (isItemContainerPivot)
		{
			filter.AddToFilter(CusExitConsignmentPivotSchema.CNP_CXN_Container, SQLComparisonOperator.NotEqual, ZGuid.Empty);
		}
		return filter;
	}
}

public enum ConsignmentItemPivotType
{
	Package,
	Container,
}

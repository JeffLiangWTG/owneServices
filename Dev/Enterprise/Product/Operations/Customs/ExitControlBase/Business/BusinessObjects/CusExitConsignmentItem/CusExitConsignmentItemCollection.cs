using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ExitControlBase.Business;

public interface ICusExitConsignmentItemCollection<out T> : IActiveBusinessObjectCollection<T>
	where T : CusExitConsignmentItem
{
	new T this[int index] { get; }
	void MarkAsNeedingValidation();
	event CollectionCountChangedEventHandler CollectionCountChange;
	IDisposable SuspendAllowNew();
}

public class CusExitConsignmentItemCollection<T> : ActiveBusinessObjectCollection<T>, ICusExitConsignmentItemCollection<T>
	where T : CusExitConsignmentItem
{
	public CusExitConsignmentItemCollection(CusExitConsignment master)
		: base(master.Factory, master, new ZQuery(), CusExitConsignmentItemSchema.CCI_CXC_Consignment)
	{
		this.EnableMaxCountValidation(master.MaxItemCount, Res.GetString("BD402659-7A8E-4FDE-AA7F-53AE3E759830", "Maximum number of items is {0}.", master.MaxItemCount), false);
	}

	#region AllowNew & Suspender

	protected override bool AllowNew => allowNewSuspender < 1;
	int allowNewSuspender;

	protected override object[] GetCollectionState()
	{
		return new object[] { allowNewSuspender, cusExitConsignmentPackagePivotsSuspenders };
	}

	IDisposable ICusExitConsignmentItemCollection<T>.SuspendAllowNew() => new DisposableAction(SuspendAllowNew, ResumeAllowNew);
	readonly List<IDisposable> cusExitConsignmentPackagePivotsSuspenders = new List<IDisposable>();

	void SuspendAllowNew()
	{
		allowNewSuspender++;
		foreach (CusExitConsignmentItem item in this)
		{
			cusExitConsignmentPackagePivotsSuspenders.Add(item.CusExitConsignmentPackagePivots.SuspendAllowNew());
		}
	}

	void ResumeAllowNew()
	{
		allowNewSuspender--;
		foreach (IDisposable suspender in cusExitConsignmentPackagePivotsSuspenders)
		{
			suspender.Dispose();
		}
	}

	#endregion
}

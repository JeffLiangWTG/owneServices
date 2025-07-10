using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ExitControlBase.Business;

public interface ICusExitReportItemCollection<out T> : IActiveBusinessObjectCollection<T>, IBindingList
{
	new T this[int index] { get; }
	void MarkAsNeedingValidation();
	event CollectionCountChangedEventHandler CollectionCountChange;
	void ApplySort(string propertyName, ListSortDirection direction);
}

public class CusExitReportItemCollection<T> : ActiveBusinessObjectCollection<T>, ICusExitReportItemCollection<T>
	where T : CusExitReportItem
{
	public CusExitReportItemCollection(CusExitReport master, ZQuery filter)
		: base(master.Factory, master, filter, CusExitReportItemSchema.ERI_CER_Report)
	{
	}

	public CusExitReportItemCollection(CusExitConsignmentItem master)
		: base(master.Factory, master, new ZQuery(), CusExitReportItemSchema.ERI_CCI_ConsignmentItem)
	{
	}

	public CusExitReportItemCollection(CusExitConsignmentPackage master)
		: base(master.Factory, master, new ZQuery(), CusExitReportItemSchema.ERI_CXP_Package)
	{
	}

	protected override bool AllowNew => false;

	bool IBindingList.AllowRemove => false;
}

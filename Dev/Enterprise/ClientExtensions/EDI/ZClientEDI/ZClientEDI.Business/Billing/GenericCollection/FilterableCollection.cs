using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.Billing.GenericCollection;

public abstract class FilterableCollection<T> : NonPersistentBusinessObject, IFilterableCollection where T : BusinessObject
{
	readonly ActiveBusinessObjectCollection<T> Parent;
	public FilterStripBusinessObject FilterObject { get; }

	protected FilterableCollection(ActiveBusinessObjectCollection<T> parent,
		FilterStripBusinessObject filterObject) : base(parent.Factory)
	{
		Parent = parent;
		FilterObject = filterObject;
		((IFilterStripBusinessObjectInternals)FilterObject).LayoutContext = "BillingPrices";
	}

	public void ApplyFilter()
	{
		var filter = FilterObject.Filter;
		if (!filter.IsNoResultQuery)
		{
			Parent.AdditionalFilter = filter;
		}
		else
		{
			ClearFilter();
		}
	}

	public void ClearFilter()
	{
		Parent.AdditionalFilter = new ZQuery();
	}
}

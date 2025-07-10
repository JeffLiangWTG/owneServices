using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.Billing.GenericCollection;
public interface IFilterableCollection : IBusiness
{
	void ApplyFilter();
	void ClearFilter();
	FilterStripBusinessObject FilterObject { get; }
}

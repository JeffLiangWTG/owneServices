using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Integration
{
	public interface IPeriodicInvoicingReferenceNumberFilter
	{
		IModuleFilter GetReferenceNumberFilter(FilterBusinessObject provider, ZString filterDescription);
	}
}

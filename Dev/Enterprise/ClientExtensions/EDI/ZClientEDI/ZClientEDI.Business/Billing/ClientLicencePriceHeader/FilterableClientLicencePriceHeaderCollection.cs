using Enterprise.Client.EDI.Billing.GenericCollection;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class FilterableClientLicencePriceHeaderCollection : FilterableCollection<ClientLicencePriceHeader>
	{
		public FilterableClientLicencePriceHeaderCollection(ClientLicencePriceHeaderCollection parent,
			FilterStripBusinessObject filterObject) : base(parent, filterObject) { }
	}
}

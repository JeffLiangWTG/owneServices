using Enterprise.Client.EDI.Billing.GenericCollection;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class FilterableClientLicencePriceItemCollection : FilterableCollection<ClientLicencePriceItem>
	{
		public FilterableClientLicencePriceItemCollection(ClientLicencePriceItemCollection parent,
			FilterStripBusinessObject filterObject) : base(parent, filterObject) { }
	}
}

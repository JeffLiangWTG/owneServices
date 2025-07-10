using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class JobComInvoiceHeaderCollection : BusinessObjectCollection<JobComInvoiceHeader>
	{
		public JobComInvoiceHeaderCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }
	}
}

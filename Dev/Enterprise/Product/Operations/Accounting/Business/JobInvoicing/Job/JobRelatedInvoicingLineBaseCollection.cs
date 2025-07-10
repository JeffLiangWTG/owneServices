using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class JobRelatedInvoicingLineBaseCollection : InvoicingLineBaseCollection
	{
		public JobRelatedInvoicingLineBaseCollection(BusinessObject businessObject)
			: base(businessObject)
		{
		}
	}
}

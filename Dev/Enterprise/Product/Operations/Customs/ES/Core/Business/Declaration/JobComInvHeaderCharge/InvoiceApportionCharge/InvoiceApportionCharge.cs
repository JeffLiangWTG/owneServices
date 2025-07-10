using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class InvoiceApportionCharge : EU.Business.Declaration.InvoiceApportionCharge, Integration.Customs.ES.IInvoiceApportionCharge
	{
		public InvoiceApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;
	}
}

using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;

namespace Enterprise.Client.JAS.Business.Invoicing
{
	public interface IJASInvoicingBase : IPayablesAndReceivables
	{
		ZString CreditNoteOrInvoice { get; }
		InvoicingBase InvoicingBase { get; }
		bool IsInJasSpecificNeedsCoreValidation { get; set; }
	}
}

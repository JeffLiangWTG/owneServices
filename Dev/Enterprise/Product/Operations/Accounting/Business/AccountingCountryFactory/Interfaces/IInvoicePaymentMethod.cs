using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IInvoicePaymentMethod
	{
		CodeDescriptionPair GetInvoicePaymentMethod(ZString invoiceTermType, ZDateTime? invoiceDate, ZDateTime? dueDate);
	}
}

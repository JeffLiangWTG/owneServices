using System;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IAccountingPayablesService
	{
		string GetTaxCode(string countryCode);

		DateTime GetDueDate(DateTime invoiceDate, ZGuid companyPK, ZGuid creditorPK);
	}
}

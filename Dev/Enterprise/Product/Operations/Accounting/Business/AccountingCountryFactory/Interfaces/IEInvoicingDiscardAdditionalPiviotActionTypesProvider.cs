using System.Collections.Generic;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IEInvoicingDiscardAdditionalPiviotActionTypesProvider
	{
		IEnumerable<string> GetAdditionalPivotActionTypes(string actionType);
	}
}

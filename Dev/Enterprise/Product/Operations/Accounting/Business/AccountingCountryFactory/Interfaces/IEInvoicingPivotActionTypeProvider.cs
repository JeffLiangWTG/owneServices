using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IEInvoicingPivotActionTypeProvider
	{
		string GetPivotActionType(AccTransactionHeader transaction);
	}
}

using CargoWise.Types;

namespace Enterprise.Accounting.Business.EInvoicing
{
	public interface IExistPivotCheckProvider
	{
		bool CheckExistActivePivot(AccEInvoicingTransactionPivot pivot);

		(bool, ZString) CanExistSucceedPivot();
	}
}

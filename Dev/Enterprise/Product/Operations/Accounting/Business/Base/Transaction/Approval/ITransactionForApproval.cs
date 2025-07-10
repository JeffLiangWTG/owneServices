using CargoWise.Types;

namespace Enterprise.Accounting.Business.TransactionApproval
{
	public interface ITransactionForApproval
	{
		ZDecimal AH_LocalTotalAmount { get; }
		ZString HumanReadableName { get; }
	}
}

#if DEBUG

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public partial class DepositBatch
	{
		public decimal CorrectSigns_ForTestOnly(string transactionType, decimal oSTotal)
		{
			return CorrectSigns(transactionType, oSTotal);
		}

		public CodeDescriptionPairList BankChargeTypes_List_ForTestOnly => BankChargeTypes_List;
	}
}

#endif

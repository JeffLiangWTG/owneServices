#if DEBUG

namespace Enterprise.Accounting.Business.Base.Reversing.BadDebtWritingOff
{
	public partial class PayablesAndReceivablesWritingOff
	{
		public void GenerateReverseTransactions_ForTestOnly()
		{
			GenerateReverseTransactions();
		}

		public void SetCancellationFlagOnTransactionsToReverse_ForTestOnly()
		{
			SetCancellationFlagOnTransactionsToReverse();
		}

		public string CantWriteOffPartiallyPaidTransactionErrorMessage_ForTestOnly => CantWriteOffPartiallyPaidTransactionErrorMessage;

		public string HaventSecuryRightsErrorMessage_ForTestOnly => HaventSecuryRightsErrorMessage;

		public string HaventBadDebtAccountInRegistryErrorMessage_ForTestOnly => HaventBadDebtAccountInRegistryErrorMessage;
	}
}

#endif

#if DEBUG

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class PayablesAndReceivablesReversing
	{
		public void DoReverseTransaction_ForTestOnly()
		{
			DoReverseTransaction();
		}

		public Matching.TransactionMatchLinkCollection ReverseTransactionMatchLinks_ForTestOnly
		{
			get { return ReverseTransactionMatchLinks; }
			set { ReverseTransactionMatchLinks = value; }
		}

		public Matching.TransactionMatchLinkCollection OriginalTransactionMatchLinks_ForTestOnly
		{
			get { return OriginalTransactionMatchLinks; }
			set { OriginalTransactionMatchLinks = value; }
		}

		public Interfaces.IPayablesAndReceivables ReversePayablesAndReceivables_ForTestOnly => ReversePayablesAndReceivables;
	}
}

#endif

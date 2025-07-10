using System;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	[Serializable]
	public class CannotGenerateCashAdvanceJournalException : Exception
	{
#if NETFRAMEWORK
		public CannotGenerateCashAdvanceJournalException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		public CannotGenerateCashAdvanceJournalException(CashAdvanceRequestHeader cashAdvance, string errorMessage) : base(errorMessage)
		{
			CashAdvanceRequestHeader = cashAdvance;
		}

		public CashAdvanceRequestHeader CashAdvanceRequestHeader { get; }
	}
}

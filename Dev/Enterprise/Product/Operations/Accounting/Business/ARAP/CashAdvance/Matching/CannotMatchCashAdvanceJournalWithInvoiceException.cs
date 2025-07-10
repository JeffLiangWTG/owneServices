using System;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	[Serializable]
	internal class CannotMatchCashAdvanceJournalWithInvoiceException : Exception
	{
#if NETFRAMEWORK
		internal CannotMatchCashAdvanceJournalWithInvoiceException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif

		internal CannotMatchCashAdvanceJournalWithInvoiceException(string errorMessage) : base(errorMessage)
		{
		}
	}
}

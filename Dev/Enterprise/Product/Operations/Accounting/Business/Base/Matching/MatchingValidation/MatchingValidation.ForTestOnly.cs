#if DEBUG

using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.Base.Matching
{
	partial class MatchingValidation
	{
		public TransactionHeader Parent_ForTestOnly => Parent;
	}
}

#endif

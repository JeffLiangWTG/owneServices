#if DEBUG

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public partial class IMatchingCollection
	{
		public IMatchingCollection MustBeMatched_ForTestOnly => MustBeMatched;
	}
}

#endif
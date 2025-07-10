#if DEBUG

namespace Enterprise.Accounting.Business.Base.Unmatching
{
	public partial class UnmatchingRow
	{
		public bool AllowFutureUnmatchDate_ForTestOnly => AllowFutureUnmatchDate;
	}
}

#endif

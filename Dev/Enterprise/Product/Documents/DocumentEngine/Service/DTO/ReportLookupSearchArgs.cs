
namespace Enterprise.DocumentEngine
{
	public class ReportLookupSearchArgs
	{
		public string SearchTerms
		{
			get { return searchTerms; }
			set { searchTerms = value?.Trim(); }
		}

		string searchTerms;

		public int Top { get; set; } = 50;
	}

	public class ReportLookupSearchArgs<T> : ReportLookupSearchArgs
	{
		public T Parent { get; set; }
	}
}

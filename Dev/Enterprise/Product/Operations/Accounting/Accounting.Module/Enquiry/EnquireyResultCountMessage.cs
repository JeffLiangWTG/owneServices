using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
#if DEBUG
	internal
#endif
	class EnquireyResultCountMessage : ResultCountMessage
	{
		internal EnquireyResultCountMessage(IFilterControl filterControl, int maxRowsToLoad, int maxRecommendedRowsToLoad)
			: base(filterControl, maxRowsToLoad, maxRecommendedRowsToLoad)
		{ }

		protected override string GetTooManyResultsErrorMessage(int numberResults)
		{
			return string.Format(TooManyRecordsError, numberResults);
		}

		static string TooManyRecordsError
		{
			get { return Res.GetString("e5841a99-5211-45bc-96fe-c05f6e30436e", "Too many records ({0:G}). Please contact admin to increase the max. number of records displayed."); }
		}
	}
}

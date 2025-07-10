using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Testing
{
	class EnquireyResultCountMessageTesting : EnquireyResultCountMessage
	{
		public EnquireyResultCountMessageTesting(IFilterControl filterControl, int maxRowsToLoad, int maxRecommendedRowsToLoad)
			: base(filterControl, maxRowsToLoad, maxRecommendedRowsToLoad)
		{ }

		public string GetTooManyResultsErrorMessageExposed(int numberResults)
		{
			return GetTooManyResultsErrorMessage(numberResults);
		}
	}
}

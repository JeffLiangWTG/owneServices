#if DEBUG

namespace Enterprise.DocumentEngine.Testing
{
	using Enterprise.DocumentEngine.FlexCelInterface;

	interface ITemplateCellSmellFinder
	{
		bool MatchesSmell(ExcelCell cell);
	}
}

#endif
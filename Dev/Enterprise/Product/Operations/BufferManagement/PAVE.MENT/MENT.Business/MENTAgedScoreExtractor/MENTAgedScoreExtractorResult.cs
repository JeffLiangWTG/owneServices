using System.Collections.Generic;
using System.Linq;

namespace Enterprise.PAVE.MENT.Business
{
	public class MENTAgedScoreExtractorResult
	{
		public MENTAgedScoreExtractorResult(IEnumerable<MENTAgedScoreExtractionResult> extractionResults)
		{
			this.extractionResults = extractionResults;
		}

		public IEnumerable<MENTAgedScoreExtractionResult> ExtractionResults
		{
			get { return extractionResults; }
		}

		readonly IEnumerable<MENTAgedScoreExtractionResult> extractionResults;

		public IEnumerable<ColumnsToCategoryIndex> ResultCategoryIndexMap
		{
			get { return extractionResults.SelectMany(r => r.Results).CreateCategoryIndexMap(); }
		}
	}
}

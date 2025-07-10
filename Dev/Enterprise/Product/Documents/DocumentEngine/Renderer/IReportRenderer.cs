namespace Enterprise.DocumentEngine
{
	using System.Collections.Generic;
	using Enterprise.DocumentEngine.Areas;
	using Enterprise.DocumentEngine.Renderer;

	interface IReportRenderer
	{
		void Render();

		Passes CurrentPass { get; set; }
		Area CurrentAreaToProcess { get; set; }
		int CurrentRow { get; }
		int CurrentColumn { get; set; }
		int CurrentDataRow { get; }

		int CurrentPageNumber { get; }
		int TotalNumberOfPages { get; }
		PageCollection Pages { get; }

		bool ExpandRowsForAutoHeight { get; }

		IEnumerable<int> HiddenColumns { get; }
		IEnumerable<int> OriginalColumnWidths { get; }
		void SaveOriginalColumnWidths();

		bool IsProcessingMacros { get; }
		bool IsProcessingPageBreaks { get; }
		bool IsProcessingAutoShapes { get; }

		bool IsCurrentSectionBodyAreaWithMoreThanOneRowData { get; }

		bool ReplaceTotalMacroInFormulaCell { get; }
	}
}

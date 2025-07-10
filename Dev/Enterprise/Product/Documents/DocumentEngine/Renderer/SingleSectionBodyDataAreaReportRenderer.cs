namespace Enterprise.DocumentEngine
{
	using System.Collections.Generic;
	using System.Linq;
	using Enterprise.DocumentEngine.Areas;
	using Enterprise.DocumentEngine.Renderer;

	class SingleSectionBodyDataAreaReportRenderer : IReportRenderer
	{
		public SingleSectionBodyDataAreaReportRenderer(Report report)
		{
			this.report = report;
			renderer = new ReportRenderer(report);
		}

		readonly Report report;
		readonly IReportRenderer renderer;

		public void Render()
		{
			RemoveAllAreasExceptForTheFirstSectionBodyDataArea();
			renderer.Render();
		}

		void RemoveAllAreasExceptForTheFirstSectionBodyDataArea()
		{
			var workSheet = report.WorkSheetCurrentlyBeingProcessed;

			report.Analyser = new ReportAnalyser(report);
			report.Analyser.Analyse();

			var areas = report.Analyser.Areas;
			var removedAreasWithFields = new List<Area>();
			foreach (var area in GetAreasToRemove(areas).Reverse())
			{
				if (area.AllDatafields.Count > 0)
				{
					removedAreasWithFields.Add(area);
				}
				workSheet.RemoveRows(area.StartingRow, area.End + 1);
			}
			this.removedAreasWithFields = removedAreasWithFields;
			report.Analyser.Reset();
			report.ResetIsPreparedForRender();
			report.ResetCachedExcelFile();
		}

		IEnumerable<Area> removedAreasWithFields;

		public IEnumerable<Area> RemovedAreasWithFields
		{
			get { return removedAreasWithFields; }
		}

		IEnumerable<Area> GetAreasToRemove(IEnumerable<Area> areas)
		{
			var areaList = areas.ToList();
			var result = new List<Area>(areaList);
			result.RemoveAll(area => area is ConfigArea || area is EndOfReportArea);

			var firstSectionBodyArea = areaList.FirstOrDefault(area => area is SectionBodyArea);
			result.Remove(firstSectionBodyArea);

			return result;
		}

		public Passes CurrentPass
		{
			get => renderer.CurrentPass;
			set => renderer.CurrentPass = value;
		}

		public Area CurrentAreaToProcess
		{
			get => renderer.CurrentAreaToProcess;
			set => renderer.CurrentAreaToProcess = value;
		}

		public int CurrentRow => renderer.CurrentRow;

		public int CurrentColumn
		{
			get => renderer.CurrentColumn;
			set => renderer.CurrentColumn = value;
		}

		public int CurrentDataRow => renderer.CurrentDataRow;

		public int CurrentPageNumber => renderer.CurrentPageNumber;

		public int TotalNumberOfPages => renderer.TotalNumberOfPages;

		public PageCollection Pages => renderer.Pages;

		public bool ExpandRowsForAutoHeight => renderer.ExpandRowsForAutoHeight;

		public IEnumerable<int> HiddenColumns => renderer.HiddenColumns;

		public IEnumerable<int> OriginalColumnWidths => renderer.OriginalColumnWidths;

		public void SaveOriginalColumnWidths() => renderer.SaveOriginalColumnWidths();
		public bool IsProcessingMacros => renderer.IsProcessingMacros;
		public bool IsProcessingPageBreaks => renderer.IsProcessingPageBreaks;
		public bool IsProcessingAutoShapes => renderer.IsProcessingAutoShapes;

		public bool IsCurrentSectionBodyAreaWithMoreThanOneRowData => renderer.IsCurrentSectionBodyAreaWithMoreThanOneRowData;

		public bool ReplaceTotalMacroInFormulaCell => true;
	}
}

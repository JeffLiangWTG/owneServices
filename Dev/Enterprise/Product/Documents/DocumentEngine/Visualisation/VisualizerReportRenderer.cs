using System.Collections.Generic;
using Enterprise.DocumentEngine.Areas;

namespace Enterprise.DocumentEngine.Visualisation
{
	class VisualizerReportRenderer : IReportRenderer
	{
		public VisualizerReportRenderer(IReportRenderer renderer)
		{
			this.renderer = renderer;
		}

		readonly IReportRenderer renderer;

		void IReportRenderer.Render()
		{
			renderer.Render();
		}

		Passes IReportRenderer.CurrentPass
		{
			get => Passes.SecondPass;
			set => renderer.CurrentPass = value;
		}

		Area IReportRenderer.CurrentAreaToProcess
		{
			get => renderer.CurrentAreaToProcess;
			set => renderer.CurrentAreaToProcess = value;
		}

		int IReportRenderer.CurrentRow => renderer.CurrentRow;

		int IReportRenderer.CurrentColumn
		{
			get => renderer.CurrentColumn;
			set => renderer.CurrentColumn = value;
		}

		int IReportRenderer.CurrentDataRow => renderer.CurrentDataRow;

		int IReportRenderer.CurrentPageNumber => renderer.CurrentPageNumber;

		int IReportRenderer.TotalNumberOfPages => renderer.TotalNumberOfPages;

		Renderer.PageCollection IReportRenderer.Pages => renderer.Pages;

		bool IReportRenderer.ExpandRowsForAutoHeight => renderer.ExpandRowsForAutoHeight;

		IEnumerable<int> IReportRenderer.HiddenColumns => renderer.HiddenColumns;

		IEnumerable<int> IReportRenderer.OriginalColumnWidths => renderer.OriginalColumnWidths;

		void IReportRenderer.SaveOriginalColumnWidths() => renderer.SaveOriginalColumnWidths();

		bool IReportRenderer.IsProcessingMacros => renderer.IsProcessingMacros;

		bool IReportRenderer.IsProcessingPageBreaks => renderer.IsProcessingPageBreaks;

		bool IReportRenderer.IsProcessingAutoShapes => renderer.IsProcessingAutoShapes;

		public bool IsCurrentSectionBodyAreaWithMoreThanOneRowData => renderer.IsCurrentSectionBodyAreaWithMoreThanOneRowData;

		public bool ReplaceTotalMacroInFormulaCell => false;
	}
}

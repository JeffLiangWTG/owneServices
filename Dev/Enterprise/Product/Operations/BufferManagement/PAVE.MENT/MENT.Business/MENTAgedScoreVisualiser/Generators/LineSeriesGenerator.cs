using System.Collections.Generic;
using CargoWise.Common;
using OxyPlot;
using OxyPlot.Series;

namespace Enterprise.PAVE.MENT.Business
{
	class LineSeriesGenerator : SeriesGeneratorBase
	{
		internal LineSeriesGenerator(MENTAgedScoreVisualisation visualisation, MENTAgedScoreExtractorResult set, IEnumerable<ColumnsToCategoryIndex> categoryIndexMap)
			: base(visualisation, set, categoryIndexMap)
		{
			Argument.NotNull(visualisation, nameof(visualisation));
			Argument.NotNull(set, nameof(set));
			Argument.NotNull(categoryIndexMap, nameof(categoryIndexMap));
		}

		protected override Series CreateSeriesCore(SeriesAndSeriesData series)
		{
			var title = string.IsNullOrWhiteSpace(series.Name) ? GetInvalidSeriesName() : series.Name;

			var columnSeries = new LineSeries
			{
				LabelFormatString = "{1}",
				TrackerFormatString = "{0}\n{1}: {2:0.#}\n{3}: {4:0.#}", // Format String for Graph
				Background = OxyColors.Automatic,
				Title = title,
				BrokenLineColor = OxyColors.Gray,
				BrokenLineThickness = 0.5
			};

			foreach (var val in series.Points)
			{
				columnSeries.Points.Add(new DataPoint(val.X, (double)val.Y));
			}

			columnSeries.InterpolationAlgorithm = InterpolationAlgorithms.CanonicalSpline;
			columnSeries.MarkerType = MarkerType.Circle;

			return columnSeries;
		}
	}
}

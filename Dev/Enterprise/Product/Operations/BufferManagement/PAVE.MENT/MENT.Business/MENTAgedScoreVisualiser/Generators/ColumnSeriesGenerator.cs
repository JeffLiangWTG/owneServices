using System.Collections.Generic;
using CargoWise.Common;
using OxyPlot;
using OxyPlot.Series;

namespace Enterprise.PAVE.MENT.Business
{
	class ColumnSeriesGenerator : SeriesGeneratorBase
	{
		internal ColumnSeriesGenerator(MENTAgedScoreVisualisation visualisation, MENTAgedScoreExtractorResult set, IEnumerable<ColumnsToCategoryIndex> categoryIndexMap)
			: base(visualisation, set, categoryIndexMap)
		{
			Argument.NotNull(visualisation, nameof(visualisation));
			Argument.NotNull(set, nameof(set));
			Argument.NotNull(categoryIndexMap, nameof(categoryIndexMap));
		}

		protected override Series CreateSeriesCore(SeriesAndSeriesData series)
		{
			var title = string.IsNullOrEmpty(series.Name) ? GetInvalidSeriesName() : series.Name;

			var columnSeries = new BarSeries
			{
				BarWidth = 100,
				Background = OxyColors.Automatic,
				Title = title
			};

			foreach (var val in series.Points)
			{
				columnSeries.Items.Add(new BarItem((double)val.Y, val.X));
			}

			columnSeries.FillColor = OxyColors.Automatic;

			return columnSeries;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using OxyPlot.Series;

namespace Enterprise.PAVE.MENT.Business
{
	class InvalidSeriesGenerator : SeriesGeneratorBase
	{
		internal InvalidSeriesGenerator(MENTAgedScoreVisualisation visualisation, MENTAgedScoreExtractorResult set, IEnumerable<ColumnsToCategoryIndex> categoryIndexMap)
			: base(visualisation, set, categoryIndexMap)
		{
			Argument.NotNull(visualisation, nameof(visualisation));
			Argument.NotNull(set, nameof(set));
			Argument.NotNull(categoryIndexMap, nameof(categoryIndexMap));
		}

		public override IEnumerable<Series> GenerateSeries()
		{
			ErrorReporter.ReportOnce("We should never have an invalid series Generator used");
			return Enumerable.Empty<Series>();
		}

		protected override Series CreateSeriesCore(SeriesAndSeriesData series)
		{
			throw new NotImplementedException();
		}
	}
}

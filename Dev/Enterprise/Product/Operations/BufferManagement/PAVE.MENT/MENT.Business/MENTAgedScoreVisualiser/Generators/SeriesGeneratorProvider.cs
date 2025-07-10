using System.Collections.Generic;
using CargoWise.Common;

namespace Enterprise.PAVE.MENT.Business
{
	static class SeriesGeneratorProvider
	{
		public static ISeriesGenerator GetSeriesGenerator(MENTAgedScoreVisualisation visualisation, MENTAgedScoreExtractorResult set, IEnumerable<ColumnsToCategoryIndex> categoryIndexMap)
		{
			Argument.NotNull(categoryIndexMap, nameof(categoryIndexMap));
			Argument.NotNull(visualisation, nameof(visualisation));
			Argument.NotNull(set, nameof(set));

			ISeriesGenerator generator;

			switch (visualisation.MVI_GraphType)
			{
				case GraphTypes.Codes.Column:
					generator = new ColumnSeriesGenerator(visualisation, set, categoryIndexMap);
					break;
				case GraphTypes.Codes.Line:
					generator = new LineSeriesGenerator(visualisation, set, categoryIndexMap);
					break;
				default:
					generator = new InvalidSeriesGenerator(visualisation, set, categoryIndexMap);
					break;
			}

			return generator;
		}
	}
}

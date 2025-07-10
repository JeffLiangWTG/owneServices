using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using OxyPlot.Series;

namespace Enterprise.PAVE.MENT.Business
{
	abstract class SeriesGeneratorBase : ISeriesGenerator
	{
		protected SeriesGeneratorBase(MENTAgedScoreVisualisation visualisation, MENTAgedScoreExtractorResult set, IEnumerable<ColumnsToCategoryIndex> categoryIndexMap)
		{
			Argument.NotNull(visualisation, nameof(visualisation));
			Argument.NotNull(set, nameof(set));
			Argument.NotNull(categoryIndexMap, nameof(categoryIndexMap));

			this.visualisation = visualisation;
			this.set = set;
			this.categoryIndexMap = categoryIndexMap;
		}

		protected readonly MENTAgedScoreVisualisation visualisation;
		protected readonly MENTAgedScoreExtractorResult set;
		protected readonly IEnumerable<ColumnsToCategoryIndex> categoryIndexMap;

		public virtual IEnumerable<Series> GenerateSeries()
		{
			var filteredSeries = GetFilteredSeries();

			var min = filteredSeries.DefaultIfEmpty(new MENTDataPoint(string.Empty, 0, string.Empty, string.Empty)).Min(r => r.YValue);
			var max = filteredSeries.DefaultIfEmpty(new MENTDataPoint(string.Empty, 0, string.Empty, string.Empty)).Max(r => r.YValue);

			var groupedSeries = filteredSeries.GroupBy(s => s.VisibleLabel);

			var alignedSeries = GetAlignedSeries(groupedSeries, max, min);

			return CreateSeries(alignedSeries);
		}

		IEnumerable<MENTDataPoint> GetFilteredSeries()
		{
			Argument.NotNull(set, nameof(set));

			return set.ExtractionResults.SelectMany(er => er.Results
				.Select(r => new MENTDataPoint(r.XCategoryString, r.YValue, r.XSeriesString, er.Name)))
				.Where(s => categoryIndexMap
					.Any(c => c.ColumnsToMap
						.Any(innerCategory => innerCategory == s.XCategoryLabel)))
				.AsParallel().AsOrdered();
		}

		IEnumerable<Series> CreateSeries(IEnumerable<SeriesAndSeriesData> alignedSeries)
		{
			foreach (var series in alignedSeries)
			{
				yield return CreateSeriesCore(series);
			}
		}

		protected abstract Series CreateSeriesCore(SeriesAndSeriesData series);

		IEnumerable<SeriesAndSeriesData> GetAlignedSeries(IEnumerable<IGrouping<string, MENTDataPoint>> groupedSeries, decimal maxSetValue, decimal minSetValue)
		{
			var range = maxSetValue - minSetValue;

			foreach (var grouping in groupedSeries)
			{
				var rows = new Collection<XYDataPoint>();
				foreach (var cat in categoryIndexMap)
				{
					var matchedValues = grouping.Where(x => cat.ColumnsToMap.Any(innerCategory => innerCategory == x.XCategoryLabel));

					if (matchedValues.Any())
					{
						var itemToAdd = matchedValues.Sum(y => y.YValue);
						if (!visualisation.IsNormalised)
						{
							rows.Add(new XYDataPoint(cat.Index, itemToAdd));
						}
						else
						{
							var normalisedY = range == 0 ? 100 : (itemToAdd - minSetValue) / range * 100;
							rows.Add(new XYDataPoint(cat.Index, normalisedY));
						}
					}
				}

				yield return new SeriesAndSeriesData(grouping.Key, rows);
			}
		}

		protected string GetInvalidSeriesName()
		{
			return Res.GetString("340ffef1-1e92-4918-8b97-0af1b7bf2376", "Unnamed");
		}

		protected class SeriesAndSeriesData
		{
			public SeriesAndSeriesData(string name, Collection<XYDataPoint> points)
			{
				this.Name = name;
				this.Points = points;
			}

			public string Name { get; private set; }

			public Collection<XYDataPoint> Points { get; private set; }
		}

		protected class XYDataPoint
		{
			public XYDataPoint(int x, decimal y)
			{
				this.Y = y;
				this.X = x;
			}

			public int X { get; set; }

			public decimal Y { get; set; }
		}
	}
}

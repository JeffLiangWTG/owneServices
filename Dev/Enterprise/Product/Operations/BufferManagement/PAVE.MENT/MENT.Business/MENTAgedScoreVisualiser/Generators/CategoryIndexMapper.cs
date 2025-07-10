using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.PAVE.MENT.Business
{
	public class CategoryIndexMapper
	{
		public CategoryIndexMapper(MENTAgedScoreVisualisation visualisation)
		{
			Argument.NotNull(visualisation, nameof(visualisation));

			this.visualisation = visualisation;
		}

		readonly MENTAgedScoreVisualisation visualisation;

		public IEnumerable<ColumnsToCategoryIndex> Map(MENTAgedScoreExtractorResult set)
		{
			Argument.NotNull(set, nameof(set));

			if (!visualisation.UseOverriddenCategorySequence)
			{
				return set.ResultCategoryIndexMap;
			}
			else if (visualisation.PerformLowerBoundAggregation || visualisation.PerformUpperBoundAggregation)
			{
				var overriddenCategoryColumns = visualisation.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().Where(s => s.Selected).OrderBy(s => s.Sequence).ThenBy(s => s.ColumnDisplay);
				var lowerBoundColumn = visualisation.PerformLowerBoundAggregation ? CreateLowerBoundColumns(overriddenCategoryColumns) : null;
				var middleBoundColumns = CreateMiddleBoundColumns(overriddenCategoryColumns);
				var upperBoundColumn = visualisation.PerformUpperBoundAggregation ? CreateUpperBoundColumns(overriddenCategoryColumns, middleBoundColumns) : null;

				return Enumerable.Repeat(lowerBoundColumn, 1).Concat(middleBoundColumns).Concat(Enumerable.Repeat(upperBoundColumn, 1)).WhereNotNull();
			}
			else
			{
				return visualisation.CategorySequenceCollection.Cast<VisualisationColumnSpecification>().Where(s => s.Selected).OrderBy(s => s.Sequence).ThenBy(s => s.ColumnDisplay).Select((s, index) => new ColumnsToCategoryIndex(new List<string>
				{
					s.Column
				}, index, s.ColumnDisplay));
			}
		}

		ColumnsToCategoryIndex CreateLowerBoundColumns(IOrderedEnumerable<VisualisationColumnSpecification> overriddenCategoryColumns)
		{
			Argument.NotNull(overriddenCategoryColumns, nameof(overriddenCategoryColumns));

			var columnsToCombine = overriddenCategoryColumns.Where(s => s.Sequence <= visualisation.LowerBoundAggregationSequence);

			if (columnsToCombine.Any())
			{
				return new ColumnsToCategoryIndex(columnsToCombine.Select(c => c.Column.ToString()), 0, columnsToCombine.Last().ColumnDisplay);
			}

			return null;
		}

		IEnumerable<ColumnsToCategoryIndex> CreateMiddleBoundColumns(IOrderedEnumerable<VisualisationColumnSpecification> overriddenCategoryColumns)
		{
			Argument.NotNull(overriddenCategoryColumns, nameof(overriddenCategoryColumns));

			Func<VisualisationColumnSpecification, bool> middleBoundWhereFunc = null;

			if (visualisation.PerformLowerBoundAggregation && visualisation.PerformUpperBoundAggregation)
			{
				middleBoundWhereFunc = c => c.Sequence > visualisation.LowerBoundAggregationSequence && c.Sequence < visualisation.UpperBoundAggregationSequence;
			}
			else if (visualisation.PerformLowerBoundAggregation)
			{
				middleBoundWhereFunc = c => c.Sequence > visualisation.LowerBoundAggregationSequence;
			}
			else if (visualisation.PerformUpperBoundAggregation)
			{
				middleBoundWhereFunc = c => c.Sequence < visualisation.UpperBoundAggregationSequence;
			}
			else
			{
				throw new InvalidOperationException("If neither lower or upper bound aggregation are occuring then don't use this.");
			}

			return overriddenCategoryColumns.Where(middleBoundWhereFunc).Select((s, index) => new ColumnsToCategoryIndex(new string[]
			{
				s.Column
			}, visualisation.PerformLowerBoundAggregation ? index + 1 : index, s.ColumnDisplay));
		}

		ColumnsToCategoryIndex CreateUpperBoundColumns(IOrderedEnumerable<VisualisationColumnSpecification> overriddenCategoryColumns, IEnumerable<ColumnsToCategoryIndex> middleBoundColumns)
		{
			Argument.NotNull(overriddenCategoryColumns, nameof(overriddenCategoryColumns));
			Argument.NotNull(middleBoundColumns, nameof(middleBoundColumns));

			var columnsToCombine = overriddenCategoryColumns.Where(s => s.Sequence >= visualisation.UpperBoundAggregationSequence);

			if (columnsToCombine.Any())
			{
				return new ColumnsToCategoryIndex(columnsToCombine.Select(c => c.Column.ToString()),
					middleBoundColumns.Any() ? middleBoundColumns.Max(c => c.Index) + 1 : visualisation.PerformLowerBoundAggregation ? 1 : 0,
					columnsToCombine.First().ColumnDisplay);
			}

			return null;
		}
	}
}

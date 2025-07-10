using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Module
{
	public abstract class TagSubGroup<T> : ModuleFilterSubGroup
	{
		protected TagSubGroup(ProcessHeaderFilterBusinessObject processHeaderFilter, string filterDescription)
		{
			this.processHeaderFilter = processHeaderFilter;
			this.filterDescription = filterDescription;
		}

		readonly ProcessHeaderFilterBusinessObject processHeaderFilter;
		readonly string filterDescription;

		protected abstract T CreateProperty(ModuleGuidAppliedToSubCollectionFilter filter);
		protected abstract ZQuery CreateQuery(List<T> tagQueryProperties, ProcessHeaderFilterBusinessObject filterObject);

		public override ZQuery GetSubQuery(ZQuery filter)
		{
			var tagFilters = processHeaderFilter.ActiveModuleFilters.OfType<ModuleGuidAppliedToSubCollectionFilter>().Where(x => x.Description.StartsWith(filterDescription, StringComparison.Ordinal)).ToArray();

			if (tagFilters.Any())
			{
				var result = new ZQuery();
				tagFilters = tagFilters.Where(t => t.GroupName == CurrentlyProcessedGroup).ToArray();
				var categories = tagFilters.Where(t => !CategoriesToIgnore.Contains(t.OrCategory)).Select(t => t.OrCategory).Distinct();

				if (CurrentlyProcessedFilterOrCategory != FilterOrCategory.None)
				{
					return ProcessCategory(CurrentlyProcessedFilterOrCategory, tagFilters);
				}

				foreach (var category in categories)
				{
					result.AddToFilter(ProcessCategory(category, tagFilters));
				}
				return result;
			}
			return filter;
		}

		protected virtual ZQuery ProcessCategory(FilterOrCategory category, IEnumerable<ModuleGuidAppliedToSubCollectionFilter> tagFilters)
		{
			var result = new ZQuery();
			var properties = tagFilters
					.Where(f => f.OrCategory == category)
					.Select(CreateProperty).ToList();

			if (category == FilterOrCategory.None)
			{
				properties.ForEach(p => result.AddToFilter(CreateQuery(new List<T> { p }, processHeaderFilter)));
			}
			else
			{
				result.AddToFilter(CreateQuery(properties, processHeaderFilter));
			}
			return result;
		}

		protected virtual bool NotIn(SQLComparisonOperator comparisonOperator)
		{
			if (comparisonOperator == SQLComparisonOperator.Contains || comparisonOperator == SQLComparisonOperator.Equal)
			{
				return false;
			}
			else if (comparisonOperator == SQLComparisonOperator.NotContains || comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				return true;
			}
			else
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, comparisonOperatorError, comparisonOperator));
			}
		}

		protected virtual bool IncludeInherited(SQLComparisonOperator comparisonOperator)
		{
			if (comparisonOperator == SQLComparisonOperator.Equal || comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				return true;
			}
			else if (comparisonOperator == SQLComparisonOperator.Contains || comparisonOperator == SQLComparisonOperator.NotContains)
			{
				return false;
			}
			else
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, comparisonOperatorError, comparisonOperator));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		const string comparisonOperatorError = "Comparison Operator '{0}' is invalid for this conversion. Accept 'Contains' and 'Not Contains' operators only.";
	}
}

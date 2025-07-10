using System.Collections.Generic;
using Enterprise.DocumentEngine.Business;

namespace Enterprise.DocumentEngine.DocBuilder
{
	internal class SectionFiltersEvaluator
	{
		internal static IEnumerable<string> GetEvaluatedSectionFilters(IDocumentConfig documentConfig, FilterEvaluator filterEvaluator)
		{
			return new SectionFiltersEvaluator(documentConfig, filterEvaluator).GetEvaluatedSectionFilters();
		}

		SectionFiltersEvaluator(IDocumentConfig documentConfig, FilterEvaluator filterEvaluator)
			: this(filterEvaluator)
		{
			if (documentConfig != null)
			{
				this.documentConfig = documentConfig;
				EvaluateAllConfigItems();
			}
		}

		internal SectionFiltersEvaluator(FilterEvaluator filterEvaluator)
		{
			this.filterEvaluator = filterEvaluator ?? new FilterEvaluator();
			evaluatedSectionFilters = new List<string>();
		}

		readonly IDocumentConfig documentConfig;
		readonly FilterEvaluator filterEvaluator;
		readonly List<string> evaluatedSectionFilters;

		void EvaluateAllConfigItems()
		{
			foreach (IDocumentConfigItem item in documentConfig.ConfigItems)
			{
				EvaluateConfigItem(item);
			}
		}

		internal void EvaluateConfigItem(IDocumentConfigItem item)
		{
			item.EvaluatedValue = filterEvaluator.MatchesFilter(item.FilterList);
			evaluatedSectionFilters.Add(string.Join("+", item.SectionName, item.EvaluatedValue.ToString()));
		}

		internal IEnumerable<string> GetEvaluatedSectionFilters()
		{
			return evaluatedSectionFilters;
		}
	}
}

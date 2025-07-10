using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Core
{
	public class SearchBoxFilter : ISearchBoxFilter
	{
		public SearchBoxFilter(SchemaStringColumn searchPhraseColumn)
		{
			this.searchPhraseColumn = searchPhraseColumn;
			MaximumRows = -1;
		}

		public int MaximumRows { get; set; }

		public void ApplySearch(IBusinessObjectCollection collection, string searchPhrase)
		{
			var filter = new ZQuery();

			if (!string.IsNullOrEmpty(searchPhrase))
			{
				filter.AddToFilter(searchPhraseColumn, SQLComparisonOperator.Contains, searchPhrase);
			}

			if (MaximumRows > 0)
			{
				filter.MaximumRows = MaximumRows;
			}

			if (collection is BusinessObjectCollection bizoCollection)
			{
				bizoCollection.LoadWithMoreFiltering(filter);
			}
			else if (collection is IActiveBusinessObjectCollection activeBizoCollection)
			{
				activeBizoCollection.AdditionalFilter = filter;
			}
		}

		public void ApplySearch(IList collection, string searchPhrase) => ApplySearch((IBusinessObjectCollection)collection, searchPhrase);

		protected SchemaStringColumn searchPhraseColumn;
	}
}

using System.Collections;

namespace Enterprise.ZArchitecture.Core
{
	public interface ISearchBoxFilter
	{
		int MaximumRows { get; set; }
		void ApplySearch(IList collection, string searchPhrase);
	}
}

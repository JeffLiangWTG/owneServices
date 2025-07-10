using System.Collections;

namespace Enterprise.Integration.UniversalCopy
{
	public interface IUniversalCopyTypeCollectionSorter
	{
		string ItemsTableName { get; }

		IEnumerable GetSortedCollection(IEnumerable collection);
	}
}

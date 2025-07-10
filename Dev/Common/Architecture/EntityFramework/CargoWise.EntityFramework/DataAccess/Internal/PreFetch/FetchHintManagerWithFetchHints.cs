using System.Collections.Generic;

namespace CargoWise.EntityFramework;

internal class FetchHintManagerWithFetchHints : FetchHintManager
{
	readonly Dictionary<string, int> LoadedFetchCounts;

	public FetchHintManagerWithFetchHints(RowFactory factory) : base(factory)
	{
		LoadedFetchCounts = new Dictionary<string, int>();
	}

	public override void FetchTable(string tableName)
	{
		FetchTable(tableName, out var loadedFetchHints);

		if (LoadedFetchCounts.ContainsKey(tableName))
		{
			LoadedFetchCounts[tableName] += loadedFetchHints;
		}
		else
		{
			LoadedFetchCounts.Add(tableName, loadedFetchHints);
		}
	}

	public override int GetLoadedFetchHintCountForTable(string tableName)
	{
		if (LoadedFetchCounts.TryGetValue(tableName, out var count))
		{
			return count;
		}

		return 0;
	}

	public override void ClearLoadedFetchHintCountForTable(string tableName)
	{
		LoadedFetchCounts.Remove(tableName);
	}
}

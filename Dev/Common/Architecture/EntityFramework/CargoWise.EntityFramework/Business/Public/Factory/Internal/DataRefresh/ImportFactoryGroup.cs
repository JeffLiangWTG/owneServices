using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	class ImportFactoryGroup
	{
		public static ImportFactoryGroup CreateGroup(ICollection<BusinessObjectFactory> incomingFactories)
		{
			var group = new ImportFactoryGroup(incomingFactories);
			foreach (var incomingFactory in incomingFactories)
			{
				incomingFactory.ThreadSentry.EnsureCurrentThreadIsOwner();
				incomingFactory.ImportFactoryGroups.Add(group);
			}
			return group;
		}

		ImportFactoryGroup(ICollection<BusinessObjectFactory> incomingFactories)
		{
			factories = incomingFactories.ToImmutableHashSet();
			importedRows = new ConcurrentDictionary<ZGuid, string>(); // PK & tablename.
		}

		readonly ConcurrentDictionary<ZGuid, string> importedRows;
		readonly ImmutableHashSet<BusinessObjectFactory> factories;

		public IEnumerable<BusinessObjectFactory> Factories => factories;

		public IEnumerable<KeyValuePair<ZGuid, string>> Rows => importedRows;

		public int Count => importedRows.Count;

		public bool ContainsAllFactories(ICollection<BusinessObjectFactory> factoriesToCheck)
		{
			return factories.Count == factoriesToCheck.Count
				&& factoriesToCheck.All(f => factories.Contains(f));
		}

		public bool Contains(ZGuid pk) => importedRows.ContainsKey(pk);

		public void Remove(ZGuid pk) => importedRows.TryRemove(pk, out var _);

		public void Add(ZGuid pk, string tableName) => importedRows.TryAdd(pk, tableName);

		public void Clear() => importedRows.Clear();
	}
}

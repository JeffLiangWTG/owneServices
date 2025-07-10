using System;
using System.Collections;
using System.Collections.Generic;

namespace Enterprise.AlwaysOn.Setup
{
	public class DatabaseFileCollection : ICollection<DatabaseFile>, IComparable
	{
		public void Add(DatabaseFile item)
		{
			var initialCount = Count;
			internalDictionary.Add(item.LogicalName, item);

			if (Count <= initialCount)
			{
				throw new InvalidOperationException("Could not add item to the dictionary");
			}
		}

		public void Clear()
		{
			internalDictionary.Clear();

			if (Count != 0)
			{
				throw new InvalidOperationException("Items are still in internalDictionary");
			}
		}

		public bool Contains(DatabaseFile item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(DatabaseFile[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public int Count
		{
			get { return internalDictionary.Keys.Count; }
		}

		public bool IsReadOnly
		{
			get { throw new NotImplementedException(); }
		}

		public bool Remove(DatabaseFile item)
		{
			var initialCount = Count;
			var result = internalDictionary.Remove(item.LogicalName);

			if ((result && Count != initialCount - 1) || Count > initialCount)
			{
				throw new InvalidOperationException("Some error");
			}

			return result;
		}

		public IEnumerator<DatabaseFile> GetEnumerator()
		{
			return internalDictionary.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		readonly Dictionary<string, DatabaseFile> internalDictionary = new Dictionary<string, DatabaseFile>(StringComparer.OrdinalIgnoreCase);

		public int CompareTo(object obj)
		{
			var result = 0;
			var anotherDbFileCollection = obj as DatabaseFileCollection;

			if (anotherDbFileCollection == null || anotherDbFileCollection.Count != Count)
			{
				result = -1;
			}
			else
			{
				foreach (var anotherDbFile in anotherDbFileCollection)
				{
					if (
						!internalDictionary.TryGetValue(anotherDbFile.LogicalName, out var thisDbFile)
						|| !string.Equals(thisDbFile.LogicalName, anotherDbFile.LogicalName, StringComparison.OrdinalIgnoreCase)
						|| !string.Equals(thisDbFile.PhysicalName, anotherDbFile.PhysicalName, StringComparison.OrdinalIgnoreCase)
						|| thisDbFile.Type != anotherDbFile.Type
					)
					{
						result = -1;
						break;
					}
				}
			}

			return result;
		}
	}
}

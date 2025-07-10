using System.Collections.Generic;

namespace CargoWise.EntityFramework
{
	internal class DictionaryKeyedByZPropertyInfo<T>
	{
		internal DictionaryKeyedByZPropertyInfo()
		{
			dictionary = new Dictionary<string, Dictionary<BusinessObject, T>>();
		}

		public int Count
		{
			get { return dictionary.Count; }
		}

		public IEnumerable<T> GetValues()
		{
			foreach (var value in dictionary.Values)
			{
				foreach (var inner in value.Values)
				{
					yield return inner;
				}
			}
		}

		public bool TryGetValue(ZPropertyInfo info, out T result)
		{
			Dictionary<BusinessObject, T> innerDictionary;
			if (dictionary.TryGetValue(info.Name, out innerDictionary))
			{
				return innerDictionary.TryGetValue(info.BizObj, out result);
			}
			else
			{
				result = default(T);
				return false;
			}
		}

		public T this[ZPropertyInfo info]
		{
			set
			{
				Dictionary<BusinessObject, T> innerDictionary;
				if (!dictionary.TryGetValue(info.Name, out innerDictionary))
				{
					innerDictionary = new Dictionary<BusinessObject, T>(BusinessObjectComparer.Instance);
					dictionary[info.Name] = innerDictionary;
				}
				innerDictionary[info.BizObj] = value;
			}
		}

		public bool ContainsKey(ZPropertyInfo info)
		{
			Dictionary<BusinessObject, T> innerDictionary;
			return dictionary.TryGetValue(info.Name, out innerDictionary) && innerDictionary.ContainsKey(info.BizObj);
		}

		public void Remove(ZPropertyInfo info)
		{
			Dictionary<BusinessObject, T> innerDictionary;
			if (dictionary.TryGetValue(info.Name, out innerDictionary))
			{
				if (innerDictionary.Remove(info.BizObj) && innerDictionary.Count == 0)
				{
					dictionary.Remove(info.Name);
				}
			}
		}

		readonly Dictionary<string, Dictionary<BusinessObject, T>> dictionary;
	}
}

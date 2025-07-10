using System;
using System.Collections.Generic;

namespace CargoWise.Common
{
	[Serializable]
	public class DictionaryOfLists<TKey, TListItemValue> : Dictionary<TKey, List<TListItemValue>>
	{
		public DictionaryOfLists()
		{
		}

#if NETFRAMEWORK
		protected DictionaryOfLists(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif

		public void AddValues(TKey key, params TListItemValue[] values)
		{
			if (!TryGetValue(key, out var list))
			{
				this[key] = list = new List<TListItemValue>();
			}

			foreach (var value in values)
			{
				list.Add(value);
			}
		}
	}
}

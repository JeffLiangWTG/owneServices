using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public abstract class StaticKeyedCodePairListProvider<K>
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList(K key)
		{
			if (!Cache.ContainsKey(key))
			{
				AddNewKeyedListToCache(key);
			}
			return GetCombinedList(key);
		}

		void AddNewKeyedListToCache(K key)
		{
			ReadOnlyCodeDescriptionPairList newList = GetCodeDescriptionPairListCore(key);
			Cache.Add(key, newList);
		}

		protected virtual CodeDescriptionPairList GetCombinedList(K key)
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList(Cache[key]);
			if (CommonBaseList.Count > 0)
			{
				list.AddRange(CommonBaseList);
			}
			return list;
		}

		Dictionary<K, ReadOnlyCodeDescriptionPairList> Cache
		{
			get
			{
				return cache ?? (cache = new Dictionary<K, ReadOnlyCodeDescriptionPairList>());
			}
		}
		Dictionary<K, ReadOnlyCodeDescriptionPairList> cache;

		protected virtual ReadOnlyCodeDescriptionPairList GetCommonBaseListCore()
		{
			return new ReadOnlyCodeDescriptionPairList();
		}

		ReadOnlyCodeDescriptionPairList CommonBaseList
		{
			get { return commonBaseList ?? (commonBaseList = GetCommonBaseListCore()); }
		}
		ReadOnlyCodeDescriptionPairList commonBaseList;

		protected virtual ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairListCore(K key)
		{
			return new ReadOnlyCodeDescriptionPairList();
		}
	}
}

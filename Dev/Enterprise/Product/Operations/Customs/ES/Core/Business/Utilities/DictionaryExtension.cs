using System.Collections.Generic;

namespace Enterprise.Customs.ES.Business
{
	public static class DictionaryExtension
	{
		public static void AddIfNotExists(this Dictionary<string, decimal> list, string key, decimal value)
		{
			if (!list.ContainsKey(key))
			{
				list.Add(key, value);
			}
		}
	}
}

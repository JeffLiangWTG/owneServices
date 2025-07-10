using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public static class ColumnValueSetterExtensions
	{
		public static void SetValueInSpecificOrder(this Dictionary<string, ValueSetter> dictionary, IEnumerable<ZString> matchingKeysInSettingOrder)
		{
			if (dictionary != null)
			{
				var setters = new Dictionary<string, ValueSetter>(dictionary);
				if (matchingKeysInSettingOrder != null)
				{
					foreach (var matchingKey in matchingKeysInSettingOrder)
					{
						ValueSetter setter;
						if (dictionary.TryGetValue(matchingKey, out setter))
						{
							setters.Remove(matchingKey);
							if (setter != null)
							{
								setter.SetValue();
							}
						}
					}
				}

				foreach (var pair in setters)
				{
					var setter = pair.Value;
					if (setter != null)
					{
						setter.SetValue();
					}
				}
			}
		}

		public static void Add(this Dictionary<string, ValueSetter> dictionary, ValueSetter setter)
		{
			if (dictionary != null)
			{
				var key = setter.MatchingKey;
				if (dictionary.ContainsKey(key))
				{
					dictionary[key] = setter;
				}
				else
				{
					dictionary.Add(key, setter);
				}
			}
		}
	}
}

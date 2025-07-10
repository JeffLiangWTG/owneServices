using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public static class CustomiseFieldsMacroHelper
	{
		public static object GetCustomField(ICustomFieldProvider provider, string name, BusinessObjectFactory factory, bool dataLibraryFlag = true, int index = 1)
		{
			if (string.IsNullOrWhiteSpace(name)
				|| !(provider?.GetCustomBusinessObject() is CustomBusinessObject customBusinessObject))
			{
				return null;
			}

			var propertyNameDictionary = GetOrCreateCachedPropertyNameDictionary(customBusinessObject, factory);

			if (propertyNameDictionary == null)
			{
				foreach (ZPropertyInfo propertyInfo in customBusinessObject.ZPropertyInfoHash)
				{
					if (string.Compare(propertyInfo.HumanReadableName, name, StringComparison.InvariantCultureIgnoreCase) == 0)
					{
						return propertyInfo.Value;
					}
				}

				return null;
			}

			object res = null;

			if (propertyNameDictionary.TryGetValue(name, out List<string> matchedPropertyNameList))
			{
				if (dataLibraryFlag)
				{
					index = matchedPropertyNameList.Count;
				}
				if (index > matchedPropertyNameList.Count)
				{
					throw new InvalidOperationException($"The Custom Field with the name '{name}' is not a Combo Box Custom Field.");
				}
				res = customBusinessObject.GetPossiblyCustomProperty(matchedPropertyNameList[index - 1]);
			}

			return res;
		}

		public static Dictionary<string, List<string>> GetOrCreateCachedPropertyNameDictionary(CustomBusinessObject customBusinessObject, BusinessObjectFactory factory)
		{
			return factory?.GetCachedValue($"CustomPropertyNameDictionary-{customBusinessObject.PK}", () =>
			{
				var propertyNameDictionary = new Dictionary<string, List<string>>(StringComparer.InvariantCultureIgnoreCase);

				foreach (ZPropertyInfo propertyInfo in customBusinessObject.ZPropertyInfoHash)
				{
					if (!propertyNameDictionary.ContainsKey(propertyInfo.HumanReadableName))
					{
						propertyNameDictionary[propertyInfo.HumanReadableName] = new List<string>();
					}
					propertyNameDictionary[propertyInfo.HumanReadableName].Add(propertyInfo.Name);
				}

				return propertyNameDictionary;
			}, CacheStalenessPolicy.StaleOnFactorySave);
		}
	}
}

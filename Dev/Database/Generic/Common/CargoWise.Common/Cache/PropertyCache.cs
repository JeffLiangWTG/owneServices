using System;
using System.Collections.Generic;

namespace CargoWise.Common.Cache
{
	/// <summary>
	/// Simple cache for class property
	/// Example:
	/// Class TestClass
	/// {
	///		public TestClass()
	///		{
	///			PropertyCache1 = new PropertyCache<string>(getProperty: ExpensiveMethod);
	///		}
	///		PropertyCache<string> PropertyCache1 { get; }
	///		public string Property1 => PropertyCache1.Get("key1");
	/// }
	/// </summary>
	/// <typeparam name="PropertyType"></typeparam>
	public class PropertyCache<PropertyType>
	{
		public PropertyCache(Func<PropertyType> getProperty)
		{
			GetProperty = getProperty;
		}

		Func<PropertyType> GetProperty { get; }

		public PropertyType Get(string key)
		{
			if (Dictionary.TryGetValue(key, out var result))
			{
				return result;
			}

			var newProperty = GetProperty();
			Dictionary[key] = newProperty;
			return newProperty;
		}

		Dictionary<string, PropertyType> Dictionary { get; } = new Dictionary<string, PropertyType>();
	}
}

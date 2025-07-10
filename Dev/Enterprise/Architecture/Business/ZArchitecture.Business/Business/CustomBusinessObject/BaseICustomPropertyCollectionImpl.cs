using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.ZArchitecture.Business
{
	class BaseICustomPropertyCollectionImpl : ICustomPropertyCollection
	{
		readonly Dictionary<string, ICustomProperty> properties = new Dictionary<string, ICustomProperty>();

		public BaseICustomPropertyCollectionImpl(IEnumerable<ICustomProperty> properties)
		{
			this.properties = properties.ToDictionary(p => p.Identifier);
		}

		public void AddCustomProperties(IEnumerable<ICustomProperty> customProperties)
		{
			foreach (var customProperty in customProperties)
			{
				if (!properties.ContainsKey(customProperty.Identifier))
				{
					properties.Add(customProperty.Identifier, customProperty);
				}
			}
		}

		public ICustomProperty GetCustomProperty(string identifier)
		{
			if (properties.TryGetValue(identifier, out var property))
			{
				return property;
			}
			else
			{
				return null;
			}
		}

		public string[] GetIdentifiers()
		{
			return properties.Keys.ToArray();
		}

		public IEnumerable<ICustomProperty> GetProperties()
		{
			return properties.Values;
		}

		public IEnumerator<ICustomProperty> GetEnumerator()
		{
			return properties.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return properties.Values.GetEnumerator();
		}
	}
}

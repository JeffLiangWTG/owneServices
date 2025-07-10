using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.DocumentVisualizer.Business
{
	class MacroBusinessObjectPropertyCollection : CustomPropertyCollection
	{
		public MacroBusinessObjectPropertyCollection(IEnumerable<IMacroBusinessObjectProperty> properties)
		{
			if (properties != null)
			{
				foreach (var property in properties)
				{
					Add(property);
				}
			}
		}

		readonly Dictionary<string, IMacroBusinessObjectProperty> propertyMap = new Dictionary<string, IMacroBusinessObjectProperty>();

		void Add(IMacroBusinessObjectProperty property)
		{
			if (property != null
				&& !propertyMap.ContainsKey(property.PropertyName)
				&& !string.IsNullOrWhiteSpace(property.PropertyName))
			{
				Add(property.PropertyType, property.PropertyName, property.Validate, property.MetaData?.ToArray() ?? System.Array.Empty<DynamicMetaData>());

				propertyMap[property.PropertyName] = property;
			}
		}

		protected override object GetValueCore(BusinessObject cusObj, string propertyName)
		{
			return propertyMap.ContainsKey(propertyName)
				? propertyMap[propertyName].Value
				: null;
		}

		protected override bool TrySetValueCore(BusinessObject cusObj, string propertyName, object value)
		{
			if (propertyMap.ContainsKey(propertyName))
			{
				propertyMap[propertyName].Value = value;
				return true;
			}
			return false;
		}
	}
}

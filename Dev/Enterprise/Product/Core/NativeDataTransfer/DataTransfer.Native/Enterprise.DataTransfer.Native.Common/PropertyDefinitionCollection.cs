using System.Collections.Generic;
using System.Linq;
using Enterprise.DataTransfer.Native.Common.Exceptions;

namespace Enterprise.DataTransfer.Native.Common
{
	public class PropertyDefinitionCollection : IEnumerable<IPropertyDef>, IPropertyDefinitionFinder
	{
		public PropertyDefinitionCollection(string entityName, IEnumerable<IPropertyDef> propertyDefs)
		{
			this.entityName = entityName;
			this.propertyDefs = propertyDefs;
			this.propertyDefinitionsDict = propertyDefs.GroupBy(d => d.PropertyName).ToDictionary(g => g.Key, g => g.FirstOrDefault());
		}
		readonly IEnumerable<IPropertyDef> propertyDefs;
		readonly Dictionary<string, IPropertyDef> propertyDefinitionsDict;
		readonly string entityName;

		public IPropertyDef this[string propertyName]
		{
			get
			{
				var def = Find(propertyName) ?? throw new PropertyNotDefinedException(entityName);
				return def;
			}
		}

		public IPropertyDef Find(string propertyName)
		{
			return propertyDefinitionsDict.TryGetValue(propertyName, out var definition) ? definition : null;
		}

		public bool HasDefinition(string propertyName)
		{
			var def = Find(propertyName);
			return def != null;
		}

		#region IEnumerable<IPropertyDef> Members

		public IEnumerator<IPropertyDef> GetEnumerator()
		{
			return propertyDefs.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion
	}
}

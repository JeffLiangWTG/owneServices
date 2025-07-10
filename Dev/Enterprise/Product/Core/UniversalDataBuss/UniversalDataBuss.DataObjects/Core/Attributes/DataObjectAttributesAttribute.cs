using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class DataObjectAttributesAttribute : Attribute, IAttributesAttribute
	{
		public DataObjectAttributesAttribute(params string[] attributeNames)
		{
			this.attributeNames = attributeNames;
		}

		readonly string[] attributeNames;

		public List<string> GetAttributeDefinitions(Type propertyType, Func<PropertyInfo, string> getXsdType)
		{
			var attributeDefinitions = new List<string>();
			foreach (var attributeName in attributeNames)
			{
				var attributeProperty = propertyType.GetProperty(attributeName);
				var xsdType = getXsdType(attributeProperty);
				attributeDefinitions.Add(string.Format(@"<xs:attribute name=""{0}"" type=""{1}"" />", attributeName, xsdType));
			}

			return attributeDefinitions;
		}

		public bool HasAttributeDefined(string attributeName)
		{
			return attributeNames.Contains(attributeName);
		}
	}
}

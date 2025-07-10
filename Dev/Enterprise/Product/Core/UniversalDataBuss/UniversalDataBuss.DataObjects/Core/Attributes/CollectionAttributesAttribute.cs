using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public sealed class CollectionAttributesAttribute : Attribute, IAttributesAttribute
	{
		public CollectionAttributesAttribute(params string[] attributeNames)
		{
			this.attributeNames = attributeNames;
		}
		readonly string[] attributeNames;

		public string GetAttributeValues(object collection, Func<PropertyInfo, int> maxLengthReader)
		{
			var collectionType = collection.GetType();
			var attributes = new List<string>();
			foreach (var attributeName in attributeNames)
			{
				var attributeProperty = collectionType.GetProperty(attributeName);
				object attributeValue = attributeProperty.GetValue(collection, null);
				if (attributeValue != null)
				{
					var attribute = SimpleTypeFormatter.GetFormattedValueAsAttributeForWritingToXml(attributeProperty.Name, attributeValue, delegate { return maxLengthReader(attributeProperty); });
					if (attribute != null)
					{
						attributes.Add(attribute);
					}
				}
			}

			return string.Concat(attributes);
		}

		public List<string> GetAttributeDefinitions(Type collectionType, Func<PropertyInfo, string> getXsdType)
		{
			var attributeDefinitions = new List<string>();
			foreach (var attributeName in attributeNames)
			{
				var attributeProperty = collectionType.GetProperty(attributeName);
				var xsdType = getXsdType(attributeProperty);
				attributeDefinitions.Add(string.Format(@"<xs:attribute name=""{0}"" type=""{1}"" />", attributeName, xsdType));
			}

			return attributeDefinitions;
		}

		public bool HasAttributeDefined(string attributeName)
		{
			return attributeNames.Contains(attributeName);
		}

#if DEBUG
		public IEnumerable<string> GetAttributeNamesForTesting()
		{
			return attributeNames;
		}
#endif
	}
}

using System;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Common;

namespace Enterprise.DataTransfer.Native.Business.Xml
{
	public static class RelationshipResolver
	{
		public const string LocationRelationshipResolver = "LRR";

		public static string GetRelationshipValue(Property property)
		{
			return property.GetAttributeValue(TagName.Relationship);
		}

		public static XAttribute CreateXAttribute(Property property, string resolverName)
		{
			var attributeValue = ZString.Empty;

			if (resolverName == LocationRelationshipResolver)
			{
				attributeValue = GetLocationRelationshipValue(property, attributeValue);
			}

			return string.IsNullOrEmpty(attributeValue) ? null : new XAttribute(TagName.Relationship, attributeValue);
		}

		static ZString GetLocationRelationshipValue(Property property, ZString attributeValue)
		{
			var propertyValue = property.Value as String;

			if (propertyValue != null)
			{
				switch (propertyValue.Length)
				{
					case 5:
						attributeValue = "PTC";
						break;
					case 2:
						attributeValue = "COU";
						break;
					case 4:
						attributeValue = "IZN";
						break;
				}
			}
			return attributeValue;
		}
	}
}

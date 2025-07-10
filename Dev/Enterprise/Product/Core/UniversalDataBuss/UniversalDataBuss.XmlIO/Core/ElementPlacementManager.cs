using System;
using System.Reflection;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO
{
	class ElementPlacementManager
	{
		internal ElementPlacementManager(Element element)
		{
			this.ElementPlacing = GetSpecialPlacing(element.PropertyInfo) ?? element.DefaultPlacing;
			this.KeyForSorting = (int)this.ElementPlacing + (element.MinOccurs == "1" ? "M" : "O") + element.ElementName;
		}

		internal readonly string KeyForSorting;
		internal readonly PlacingWithinXml ElementPlacing;

		static PlacingWithinXml? GetSpecialPlacing(PropertyInfo propertyInfo)
		{
			if (propertyInfo != null)
			{
				foreach (Attribute attribute in propertyInfo.GetCustomAttributes(false))
				{
					var attributeType = attribute.GetType();
					if (attributeType == typeof(ReferencePropertyAttribute))
					{
						return PlacingWithinXml.References;
					}

					if (attributeType == typeof(CandidateKeyAttribute))
					{
						return PlacingWithinXml.CandidateKeys;
					}

					if (attributeType == typeof(VerticalPartitionAttribute))
					{
						return PlacingWithinXml.VerticalPartitions;
					}
				}
			}

			return null;
		}
	}
}

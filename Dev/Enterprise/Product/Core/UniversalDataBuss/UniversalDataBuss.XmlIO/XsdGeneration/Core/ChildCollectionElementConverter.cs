using System;
using System.Reflection;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration
{
	abstract class ChildCollectionElementConverter : ObjectElementConverter
	{
		protected ChildCollectionElementConverter(PropertyInfo propertyInfo, Type containedType, UniversalXsdGenerator schemaGenerator)
			: base(propertyInfo, containedType, schemaGenerator)
		{
			this.InnerElementName = GetContainedElementName(propertyInfo, ElementName);
		}

		protected readonly string InnerElementName;

		internal override PlacingWithinXml DefaultPlacing
		{
			get { return PlacingWithinXml.Collections; }
		}

		internal static string GetContainedElementName(PropertyInfo propertyInfo, string elementName)
		{
			if (!elementName.EndsWith("Collection"))
			{
				throw new XmlProcessingException(propertyInfo, "All List<DataObject> typed property names must end in 'Collection'.");
			}

			return elementName.Substring(0, elementName.Length - 10);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Reflection;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XsdGeneration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlWriting
{
	class SequenceWriter : ComplexElement, IElementWriter
	{
		internal SequenceWriter(PropertyInfo propertyInfo, Type typeOfContainedObjects, XmlBuilder builder)
			: base(propertyInfo, builder)
		{
			this.typeOfContainedObjects = typeOfContainedObjects;
			this.builder = builder;
		}

		readonly XmlBuilder builder;
		readonly Type typeOfContainedObjects;

		internal override PlacingWithinXml DefaultPlacing
		{
			get { return PlacingWithinXml.Collections; }
		}

		void IElementWriter.WriteXML(IDataObject dataStructure, DataOverrideManager overrideManager)
		{
			var elements = dataStructure != null ? PropertyInfo.GetValue(dataStructure, null) as IEnumerable<IDataObject> : null;

			if (elements != null)
			{
				builder.AddBlankLineIfNotFirstInIndentedSection();

				var attributes = GetAttributes(elements);

				builder.AddStartElementWithAttributes(ElementName, attributes);

				var childElementWriter = (IElementWriter)GetNewObjectElement(PropertyInfo, typeOfContainedObjects);

				foreach (var element in elements)
				{
					childElementWriter.WriteXML(element, overrideManager);
				}

				builder.AddEndElement(ElementName);
			}
		}

		string[] GetAttributes(object collection)
		{
			var attributeSource = collection.GetAttribute<CollectionAttributesAttribute>();
			if (attributeSource != null)
			{
				return new [] { attributeSource.GetAttributeValues(collection, (propertyInfo) => builder.GetMaxLengthCached(propertyInfo)) };
			}

			return Array.Empty<string>();
		}

		protected override Element GetNewComplexElement(PropertyInfo propertyInfo, Type calculatedPropertyType)
		{
			var elementName = ChildCollectionElementConverter.GetContainedElementName(PropertyInfo, ElementName);
			return new ComplexTypeWriter(elementName, calculatedPropertyType, builder);
		}

		protected override Element GetNewFlattenedElement(PropertyInfo propertyInfo, Type calculatedPropertyType, string baseElementPropertyName)
		{
			return new SimpleTypeWithAttributesWriter(propertyInfo, calculatedPropertyType, baseElementPropertyName, builder, true);
		}
	}
}

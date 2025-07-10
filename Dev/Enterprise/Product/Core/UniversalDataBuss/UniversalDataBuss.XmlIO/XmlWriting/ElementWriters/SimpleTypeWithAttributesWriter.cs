using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.XmlIO.XsdGeneration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlWriting
{
	class SimpleTypeWithAttributesWriter : Element, IElementWriter
	{
		public SimpleTypeWithAttributesWriter(PropertyInfo propertyInfo, Type calculatedPropertyType, string baseElementPropertyName, XmlBuilder builder, bool isCollection = false)
			: base(propertyInfo, builder)
		{
			this.baseElementPropertyName = baseElementPropertyName;
			this.builder = builder;
			this.elementType = isCollection ? ElementType.List : ElementType.Property;
		}

		readonly string baseElementPropertyName;
		readonly XmlBuilder builder;
		readonly ElementType elementType;

		enum ElementType { List, Property }

		internal override PlacingWithinXml DefaultPlacing
		{
			get { return PlacingWithinXml.FieldsAndRelatedObjects; }
		}

		public void WriteXML(IDataObject dataStructure, DataOverrideManager overrideManager)
		{
			var dataToWrite = elementType == ElementType.Property && dataStructure != null
				? (IDataObject)PropertyInfo.GetValue(dataStructure, null)
				: dataStructure;

			if (dataToWrite == null)
			{
				return;
			}

			WriteSimpleTypeXML(dataToWrite, builder);

			if (elementType == ElementType.Property)
			{
				overrideManager.QueueForWritingLaterOnParent(dataToWrite,
					dataStructure,
					PropertyInfo.Name,
					WriteSimpleTypeXML);
			}
		}

		void WriteSimpleTypeXML(IDataObject dataToWrite, XmlBuilder xmlBuilder)
		{
			string formattedBaseValue = null;
			var attributes = new List<string>();

			foreach (var propertyInfo in dataToWrite.GetType().GetProperties().OrderBy(o => o.Name))
			{
				var propertyType = propertyInfo.PropertyType;

				if (!propertyType.IsGenericType)
				{
					continue;
				}

				var genericType = propertyType.GetGenericTypeDefinition();

				if (genericType != typeof(Nullable<>))
				{
					continue;
				}

				object value = propertyInfo.GetValue(dataToWrite, null);

				var isBaseElementProperty = propertyInfo.Name == baseElementPropertyName;

				if (value != null)
				{
					if (isBaseElementProperty)
					{
						formattedBaseValue = SimpleTypeFormatter.GetFormattedValueForWritingToXml(value, delegate { return xmlBuilder.GetMaxLengthCached(propertyInfo); });
					}
					else
					{
						var attribute = SimpleTypeFormatter.GetFormattedValueAsAttributeForWritingToXml(propertyInfo.Name, value, delegate { return xmlBuilder.GetMaxLengthCached(propertyInfo); });
						if (attribute != null)
						{
							attributes.Add(attribute);
						}
					}
				}
			}

			if (formattedBaseValue != null)
			{
				string elementName = elementType == ElementType.Property ? PropertyInfo.Name : ChildCollectionElementConverter.GetContainedElementName(PropertyInfo, PropertyInfo.Name);
				xmlBuilder.AddElementWithValueWithAttributes(elementName, formattedBaseValue, string.Concat(attributes));
			}
		}
	}
}

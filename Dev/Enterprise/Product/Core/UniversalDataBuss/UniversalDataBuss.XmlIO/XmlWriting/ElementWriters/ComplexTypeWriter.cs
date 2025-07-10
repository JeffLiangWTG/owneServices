using System;
using System.Reflection;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XmlWriting
{
	class ComplexTypeWriter : ElementWithChildren, IElementWriter
	{
		internal ComplexTypeWriter(string name, Type typeContainingChildren, XmlBuilder builder)
			: base(name, typeContainingChildren, builder)
		{
			this.builder = builder;
		}

		internal ComplexTypeWriter(PropertyInfo propertyInfo, Type calculatedPropertyType, XmlBuilder builder)
			: base(propertyInfo, calculatedPropertyType, builder)
		{
			this.builder = builder;
		}

		readonly XmlBuilder builder;

		protected override Element GetNewFieldElement(PropertyInfo propertyInfo)
		{
			return new SimpleTypeWriter(propertyInfo, builder);
		}

		protected override Element GetNewFlattenedElement(PropertyInfo propertyInfo, Type calculatedPropertyType, string baseElementPropertyName)
		{
			return new SimpleTypeWithAttributesWriter(propertyInfo, calculatedPropertyType, baseElementPropertyName, builder);
		}

		protected override Element GetNewComplexElement(PropertyInfo propertyInfo, Type calculatedPropertyType)
		{
			return new ComplexTypeWriter(propertyInfo, calculatedPropertyType, builder);
		}

		protected override Element GetNewListElement(PropertyInfo propertyInfo, Type typeOfContainedObjects)
		{
			return new SequenceWriter(propertyInfo, typeOfContainedObjects, builder);
		}

		internal override PlacingWithinXml DefaultPlacing
		{
			get { return PlacingWithinXml.FieldsAndRelatedObjects; }
		}

		public void WriteXML(IDataObject dataStructure, DataOverrideManager overrideManager)
		{
			var dataToWrite = PropertyInfo != null && dataStructure != null
				? (IDataObject)PropertyInfo.GetValue(dataStructure, null)
				: dataStructure;

			if (dataToWrite == null)
			{
				return;
			}

			if (ElementPlacing == PlacingWithinXml.VerticalPartitions)
			{
				builder.AddBlankLineIfNotFirstInIndentedSection();
			}

			builder.AddStartElement(ElementName);

			using (overrideManager.OverrideWriter(dataToWrite))
			{
				var propertyType = dataToWrite.GetType();

				if (TypeForFields == typeof(IDataObject))
				{
					new UntypedElementWriter(PropertyInfo.Name, propertyType, builder).WriteXML(dataToWrite, overrideManager);
				}
				else
				{
					foreach (IElementWriter writer in ChildElements)
					{
						writer.WriteXML(dataToWrite, overrideManager);

						if (writer.ElementPlacing == PlacingWithinXml.References)
						{
							builder.AddBlankLine();
						}
					}
				}
			}

			builder.AddEndElement(ElementName);
		}

		ElementList<Element> childElements;
		ElementList<Element> ChildElements
		{
			get { return childElements ?? (childElements = GetNewChildElements()); }
		}

		ElementList<Element> GetNewChildElements()
		{
			var result = new ElementList<Element>();
			ReflectOutChildren(result);
			result.Sort();
			return result;
		}
	}
}



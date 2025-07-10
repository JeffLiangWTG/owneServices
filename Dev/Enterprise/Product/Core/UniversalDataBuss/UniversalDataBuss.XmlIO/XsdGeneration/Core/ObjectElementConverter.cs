using System;
using System.Reflection;
using CargoWise.Common;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration
{
	internal abstract class ObjectElementConverter : ElementWithChildren, IElementConverter
	{
		protected ObjectElementConverter(PropertyInfo propertyInfo, Type typeForFields, UniversalXsdGenerator schemaGenerator)
			: base(propertyInfo, typeForFields, schemaGenerator)
		{
			Initialise(typeForFields, schemaGenerator);
		}

		protected ObjectElementConverter(string name, Type typeForFields, UniversalXsdGenerator schemaGenerator)
			: base(name, typeForFields, schemaGenerator)
		{
			Initialise(typeForFields, schemaGenerator);
		}

		void Initialise(Type typeForFields, UniversalXsdGenerator schemaGenerator)
		{
			this.schemaGenerator = schemaGenerator;
			Argument.NotNull(typeForFields, "Type typeForFields");
		}

		protected UniversalXsdGenerator schemaGenerator;

		internal override PlacingWithinXml DefaultPlacing
		{
			get { return PlacingWithinXml.FieldsAndRelatedObjects; }
		}

		internal void ReflectOutChildConverters()
		{
			childConverters = new ElementList<Element>();
			ReflectOutChildren(childConverters);
			childConverters.Sort();
			foreach (var childConverter in childConverters)
			{
				if (childConverter == this)
				{
					throw new InvalidOperationException("Cannot have an ObjectElementConverter instance as a child of itself.");
				}

				var converterWithChildren = childConverter as ObjectElementConverter;
				if (converterWithChildren != null)
				{
					converterWithChildren.ReflectOutChildConverters();
				}
			}
		}

		ElementList<Element> childConverters;

		public void WriteToList(XsdBuilder result)
		{
			var elementPlacing = ElementPlacing;
			if (elementPlacing == PlacingWithinXml.VerticalPartitions || elementPlacing == PlacingWithinXml.Collections)
			{
				result.AddBlankLineIfNotFirstInIndentedSection();
			}

			WriteHeader(result);

			foreach (IElementConverter childConverter in childConverters)
			{
				childConverter.WriteToList(result);
			}

			WriteFooter(result);

			if (elementPlacing == PlacingWithinXml.References)
			{
				result.AddBlankLine();
			}
		}

		protected abstract void WriteHeader(XsdBuilder result);
		protected abstract void WriteFooter(XsdBuilder result);

		protected override Element GetNewFieldElement(PropertyInfo propertyInfo)
		{
			return new DataFieldElementConverter(propertyInfo, schemaGenerator);
		}

		protected override Element GetNewComplexElement(PropertyInfo propertyInfo, Type calculatedPropertyType)
		{
			if (calculatedPropertyType == typeof(IDataObject))
			{
				return new UntypedElementConverter(propertyInfo, schemaGenerator);
			}
			else
			{
				var schemaInfo = schemaGenerator.GetXsdSchemaInfoCheckingForDuplicateInnerTypes(calculatedPropertyType, propertyInfo);
				if (schemaInfo.Placement == Placement.Inner)
				{
					return new RelatedObjectInnerElementConverter(propertyInfo, calculatedPropertyType, schemaGenerator);
				}
				else
				{
					return new RelatedObjectOuterElementConverter(propertyInfo, calculatedPropertyType, schemaGenerator);
				}
			}
		}

		protected override Element GetNewFlattenedElement(PropertyInfo propertyInfo, Type calculatedPropertyType, string baseElementPropertyName)
		{
			var schemaInfo = schemaGenerator.GetXsdSchemaInfoCheckingForDuplicateInnerTypes(calculatedPropertyType, propertyInfo);
			if (schemaInfo.Placement == Placement.Inner)
			{
				return new FlattenedRelatedObjectInnerElementConverter(propertyInfo, calculatedPropertyType, schemaGenerator);
			}
			else
			{
				return new FlattenedRelatedObjectOuterElementConverter(propertyInfo, calculatedPropertyType, schemaGenerator);
			}
		}

		protected override Element GetNewListElement(PropertyInfo propertyInfo, Type typeOfContainedObjects)
		{
			var schemaInfo = schemaGenerator.GetXsdSchemaInfoCheckingForDuplicateInnerTypes(typeOfContainedObjects, propertyInfo);
			if (schemaInfo.Placement == Placement.Inner)
			{
				return new ChildCollectionInnerElementConverter(propertyInfo, typeOfContainedObjects, schemaGenerator);
			}
			else
			{
				return new ChildCollectionOuterElementConverter(propertyInfo, typeOfContainedObjects, schemaGenerator);
			}
		}

		protected void AddAttributesIfPresent<T>(Type propertyType, XsdBuilder builder)
			where T : Attribute, IAttributesAttribute
		{
			var attributeSource = propertyType.GetAttribute<T>();
			if (attributeSource != null)
			{
				attributeSource.GetAttributeDefinitions(propertyType, schemaGenerator.GetSimpleXmlType).ForEach(o => builder.Add(o));
			}
		}
	}
}

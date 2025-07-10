using System;
using System.Reflection;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration
{
	internal class FlattenedRelatedObjectElementConverter : ObjectElementConverter
	{
		internal FlattenedRelatedObjectElementConverter(Type type, UniversalXsdGenerator schemaGenerator)
			: base(type.Name, type, schemaGenerator)
		{
			flattenedAttribute = type.GetAttribute<FlattenedIntoAttributesAttribute>();
		}

		protected FlattenedRelatedObjectElementConverter(PropertyInfo propertyInfo, Type calculatedPropertyType, UniversalXsdGenerator schemaGenerator)
			: base(propertyInfo, calculatedPropertyType, schemaGenerator)
		{
			flattenedAttribute = TypeForFields.GetAttribute<FlattenedIntoAttributesAttribute>();
		}

		readonly FlattenedIntoAttributesAttribute flattenedAttribute;

		protected virtual void WriteOpening(XsdBuilder result)
		{
			result.Add(@"<xs:complexType name=""{0}"">", TypeForFields.Name);
		}

		protected override void WriteHeader(XsdBuilder result)
		{
			WriteOpening(result);

			var baseElementPropertyInfo = TypeForFields.GetProperty(flattenedAttribute.BaseElementPropertyName);
			string baseXmlType = schemaGenerator.GetSimpleXmlType(baseElementPropertyInfo); // Add some logic here to add restrictions to string lengths etc.

			result.Add(@"<xs:simpleContent>");
			result.Add(@"<xs:extension base=""{0}"">", baseXmlType);
		}

		protected override Element GetChildElementHandler(PropertyInfo propertyInfo)
		{
			if (propertyInfo.Name == flattenedAttribute.BaseElementPropertyName) { return null; }

			return new AttributeElementConverter(propertyInfo, schemaGenerator, schemaGenerator.GetSimpleXmlType(propertyInfo));
		}

		protected override void WriteFooter(XsdBuilder result)
		{
			result.Add(@"</xs:extension>");
			result.Add(@"</xs:simpleContent>");
			WriteClosing(result);
		}

		protected virtual void WriteClosing(XsdBuilder result)
		{
			result.Add(@"</xs:complexType>");
		}
	}
}


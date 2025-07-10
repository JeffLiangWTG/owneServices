using System;
using System.Reflection;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration
{
	class ChildCollectionOuterElementConverter : ChildCollectionElementConverter
	{
		internal ChildCollectionOuterElementConverter(PropertyInfo propertyInfo, Type containedType, UniversalXsdGenerator schemaGenerator)
			: base(propertyInfo, containedType, schemaGenerator)
		{
		}

		protected override void WriteHeader(XsdBuilder result)
		{
			result.Add(@"<xs:element name=""{0}"" minOccurs=""{1}"">", ElementName, MinOccurs);
			result.Add(@"<xs:complexType>");
			result.Add(@"<xs:sequence>");
			result.Add(@"<xs:element name=""{0}"" minOccurs=""{1}"" maxOccurs=""unbounded"" type=""{2}"" />", InnerElementName, MinOccurs, TypeForFields.Name);
		}

		protected override void ReflectOutChildren(ElementList<Element> childConverters)
		{
			schemaGenerator.AddOutermostDefinition(TypeForFields);
		}

		protected override void WriteFooter(XsdBuilder result)
		{
			result.Add(@"</xs:sequence>");
			AddAttributesIfPresent<CollectionAttributesAttribute>(this.PropertyInfo.PropertyType, result);
			result.Add(@"</xs:complexType>");
			result.Add(@"</xs:element>");
		}
	}
}

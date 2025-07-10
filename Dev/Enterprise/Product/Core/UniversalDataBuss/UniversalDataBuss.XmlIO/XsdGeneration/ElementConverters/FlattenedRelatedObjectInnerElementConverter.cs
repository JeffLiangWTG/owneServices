using System;
using System.Reflection;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration
{
	class FlattenedRelatedObjectInnerElementConverter : FlattenedRelatedObjectElementConverter
	{
		internal FlattenedRelatedObjectInnerElementConverter(PropertyInfo propertyInfo, Type calculatedPropertyType, UniversalXsdGenerator schemaGenerator)
			: base(propertyInfo, calculatedPropertyType, schemaGenerator)
		{
		}

		protected override void WriteOpening(XsdBuilder result)
		{
			result.Add(@"<xs:element name=""{0}"" minOccurs=""{1}"">", ElementName, MinOccurs);
			result.Add(@"<xs:complexType>");
		}

		protected override void WriteClosing(XsdBuilder result)
		{
			result.Add(@"</xs:complexType>");
			result.Add(@"</xs:element>");
		}
	}
}

using System;
using System.Reflection;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration
{
	class RelatedObjectInnerElementConverter : ObjectElementConverter
	{
		internal RelatedObjectInnerElementConverter(PropertyInfo propertyInfo, Type calculatedPropertyType, UniversalXsdGenerator schemaGenerator)
			: base(propertyInfo, calculatedPropertyType, schemaGenerator)
		{
		}

		protected override void WriteHeader(XsdBuilder result)
		{
			result.Add(@"<xs:element name=""{0}"" minOccurs=""{1}"">", ElementName, MinOccurs);
			result.Add(@"<xs:complexType>");
			result.Add(@"<xs:all>");
		}

		protected override void WriteFooter(XsdBuilder result)
		{
			result.Add(@"</xs:all>");
			result.Add(@"</xs:complexType>");
			result.Add(@"</xs:element>");
		}
	}
}

using System;
using System.Reflection;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration
{
	internal class RelatedObjectOuterElementConverter : ObjectElementConverter
	{
		internal RelatedObjectOuterElementConverter(PropertyInfo propertyInfo, Type calculatedPropertyType, UniversalXsdGenerator schemaGenerator)
			: base(propertyInfo, calculatedPropertyType, schemaGenerator)
		{
		}

		protected override void WriteHeader(XsdBuilder result)
		{
			result.Add(@"<xs:element name=""{0}"" minOccurs=""{1}"" type=""{2}"" />", ElementName, MinOccurs, TypeForFields.Name);
		}

		protected override void ReflectOutChildren(ElementList<Element> childConverters)
		{
			schemaGenerator.AddOutermostDefinition(TypeForFields);
		}

		protected override void WriteFooter(XsdBuilder result)
		{
			// There is no footer.
		}
	}
}

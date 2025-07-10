using System;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration
{
	internal class OuterObjectElementConverter : ObjectElementConverter
	{
		internal OuterObjectElementConverter(Type type, UniversalXsdGenerator schemaGenerator)
			: base(type.Name, type, schemaGenerator)
		{
		}

		protected override void WriteHeader(XsdBuilder result)
		{
			result.Add(@"<xs:complexType name=""{0}"">", TypeForFields.Name);
			result.Add(@"<xs:all>");
		}

		protected override void WriteFooter(XsdBuilder result)
		{
			result.Add(@"</xs:all>");
			AddAttributesIfPresent<DataObjectAttributesAttribute>(TypeForFields, result);
			result.Add(@"</xs:complexType>");
		}
	}
}

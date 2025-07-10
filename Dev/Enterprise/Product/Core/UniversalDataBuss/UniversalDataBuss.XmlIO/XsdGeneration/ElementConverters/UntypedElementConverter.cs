using System.Reflection;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration
{
	internal class UntypedElementConverter : ObjectElementConverter
	{
		internal UntypedElementConverter(PropertyInfo propertyInfo, UniversalXsdGenerator schemaGenerator)
			: base(propertyInfo, propertyInfo.PropertyType, schemaGenerator)
		{
		}

		protected override void WriteHeader(XsdBuilder result)
		{
			result.Add(@"<xs:element name=""{0}"" minOccurs=""{1}"" type=""xs:anyType"" />", ElementName, MinOccurs);
		}

		protected override void WriteFooter(XsdBuilder result)
		{
			// There is no footer.
		}
	}
}
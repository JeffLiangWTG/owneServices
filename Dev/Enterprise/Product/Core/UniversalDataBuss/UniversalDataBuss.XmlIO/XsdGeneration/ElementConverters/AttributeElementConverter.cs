using System.Reflection;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration
{
	class AttributeElementConverter : Element, IElementConverter
	{
		internal AttributeElementConverter(PropertyInfo propertyInfo, UniversalXsdGenerator schemaGenerator, string simpleTypeName)
			: base(propertyInfo, schemaGenerator)
		{
			this.simpleTypeName = simpleTypeName;
		}

		readonly string simpleTypeName;

		internal override PlacingWithinXml DefaultPlacing
		{
			get { return PlacingWithinXml.FieldsAndRelatedObjects; }
		}

		public void WriteToList(XsdBuilder result)
		{
			result.Add(@"<xs:attribute name=""{0}"" type=""{1}"" />", PropertyInfo.Name, simpleTypeName);
		}
	}
}



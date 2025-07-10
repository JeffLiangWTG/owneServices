using System;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration
{
	class DataFieldElementConverter : Element, IElementConverter
	{
		readonly UniversalXsdGenerator schemaGenerator;

		internal DataFieldElementConverter(PropertyInfo propertyInfo, UniversalXsdGenerator schemaGenerator)
			: base(propertyInfo, schemaGenerator)
		{
			this.schemaGenerator = schemaGenerator;
		}

		internal override PlacingWithinXml DefaultPlacing => PlacingWithinXml.FieldsAndRelatedObjects;

		public void WriteToList(XsdBuilder result)
		{
			Type typeOfDataInField;

			if (PropertyInfo.PropertyType.IsGenericType)
			{
				typeOfDataInField = PropertyInfo.PropertyType.GetGenericArguments()[0];
			}
			else
			{
				typeOfDataInField = PropertyInfo.PropertyType;
			}

			Type[] typesForString = new[] { typeof(ZString), typeof(ZCodeMappedZString) };
			if (typesForString.Contains(typeOfDataInField))
			{
				result.Add(@"<xs:element name=""{0}"" minOccurs=""{1}"">", PropertyInfo.Name, MinOccurs);
				result.Add(@"<xs:simpleType>");
				result.Add(@"<xs:restriction base=""xs:string"">");
				result.Add(@"<xs:maxLength value=""{0}"" />", PropertyInfo.GetMaxLength());
				result.Add(@"</xs:restriction>");
				result.Add(@"</xs:simpleType>");
				result.Add(@"</xs:element>");
			}
			else if (typeof(Enum).IsAssignableFrom(typeOfDataInField))
			{
				result.Add(@"<xs:element name=""{0}"" minOccurs=""{1}"">", PropertyInfo.Name, MinOccurs);
				result.Add(@"<xs:simpleType>");
				result.Add(@"<xs:restriction base=""xs:string"">");

				foreach (var enumerationValueName in Enum.GetNames(typeOfDataInField))
				{
					result.Add(@"<xs:enumeration value=""{0}"" />", enumerationValueName);
				}

				result.Add(@"</xs:restriction>");
				result.Add(@"</xs:simpleType>");
				result.Add(@"</xs:element>");
			}
			else
			{
				result.Add(@"<xs:element name=""{0}"" minOccurs=""{2}"" type=""{1}"" />", PropertyInfo.Name, schemaGenerator.GetXMLType(typeOfDataInField), MinOccurs);
			}
		}
	}
}

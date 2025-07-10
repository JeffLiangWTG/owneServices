using System.IO;
using System.Xml.Schema;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class TypeNameSameAsElementNameTestCase : TestCaseWithFactory
	{
		public void TestSerializingArrayWithElementNameSameAsXsdTypeName()
		{
			ValueObjectWithArray valueObject = new ValueObjectWithArray();
			ValueObjectForArrayElement_Subclass arrayElement = valueObject.Elements.AddNew();
			arrayElement.String = "Value";

			string expectedXml = XmlDocumentVersion + @"
<ValueObjectWithArray " + NamespaceValue + @">
  <Elements>
    <ValueObjectForArrayElement>
      <String>Value</String>
    </ValueObjectForArrayElement>
  </Elements>
</ValueObjectWithArray>".Trim().Replace("'", "\"");
			TestGeneratedXml(expectedXml, valueObject, TestXmlSchemaDefinitions.Instance.XmlArrayItemElementNameAttributeToBeClassNameAgnosticTestXsd);
		}

		public void TestSerializingElementWithSameNameAsXsdType()
		{
			ValueObjectForElementWithXsdTypeOfSameName valueObject = new ValueObjectForElementWithXsdTypeOfSameName();
			valueObject.XsdTypeForElementOfSameName.String = "Value";

			string expectedXml = XmlDocumentVersion + @"
<ValueObjectForElementWithXsdTypeOfSameName " + NamespaceValue + @">
  <XsdTypeForElementOfSameName>
    <String>Value</String>
  </XsdTypeForElementOfSameName>
</ValueObjectForElementWithXsdTypeOfSameName>".Trim().Replace("'", "\"");

			TestGeneratedXml(expectedXml, valueObject, TestXmlSchemaDefinitions.Instance.XsiTypeAttributeToBeClassNameAgnosticTestXsd);
		}

		void TestGeneratedXml(string expectedXml, IValueObject valueObject, XmlSchema validatingSchema)
		{
			XmlValueObjectSerializer serializer = new XmlValueObjectSerializer(valueObject.GetType());

			StringWriter writer = new StringWriter();
			serializer.Serialize(writer, valueObject);
			string actualXml = writer.GetStringBuilder().ToString();
			AssertXMLEquals("Xml Correct", expectedXml, actualXml);

			NotificationBuffer notify = new NotificationBuffer();
			new XmlValidator(validatingSchema).Validate(expectedXml, notify);
			AssertEquals("Validation errors occurred: \r\n\r\n" + notify.AsString, false, notify.HasErrors);
		}

		#region NamespaceValue

		const string XmlDocumentVersion = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
		const string NamespaceValue = "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://www.edi.com.au/EnterpriseService/\"";

		#endregion
	}
}

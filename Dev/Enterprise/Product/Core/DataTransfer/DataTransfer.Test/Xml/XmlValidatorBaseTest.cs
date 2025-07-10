using System;
using System.IO;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.IO;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	public abstract class XmlValidatorBaseTest : TransactionedTestCase
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorThrowsExceptionIfSchemaNull()
		{
			new XmlValidator(null);
		}

		public void TestValidate_ValidContentWithEDINamespace()
		{
			if (!IsSchemaInNoNamespace)
			{
				NotificationBuffer notify = new NotificationBuffer();
				Validate(ValidXmlString_WithEDINamespace, Schema, notify);
				AssertEquals("Should validate correctly with xml content in the EDI namespace", false, notify.HasErrors);
			}
			else
			{
				Assert("EDI NS on schema and no NS in xml content not supported", true);
			}
		}

		public void TestValidate_InvalidContentWithEDINamespace()
		{
			NotificationBuffer notify = new NotificationBuffer();
			Validate(InvalidXmlString_WithEDINamespace, Schema, notify);
			AssertEquals("An invalid xml with xml content in the EDI namespace should have errors", true, notify.HasErrors);
		}

		public void TestValidate_ValidContentWithNoNamespace()
		{
			NotificationBuffer notify = new NotificationBuffer();
			Validate(ValidXmlString_WithNoNamespace, Schema, notify);
			AssertEquals("Should validate correctly with xml content in NO namespace", false, notify.HasErrors);
		}

		public void TestValidate_InvalidContentWithNoNamespace()
		{
			NotificationBuffer notify = new NotificationBuffer();
			Validate(InvalidXmlString_WithNoNamespace, Schema, notify);
			AssertEquals("An invalid xml with xml content in NO namespace should have errors", true, notify.HasErrors);
		}

		public void TestValidate_WithMissingElementsAndAttributes()
		{
			NotificationBuffer notify = new NotificationBuffer();
			Validate(XmlMissingMandatoryElementsAndAttributes, TestXmlSchemaDefinitions.Instance.SchemaWithMandatoryElementsAndAttributes, notify);

			string errorText =
				"Error: The required attribute 'MandatoryAttribute1' is missing.\r\n" +
				"Error: The element 'NestedElement' in namespace 'http://www.edi.com.au/EnterpriseService/' has invalid child element 'MandatoryElement2' in namespace 'http://www.edi.com.au/EnterpriseService/'. List of possible elements expected: 'MandatoryElement1' in namespace 'http://www.edi.com.au/EnterpriseService/'.\r\n";

			AssertEquals("Should have only 2 xml errors, because of missing MandatoryElement2 and MandatoryAttribute2", errorText, notify.AsString);
		}

		public void TestValidate_WithExtraElementsAndAttributes()
		{
			NotificationBuffer notify = new NotificationBuffer();
			Validate(XmlWithExtraElementsAndAttributes, TestXmlSchemaDefinitions.Instance.SchemaWithMandatoryElementsAndAttributes, notify);

			string errorText =
				"Error: The required attribute 'MandatoryAttribute2' is missing.\r\n" +
				"Error: The element 'NestedElement' in namespace 'http://www.edi.com.au/EnterpriseService/' has incomplete content. List of possible elements expected: 'MandatoryElement2' in namespace 'http://www.edi.com.au/EnterpriseService/'.\r\n";

			AssertEquals("Should have only 2 xml errors, because of missing MandatoryElement2 and MandatoryAttribute2", errorText, notify.AsString);
		}

		public void TestValidate_WithExtraElementsAndAttributes_WithStrictValidationOn()
		{
			SystemDataRegistry.Instance.XmlSchemaValidationStrict.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			NotificationBuffer notify = new NotificationBuffer();
			Validate(XmlWithExtraElementsAndAttributes, TestXmlSchemaDefinitions.Instance.SchemaWithMandatoryElementsAndAttributes, notify);

			string errorText =
				"Error: The 'ExtraAttribute1' attribute is not declared.\r\n" +
				"Error: The 'ExtraAttribute2' attribute is not declared.\r\n" +
				"Error: The required attribute 'MandatoryAttribute2' is missing.\r\n" +
				"Error: The element 'NestedElement' in namespace 'http://www.edi.com.au/EnterpriseService/' has invalid child element 'ExtraElement1' in namespace 'http://www.edi.com.au/EnterpriseService/'. List of possible elements expected: 'MandatoryElement1' in namespace 'http://www.edi.com.au/EnterpriseService/'.\r\n";

			AssertEquals("Should have only 2 xml errors, because of missing MandatoryElement2 and MandatoryAttribute2", errorText, notify.AsString);
		}

		protected abstract void Validate(string xml, XmlSchema schema, INotifications notifications);
		protected abstract bool IsSchemaInNoNamespace { get; }

		#region Setup

		XmlSchema Schema;

		protected override void SetUp()
		{
			base.SetUp();
			SystemDataRegistry.Instance.XmlSchemaValidationStrict.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			string xmlSchemaString = IsSchemaInNoNamespace ? XmlSchemaWithNoNamespace : XmlSchemaWithEdiNamespace;
			Stream xsdStream = StreamConverter.StringToStream(xmlSchemaString);

			XsdSchemaBuilder xsdSchemaBuilder = new XsdSchemaBuilder();
			Schema = xsdSchemaBuilder.Read(xsdStream);
			xsdSchemaBuilder.CompileSchema(Schema);
		}

		#endregion

		#region Test Xml Schemas

		const string XmlSchemaWithEdiNamespace =
			"<xs:schema targetNamespace=\"http://www.edi.com.au/EnterpriseService/\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns=\"http://www.edi.com.au/EnterpriseService/\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">" +
			"	<xs:element name=\"Test\">" +
			"		<xs:complexType>" +
			"			<xs:sequence>" +
			"				<xs:element name=\"Field1\" minOccurs=\"1\"/>" +
			"				<xs:element name=\"Field2\"/>" +
			"			</xs:sequence>" +
			"		</xs:complexType>" +
			"	</xs:element>" +
			"</xs:schema>";

		const string XmlSchemaWithNoNamespace =
			"<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">" +
			"	<xs:element name=\"Test\">" +
			"		<xs:complexType>" +
			"			<xs:sequence>" +
			"				<xs:element name=\"Field1\" minOccurs=\"1\"/>" +
			"				<xs:element name=\"Field2\"/>" +
			"			</xs:sequence>" +
			"		</xs:complexType>" +
			"	</xs:element>" +
			"</xs:schema>";

		#endregion

		#region Test Xml Content

		const string ValidXmlString_WithEDINamespace =
			"<Test xmlns=\"http://www.edi.com.au/EnterpriseService/\">\n" +
			"	<Field1>Text</Field1>\n" +
			"	<Field2>Text</Field2>\n" +
			"</Test>\n";

		const string InvalidXmlString_WithEDINamespace =
			"<Test xmlns=\"http://www.edi.com.au/EnterpriseService/\">\n" +
			"	<Field2/>\n" +
			"</Test>";

		const string ValidXmlString_WithNoNamespace =
			"<Test>\n" +
			"	<Field1>Text</Field1>\n" +
			"	<Field2>Text</Field2>\n" +
			"</Test>\n";

		const string InvalidXmlString_WithNoNamespace =
			"<Test>\n" +
			"	<Field2/>\n" +
			"</Test>";

		const string XmlMissingMandatoryElementsAndAttributes =
			"<WithMandatoryElementsAndAttributes xmlns=\"http://www.edi.com.au/EnterpriseService/\" MandatoryAttribute2=\"Value\">\n" +
			"	<NestedElement>\n" +
			"		<MandatoryElement2>Value</MandatoryElement2>\n" +
			"	</NestedElement>\n" +
			"</WithMandatoryElementsAndAttributes>\n";

		const string XmlWithExtraElementsAndAttributes =
			"<WithMandatoryElementsAndAttributes xmlns=\"http://www.edi.com.au/EnterpriseService/\" ExtraAttribute1=\"Value\" MandatoryAttribute1=\"Value\" ExtraAttribute2=\"Value\">\n" +
			"	<NestedElement>\n" +
			"		<ExtraElement1 />\n" +
			"		<MandatoryElement1>Value</MandatoryElement1>\n" +
			"		<ExtraElement2>Value</ExtraElement2>\n" +
			"	</NestedElement>\n" +
			"</WithMandatoryElementsAndAttributes>\n";

		#endregion
	}
}

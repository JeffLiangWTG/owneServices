using System.IO;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.IO;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	class XsdSchemaBuilderTest : TestCase
	{
		public void TestReadFromStream()
		{
			Stream xsdStream = StreamConverter.StringToStream(XsdString);
			AssertEquals(typeof(ZXmlSchema), SchemaBuilder.Read(xsdStream).GetType());
			AssertEquals(false, SchemaBuilder.HasErrors);
		}

		public void TestReadFromString()
		{
			AssertEquals(typeof(ZXmlSchema), SchemaBuilder.Read(XsdString).GetType());
			AssertEquals(false, SchemaBuilder.HasErrors);
		}

		public void TestIncludeMultipleIncludes()
		{
			ZXmlSchema parentSchema = SchemaBuilder.Read(XsdStringWithTypesInTwoOtherXsds);
			ZXmlSchema includedSchema = SchemaBuilder.Read(XsdInclude);
			ZXmlSchema included1Schema = SchemaBuilder.Read(XsdInclude1);
			SchemaBuilder.Include(parentSchema, new ZXmlSchema[] { includedSchema, included1Schema });
			AssertEquals(false, SchemaBuilder.HasErrors);
		}

		public void TestInclude()
		{
			ZXmlSchema parentSchema = SchemaBuilder.Read(XsdStringWithTypeInAnotherXsd);
			ZXmlSchema includedSchema = SchemaBuilder.Read(XsdInclude);
			SchemaBuilder.Include(parentSchema, includedSchema);
			AssertEquals(false, SchemaBuilder.HasErrors);
		}

		public void TestCompileSchema()
		{
			ZXmlSchema parentSchema = SchemaBuilder.Read(XsdStringWithTypeInAnotherXsd);
			ZXmlSchema includedSchema = SchemaBuilder.Read(XsdInclude);
			SchemaBuilder.Include(parentSchema, includedSchema);
			SchemaBuilder.CompileSchema(parentSchema);
			AssertEquals(false, SchemaBuilder.HasErrors);
		}

		public void TestGetSchemaWithElementOfType()
		{
			ZXmlSchema parentSchema = SchemaBuilder.Read(XsdToGetSchemaWithElementOfType);
			CompileSchema(parentSchema);
			ZXmlSchema schema = SchemaBuilder.GetSchemaWithElementOfType(parentSchema, "SomeSimpleType", "Element");
			AssertEquals("Schema must be cloned at the end to fix an unreproducible bug", schema, SchemaBuilder.LastSchemaClonedToFixUnreproducibleBug);
			CompileSchema(schema);

			AssertEquals("Should have 1 element only, other elements should be deleted", 1, schema.Elements.Count);
			XmlSchemaElement element = schema.Items[2] as XmlSchemaElement;
			AssertEquals("Should have an element", true, schema.Items[2] is XmlSchemaElement);
			AssertEquals("Should have an element of the simple type", true, element.ElementSchemaType is XmlSchemaSimpleType);
			AssertEquals("Should have an element of the simple type with name SomeSimpleType", "SomeSimpleType", ((XmlSchemaSimpleType)element.ElementSchemaType).Name);
		}

		public void TestGetSchemaOfNestedElement()
		{
			ZXmlSchema parentSchema = SchemaBuilder.Read(XsdToGetSchemaOfNestedElement);
			CompileSchema(parentSchema);
			ZXmlSchema schema = SchemaBuilder.GetSchemaOfNestedElement(parentSchema, "OuterElement");
			AssertEquals("Schema must be cloned at the end to fix an unreproducible bug", schema, SchemaBuilder.LastSchemaClonedToFixUnreproducibleBug);
			CompileSchema(schema);

			AssertEquals("Should have 1 element only", 1, schema.Elements.Count);
			XmlSchemaElement element = schema.Items[0] as XmlSchemaElement;
			AssertEquals("Should have an element", true, schema.Items[0] is XmlSchemaElement);
			AssertEquals("Should have correct inner element", "InnerElement", element.Name);
		}

		void CompileSchema(ZXmlSchema schema)
		{
			XmlSchemas schemas = new XmlSchemas();
			schemas.Add(schema);
			schemas.Compile(null, true);
		}

		#region Setup

		readonly TestXsdSchemaBuilder SchemaBuilder = new TestXsdSchemaBuilder();

		protected class TestXsdSchemaBuilder : XsdSchemaBuilder
		{
			public ZXmlSchema LastSchemaClonedToFixUnreproducibleBug;
			protected override ZXmlSchema CloneSchemaToFixUnreproducibleBug(ZXmlSchema xmlSchema)
			{
				ZXmlSchema result = base.CloneSchemaToFixUnreproducibleBug(xmlSchema);
				LastSchemaClonedToFixUnreproducibleBug = result;
				return result;
			}
		}

		readonly string XsdString =
			"<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
			"	<xs:element name=\"Test\">" +
			"		<xs:complexType>" +
			"			<xs:sequence>" +
			"				<xs:element name=\"Field1\"/>" +
			"				<xs:element name=\"Field2\"/>" +
			"			</xs:sequence>" +
			"		</xs:complexType>" +
			"	</xs:element>" +
			"</xs:schema>";

		readonly string XsdStringWithTypeInAnotherXsd =
			"<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
			"	<xs:element name=\"Test\">" +
			"		<xs:complexType>" +
			"			<xs:sequence>" +
			"				<xs:element name=\"IsTrue\" type=\"TrueFalse\"/>" +
			"			</xs:sequence>" +
			"		</xs:complexType>" +
			"	</xs:element>" +
			"</xs:schema>";

		readonly string XsdStringWithTypesInTwoOtherXsds =
			"<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
			"	<xs:element name=\"Test\">" +
			"		<xs:complexType>" +
			"			<xs:sequence>" +
			"				<xs:element name=\"IsTrue\" type=\"TrueFalse\"/>" +
			"				<xs:element name=\"BooleanString\" type=\"TrueFalse\"/>" +
			"			</xs:sequence>" +
			"		</xs:complexType>" +
			"	</xs:element>" +
			"</xs:schema>";

		readonly string XsdInclude =
			"<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
			"	<xs:simpleType name=\"TrueFalse\">" +
			"		<xs:restriction base=\"xs:NMTOKEN\">" +
			"			<xs:enumeration value=\"true\"/>" +
			"			<xs:enumeration value=\"false\"/>" +
			"		</xs:restriction>" +
			"	</xs:simpleType>" +
			"</xs:schema>";

		readonly string XsdInclude1 =
			"<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
			"	<xs:simpleType name=\"BooleanString\">" +
			"		<xs:restriction base=\"xs:NMTOKEN\">" +
			"			<xs:enumeration value=\"true\"/>" +
			"			<xs:enumeration value=\"false\"/>" +
			"		</xs:restriction>" +
			"	</xs:simpleType>" +
			"</xs:schema>";

		readonly string XsdToGetSchemaWithElementOfType =
			"<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
			"   <xs:element name=\"should_delete\" type=\"BooleanString\" />" +
			"   <xs:element name=\"should_delete2\" type=\"BooleanString\" />" +
			"	<xs:simpleType name=\"BooleanString\">" +
			"		<xs:restriction base=\"xs:NMTOKEN\">" +
			"			<xs:enumeration value=\"true\"/>" +
			"			<xs:enumeration value=\"false\"/>" +
			"		</xs:restriction>" +
			"	</xs:simpleType>" +
			"	<xs:simpleType name=\"SomeSimpleType\">" +
			"		<xs:restriction base=\"xs:NMTOKEN\">" +
			"			<xs:enumeration value=\"true\"/>" +
			"			<xs:enumeration value=\"false\"/>" +
			"		</xs:restriction>" +
			"	</xs:simpleType>" +
			"</xs:schema>";

		readonly string XsdToGetSchemaOfNestedElement =
			"<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" +
			"	<xs:element name=\"OuterElement\">" +
			"		<xs:complexType>" +
			"			<xs:sequence>" +
			"				<xs:element name=\"InnerElement\" minOccurs=\"0\" />" +
			"			</xs:sequence>" +
			"		</xs:complexType>" +
			"	</xs:element>" +
			"</xs:schema>";

		#endregion
	}
}

using System;
using System.Xml.Schema;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class TestXmlSchemaDefinitions : XmlSchemaDefinitionsBase
	{
		// for the test this is public but usually for subclasses of XmlSchemaDefinitionsBase you should make this protected
		public TestXmlSchemaDefinitions()
		{
		}

		public static TestXmlSchemaDefinitions Instance
		{
			get
			{
				TestXmlSchemaDefinitions result = (TestXmlSchemaDefinitions)WeakInstance.Target;
				if (result == null)
				{
					result = new TestXmlSchemaDefinitions();
					WeakInstance.Target = result;
				}
				return result;
			}
		}

		static WeakReference WeakInstance
		{
			get { return weakInstance ?? (weakInstance = new WeakReference(null)); }
		}
		[ThreadStatic] static WeakReference weakInstance;

		[ExpectXmlSchemaContainsRootElement("TestElements")]
		public XmlSchema TestElementsSchema
		{
			get { return GetCompiledSchema("Enterprise.DataTransfer.Test.Xml.Testing", "TestElementsSchema.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("TestElement")]
		public XmlSchema SingleTestElementSchema
		{
			get { return GetCompiledSchemaNestedElement(TestElementsSchema, "TestElements"); }
		}

		[ExpectXmlSchemaContainsRootElement("WithMandatoryElementsAndAttributes")]
		public XmlSchema SchemaWithMandatoryElementsAndAttributes
		{
			get { return GetCompiledSchema("Enterprise.DataTransfer.Test.Xml.Testing", "TestSchemaWithMandatoryElementsAndAttributes.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("ValueObjectWithArray")]
		public XmlSchema XmlArrayItemElementNameAttributeToBeClassNameAgnosticTestXsd
		{
			get { return GetCompiledSchema("Enterprise.DataTransfer.Xml.Testing.XsdTypeNameSameAsElementNameTestCase", "XmlArrayItemElementNameAttributeToBeClassNameAgnosticTestXsd.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("ValueObjectForElementWithXsdTypeOfSameName")]
		public XmlSchema XsiTypeAttributeToBeClassNameAgnosticTestXsd
		{
			get { return GetCompiledSchema("Enterprise.DataTransfer.Xml.Testing.XsdTypeNameSameAsElementNameTestCase", "XsiTypeAttributeToBeClassNameAgnosticTestXsd.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("SomeElementName")]
		public XmlSchema SomeElementWithADifferentSchemaTypeSchema
		{
			get { return GetCompiledSchemaWithElementOfType(TestElementsSchema, "ElementTypeFromZArchitectureAssembly", "SomeElementName"); }
		}

		[ExpectXmlSchemaContainsRootElement("TestElements")]
		public XmlSchema SingleTestElementSchemaFromRelativeSchemaName
		{
			get { return GetCompiledSchema("Enterprise.DataTransfer.Test.Xml.Testing.blah.blah", "  ..\\../..\\Testing/TestElementsSchema.xsd  "); }
		}

		[ExpectXmlSchemaContainsRootElement("TestElements")]
		public XmlSchema SingleTestElementSchemaFromAbsoluteSchemaName
		{
			get { return GetCompiledSchema("Enterprise.DataTransfer.Test.Xml.Testing.blah.blah", "  /Enterprise.DataTransfer.Test.Xml.Testing\\TestElementsSchema.xsd/  "); }
		}
	}
}

using System.Xml.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(TestXmlSchemaDefinitions))]
	sealed class XmlSchemaDefinitionsBaseTest_ForCoreFunctionality : XmlSchemaDefinitionsBaseTest
	{
		public void TestGetCompiledSchema_FromRelativeSchemaName()
		{
			AssertNotNull("Should find this schema", new TestXmlSchemaDefinitions().SingleTestElementSchemaFromRelativeSchemaName);
		}

		public void TestGetCompiledSchema_FromAbsoluteSchemaName()
		{
			AssertNotNull("Should find this schema", new TestXmlSchemaDefinitions().SingleTestElementSchemaFromAbsoluteSchemaName);
		}

		public void TestAllSchemas()
		{
			XmlSchema[] schemas = GetXmlSchemaDefinitions().AllSchemas;
			AssertEquals("Should return the right number of schemas", 8, schemas.Length);
			foreach (XmlSchema schema in schemas)
			{
				AssertNotNull("No schema objects should return null", schema);
			}
		}

		protected override XmlSchemaDefinitionsBase GetXmlSchemaDefinitions()
		{
			return TestXmlSchemaDefinitions.Instance;
		}
	}
}

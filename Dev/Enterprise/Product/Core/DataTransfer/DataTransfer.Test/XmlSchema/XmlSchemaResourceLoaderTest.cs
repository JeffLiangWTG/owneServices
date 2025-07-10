using System.Xml.Schema;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class XmlSchemaResourceLoaderTest : TestCase
	{
		public void TestReadSchemaFromAbsoluteResourceName()
		{
			XmlSchema schema = new XmlSchemaResourceLoader().ReadSchema(GetType().Assembly, "splaty.voteclinty.blah.blah", "  /Enterprise.DataTransfer.Test.Xml.Testing\\TestElementsSchema.xsd/  ");
			AssertNotNull("Schema should be read and compiled successfully", schema);
		}

		public void TestReadSchemaFromRelativeResourceName()
		{
			XmlSchema schema = new XmlSchemaResourceLoader().ReadSchema(GetType().Assembly, "Enterprise.DataTransfer.Test.Xml.Testing.blah.blah", "  ..\\../..\\Testing/TestElementsSchema.xsd  ");
			AssertNotNull("Schema should be read and compiled successfully", schema);
		}

		public void TestReadSchemaShouldBeAbleToIncludeSchema_UsingRelativePath_UsingRelatedIncludingPath_AndFromAnotherAssembly()
		{
			XmlSchema schema = new XmlSchemaResourceLoader().ReadSchema(GetType().Assembly, "Enterprise.DataTransfer.Test.Xml.Testing.blah.blah", "  ..\\../..\\Testing/TestElementsSchema.xsd  ");
			AssertNotNull("Schema should be read and compiled successfully", schema);

			bool foundIncludedSchemaType = false;
			foreach (XmlSchemaInclude include in schema.Includes)
			{
				foreach (XmlSchemaObject current in include.Schema.Items)
				{
					XmlSchemaComplexType complexType = current as XmlSchemaComplexType;
					if (complexType != null &&
						complexType.Name == "ElementTypeFromZArchitectureAssembly")
					{
						foundIncludedSchemaType = true;
					}
				}
			}
			AssertEquals("Should find xsd type from included schema, even though it is in another assembly", true, foundIncludedSchemaType);
		}
	}
}

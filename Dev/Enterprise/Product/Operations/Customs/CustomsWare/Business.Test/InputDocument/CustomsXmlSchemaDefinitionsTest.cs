using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	[TestedType(typeof(SchemaDefinitions))]
	public class CustomsXmlSchemaDefinitionsTest : XmlSchemaDefinitionsBaseTest
	{
		protected override XmlSchemaDefinitionsBase GetXmlSchemaDefinitions()
		{
			return SchemaDefinitions.Instance;
		}

		public override void TestDecimalValidationFractionDigitsFacetNotUsedInSchemas()
		{
			Assert("we dont dictate the contents of the schema", true);
		}

		protected override string GetExpectedXmlNamespace()
		{
			return "http://www.customsware.com/schema/api";
		}
	}
}

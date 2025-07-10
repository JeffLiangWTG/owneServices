using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(CustomsXmlSchemaDefinitions))]
	sealed class CustomsXmlSchemaDefinitionsTest : XmlSchemaDefinitionsBaseTest
	{
		protected override XmlSchemaDefinitionsBase GetXmlSchemaDefinitions()
		{
			return CustomsXmlSchemaDefinitions.Instance;
		}
	}
}

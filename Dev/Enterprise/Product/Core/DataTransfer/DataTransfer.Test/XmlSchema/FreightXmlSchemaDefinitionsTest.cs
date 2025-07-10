using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(FreightXmlSchemaDefinitions))]
	sealed class FreightXmlSchemaDefinitionsTest : XmlSchemaDefinitionsBaseTest
	{
		protected override XmlSchemaDefinitionsBase GetXmlSchemaDefinitions()
		{
			return FreightXmlSchemaDefinitions.Instance;
		}
	}
}

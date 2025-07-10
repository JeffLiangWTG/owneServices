using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(XmlSchemaDefinitions))]
	sealed class XmlSchemaDefinitionsTest : XmlSchemaDefinitionsBaseTest
	{
		protected override XmlSchemaDefinitionsBase GetXmlSchemaDefinitions()
		{
			return XmlSchemaDefinitions.Instance;
		}
	}
}

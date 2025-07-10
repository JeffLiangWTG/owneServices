using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(WebServicesXmlSchemaDefinitions))]
	sealed class WebServicesXmlSchemaDefinitionsTest : XmlSchemaDefinitionsBaseTest
	{
		protected override XmlSchemaDefinitionsBase GetXmlSchemaDefinitions()
		{
			return WebServicesXmlSchemaDefinitions.Instance;
		}
	}
}

using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	[TestedType(typeof(AccountingXmlSchemaDefinitions))]
	sealed class AccountingXmlSchemaDefinitionsTest : XmlSchemaDefinitionsBaseTest
	{
		protected override XmlSchemaDefinitionsBase GetXmlSchemaDefinitions()
		{
			return AccountingXmlSchemaDefinitions.Instance;
		}
	}
}

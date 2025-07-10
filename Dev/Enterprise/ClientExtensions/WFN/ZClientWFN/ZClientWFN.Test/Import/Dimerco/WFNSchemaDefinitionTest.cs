using CargoWise.IO;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.WFN.Testing
{
	[TestedType(typeof(WFNSchemaDefinition))]
	public class WFNSchemaDefinitionTest : XmlSchemaDefinitionsBaseTest
	{
		public void TestSchemaWellFormedAndValidatesExampleDocument()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var str = resourceRetriever.GetString("Import.TestFiles.Test.xml", System.Text.Encoding.UTF8);

				NotificationBuffer notify = new NotificationBuffer();
				XmlValidator validator = new XmlValidator(WFNSchemaDefinition.Instance.WFNSchema);
				validator.Validate(str, notify);
				AssertEquals(false, notify.HasErrors);
			}
		}

		protected override string GetExpectedXmlNamespace()
		{
			return "";
		}

		protected override XmlSchemaDefinitionsBase GetXmlSchemaDefinitions()
		{
			return WFNSchemaDefinition.Instance;
		}
	}
}

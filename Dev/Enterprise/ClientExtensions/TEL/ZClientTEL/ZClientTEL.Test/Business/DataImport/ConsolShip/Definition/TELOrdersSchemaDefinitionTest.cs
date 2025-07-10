using System.IO;
using CargoWise.IO;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.TEL.Definition.Testing
{
	[TestedType(typeof(TELConsolShipSchemaDefinition))]
	public class TELOrdersSchemaDefinitionTest : XmlSchemaDefinitionsBaseTest
	{
		public void TestSchemaWellFormedAndValidatesExampleDocument()
		{
			EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var testFile = resourceRetriever.GetBytes("XMN-SB0800639.xml");
			using (StreamReader reader = new StreamReader(new MemoryStream(testFile)))
			{
				NotificationBuffer notify = new NotificationBuffer();
				XmlValidator validator = new XmlValidator(TELConsolShipSchemaDefinition.Instance.ConsolShipSchema);
				validator.Validate(reader.ReadToEnd(), notify);
				AssertEquals(false, notify.HasErrors);
			}
		}

		public void TestSchemaBadFormedAndValidatesExampleDocument()
		{
			EmbeddedResourceRetriever resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var testFile = resourceRetriever.GetBytes("InvalidFormat.xml");
			using (StreamReader reader = new StreamReader(new MemoryStream(testFile)))
			{
				NotificationBuffer notify = new NotificationBuffer();
				XmlValidator validator = new XmlValidator(TELConsolShipSchemaDefinition.Instance.ConsolShipSchema);
				validator.Validate(reader.ReadToEnd(), notify);
				AssertEquals(true, notify.HasErrors);
			}
		}

		protected override string GetExpectedXmlNamespace()
		{
			return "";
		}

		protected override XmlSchemaDefinitionsBase GetXmlSchemaDefinitions()
		{
			return TELConsolShipSchemaDefinition.Instance;
		}
	}
}

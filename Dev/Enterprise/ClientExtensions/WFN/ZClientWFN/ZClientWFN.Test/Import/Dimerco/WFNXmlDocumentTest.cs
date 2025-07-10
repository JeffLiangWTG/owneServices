using System.IO;
using CargoWise.IO;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using BtaXsd = Enterprise.Client.WFN.Definition;

namespace Enterprise.Client.WFN
{
	public class WFNXmlDocumentTest : TestCase
	{
		WFNXmlDocument xmlDocument;
		NotificationBuffer notification;
		public void TestWFNXmlDocumentProperties()
		{
			AssertEquals(WFNSchemaDefinition.Instance.WFNSchema, xmlDocument.internalDocumentSchemaTest);
			AssertEquals("MASTER", xmlDocument.internalRootElementNameTest);
			AssertEquals(typeof(BtaXsd.MASTER), xmlDocument.internalRootElementTypeTest);
			AssertNotNull(xmlDocument.internalGetNewIValueObjectTest() as BtaXsd.MASTER);
		}

		protected override void SetUp()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var srcTestFilePath = resourceRetriever.SaveResourceToFile("Import.TestFiles.Test.xml");
				using (StreamReader reader = new StreamReader(srcTestFilePath))
				{
					notification = new NotificationBuffer();
					xmlDocument = new WFNXmlDocument(reader, notification);
				}
			}
			base.SetUp();
		}
	}
}

using System;
using System.IO;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.IO;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.TEL.Definition.Testing
{
	public class TELOrdersXmlDocumentTest : TestCase
	{
		TELManifestXmlDocumentTesting TELXmlDocument;
		public void TestTELDocumentSchema()
		{
			AssertEquals(TELConsolShipSchemaDefinition.Instance.ConsolShipSchema, TELXmlDocument.DocumentXMLSchema());
		}

		public void TestRootElementName()
		{
			AssertEquals("SeaMasterBills", TELXmlDocument.XmlRootElementName());
		}

		public void TestGetNewIValueObject()
		{
			AssertEquals(typeof(SeaMasterBills), TELXmlDocument.GetNewValueObject().GetType());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			var testFile = resourceRetriever.GetBytes("XMN-SB0800639.xml");
			using (StreamReader reader = new StreamReader(new MemoryStream(testFile)))
			{
				TELXmlDocument = new TELManifestXmlDocumentTesting(reader, new NotificationBuffer());
			}
		}

		class TELManifestXmlDocumentTesting : TELConsolShipXmlDocument
		{
			public TELManifestXmlDocumentTesting(StreamReader document, INotifications notification) : base(document, notification)
			{
			}

			public XmlSchema DocumentXMLSchema()
			{
				return DocumentSchema;
			}

			public String XmlRootElementName()
			{
				return RootElementName;
			}

			public DataTransfer.Xml.IValueObject GetNewValueObject()
			{
				return GetNewIValueObject();
			}
		}
	}
}

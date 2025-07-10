using System.IO;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.MessageDelivery.Testing
{
	sealed class DxlBusinessObjectSerializerTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2008, 10, 10)]
		public void TestConvertXmlToDxlStream()
		{
			EDICommunicationsMode mode = Factory.NewWithValidTestData<EDICommunicationsMode>();
			mode.EK_LocalPartyVanID = "senderID";
			mode.EK_RelatedPartyVanID = "resiverID";
			mode.EK_MessagePurpose = "APP";
			DxlBusinessObjectSerializerForTesting messageDeliver = new DxlBusinessObjectSerializerForTesting(Factory.NewWithValidTestData<OrgHeader>());
			byte[] buffer = File.ReadAllBytes(Path.Combine(BaseSourcePath, @"Enterprise\Product\Core\DataTransfer\DataTransfer.Test\MessageDelivery\Testing\TestShipmentXML.dat"));
			MemoryStream xmlStream = new MemoryStream(buffer);

			Stream dxlStream = messageDeliver.ConvertXmlToDxl(xmlStream, mode);
			xmlReader = new XmlTextReader(dxlStream);

			AssertElement("S:Envelope", null);
			AssertNextAttribute("http://www.w3.org/2003/05/soap-envelope");
			AssertNextAttribute("http://schemas.xmlsoap.org/ws/2004/03/addressing");
			AssertNextAttribute("http://www.myvan.descartes.com/ebi/2004/r1");
			AssertElement("S:Header", null);
			AssertElement("wsa:From", null);
			AssertElement("wsa:Address", "urn:zz:senderID");
			AssertElement("wsa:To", "urn:zz:resiverID");
			AssertElement("wsa:Action", "urn:myvan:APP");
			AssertElement("ebi:Sequence", null);
			AssertElement("ebi:MessageNumber", "1");
			AssertElement("ebi:Created", null);
			Assert("Element value should start with 2008-10-10", xmlReader.ReadElementContentAsString().StartsWith("2008-10-10"));
			AssertElement("S:Body", null);
			AssertElement("XmlInterchange", null);
			XmlReader dxlReader = xmlReader.ReadSubtree();
			dxlReader.MoveToContent();
			XmlReader expectedXmlReader = new XmlTextReader(xmlStream);
			expectedXmlReader.MoveToContent();
			AssertEquals("XmlInterchange elements should be same", expectedXmlReader.ReadOuterXml(), dxlReader.ReadOuterXml());
		}

		void AssertNextAttribute(string expectedAttributeValue)
		{
			xmlReader.MoveToNextAttribute();
			AssertEquals("Attribute value", expectedAttributeValue, xmlReader.Value);
		}

		void AssertElement(string elementName, string expectedValue)
		{
			Assert("Element exists", xmlReader.ReadToFollowing(elementName));
			if (expectedValue != null)
			{
				AssertEquals("Element value", expectedValue, xmlReader.ReadElementContentAsString());
			}
		}

		XmlTextReader xmlReader;
	}
}

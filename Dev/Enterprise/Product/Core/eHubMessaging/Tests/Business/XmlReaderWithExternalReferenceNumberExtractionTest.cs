using System.IO;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Business.DownloadHandler.Tests
{
	class XmlReaderWithExternalReferenceNumberExtractionTest : TestCase
	{
		public void TestXmlReaderWrapperForXmlWithExternalReferenceNumber()
		{
			AssertExternalReferenceNumber(GetValidXml("External"), "0cf5d5be-46b0-4044-a714-d92d9aa1c803");
		}

		public void TestXmlReaderWrapperForXmlWithoutExternalReferenceNumber()
		{
			AssertExternalReferenceNumber(GetValidXml("TrackingID"), string.Empty);
		}

		public void TestXmlReaderWrapperForXmlWithExternalReferenceNumberOutsideMessageNumberCollection()
		{
			AssertExternalReferenceNumber(GetValidXmlWithInvalidExternalReferenceNumber(), string.Empty);
		}

		void AssertExternalReferenceNumber(string xml, string expectedExternalReferenceNumber)
		{
			var stringBuilder = new StringBuilder();
			using (var sr = new StringReader(xml))
			using (var reader = XmlReader.Create(sr))
			using (var readerWrapper = new XmlReaderWithExternalReferenceNumberExtraction(reader))
			using (var writer = XmlWriter.Create(stringBuilder))
			{
				writer.WriteNode(readerWrapper, defattr: false);
				writer.Flush();
				AssertEquals(xml, stringBuilder.ToString());
				AssertEquals(expectedExternalReferenceNumber, readerWrapper.ExternalReferenceNumber);
			}
		}

		string GetValidXml(string messageNumberType)
		{
			return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Event>
		<EventType>ARV</EventType>
		<MessageNumberCollection>
			<MessageNumber Type=""{messageNumberType}"">0cf5d5be-46b0-4044-a714-d92d9aa1c803</MessageNumber>
		</MessageNumberCollection>
	</Event>
</UniversalEvent>";
		}

		string GetValidXmlWithInvalidExternalReferenceNumber()
		{
			return @"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"">
	<Event>
		<EventType>ARV</EventType>
		<MessageNumberCollection>
			<MessageNumber Type=""TrackingID"">CE2C080E-7F3D-4ACE-9363-4A7288F04C66</MessageNumber>
		</MessageNumberCollection>
		<MessageNumber Type=""External"">0cf5d5be-46b0-4044-a714-d92d9aa1c803</MessageNumber>
	</Event>
</UniversalEvent>";
		}
	}
}

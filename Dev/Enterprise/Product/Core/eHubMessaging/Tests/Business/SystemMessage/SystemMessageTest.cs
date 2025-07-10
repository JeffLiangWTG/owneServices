namespace Enterprise.eHubMessaging.Tests
{
	using System;
	using System.IO;
	using System.Xml;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.eHubMessaging.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Messaging.Business;
	using Enterprise.Messaging.Business.Testing;
	using Enterprise.Messaging.Integration;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Xml;

	public class SystemMessageTest : TestCaseWithFactory
	{
		public void TestCreateInterchange()
		{
			TestHelpers.DropSequenceIfExists("SystemEDIInterchangeNumber-00000000-0000-0000-0000-000000000000");
			SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "123AAA456");
			const string expectedBodyStartFormat =
@"<SystemInterchange xmlns=""http://www.cargowise.com/Schemas/System"">
  <Header>
    <SenderID>{0}</SenderID>
    <RecipientID>{1}</RecipientID>
  </Header>
  <Body>
    ";
			var currentLicenceKeyIdentifier = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			const string expectedBodyEnd = "\r\n  </Body>\r\n</SystemInterchange>";

			// default recipient
			string messageXml = "<SomeMessage xmlns=\"\">foo</SomeMessage>";
			var interchange = SystemMessage.CreateInterchange(Factory, messageXml);
			string expectedBody = string.Format(expectedBodyStartFormat, currentLicenceKeyIdentifier, "123AAA456") + messageXml + expectedBodyEnd;
			Factory.Save();
			AssertEquals("EI_ApplicationCode", ApplicationCodeList.Codes.SYS, interchange.EI_ApplicationCode);
			AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("EI_Status", EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertEquals("EI_From", currentLicenceKeyIdentifier, interchange.EI_From);
			AssertEquals("EI_To", "123AAA456", interchange.EI_To);
			AssertEquals("EI_GB", GlbBranch.CurrentBranch.PK, interchange.EI_GB);
			AssertEquals("EI_InterchangeNum", "SYS00000000000000001", interchange.EI_InterchangeNum);
			AssertEquals("EI_BodyText", expectedBody, interchange.EI_BodyText);
			AssertEquals("ContainedMessages", 0, interchange.ContainedMessages.Count);

			// recipient specified
			interchange = SystemMessage.CreateInterchange(Factory, messageXml, "RECIPIENT");
			expectedBody = string.Format(expectedBodyStartFormat, currentLicenceKeyIdentifier, "RECIPIENT") + messageXml + expectedBodyEnd;
			Factory.Save();
			AssertEquals("EI_To", "RECIPIENT", interchange.EI_To);
			AssertEquals("EI_InterchangeNum", "SYS00000000000000002", interchange.EI_InterchangeNum);
			AssertEquals("EI_BodyText", expectedBody, interchange.EI_BodyText);

			// message with xml declaration
			string xmlDeclaration = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n	";
			messageXml = "<Parent xmlns=\"\">\r\n      <Child>stuff</Child>\r\n    </Parent>";
			expectedBody = string.Format(expectedBodyStartFormat, currentLicenceKeyIdentifier, "123AAA456") + messageXml + expectedBodyEnd;
			interchange = SystemMessage.CreateInterchange(Factory, xmlDeclaration + messageXml);
			AssertEquals("EI_BodyText does not contain message XML declaration", expectedBody, interchange.EI_BodyText);
		}

		public void TestCreateSecureInterchange()
		{
			SystemDataRegistry.Instance.EdiProdLicenceIdentifier.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ENTCOMSER");
			string messageName = "SomeMessage";
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(string));
			string valueObject = "12345";
			string recipientId = null;
			var interchange = SystemMessage.CreateSecureInterchange(Factory, messageName, serializer, valueObject, recipientId);
			Factory.Save();

			string expectedBodyStart =
@"<SystemInterchange xmlns=""http://www.cargowise.com/Schemas/System"">
  <Header>
    <SenderID>{0}</SenderID>
    <RecipientID>ENTCOMSER</RecipientID>
  </Header>
  <Body>
    <SomeMessage compressed=""1""";

			var currentLicenceKeyIdentifier = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			AssertStartsWith("EI_BodyText", string.Format(expectedBodyStart, currentLicenceKeyIdentifier), interchange.EI_BodyText);
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(interchange.EI_BodyText);
			var bodyNode = doc.SelectSingleNode("/*[local-name()='SystemInterchange']/*[local-name()='Body']");
			var messageNode = bodyNode.FirstChild;
			AssertEquals(messageName, messageNode.LocalName);
			string expectedMessage = @"<?xml version=""1.0""?>
<string>12345</string>";

			AssertEquals(expectedMessage, SystemMessage.Unpack(messageNode.InnerText));
		}

		public void TestDeserialize()
		{
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(string));
			string valueObject = "12345";
			string messageName = "SomeMessage";
			var interchange = SystemMessage.CreateSecureInterchange(Factory, messageName, serializer, valueObject);
			var reader = new XmlTextReader(interchange.GetEI_BodyTextReader());
			reader.ReadToFollowing(messageName);
			var ediMessage = EDIMessageTestFactory.New(Factory);
			using (var textReader = new StreamReader(LargeMessageHelper.GetStreamFromNode(reader)))
			{
				ediMessage.EM_MessageText = textReader.ReadToEnd();
			}
			string messageText = (string)SystemMessage.Deserialize(ediMessage, serializer);
			AssertEquals(valueObject, messageText);
		}

		public void TestPack()
		{
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(string));
			string valueObject = ZString.Replicate('1', 1000);
			string encrypted = SystemMessage.Pack(serializer, valueObject, "a");
			Assert("Compressed before encrypting " + encrypted.Length, encrypted.Length < 500);

			string decrypted = SystemMessage.Unpack(encrypted, "a");
			AssertEquals("decrypted", "<?xml version=\"1.0\"?>\r\n<string>" + valueObject + "</string>", decrypted);
		}
	}
}

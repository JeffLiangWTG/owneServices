using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.KR.Business.Testing
{
	class KRCInterchangeProviderTest : TestCaseWithFactory
	{
		public void TestPopulateInterchangeForOriginalMessage()
		{
			var messages = new NonDependentEDIMessageCollection(Factory) { new TestDataSetupHelper(Factory).Create830EDIMessage() };
			var provider = new KRCInterchangeProvider(messages);
			AssertCommonElements(provider);

			var interchangeBody = MessageEncoding.UTF8WithoutBOM.GetString(provider.Interchanges[0].EI_BodyData);
			AssertContains("Authorization: Basic cmVhZHlrb3JlYQ==", interchangeBody);
			AssertContains(@"Content-Type: multipart/related; type=""text/xml""; boundary=", interchangeBody);
			AssertContains(@"Content-Id: <SOAPPART>", interchangeBody);
			AssertContains(@"Content-Id: <payload-1>", interchangeBody);
			AssertEquals(2, Regex.Matches(interchangeBody, "<ds:Signature ").Count);
			AssertEquals(2, Regex.Matches(interchangeBody, "Content-Type: text/xml; charset=UTF-8").Count);
			var matches = Regex.Matches(interchangeBody, @"boundary=""(.*?)""");
			AssertEquals(1, matches.Count);
			var boundary = matches[0].Groups[1].Value;
			AssertEquals(2, Regex.Matches(interchangeBody, @"(^|\s)--" + boundary + @"(\s|$)").Count);
		}

		public void TestPopulateInterchangeForDLTMessage()
		{
			var messages = new NonDependentEDIMessageCollection(Factory) { CreateMessage(Constants.EDIInterchangeType.DLT) };
			var provider = new KRCInterchangeProvider(messages);
			AssertCommonElements(provider);
			var interchangeBody = MessageEncoding.UTF8WithoutBOM.GetString(provider.Interchanges[0].EI_BodyData);
			AssertEquals(1, Regex.Matches(interchangeBody, "<ds:Signature ").Count);
			AssertEquals(1, Regex.Matches(interchangeBody, "Content-Type: text/xml; charset=UTF-8").Count);
		}

		public void TestPopulateInterchangeForDOCMessage()
		{
			var messages = new NonDependentEDIMessageCollection(Factory) { CreateMessage(Constants.EDIInterchangeType.DOC) };
			var provider = new KRCInterchangeProvider(messages);
			AssertCommonElements(provider);
			var interchangeBody = MessageEncoding.UTF8WithoutBOM.GetString(provider.Interchanges[0].EI_BodyData);
			AssertEquals(1, Regex.Matches(interchangeBody, "<ds:Signature ").Count);
			AssertEquals(1, Regex.Matches(interchangeBody, "Content-Type: text/xml; charset=UTF-8").Count);
		}

		EDIMessage CreateMessage(string messageType)
		{
			var message = Factory.New<EDIMessage>();
			message.EM_MessageType = messageType;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			return message;
		}

		void AssertCommonElements(KRCInterchangeProvider provider)
		{
			new CertificateTestHelper().GetGlbExternalPassword("12840.pfx", "rk34879800!");
			CertificateTestHelper.SetCustomsPublicKey();
			AssertEquals("NumberOfInterchanges", 1, provider.Interchanges.Length);

			var interchange = provider.Interchanges[0];
			AssertEquals("Interchange Header should be empty", ZString.Empty, interchange.EI_HeaderText);

			var interchangeBody = MessageEncoding.UTF8WithoutBOM.GetString(interchange.EI_BodyData);
			AssertContains(@"SOAPACTION: ""ebXML""", interchangeBody);
			AssertContains("Authorization: Basic ", interchangeBody);
		}
	}
}

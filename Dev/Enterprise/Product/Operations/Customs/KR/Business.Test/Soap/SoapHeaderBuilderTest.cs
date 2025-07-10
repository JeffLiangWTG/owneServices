using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Customs.KR.MessageDefinitions.SoapEnvelope.SOAP;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class SoapHeaderBuilderTest : TestCaseWithFactory
	{
		public static Stream EnvelopeSerializeAsStream<T>(T xmlObject)
		{
			var namespaces = new XmlSerializerNamespaces();
			namespaces.Add("SOAP", @"http://schemas.xmlsoap.org/soap/envelope/");
			namespaces.Add("xlink", @"http://www.w3.org/1999/xlink");
			namespaces.Add("xsi", @"http://www.w3.org/2001/XMLSchema-instance");

			var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

			return SoapHeaderBuilder.SerializeAsStream(xmlObject, settings, namespaces);
		}

		[TestDate(2021, 03, 03)]
		public void TestSoapHederSend()
		{
			var entry = new TestDataSetupHelper(Factory).GetEntryHas830Snapshot();
			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange1.EI_Status = EDIInterchange.Status.Queued;
			interchange1.EI_ApplicationCode = EDIInterchange.ApplicationCodes.KRCustoms;
			interchange1.EI_InterchangeType = ElectronicDocumentTypeList.Codes._830;
			interchange1.EI_SessionGUID = new ZGuid("2d38c654-20ee-41a8-852d-b075102b5801");
			interchange1.EI_From = "KR Customs";
			interchange1.EI_To = "TEST";
			interchange1.EI_BodyText = "";

			var message = interchange1.ContainedMessages.AddNew();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.KRCustoms;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_LinkTable = "CusEntryHeader";
			message.EM_LinkedObject = entry;
			message.EM_MessageNum = "1";

			var password = Factory.New<GlbExternalPassword>();
			password.GP_PasswordType = "KRB";
			password.GP_UserID = "3";
			password.GP_MailBoxID = "VC102814229901";
			password.GP_GC = GlbBranch.CurrentBranch.GB_GC;
			password.GP_Certificate = new byte[] { 48, 130, 8 };

			var result = new SoapHeaderBuilder(interchange1).GenerateMessage();
			AssertXML(result, "VC102814229901", "KCSSingleAction", "6N00221000025X", "2d38c654-20ee-41a8-852d-b075102b5801", "GOVCBR830");
		}

		void AssertXML(Envelope result, string mailboxid, string action, string conversationId, string messageId, string documenttype)
		{
			using (var makeStream = EnvelopeSerializeAsStream(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				var xmldoc = new XmlDocument();
				xmldoc.LoadXml(serialisedXml);

				var nsmgr = new XmlNamespaceManager(xmldoc.NameTable);
				nsmgr.AddNamespace("SOAP", @"http://schemas.xmlsoap.org/soap/envelope/");
				nsmgr.AddNamespace("eb", @"http://www.oasis-open.org/committees/ebxml-msg/schema/msg-header-2_0.xsd");

				XmlNode header = xmldoc.SelectSingleNode("//SOAP:Envelope/SOAP:Header/eb:MessageHeader", nsmgr);

				string getMailboxid = header.SelectSingleNode("eb:From/eb:PartyId", nsmgr).InnerText;
				string getAction = header.SelectSingleNode("eb:Action", nsmgr).InnerText;
				string getConversationId = header.SelectSingleNode("eb:ConversationId", nsmgr).InnerText;
				string getMessageId = header.SelectSingleNode("eb:MessageData/eb:MessageId", nsmgr).InnerText;
				string getDocumenttype = header.SelectSingleNode("eb:Description", nsmgr).InnerText;

				AssertEquals(mailboxid, getMailboxid);
				AssertEquals(action, getAction);
				AssertEquals(conversationId, getConversationId);
				AssertEquals(messageId, getMessageId);
				AssertEquals(documenttype, getDocumenttype);
			}
		}

		[TestDate(2021, 03, 03)]
		public void TestSoapHederList()
		{
			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange1.EI_Status = EDIInterchange.Status.Queued;
			interchange1.EI_ApplicationCode = EDIInterchange.ApplicationCodes.KRCustoms;
			interchange1.EI_InterchangeType = EDIInterchangeType.DLT;
			interchange1.EI_SessionGUID = new ZGuid("2d38c654-20ee-41a8-852d-b075102b5801");
			interchange1.EI_From = "KR Customs";
			interchange1.EI_To = "TEST";
			interchange1.EI_BodyText = "";

			var message = interchange1.ContainedMessages.AddNew();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.KRCustoms;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_MessageNum = "2";

			var password = Factory.New<GlbExternalPassword>();
			password.GP_PasswordType = "KRB";
			password.GP_UserID = "3";
			password.GP_MailBoxID = "VC102814229901";
			password.GP_GC = GlbBranch.CurrentBranch.GB_GC;
			password.GP_Certificate = new byte[] { 48, 130, 8 };

			var result = new SoapHeaderBuilder(interchange1).GenerateMessage();
			AssertXML(result, "VC102814229901", "KCSListReqAction", "", "2d38c654-20ee-41a8-852d-b075102b5801", "REQLST-000");
		}

		[TestDate(2021, 03, 03)]
		public void TestSoapHederDoc()
		{
			var queryGUID = "4142285b-6b4f-4eb8-9bfa-6baa3b884b06";
			var cusPollingTransaction = Factory.New<CusPollingTransaction>();
			cusPollingTransaction.CPT_Reference = ElectronicDocumentTypeList.Codes._830;
			cusPollingTransaction.CPT_Status = "OPN";
			cusPollingTransaction.CPT_NumberOfAttempts = 1;
			cusPollingTransaction.CPT_TransactionID = queryGUID;

			var interchange1 = Factory.New<EDIInterchange>();
			interchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			interchange1.EI_Status = EDIInterchange.Status.Queued;
			interchange1.EI_ApplicationCode = EDIInterchange.ApplicationCodes.KRCustoms;
			interchange1.EI_InterchangeType = EDIInterchangeType.DOC;
			interchange1.EI_SessionGUID = new ZGuid("2d38c654-20ee-41a8-852d-b075102b5801");
			interchange1.EI_From = "KR Customs";
			interchange1.EI_To = "TEST";
			interchange1.EI_BodyText = "";

			var message = interchange1.ContainedMessages.AddNew();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.KRCustoms;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_LinkTable = "cusPollingTransaction";
			message.EM_LinkedObject = cusPollingTransaction;

			var password = Factory.New<GlbExternalPassword>();
			password.GP_PasswordType = "KRB";
			password.GP_UserID = "3";
			password.GP_MailBoxID = "VC102814229901";
			password.GP_GC = GlbBranch.CurrentBranch.GB_GC;
			password.GP_Certificate = new byte[] { 48, 130, 8 };

			var result = new SoapHeaderBuilder(interchange1).GenerateMessage();
			AssertXML(result, "VC102814229901", "KCSFileReqAction", "4142285b-6b4f-4eb8-9bfa-6baa3b884b06", "2d38c654-20ee-41a8-852d-b075102b5801", "DOCFND-000");
		}
	}
}

using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	class EMCSInboundInterchangeProcessorTests : TestCaseWithFactory
	{
		public void TestProcessEmptyInterchange()
		{
			var serviceRef = "67890";
			var interchangeNum = "1234";
			var interchange = CreateInterchange(interchangeNum, string.Empty, ServiceReferenceResponseHeader, serviceReference: serviceRef);

			var processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			processor.ProcessInterchangeForTest(interchange);

			AssertEquals("Empty interchange, should have no messages", 0, interchange.ContainedMessages.Count);
		}

		public void TestProcessInterchangeWithInvalidMessage()
		{
			var serviceRef = "67890";
			var interchangeNum = "1234";
			var interchange = CreateInterchange(interchangeNum, InvalidMessage, ServiceReferenceResponseHeader, serviceReference: serviceRef);

			var processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			processor.ProcessInterchangeForTest(interchange);

			AssertEquals(1, interchange.ContainedMessages.Count);
			CheckMessage(interchange.ContainedMessages[0], EDIMessage.Status.Failed, ZString.Empty, ExpectedInvalidMessage, serviceRef, interchangeNum);
		}

		public void TestProcessInterchangeWithValidMessage1()
		{
			var serviceRef = "67890";
			var interchangeNum = "1234";
			var interchange = CreateInterchange(interchangeNum, ValidMessage1, ServiceReferenceResponseHeader, serviceReference: serviceRef);

			var processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			processor.ProcessInterchangeForTest(interchange);

			AssertEquals(1, interchange.ContainedMessages.Count);
			CheckMessage(interchange.ContainedMessages[0], EDIMessage.Status.Queued, "837", ExpectedValidMessage1, serviceRef, interchangeNum);
		}

		public void TestProcessInterchangeWithValidMessage2()
		{
			var serviceRef = "67890";
			var interchangeNum = "1234";
			var interchange = CreateInterchange(interchangeNum, ValidMessage2, ServiceReferenceResponseHeader, serviceReference: serviceRef);

			var processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			processor.ProcessInterchangeForTest(interchange);

			AssertEquals(1, interchange.ContainedMessages.Count);
			CheckMessage(interchange.ContainedMessages[0], EDIMessage.Status.Queued, "829", ExpectedValidMessage2, serviceRef, interchangeNum);
		}

		public void TestProcessInterchangeWithValidMessage1WithoutJsonEncapsulation()
		{
			var serviceRef = "67890";
			var interchangeNum = "1234";
			var interchange = CreateInterchange(interchangeNum, ExpectedValidMessage1, ServiceReferenceResponseHeader, serviceReference: serviceRef);

			var processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			processor.ProcessInterchangeForTest(interchange);

			AssertEquals(1, interchange.ContainedMessages.Count);
			CheckMessage(interchange.ContainedMessages[0], EDIMessage.Status.Queued, "837", ExpectedValidMessage1, serviceRef, interchangeNum);
		}

		public void TestProcessInterchangeWithValidMessage2WithoutJsonEncapsulation()
		{
			var serviceRef = "67890";
			var interchangeNum = "1235";
			var interchange = CreateInterchange(interchangeNum, ExpectedValidMessage2, ServiceReferenceResponseHeader, serviceReference: serviceRef);

			var processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			processor.ProcessInterchangeForTest(interchange);

			AssertEquals(1, interchange.ContainedMessages.Count);
			CheckMessage(interchange.ContainedMessages[0], EDIMessage.Status.Queued, "829", ExpectedValidMessage2, serviceRef, interchangeNum);
		}

		void CheckMessage(EDIMessage message, ZString expectedStatus, ZString expectedType, ZString expectedBodyText, ZString expectedApplicationReference, ZString expectedNum)
		{
			AssertXMLEquals(expectedBodyText, message.EM_MessageText);
			AssertEquals(expectedApplicationReference, message.EM_ApplicationReference);
			AssertEquals(expectedNum, message.EM_MessageNum);
			AssertEquals(expectedStatus, message.EM_Status);
			AssertEquals(expectedType, message.EM_MessageType);
		}

		public void TestProcessInterchangeSpawnedMessageLinkedObjectWitheHubTrackingId()
		{
			var responseHeader = "<eHubTrackingId><<eHubTrackingId>></eHubTrackingId>";
			var eHubTrackingID = "02C64674-184A-4A6D-9CCC-EF53FBB5C512";
			var interchangeNum = "1234";
			var interchange = CreateInterchange(interchangeNum, ValidMessage1, responseHeader, eHubTrackingID);
			var processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			_ = processor.ProcessInterchangeForTest(interchange);
			CheckInterchangeSpawnedMessage(interchange, interchangeNum);

			var declaration = SetupDeclarationAndOutgoingMessage(eHubTrackingID);
			interchangeNum = "1235";
			interchange = CreateInterchange(interchangeNum, ValidMessage1, responseHeader, eHubTrackingID);
			processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			_ = processor.ProcessInterchangeForTest(interchange);
			CheckInterchangeSpawnedMessage(interchange, interchangeNum, declaration);
		}

		public void TestProcessInterchangeSpawnedMessageLinkedObjectWithJobNumber()
		{
			var responseHeader = "<JobNumber><<JobNumber>></JobNumber>";
			var jobNumber = "JOBNUMBERVALUE";
			var interchangeNum = "1234";
			var interchange = CreateInterchange(interchangeNum, ValidMessage1, responseHeader, string.Empty, jobNumber);
			var processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			_ = processor.ProcessInterchangeForTest(interchange);
			CheckInterchangeSpawnedMessage(interchange, interchangeNum);

			var declaration = SetupDeclarationAndOutgoingMessage(string.Empty, jobNumber);
			interchangeNum = "1235";
			interchange = CreateInterchange(interchangeNum, ValidMessage1, responseHeader, string.Empty, jobNumber);
			processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			_ = processor.ProcessInterchangeForTest(interchange);
			CheckInterchangeSpawnedMessage(interchange, interchangeNum, declaration);
		}

		public void TestProcessInterchangeSpawnedMessageLinkedObjectWithApplicationReference()
		{
			var serviceReference = "SERVICEREFERENCEVALUE";
			var interchangeNum = "1234";
			var interchange = CreateInterchange(interchangeNum, ValidMessage1, ServiceReferenceResponseHeader, string.Empty, string.Empty, serviceReference);
			var processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			_ = processor.ProcessInterchangeForTest(interchange);
			CheckInterchangeSpawnedMessage(interchange, interchangeNum, null, serviceReference);

			var declaration = SetupDeclarationAndOutgoingMessage(string.Empty, string.Empty, serviceReference);
			interchangeNum = "1235";
			interchange = CreateInterchange(interchangeNum, ValidMessage1, ServiceReferenceResponseHeader, string.Empty, string.Empty, serviceReference);
			processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			_ = processor.ProcessInterchangeForTest(interchange);
			CheckInterchangeSpawnedMessage(interchange, interchangeNum, declaration, serviceReference);
		}

		public void TestProcessInterchangeSpawnedMessageLinkedObjectWithApplicationReferenceAsJobNumber()
		{
			var serviceReference = "SERVICEREFERENCEVALUE";
			var interchangeNum = "1234";

			var declaration = SetupDeclarationAndOutgoingMessage(string.Empty, serviceReference, string.Empty);
			var interchange = CreateInterchange(interchangeNum, ValidMessage1, ServiceReferenceResponseHeader, string.Empty, string.Empty, serviceReference);
			var processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			_ = processor.ProcessInterchangeForTest(interchange);
			AssertEquals(1, interchange.ContainedMessages.Count);
			var message = interchange.ContainedMessages[0];
			AssertEquals(interchangeNum, message.EM_MessageNum);
			AssertType<EMCSJobDeclaration>(message.EM_LinkedObject);
			AssertEquals(declaration, message.EM_LinkedObject);
			AssertEquals(serviceReference, message.EM_ApplicationReference);
		}

		public void TestProcessInvalidInterchangeChangesDeclarationMessageStatus()
		{
			var responseHeader = "<eHubTrackingId><<eHubTrackingId>></eHubTrackingId>";
			var eHubTrackingID = "02C64674-184A-4A6D-9CCC-EF53FBB5C512";

			var declaration = SetupDeclarationAndOutgoingMessage(eHubTrackingID);
			var interchange = CreateInterchange("1234", InvalidMessage, responseHeader, eHubTrackingID);
			var processor = new EMCSInboundInterchangeProcessorForTest(new LoggingInformation());
			_ = processor.ProcessInterchangeForTest(interchange);

			AssertEquals(EDIMessageStatusList.Codes.Error, declaration.JE_MessageStatus);
		}

		void CheckInterchangeSpawnedMessage(EMCSInterchange interchange, string expectedInterchangeNumber, BusinessObject expectedLinkedObject = null, string expectedApplicationReference = "")
		{
			AssertEquals(1, interchange.ContainedMessages.Count);
			var message = interchange.ContainedMessages[0];
			AssertEquals(expectedInterchangeNumber, message.EM_MessageNum);
			AssertEquals(expectedApplicationReference, message.EM_ApplicationReference);

			if (expectedLinkedObject == null)
			{
				AssertNull(message.EM_LinkedObject);
			}
			else
			{
				AssertType<EMCSJobDeclaration>(message.EM_LinkedObject);
				AssertEquals(expectedLinkedObject, message.EM_LinkedObject);
			}
		}

		EMCSInterchange CreateInterchange(string interchangeNumber, string messageBody, string responseHeader, string eHubTrackingId = "", string jobNumber = "", string serviceReference = "")
		{
			var interchange = Factory.New<EMCSInterchange>();
			interchange.EI_ReceiveTransmit = EDIInterchange.Status.Received;
			interchange.EI_Status = EDIInterchange.Status.Received;
			interchange.EI_From = "eHub";
			interchange.EI_To = "WTG";
			interchange.EI_InterchangeNum = interchangeNumber;
			var encodedMessageBody = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(messageBody));
			var interchangeText = EmptyInterchange.Replace(ResponseHeaderPlaceHolder, responseHeader)
				.Replace(ResponseBodyPlaceholder, encodedMessageBody)
				.Replace(EHubTrackingIdPlaceholder, eHubTrackingId)
				.Replace(JobNumberPlaceholder, jobNumber)
				.Replace(ServiceReferencePlaceholder, serviceReference);
			interchange.EI_BodyText = interchangeText;
			return interchange;
		}

		EMCSJobDeclaration SetupDeclarationAndOutgoingMessage(string eHubTrackingId = "", string jobNumber = "", string applicationReference = "")
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_DeclarationReference = jobNumber;
			declaration.JE_HouseBill = "TESTHOUSE";

			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_SessionGUID = new ZGuid(eHubTrackingId);

			var outgoingMessage = outgoingInterchange.ContainedMessages.AddNew(typeof(EMCSOutboundEDIMessage));
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			outgoingMessage.EM_ApplicationReference = applicationReference;
			outgoingMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, EDIMessage.ApplicationCodes.GbCustomsEMCS);
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_MessageNum = "999";

			outgoingInterchange.EI_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingInterchange.EI_Status = EDIMessage.Status.Acknowledged;
			outgoingInterchange.EI_From = "AR1";
			outgoingInterchange.EI_To = "GOD";
			outgoingInterchange.EI_BodyText = "";

			declaration.Messages.Add(outgoingMessage);
			Factory.Save();

			return declaration;
		}

		class EMCSInboundInterchangeProcessorForTest : EMCSInboundInterchangeProcessor
		{
			public EMCSInboundInterchangeProcessorForTest(LoggingInformation logger) : base(logger)
			{
			}

			public bool ProcessInterchangeForTest(EDIInterchange interchange) => ProcessInterchange(interchange);
		}

		const string ResponseHeaderPlaceHolder = "<<<ResponseHeader>>>";
		const string ResponseBodyPlaceholder = "<<<ResponseBody>>>";
		const string EHubTrackingIdPlaceholder = "<<eHubTrackingId>>";
		const string JobNumberPlaceholder = "<<JobNumber>>";
		const string ServiceReferencePlaceholder = "<<ServiceReference>>";

		const string ServiceReferenceResponseHeader = "<ServiceReference><<ServiceReference>></ServiceReference>";
		const string EmptyInterchange = @"<s0:GBCustomsBusinessResponse xmlns:s0=""http://cargowise.com/ehub/products/GBCustoms"">
  <s0:ResponseHeader Provider=""EMCS"">
    <<<ResponseHeader>>>
  </s0:ResponseHeader>
  <s0:ResponseBody ContentType=""XML"" Encoding=""base64"">
    <<<ResponseBody>>>
  </s0:ResponseBody>
</s0:GBCustomsBusinessResponse>";
		const string InvalidMessage = @"[{""encodedMessage"":""PEV4YW1wbGVNZXNzYWdlMSB4bWxucz0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvRXhjaXNlL05ld01lc3NhZ2VzRGF0YS8zIi8+"",""messageType"":""IE801"",""createdOn"":""2024-02-07T14:36:56.054536Z""}]";
		const string ValidMessage1 = @"[{""encodedMessage"":""PG5zMTpJRTgzNyB4bWxucz0iaHR0cDovL3d3dy5nb3Z0YWxrLmdvdi51ay90YXhhdGlvbi9JbnRlcm5hdGlvbmFsVHJhZGUvRXhjaXNlL05ld01lc3NhZ2VzRGF0YS8zIgp4bWxuczp0bXM9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6VE1TOlYzLjEzIgp4bWxuczpuczE9InVybjpwdWJsaWNpZDotOkVDOkRHVEFYVUQ6RU1DUzpQSEFTRTQ6SUU4Mzc6VjMuMTMiCnhtbG5zOnhzaT0iaHR0cDovL3d3dy53My5vcmcvMjAwMS9YTUxTY2hlbWEtaW5zdGFuY2UiPgo8bnMxOkhlYWRlcj48L25zMTpIZWFkZXI+CjxuczE6Qm9keT48L25zMTpCb2R5Pgo8L25zMTpJRTgzNz4="",""messageType"":""IE837"",""createdOn"":""2024-02-07T14:36:56.054536Z""}]";
		const string ValidMessage2 = @"[{""encodedMessage"":""PHExOklFODI5IHhtbG5zOnExPSJ1cm46cHVibGljaWQ6LTpFQzpER1RBWFVEOkVNQ1M6UEhBU0U0OklFODI5OlYzLjEzIj4KCQkJCQkJCQkJICA8cTE6SGVhZGVyPjwvcTE6SGVhZGVyPgoJCQkJCQkJCQkgIDxxMTpCb2R5PjwvcTE6Qm9keT4KCQkJCQkJCQkJPC9xMTpJRTgyOT4="",""messageType"":""IE829"",""createdOn"":""2024-02-07T14:36:56.054536Z""}]";

		const string ExpectedInvalidMessage = @"<ExampleMessage1 xmlns=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Excise/NewMessagesData/3""/>";
		const string ExpectedValidMessage1 = @"<ns1:IE837 xmlns=""http://www.govtalk.gov.uk/taxation/InternationalTrade/Excise/NewMessagesData/3""
xmlns:tms=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:TMS:V3.13""
xmlns:ns1=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE837:V3.13""
xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<ns1:Header></ns1:Header>
<ns1:Body></ns1:Body>
</ns1:IE837>";
		const string ExpectedValidMessage2 = @"<q1:IE829 xmlns:q1=""urn:publicid:-:EC:DGTAXUD:EMCS:PHASE4:IE829:V3.13"">
									  <q1:Header></q1:Header>
									  <q1:Body></q1:Body>
									</q1:IE829>";
	}
}

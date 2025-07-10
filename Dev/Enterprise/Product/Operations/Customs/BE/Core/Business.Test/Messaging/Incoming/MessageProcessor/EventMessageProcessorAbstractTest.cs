using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using MessageStatusList = Enterprise.Customs.Common.Shared.MessageStatusList;

namespace Enterprise.Customs.BE.Business.Testing;

public abstract class EventMessageProcessorAbstractTest<T> : MessageProcessorTestCase<EventMessageProcessor, UniversalEventWrapper> where T : EventMessageProcessor
{
	protected override string ExpectedMessageFriendlyName => BEIncomingMessageSubTypes.Descriptions.CustomsServiceErrorUniversalEvent;

	protected override Type ExpectedMessageInterpreterType => null;

	public void TestApplicationCode()
	{
		AssertEquals(ApplicationCodeList.Codes.BECustoms, processor.ApplicationCode);
	}

	public void TestMessageTypesToInclude()
	{
		AssertSequencesEqual(new ZString[] { BEIncomingMessageSubTypes.Codes.CustomsServiceErrorUniversalEvent }, processor.MessageTypesToInclude);
	}

	public abstract void TestProcessServiceErrorMessage();

	protected EDIInterchange SetSentInterchange(BusinessObject businessObject)
	{
		var sentInterchange = Factory.New<EDIInterchange>();
		sentInterchange.EI_SessionGUID = ZGuid.BrettsGuid;
		sentInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.BECustoms;
		sentInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		sentInterchange.EI_From = "CW1";
		sentInterchange.EI_To = "BECustoms";
		sentInterchange.EI_Status = MessageStatusList.Codes.Sent;

		var sentMessage = Factory.New<BEMessage>();
		sentMessage.EM_ApplicationCode = ApplicationCodeList.Codes.BECustoms;
		sentMessage.EM_Status = MessageStatusList.Codes.Sent;
		sentMessage.EM_LinkedObject = businessObject;
		sentMessage.EM_EI = sentInterchange.PK;
		sentInterchange.ContainedMessages.Add(sentMessage);

		return sentInterchange;
	}

	protected BEMessage CreateNewEDIMessage(ZString reference, ZString messageText, ZGuid interchangeID, string interchangeTransportType = EDIInterchange.TransportType.eHub)
	{
		var message = Factory.New<BEMessage>();
		message.EM_ApplicationCode = ApplicationCodeList.Codes.BECustoms;
		message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
		message.EM_Status = EDIMessage.Status.Queued;
		message.EM_MessageSubType = "AAA";
		message.EM_ApplicationReference = reference;
		message.EM_GB = new ZGuid();
		message.EM_MessageText = messageText;

		var responseInterchange = CreateTestResponseInterchange(interchangeID, interchangeTransportType);
		responseInterchange.ContainedMessages.Add(message);

		Factory.Save();
		return message;
	}

	EDIInterchange CreateTestResponseInterchange(ZGuid interchangeID, ZString interchangeTransportType)
	{
		var responseInterchange = Factory.New<EDIInterchange>();
		responseInterchange.EI_ApplicationCode = ApplicationCodeList.Codes.ESCustomsMessage;
		responseInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
		responseInterchange.EI_SessionGUID = interchangeID;
		responseInterchange.EI_From = "BECustoms";
		responseInterchange.EI_To = "CW1";
		responseInterchange.EI_TransportType = interchangeTransportType;
		responseInterchange.EI_HeaderText = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
				<Headers>
				  <BrokerCode>AZ</BrokerCode>
				  <CertificateName>CertName</CertificateName>
				  <CertificateThumbPrint>CertThumbPrint</CertificateThumbPrint>
				  <EntryReferenceNumber>1234123444</EntryReferenceNumber>
				  <TestMessage>N</TestMessage>
				  <Service>Service</Service>
				  <Operation>Operation</Operation>
				  <SentEDIMessageNumber>10</SentEDIMessageNumber>
				</Headers>");

		return responseInterchange;
	}

	protected override void SetUp()
	{
		base.SetUp();
		logger = new LoggingInformation();
		processor = GetProcessorCore(logger);
	}

	protected abstract T GetProcessorCore(LoggingInformation logger);

	LoggingInformation logger;
	protected T processor;

	protected string serviceErrorMessage = string.Format(@"<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""1.1"">
						<Event>
							<EventTime>2022-08-09 03:50:11.903</EventTime>
							<EventType>IRJ</EventType>
							<EventParameters>
								<Reason>The MRN 21ES00999930MXRZN5 exceeded the retry count of 5 and still receives no update</Reason>
								<MessageType>NPI</MessageType>
							</EventParameters>
						</Event>
					</UniversalEvent>");
}

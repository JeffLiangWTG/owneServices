using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PaymentStatusResponseProcessorTest : COLSMessageProcessorAbstractTest
	{
		public void TestProcessPaymentStatusResponseMessage_Success()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;

			var outboundMessage = colsHeader.Messages.AddNew();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.PaymentStatus;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;

			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.PaymentStatus;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"referenceNumberType\": \"INVOICE\", \"result\": \"SUCCESS\", \"message\": null }";
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process SUCCESS PaymentStatus response message", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.SuccessfulGetPaymentStatus, colsHeader.QCH_MessageStatus);
			});
		}

		public void TestProcessPaymentStatusResponseMessage_Failure()
		{
			var sessionID = ZGuid.NewZGuid();
			var outboundInterchange = Factory.New<EDIInterchange>();
			outboundInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outboundInterchange.EI_SessionGUID = sessionID;

			var outboundMessage = colsHeader.Messages.AddNew();
			outboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			outboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.PaymentStatus;
			outboundMessage.EM_EI = outboundInterchange.PK;
			outboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outboundMessage.EM_Status = EDIMessage.Status.Sent;

			var inboundInterchange = Factory.New<EDIInterchange>();
			inboundInterchange.EI_SessionGUID = sessionID;

			var inboundMessage = Factory.New<EDIMessage>();
			inboundMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.COLS;
			inboundMessage.EM_MessageType = AUCOLSMessageTypeList.Codes.PaymentStatus;
			inboundMessage.EM_EI = inboundInterchange.PK;
			inboundMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			inboundMessage.EM_Status = EDIMessage.Status.Queued;
			inboundMessage.EM_MessageText = "{ \"result\": \"FAILED\", \"message\": [ { \"messageCode\": \"EM.20\", \"messageText\": \"A valid reference number must be supplied\" } ] }";
			processor.ProcessMessage(inboundMessage);

			CombineAssertions("Process FAILED PaymentStatus response message", () =>
			{
				AssertEquals(colsHeader.PK, inboundMessage.EM_LinkedObject.PK);
				AssertEquals(COLSHeaderStatusList.Codes.FailedGetPaymentStatus, colsHeader.QCH_MessageStatus);
			});
		}

		protected override COLSMessageProcessor GetMessageProcessor() => new PaymentStatusResponseProcessor(new LoggingInformation());
	}
}

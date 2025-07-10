using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.IE.H7.Business.Testing
{
	class MessageAcknowledgementProcessorTest : TestCaseWithFactory
	{
		public void TestMessageAcknowlegement_Rejected_ROSError()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, parent: eun);
			helper.CreateNewOrGetExistingCusCodeType(IE.Business.Constants.RefCusCodeListTypes.RevenueErrorType, "IEROS");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, IE.Business.Constants.RefCusCodeListTypes.RevenueErrorType, "CODE", "DESCRIPTION", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var (asycudaBill, outgoingMessage, processor) = CreateOriginalData(Factory);

			var incomingMessage = InterchangeProcessorTestHelper.CreateMessageAcknowledgementServiceErrorMessage<AISInboundEDIMessage>(factory: Factory,
				applicationCode: EDIMessage.ApplicationCodes.IECustomsImport,
				messageType: AISInterchangeTypeList.Codes.IM415V,
				errorCode: "CODE"
			);

			incomingMessage.EM_LinkedObject = outgoingMessage.EM_LinkedObject;
			outgoingMessage.EM_Status = EDIMessageStatusList.Codes.Sent;

			Factory.Save();

			using (Factory.AddDisposableService())
			{
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				CombineAssertions("Result for processing ROS ERROR message.", () =>
				{
					AssertEquals("AsycudaBill.ABL_MessageStatus", "FAL", asycudaBill.ABL_MessageStatus);
					var messageTypeDesc = outgoingMessage.MessageTypeWithDescription;
					var expectedInterpretation = $@"Error submitting message. Error Code: CODE - DESCRIPTION<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Transaction ID</td><td>TRS0000001</td></tr><tr><td>Message Type</td><td>15V - IM415V: Customs Declaration Acknowledgment</td></tr><tr><td>Message Number</td><td>IEI00000000000001</td></tr></table><br />Entry status has been set ERR<br />";
					AssertXMLEquals("Interpretation", expectedInterpretation, incomingMessage.EM_MessageInterpretation);
					MessageProcessorNotificationTestHelper.AssertNoEmailsSent();
					AssertEquals("outgoingMessage.EM_Status", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
					AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.Received, incomingMessage.EM_Status);
				});
			}
		}

		const string transactionId = "TRS0000001";

		(AsycudaBill asycudaBill, AISOutboundEDIMessage outgoingMessage, MessageAcknowledgementProcessor processor) CreateOriginalData(BusinessObjectFactory factory, LoggingInformation logger = null)
		{
			var header = factory.New<AsycudaManifestHeader>();
			header.AMA_JobReference = "H700035624";
			var bill = header.Bills.AddNew();
			var outgoingMessage = factory.New<AISOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsImport;
			outgoingMessage.EM_MessageType = AISInterchangeTypeList.Codes.IM415V;
			outgoingMessage.EM_ApplicationReference = transactionId;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = bill;
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			return (bill, outgoingMessage, new MessageAcknowledgementProcessor(logger ?? new LoggingInformation()));
		}
	}
}

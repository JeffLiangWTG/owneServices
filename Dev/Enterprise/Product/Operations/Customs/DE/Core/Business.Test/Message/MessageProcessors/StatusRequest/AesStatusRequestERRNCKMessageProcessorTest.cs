using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(AesStatusRequestERRNCKMessageProcessor))]
	sealed class AesStatusRequestERRNCKMessageProcessorTest : MessageProcessorAbstractTest<AesStatusRequestERRNCKMessageProcessor, AesInboundEDIMessage<IERRNCK>>
	{
		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				var linkedObject = message.EM_LinkedObject;
				AssertType<StatusRequest>("Type", linkedObject);
				AssertEquals("Correct LinkedObject obtained", linkedObject, outgoingMessageAsStatusRequest);
			});
		}

		public void TestNoLinkedObject()
		{
			outgoingMessageAsStatusRequest.EM_MessageNum = "11111";
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", message.EM_LinkedObject);
				AssertEquals("Message status == error", EDIMessage.Status.Error, message.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IERRNCK)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("StatusRequest: EM_Status", EDIMessage.Status.Error, outgoingMessageAsStatusRequest.EM_Status);
				AssertEquals("StatusRequest: MRN => LogbookRegistrationNumber", ReferenceNumber, message.GetLogbookRegistrationNumber());
			});
		}

		public void TestProcessMessage_LogBookRegistrationNumberFallBack()
		{
			var testCases = new[] { string.Empty, null };
			foreach (var testCase in testCases)
			{
				errnckMock.Setup(m => m.ReferenceNumber).Returns(testCase);
				ProcessMessage(message);
				AssertEquals(ReferencedMessageIdentifier, message.GetLogbookRegistrationNumber());
			}
		}

		protected override ZString MessageFriendlyName => "AES StatusRequest ERRNCK Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IERRNCK>> Processor => new AesStatusRequestERRNCKMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			errnckMock = new Mock<IERRNCK>();
			errnckMock.Setup(x => x.ReferenceNumber).Returns(ReferenceNumber);
			errnckMock.Setup(x => x.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IERRNCK>>();
			messageMock.Setup(x => x.DataProvider).Returns(errnckMock.Object);
			message = messageMock.Object;

			outgoingMessageAsStatusRequest = Factory.New<StatusRequest>();
			outgoingMessageAsStatusRequest.Module = ExportStatusRequestModuleCodeList.Codes.AES;
			outgoingMessageAsStatusRequest.EM_Status = EDIMessage.Status.Sent;
			outgoingMessageAsStatusRequest.EM_MessageNum = ReferencedMessageIdentifier;
		}
		Mock<AesInboundEDIMessage<IERRNCK>> messageMock;
		Mock<IERRNCK> errnckMock;
		AesInboundEDIMessage<IERRNCK> message;
		StatusRequest outgoingMessageAsStatusRequest;
		const string ReferencedMessageIdentifier = "DE441715100000000000000000000477553";
		const string ReferenceNumber = "22DE000000001234E0";
	}
}

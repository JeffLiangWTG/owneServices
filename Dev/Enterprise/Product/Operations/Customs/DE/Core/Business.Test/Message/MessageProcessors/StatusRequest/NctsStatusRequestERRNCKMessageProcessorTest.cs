using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(NctsStatusRequestERRNCKMessageProcessor))]
	sealed class NctsStatusRequestERRNCKMessageProcessorTest : MessageProcessorAbstractTest<NctsStatusRequestERRNCKMessageProcessor, AtlasInboundEDIMessage<IERRNCK>>
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
			errnckMock.Setup(m => m.ReferenceNumber).Returns((string)null);
			ProcessMessage(message);
			AssertEquals(ReferencedMessageIdentifier, message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "Ncts StatusRequest ERRNCK Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IERRNCK>> Processor => new NctsStatusRequestERRNCKMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			var error1Mock = new Mock<IERRNCKError>();
			error1Mock.Setup(x => x.Code).Returns("VEE00717");
			error1Mock.Setup(x => x.Pointer).Returns("/DETQQC/Consignee");
			error1Mock.Setup(x => x.Text).Returns("Der Beteiligte Empfänger ist nicht mit dem Beteiligten im Vorgang identisch.");

			var error2Mock = new Mock<IERRNCKError>();
			error2Mock.Setup(x => x.Code).Returns("VEE00717");
			error2Mock.Setup(x => x.Pointer).Returns("/DETQQC/Consignee");
			error2Mock.Setup(x => x.Text).Returns("Der Beteiligte Empfänger ist nicht im Vorgang vorhanden.");

			errnckMock = new Mock<IERRNCK>();
			errnckMock.Setup(x => x.MessageIdentifier).Returns(MessageIdentifier);
			errnckMock.Setup(x => x.ReferenceNumber).Returns(ReferenceNumber);
			errnckMock.Setup(x => x.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			errnckMock.Setup(x => x.Errors).Returns(new IERRNCKError[] { error1Mock.Object, error2Mock.Object });

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IERRNCK>>();
			messageMock.Setup(x => x.DataProvider).Returns(errnckMock.Object);
			message = messageMock.Object;

			outgoingMessageAsStatusRequest = Factory.New<StatusRequest>();
			Factory.Save();
			outgoingMessageAsStatusRequest.EM_Status = EDIMessage.Status.Sent;
			outgoingMessageAsStatusRequest.EM_MessageNum = ReferencedMessageIdentifier;
		}
		Mock<AtlasInboundEDIMessage<IERRNCK>> messageMock;
		Mock<IERRNCK> errnckMock;
		AtlasInboundEDIMessage<IERRNCK> message;
		StatusRequest outgoingMessageAsStatusRequest;
		const string ReferencedMessageIdentifier = "DE441715100000000000000000000477553";
		const string MessageIdentifier = "ERRNCK58750000000381119050419125839";
		const string ReferenceNumber = "22DE000000001234E0";
	}
}

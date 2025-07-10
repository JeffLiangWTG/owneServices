using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDESSTAMessageProcessor))]
	sealed class NctsDESSTAMessageProcessorTest : MessageProcessorAbstractTest<NctsDESSTAMessageProcessor, AtlasInboundEDIMessage<IDESSTA>>
	{
		public void TestLinkedObjectNotFound()
		{
			outgoingMessage.EM_MessageNum = "NotOriginalMessage";

			ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Error, message.EM_Status);
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			AssertEquals(nctsHeader, message.EM_LinkedObject);
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("BM_CustomsStatus", "DDR", nctsHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("EffectiveMessageStatus changed to 'ACC' if 'SNT' before", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
			});
		}

		public void TestProcessMessage_MessageStatusNotChanged()
		{
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Accepted;

			ProcessMessage(message);

			AssertEquals("EffectiveMessageStatus not changed if not 'SNT' before", LogicalStatusList.Codes.Accepted, nctsHeader.EffectiveMessageStatus);
		}

		public void TestMailIsSentToUser()
		{
			ProcessMessage(message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var title = "NCTS Arrival Status Update. Response for DESSTANCTSTEST123";
			var subject = $"{title} Ref.: {ReferenceNumber}";
			var bodyMessageTitle = $"<title>{title}</title>";
			var bodyMessageHeader = string.Empty;
			var bodyMessageSummary = $"Your NCTS Arrival Declaration for Job DESSTANCTSTEST123 has received a Status Update. For details please follow the Link to the Job.";

			var table = new HtmlTableCreator();
			table.WriteRow("MRN", MovementReferenceNumber);
			table.WriteRow("Status Update", "Vorgang angefordert");

			AssertEmailForSingleRecipientWithTable("Email Sent", email, "bob@thebuilder.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, table.ToHtml());
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals(MovementReferenceNumber, message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "NCTS DESSTA Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IDESSTA>> Processor => new NctsDESSTAMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_JobReference = MessageIdentifier;
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Sent;
			nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = ReferenceNumber;

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			dataProviderMock = new Mock<IDESSTA>();
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns(MessageIdentifier);
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns(MovementReferenceNumber);
			dataProviderMock.Setup(m => m.TransitOperationDestinationStatus).Returns(A0116StateOfCompletionCodeList.Codes._311);
			dataProviderMock.Setup(m => m.MessageType).Returns(nameof(DETSSBMessageType.DETSSB));

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IDESSTA>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			messageMock.Setup(m => m.AttachedDocuments).Returns(new List<AttachedDocument>());
			message = messageMock.Object;

			Factory.Save();

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_FullName = "BOB THE BUILDER";
			staff.GS_EmailAddress = "bob@thebuilder.com";

			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
		}
		Mock<IDESSTA> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IDESSTA>> messageMock;
		AtlasInboundEDIMessage<IDESSTA> message;
		NctsHeader nctsHeader;
		EDIMessage outgoingMessage;

		const string MessageIdentifier = "DESSTANCTSTEST123";
		const string MovementReferenceNumber = "22DE000000001234E0";
		const string ReferencedMessageIdentifier = "RMI123456";
		const string ReferenceNumber = "19DE485154386041M4";
	}
}

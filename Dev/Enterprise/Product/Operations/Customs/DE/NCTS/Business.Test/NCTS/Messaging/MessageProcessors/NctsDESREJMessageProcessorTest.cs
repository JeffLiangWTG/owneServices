using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDESREJMessageProcessor))]
	sealed class NctsDESREJMessageProcessorTest : MessageProcessorAbstractTest<NctsDESREJMessageProcessor, AtlasInboundEDIMessage<IDESREJ>>
	{
		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IDESREJ)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestLinkedObjectNotFound()
		{
			outgoingMessage.EM_MessageNum = "12345";
			ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Error, message.EM_Status);
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			AssertEquals(nctsHeader, message.EM_LinkedObject);
		}

		[TestDate(2023, 3, 1, 10, 30, 50)]
		public void TestProcessMessage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "KCH";
			staff.GS_FullName = "Karim";
			staff.GS_EmailAddress = "kch@wtg.com";

			Factory.Save();
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			ProcessMessage(message);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("ArrivalMovementHeader.BM_MessageStatus", LogicalStatusList.Codes.Error, nctsHeader.ArrivalMovementHeader.BM_MessageStatus);

				var mscErrorEvent = nctsHeader.ArrivalMovementHeader.Logs.MostRecentLogByEventTime(AutoEvents.MessageStatusChange, LogicalStatusList.Codes.Error);
				AssertNotNull(mscErrorEvent);
				AssertEquals("SL_EventTime", new ZDateTime(2023, 3, 1, 10, 30, 50), mscErrorEvent.SL_EventTime);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				var reference = nctsHeader.BH_JobReference;
				var title = $"NCTS Arrival Rejection Notification Response for {reference}";
				var subject = $"{title} Ref.: {ReferenceNumber}";
				var bodyMessageTitle = $"<title>{title}</title>";
				var bodyMessageHeader = ZString.Empty;
				var bodyMessageSummary = "Your NCTS Arrival Declaration for Job REFERENCE1 has received a Rejection Notification. For details please follow the Link to the Job.";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								+ $"<tr><td>MRN</td><td>{MovementReferenceNumber}</td></tr>"
								+ "<tr><td>Rejection Type</td><td>007 - Arrival Notification rejected</td></tr>"
								+ "</table>";
				AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "kch@wtg.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		[TestDate(2023, 3, 1, 10, 30, 50)]
		public void TestEMailWithRejectionType044()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "KCH";
			staff.GS_FullName = "Karim";
			staff.GS_EmailAddress = "kch@wtg.com";

			Factory.Save();

			dataProviderMock.Setup(m => m.RejectionType).Returns("044");

			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;
			Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

			ProcessMessage(message);
			Factory.Save();
			CombineAssertions(() =>
			{
				AssertEquals("incomingMessage.EM_Status", EDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("ArrivalMovementHeader.BM_MessageStatus", LogicalStatusList.Codes.Error, nctsHeader.ArrivalMovementHeader.BM_MessageStatus);

				var mscErrorEvent = nctsHeader.ArrivalMovementHeader.Logs.MostRecentLogByEventTime(AutoEvents.MessageStatusChange, LogicalStatusList.Codes.Error);
				AssertNotNull(mscErrorEvent);
				AssertEquals("SL_EventTime", new ZDateTime(2023, 3, 1, 10, 30, 50), mscErrorEvent.SL_EventTime);

				var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
				var reference = nctsHeader.BH_JobReference;
				var title = $"NCTS Arrival Rejection Notification Response for {reference}";
				var subject = $"{title} Ref.: {ReferenceNumber}";
				var bodyMessageTitle = $"<title>{title}</title>";

				var bodyMessageHeader = ZString.Empty;
				var bodyMessageSummary = "Your NCTS Arrival Declaration for Job REFERENCE1 has received a Rejection Notification. For details please follow the Link to the Job.";
				var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								+ $"<tr><td>MRN</td><td>{MovementReferenceNumber}</td></tr>"
								+ "<tr><td>Rejection Type</td><td>044 - Unloading Remarks rejected</td></tr>"
								+ "</table>";
				AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "kch@wtg.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
			});
		}

		public void TestPopulateLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			AssertEquals(MovementReferenceNumber, message.GetLogbookRegistrationNumber());
		}

		protected override ZString MessageFriendlyName => "NCTS DESREJ Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IDESREJ>> Processor => new NctsDESREJMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = "REFERENCE1";
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.ArrivalMovementHeader.BM_PaperlessInbondNum = ReferenceNumber;
			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(nctsHeader, ReferencedMessageIdentifier);

			dataProviderMock = new Mock<IDESREJ>();
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("0624123347");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns(MovementReferenceNumber);
			dataProviderMock.Setup(m => m.RejectionType).Returns("007");

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IDESREJ>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}

		Mock<IDESREJ> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IDESREJ>> messageMock;
		AtlasInboundEDIMessage<IDESREJ> message;
		NctsHeader nctsHeader;
		EDIMessage outgoingMessage;

		const string ReferencedMessageIdentifier = "DE302989100000000000000000000487287";
		const string MovementReferenceNumber = "22DE000000001234E0";
		const string ReferenceNumber = "19DE485154386041M4";
	}
}

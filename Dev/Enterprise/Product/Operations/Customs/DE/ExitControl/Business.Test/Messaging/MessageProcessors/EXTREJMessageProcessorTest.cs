using System.Linq;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	[TestedType(typeof(EXTREJMessageProcessor))]
	sealed class EXTREJMessageProcessorTest : MessageProcessorAbstractTest<EXTREJMessageProcessor, AesInboundEDIMessage<IEXTREJ>>
	{
		[TestDate(2023, 08, 17, 15, 18, 00)]
		public void TestNewEventAlwaysCreatdWhenConsignmentLogAdded()
		{
			ProcessMessage(message);
			var errEvent = exitReport.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, e => e.SL_Reference == ConsignmentStatusCode);
			AssertEquals("NewEventRaised", new ZDateTime(2023, 08, 17, 15, 18, 00), errEvent.SL_EventTime);
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);

			AssertEquals(exitReport, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IEXTREJ)null);

			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestMessageStatus()
		{
			ProcessMessage(message);

			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestExitStatuses()
		{
			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("CER_Status", ConsignmentStatusCode, exitReport.CER_Status);
				AssertEquals("CXC_Status", ConsignmentStatusCode, consignment.CXC_Status);
			});
		}

		public void TestStatusEvent()
		{
			ProcessMessage(message);

			var mostRecentLog = consignment.Logs.MostRecentLog;

			CombineAssertions(() =>
			{
				AssertEquals("SL_SE_NKEvent", "CES", mostRecentLog.SL_SE_NKEvent);
				AssertEquals("SL_Reference", ConsignmentStatusCode, mostRecentLog.SL_Reference);
			});
		}

		public void TestEmailMessage()
		{
			const string consignmentStatusText = "507 status description here";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "Customs Status Export AES-EXP");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, ConsignmentStatusCode,
				consignmentStatusText, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("mrn123456789");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("lrn123456789");

			ProcessMessage(message);
			var email = EnvProxy.Instance.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"AES EXT Rejection Message Response for {exitReport.Header.CXH_JobReference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = string.Empty;
			var bodyMessageSummary = $"Your Exit Control Message for {exitReport.Header.CXH_JobReference} has a Rejection Message. For details, please follow the Link to the Job.";

			var table = new HtmlTableCreator();
			table.WriteRow("MRN", "mrn123456789");
			table.WriteRow("LRN", "lrn123456789");
			table.WriteRow("Status", ConsignmentStatusCode);
			table.WriteRow("Status Text", consignmentStatusText);

			AssertEmailForSingleRecipientWithTable("Email sent", email, "bob@thebuilder.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, table.ToHtml());
		}

		public void TestLogbookRegistrationNumber()
		{
			const string mrn = "19DE000000001234E0";
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns(mrn);
			ProcessMessage(message);
			var actualLogbookRegistrationNumber =
				message.GetNote(LogbookHelper.LogbookRegistrationNumberNoteDescription);
			AssertEquals(mrn, actualLogbookRegistrationNumber);
		}

		protected override ZString MessageFriendlyName => "Export EXTREJ Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IEXTREJ>> Processor => new EXTREJMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.NewWithValidTestData<CusExitHeader>();

			consignment = header.CusExitConsignments.AddNew();

			exitReport = header.CusExitReports.AddNew();
			exitReport.CER_CXC_Consignment = consignment.PK;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_FullName = "BOB THE BUILDER";
			staff.GS_EmailAddress = "bob@thebuilder.com";

			const string outgoingMessageIdentifier = "EXPREJTest";
			outgoingMessage = CreateOriginalMessageLinkedToParent<AesEDIMessage>(exitReport, outgoingMessageIdentifier);
			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			dataProviderMock = new Mock<IEXTREJ>();
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(outgoingMessageIdentifier);
			dataProviderMock.Setup(m => m.ConsignmentStatus).Returns(ConsignmentStatusCode);

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IEXTREJ>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}

		CusExitReport exitReport;
		CusExitConsignment consignment;
		Mock<IEXTREJ> dataProviderMock;
		Mock<AesInboundEDIMessage<IEXTREJ>> messageMock;
		EDIMessage outgoingMessage;
		AesInboundEDIMessage<IEXTREJ> message;

		const string ConsignmentStatusCode = "507";
	}
}

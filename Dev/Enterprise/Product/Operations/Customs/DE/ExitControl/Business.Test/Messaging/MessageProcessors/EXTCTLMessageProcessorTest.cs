using System.Linq;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	[TestedType(typeof(EXTCTLMessageProcessor))]
	sealed class EXTCTLMessageProcessorTest : MessageProcessorAbstractTest<EXTCTLMessageProcessor, AesInboundEDIMessage<IEXTCTL>>
	{
		protected override ZString MessageFriendlyName => "Export EXTCTL Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IEXTCTL>> Processor => new EXTCTLMessageProcessor(logger);

		[TestDate(2023, 08, 17, 15, 18, 00)]
		public void TestNewEventAlwaysCreatdWhenCER_StatusSet()
		{
			ProcessMessage(message);
			var errEvent = report.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, e => e.SL_Reference == A0116ATLASStatusCodeList.Codes._351);
			AssertEquals("NewEventRaised", new ZDateTime(2023, 08, 17, 15, 18, 00), errEvent.SL_EventTime);
		}

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals(report, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IEXTCTL)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals("351", report.CER_Status);
				AssertEquals("351", consignment.CXC_Status);
			});
		}

		public void TestEventCreated()
		{
			ProcessMessage(message);

			var mostRecentLog = consignment.GetLogs().MostRecentLog;

			CombineAssertions(() =>
			{
				AssertNotNull(mostRecentLog);
				AssertEquals("CES", mostRecentLog.SL_SE_NKEvent);
				AssertEquals("351", mostRecentLog.SL_Reference);
				AssertEquals(nameof(CusExitConsignment), mostRecentLog.SL_Table);
				AssertEquals(consignment.PK, mostRecentLog.SL_Parent);
			});
		}

		public void TestEmailIsSentToUser()
		{
			ProcessMessage(message);
			var email = EnvProxy.Instance.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"AES EXT Control Measure Message Response for {report.Header.CXH_JobReference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = string.Empty;
			var bodyMessageSummary = $"Your Exit Control Message for {report.Header.CXH_JobReference} has a control measure request. For details, please follow the Link to the Job.<br/><br/>MRN: {mrn}<br/><br/>";

			var table = new HtmlTableCreator(new[]
			{
				"Control Type", "Annotation"
			});
			table.WriteRow("10 - Documentary controls", "TEST1");
			table.WriteRow("20 - Nuclear/radioactive material check", "TEST2");

			AssertEmailForSingleRecipientWithTable("Email sent", email, "bob@thebuilder.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, table.ToHtml());
		}

		public void TestLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			var actualLogbookRegistrationNumber =
				message.GetNote(LogbookHelper.LogbookRegistrationNumberNoteDescription);
			AssertEquals(mrn, actualLogbookRegistrationNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.NewWithValidTestData<CusExitHeader>();
			consignment = header.CusExitConsignments.AddNew();
			report = header.CusExitReports.AddNew();

			report.CER_CXC_Consignment = consignment.PK;
			report.CER_OfficeOfExit = "DE001";

			consignment.CXC_MovementReference = mrn;

			outgoingMessage = CreateOriginalMessageLinkedToParent<AesInboundEDIMessage<IEXTCTL>>(report, "EXTCTLTest");

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_FullName = "BOB THE BUILDER";
			staff.GS_EmailAddress = "bob@thebuilder.com";

			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			dataProviderMock = new Mock<IEXTCTL>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("1414678");
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns("EXTCTLTest");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns(mrn);

			var typeOfControlsMock1 = new Mock<IEXTCTLTypeOfControls>();
			typeOfControlsMock1.Setup(m => m.Type).Returns("10");
			typeOfControlsMock1.Setup(m => m.Text).Returns("TEST1");

			var typeOfControlsMock2 = new Mock<IEXTCTLTypeOfControls>();
			typeOfControlsMock2.Setup(m => m.Type).Returns("20");
			typeOfControlsMock2.Setup(m => m.Text).Returns("TEST2");

			dataProviderMock.Setup(m => m.TypeOfControls).Returns(new[]
			{
				typeOfControlsMock1.Object,
				typeOfControlsMock2.Object
			});

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IEXTCTL>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}

		Mock<AesInboundEDIMessage<IEXTCTL>> messageMock;
		AesInboundEDIMessage<IEXTCTL> message;
		Mock<IEXTCTL> dataProviderMock;

		CusExitReport report;
		CusExitConsignment consignment;
		EDIMessage outgoingMessage;

		readonly string mrn = "23DE12345678901238";
	}
}

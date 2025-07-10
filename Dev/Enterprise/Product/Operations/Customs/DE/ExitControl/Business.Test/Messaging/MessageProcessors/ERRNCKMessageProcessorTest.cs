using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
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
	[TestedType(typeof(ERRNCKMessageProcessor))]
	sealed class ERRNCKMessageProcessorTest : MessageProcessorAbstractTest<ERRNCKMessageProcessor, AesInboundEDIMessage<IERRNCK>>
	{
		protected override ZString MessageFriendlyName => "Export ERRNCK Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IERRNCK>> Processor => new ERRNCKMessageProcessor(logger);

		[TestDate(2023, 08, 17, 15, 18, 00)]
		public void TestNewErrEventAlwaysCreatd()
		{
			ProcessMessage(message);
			var errEvent = report.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, e => e.SL_Reference == UniversalReferenceConstants.EntryStatus.ERR);
			AssertEquals("NewEventRaised", new ZDateTime(2023, 08, 17, 15, 18, 00), errEvent.SL_EventTime);
		}

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals(report, message.EM_LinkedObject);
			AssertEquals(LogicalStatusList.Codes.Error, report.CER_MessageStatus);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IERRNCK)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);
			var reference = header.CXH_JobReference;
			var email = EnvProxy.Instance.OutgoingCustomsMailManager.EmailsCreated.Single();
			var subject = $"Exit Declaration Message Status Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = string.Empty;
			var bodyMessageSummary = $"Your Exit Declaration Message for Job {reference} has been rejected. For details please follow the Link to the Job.";

			var table = new HtmlTableCreator(new[]
			{
				"Error Code", "Pointer", "Text", "Original Value"
			});
			table.WriteRow("COM0000", "CFCDEC/KOPF", "duplicate detected", "00000000000006");
			table.WriteRow("COM0001", "CFCDEC/KOPF", "duplicate detected nb", "00000000000007");

			AssertEmailForSingleRecipientWithTable("Email sent", email, "bob@thebuilder.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, table.ToHtml());
		}

		public void TestLogbookRegistrationNumber()
		{
			const string mrn = "19DE000000001234E0";
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(mrn);
			ProcessMessage(message);
			var actualLogbookRegistrationNumber =
				message.GetNote(LogbookHelper.LogbookRegistrationNumberNoteDescription);
			AssertEquals(mrn, actualLogbookRegistrationNumber);
		}

		public void TestLogbookRegistrationNumber_ShouldUseReferencedMessageIdentifier_WhenReferenceNumberIsNullOrEmpty()
		{
			var testCases = new[] { string.Empty, null };
			foreach (var testCase in testCases)
			{
				dataProviderMock.Setup(m => m.ReferenceNumber).Returns(testCase);

				ProcessMessage(message);

				AssertEquals($"{nameof(IERRNCK.ReferenceNumber)} = \"{testCase}\"", "ERRNCKTest", message.GetLogbookRegistrationNumber());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<CusExitHeader>();
			consignment = header.CusExitConsignments.AddNew();
			report = header.CusExitReports.AddNew();

			report.CER_CXC_Consignment = consignment.PK;
			report.CER_OfficeOfExit = "DE001";

			outgoingMessage = CreateOriginalMessageLinkedToParent<AesInboundEDIMessage<IERRNCK>>(report, "ERRNCKTest");

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_FullName = "BOB THE BUILDER";
			staff.GS_EmailAddress = "bob@thebuilder.com";

			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			dataProviderMock = new Mock<IERRNCK>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("1414678");
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns("ERRNCKTest");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(lrn);
			dataProviderMock.Setup(m => m.MessageGroup).Returns("EXT");

			var error1 = new Mock<IERRNCKError>();
			error1.Setup(m => m.Code).Returns("COM0000");
			error1.Setup(x => x.Pointer).Returns("CFCDEC/KOPF");
			error1.Setup(m => m.Text).Returns("duplicate detected");
			error1.Setup(m => m.OriginalValue).Returns("00000000000006");

			var error2 = new Mock<IERRNCKError>();
			error2.Setup(m => m.Code).Returns("COM0001");
			error2.Setup(x => x.Pointer).Returns("CFCDEC/KOPF");
			error2.Setup(m => m.Text).Returns("duplicate detected nb");
			error2.Setup(m => m.OriginalValue).Returns("00000000000007");

			dataProviderMock.Setup(m => m.Errors).Returns(new[]
			{
				error1.Object,
				error2.Object
			});

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IERRNCK>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}

		Mock<AesInboundEDIMessage<IERRNCK>> messageMock;
		AesInboundEDIMessage<IERRNCK> message;
		Mock<IERRNCK> dataProviderMock;

		CusExitHeader header;
		CusExitReport report;
		CusExitConsignment consignment;
		EDIMessage outgoingMessage;

		readonly string lrn = "JOJO123";
	}
}

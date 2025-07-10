using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;
using EDIMessageTypeList = Enterprise.Customs.DE.Messaging.EDIMessageTypeList;

namespace Enterprise.Customs.DE.Business.MonthlyClosing.Testing
{
	[TestedType(typeof(MonthlyClosingERRNCKMessageProcessor))]
	sealed class MonthlyClosingERRNCKMessageProcessorTest : MessageProcessorAbstractTest<MonthlyClosingERRNCKMessageProcessor, AtlasInboundEDIMessage<IERRNCK>>
	{
		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals("LinkedObject from OriginalMessage", declaration, message.EM_LinkedObject);
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns("IMP002");
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
				AssertEquals("EM_Status", AesEDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("CRD_MessageStatus", EDIMessageStatusList.Codes.Rejected, declaration.CRD_MessageStatus);
				AssertEquals("CRD_CustomsStatus", UniversalReferenceConstants.EntryStatus.ERR, declaration.CRD_CustomsStatus);
			});
		}

		public void TestProcessMessage_CustomsStatusNotEmpty()
		{
			declaration.CRD_CustomsStatus = UniversalReferenceConstants.EntryStatus.RC2;
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", AesEDIMessage.Status.ProcessedOK, message.EM_Status);
				AssertEquals("CRD_MessageStatus", EDIMessageStatusList.Codes.Rejected, declaration.CRD_MessageStatus);
				AssertEquals("CRD_CustomsStatus", UniversalReferenceConstants.EntryStatus.RC2, declaration.CRD_CustomsStatus);
			});
		}

		public void TestEmailNotification()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@wtg.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(message);
			var reference = declaration.CRD_JobReferenceNumber;
			var subject = $"Monthly Closing Declaration Status Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Monthly Closing Declaration Status Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=DEMonthlyClosing&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Monthly Closing Declaration for Job {reference} has been rejected. For details please follow the Link to the Job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><thead><tr class=""tableheadings"">"
									+ "<th>Error Code</th>"
									+ "<th>Pointer</th>"
									+ "<th>Text</th>"
									+ "<th>Original Value</th>"
									+ "</tr></thead>"
									+ "<tr><td>VEE00701</td>"
									+ "<td>CFCDEC/KOPF</td>"
									+ "<td>Die Beendigung zu diesem Vorgang wurde bereits von der Dienststelle abgeschlossen.</td>"
									+ "<td>00000000000006</td>"
									+ "</tr></table>";
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			CombineAssertions(() => AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@wtg.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable));
		}

		public void TestPopulateLogbookRegNum()
		{
			const string referenceNumber = "abc";
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(referenceNumber);

			ProcessMessage(message);

			AssertEquals(referenceNumber, message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegNum_ShouldUseReferencedMessageIdentifier_WhenReferenceNumberIsNullOrEmpty()
		{
			var testCases = new[] { string.Empty, null };
			foreach (var testCase in testCases)
			{
				dataProviderMock.Setup(m => m.ReferenceNumber).Returns(testCase);

				ProcessMessage(message);

				AssertEquals($"{nameof(IERRNCK.ReferenceNumber)} = \"{testCase}\"", "IMP001", message.GetLogbookRegistrationNumber());
			}
		}

		protected override ZString MessageFriendlyName => "Monthly Closing ERRNCK Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IERRNCK>> Processor => new MonthlyClosingERRNCKMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.NewWithValidTestData<CusReconDeclaration>();

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(declaration, "IMP001");
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = MonthlyClosingMessageSubTypeList.Codes.MonthlyClosingFreeCirculation;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			var errorMock = new Mock<IERRNCKError>();
			errorMock.Setup(x => x.Code).Returns("VEE00701");
			errorMock.Setup(x => x.Pointer).Returns("CFCDEC/KOPF");
			errorMock.Setup(x => x.Text).Returns("Die Beendigung zu diesem Vorgang wurde bereits von der Dienststelle abgeschlossen.");
			errorMock.Setup(x => x.OriginalValue).Returns("00000000000006");

			dataProviderMock = new Mock<IERRNCK>();
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns("IMP001");
			dataProviderMock.Setup(x => x.Errors).Returns(new IERRNCKError[] { errorMock.Object });

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<IERRNCK>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;

			Factory.Save();
		}
		CusReconDeclaration declaration;
		Mock<IERRNCK> dataProviderMock;
		Mock<AtlasInboundEDIMessage<IERRNCK>> messageMock;
		AtlasInboundEDIMessage<IERRNCK> message;
		EDIMessage outgoingMessage;
	}
}

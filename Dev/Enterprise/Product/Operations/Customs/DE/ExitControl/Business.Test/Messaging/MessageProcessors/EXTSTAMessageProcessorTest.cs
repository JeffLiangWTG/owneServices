using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.ExitControl.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business.Customs;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	[TestedType(typeof(EXTSTAMessageProcessor))]
	sealed class EXTSTAMessageProcessorTest : MessageProcessorAbstractTest<EXTSTAMessageProcessor, AesInboundEDIMessage<IEXTSTA>>
	{
		[TestDate(2023, 08, 17, 15, 18, 00)]
		public void TestNewEventAlwaysCreatdWhenCER_StatusSet()
		{
			ProcessMessage(message);
			var errEvent = report.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, e => e.SL_Reference == A0116ATLASStatusCodeList.Codes._300);
			AssertEquals("NewEventRaised", new ZDateTime(2023, 08, 17, 15, 18, 00), errEvent.SL_EventTime);
		}

		public void TestGetLinkedObject()
		{
			ProcessMessage(message);
			AssertEquals(report, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((IEXTSTA)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(message);

			CombineAssertions(() =>
			{
				AssertEquals(A0116ATLASStatusCodeList.Codes._300, report.CER_Status);
				AssertEquals(A0116ATLASStatusCodeList.Codes._300, consignment.CXC_Status);
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
				AssertEquals(A0116ATLASStatusCodeList.Codes._300, mostRecentLog.SL_Reference);
				AssertEquals(nameof(CusExitConsignment), mostRecentLog.SL_Table);
				AssertEquals(consignment.PK, mostRecentLog.SL_Parent);
			});
		}

		public void TestEmailIsSentToUser()
		{
			ProcessMessage(message);

			AssertEmailSent("bob@thebuilder.com");
		}

		public void TestEmailIsSentToPREUser()
		{
			var preMessage = (EDIMessage)outgoingMessage.Clone();
			preMessage.EM_SystemCreateUser = staff.GS_Code;
			preMessage.EM_SystemCreateTimeUtc = DateTime.UtcNow.AddHours(-1);
			outgoingMessage.EM_SystemCreateUser = User.ServiceUserCode;
			ProcessMessage(message);

			AssertEmailSent("bob@thebuilder.com");
		}

		public void TestEmailAttachments()
		{
			var attachedDocuments = GetAttachedDocuments();

			messageMock.Setup(m => m.AttachedDocuments).Returns(attachedDocuments);

			ProcessMessage(message);

			var subject = $"AES EXT Status Message Response for {report.Header.CXH_JobReference}";
			var email = EnvProxy.Instance.OutgoingCustomsMailManager.EmailsCreated.Single(x => x.Subject == subject);
			var emailAttachments = email.Attachments.Cast<AttachmentDef>();
			var file1 = emailAttachments.Single(x => x.DisplayName == "file1.pdf");
			var file2 = emailAttachments.Single(x => x.DisplayName == "file2.pdf");

			CombineAssertions(() =>
			{
				AssertEquals("File1 content", Convert.FromBase64String("XXX="), file1.Data);
				AssertEquals("File2 content", Convert.FromBase64String("YYY="), file2.Data);
			});
		}

		List<AttachedDocument> GetAttachedDocuments()
		{
			return new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "file1.pdf",
					Type = new DocumentType { Code = "AAA", Description = "AAA Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX=")),
				},
				new AttachedDocument
				{
					FileName = "file2.pdf",
					Type = new DocumentType { Code = "BBB", Description = "BBB Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("YYY=")),
				}
			};
		}

		public void TestLogbookRegistrationNumber()
		{
			ProcessMessage(message);
			var actualLogbookRegistrationNumber =
				message.GetNote(LogbookHelper.LogbookRegistrationNumberNoteDescription);
			AssertEquals(mrn, actualLogbookRegistrationNumber);
		}

		public void TestGetEmailGroupRegistryItem()
		{
			var (messageProcessor, _) = GetEXTSTAMessageProcessorForTest();
			AssertType<GroupNotificationRegistryItem<ExitControlGroupNotification>>(messageProcessor.GetEmailGroupRegistryItemExposed());
		}

		public void TestGetEmailGroupPK()
		{
			var (messageProcessor, sendToRegistry) = GetEXTSTAMessageProcessorForTest();
			using (ExitControlCustomsDataRegistry.Instance.SendExitControlAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				AssertEquals(sendToRegistry.SendGroupPK, messageProcessor.GetEmailGroupPKExposed(ExitControlCustomsDataRegistry.Instance.SendExitControlAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailGroupPK_FallBack()
		{
			var (messageProcessor, sendToRegistry) = GetEXTSTAMessageProcessorForTest();
			using (ExitControlCustomsDataRegistry.Instance.SendExitControlAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistry))
			{
				AssertEquals(sendToRegistry.SendGroupPK, messageProcessor.GetEmailGroupPKExposed(ExitControlCustomsDataRegistry.Instance.SendExitControlAcknowledgements, GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailSendMode()
		{
			var (messageProcessor, sendToRegistry) = GetEXTSTAMessageProcessorForTest();
			using (ExitControlCustomsDataRegistry.Instance.SendExitControlAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, sendToRegistry))
			{
				AssertEquals(Core.Constants.EmailTo.NominatedGroup, messageProcessor.GetEmailSendModeExposed(GlbBranch.CurrentBranch));
			}
		}

		public void TestGetEmailSendMode_Fallback()
		{
			var (messageProcessor, sendToRegistry) = GetEXTSTAMessageProcessorForTest();
			using (ExitControlCustomsDataRegistry.Instance.SendExitControlAcknowledgements.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, sendToRegistry))
			{
				AssertEquals(Core.Constants.EmailTo.NominatedGroup, messageProcessor.GetEmailSendModeExposed(GlbBranch.CurrentBranch));
			}
		}

		protected override ZString MessageFriendlyName => "Export EXTSTA Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IEXTSTA>> Processor => new EXTSTAMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.NewWithValidTestData<CusExitHeader>();
			consignment = header.CusExitConsignments.AddNew();
			report = header.CusExitReports.AddNew();

			report.CER_CXC_Consignment = consignment.PK;
			report.CER_OfficeOfExit = "DE001";

			consignment.CXC_MovementReference = mrn;

			outgoingMessage = CreateOriginalMessageLinkedToParent<AesInboundEDIMessage<IEXTSTA>>(report, "EXTSTAOutgoing");

			staff = Factory.New<GlbStaff>();
			staff.GS_Code = "BOB";
			staff.GS_FullName = "BOB THE BUILDER";
			staff.GS_EmailAddress = "bob@thebuilder.com";

			outgoingMessage.EM_SystemCreateUser = staff.GS_Code;

			dataProviderMock = new Mock<IEXTSTA>();
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns("1414678");
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns("EXTSTAOutgoing");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns(mrn);
			dataProviderMock.Setup(m => m.ExitStatus).Returns(A0116ATLASStatusCodeList.Codes._300);

			messageMock = Factory.NewMoq<AesInboundEDIMessage<IEXTSTA>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			message = messageMock.Object;
			Factory.Save();
		}

		Mock<AesInboundEDIMessage<IEXTSTA>> messageMock;
		AesInboundEDIMessage<IEXTSTA> message;
		Mock<IEXTSTA> dataProviderMock;

		CusExitReport report;
		CusExitConsignment consignment;
		EDIMessage outgoingMessage;
		GlbStaff staff;

		readonly string mrn = "23DE12345678901238";

		(EXTSTAMessageProcessorForTest, ExitControlGroupNotification) GetEXTSTAMessageProcessorForTest()
		{
			var emailGroupPK = Factory.NewWithValidTestData<GlbGroup>().PK;
			Factory.Save();

			var messageProcessor = new EXTSTAMessageProcessorForTest(new LoggingInformation());
			var sendToRegistry = new ExitControlGroupNotification(Core.Constants.EmailTo.NominatedGroup, emailGroupPK);
			return (messageProcessor, sendToRegistry);
		}

		void AssertEmailSent(string expectedRecipient)
		{
			var email = EnvProxy.Instance.OutgoingCustomsMailManager.EmailsCreated.Single();
			var subject = $"AES EXT Status Message Response for {report.Header.CXH_JobReference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = string.Empty;
			var bodyMessageSummary = $"Your Exit Control Message for {report.Header.CXH_JobReference} has a Status Message. For details, please follow the Link to the Job.";

			var emailTable = new HtmlTableCreator();

			emailTable.WriteRow("MRN:", mrn);
			emailTable.WriteRow("Status:", A0116ATLASStatusCodeList.Codes._300);
			emailTable.WriteRow("Status Text:", "Declaration available");

			CombineAssertions(() =>
			{
				AssertEmailForSingleRecipientWithTable("Email sent", email, expectedRecipient, subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, emailTable.ToHtml());
			});
		}
	}

	sealed class EXTSTAMessageProcessorForTest : EXTSTAMessageProcessor
	{
		internal EXTSTAMessageProcessorForTest(LoggingInformation logger)
			: base(logger)
		{
		}

		public IRegistryItem GetEmailGroupRegistryItemExposed() => GetEmailGroupRegistryItem();

		public ZGuid GetEmailGroupPKExposed(IRegistryItem registryItem, IGlbBranch branch) => GetEmailGroupPK(registryItem, branch);

		public ZString GetEmailSendModeExposed(IGlbBranch branch) => GetEmailSendMode(branch);
	}
}

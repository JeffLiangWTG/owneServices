using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageCUSRECMessageProcessor))]
	sealed class TemporaryStorageCUSRECMessageProcessorTest : MessageProcessorAbstractTest<TemporaryStorageCUSRECMessageProcessor, AtlasInboundEDIMessage<ICUSREC>>
	{
		public void TestLinkedObjectNotFound()
		{
			originalMessage.EM_MessageNum = "NOTORIGINALMSG";

			ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Error, message.EM_Status);
		}

		public void TestGetLinkedObjectFromOriginalMessage()
		{
			ProcessMessage(message);
			AssertEquals(cusTempStorageDec, message.EM_LinkedObject);
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ICUSREC)null);
			AssertNoExceptionThrown(() => ProcessMessage(message));
		}

		public void TestUpdateEntryNumberForReExportResponses()
		{
			message.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.ReExport;
			ProcessMessage(message);
			AssertEquals("AT number updated from message.", "AT/B/15/000293/02/2019/5875", cusTempStorageDec.ReferenceNumber);
		}

		public void TestUpdateEntryNumberForPreliminarySummaryDeclarationResponses()
		{
			message.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration;
			ProcessMessage(message);
			AssertEquals("AT number updated from message.", "AT/B/15/000293/02/2019/5875", cusTempStorageDec.ReferenceNumber);
		}

		public void TestUpdateEntryNumberForSummaryDeclarationAfterPresentationResponses()
		{
			message.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.SummaryDeclarationAfterPresentation;
			ProcessMessage(message);
			AssertEquals("AT number updated from message.", "AT/B/15/000293/02/2019/5875", cusTempStorageDec.ReferenceNumber);
		}

		public void TestUpdateEntryNumberForReExportResponsesMRN()
		{
			message.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.ReExport;
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns((string)null);
			dataProviderMock.Setup(x => x.MRN).Returns(MRN);
			ProcessMessage(message);
			AssertEquals("MRN updated from message.", MRN, cusTempStorageDec.ReferenceNumber);
		}

		public void TestUpdateEntryNumberForPreliminarySummaryDeclarationResponsesMRN()
		{
			message.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration;
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns((string)null);
			dataProviderMock.Setup(x => x.MRN).Returns(MRN);
			ProcessMessage(message);
			AssertEquals("MRN updated from message.", MRN, cusTempStorageDec.ReferenceNumber);
		}

		public void TestUpdateEntryNumberForSummaryDeclarationAfterPresentationResponsesMRN()
		{
			message.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.SummaryDeclarationAfterPresentation;
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns((string)null);
			dataProviderMock.Setup(x => x.MRN).Returns(MRN);
			ProcessMessage(message);
			AssertEquals("MRN updated from message.", MRN, cusTempStorageDec.ReferenceNumber);
		}

		public void TestEntryNumberNotUpdated()
		{
			cusTempStorageDec.ReferenceNumber = "ATB150002930220195877"; // Existing AT number
			message.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.SummaryDeclarationAfterPresentation;
			ProcessMessage(message);
			AssertEquals("AT number is NOT updated from message.", "AT/B/15/000293/02/2019/5877", cusTempStorageDec.ReferenceNumber);
		}

		public void TestEntryNumberForNonReExportResponses()
		{
			var testMessageSubTypeList = new TemporaryStorageMessageSubTypeList();
			testMessageSubTypeList.RemoveCode(TemporaryStorageMessageSubTypeList.Codes.ReExport);
			testMessageSubTypeList.RemoveCode(TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration);
			testMessageSubTypeList.RemoveCode(TemporaryStorageMessageSubTypeList.Codes.SummaryDeclarationAfterPresentation);

			CombineAssertions(() =>
			{
				foreach (var messageSubType in testMessageSubTypeList.GetAllCodesZString())
				{
					cusTempStorageDec.ReferenceNumber = ZString.Empty;
					message.EM_MessageSubType = messageSubType;
					ProcessMessage(message);
					AssertEquals($"{messageSubType} AT Number remain empty.", ZString.Empty, cusTempStorageDec.ReferenceNumber);
				}
			});
		}

		public void TestSTH_MessageStatusToAcknowledged()
		{
			ProcessMessage(message);
			AssertEquals("MessageStatus update to ACK.", EDIMessageStatusList.Codes.Acknowledged, cusTempStorageDec.STH_MessageStatus);
		}

		public void TestSTH_MessageStatusUnchanged()
		{
			cusTempStorageDec.STH_MessageStatus = EDIMessageStatusList.Codes.ProcessedOK;
			ProcessMessage(message);
			AssertEquals("MessageStatus is NOT updated.", EDIMessageStatusList.Codes.ProcessedOK, cusTempStorageDec.STH_MessageStatus);
		}

		public void TestSTH_MessageStatusHeaderHasError()
		{
			dataProviderMock.Setup(m => m.NotificationSeverity).Returns(new ZString[] { NotificationTypeList.Codes.Error });
			ProcessMessage(message);
			AssertEquals(EDIMessageStatusList.Codes.Rejected, cusTempStorageDec.STH_MessageStatus);
		}

		public void TestSTH_MessageStatusHeaderHasWarning()
		{
			dataProviderMock.Setup(m => m.NotificationSeverity).Returns(new ZString[] { NotificationTypeList.Codes.Warning });
			ProcessMessage(message);
			AssertEquals(EDIMessageStatusList.Codes.Acknowledged, cusTempStorageDec.STH_MessageStatus);
		}

		public void TestSTH_MessageStatusHeaderHasInformationAndLineHasError()
		{
			goodsItemDataProviderMock.Setup(m => m.NotificationSeverity).Returns(NotificationTypeList.Codes.Error);
			var line = AddLine(1, true, ZDateTime.Today.AddDays(-1));
			ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Header Status", EDIMessageStatusList.Codes.Acknowledged, cusTempStorageDec.STH_MessageStatus);
				AssertEquals("Modified not reset", true, line.TSL_IsModified);
			});
		}

		public void TestLineHasNotBeenEditedSinceSend()
		{
			var line = AddLine(1, true, ZDateTime.Today.AddDays(-1));
			ProcessMessage(message);
			AssertEquals(false, line.TSL_IsModified);
		}

		public void TestLineHasNotBeenEditedAfterSend()
		{
			var line = AddLine(1, true, ZDateTime.Today.AddDays(1));
			ProcessMessage(message);
			AssertEquals(true, line.TSL_IsModified);
		}

		public void TestMissingLine()
		{
			var line = AddLine(2, true, ZDateTime.Today.AddDays(-1));
			ProcessMessage(message);
			var stmNote = GetStmNote(message);
			CombineAssertions(() =>
			{
				AssertEquals("TSL_IsModified NOT set to false.", true, line.TSL_IsModified);
				AssertEquals("Register Record Note",
					"This is the list of Sequence Numbers system could not locate matching CusTempStorageLine records for:\r\nSequence Number: 1",
					stmNote.ST_NoteText);
			});
		}

		[TestDate(2020, 5, 15, 12, 58, 32)]
		public void TestIssueDateIsFilledWithCurrentUTCWhenNotPreviouslySet()
		{
			message.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration;
			cusTempStorageDec.CusEntryNumber.CE_IssueDate = ZDateTime.Empty;
			ProcessMessage(message);
			AssertEquals(ZDateTime.UtcNow, cusTempStorageDec.CusEntryNumber.CE_IssueDate);
		}

		[TestDate(2020, 5, 18, 15, 41, 29)]
		public void TestIssueDateIsFilledWithCurrentUTCWhenATBNumberAlreadyPopulated()
		{
			message.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration;
			cusTempStorageDec.ReferenceNumber = "ATB150002930220195875";
			cusTempStorageDec.CusEntryNumber.CE_IssueDate = ZDateTime.Empty;
			ProcessMessage(message);
			AssertEquals(ZDateTime.UtcNow, cusTempStorageDec.CusEntryNumber.CE_IssueDate);
		}

		public void TestIssueDateIsUnchangedWhenAlreadyPopulated()
		{
			var testDateTime = new ZDateTime(2019, 12, 24, 17, 0, 0);
			message.EM_MessageSubType = TemporaryStorageMessageSubTypeList.Codes.PreliminarySummaryDeclaration;
			cusTempStorageDec.CusEntryNumber.CE_IssueDate = testDateTime;
			ProcessMessage(message);
			AssertEquals(testDateTime, cusTempStorageDec.CusEntryNumber.CE_IssueDate);
		}

		public void TestPopulateLogbookRegNums()
		{
			ProcessMessage(message);
			AssertEquals("ATB150002930220195875, 24DE12345678901234", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegNumMRN()
		{
			dataProviderMock.Setup(p => p.ReferenceNumber).Returns((string)null);
			ProcessMessage(message);
			AssertEquals("24DE12345678901234", message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegNumReferenceNumber()
		{
			dataProviderMock.Setup(p => p.MRN).Returns((string)null);
			ProcessMessage(message);
			AssertEquals("ATB150002930220195875", message.GetLogbookRegistrationNumber());
		}

		public void TestSendAcknownledgementEMail_StaffMember()
		{
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				AssertUnsolicitedMessageOrAcknowledgeEmail(jobHeader, new ZString[] { "senduser@mail.com" });
			}
		}

		public void TestSendAcknownledgementEMail_NominatedGroup()
		{
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageGroupNotification(Core.Constants.EmailTo.NominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				AssertUnsolicitedMessageOrAcknowledgeEmail(jobHeader, new ZString[] { "staff1@group-suma.com", "staff2@group-suma.com" });
			}
		}

		public void TestSendAcknownledgementEMail_StaffMemberAndNominatedGroup()
		{
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageAcknowledgements.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				ProcessMessage(message);
				AssertUnsolicitedMessageOrAcknowledgeEmail(jobHeader, new ZString[] { "senduser@mail.com", "staff1@group-suma.com", "staff2@group-suma.com" });
			}
		}

		public void TestSendErrorEMail_StaffMember()
		{
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageGroupNotification(Core.Constants.EmailTo.StaffMember, ZGuid.Empty);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageErrors.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				dataProviderMock.Setup(m => m.NotificationSeverity).Returns(new ZString[] { NotificationTypeList.Codes.Error });
				ProcessMessage(message);
				AssertUnsolicitedMessageOrAcknowledgeEmail(jobHeader, new ZString[] { "senduser@mail.com" });
			}
		}

		public void TestSendErrorEMail_NominatedGroup()
		{
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageGroupNotification(Core.Constants.EmailTo.NominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageErrors.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				dataProviderMock.Setup(m => m.NotificationSeverity).Returns(new ZString[] { NotificationTypeList.Codes.Error });
				ProcessMessage(message);
				AssertUnsolicitedMessageOrAcknowledgeEmail(jobHeader, new ZString[] { "staff1@group-suma.com", "staff2@group-suma.com" });
			}
		}

		public void TestSendErrorEMail_StaffMemberAndNominatedGroup()
		{
			var groupNotificationRegistryItem = new EU.Registry.TemporaryStorageGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, TempStorageTestHelpers.CreateGlbGroup(Factory).PK);
			using (EU.Registry.EUCustomsDataRegistry.Instance.SendTemporaryStorageErrors.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, groupNotificationRegistryItem))
			{
				dataProviderMock.Setup(m => m.NotificationSeverity).Returns(new ZString[] { NotificationTypeList.Codes.Error });
				ProcessMessage(message);
				AssertUnsolicitedMessageOrAcknowledgeEmail(jobHeader, new ZString[] { "senduser@mail.com", "staff1@group-suma.com", "staff2@group-suma.com" });
			}
		}

		protected override ZString MessageFriendlyName => "Temporary Storage CUSREC Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSREC>> Processor => new TemporaryStorageCUSRECMessageProcessor(logger);

		protected override void SetUp()
		{
			base.SetUp();

			jobHeader = Factory.New<CusTempStorageJobHeader>();
			jobHeader.SJH_OH_Customer = Factory.NewWithValidTestData<OrgHeader>().PK;
			jobHeader.SJH_JobReference = "REFERENCE1";

			cusTempStorageDec = CUSPRLCusTempStorageDec.New(jobHeader);
			originalMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(cusTempStorageDec, ReferencedMessageIdentifier, "senduser@mail.com");

			dataProviderMock = new Mock<ICUSREC> { CallBase = true };
			dataProviderMock.Setup(m => m.MessageIdentifier).Returns(MessageIdentifier);
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(RegistrationNumber);
			dataProviderMock.Setup(m => m.MRN).Returns(MRN);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(LocalReferenceNumber);
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ReferencedMessageIdentifier);
			dataProviderMock.Setup(m => m.NotificationSeverity).Returns(new ZString[] { NotificationTypeList.Codes.Information });
			goodsItemDataProviderMock = new Mock<ICUSRECGoodsItem> { CallBase = true };
			goodsItemDataProviderMock.Setup(d => d.SequenceNumber).Returns("1");
			goodsItemDataProviderMock.Setup(d => d.NotificationSeverity).Returns(NotificationTypeList.Codes.Information);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new ICUSRECGoodsItem[] { goodsItemDataProviderMock.Object });

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSREC>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			message = messageMock.Object;
			Factory.Save();
		}
		CusTempStorageJobHeader jobHeader;
		CusTempStorageDec cusTempStorageDec;
		EDIMessage originalMessage;
		Mock<ICUSREC> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ICUSREC>> messageMock;
		Mock<ICUSRECGoodsItem> goodsItemDataProviderMock;
		AtlasInboundEDIMessage<ICUSREC> message;

		const string MessageIdentifier = "CUSREC58750000000375302250219160050";
		const string ReferencedMessageIdentifier = "DE899978300000000812";
		const string RegistrationNumber = "ATB150002930220195875";
		const string MRN = "24DE12345678901234";
		const string LocalReferenceNumber = "19DE587500026775M6";

		CusTempStorageLine AddLine(ZInt lineNo, ZBool isModified, ZDateTime lastEdit)
		{
			var line = (CusTempStorageLine)cusTempStorageDec.CusTempStorageLines.AddNew();
			line.TSL_LineNo = lineNo;
			line.TSL_IsModified = isModified;
			line.TSL_SystemLastEditTimeUtc = lastEdit;
			return line;
		}

		void AssertUnsolicitedMessageOrAcknowledgeEmail(CusTempStorageJobHeader jobHeader, ZString[] expectedEmailRecipientsMailAddress)
		{
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			var reference = jobHeader.SJH_JobReference;
			var subject = $"SumA CUSREC - Customs Receipt Message Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>SumA CUSREC - Customs Receipt Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=TemporaryStorage&BusinessEntityPK={jobHeader.PK}";
			var bodyMessageSummary = $"Your SumA Declaration for Job {reference} received a Customs Response Message. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + $"<tr><td>MRN</td><td>{MRN}</td></tr>"
								   + $"<tr><td>Registration Number</td><td>{RegistrationNumber}</td></tr>"
								   + $"<tr><td>Local Reference Number</td><td>{LocalReferenceNumber}</td></tr>"
								   + "</table>";
			CombineAssertions(() => AssertEmailWithTable(ZString.Empty, email, expectedEmailRecipientsMailAddress, subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable));
		}
	}
}

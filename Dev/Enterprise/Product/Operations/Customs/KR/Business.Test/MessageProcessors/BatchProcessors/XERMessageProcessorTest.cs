using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class XERMessageProcessorTest : TestCaseWithFactory
	{
		public void TestXERMessageProcessor()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				var interchange = helper.CreateIncomingEDIInterchangeForTest("XER_OriginalMessage.txt", EDIInterchangeType.XER);
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				((IInboundInterchangeProcessor)new KRCInboundInterchangeProcessor(new LoggingInformation())).Execute();
				new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch();

				var incomingMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange.PK));
				AssertEquals(helper.ReturnEmbeddedFileData("XER_Message.txt"), incomingMessage.EM_MessageData);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertContains(NotificationSender.XERNotificationEmailBody, email.Body);
				AssertContains("시스템 에러 수신", email.Subject);
			}
		}

		public void TestStatusUpdateWithEntry()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				var inInterchange = helper.CreateIncomingEDIInterchangeForTest("XER_OriginalMessage.txt", EDIInterchangeType.XER);
				var incomingMessage = helper.CreateIncomingMessageForTest("XER_OriginalMessage.txt", EDIInterchangeType.XER);
				inInterchange.ContainedMessages.Add(incomingMessage);
				var outInterchange = helper.CreateOutgoingEDIInterchangeForTest(ElectronicDocumentTypeList.Codes._830);

				var declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				outInterchange.ContainedMessages[0].EM_LinkedObject = entryHeader;
				outInterchange.ContainedMessages[0].EM_SystemCreateUser = "ORG";
				entryHeader.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch();
				entryHeader.Reload();
				incomingMessage.Reload();
				AssertEquals(CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal, entryHeader.CH_Status);
				AssertEquals(incomingMessage.EM_LinkedObject, entryHeader);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("OriginalSender@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains(NotificationSender.XERNotificationEmailBody, email.Body);
				AssertContains("XER Response", email.Subject);

				incomingMessage = helper.CreateIncomingMessageForTest("XER_OriginalMessage.txt", EDIInterchangeType.XER);
				inInterchange.ContainedMessages.Add(incomingMessage);
				entryHeader.CH_Status = CustomsMessageStatusTypeList.Codes.AmendmentSent;
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch();
				entryHeader.Reload();
				incomingMessage.Reload();
				AssertEquals(CustomsMessageStatusTypeList.Codes.ErrorSendingAmendment, entryHeader.CH_Status);
				AssertEquals(incomingMessage.EM_LinkedObject, entryHeader);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("OriginalSender@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains(NotificationSender.XERNotificationEmailBody, email.Body);
				AssertContains("XER Response", email.Subject);

				incomingMessage = helper.CreateIncomingMessageForTest("XER_OriginalMessage.txt", EDIInterchangeType.XER);
				inInterchange.ContainedMessages.Add(incomingMessage);
				entryHeader.CH_Status = CustomsMessageStatusTypeList.Codes.CancellationSent;
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch();
				entryHeader.Reload();
				incomingMessage.Reload();
				AssertEquals(CustomsMessageStatusTypeList.Codes.ErrorSendingCancellation, entryHeader.CH_Status);
				AssertEquals(incomingMessage.EM_LinkedObject, entryHeader);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("OriginalSender@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains(NotificationSender.XERNotificationEmailBody, email.Body);
				AssertContains("XER Response", email.Subject);
				AssertNotContains("A response message has been received from Customs.", email.Body);
			}
		}

		public void TestStatusUpdateWithMiscRequest()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				var outInterchange = helper.CreateOutgoingEDIInterchangeForTest(ElectronicDocumentTypeList.Codes._5AC);
				var miscRequestHeader = Factory.New<CusMiscRequestHeader>();
				var entryNum1 = Factory.New<CusEntryNumber>();
				entryNum1.CE_EntryIsSystemGenerated = true;
				entryNum1.CE_ParentID = miscRequestHeader.PK;
				entryNum1.CE_ParentTable = miscRequestHeader.TableName;
				entryNum1.CE_RN_NKCountryCode = GlbBranch.CurrentBranch.Country.RN_Code;
				entryNum1.CE_EntryType = ElectronicDocumentTypeList.Codes._5AC;
				entryNum1.CE_EntryNum = "6N00220000042X";
				entryNum1.CE_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);

				miscRequestHeader.CMR_MessageType = ElectronicDocumentTypeList.Codes._5AC;
				miscRequestHeader.CMR_GB = GlbBranch.CurrentBranch.PK;
				miscRequestHeader.CMR_RequestDate = ZDateTime.Today;
				miscRequestHeader.CMR_CustomsOffice = "01020";
				outInterchange.ContainedMessages[0].EM_LinkedObject = miscRequestHeader;
				outInterchange.ContainedMessages[0].EM_SystemCreateUser = "ORG";

				var inInterchange = helper.CreateIncomingEDIInterchangeForTest("XER_OriginalMessage.txt", EDIInterchangeType.XER);
				var incomingMessage = helper.CreateIncomingMessageForTest("XER_OriginalMessage.txt", EDIInterchangeType.XER);
				inInterchange.ContainedMessages.Add(incomingMessage);
				miscRequestHeader.CMR_Status = CustomsMessageStatusTypeList.Codes.OriginalSent;
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch();
				miscRequestHeader.Reload();
				incomingMessage.Reload();
				AssertEquals(CustomsMessageStatusTypeList.Codes.ErrorSendingOriginal, miscRequestHeader.CMR_Status);
				AssertEquals(incomingMessage.EM_LinkedObject, miscRequestHeader);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals(1, email.Recipients.Count);
				AssertEquals("OriginalSender@wisetechglobal.com", email.Recipients[0].Email);
				AssertContains(NotificationSender.XERNotificationEmailBody, email.Body);
				AssertNotContains("A response message has been received from Customs.", email.Body);
				AssertEquals("XER Response for Request Number: MSC00000001 / 제출번호: 6N00220000042X", email.Subject);
			}
		}

		public void TestStatusUpdateWithCusPollingTransaction()
		{
			using (KRCustomsRegistry.Instance.ImportEmailGroup.SetTemporaryValue(GlbBranch.CurrentBranch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, importGroup.PK.ToGuid()))
			{
				var outInterchange = helper.CreateOutgoingEDIInterchangeForTest(Constants.EDIInterchangeType.DOC);
				var cusPollingTransaction = Factory.New<CusPollingTransaction>();
				cusPollingTransaction.CPT_TransactionID = "TEST";
				cusPollingTransaction.CPT_Reference = ElectronicDocumentTypeList.Codes._830;
				cusPollingTransaction.CPT_NumberOfAttempts = 1;
				outInterchange.ContainedMessages[0].EM_LinkedObject = cusPollingTransaction;
				outInterchange.ContainedMessages[0].EM_SystemCreateUser = "ORG";

				var inInterchange = helper.CreateIncomingEDIInterchangeForTest("XER_OriginalMessage.txt", EDIInterchangeType.XER);
				var incomingMessage = helper.CreateIncomingMessageForTest("XER_OriginalMessage.txt", EDIInterchangeType.XER);
				inInterchange.ContainedMessages.Add(incomingMessage);
				cusPollingTransaction.CPT_Status = CusPollingTransactionStatusList.Codes.Opened;
				Factory.Save();

				Env.OutgoingCustomsMailManager.EmailsCreated.Clear();

				new KRCIncomingMessageProcessor(new LoggingInformation()).ExecuteBatch();
				cusPollingTransaction.Reload();
				incomingMessage.Reload();
				AssertEquals(CusPollingTransactionStatusList.Codes.Error, cusPollingTransaction.CPT_Status);
				AssertEquals(incomingMessage.EM_LinkedObject, cusPollingTransaction);

				AssertEquals(1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				var email = Env.OutgoingCustomsMailManager.EmailsCreated[0];
				AssertEquals(1, email.Recipients.Count);
				AssertNotContains("A response message has been received from Customs.", email.Body);
				AssertEquals("ImportGroupTest@wisetechglobal.com", email.Recipients[0].Email);
			}
		}

		protected override void SetUp()
		{
			var staff1 = Factory.New<GlbStaff>();
			staff1.GS_Code = "ORG";
			staff1.GS_LoginName = "Orgin";
			staff1.GS_EmailAddress = "OriginalSender@wisetechglobal.com";

			var staff2 = Factory.New<GlbStaff>();
			staff2.GS_Code = "T1";
			staff2.GS_LoginName = "Test1";
			staff2.GS_EmailAddress = "ImportGroupTest@wisetechglobal.com";
			importGroup = Factory.New<GlbGroup>();
			var link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = importGroup.PK;
			link1.GK_GS = staff2.PK;

			helper = new TestDataSetUpHelper(Factory, typeof(ESRMessageProcessorTest));
		}
		TestDataSetUpHelper helper;
		GlbGroup importGroup;
	}
}

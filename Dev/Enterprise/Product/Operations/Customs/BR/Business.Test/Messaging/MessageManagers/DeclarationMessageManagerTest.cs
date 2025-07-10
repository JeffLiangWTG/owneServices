using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	public abstract class DeclarationMessageManagerTest : TestCaseWithFactory
	{
		public void TestBusinessObject()
		{
			AssertEquals(MessageSender, MessageManager.BusinessObject);
		}

		public void TestMessageFriendlyName()
		{
			AssertEquals(ExpectedMessageFriendlyName, MessageManager.MessageFriendlyName);
		}

		public void TestCanSendOriginal()
		{
			AssertEquals(true, MessageManager.CanSendOriginal);
		}

		public void TestCanSendWithdrawal()
		{
			AssertEquals(false, MessageManager.CanSendWithdrawal);
		}

		public void TestIsWaitingForResponse()
		{
			var entryHeader = MessageSender.Header;
			Assert(!MessageManager.IsWaitingForResponse);
			entryHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			Assert(MessageManager.IsWaitingForResponse);
			entryHeader.CH_Status = MessageStatusList.Codes.AcknowledgedOriginal;
			Assert(!MessageManager.IsWaitingForResponse);
		}

		public void TestHasActiveMessages()
		{
			AssertEquals("HasActiveMessages is false", false, MessageManager.HasActiveMessages);
			MessageSender.Header.CH_EntryStatus = "CLR";
			AssertEquals("HasActiveMessages is true", true, MessageManager.HasActiveMessages);
		}

		[TestDate(2021, 7, 19)]
		public void TestGenerateOriginalMessage()
		{
			if (OriginalMessageType != null)
			{
				MessageSender.MessageType = "XXX";
				AssertGenerateMessages(() => MessageManager.GenerateOriginalMessages(MessageSender), OriginalMessageType);
				AssertEquals("MessageType not changed", "XXX", MessageSender.MessageType);
			}
			else
			{
				Assert("Original Message is not supported", true);
			}
		}

		[TestDate(2021, 7, 19)]
		public void TestGenerateAmendmentMessage()
		{
			if (AmendmentMessageType != null)
			{
				MessageSender.MessageType = "XXX";
				AssertGenerateMessages(() => MessageManager.GenerateAmendmentMessages(MessageSender), AmendmentMessageType);
				AssertEquals("MessageType not changed", "XXX", MessageSender.MessageType);
			}
			else
			{
				Assert("Amendment Message is not supported", true);
			}
		}

		[TestDate(2021, 7, 19)]
		public void TestGenerateWithdrawalMessagesCore()
		{
			if (WithdrawalMessageType != null)
			{
				MessageSender.MessageType = "XXX";
				AssertGenerateMessages(() => MessageManager.GenerateWithdrawalMessages(MessageSender), WithdrawalMessageType);
				AssertEquals("MessageType not changed", "XXX", MessageSender.MessageType);
			}
			else
			{
				Assert("Withdrawal Message is not supported", true);
			}
		}

		[TestDate(2021, 7, 19)]
		public void TestGenerateMessages()
		{
			if (MessageSender.MessageType.IsEmpty)
			{
				MessageSender.MessageType = "XXX";
			}
			AssertGenerateMessages(MessageManager.GenerateMessages, MessageSender.MessageType, ExpectedEM_MessageText);
		}

		public void TestCH_StatusNotChangesWhenNoMessageIsCreated()
		{
			var entryHeader = MessageSender.Header;
			entryHeader.CH_CustomsPostedStatus = "ACC";
			entryHeader.EntryNumber = "123";

			var ediMessages = MessageManager.GenerateMessages();

			AssertEquals("CH_Status", ExpectedCH_StatusWhenNoMessages, entryHeader.CH_Status);
		}

		protected void AssertGenerateMessages(Func<EDIMessage[]> generateMessagesFunc, ZString messageSubType, string expectedMessageText = null)
		{
			Factory.Save();
			var entryHeader = MessageSender.Header;
			MessageSender.ShouldSend = true;
			AssertGenerateMessages(generateMessagesFunc(), messageSubType, entryHeader, expectedMessageText);

			if (ShouldSendMessageWhenEntryNumberHasValue(messageSubType))
			{
				TestDateAttribute.AddDays(1);
				entryHeader.MovementReferenceNumberSetter("21BR0000022649");
				AssertGenerateMessages(generateMessagesFunc(), messageSubType, entryHeader, expectedMessageText);
			}
		}

		protected virtual bool ShouldSendMessageWhenEntryNumberHasValue(ZString messageType) => true;

		protected void AssertGenerateMessages(EDIMessage[] messages, ZString messageSubType, CusEntryHeader entryHeader, string expectedMessageText = null)
		{
			Factory.Save();
			CombineAssertions($"EDIMessage generated for {messageSubType}", () =>
			{
				AssertEquals("Messages count", 1, messages.Length);
				AssertGenerateMessages(messages[0], ExpectedEM_MessageType, messageSubType, ExpectedEM_ApplicationReference, entryHeader, expectedMessageText);
			});
		}

		protected void AssertGenerateMessages(EDIMessage message, ZString messageType, ZString messageSubType, ZString applicationReference, CusEntryHeader entryHeader, string expectedMessageText = null)
		{
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.BRCustoms, message.EM_ApplicationCode);
			AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals("EM_ReceiveTransmit", EDIMessage.Direction.Transmit, message.EM_ReceiveTransmit);
			AssertEquals("EM_MessageType", messageType, message.EM_MessageType);
			AssertEquals("EM_MessageSubType", messageSubType, message.EM_MessageSubType);
			AssertEquals("EM_LinkUniqueID", entryHeader.PK, message.EM_LinkUniqueID);
			AssertEquals("EM_LinkTable", CusEntryHeaderSchema.Constants.TableName, message.EM_LinkTable);
			AssertEquals("EM_ApplicationReference", applicationReference, message.EM_ApplicationReference);
			AssertEquals("EM_MessageOwner", ZString.Empty, message.EM_MessageOwner);
			AssertEquals("EM_GP", ExpectedEM_GP, message.EM_GP);
			AssertEquals("CH_Status", "AWA", entryHeader.CH_Status);
			AssertEquals("CH_EntrySubmittedDate", new ZDateTime(2021, 7, 19), entryHeader.CH_EntrySubmittedDate);

			if (expectedMessageText == null)
			{
				Assert("EM_MessageText should not be empty", !message.EM_MessageText.IsEmpty);
			}
			else if (expectedMessageText.Length == 0)
			{
				Assert("EM_MessageText should be empty", message.EM_MessageText.IsEmpty);
			}
			else
			{
				var unformatedMessageText = message.EM_MessageText.Replace("\n", string.Empty).Replace("\r", string.Empty).Replace(" ", string.Empty);
				AssertContains("EM_MessageText", expectedMessageText, unformatedMessageText);
			}
		}

		public void TestRequiresAmendment()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobDeclarationMessageType;
			if ((declaration as IMessageManageableBizObj).GetMessageManagerForAmendmentDetection() == null)
			{
				Assert($"{JobDeclarationMessageType} does not support Amendment Detection", true);
			}
			else
			{
				var ediMessageQuery = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.BRCustoms);
				Factory.Load<EDIMessage>(ediMessageQuery).ForEach(m => m.Delete());

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_AdditionalInformationOption = AdditionalInformationOptions.Codes.SystemGeneratedAndFreeText;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.EntryNumber = "1234";
				entryHeader.CH_EntryStatus = "CUS";
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryHeader.EntryNumber = "B32342";
				var entryLine1 = entryHeader.MergedLines.AddNew();
				var entryLine2 = entryHeader.MergedLines.AddNew();
				var invoiceHeader = declaration.Invoices.AddNew();
				invoiceHeader.JZ_IncoTerm = "TET";
				invoiceHeader.JZ_InvoiceAmount = 1000.00m;
				invoiceHeader.JZ_RX_NKInvoice_Currency = "BRL";
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.JI_CEI = entryInstruction.PK;
				invoiceLine1.JI_CL = entryLine1.PK;
				invoiceLine1.JI_Procedure = "AB10";
				invoiceLine1.JI_LinePrice = 10m;
				invoiceLine2.JI_CEI = entryInstruction.PK;
				invoiceLine2.JI_CL = entryLine2.PK;
				invoiceLine2.JI_Procedure = "CD20";
				invoiceLine2.JI_LinePrice = 20m;
				Factory.Save();

				var messageSender = CreateMessageSendingObject(entryHeader);
				var messageManager = MessageManagerCreator.CreateNew(messageSender);
				AssertEquals($"{nameof(messageManager)} - RequiresAmendment should be false", false, messageManager.RequiresAmendment());
				Factory.Save();

				AssertEquals("CH_Status should not be changed", ZString.Empty, entryHeader.CH_Status);
				AssertEquals("CH_EntrySubmittedDate should not be changed", ZDateTime.Empty, entryHeader.CH_EntrySubmittedDate);
				AssertEquals("No EDIMessage should be saved", 0, new BusinessObjectFactory().Load<EDIMessage>(ediMessageQuery).Length);

				DoChangesRequiresAmendment(declaration);

				AssertEquals($"{nameof(messageManager)} - RequiresAmendment should be true", true, messageManager.RequiresAmendment());
				Factory.Save();

				AssertEquals("CH_Status should not be changed", ZString.Empty, entryHeader.CH_Status);
				AssertEquals("CH_EntrySubmittedDate should not be changed", ZDateTime.Empty, entryHeader.CH_EntrySubmittedDate);
				AssertEquals("No EDIMessage should be saved", 0, new BusinessObjectFactory().Load<EDIMessage>(ediMessageQuery).Length);
			}
		}

		protected virtual void DoChangesRequiresAmendment(JobDeclaration declaration)
		{
			declaration.InvoiceLines[0].JI_LinePrice = 98m;
			declaration.InvoiceLines[1].JI_LinePrice = 12m;
		}

		protected virtual CusEntryHeader CreateEntryHeader()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "BR1";
			var password = BRGlbStaffWrapper.Get(broker).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobDeclarationMessageType;
			declaration.JE_GS_NKCusAgent = broker.GS_Code;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_CEI = entryInstruction.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			return entryHeader;
		}

		protected abstract ZString JobDeclarationMessageType { get; }
		protected abstract string OriginalMessageType { get; }
		protected abstract string AmendmentMessageType { get; }
		protected abstract string WithdrawalMessageType { get; }
		protected virtual string ExpectedCH_StatusWhenNoMessages => BRMessageStatusList.Codes.AwaitingResponse;

		protected abstract ZString ExpectedMessageFriendlyName { get; }
		protected abstract ZString ExpectedEM_MessageType { get; }
		protected virtual string ExpectedEM_MessageText => null;
		protected virtual ZGuid ExpectedEM_GP => MessageSender.Header.Declaration.BrokerCertificate.PK;
		protected virtual ZString ExpectedEM_ApplicationReference => MessageSender.Header.MovementReferenceNumber;

		protected abstract DeclarationMessageSendingObject CreateMessageSendingObject(CusEntryHeader entryHeader);

		protected DeclarationMessageSendingObject MessageSender => messageSender ?? (messageSender = CreateMessageSendingObject(CreateEntryHeader()));
		DeclarationMessageSendingObject messageSender;

		protected virtual DeclarationMessageManager MessageManager => messageManager ?? (messageManager = MessageManagerCreator.CreateNew(MessageSender) as DeclarationMessageManager);
		DeclarationMessageManager messageManager;
	}
}

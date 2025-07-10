using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class LPCODeclarationMessageManagerTest : DeclarationMessageManagerTest
	{
		[TestDate(2021, 7, 19)]
		public void TestGenerateMessages_ALE()
		{
			MessageSender.MessageType = LPCOEntryActionCodeList.Codes.ALE;
			MessageSender.NewEffectiveDate = new ZDate(2021, 7, 19);
			MessageSender.Reason = "reason";
			AssertGenerateMessages(MessageManager.GenerateMessages, LPCOEntryActionCodeList.Codes.ALE, "{\"novaDataFimVigencia\":\"2021-07-19\",\"justificativa\":\"reason\"}");
		}

		[TestDate(2021, 7, 19)]
		public void TestGenerateMessages_RCA()
		{
			MessageSender.MessageType = LPCOEntryActionCodeList.Codes.RCA;
			MessageSender.Reason = "reason";
			AssertGenerateMessages(MessageManager.GenerateMessages, LPCOEntryActionCodeList.Codes.RCA, "{\"justificativa\":\"reason\"}");
		}

		[TestDate(2021, 7, 19)]
		public void TestGenerateMessages_REQ()
		{
			MessageSender.MessageType = LPCOEntryActionCodeList.Codes.REQ;
			MessageSender.Reason = "reason";
			MessageSender.Requirement = 444;
			AssertGenerateMessages(MessageManager.GenerateMessages, LPCOEntryActionCodeList.Codes.REQ, "{\"justificativa\":\"reason\"}");
		}

		[TestDate(2021, 7, 19)]
		public void TestGenerateMessages_COM()
		{
			MessageSender.MessageType = LPCOEntryActionCodeList.Codes.COM;
			MessageSender.Reason = "reason";
			MessageSender.EntryNumber = "LPCO1234567890";
			MessageSender.EntryLineNumber = 444;
			MessageSender.Version = "1.1";
			AssertGenerateMessages(MessageManager.GenerateMessages, LPCOEntryActionCodeList.Codes.COM, "{\"numeroDocumento\":\"LPCO1234567890\",\"numeroItemDocumento\":444,\"versaoCompatibilizacao\":\"1.1\",\"justificativa\":\"reason\"}");
		}

		[TestDate(2021, 7, 19)]
		public void TestGenerateMessages_MSG()
		{
			MessageSender.MessageType = LPCOEntryActionCodeList.Codes.MSG;
			MessageSender.Message = "message";
			AssertGenerateMessages(MessageManager.GenerateMessages, LPCOEntryActionCodeList.Codes.MSG, "{\"mensagem\":\"message\"}");
		}

		protected override ZString ExpectedMessageFriendlyName => MessageTypeList.Descriptions.LPC;

		protected override ZString JobDeclarationMessageType => BRJobMessageTypeList.Codes.LPCO;

		protected override string OriginalMessageType => null;

		protected override string AmendmentMessageType => null;

		protected override string WithdrawalMessageType => null;

		protected override ZString ExpectedEM_MessageType => MessageTypeList.Codes.LPC;

		protected override string ExpectedEM_MessageText => ZString.Empty;

		protected override DeclarationMessageSendingObject CreateMessageSendingObject(CusEntryHeader entryHeader) => new LPCODeclarationMessageSendingObject(entryHeader);

		protected override ZString ExpectedEM_ApplicationReference
		{
			get
			{
				return MessageSender.MessageType == LPCOEntryActionCodeList.Codes.REQ ? $"{MessageSender.Header.MovementReferenceNumber}|{MessageSender.Requirement}" : base.ExpectedEM_ApplicationReference.ToString();
			}
		}

		new LPCODeclarationMessageSendingObject MessageSender => base.MessageSender as LPCODeclarationMessageSendingObject;
	}
}

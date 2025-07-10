using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Business
{
	public class DeclarationMessageManager : SingleMessageManager
	{
		public DeclarationMessageManager(CNJobDeclarationMessageSendingObject messageSender)
		{
			this.messageSender = Argument.NotNull(messageSender, nameof(messageSender));
		}
		readonly CNJobDeclarationMessageSendingObject messageSender;

		public override string MessageFriendlyName => messageSender.MessageTypeDescription;

		public override BusinessObject BusinessObject => messageSender;

		public override bool CanSendOriginal => true;

		public override bool CanSendWithdrawal => false;

		public override bool IsWaitingForResponse => messageSender.Header.IsWaitingForResponse;

		public override bool HasActiveMessages => messageSender.Header.HasBeenLodgedAtCustoms;

		public EDIMessage[] GenerateMessages()
		{
			var message = GenerateMessage();

			var entryHeader = messageSender.Header;
			entryHeader.CH_Status = JobMessageStatusList.GetAwaitingStatusByDeclarationType(entryHeader.GetDeclarationType());
			entryHeader.Logs.AddNew(AutoEvents.MessageSent, $"Send to Customs;{entryHeader.CH_BGMReference}");

			return new EDIMessage[] { message };
		}

		CNEDIMessage GenerateMessage()
		{
			var entry = messageSender.Header;

			var message = entry.Factory.New<CNEDIMessage>();
			entry.Messages.Add(message);
			message.EM_MessageType = EDIMessageTypeList.Codes.DEC;
			message.EM_MessageSubType = entry.CH_MessageType;
			message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			message.EM_Status = Messaging.Integration.EDIMessageStatusList.Codes.Queued;
			message.EM_ApplicationReference = entry.CH_BGMReference;
			message.EM_MessageText = messageSender.ToMessageString();
			message.EM_MessageInterpretation = entry.HtmlFormatEntryData;

			if (messageSender.DeclarationType != DeclarationTypeList.Codes.PreliminaryDeclaration && entry.EntryInstruction != null && !entry.EntryInstruction.Attachments.IsNullOrEmpty())
			{
#if NETFRAMEWORK
				var attachments = entry.EntryInstruction.Attachments.Cast<EntryInstructionAttachment>()
					.Where(x => x.EDoc.IsValid)
					.DistinctBy(x => x.EDoc);
#else
				var attachments = IEnumerableExtensions.DistinctBy(entry.EntryInstruction.Attachments.Cast<EntryInstructionAttachment>()
					.Where(x => x.EDoc.IsValid), x => x.EDoc);
#endif
				foreach (var attachment in attachments)
				{
					var attach = message.MessageAttachments.AddNew();
					attach.EG_StorageDocsGuid = attachment.EDoc;
					attach.EG_FileName = attachment.FileName;
				}
			}
			return message;
		}

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo) => GenerateMessages();

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo) => GenerateMessages();

		protected override EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo) => GenerateMessages();

		public void RollbackOnSavingFailed()
		{
			messageSender.Header.Logs.LogsNotInDB.DeleteAll();
		}
	}
}

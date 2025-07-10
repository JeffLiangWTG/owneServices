using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public abstract class BaseConsultMessageSender
	{
		public BaseConsultMessageSender(CusEntryHeader entryHeader)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}
		protected readonly CusEntryHeader EntryHeader;

		protected abstract IMessageSendingObject GetMessageSendingObject();

		public abstract string CanSendMessage { get; }

		public EDIMessage SendMessage()
		{
			var sendingObject = GetMessageSendingObject();
			sendingObject.MessageType = EDIMessageSubTypeList.Codes.CompleteConsult;
			var message = sendingObject.CreateCustomsMessage();
			EntryHeader.Messages.Add(message);
			EntryHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			return message;
		}

		public int SendMessageAndSave()
		{
			var message = SendMessage();
			try
			{
				EntryHeader.Factory.Save();
				return 1;
			}
			catch (ZSaveException ex)
			{
				message.Delete();
				ZExceptionReporting.HandleSaveException(ex);
				return 0;
			}
		}
	}
}

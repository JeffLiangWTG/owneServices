using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Business
{
	public abstract class IEAutoSendCustomsMessageProcessor : AutoSendCustomsMessageProcessor
	{
		protected IEAutoSendCustomsMessageProcessor(BaseJobDeclaration declaration) : base(declaration)
		{
		}

		protected override IEnumerable<CusEntryHeader> GetEntryHeadersToSendCore() => Declaration.ActiveEntryHeaders.Where(h => h.Messages.Count is 0);

		protected override ZBool SendCustomsMessageCore(INotifications notifications, CusEntryHeader entryHeader)
		{
			var entry = entryHeader as Declaration.CusEntryHeader;
			if (entry is null)
			{
				return false;
			}
			else
			{
				var sendingAction = GetSendingAction(entry);
				sendingAction.MessageType = MessageType;
				return sendingAction.CreateSender().Send() is not null;
			}
		}

		protected abstract CusEntryHeaderMessageSendingAction GetSendingAction(Declaration.CusEntryHeader entryHeader);

		protected abstract string MessageType { get; }
	}
}

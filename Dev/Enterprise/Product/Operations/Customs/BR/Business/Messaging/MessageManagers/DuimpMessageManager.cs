using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class DuimpMessageManager : DeclarationMessageManager<DuimpMessageSendingObject>
	{
		public DuimpMessageManager(DuimpMessageSendingObject messageSender) : base(messageSender)
		{
		}

		public override string MessageFriendlyName => MessageTypeList.Descriptions.CDD;

		protected override string OriginalMessageType => ImportEntryActionCodeList.Codes.ORI;

		protected override string AmendmentMessageType => null;

		protected override string WithdrawalMessageType => null;

		public override bool HasActiveMessages => base.HasActiveMessages || IsWaitingForResponse;

		protected override IEnumerable<EDIMessage> GenerateCustomsMessage(IMessageSendingObject sendingObject, string forceMessageType = null)
		{
			var headerSendingObject = sendingObject as DuimpMessageSendingObject;
			var entryHeader = headerSendingObject.Header;
			var factory = headerSendingObject.Factory;

			var messages = new List<EDIMessage>();

			var messageType = forceMessageType ?? sendingObject.MessageType;
			if (messageType == ImportEntryActionCodeList.Codes.ORI)
			{
				if (entryHeader.MovementReferenceNumber.IsEmpty)
				{
					messages.AddRange(base.GenerateCustomsMessage(headerSendingObject, EDIMessageSubTypeList.Codes.Original));
				}
				else
				{
					if (entryHeader.CH_CustomsPostedStatus.NeedsToSendMessage())
					{
						messages.AddRange(base.GenerateCustomsMessage(headerSendingObject, EDIMessageSubTypeList.Codes.Update));
					}

					var maxNumberOfEntryLine = BRCustomsDataRegistry.Instance.MaxNumberOfEntryLineInDuimpMessage.Value;

					foreach (var groupedLines in entryHeader.AllEntryLines.Where(x => x.CL_CustomsPostedStatus.NeedsToSendMessage()).GroupBy(x => x.CL_CustomsPostedStatus))
					{
						if (GetMessageSubTypeByCustomsPostedStatus(groupedLines.Key) is ZString messageSubType && !messageSubType.IsEmpty)
						{
							foreach (var entryLines in groupedLines.Batch(messageSubType == EDIMessageSubTypeList.Codes.Deletion ? 1 : maxNumberOfEntryLine))
							{
								messages.Add(new DuimpLinesMessageSendingObject(entryLines, messageSubType).CreateCustomsMessage());
							}
						}
					}
				}
			}
			else
			{
				messages.AddRange(base.GenerateCustomsMessage(headerSendingObject, messageType));
			}

			return messages;
		}

		ZString GetMessageSubTypeByCustomsPostedStatus(string customsPostedStatus)
		{
			return customsPostedStatus switch
			{
				CustomsPostedStatusList.Codes.Active => EDIMessageSubTypeList.Codes.Addition,
				CustomsPostedStatusList.Codes.UpdatePending => EDIMessageSubTypeList.Codes.Update,
				CustomsPostedStatusList.Codes.DeletePending => EDIMessageSubTypeList.Codes.Deletion,
				_ => ZString.Empty,
			};
		}
	}
}

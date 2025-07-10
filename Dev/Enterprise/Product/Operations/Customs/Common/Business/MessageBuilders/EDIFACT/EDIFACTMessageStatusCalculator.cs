using System;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.Common.MessageBuilders
{
	public abstract class EDIFACTMessageStatusCalculator
	{
		public ZString GetMessageSubType(ZString currentMessageStatus)
		{
			return GetMessageSubTypeCore(currentMessageStatus);
		}

		protected virtual ZString GetMessageSubTypeCore(ZString currentMessageStatus)
		{
			if (currentMessageStatus.EndsWith("O", StringComparison.OrdinalIgnoreCase))
			{
				return MessageSubTypeCodes.Codes.Original;
			}

			if (currentMessageStatus.EndsWith("C", StringComparison.OrdinalIgnoreCase))
			{
				return MessageSubTypeCodes.Codes.Change;
			}

			if (currentMessageStatus.EndsWith("D", StringComparison.OrdinalIgnoreCase))
			{
				return MessageSubTypeCodes.Codes.Cancellation;
			}

			return MessageSubTypeCodes.Codes.Undefined;
		}

		#region Booleans

		public virtual bool IsAwaitingReply(ZString currentMessageStatus)
		{
			return MessageStatusList.IsAwaiting(currentMessageStatus);
		}

		public virtual bool IsWithdrawn(ZString currentJobStatus)
		{
			return currentJobStatus == EntryStatusList.Codes.Cancelled;
		}

		public virtual bool IsClear(ZString currentJobStatus)
		{
			return currentJobStatus == EntryStatusList.Codes.Clear;
		}

		public virtual bool IsLodged(ZString currentJobStatus)
		{
			return !currentJobStatus.IsEmpty && !IsWithdrawn(currentJobStatus);
		}

		#endregion

		#region GetMessageStatus

		public ZString GetMessageAwaitingStatus(EDIMessage message)
		{
			return GetMessageAwaitingStatus(message.EM_MessageSubType);
		}

		public virtual ZString GetMessageAwaitingStatus(ZString messageSubType)
		{
			switch (messageSubType)
			{
				case MessageSubTypeCodes.Codes.Original:
					return MessageStatusList.Codes.AwaitingOriginal;
				case MessageSubTypeCodes.Codes.Change:
					return MessageStatusList.Codes.AwaitingChange;
				case MessageSubTypeCodes.Codes.Cancellation:
					return MessageStatusList.Codes.AwaitingDelete;
				default:
					return ZString.Empty;
			}
		}

		public ZString GetMessageAcknowledgedStatus(EDIMessage message)
		{
			return GetMessageAcknowledgedStatus(message.EM_MessageSubType);
		}

		public virtual ZString GetMessageAcknowledgedStatus(ZString messageSubType)
		{
			switch (messageSubType)
			{
				case MessageSubTypeCodes.Codes.Original:
					return MessageStatusList.Codes.AcknowledgedOriginal;
				case MessageSubTypeCodes.Codes.Change:
					return MessageStatusList.Codes.AcknowledgedChange;
				case MessageSubTypeCodes.Codes.Cancellation:
					return MessageStatusList.Codes.AcknowledgedDelete;
				default:
					return ZString.Empty;
			}
		}

		public ZString GetMessageRejectedStatus(EDIMessage message)
		{
			return GetMessageRejectedStatus(message.EM_MessageSubType);
		}

		public virtual ZString GetMessageRejectedStatus(ZString messageSubType)
		{
			switch (messageSubType)
			{
				case MessageSubTypeCodes.Codes.Original:
					return MessageStatusList.Codes.ErrorOriginal;
				case MessageSubTypeCodes.Codes.Change:
					return MessageStatusList.Codes.ErrorChange;
				case MessageSubTypeCodes.Codes.Cancellation:
					return MessageStatusList.Codes.ErrorDelete;
				default:
					return ZString.Empty;
			}
		}

		public ZString GetMessageClearedStatus(EDIMessage message)
		{
			return GetMessageClearedStatus(message.EM_MessageSubType);
		}

		public virtual ZString GetMessageClearedStatus(ZString messageSubType)
		{
			switch (messageSubType)
			{
				case MessageSubTypeCodes.Codes.Original:
					return MessageStatusList.Codes.ClearOriginal;
				case MessageSubTypeCodes.Codes.Change:
					return MessageStatusList.Codes.ClearChange;
				case MessageSubTypeCodes.Codes.Cancellation:
					return MessageStatusList.Codes.ClearDelete;
				default:
					return ZString.Empty;
			}
		}

		#endregion

		public abstract ZString MessageTypeDescription { get; }
		public abstract ZString CalculatedJobStatus(IEDIFACTMessageAttachee linkedObject);
	}
}

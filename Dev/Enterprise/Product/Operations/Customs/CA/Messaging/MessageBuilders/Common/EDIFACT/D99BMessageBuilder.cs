using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CA.Messaging
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class D99BMessageBuilder<TData, TSegmentGroup, TResult> : EDIFACTMessageBuilder<TData, TSegmentGroup, TResult>
		where TData : IEDIMessageCollectionProvider
		where TSegmentGroup : SegmentGroup
		where TResult : EDIMessage
	{
		protected D99BMessageBuilder(TData data, MessageSubTypes messageSubType)
			: base(data, messageSubType, new CACharSet())
		{
		}

		protected MessageFunctionCodeList MessageFunctionCode
		{
			get
			{
				MessageFunctionCodeList result = null;
				switch (messageSubType)
				{
					case MessageSubTypes.Create:
						result = MessageFunctionCodeList.Original;
						break;
					case MessageSubTypes.Withdraw:
						result = MessageFunctionCodeList.Cancellation;
						break;
					case MessageSubTypes.Change:
						result = MessageFunctionCodeList.Change;
						break;
					case MessageSubTypes.AddLines:
						result = MessageFunctionCodeList.Addition;
						break;
					case MessageSubTypes.Delete:
						result = MessageFunctionCodeList.Deletion;
						break;
				}
				return result;
			}
		}

		protected ActionRequestNotificationDescriptionCodeList ActionRequestNotificationDescriptionCode
		{
			get
			{
				ActionRequestNotificationDescriptionCodeList result = null;
				switch (messageSubType)
				{
					case MessageSubTypes.Amend:
					case MessageSubTypes.Change:
						result = ActionRequestNotificationDescriptionCodeList.Amendments;
						break;
					case MessageSubTypes.Create:
						result = ActionRequestNotificationDescriptionCodeList.NotAmended;
						break;
				}
				return result;
			}
		}
	}
}

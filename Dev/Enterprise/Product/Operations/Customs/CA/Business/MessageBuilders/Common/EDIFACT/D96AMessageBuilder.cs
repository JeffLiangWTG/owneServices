namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Edifact.Auto;
	using Enterprise.Edifact.D96A.Elements;
	using Enterprise.Messaging.Business;
	using EDIMessage = EDIMessage;

	public abstract class D96AMessageBuilder<TData, TSegmentGroup, TResult> : EDIFACTMessageBuilder<TData, TSegmentGroup, TResult>
		where TData : IEDIMessageCollectionProvider
		where TSegmentGroup : SegmentGroup
		where TResult : EDIMessage
	{
		protected D96AMessageBuilder(TData data, MessageSubTypes messageSubType)
			: base(data, messageSubType, new CACharSet())
		{
		}

		protected MessageFunctionCodedList MessageFunctionCode
		{
			get
			{
				MessageFunctionCodedList result = null;
				switch (messageSubType)
				{
					case MessageSubTypes.Create:
						result = MessageFunctionCodedList.Original;
						break;
					case MessageSubTypes.Withdraw:
						result = MessageFunctionCodedList.Cancellation;
						break;
					case MessageSubTypes.Change:
						result = MessageFunctionCodedList.Change;
						break;
				}
				return result;
			}
		}
	}
}

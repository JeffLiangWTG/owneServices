namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Edifact.Auto;
	using Enterprise.Edifact.D11B.Elements;
	using Enterprise.Messaging.Business;
	using EDIMessage = EDIMessage;

	public abstract class D11BMessageBuilder<TData, TSegmentGroup, TResult> : EDIFACTMessageBuilder<TData, TSegmentGroup, TResult>
		where TData : IEDIMessageCollectionProvider
		where TSegmentGroup : SegmentGroup
		where TResult : EDIMessage
	{
		protected D11BMessageBuilder(TData data, MessageSubTypes messageSubType)
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
					case MessageSubTypes.Request:
						result = MessageFunctionCodeList.ProposedAmendment;
						break;
				}
				return result;
			}
		}
	}
}

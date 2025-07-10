namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using Enterprise.Customs.CA.Messaging;
	using Enterprise.Customs.Common.MessageBuilders;
	using Enterprise.Edifact.Auto;
	using Enterprise.Edifact.D00A.Elements;
	using Enterprise.Messaging.Business;
	using EDIMessage = EDIMessage;

	public abstract class D00AMessageBuilder<TData, TSegmentGroup, TResult> : EDIFACTMessageBuilder<TData, TSegmentGroup, TResult>
		where TData : IEDIMessageCollectionProvider
		where TSegmentGroup : SegmentGroup
		where TResult : EDIMessage
	{
		protected D00AMessageBuilder(TData data, MessageSubTypes messageSubType, DocumentNameCodeList documentNameCode)
			: base(data, messageSubType, new CACharSet())
		{
			DocumentNameCode = documentNameCode;
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
				}
				return result;
			}
		}

		protected DocumentNameCodeList DocumentNameCode { get; private set; }
	}
}

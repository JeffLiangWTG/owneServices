using System;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Common.Shared;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.Common.MessageBuilders
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1005:AvoidExcessiveParametersOnGenericTypes")]
	public abstract class EDIFACTMessageBuilder<TData, TSegmentGroup, TResult> : IMessageBuilder
		where TData : IEDIMessageCollectionProvider
		where TSegmentGroup : SegmentGroup
		where TResult : EDIMessage
	{
		protected EDIFACTMessageBuilder(TData data, MessageSubTypes messageSubType, UNCharacterSet characterSet = null)
		{
			this.data = data;
			this.messageSubType = messageSubType;
			this.characterSet = characterSet ?? new UNOACharacterSet();
			edifactMessage = Activator.CreateInstance<TSegmentGroup>();
			interpretation = new MessageInterpretation(edifactMessage, characterSet);
		}

		public virtual IMessageBuilderResult PopulateMessages()
		{
			var messageBuilderResult = new MessageBuilderResult();
			var builderResult = new BuilderResult(null, Array.Empty<string>(), null);
			builderResult.Message = PopulateMessagesReturningResult();
			messageBuilderResult.AddBuilderResult(builderResult);
			return messageBuilderResult;
		}

		protected virtual TResult PopulateMessagesReturningResult()
		{
			PopulateEdifactMessage();
			var messageToSend = (TResult)data.Messages.AddNew(typeof(TResult));
			messageToSend.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			messageToSend.EM_MessageSubType = GetMessageSubType();
			messageToSend.EM_MessageText = edifactMessage.ToString(characterSet);
			messageToSend.EM_MessageInterpretation = interpretation.ToHtml();
			return messageToSend;
		}

		protected virtual ZString GetMessageSubType()
		{
			var result = ZString.Empty;
			switch (messageSubType)
			{
				case MessageSubTypes.Create:
					result = MessageSubTypeCodes.Codes.Original;
					break;
				case MessageSubTypes.Withdraw:
					result = MessageSubTypeCodes.Codes.Cancellation;
					break;
				case MessageSubTypes.Change:
					result = MessageSubTypeCodes.Codes.Change;
					break;
			}
			return result;
		}

		protected abstract void PopulateEdifactMessage();
		protected readonly TData data;
		protected TSegmentGroup edifactMessage;
		protected MessageInterpretation interpretation;
		protected readonly MessageSubTypes messageSubType;
		protected readonly UNCharacterSet characterSet;
	}
}

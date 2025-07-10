using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public abstract class TemporaryStorageMessageBuilder : IMessageBuilder, IDisposable
	{
		protected TemporaryStorageMessageBuilder(TemporaryStorageMessageSendingObject messageSendingObject, TemporaryStorageMessageFunction messageFunction)
		{
			MessageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
			MessageFunction = Argument.NotNull(messageFunction, nameof(messageFunction));
			Header = messageSendingObject.Header;
		}

		protected readonly TemporaryStorageHeader Header;
		protected readonly TemporaryStorageMessageFunction MessageFunction;
		protected readonly TemporaryStorageMessageSendingObject MessageSendingObject;

		public IMessageBuilderResult PopulateMessages()
		{
			var messageBuilderResult = new MessageBuilderResult();
			var builderResult = Generate();
			if (builderResult != null)
			{
				messageBuilderResult.AddBuilderResult(builderResult);
			}
			return messageBuilderResult;
		}

		IBuilderResult Generate()
		{
			var errorCollector = new ErrorCollector();
			var messageText = GetMessageText(errorCollector);
			var builderResult = new BuilderResult(Header, errorCollector.GetErrors(), AfterFullSuccess);
			var message = GetNewMessage();
			builderResult.Message = message;
			message.EM_ApplicationCode = GetApplicationCode();
			message.EM_MessageOwner = GetMessageOwner(Header);
			message.MessageNumberStrategy = GetMessageNumberStrategy(builderResult);
			message.EM_MessageText = MessageSendingObject.MessageCreated(messageText);
			message.EM_SendWithMessageErrors = Header.HasErrors || Header.HasMessageErrors;
			message.EM_MessageType = GetMessageType();
			message.EM_MessageSubType = GetMessageSubType();
			message.EM_ApplicationReference = ZString.Empty;
			message.EM_GP = GetGlbExternalPassword();
			message.EM_LinkedObject = Header;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.Saving += Message_Saving;
			Messages.Add(message);
			return builderResult;
		}
		List<EDIMessage> Messages => messages ?? (messages = new List<EDIMessage>());
		List<EDIMessage> messages;

		protected virtual EDIMessage GetNewMessage() => Header.Messages.AddNew();

		protected abstract ZString GetApplicationCode();

		protected abstract IMessageNumberStrategy GetMessageNumberStrategy(BuilderResult builderResult);

		protected abstract ZString GetMessageText(ErrorCollector errorCollector);

		protected virtual ZString GetMessageOwner(TemporaryStorageHeader header)
		{
			return header.Declarant?.Header?.GetEuIdentificationNumber() ?? ZString.Empty;
		}

		protected virtual ZString GetMessageType() => TemporaryStorageConstants.MessageTypeList.EDIMessageType;

		protected virtual ZString GetMessageSubType() => MessageFunction.MessageType;

		protected virtual ZGuid GetGlbExternalPassword() => ZGuid.Empty;

		protected virtual void Message_Saving(EDIMessage message)
		{
			message.EM_MessageText = message.EM_MessageText.Replace(XML_MESSAGEID_PLACEHOLDER, message.EM_MessageNum);
		}

		void AfterFullSuccess(IBuilderResult builderResult)
		{
			var header = (TemporaryStorageHeader)builderResult.Owner;
			if (!MessageFunction.SentCustomsStatus.IsEmpty)
			{
				header.CustomsStatus = MessageFunction.SentCustomsStatus;
			}
			if (!MessageFunction.SentMessageStatus.IsEmpty)
			{
				header.AMA_MessageStatus = MessageFunction.SentMessageStatus;
			}
		}

		protected const string XML_MESSAGEID_PLACEHOLDER = "{{XML_MESSAGEID_PLACEHOLDER}}";

		public void Dispose()
		{
			if (messages != null)
			{
				messages.ForEach(x => x.Saving -= Message_Saving);
				messages = null;
			}
		}
	}
}

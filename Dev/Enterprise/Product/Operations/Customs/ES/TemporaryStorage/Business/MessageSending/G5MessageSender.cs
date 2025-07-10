using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using static Enterprise.Customs.ES.TemporaryStorage.Business.G5MessageSender;

namespace Enterprise.Customs.ES.TemporaryStorage.Business;

public class G5MessageSender : GenericMessageSender<MessageBuilderData, G5TemporaryStorageMessageSendingObject>
{
	public G5MessageSender(G5TemporaryStorageMessageSendingObjectParent sendingObjectParent)
	{
		this.sendingObjectParent = Argument.NotNull(sendingObjectParent, nameof(sendingObjectParent));
	}
	readonly G5TemporaryStorageMessageSendingObjectParent sendingObjectParent;

	public override List<MessageBuilderData> GetMessageBuildersData()
	{
		var messageBuilders = new List<MessageBuilderData>();
		var certificateData = sendingObjectParent.CertificateData;
		foreach (var objectToSend in sendingObjectParent.SelectedSendingObjects.Cast<G5TemporaryStorageMessageSendingObject>())
		{
			messageBuilders.AddRange(GetIndividualMessageBuilder(objectToSend, certificateData));
		}

		return messageBuilders;
	}

	protected override List<MessageBuilderData> GetIndividualMessageBuilder(G5TemporaryStorageMessageSendingObject objectToSend, ICertificateProvider certificateData)
	{
		Argument.NotNull(objectToSend, "objectToSend cannot be null");

		var messageBuilders = new List<MessageBuilderData>();
		AddSpecificMessageBuilders(messageBuilders, objectToSend, certificateData);

		return messageBuilders;
	}

	void AddSpecificMessageBuilders(List<MessageBuilderData> messageBuilders, G5TemporaryStorageMessageSendingObject objectToSend, ICertificateProvider certificateData)
	{
		try
		{
			var builderManager = new G5MessageBuilderManager(objectToSend, certificateData);

			messageBuilders.Add(new MessageBuilderData
			{
				MessageBuilder = builderManager.NewMessageBuilder(),
				Header = objectToSend.Header
			});
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			ErrorReporter.ReportOnce("G5MessageSender.GetIndividualMessageBuilder", "Exception thrown when trying to create builder to send declaration", ex);
			messageBuilders.Add(new MessageBuilderData
			{
				FailureFlag = true
			});
		}
	}

	public static MessagesInfo Send(List<MessageBuilderData> messageBuildersToSend, List<ESEDIMessage> messages = null)
	{
		var messagesInfo = new MessagesInfo
		{
			MessagesSent = ZInt.Zero,
			MessagesWithCreateFailure = ZInt.Zero,
			MessagesWithSendFailure = ZInt.Zero
		};

		foreach (var objectToSend in messageBuildersToSend)
		{
			SendIndividualDeclaration(objectToSend, messagesInfo, messages);
		}

		return messagesInfo;
	}

	static void SendIndividualDeclaration(MessageBuilderData messageBuilderToSend, MessagesInfo messagesInfo, List<ESEDIMessage> messages = null)
	{
		ESEDIMessage message = null;

		if (!messageBuilderToSend.FailureFlag)
		{
			var initialMessagesSent = messagesInfo.MessagesSent;
			var header = messageBuilderToSend.Header;
			var previousMessageStatus = header?.AMA_MessageStatus ?? ZString.Empty;

			try
			{
				var messageBuilder = Argument.NotNull(messageBuilderToSend.MessageBuilder, "messageBuilderToSend cannot be null");

				message = TrySendDeclaration(messageBuilderToSend);
				messagesInfo.MessagesSent++;

				if (messages != null)
				{
					messages.Add(message);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				messagesInfo.MessagesWithSendFailure++;
				ErrorReporter.ReportOnce("G5MessageSender.Send", "Exception thrown when trying to send declaration", ex);
			}
			finally
			{
				CleanUpDatabaseAfterFailure(message, header, previousMessageStatus, initialMessagesSent, messagesInfo);
			}
		}
		else
		{
			messagesInfo.MessagesWithSendFailure++;
		}
	}

	static ESEDIMessage TrySendDeclaration(MessageBuilderData objectToSend)
	{
		var messageBuilderToSend = Argument.NotNull(objectToSend.MessageBuilder, "messageBuilderToSend cannot be null");
		var header = Argument.NotNull(objectToSend.Header, "entryHeader cannot be null");

		var messageCreated = SendMessageBuilder(header, messageBuilderToSend);

		header.AMA_MessageStatus = PNTSMessageStatusList.Codes.Sent;

		return messageCreated;
	}

	static ESEDIMessage SendMessageBuilder(TemporaryStorageHeader header, IMessageBuilderBase messageBuilder)
	{
		var factory = header.Factory;

		var messageCreator = new EDIMessageCreator(messageBuilder, factory);
		var message = messageCreator.CreateMessage();
		message.EM_LinkedObject = header;

		return message;
	}

	static void CleanUpDatabaseAfterFailure(ESEDIMessage message, TemporaryStorageHeader header, ZString previousMessageStatus, ZInt initialMessagesSent, MessagesInfo messagesInfo)
	{
		if (messagesInfo.MessagesSent == initialMessagesSent && message == null && header != null)
		{
			header.AMA_MessageStatus = previousMessageStatus;
		}
	}

	public class MessageBuilderData
	{
		public IMessageBuilderBase MessageBuilder;
		public TemporaryStorageHeader Header;
		public ZBool FailureFlag;
	}

	public class MessagesInfo
	{
		public ZInt MessagesSent;
		public ZInt MessagesWithCreateFailure;
		public ZInt MessagesWithSendFailure;
	}
}

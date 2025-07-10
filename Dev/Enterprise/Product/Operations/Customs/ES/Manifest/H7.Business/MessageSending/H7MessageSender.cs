using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Business.MessageSending;
using Enterprise.Customs.ES.Manifest.H7.Business.BusinessObjects.Interfaces;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class H7MessageSender : GenericMessageSender<MessageBuilderData, H7MessageSendingObject>
	{
		public H7MessageSender(IEnumerable<IH7CommonMessageSendingObject> sendingObjects)
		{
			this.sendingObjects = Argument.NotNull(sendingObjects, nameof(sendingObjects));
		}

		public H7MessageSender(IEnumerable<IH7CommonMessageSendingObject> sendingObjects, Action<int, int> updateProgressCallback)
			: this(sendingObjects)
		{
			this.updateProgressCallback = updateProgressCallback;
		}

		readonly IEnumerable<IH7CommonMessageSendingObject> sendingObjects;
		readonly Action<int, int> updateProgressCallback;

		ICertificateProvider CertificateData
		{
			get
			{
				if (certificateData == null)
				{
					var header = sendingObjects.FirstOrDefault()?.Bill?.Header;
					certificateData = header.Certificate;
				}
				return certificateData;
			}
		}
		ICertificateProvider certificateData;

		public override List<MessageBuilderData> GetMessageBuildersData()
		{
			var messageBuilders = new List<MessageBuilderData>();
			foreach (var objectToSend in sendingObjects)
			{
				Argument.NotNull(objectToSend, "objectToSend cannot be null");
				if (objectToSend.Action == DeclarationMessageTypeList.Codes.H7Annexes)
				{
					AddCommonAnnexMessageBuilders(messageBuilders, objectToSend, CertificateData);
				}
				else
				{
					AddSpecificMessageBuilders(messageBuilders, (H7MessageSendingObject)objectToSend, CertificateData);
				}
			}

			return messageBuilders;
		}

		void AddSpecificMessageBuilders(List<MessageBuilderData> messageBuilders, H7MessageSendingObject objectToSend, ICertificateProvider certificateData)
		{
			try
			{
				var bill = objectToSend.Bill;
				var builderManager = new H7MessageBuilderManager(objectToSend, certificateData);

				messageBuilders.Add(new MessageBuilderData
				{
					MessageBuilder = builderManager.NewMessageBuilder(),
					Bill = bill,
					SendingObject = objectToSend
				});
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("H7MessageSender.GetIndividualMessageBuilder", "Exception thrown when trying to create builder to send declaration", ex);
				messageBuilders.Add(new MessageBuilderData
				{
					FailureFlag = true
				});
			}
		}

		void AddCommonAnnexMessageBuilders(List<MessageBuilderData> messageBuilders, IH7CommonMessageSendingObject objectToSend, ICertificateProvider certificateData)
		{
			try
			{
				var bill = objectToSend.Bill;
				var builderManager = new H7MessageBuilderManager(objectToSend, certificateData);
				messageBuilders.AddRange(GetCommonAnnexMessageBuilders(bill, (UploadDocumentsSendingAction)objectToSend, builderManager));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("H7MessageSender.GetCommonAnnexMessageBuilder", "Exception thrown when trying to create builder to send declaration", ex);
				messageBuilders.Add(new MessageBuilderData
				{
					FailureFlag = true
				});
			}
		}

		public MessagesInfo Send(List<MessageBuilderData> messageBuildersToSend, List<ESEDIMessage> messages = null)
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
				updateProgressCallback?.Invoke(messagesInfo.MessagesSent, messageBuildersToSend.Count);
			}

			return messagesInfo;
		}

		void SendIndividualDeclaration(MessageBuilderData messageBuilderToSend, MessagesInfo messagesInfo, List<ESEDIMessage> messages = null)
		{
			ESEDIMessage message = null;

			if (!messageBuilderToSend.FailureFlag)
			{
				var initialMessagesSent = messagesInfo.MessagesSent;
				var bill = messageBuilderToSend.Bill;
				var previousMessageStatus = bill?.ABL_MessageStatus ?? ZString.Empty;

				try
				{
					var messageBuilder = Argument.NotNull(messageBuilderToSend.MessageBuilder, "messageBuilderToSend cannot be null");

					message = TrySendDeclaration(messageBuilderToSend);
					messagesInfo.MessagesSent++;
					bill.Messages.Reload(false);

					if (messages != null)
					{
						messages.Add(message);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					messagesInfo.MessagesWithSendFailure++;
					ErrorReporter.ReportOnce("H7MessageSender.Send", "Exception thrown when trying to send declaration", ex);
				}
				finally
				{
					CleanUpDatabaseAfterFailure(message, bill, previousMessageStatus, initialMessagesSent, messagesInfo);
				}
			}
			else
			{
				messagesInfo.MessagesWithSendFailure++;
			}
		}

		ESEDIMessage TrySendDeclaration(MessageBuilderData objectToSend)
		{
			var messageBuilderToSend = Argument.NotNull(objectToSend.MessageBuilder, "messageBuilderToSend cannot be null");
			var bill = Argument.NotNull(objectToSend.Bill, "bill cannot be null");

			var messageCreated = SendMessageBuilder(bill, messageBuilderToSend);
			messageCreated.EM_MessageText = objectToSend.SendingObject.MessageCreated(messageCreated.EM_MessageText);

			bill.ABL_MessageStatus = LogicalStatusList.Codes.Sent;

			return messageCreated;
		}

		ESEDIMessage SendMessageBuilder(AsycudaBill bill, IMessageBuilderBase messageBuilder)
		{
			var factory = bill.Factory;
			var messageCreator = new EDIMessageCreator(messageBuilder, factory);
			var message = messageCreator.CreateMessage();
			message.EM_LinkedObject = bill;
			return message;
		}

		void CleanUpDatabaseAfterFailure(ESEDIMessage message, AsycudaBill bill, ZString previousMessageStatus, ZInt initialMessagesSent, MessagesInfo messagesInfo)
		{
			if (messagesInfo.MessagesSent == initialMessagesSent && message == null && bill != null)
			{
				bill.ABL_MessageStatus = previousMessageStatus;
			}
		}

		public List<MessageBuilderData> GetCommonAnnexMessageBuilders(AsycudaBill bill, UploadDocumentsSendingAction objectToSend, H7MessageBuilderManager builderManager)
		{
			var messageBuilders = new List<MessageBuilderData>();

			foreach (var addInfoObject in (objectToSend.AddInfoCollection.Cast<AdditionalInfoSendingObject>()))
			{
				foreach (var suppDocObj in addInfoObject.EDocsCollection.Cast<DocumentSendingObject>())
				{
					messageBuilders.Add(new MessageBuilderData
					{
						MessageBuilder = builderManager.NewCommonAnnexMessageBuilder(suppDocObj.Document, addInfoObject),
						Bill = bill,
						SendingObject = objectToSend,
					});
				}
			}
			return messageBuilders;
		}

		protected override List<MessageBuilderData> GetIndividualMessageBuilder(H7MessageSendingObject objectToSend, ICertificateProvider certificateData)
		{
			throw new NotImplementedException();
		}
	}

	public class MessageBuilderData
	{
		public IMessageBuilderBase MessageBuilder;
		public BaseMessageSendingObject SendingObject;
		public AsycudaBill Bill;
		public ZBool FailureFlag;
	}

	public class MessagesInfo
	{
		public ZInt MessagesSent;
		public ZInt MessagesWithCreateFailure;
		public ZInt MessagesWithSendFailure;
	}
}

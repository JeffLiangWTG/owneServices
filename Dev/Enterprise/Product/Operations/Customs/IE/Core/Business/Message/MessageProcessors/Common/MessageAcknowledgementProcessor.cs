using System;
using System.Collections;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Messaging;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.Abstractions;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

namespace Enterprise.Customs.IE.Business
{
	public class MessageAcknowledgementProcessor : SendingNotificationCommonProcessor
	{
		public MessageAcknowledgementProcessor(LoggingInformation logger) : base(logger) { }

		protected override string MessageFriendlyNameCore => Res.GetString("0821BF54-8FF3-4638-96EA-26CE6484AE2B", "Message Acknowledgement");

		protected override string ApplicationCodeCore => throw new InvalidOperationException("ApplicationCode should not be used. Should override MessageFilterCore.");

		protected override void PreProcessMessageCore(BaseEDIMessage targetMessage)
		{
			base.PreProcessMessageCore(targetMessage);
			(_, var originalMessage) = GetMappedData(targetMessage);
			if (originalMessage != null)
			{
				MessageCreator.SetPreProcessData(targetMessage, originalMessage);
			}
			else
			{
				Logger.LogError(Res.GetString("D9BEBFE2-2AB6-4448-B9D5-03EB88989E57", "Unable to find a related outgoing message for the Message ({0})", targetMessage.EM_MessageNum));
				targetMessage.EM_Status = EDIMessage.Status.Error;
			}
		}

		protected (ITransaction transactionProvider, BaseEDIMessage originalMessage) GetMappedData(BaseEDIMessage targetMessage)
		{
			dataMapping = dataMapping ?? new Dictionary<BaseEDIMessage, (ITransaction transactionProvider, BaseEDIMessage originalMessage)>();
			if (!dataMapping.TryGetValue(targetMessage, out var result))
			{
				var transactionProvider = GetTransactionProvider(targetMessage);
				var transactionId = (ZString)transactionProvider.TransactionId;
				var originalMessage = transactionId.IsEmpty ?
					InboundEDIMessage.GetOriginalMessageWithoutTID(targetMessage.Factory, targetMessage.EM_ApplicationCode, targetMessage.EM_LinkUniqueID, targetMessage.EM_SystemCreateTimeUtc) :
					InboundEDIMessage.GetOriginalMessage(targetMessage.Factory, targetMessage.EM_ApplicationCode, transactionId);
				result = (transactionProvider, originalMessage);
				dataMapping.Add(targetMessage, result);
			}

			return result;
		}
		Dictionary<BaseEDIMessage, (ITransaction transactionProvider, BaseEDIMessage originalMessage)> dataMapping;

		protected override void ProcessMessageCore(BaseEDIMessage targetMessage)
		{
			var (provider, originalMessage) = GetMappedData(targetMessage);
			if (provider != null)
			{
				if (string.IsNullOrEmpty(provider.MessageStatus) && !string.IsNullOrEmpty(provider.ErrorCode))
				{
					SendFailureNotification<MessageAcknowledgementServiceErrorInterpreter>(originalMessage, provider, targetMessage);
					SetJobFailed(originalMessage);
					targetMessage.EM_Status = EDIMessage.Status.Received;
				}
				else
				{
					var messageStatus = provider.MessageStatus.ToUpper();
					switch (messageStatus)
					{
						case MessageAcknowledgementStatusList.Codes.Accepted:
							ObjectFactory.Get<IServiceTaskNudger>().NudgeServiceTask(ServiceTaskApplicationCodeList.Codes.IEMessageRetriever);
							targetMessage.EM_Status = EDIMessage.Status.Received;
							originalMessage.EM_Status = EDIMessage.Status.Acknowledged;
							GetAdditionalAction(targetMessage.EM_ApplicationCode, targetMessage.EM_MessageType, targetMessage.EM_MessageSubType)?.Process(originalMessage, targetMessage, provider);
							break;
						case MessageAcknowledgementStatusList.Codes.Rejected:
							HandleRejection(targetMessage, provider, originalMessage);
							targetMessage.EM_Status = EDIMessage.Status.Received;
							break;
						default:
							Logger.LogError(Res.GetString("186F9940-070E-4333-B729-D881B2C390F4", "Message ({0}) has unrecognized Message Status ({1}).", targetMessage.EM_MessageNum, messageStatus));
							targetMessage.EM_Status = EDIMessage.Status.Error;
							break;
					}
				}
			}
		}

		IAdditionalMessageProcessing GetAdditionalAction(string applicationCode, string messageType, string messageSubType)
		{
			var hashTable = (Hashtable)ObjectFactory.Get("IEAdditionalMessageProcessings");
			var objectHandle = hashTable[string.Join("|", applicationCode, messageType, messageSubType)] as ObjectHandle;
			var obj = objectHandle?.GetObject() as IAdditionalMessageProcessing;
			return obj;
		}

		ITransaction GetTransactionProvider(BaseEDIMessage targetMessage)
		{
			using (var messageTextReader = targetMessage.GetEM_MessageTextReader())
			{
				return IEXmlObjectSerializer.Deserialize<MessageAcknowledgement>(messageTextReader, withXSDValidation: false);
			}
		}

		#region Rejections Handling

		void HandleRejection(BaseEDIMessage incomingMessage, ITransaction transaction, BaseEDIMessage originalMessage)
		{
			var transactionIdStatus = transaction.TransactionIdStatus.ToUpper();
			switch (transactionIdStatus)
			{
				case TransactionIdStatusList.Codes.Error:
					SendFailureNotification<MessageAcknowledgementInterpreter>(originalMessage, transaction);
					SetJobFailed(originalMessage);
					break;
				case TransactionIdStatusList.Codes.Invalid:
				case TransactionIdStatusList.Codes.Expired:
					ArrangeResubmissionOrHandleFailure(originalMessage, incomingMessage, transaction);
					break;
				default:
					Logger.LogError(Res.GetString("B79C12B5-3E17-4595-87FC-7BA0D9299AB2", "Message ({0}) has unrecognized Transaction ID Status ({1}).", incomingMessage.EM_MessageNum, transactionIdStatus));
					break;
			}
		}

		void ArrangeResubmissionOrHandleFailure(BaseEDIMessage originalMessage, BaseEDIMessage incomingMessage, ITransaction status)
		{
			var resubmissionCount = GetResubmissionCount(originalMessage);
			if (resubmissionCount < MaxReSubmissionTime)
			{
				CreateReSubmissionMessage(originalMessage, resubmissionCount + 1, status.TransactionIdStatus);
			}
			else
			{
				SendFailureNotification<MessageAcknowledgementInterpreter>(originalMessage, status);
				SetJobFailed(originalMessage);
			}
		}

		protected virtual void SetJobFailed(BaseEDIMessage originalMessage)
		{
			if (originalMessage.EM_LinkedObject is IMessageAttachee messageAttachee)
			{
				messageAttachee.LogicalStatus = LogicalStatusList.Codes.Failed;
			}
		}

		#region Re-Submissions

		int GetResubmissionCount(BaseEDIMessage originalMessage)
		{
			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, originalMessage.EM_ApplicationCode)
				.AddToFilter(EDIMessageSchema.EM_ApplicationReference, SQLComparisonOperator.Like, originalMessage.EM_ApplicationReference + TransactionSerieSeparator + "%")
				.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);

			return originalMessage.Factory.GetDatabaseCount(typeof(BaseEDIMessage), query);
		}

		const string TransactionSerieSeparator = "_";

		const short MaxReSubmissionTime = 3;

		BaseEDIMessage CreateReSubmissionMessage(BaseEDIMessage originalMessage, int reSubmissionTime, string transactionIdStatus)
		{
			Logger.Log(Res.GetString("DC7716A4-EBCF-4D07-A7F1-F223C230E8BD", "Submission rejected, Number: {0}, Status: {1}, creating a re-submission.", originalMessage.EM_MessageNum, transactionIdStatus));
			var result = (BaseEDIMessage)originalMessage.Clone();
			result.EM_ApplicationReference = ZString.Empty;
			result.EM_MessageNum = originalMessage.EM_MessageNum + TransactionSerieSeparator + reSubmissionTime;
			return result;
		}

		#endregion

		#region eMail Notifications for Failure

		protected virtual void SendFailureNotification<T>(BaseEDIMessage originalMessage, ITransaction transaction, BaseEDIMessage targetMessage = null) where T : InboundMessageInterpreter<ITransaction>
		{
			if (originalMessage.EM_LinkedObject is IMessageAttachee messageAttachee)
			{
				var interpretation = ((T)Activator.CreateInstance(typeof(T), originalMessage, transaction)).GetInterpretation();
				if (targetMessage is not null)
				{
					targetMessage.EM_MessageInterpretation = interpretation;
				}

				if (messageAttachee is not IAISMessageAttacheeWithEmailLogic ||
					(messageAttachee is IAISMessageAttacheeWithEmailLogic messageAttacheeWithEmailLogic && messageAttacheeWithEmailLogic.ShouldSendEmailNotification(targetMessage)))
				{
					using (SetApplicationCodeForEmail(originalMessage.EM_ApplicationCode))
					{
						GenerateHtmlEmailAndSendToOriginalOrGroup(
							factory: originalMessage.Factory,
							relatedJob: messageAttachee.RelatedJob,
							messageTypeInSubject: originalMessage.MessageTypeWithDescription,
							body: interpretation,
							isFailure: true,
							branchForEmailLogo: messageAttachee.Branch,
							sourceBusinessObject: (BusinessObject)messageAttachee,
							getEmailAddressToSendTo: () => GetEmailAddressToSendToFromQueuedUser(originalMessage)
						);
					}
				}
			}
		}

		#endregion

		#endregion
	}
}

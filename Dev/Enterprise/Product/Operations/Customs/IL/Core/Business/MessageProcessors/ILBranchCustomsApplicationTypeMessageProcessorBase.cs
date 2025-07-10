using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.IL.Business.MessageProcessors
{
	public abstract class ILBranchCustomsApplicationTypeMessageProcessorBase : BranchCustomsApplicationTypeMessageProcessor, IMessageProcessor
	{
		protected ILBranchCustomsApplicationTypeMessageProcessorBase(LoggingInformation logger) : base(logger)
		{
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			SendNotificationEmailIfNeeded((ILEDIMessage)message);
		}

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.ILCustoms;

		protected virtual bool ShouldSendNotificationEmail() => false;

		protected virtual string GetJobNumber(IEDIMessageCollectionOwner owner) => string.Empty;

		protected virtual ZBool GetShouldSendErrorEmailsOnly(IEDIMessageCollectionOwner owner, ILEDIMessage message) => false;

		protected virtual ControllerID ControllerIDForEmail => null;

		protected virtual string MessageTypeInSubject => string.Empty;

		protected void SendNotificationEmailIfNeeded(ILEDIMessage message)
		{
			if (!ShouldSendNotificationEmail() || message.EM_LinkedObject is not IEDIMessageCollectionOwner owner)
			{
				return;
			}

			bool isSuccess = message.EM_Status == EDIMessage.Status.ProcessedOK;
			var shouldSendErrorEmailsOnly = GetShouldSendErrorEmailsOnly(owner, message);
			var url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDForEmail, owner.MessageOwner.PK.ToGuid());

			if (!shouldSendErrorEmailsOnly || (shouldSendErrorEmailsOnly && !isSuccess))
			{
				GenerateHtmlEmailAndSendToOriginalOrGroup(
				factory: message.Factory,
					uri: url,
					jobNumber: GetJobNumber(owner),
					messageTypeInSubject: MessageTypeInSubject,
					body: message.EM_MessageInterpretation,
					isFailure: !isSuccess,
					branchForEmailLogo: message.Branch,
					sourceBusinessObject: owner.MessageOwner,
					getEmailAddressToSendTo: () => GetEmailAddressToSendTo(owner, message));
			}
		}

		protected void UpdateMessageStatus(ILEDIMessage ilEDIMessage, string messageStatus, string noteText = null)
		{
			ilEDIMessage.EM_Status = messageStatus;
			if (!noteText.IsNullOrEmpty())
			{
				var note = ilEDIMessage.Notes.AddNew();
				note.ST_NoteText = noteText;
			}
		}

		ZString GetEmailAddressToSendTo(IEDIMessageCollectionOwner owner, EDIMessage baseMessage)
		{
			var result = ZString.Empty;
			if (owner != null)
			{
				var originalMessage = GetOriginalMessage(owner, baseMessage);
				var originalSender = (IUser)originalMessage?.UserWhoQueuedThisRecord;

				if (originalSender == null || originalSender.IsBatchProcessor)
				{
					originalMessage = GetLastTransmitMessage(owner);
					originalSender = originalMessage?.UserWhoQueuedThisRecord;
				}
				result = originalSender?.EmailAddress ?? string.Empty;
			}
			return result;
		}

		EDIMessage GetOriginalMessage(IEDIMessageCollectionOwner owner, EDIMessage baseMessage)
		{
			EDIMessage result = null;
			if (owner != null)
			{
				if (originalMessageCached == null || originalMessageCached.EM_LinkUniqueID != owner.MessageOwner.PK)
				{
					var messageNumber = baseMessage.EM_MessageNum;
					if (!messageNumber.IsEmpty)
					{
						var originalMessageType = OriginalMessageType.IsEmpty ? baseMessage.EM_MessageType : OriginalMessageType;
						originalMessageCached = owner.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageNum == messageNumber && x.PK != baseMessage.PK && x.EM_MessageType == originalMessageType);
					}
				}

				var interchangeSessionGuid = baseMessage.Interchange?.EI_SessionGUID ?? ZGuid.Empty;
				if (originalMessageCached == null && !interchangeSessionGuid.IsEmpty)
				{
					originalMessageCached = owner.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.IsTransmitMessage && x.Interchange != null && x.Interchange.EI_SessionGUID == interchangeSessionGuid);
				}
				result = originalMessageCached;
			}
			return result;
		}

		EDIMessage originalMessageCached;

		EDIMessage GetLastTransmitMessage(IEDIMessageCollectionOwner owner)
		{
			EDIMessage result = null;
			if (owner != null)
			{
				if (lastTransmitMessageCached == null || lastTransmitMessageCached.EM_LinkUniqueID != owner.MessageOwner.PK)
				{
					lastTransmitMessageCached
						= owner.Messages.Cast<EDIMessage>()
							.Where(x => x.IsTransmitMessage && x.EM_SystemCreateUser != User.ServiceUserCode && (LastTransmitMessageType.IsEmpty || x.EM_MessageType == LastTransmitMessageType))
							.OrderByDescending(x => x.EM_SystemCreateTimeUtc)
							.FirstOrDefault();
				}
				result = lastTransmitMessageCached;
			}
			return result;
		}

		protected (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObject(EDIMessage message)
		{
			var linkedObject = message.EM_LinkedObject;
			if (linkedObject != null)
			{
				return (message.EM_GB, linkedObject, (NoResString)ZString.Empty);
			}
			else
			{
				return TryFindLinkedObjectCore(message);
			}
		}

		protected abstract (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObjectCore(EDIMessage message);

		public virtual ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
		{
			var linkedObjectResult = TryFindLinkedObject(message);
			if (linkedObjectResult.LinkedObject is { } linkedObject)
			{
				return ProcessingResult.New(new LinkedBusinessObjectMetaData(linkedObject.TableName, linkedObject.PK, linkedObjectResult.BranchPK, ZString.Empty), linkedObjectResult.DiscardReason);
			}
			return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, linkedObjectResult.DiscardReason);
		}

		public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
			=> linkedBusinessObjectBranchPk.IsEmpty ? message.Branch.PK : linkedBusinessObjectBranchPk;

		public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			var keys = new HashSet<string>();

			var (_, linkedObject, _) = TryFindLinkedObject(message);
			switch (linkedObject)
			{
				case AsycudaManifestHeader manifestHeader:
					keys.Add(manifestHeader.AMA_JobReference);
					break;
				case CusEntryHeader entryheader:
					{
						keys.Add(entryheader.DeclarationReference);
						var lrn = entryheader.CH_BGMReference;
						if (!lrn.IsEmpty)
						{
							keys.Add(lrn);
						}

						break;
					}
				case ForwardingShipment shipment:
					keys.Add(shipment.JS_UniqueConsignRef);
					break;
				case ForwardingConsol consol:
					keys.Add(consol.JK_UniqueConsignRef);
					break;
				default:
					return SerializationKeysResult.SerialProcessingInReceivedOrder;
			}

			return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, keys);
		}

		EDIMessage lastTransmitMessageCached;

		ZString OriginalMessageType => ZString.Empty;

		ZString LastTransmitMessageType => ZString.Empty;
	}
}

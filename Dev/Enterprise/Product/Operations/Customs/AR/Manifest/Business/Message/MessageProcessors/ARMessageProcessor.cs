using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.Business
{
	public abstract class ARMessageProcessor<T> : BranchCustomsApplicationTypeMessageProcessor
		where T : EDIMessage
	{
		public ARMessageProcessor(LoggingInformation logger)
			: base(logger)
		{ }

		protected ZString errorMessage = Res.GetString("8B6BA401-63BB-4506-A6A3-F44DC90F85ED", "Error");
		protected HtmlTableCreator tableCreator;
		protected ZBool isFailure = ZBool.True;

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.ARCustoms;

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem() => ARCustomsDataRegistry.Instance.ARMANGroupNotification;

		protected override void PreProcessMessageCore(EDIMessage message)
		{
			base.PreProcessMessageCore(message);

			var bodyText = message.EM_MessageText;
			if (bodyText.IsEmpty || !XmlUtils.IsValidXml(bodyText))
			{
				Logger.LogWarning(Res.GetString("43AE15E0-35D4-4E71-8D75-3A8E4A5ACEBB", "Message {0} failed to parse the message as {1} message.", message.EM_MessageNum, message.EM_MessageType));
				SetMessageStatusAsDiscarded(message);
			}
			else
			{
				var messageAttachee = FindRelevantBusinessObject(message as T);
				if (messageAttachee != null)
				{
					message.EM_GB = messageAttachee.GlobalBranchPK;
					message.EM_LinkTable = messageAttachee.TableName;
					message.EM_LinkUniqueID = messageAttachee.PK;
					message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
				}
				else
				{
					SetMessageStatusAsFailed(message);
					Logger.LogWarning(Res.GetString("F494C737-F570-4B3F-B025-B5FE9B48C14F", "Unable to find business object for message {0}", GetEDIMessageStatusLogDescription(message)));
				}
			}
		}

		protected virtual IMessageAttachee FindRelevantBusinessObject(T message) => null;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			if (message.EM_Status != EDIMessageStatusList.Codes.Discarded)
			{
				message.EM_Status = ProcessMessageCore(message as T);
			}
			SendNotificationEmailIfNeeded(message);
		}

		protected abstract ZString ProcessMessageCore(T message);

		protected void SetMessageStatusAsDiscarded(EDIMessage message) => message.EM_Status = EDIMessageStatusList.Codes.Discarded;

		protected void SetMessageStatusAsFailed(EDIMessage message) => message.EM_Status = EDIMessageStatusList.Codes.Failed;

		protected ZString GetEDIMessageStatusLogDescription(EDIMessage message) => Res.GetString("98010801-D6FF-4A21-B033-1774150C8A67", "(Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to {4}.", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference, new EDIMessageStatusList().GetDescriptionFromCode(message.EM_Status));

		#region Send Notification Email

		void SendNotificationEmailIfNeeded(EDIMessage message)
		{
			var messageAttachee = message.EM_LinkedObject as IMessageAttachee;

			if (messageAttachee != null)
			{
				var branchPK = messageAttachee.GlobalBranchPK;
				if (branchPK.IsValid)
				{
					messageBranch = message.Branch;
				}
				else
				{
					messageBranch = MasterFiles.Business.GlbBranch.CurrentBranch;
				}

				var emailGroupRegistryItem = GetEmailGroupRegistryItem();
				var supportMessageSuppressRegistry = emailGroupRegistryItem as ISupportMessageSuppressRegistry;
				var shouldSendErrorEmailsOnly = supportMessageSuppressRegistry?.ShouldSEndErrorsOnly(MessageBranch.GB_GC, MessageBranch.PK, ZGuid.Empty) ?? false;

				if (!shouldSendErrorEmailsOnly || (shouldSendErrorEmailsOnly && isFailure))
				{
					var url = GetShowEditFormUrlCreator(messageAttachee);
					GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory, url, messageAttachee.JobReference, GetMailSubject(),
						GetEmailBody(messageAttachee),
						isFailure, message.Branch, messageAttachee as BusinessObject,
						() => GetEmailAddressToSendTo(messageAttachee, message));
				}
			}
		}

		protected abstract ZString GetShowEditFormUrlCreator(IMessageAttachee messageAttachee);
		protected abstract ZString GetMailSubject();
		protected abstract ZString GetFailureTitle(IMessageAttachee messageAttachee);
		protected abstract ZString GetSuccessfulTitle(IMessageAttachee messageAttachee);

		ZString GetEmailBody(IMessageAttachee messageAttachee)
		{
			var htmlBody = new StringBuilder();

			if (isFailure)
			{
				htmlBody.Append(GetFailureTitle(messageAttachee));
			}
			else
			{
				htmlBody.Append(GetSuccessfulTitle(messageAttachee));
			}
			htmlBody.Append("<br /><br />");
			htmlBody.Append(tableCreator.ToHtml());

			return htmlBody.ToString();
		}

		ZString GetEmailAddressToSendTo(IMessageAttachee messageAttachee, EDIMessage message)
		{
			var result = ZString.Empty;
			if (messageAttachee != null)
			{
				var originalMessage = GetOriginalMessage(messageAttachee, message);
				var originalSender = (IUser)originalMessage?.UserWhoQueuedThisRecord;

				if (originalSender == null || originalSender.IsBatchProcessor)
				{
					originalMessage = GetLastTransmitMessage(messageAttachee);
					originalSender = originalMessage?.UserWhoQueuedThisRecord;
				}
				result = originalSender?.EmailAddress ?? string.Empty;
			}
			return result;
		}

		EDIMessage GetOriginalMessage(IMessageAttachee messageAttachee, EDIMessage message)
		{
			EDIMessage result = null;
			if (messageAttachee != null)
			{
				if (originalMessageCached == null || originalMessageCached.EM_LinkUniqueID != messageAttachee.PK)
				{
					var messageNumber = message.EM_MessageNum;
					if (!messageNumber.IsEmpty)
					{
						originalMessageCached = messageAttachee.Messages.Cast<EDIMessage>().FirstOrDefault(x => x.EM_MessageNum == messageNumber && x.PK != message.PK && x.EM_MessageType == message.EM_MessageType);
					}
				}
				result = originalMessageCached;
			}
			return result;
		}
		EDIMessage originalMessageCached;

		EDIMessage GetLastTransmitMessage(IMessageAttachee messageAttachee)
		{
			EDIMessage result = null;
			if (messageAttachee != null)
			{
				if (lastTransmitMessageCached == null || lastTransmitMessageCached.EM_LinkUniqueID != messageAttachee.PK)
				{
					lastTransmitMessageCached = messageAttachee.Messages.OfType<EDIMessage>().Where(x => x.IsTransmitMessage && x.EM_SystemCreateUser != User.ServiceUserCode)
						.OrderByDescending(x => x.EM_SystemCreateTimeUtc).FirstOrDefault();
				}
				result = lastTransmitMessageCached;
			}
			return result;
		}
		EDIMessage lastTransmitMessageCached;

		IGlbBranch MessageBranch
		{
			get { return messageBranch; }
			set { messageBranch = value; }
		}
		IGlbBranch messageBranch;

		#endregion
	}
}

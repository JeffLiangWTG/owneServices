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
using Enterprise.Customs.ASYCUDA.Business;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public abstract class MXMessageProcessor<T> : BranchCustomsApplicationTypeMessageProcessor
		where T : EDIMessage
	{
		public MXMessageProcessor(LoggingInformation logger)
			: base(logger)
		{ }

		protected ZString errorMessage = Res.GetString("48B57F6D-F873-468E-8476-C54884067F27", "Error");
		protected HtmlTableCreator tableCreator;
		protected ZBool isFailure = ZBool.True;

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.MXCustoms;

		protected override Integration.IRegistryItem GetEmailGroupRegistryItem() => MXCustomsDataRegistry.Instance.MXMANGroupNotification;

		protected override void PreProcessMessageCore(EDIMessage message)
		{
			base.PreProcessMessageCore(message);

			var bodyText = message.EM_MessageText;
			if (message.EM_MessageType != MXMessageConstants.XER & (bodyText.IsEmpty || !XmlUtils.IsValidXml(bodyText)))
			{
				Logger.LogWarning(Res.GetString("7504DE34-658C-493E-9772-9C76C969F57F", "Message {0} failed to parse the message as {1} message.", message.EM_MessageNum, message.EM_MessageType));
				SetMessageStatusAsDiscarded(message);
			}
			else
			{
				if (message.EM_LinkUniqueID.IsEmpty)
				{
					SetMessageStatusAsFailed(message);
					Logger.LogWarning(Res.GetString("C2B85BEA-A067-4FAF-A24E-AAFACE12B4A6", "Unable to find business object for message {0}", GetEDIMessageStatusLogDescription(message)));
				}
				else
				{
					message.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
				}
			}
		}

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

		protected ZString GetEDIMessageStatusLogDescription(EDIMessage message) => Res.GetString("3B329D66-54E5-4687-BB22-406BA1555E84", "(Number:{0}, Type:{1}, Sub:{2}, Ref:{3}); message status set to {4}.", message.EM_MessageNum, message.EM_MessageType, message.EM_MessageSubType, message.EM_ApplicationReference, new EDIMessageStatusList().GetDescriptionFromCode(message.EM_Status));

		#region Send Notification Email

		void SendNotificationEmailIfNeeded(EDIMessage message)
		{
			var messageAttachee = message.EM_LinkedObject as IMessageAttachee;
			if (messageAttachee != null)
			{
				var branchPK = messageAttachee.GlobalBranchPK;
				if (branchPK.IsValid)
				{
					messageBranch = message.Factory.Load<IGlbBranch>(branchPK);
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

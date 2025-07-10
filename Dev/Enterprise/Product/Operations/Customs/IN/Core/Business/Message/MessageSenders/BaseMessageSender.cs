using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IN.Business;

public abstract class BaseMessageSender<T> : IMessageSender
	where T : BaseMessageSendingObject, IMessageSendingObject
{
	public BaseMessageSender(T messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
	}

	public EDIMessage Send(MessageSendingContext context)
	{
		var messageAttachee = messageSendingObject.MessageAttachee;
		var message = messageAttachee.Factory.New<EDIMessage>();
		message.EM_LinkedObject = (BusinessObject)messageAttachee;
		message.EM_MessageOwner = messageAttachee.MessageOwner;
		SetMessageDetails(context, message);

		messageAttachee.Messages.Add(message);
		UpdateMessageAttacheeStatus(context, messageAttachee);
		return message;
	}

	void SetMessageDetails(MessageSendingContext context, EDIMessage message)
	{
		message.EM_MessageType = MessageType;
		message.EM_MessageSubType = MessageSubType;
		message.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
		message.EM_MessageText = GetMessageText();
		message.EM_GP = GetLoginPassword()?.PK ?? ZGuid.Empty;
		message.Saving += MessageSaving;
		message.Saved += MessageSaved;

		if (context == MessageSendingContext.DOWNLOAD)
		{
			message.EM_Status = EDIMessageStatusList.Codes.Sent;
			message.EM_ApplicationReference = Constants.Messaging.MessageDownloaded;
		}
		else
		{
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
		}

		void MessageSaving(Messaging.Business.EDIMessage message)
		{
			message.EM_MessageText = SignMessage(message.EM_MessageText);
		}

		void MessageSaved(Messaging.Business.EDIMessage message, bool saveSucceeded)
		{
			message.Saving -= MessageSaving;
			message.Saved -= MessageSaved;

			if (!saveSucceeded)
			{
				if (message.EM_LinkedObject is IMessageAttachee messageAttachee)
				{
					messageAttachee.RollbackChangesOnStatus();
				}

				message.Delete();
			}
		}
	}

	void UpdateMessageAttacheeStatus(MessageSendingContext context, IMessageAttachee messageAttachee)
	{
		if (context == MessageSendingContext.EMAIL)
		{
			messageAttachee.MessageStatus = (ZString)MessageStatusList.Codes.MessageQueued;

			if (messageAttachee.CalculateStatusAfterSending(messageSendingObject.MessageType) is ZString customsStatus && !customsStatus.IsEmpty)
			{
				messageAttachee.CustomsStatus = customsStatus;
			}
		}
	}

	protected readonly T messageSendingObject;

	protected abstract ZString MessageType { get; }

	protected abstract ZString MessageSubType { get; }

	protected abstract ZString GetMessageText();

	string SignMessage(ZString message)
	{
		var result = string.Empty;
		if (!message.IsEmpty)
		{
			var messageSigner = GetManifestMessageSigner();
			result = messageSigner?.Sign(message) ?? message;
		}
		return result;
	}

	ITextSigner GetManifestMessageSigner()
	{
		var signingCertificate = GlbStaffWrapper.GetWrapperForCurrentUser().GetCertificatePassword();

		if (signingCertificate == null && !Globals.IsTest && !GlbStaff.CurrentUser.IsSupportUser)
		{
			throw new DeveloperNotificationException("CertificatePassword missing for current user");
		}

		return signingCertificate != null
			? new INMessageSigner(new CertificateCryptokiDetails(signingCertificate))
			: null;
	}

	GlbLoginPassword GetLoginPassword()
	{
		var loginPassword = GlbStaffWrapper.GetWrapperForCurrentUser().GetLoginPassword();

		if (loginPassword is null && !Globals.IsTest && !GlbStaff.CurrentUser.IsSupportUser)
		{
			throw new DeveloperNotificationException("LoginPassword missing for current user.");
		}

		return loginPassword;
	}
}

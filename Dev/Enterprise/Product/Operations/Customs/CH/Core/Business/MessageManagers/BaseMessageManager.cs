using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business;

public abstract class BaseMessageManager<T> : SingleMessageManager, IMessageManager where T : BusinessObject, IMessageSendingObject
{
	protected BaseMessageManager(T messageSender)
	{
		SendingObject = Argument.NotNull(messageSender, nameof(messageSender));
	}

	public T SendingObject { get; }

	public override BusinessObject BusinessObject => SendingObject;

	public override bool CanSendOriginal => true;

	public override bool CanSendWithdrawal => false;

	public override bool IsWaitingForResponse => false;

	protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo) => GenerateMessages((T)bizo);

	protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo) => GenerateMessages((T)bizo);

	protected override EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo) => GenerateMessages((T)bizo);

	public EDIMessage[] GenerateMessages() => GenerateMessages(SendingObject);

	EDIMessage[] GenerateMessages(T sendingObject)
	{
		var message = GenerateMessage(sendingObject ?? SendingObject);
		return message != null ? new EDIMessage[] { message } : Array.Empty<EDIMessage>();
	}

	EDIMessage GenerateMessage(T sendingObject)
	{
		BeforeGenerateMessage(sendingObject);

		var message = CreateEDIMessageCore(sendingObject);

		AfterGenerateMessage(sendingObject, message);

		return message;
	}

	protected virtual void BeforeGenerateMessage(T sendingObject) { }

	protected abstract void AfterGenerateMessage(T sendingObject, EDIMessage message);

	protected virtual EDIMessage CreateEDIMessageCore(T sendingObject)
	{
		var message = sendingObject.Factory.New<CHEDIMessage>();

		message.EM_ApplicationCode = sendingObject.ApplicationCode;
		message.EM_MessageType = sendingObject.MessageTypeForEDIMessage;
		message.EM_MessageSubType = sendingObject.MessageSubTypeForEDIMessage;
		message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		message.EM_Status = EDIMessageStatusList.Codes.Queued;
		message.EM_MessageText = sendingObject.ToMessageString();
		message.EM_GP = sendingObject.GetCredentialPK();
		message.EM_ApplicationReference = sendingObject.GetApplicationReference();

		return message;
	}

	public abstract void RollbackOnSaveFailed();
}

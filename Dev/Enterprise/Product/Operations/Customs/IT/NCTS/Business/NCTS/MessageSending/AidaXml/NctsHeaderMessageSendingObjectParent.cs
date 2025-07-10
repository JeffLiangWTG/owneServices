using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

public sealed class NctsHeaderMessageSendingObjectParent : EU.NCTS.Business.NctsHeaderMessageSendingObjectParent, IDisposable
{
	public NctsHeaderMessageSendingObjectParent(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties
	{
		get
		{
			yield return new MessageSendingObjectProperty(AutoNctsHeaderMessageSendingObject.Schema.MessageType, true, 160);
			yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.Reason, true, 80);
			yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.LegislativeReference, true, 80);
			yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.JobReferenceNumber, true, 160);
			yield return new MessageSendingObjectProperty(NctsHeaderMessageSendingObject.Schema.MessageSubType, true, 80);
		}
	}

	public new NctsHeaderMessageSendingObjectCollection SendingObjectsCollection => (NctsHeaderMessageSendingObjectCollection)base.SendingObjectsCollection;
	bool HasAnySelectedCancelMessage => SelectedSendingObjects.Any(x => x.IsCancel);

	protected override NonPersistentBusinessObjectCollection<EU.NCTS.Business.NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore()
	{
		var sendingObjectCollection = new NctsHeaderMessageSendingObjectCollection(Factory);
		sendingObjectCollection.Add(new NctsHeaderMessageSendingObject(NctsHeader));
		return sendingObjectCollection;
	}

	protected override void HookMessageSendingObjectEvents(BaseMessageSendingObject baseSendingObject)
	{
		base.HookMessageSendingObjectEvents(baseSendingObject);
		if (baseSendingObject is NctsHeaderMessageSendingObject nctsSendingObject)
		{
			nctsSendingObject.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
		}
	}

	void UnHookMessageSendingObjectEvents(BaseMessageSendingObject baseSendingObject)
	{
		if (baseSendingObject is NctsHeaderMessageSendingObject nctsSendingObject)
		{
			nctsSendingObject.MessageTypeInfo.ValueChanged -= MessageTypeInfo_ValueChanged;
		}
	}

	void MessageTypeInfo_ValueChanged(object sender, EventArgs e)
	{
		ResetValidationMessages();
	}

	protected override ZString GetBizObjValidationMessageErrors()
	{
		return HasAnySelectedCancelMessage
			? ZString.Empty
			: base.GetBizObjValidationMessageErrors();
	}

	protected override IEnumerable<INotification> GetNewMessageErrorCollector()
	{
		return base.GetNewMessageErrorCollector()
			.Concat(new NctsHeaderMessageSendingObjectAdditionalValidation(MessageSendingObject).GetAdditionalErrors());
	}

	void IDisposable.Dispose()
	{
		if (SendingObjectsCollection != null)
		{
			foreach (BaseMessageSendingObject item in SendingObjectsCollection)
			{
				UnHookMessageSendingObjectEvents(item);
			}
		}
	}

	protected override bool SendAndSaveMessagesCore()
	{
		var messageSendingObject = MessageSendingObject;
		var messageCreationStrategy = new AidaXmlOutgoingCustomsMessageCreationStrategy(Factory, messageSendingObject);

		var nctsHeader = messageSendingObject.NctsHeader;
		AssignDeclarationGoodsItemNumbers(messageSendingObject, nctsHeader);

		var messageSender = new ITMessageSender(Factory, messageCreationStrategy, new NctsHeaderSendableCustomsEntry(nctsHeader, messageSendingObject.MessageType));
		messageSender.Send();

		nctsHeader.MovementHeader.MessageStatusLogAdded += MovementHeader_MessageStatusLogAdded;
		Factory.Save();
		nctsHeader.MovementHeader.MessageStatusLogAdded -= MovementHeader_MessageStatusLogAdded;

		return true;
	}

	void MovementHeader_MessageStatusLogAdded(object sender, NctsCommonMovementHeader.CustomsStatusLogAddedEventArgs e)
	{
		if (e.MovementHeader?.Header is NctsHeader nctsHeader)
		{
			nctsHeader.LockFileIfEnabledByConfiguration(
				Res.GetString("5B8F1622-597A-4A16-A206-4070DB11D824", "The tabs are locked for editing because the message has been sent."),
				EUJobMessageTypeList.Codes.NctsDeparture);
		}
	}

	void AssignDeclarationGoodsItemNumbers(NctsHeaderMessageSendingObject messageSendingObject, NctsHeader nctsHeader)
	{
		if (!messageSendingObject.IsCancel)
		{
			nctsHeader.AssignDeclarationGoodsItemNumbers(!messageSendingObject.IsAmend);
		}
	}

	NctsHeaderMessageSendingObject MessageSendingObject => SelectedSendingObjects.Cast<NctsHeaderMessageSendingObject>().Single();
}

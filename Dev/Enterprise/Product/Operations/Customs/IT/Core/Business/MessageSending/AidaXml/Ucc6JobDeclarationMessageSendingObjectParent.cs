using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.MessageSending.AidaXml;
using MessageSendingObjectProperty = Enterprise.Customs.Business.MessageSendingObjectProperty;

namespace Enterprise.Customs.IT.Business;

public class Ucc6JobDeclarationMessageSendingObjectParent : JobDeclarationMessageSendingObjectParent
{
	public Ucc6JobDeclarationMessageSendingObjectParent(JobDeclaration declaration) : base(declaration)
	{
	}

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties { get; } = new MessageSendingObjectProperty[]
	{
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.MessageType, true, 100),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.VOCReason, true, 100),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.CancellationAndAmendmentLegislativeReference, true, 100),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.CombinedCustomsMessageSubType, true, 70),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.DeclarationType, true, 100),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.DeclarationDescription, true, 100),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.EntryStatus, true, 100),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.MessageStatus, true, 100),
		new MessageSendingObjectProperty(JobDeclarationMessageSendingObject.Schema.BGMReference, true, 150)
	};

	public bool HasAnySelectedCancelMessage => SelectedSendingObjects.Any(x => x.IsCancel);

	protected override JobDeclarationMessageSendingObjectAbstractFactory GetNewSendingObjectFactory(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent sendingObjectParent)
		=> new Ucc6XmlJobDeclarationMessageSendingObjectFactory(entryHeader, sendingObjectParent);

	protected override void HookMessageSendingObjectEvents(Customs.Business.BaseMessageSendingObject baseSendingObject)
	{
		base.HookMessageSendingObjectEvents(baseSendingObject);
		if (baseSendingObject is Ucc6JobDeclarationMessageSendingObject ucc6SendingObject)
		{
			ucc6SendingObject.MessageTypeInfo.ValueChanged += MessageTypeInfo_ValueChanged;
		}
	}

	void MessageTypeInfo_ValueChanged(object sender, System.EventArgs e)
	{
		ResetValidationMessages();
	}

	protected override ZString GetBizObjValidationMessageErrors()
	{
		return HasAnySelectedCancelMessage
			? ZString.Empty
			: base.GetBizObjValidationMessageErrors();
	}
}

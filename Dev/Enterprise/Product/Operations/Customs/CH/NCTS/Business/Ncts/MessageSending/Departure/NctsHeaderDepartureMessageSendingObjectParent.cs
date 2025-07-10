using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderDepartureMessageSendingObjectParent : NctsHeaderMessageSendingObjectParent, IMessageSendingObjectParent
{
	public NctsHeaderDepartureMessageSendingObjectParent(NctsHeader nctsHeader) : base(nctsHeader)
	{
		this.nctsHeader = SendingObjectHelper.CheckHeaderNotNullAndDepartureType(nctsHeader);
	}

	readonly NctsHeader nctsHeader;

	public override BusinessObject TopLevelBusinessObject => nctsHeader;

	public override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.EuNctsSendWithMessageErrors;

	public new NctsHeaderDepartureMessageSendingObjectCollection SendingObjectsCollection => (NctsHeaderDepartureMessageSendingObjectCollection)base.SendingObjectsCollection;

	protected override NonPersistentBusinessObjectCollection<NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore()
	{
		var sendingObjectsCollection = new NctsHeaderDepartureMessageSendingObjectCollection(Factory)
		{
			new NctsHeaderDepartureMessageSendingObject(nctsHeader)
		};

		return sendingObjectsCollection;
	}

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

	IEnumerable<IMessageSendingObject> IMessageSendingObjectParent.SelectedSendingObjects => SelectedSendingObjects.Cast<NctsHeaderDepartureMessageSendingObject>();

	readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
	{
		new MessageSendingObjectProperty(NctsHeaderDepartureMessageSendingObject.Schema.LRN, true, 160),
		new MessageSendingObjectProperty(NctsHeaderDepartureMessageSendingObject.Schema.MRN, true, 160),
		new MessageSendingObjectProperty(NctsHeaderDepartureMessageSendingObject.Schema.MessageType, true, 100),
		new MessageSendingObjectProperty(NctsHeaderDepartureMessageSendingObject.Schema.ReasonCode, true, 100),
	};

	public ZString CanSendMessage() => EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera();

	public void UpdateSendingObjectsBeforeSending() { }

	protected override void HookMessageSendingObjectEvents(BaseMessageSendingObject baseSendingObject)
	{
		base.HookMessageSendingObjectEvents(baseSendingObject);
		if (baseSendingObject is NctsHeaderDepartureMessageSendingObject nctsSendingObject)
		{
			nctsSendingObject.MessageTypeInfo.ValueChanged += (o, e) => ResetValidationMessages();
		}
	}

	public bool ShowValidationErrors => SelectedSendingObjects.Cast<NctsHeaderDepartureMessageSendingObject>().Any(x => x.ShowValidationErrors);

	protected override ZString GetBizObjValidationMessageErrors() => ShowValidationErrors ? base.GetBizObjValidationMessageErrors() : ZString.Empty;

	protected override ZString GetAdditionalWarningsCore() => ShowValidationErrors ? base.GetAdditionalWarningsCore() : ZString.Empty;
}

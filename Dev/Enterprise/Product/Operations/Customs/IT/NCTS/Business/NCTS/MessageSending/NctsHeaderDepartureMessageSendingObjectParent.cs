using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IT.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.IT.NCTS.Business;

public sealed class NctsHeaderDepartureMessageSendingObjectParent : BaseMessageSendingObjectParent<NctsHeaderDepartureMessageSendingObject>
{
	public NctsHeaderDepartureMessageSendingObjectParent(NctsHeader nctsHeader) : base(nctsHeader?.Factory)
	{
		this.nctsHeader = nctsHeader.CheckNotNullAndDepartureType();
		movementHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
	}

	readonly NctsHeader nctsHeader;
	readonly NctsDepartureMovementHeader movementHeader;

	public override BusinessObject TopLevelBusinessObject => nctsHeader;

	public override SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.EuNctsSendWithMessageErrors;

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => cachedMessageSendingObjectProperties ?? (cachedMessageSendingObjectProperties = GetMessageSendingObjectProperties().ToArray());
	IEnumerable<MessageSendingObjectProperty> cachedMessageSendingObjectProperties;

	protected override NonPersistentBusinessObjectCollection<NctsHeaderDepartureMessageSendingObject> GetSendingObjectsCollectionCore()
	{
		if (sendingObjectsCollection == null)
		{
			sendingObjectsCollection = new NctsHeaderDepartureMessageSendingObjectCollection(Factory);
			sendingObjectsCollection.Add(GetSingleMessageSendingObject());
		}
		RegisterEditableChildObject(sendingObjectsCollection);
		return sendingObjectsCollection;
	}

	NctsHeaderDepartureMessageSendingObjectCollection sendingObjectsCollection;

	public ZString CustomsMessageSendingMode
	{
		get => SingleSendingObject.CustomsMessageSendingMode;
		set => SingleSendingObject.CustomsMessageSendingMode = value;
	}

	public ZString CustomsProfile => nctsHeader.BH_CustomsProfile;

	#region Implementation

	NctsHeaderDepartureMessageSendingObject GetSingleMessageSendingObject()
	{
		if (movementHeader.BM_CustomsStatus == ITEntryStatusList.Codes.NbRejected)
		{
			return new NBStandaloneMessageSendingObject(nctsHeader);
		}
		if (movementHeader.IsTIRDeclaration)
		{
			return new TIRMessageSendingObject(nctsHeader);
		}
		return new TransitMessageSendingObject(nctsHeader);
	}

	IEnumerable<MessageSendingObjectProperty> GetMessageSendingObjectProperties()
	{
		yield return new MessageSendingObjectProperty(NctsHeaderDepartureMessageSendingObject.Schema.DepartureStatus, true, 120);
		yield return new MessageSendingObjectProperty(NctsHeaderDepartureMessageSendingObject.Schema.CombinedCustomsMessageSubType, true, 100);
		yield return new MessageSendingObjectProperty(NctsHeaderDepartureMessageSendingObject.Schema.MessageStatus, true, 100);
		yield return new MessageSendingObjectProperty(NctsHeaderDepartureMessageSendingObject.Schema.ReferenceNumber, true, 150);
	}

	NctsHeaderDepartureMessageSendingObject SingleSendingObject => (NctsHeaderDepartureMessageSendingObject)SendingObjectsCollection.Single();

	#endregion
}

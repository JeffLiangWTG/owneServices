using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderArrivalMessageSendingObjectParent : NctsHeaderMessageSendingObjectParent, IMessageSendingObjectParent
{
	public NctsHeaderArrivalMessageSendingObjectParent(NctsHeader nctsHeader) : base(nctsHeader)
	{
		this.nctsHeader = SendingObjectHelper.CheckHeaderNotNullAndArrivalType(nctsHeader);
	}
	readonly NctsHeader nctsHeader;

	public override BusinessObject TopLevelBusinessObject => nctsHeader;

	public new NctsHeaderArrivalMessageSendingObjectCollection SendingObjectsCollection => (NctsHeaderArrivalMessageSendingObjectCollection)base.SendingObjectsCollection;

	public NctsHeaderArrivalMessageSendingObjectParentValidation Validation => new NctsHeaderArrivalMessageSendingObjectParentValidation(this);

	public override Security.SecurityCheckpoint SecurityCheckpointToSendWithMessageError => Env.Security.EuNctsSendWithMessageErrors;

	protected override NonPersistentBusinessObjectCollection<NctsHeaderMessageSendingObject> GetSendingObjectsCollectionCore()
	{
		var sendingObjectCollection = new NctsHeaderArrivalMessageSendingObjectCollection(base.Factory);

		if (IsMultiMrnView)
		{
			nctsHeader.ArrivalMovementHeader.RelatedArrivalMovements.ToList<RelatedArrivalMovementGenPivot>().ForEach(relatedArrivalMovement => sendingObjectCollection.Add(new NctsHeaderArrivalMessageSendingObject(relatedArrivalMovement.ChildMovement.Header, true)));
		}
		else
		{
			sendingObjectCollection.Add(new NctsHeaderArrivalMessageSendingObject(nctsHeader));
		}

		return sendingObjectCollection;
	}

	public bool IsMultiMrnView => nctsHeader.ArrivalMovementHeader.MultipleMRNIndicator && nctsHeader.ArrivalMovementHeader.BM_CustomsStatus == NCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted;

	public override IEnumerable<MessageSendingObjectProperty> MessageSendingObjectProperties => columnDefinitions;

	IEnumerable<IMessageSendingObject> IMessageSendingObjectParent.SelectedSendingObjects => SelectedSendingObjects.Cast<NctsHeaderArrivalMessageSendingObject>();

	readonly IEnumerable<MessageSendingObjectProperty> columnDefinitions = new MessageSendingObjectProperty[]
	{
		new MessageSendingObjectProperty(NctsHeaderCommonMessageSendingObject.Schema.LRN, true, 160),
		new MessageSendingObjectProperty(NctsHeaderCommonMessageSendingObject.Schema.MRN, true, 160),
		new MessageSendingObjectProperty(NctsHeaderCommonMessageSendingObject.Schema.MessageType, true, 100),
	};

	public ZString CanSendMessage() => EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera();

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		DateOfUnloading = ZDateTimeOffset.Now;
	}

	public void UpdateSendingObjectsBeforeSending()
	{
		SelectedSendingObjects.Cast<NctsHeaderArrivalMessageSendingObject>().ForEach(sendingObject =>
		{
			sendingObject.NctsHeader.ArrivalMovementHeader.BM_UnloadingDate = DateOfUnloading;
		});
	}

	[ResourceStringData("CH.NCTS.NctsHeaderArrivalMessageSendingObjectParent|DateOfUnloading", Caption = "Date of unloading")]
	public ZDateTimeOffset DateOfUnloading
	{
		get { return dateOfUnloading; }
		set
		{
			SetNonPersistentPropertyValue(DateOfUnloadingInfo, ref dateOfUnloading, value);
			if (!IsValidationSuspended)
			{
				Validation.ValidateDateOfUnloading();
			}
		}
	}
	ZDateTimeOffset dateOfUnloading;

	public ZPropertyInfo DateOfUnloadingInfo => GetZPropertyInfo(nameof(DateOfUnloading));

	[ResourceStringData("CH.NCTS.NctsHeaderArrivalMessageSendingObjectParent|IsConformed", Caption = "Unloaded cargo conforms to declaration")]
	[ReadOnly(true)]
	public ZBool IsConformed => true;

	public ZPropertyInfo IsConformedInfo => GetZPropertyInfo(nameof(IsConformed));
}

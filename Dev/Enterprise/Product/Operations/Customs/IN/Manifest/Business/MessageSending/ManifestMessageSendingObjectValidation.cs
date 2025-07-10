using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IN.Manifest.Business;

public class ManifestMessageSendingObjectValidation : ZValidation
{
	public ManifestMessageSendingObjectValidation(ManifestMessageSendingObject parent) : base(parent)
	{
		this.parent = parent;
	}
	readonly ManifestMessageSendingObject parent;

	public override Type AutoValidationType => typeof(ManifestMessageSendingObjectValidation);

	public override void ValidateAll()
	{
		ValidateMessageType();
	}

	public void ValidateMessageType()
	{
		ValidateCalculatedProperty(parent.MessageTypeInfo);
	}

	protected void CheckMessageType()
	{
		ListValidation.ErrorIfInvalidCode(parent.MessageTypeInfo);
		ValidateMessageTypeWithAction();
	}

	void ValidateMessageTypeWithAction()
	{
		if (parent.MessageType == ManifestMessageTypeList.Codes.Amendment
			&& parent.Parent is CGMAsycudaManifestHeader header
			&& !(header.Action == ManifestMessageTypeList.Codes.Amendment
			|| header.Bills.AsEnumerable().Any(bill => bill.ABL_BillStatus == BillActionList.Codes.Amendment || bill.ABL_BillStatus == BillActionList.Codes.Supplementary)))
		{
			parent.MessageTypeInfo.AddError(Res.GetString("7638D488-2171-4DA3-86C2-4610F1E51DE7", "Could not find any Amendment in the current transaction. Cannot Proceed."));
		}
	}
}

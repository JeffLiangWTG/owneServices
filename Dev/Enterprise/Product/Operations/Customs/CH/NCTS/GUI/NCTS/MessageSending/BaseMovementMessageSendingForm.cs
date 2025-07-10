using System.Collections.Generic;
using Enterprise.Customs.CH.GUI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using NctsHeader = Enterprise.Customs.CH.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.CH.NCTS.GUI;

public abstract class BaseMovementMessageSendingForm : EU.NCTS.GUI.MessageSendingForm
{
	public BaseMovementMessageSendingForm(NctsHeaderMessageSendingObjectParent messageSendingObjectParent) : base(messageSendingObjectParent)
	{
	}

	new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

	protected override bool CheckIsOKToSend()
	{
		return ConfirmWhenAlreadySend() && base.CheckIsOKToSend();
	}

	bool ConfirmWhenAlreadySend()
	{
		var isConfirmed = true;
		if (NctsHeader?.IsMessageStatusSent ?? false)
		{
			using (var form = new ResendReasonSendForm())
			{
				if (isConfirmed = ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK)
				{
					NctsHeader.Logs.AddNew(AutoEvents.Authorised, ResendEventReference, new KeyValuePair<string, string>(EventReferenceParameters.Reason, form.Reason));
				}
			}
		}
		return isConfirmed;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string ResendEventReference = "Submitting when Message status is sent";
}

using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.NCTS.GUI
{
	public class MessageSendingForm : EU.NCTS.GUI.MessageSendingForm
	{
		public MessageSendingForm(NctsHeaderMessageSendingObjectParent sendingObjectParent) : base(sendingObjectParent)
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			RunPreSendValidation();
		}

		protected override bool CheckIsOKToSend()
		{
			return ConfirmWhenAlreadySend() && base.CheckIsOKToSend();
		}

		bool ConfirmWhenAlreadySend()
		{
			var isConfirmed = true;
			if (NctsHeader.CommonMovementHeader.BM_MessageStatus.EqualsIgnoringCase(NctsMessageStatusList.Codes.SentToCustoms))
			{
				using (var form = new ConfirmSendForm())
				{
					if (isConfirmed = ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK)
					{
						var confirmReason = form.Reason;
						if (NctsHeader.IsArrivalMovement)
						{
							NctsHeader.ArrivalMovementHeader.Logs.AddNew(AutoEvents.Authorised, IE.Business.Constants.SendingMessageEventList.Reference, ZDateTimeOffset.Now, new KeyValuePair<string, string>(IE.Business.Constants.SendingMessageEventList.RES, confirmReason));
						}
						else
						{
							NctsHeader.MovementHeader.Logs.AddNew(AutoEvents.Authorised, IE.Business.Constants.SendingMessageEventList.Reference, ZDateTimeOffset.Now, new KeyValuePair<string, string>(IE.Business.Constants.SendingMessageEventList.RES, confirmReason));
						}
					}
				}
			}
			return isConfirmed;
		}

		protected override MessageSendingNotificationCollection RunPreSendValidation()
		{
			var notificationCollection = base.RunPreSendValidation();
			CheckForFinalState(notificationCollection);
			return notificationCollection;
		}

		void CheckForFinalState(MessageSendingNotificationCollection originalCollection)
		{
			if (NctsHeader.MovementHeader is NctsDepartureMovementHeader movementHeader)
			{
				if (movementHeader.IsPhase5Departure && movementHeader.BM_CustomsStatus.EqualsIgnoringCase(NCTS5DepartureCustomsStatusList.Codes.GoodsWrittenOffClosed)
					|| movementHeader.IsPhase5Arrival && movementHeader.BM_CustomsStatus.EqualsIgnoringCase(NCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease)
				)
				{
					originalCollection.AddError(Res.GetString("A1DE41D6-69CD-47FE-B574-3630EA463052", "The entry has reached it’s final state, please do not submit any further message."));
				}
			}
		}
	}
}

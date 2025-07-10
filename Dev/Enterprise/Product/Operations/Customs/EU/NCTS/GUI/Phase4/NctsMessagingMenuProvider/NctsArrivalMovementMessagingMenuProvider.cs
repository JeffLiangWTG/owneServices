using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class NctsArrivalMovementMessagingMenuProvider : NctsMessagingMenuProvider
	{
		public NctsArrivalMovementMessagingMenuProvider(NctsHeader header)
			: base(header)
		{
		}

		public NctsArrivalMovementMessagingMenuProvider(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper)
			: base(header, ntcsHeaderUniversalMessagingHelper)
		{
		}

		public override IEnumerable<ZMenuItem> CreateMenuItems()
		{
			yield return sendArrivalMessageMenuItem = new ZMenuItem(ResString.GetMultilingualString("025A1851-C73C-4BAD-85FE-70E7F57DD494", "Send &Arrival Message"), SendArrivalMessageClick);
			yield return sendUnloadingRemarksMenuItem = new ZMenuItem(ResString.GetMultilingualString("3F21F4E3-D565-4CA9-8168-D71897060B37", "Send &Unloading Remarks"), SendUnloadingRemarksClick);
		}

		protected ZMenuItem sendArrivalMessageMenuItem;
		void SendArrivalMessageClick(object sender, EventArgs e)
		{
			CheckNctsHeaderIsNotNullAndThenDoSending(
				delegate
				{
					if (this.Header.IsArrivalTabReadOnly)
					{
						if (Globals.Message.ShowConfirmation(
							ResString.GetMultilingualString("B0E64FB3-555E-4214-A44D-D70CA703BF14", "Are you sure you want to resend this message?"),
							ResString.GetMultilingualString("920DDF24-17B7-4FC4-A249-0D7C61059498", "Resend Message"),
							ResString.GetMultilingualString("F16AD969-4232-4789-8724-8A2D4C6C52FF", "yes"),
							MessageBoxIcon.Question) != DialogResult.OK)
						{
							return;
						}
					}
					SendMessageToNcts(new NctsMessageFunctionSet.ArrivalNotificationMessage());
				});
		}

		protected ZMenuItem sendUnloadingRemarksMenuItem;
		void SendUnloadingRemarksClick(object sender, EventArgs e)
		{
			CheckNctsHeaderIsNotNullAndThenDoSending(
				delegate
				{
					if (Header.IsUnloadingRemarksTabReadOnly)
					{
						if (Header.EffectiveMessageStatus == NctsMessageStatusList.Codes.UnloadingRemarksSent)
						{
							if (Globals.Message.ShowConfirmation(
								ResString.GetMultilingualString("B0E64FB3-555E-4214-A44D-D70CA703BF14", "Are you sure you want to resend this message?"),
								ResString.GetMultilingualString("920DDF24-17B7-4FC4-A249-0D7C61059498", "Resend Message"),
								ResString.GetMultilingualString("F16AD969-4232-4789-8724-8A2D4C6C52FF", "yes"),
								MessageBoxIcon.Question) != DialogResult.OK)
							{
								return;
							}
						}
					}
					SendMessageToNcts(new NctsMessageFunctionSet.UnloadingRemarksMessage());
				});
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			SetMenuItemVisibility(sendArrivalMessageMenuItem, () => CanSendArrivalMessage);
			SetMenuItemVisibility(sendUnloadingRemarksMenuItem, () => CanSendUnloadingRemarks);
		}

		protected virtual ZBool CanSendArrivalMessage => (Header.ArrivalMovementHeader?.BM_CustomsStatus ?? ZString.Empty) == NctsTransitStatusList.Codes.Unknown;

		protected virtual ZBool CanSendUnloadingRemarks => Header.UnloadingRemarksAllowedOverride || Header.IsUnloadingAllowedOrComplete;
	}
}

using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.EMCS.GUI;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.EMCS.GUI
{
	public sealed class EMCSMenu : EU.EMCS.GUI.EMCSMenu
	{
		public EMCSMenu(EU.EMCS.Business.EMCSJobDeclaration declaration) : base(declaration)
		{
			emcsDeclaration = declaration;
			var preValidateTrader = new ZMenuItem(ResString.GetMultilingualString("C90753F2-6ADF-420F-935B-A71347A5C671", "Pre-Validate Trader"), PreValidateTrader_Click);
			_ = MenuItems.Add("-");
			_ = MenuItems.Add(preValidateTrader);
		}

		void PreValidateTrader_Click(object sender, EventArgs args)
		{
			var preValidateTraderInfo = new PreValidateTraderInfo(emcsDeclaration);
			if (emcsDeclaration.CanSend((ZForm)GetMainMenu()?.GetForm(), EMCSPreValidateTraderInfoHelper.Constants.PreValidateTraderInfoWarningMessage, () => preValidateTraderInfo.CanSend))
			{
				var manager = new PreValidateTraderSendingManager(emcsDeclaration, preValidateTraderInfo);
				manager.Send();
				ShowResultNotification(manager.NotificationCollection);
			}
		}

		void ShowResultNotification(MessageSendingNotificationCollection notifications)
		{
			if (notifications.ContainsError())
			{
				Globals.Message.ShowError(notifications.ErrorNotificationsAsString(), Res.GetString("9E9F115F-D577-46F3-BCFB-9353DBEBCDEE", "Error in Sending Pre-Validate Trader"));
			}
			else if (notifications.ContainsInformation())
			{
				Globals.Message.ShowInformation(notifications.InformationNotificationsAsString(), Res.GetString("AC06ED6F-41B1-4E5A-96FC-6241510B61A5", "Pre-Validate Trader Sending Result"));
			}
		}

		readonly EU.EMCS.Business.EMCSJobDeclaration emcsDeclaration;
	}
}

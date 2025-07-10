using System;
using System.Linq;
using System.Windows.Forms;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class Phase5NctsMessagingMenuItem : CommonNctsMessagingMenuItem
	{
		public Phase5NctsMessagingMenuItem()
		{
			CaptionResourceString = Res.GetData("98502205-C0BD-43B5-8B1E-4153BBE49804", "&NCTS");
		}
		Phase5MessagingMenuProvider messagingMenuProvider;

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			messagingMenuProvider?.RefreshMenu();
		}

		protected override void SetupMessageSenderProvider()
		{
			if (NctsHeader != null)
			{
				messagingMenuProvider = Phase5MessagingMenuProvider.GetProvider(NctsHeader);
				if (messagingMenuProvider != null)
				{
					MenuItems.AddRange(messagingMenuProvider.CreateMenuItems().ToArray<MenuItem>());
				}
			}
		}
	}
}

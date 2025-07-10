using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public class SeaCargoDepotOutturnMessagingMenu : CMRMessageManagementMenu
	{
		public SeaCargoDepotOutturnMessagingMenu(CusOutturnHeaderMessageManager manager)
			: base(manager)
		{
		}

		protected CusOutturnHeaderMessageManager Manager
		{
			get { return (CusOutturnHeaderMessageManager)manager; }
		}

		protected override bool SendMessagesClickCore(object sender)
		{
			try
			{
				Manager.SendOnMenuItem = true;
				if (Manager.ShouldSendOriginal)
				{
					return base.SendMessagesClickCore(sender);
				}
				else
				{
					amendMessages.PerformClick();
				}
				return true;
			}
			finally
			{
				Manager.SendOnMenuItem = false;
			}
		}
	}
}

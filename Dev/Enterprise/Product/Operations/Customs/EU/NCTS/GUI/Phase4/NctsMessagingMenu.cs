using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public class NctsMessagingMenu : CommonNctsMessagingMenuItem
	{
		public NctsMessagingMenu(ZForm parentForm)
		{
			CaptionResourceString = Res.GetData("B241DEE2-FD1E-4CF7-BA36-676A3906BFA4", "&Messaging");
			this.parentForm = parentForm;
		}
		readonly ZForm parentForm;

		protected override void Dispose(bool disposing)
		{
			if (NctsHeader != null)
			{
				NctsHeader.BH_HeaderTypeInfo.ValueChanged -= BH_HeaderTypeInfo_ValueChanged;
			}
			base.Dispose(disposing);
		}

		protected override void SetupMessageSenderProvider()
		{
			MenuItems.Clear();

			if (NctsHeader != null)
			{
				messagingMenuProvider = NctsMessagingMenuProvider.New(NctsHeader, GetNctsHeaderUniversalMessagingHelper(), parentForm ?? FallbackToMainMenuForm());
				AddMenuItemsIfProviderIsNotNull();
			}

			ZForm FallbackToMainMenuForm() => (ZForm)GetMainMenu()?.GetForm();
		}

		void AddMenuItemsIfProviderIsNotNull()
		{
			if (messagingMenuProvider != null)
			{
				MenuItems.AddRange(messagingMenuProvider.CreateMenuItems().ToArray());
			}
		}

		NctsMessagingMenuProvider messagingMenuProvider;

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			if (messagingMenuProvider != null)
			{
				messagingMenuProvider.RefreshMenu();
			}
		}
		NctsHeaderUniversalMessagingHelper GetNctsHeaderUniversalMessagingHelper() => GetNctsHeaderUniversalMessagingHelperCore();
		protected virtual NctsHeaderUniversalMessagingHelper GetNctsHeaderUniversalMessagingHelperCore() => new NctsHeaderUniversalMessagingHelper();
	}
}

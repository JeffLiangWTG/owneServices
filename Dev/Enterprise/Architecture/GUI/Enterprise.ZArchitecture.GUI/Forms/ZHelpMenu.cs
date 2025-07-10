using System;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	#region Key Status Menu Item

	public class ZKeyStatusMenuItem : ZMenuItem
	{
		ZKeyStatusMenuItem() : base(ResString.GetMultilingualString("F8DFE353-D09E-42CC-BA25-45FC495A973E", "&Key Status"))
		{ }

		public static void AddZKeyStatusMenuItem(ZToolStripMenuItem parentMenu)
		{
			var item = new ZKeyStatusMenuItem();
			var tolStripItem = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(item);
			tolStripItem.Click += (sender, args) => item.OnClick(args);
			parentMenu.DropDownItems.Add(tolStripItem);
		}

		public static void AddZKeyStatusMenuItem(MenuItem parentMenu)
		{
			var item = new ZKeyStatusMenuItem();
			parentMenu.MenuItems.Add(item);
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			ZKeyStatusForm.Instance.Show();
		}
	}

	#endregion

	#region Training Menu Item

	public class ZTrainingModeMenuItem : ZMenuItem
	{
		ZTrainingModeMenuItem()
			: base(ResString.GetMultilingualString("f5721ef0-0304-4e3d-8130-e123381b624f", "&Training Mode"))
		{ }

		public static void AddTrainingModeMenuItem(ZToolStripMenuItem parentMenu)
		{
			var item = new ZTrainingModeMenuItem();
			var toolStripItem = (ToolStripMenuItem)MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(item);
			toolStripItem.Click += (sender, args) => item.OnClick(args);
			parentMenu.DropDownOpening += (sender, args) => ParentMenu_Popup(toolStripItem);
			parentMenu.DropDownItems.Add(toolStripItem);
		}

		public static void AddTrainingModeMenuItem(MenuItem parentMenu)
		{
			var item = new ZTrainingModeMenuItem();
			parentMenu.Popup += (sender, args) => ParentMenu_Popup(item);
			parentMenu.MenuItems.Add(item);
		}

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			var instance = EnvProxy.Instance;
			instance.Registry.TraningModeEnabled = !instance.Registry.TraningModeEnabled;
#if WINZOR
			Checked = EnvProxy.Instance.Registry.TraningModeEnabled;
#endif
		}

		static void ParentMenu_Popup(object clickedItem)
		{
			var item = clickedItem as ZMenuItem;
			if (item != null)
			{
				item.Checked = EnvProxy.Instance.Registry.TraningModeEnabled;
			}
			else
			{
				var item2 = clickedItem as ZToolStripMenuItem;
				if (item2 != null)
				{
					item2.Checked = EnvProxy.Instance.Registry.TraningModeEnabled;
				}
			}
		}
	}

	#endregion

	#region Service Request Menu Item

	public class ServiceRequestMenuItem : ZMenuItem
	{
		ServiceRequestMenuItem()
			: base(ServiceRequestMenuName)
		{
			Shortcut = Shortcut.F1;
			ShowShortcut = true;
		}

		public static MultilingualString ServiceRequestMenuName
		{
			get { return ResString.GetMultilingualString("de0c95d8-f23f-4e2e-9a9c-96892c7cc91a", "New eRequest"); }
		}

		public static void AddServiceRequestMenuItem(ZToolStripMenuItem parentMenu, Form parentForm)
		{
			var item = new ServiceRequestMenuItem { ShowingForm = parentForm };
			var tolStripItem = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(item);
			tolStripItem.Click += (sender, args) => item.OnClick(args);
			parentMenu.DropDownItems.Add(tolStripItem);
		}

		public static void AddServiceRequestMenuItem(MenuItem parentMenu, Form parentForm)
		{
			var item = new ServiceRequestMenuItem { ShowingForm = parentForm };
			parentMenu.MenuItems.Add(item);
		}

		Form ShowingForm;

		protected override void OnClick(EventArgs e)
		{
			using (new MenuClickPendingTracker())
			{
				base.OnClick(e);
				ServiceRequestHandler.Launch(ShowingForm);
			}
		}
	}

	public static class ServiceRequestHandler
	{
		public static void Launch(Form parentForm)
		{
			if (!CheckCanConnectToUserPortal())
			{
				return;
			}

			var controller = ZControllerFactory.Create(ControllerIDs.ServiceRequest);
			((IServiceRequestController)controller).SetParentForm(parentForm);

			controller.ShowNewForm();
		}

		static bool CheckCanConnectToUserPortal()
		{
			var result = true;
			var currentUser = Env.CurrentUser;
			if (currentUser.IsSystemAccount)
			{
				Globals.Message.ShowError(
					Res.GetString("B4C99B1A-E94B-417D-BB37-76C753DEC739", "System users do not have access to {0}.", ServiceRequestMenuItem.ServiceRequestMenuName),
					Res.GetString("5F3B9CF8-5BF6-41F6-B860-522BDAF41E14", "Access Denied"));
				result = false;
			}
			else
			{
				var helpProvider = ObjectFactory.Get<IHelpMenuProvider>();
				result = helpProvider.ShowDisclaimerConfirmation();
			}

			return result;
		}
	}

	#endregion

	public class ZHelpMenu : ZMenuItem
	{
		public ZHelpMenu(Form parentForm)
			: base(ResString.GetMultilingualString("8801282c-3a6d-4130-a87c-b4348dd079f6", "&Help"))
		{
			var helpProvider = ObjectFactory.Get<IHelpMenuProvider>();

			MenuItems.Add(new ZMenuItem(CargoWiseWebName, (sender, args) => helpProvider.ShowUserPortal()));
			var wtaMenuItem = new ZMenuItem(WiseTechAcademyName);
			wtaMenuItem.MenuItems.Add(new ZMenuItem(WiseTechAcademyMyLearningName, (sender, args) => helpProvider.ShowWiseTechAcademy()));
			wtaMenuItem.MenuItems.Add(new ZMenuItem(WiseTechAcademyContentAndSupportName, (sender, args) => helpProvider.ShowWiseTechAcademy(WiseTechAcademyContentAndSupportPath, WiseTechAcademyContentAndSupportTarget)));
			wtaMenuItem.MenuItems.Add(new ZMenuItem(ReleaseNotesName, (sender, args) => helpProvider.ShowWiseTechAcademy(WiseTechAcademyContentAndSupportPath, WiseTechAcademyUpdateNotesTarget)));
			MenuItems.Add(wtaMenuItem);
			MenuItems.Add(new ZMenuItem(BorderWiseLogin, (sender, args) => helpProvider.ShowBorderWiseWebApp()));
			MenuItems.Add("-");

			ZKeyStatusMenuItem.AddZKeyStatusMenuItem(this);
			ZTrainingModeMenuItem.AddTrainingModeMenuItem(this);

			ServiceRequestMenuItem.AddServiceRequestMenuItem(this, parentForm);
			if (!DesignModeFinder.IsDesigning)
			{
				MenuItems.Add(new ZMenuItem(eRequestPortalName, (sender, args) => helpProvider.ShowERequestPortal()));
			}

			MenuItems.Add("-");
			MenuItems.Add(new ZMenuItem(HotKeyHelpName, (sender, args) => helpProvider.ShowHotKeyHelp(((ZMenuItem)sender).ParentControl)));

			MenuItems.Add("-");
			MenuItems.Add(new ZMenuItem(AboutName, (sender, args) => helpProvider.ShowAbout()));
		}

		public static MultilingualString CargoWiseWebName => ResString.GetMultilingualString("7b176a54-bbd6-4343-bdae-63b0eddd9c1c", "My Account");

		public static MultilingualString WiseTechAcademyName => ResString.GetMultilingualString("70e6a2e0-2eb8-4a65-9da2-8aaa2ecfffbc", "WiseTech Academy");

		public static MultilingualString WiseTechAcademyMyLearningName => ResString.GetMultilingualString("cc1104bb-5890-48da-ab53-83acc35d4add", "My Learning");

		public static MultilingualString WiseTechAcademyContentAndSupportName => ResString.GetMultilingualString("f625c71a-810d-442c-b72f-bf10d58e30a7", "Content && Support");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Path element")]
		public static string WiseTechAcademyContentAndSupportPath => "product";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Path element")]
		public static string WiseTechAcademyContentAndSupportTarget => "cargowise";

		public static string WiseTechAcademyUpdateNotesTarget => (NoResString)"cargowise&type=update+note";

		public static MultilingualString ReleaseNotesName => ResString.GetMultilingualString("2342D7B2-2521-4931-B407-175D54478B3D", "Update Notes Portal");

		public static MultilingualString eRequestPortalName => ResString.GetMultilingualString("f53354b3-d281-4e69-a44c-332eb796bcae", "Manage eRequests Online");

		public static MultilingualString AboutName => ResString.GetMultilingualString("MenuItem.Main.About", "About");

		public static MultilingualString HotKeyHelpName => ResString.GetMultilingualString("MenuItem.Main.HotKeyHelp", "Hot Key Help");

		public static MultilingualString DeveloperFeatureControlOverride => ResString.GetMultilingualString("MenuItem.Main.DeveloperFeatureControlOverrideName", "Developer Feature Override");

		public static MultilingualString BorderWiseLogin => ResString.GetMultilingualString("MenuItem.Main.BorderWiseLogin", "BorderWise");

		public static MultilingualString CargoWiseWebPortals => ResString.GetMultilingualString("MenuItem.Main.CargoWiseWebPortals", "CargoWise Web Portals");

		#region Setup Client Hook Help Menu

		protected void SetupClientHookHelpMenu()
		{
			var clientHook = ClientHookLoader.Instance.ClientHook;
			if (clientHook != null && !string.IsNullOrEmpty(clientHook.HelpWebPage))
			{
				MenuItems.Add(0, new ZMenuItem(ResString.GetMultilingualString("aba46e3a-c340-4b40-8759-8d177f0bc4bc", "{0} Extensions", clientHook.ClientDisplayName), (sender, args) => WebUrlLauncher.Launch(ClientHookLoader.Instance.ClientHook.HelpWebPage)));
			}
		}

		protected override void OnPopup(EventArgs e)
		{
			base.OnPopup(e);
			SetupClientHookHelpMenu();
		}

		#endregion
	}
}

using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.IL.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Testing;

namespace Enterprise.Customs.IL.GUI.Testing
{
	public abstract class CustomsMessagingPlugInTestBase : ZPlugInGenericTest
	{
		public void TestProperties()
		{
			using (var plugin = GetPlugInForTest())
			{
				AssertEquals("Name", MainMenu, plugin.Name);
				AssertEquals("CanDelete", false, plugin.CanDelete);
				AssertEquals("HasUserControl", true, plugin.HasUserControl);
				AssertEquals("LicenseCheckPoint", Env.Licence.Forwarder, plugin.LicenceCheckPoint);
				AssertType<CustomsMessagingMessageManager>(plugin.BusinessEntity);
			}
		}

		public void TestGetNewUserControl()
		{
			using (var plugin = (CustomsMessagingPlugInBase)GetPlugInToTest())
			{
				AssertType<CustomsMessagingControl>(plugin.UserControl);
			}
		}

		protected const string MainMenu = "Customs Messaging";

		protected void AssertMenuItemVisible(ZTemplateForm form, string menuItemName, bool isVisible)
		{
			var customsMessagingItem = FindMenuItem(form, "Customs Messaging");
			customsMessagingItem.OnPopup(EventArgs.Empty);
			var menuItem = FindMenuItem(form, menuItemName);
			AssertEquals(menuItemName, isVisible, menuItem.Visible);
		}

		protected static MenuItem FindMenuItem(ZForm form, string path)
		{
			return !string.IsNullOrEmpty(path)
				? FindMenuItem(form.Menu.MenuItems, path.Split('|'))
				: null;
		}

		protected static MenuItem FindMenuItem(Menu.MenuItemCollection items, string[] path)
		{
			if (path == null || path.Length == 0)
			{
				return null;
			}

			MenuItem menuItem = null;

			foreach (MenuItem item in items)
			{
				var itemText = item.Text.Replace("&", "");

				if (itemText == path[0])
				{
					menuItem = item;
					break;
				}
			}

			return path.Length == 1 || menuItem == null
				? menuItem
				: FindMenuItem(menuItem.MenuItems, path.Skip(1).ToArray());
		}

		protected class FormForTest : ZTemplateForm
		{
			public FormForTest(BusinessObject businessEntity)
				: base(businessEntity)
			{
				PlugIns.Add(ControllerIDs.Customs.IL.CustomsMessaging);
			}
		}

		protected abstract ICustomsMessagingPlugInBaseForTest GetPlugInForTest();
	}

	public interface ICustomsMessagingPlugInBaseForTest : IDisposable
	{
		string Name { get; }

		bool CanDelete { get; }

		bool HasUserControl { get; }

		Licensing.LicenceCheckpoint LicenceCheckPoint { get; }

		IBusiness BusinessEntity { get; }
	}
}

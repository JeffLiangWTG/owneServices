using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.IL.GUI
{
	public abstract class CustomsMessagingPlugInBase : ZPlugIn
	{
		protected CustomsMessagingPlugInBase(BusinessObject entity) : base(entity)
		{
			this.entity = entity;
		}

		public override string Name => ResString.GetMultilingualString("4E2AEBD3-EA30-40BB-AA6C-7C648DC6B625", "Customs Messaging");

		public override bool CanDelete => false;

		protected override Control GetNewUserControl() => new CustomsMessagingControl();

		protected override IBusiness GetBusinessEntityForPlugIn()
		{
			if (messageManager == null && entity != null)
			{
				messageManager = CustomsMessagingMessageManager.New(entity);
			}

			return messageManager;
		}

		protected override MenuItem GetNewTopLevelMenu()
		{
			var businessObject = entity;
			var moduleIdentifier = ModuleIdentifier;

			var menus = GetMenuItems();

			if (menus.Any())
			{
				mainMenuItem = new ZMenuItem(Name);
				mainMenuItem.AddFormsMenuItems(businessObject, moduleIdentifier, CreateCustomsMessagingMenuItemInfos());
				foreach (var menuItem in menus)
				{
					mainMenuItem.MenuItems.Add(menuItem);
				}
			}

			return mainMenuItem;
		}

		protected override ZBool HasUserControl => true;

		protected override LicenceCheckpoint LicenceCheckPoint => Env.Licence.Forwarder;

		protected void UpdateMenuVisibility(MenuItem menuItem, bool visible)
		{
			menuItem.Visible = visible;
			var subMenuItems = menuItem.MenuItems.OfType<MenuItem>().ToArray();

			foreach (var subMenuItem in subMenuItems)
			{
				var subMenuVisible = IsMenuItemVisible(subMenuItem, visible);
				UpdateMenuVisibility(subMenuItem, subMenuVisible);
			}
		}

		protected virtual bool IsMenuItemVisible(MenuItem subMenuItem, bool visible) => visible;

		protected void OnChangeTheVisibilityRequired(object sender, EventArgs e)
		{
			if (TopLevelMenu == null || TopLevelMenu.MenuItems.Count == 0)
			{
				GetNewTopLevelMenu();
			}

			ChangeTheVisibility();
		}

		protected void ChangeTheVisibility()
		{
			Enabled = IsEnabled;

			if (TopLevelMenu != null)
			{
				UpdateMenuVisibility(TopLevelMenu, Enabled);
			}
		}

		protected abstract bool IsEnabled { get; }

		protected abstract ModuleIdentifier ModuleIdentifier { get; }

		protected abstract IEnumerable<MenuItem> GetMenuItems();

		IEnumerable<IMenuItemInfo> CreateCustomsMessagingMenuItemInfos()
		{
			yield return new SystemMenuItemInfo
			{
				ID = new ZGuid("D4D60900-3B00-4069-AB1E-52D7E6A854B1")
			};
		}

		readonly BusinessObject entity;
		CustomsMessagingMessageManager messageManager;
		MenuItem mainMenuItem;
	}
}

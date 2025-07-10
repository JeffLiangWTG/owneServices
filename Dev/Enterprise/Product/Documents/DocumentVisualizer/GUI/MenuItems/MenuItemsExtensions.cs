using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentVisualizer.GUI
{
	public static class MenuItemsExtensions
	{
		public static void AddFormsMenuItems(this MenuItem parentMenuItem, BusinessObject bizObj, ModuleIdentifier moduleIdentifier, IEnumerable<IMenuItemInfo> menuItemInfos)
		{
			Argument.NotNull(parentMenuItem, "parentMenuItem");
			Argument.NotNull(bizObj, "bizObj");
			Argument.NotNull(moduleIdentifier, "moduleIdentifier");
			Argument.NotNull(menuItemInfos, "menuItemInfos");

			var provider = ObjectFactory.Get<IVisualizableDocumentCommandProvider>();

			var actions = new List<Action>();

			var subMenuItems = CreateMenuItems(provider, bizObj, moduleIdentifier, actions, menuItemInfos).ToArray();

			if (!subMenuItems.Any())
			{
				return;
			}

			var noMessagesAvailableMenuItem = new ZMenuItem(ResString.GetMultilingualString("4acdc06c-0e62-42a3-8841-518f620dcc09", "No Messages Available"));
			parentMenuItem.MenuItems.Add(noMessagesAvailableMenuItem);

			parentMenuItem.MenuItems.AddRange(subMenuItems);

			parentMenuItem.Popup += (s, e) =>
			{
				actions.ForEach(action => action());

				var subMenus = parentMenuItem
					.MenuItems
					.OfType<MenuItem>()
					.Where(mi => mi != noMessagesAvailableMenuItem)
					.ToArray();

				var isAnySubMenuVisible = false;

				foreach (var subMenu in subMenus)
				{
					isAnySubMenuVisible = UpdateMenuVisibility(subMenu) || isAnySubMenuVisible;
				}

				noMessagesAvailableMenuItem.Visible = !isAnySubMenuVisible;
			};
		}

		static IEnumerable<ZMenuItem> CreateMenuItems(IVisualizableDocumentCommandProvider provider, BusinessObject bizObj, ModuleIdentifier moduleIdentifier, IList<Action> actions, IEnumerable<IMenuItemInfo> menuItemInfos)
		{
			var businessContext = (bizObj as IDocumentSupportable)?.DocumentSupporter.BusinessContext;

			if (!businessContext.HasValue)
			{
				yield break;
			}

			foreach (var menuItemInfo in menuItemInfos)
			{
				ZMenuItem menuItem = null;

				switch (menuItemInfo)
				{
					case ParentMenuItemInfo parentMenuItemInfo:
						menuItem = new ZMenuItem((NoResString)parentMenuItemInfo.Name);

						foreach (var subMenuItem in CreateMenuItems(provider, bizObj, moduleIdentifier, actions, parentMenuItemInfo.SubMenus))
						{
							menuItem.MenuItems.Add(subMenuItem);
						}

						if (parentMenuItemInfo.OnPopup != null)
						{
							actions.Add(() => parentMenuItemInfo.OnPopup.Invoke(menuItem));
						}
						break;

					case CustomMenuItemInfo customMenuItemInfo:
						menuItem = new ZMenuItem(
							(NoResString)customMenuItemInfo.Name,
							(s, e) => customMenuItemInfo.OnClick?.Invoke(bizObj));

						actions.Add(() => menuItem.Visible = customMenuItemInfo.IsApplicable?.Invoke(bizObj) ?? true);
						break;

					case SystemMenuItemInfo systemMenuItemInfo:
						var stmMenuItem = systemMenuItemInfo.ID.IsValid
							? GetStmMenuItem(bizObj.Factory, systemMenuItemInfo.ID, businessContext.Value)
							: null;

						var command = stmMenuItem != null
							? provider?.GetCommand(bizObj, stmMenuItem, moduleIdentifier)
							: null;

						if (command == null)
						{
							continue;
						}

						menuItem = new ZMenuItem((NoResString)command.Name);

						menuItem.Click += (s, e) =>
						{
							if (systemMenuItemInfo.Precondition == null || systemMenuItemInfo.Precondition())
							{
								command.Execute();
							}
						};

						actions.Add(() => menuItem.Visible = command.IsApplicable);

						break;
				}

				yield return menuItem;
			}
		}

		static IStmMenuItem GetStmMenuItem(BusinessObjectFactory factory, ZGuid pk, BusinessContext businessContext)
		{
			var query = new ZQuery(StmMenuItemSchema.PK, pk);
			query.AddToFilter(StmMenuItemSchema.SU_MenuType, Enterprise.Core.Constants.StmMenuItemTypes.Forms);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, businessContext.ToString());

			return factory.LoadTop1<StmMenuItemBase>(query);
		}

		static bool UpdateMenuVisibility(MenuItem menuItem)
		{
			var subMenuItems = menuItem
				.MenuItems
				.OfType<MenuItem>()
				.ToArray();

			if (subMenuItems.Any())
			{
				foreach (var subMenuItem in subMenuItems)
				{
					UpdateMenuVisibility(subMenuItem);
				}

				menuItem.Visible = subMenuItems.Any(mi => mi.Visible);
			}

			return menuItem.Visible;
		}
	}
}


using Enterprise.Core.Modules;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup
{
	public class FavoriteProvider : IFavoriteProvider
	{
		ModuleCategory JumpCategory => ModuleTree.Tree.Categories[ModuleTreeLoaderConstant.Category.Jump.Name];
		ModuleSection FavoritesSection => JumpCategory?.Sections[ModuleTreeLoaderConstant.Section.Favorites.Name];
		ModuleSection RecentItemsSection => JumpCategory?.Sections[ModuleTreeLoaderConstant.Section.RecentItems.Name];

		public bool AddToFavorites(LinkWrapper shortcut)
		{
			var module = new LinkMainFormModule(shortcut);

			if (!RecentItemManager.Instance.IsInFavoriteModules(shortcut))
			{
				if (RecentItemManager.Instance.FavoriteModules.Count >= RecentItemManager.Instance.MaximumNumberOfFavorites)
				{
					Globals.Message.ShowWarning(Res.GetString("5f94df76-63e8-4322-8efd-550a793f6eb4", "Favorites list full. Please remove unwanted items before adding new ones."), Res.GetString("13b1dff7-79d7-46ba-8e5e-a07870884152", "Favorites List Full"));
				}
				else
				{
					if (StartupOpenMainFormTask.MainFormInstance != null)
					{
						StartupOpenMainFormTask.MainFormInstance.NavigationBar.AddModule(JumpCategory, FavoritesSection, module);
					}

					RecentItemManager.Instance.AddToFavoriteModules(shortcut);

					return true;
				}
			}

			return false;
		}

		public void AddToRecentItems(LinkWrapper shortcut)
		{
			var module = new LinkMainFormModule(shortcut);
			if (module != null && module.ModuleID != null)
			{
				if (RecentItemManager.Instance.IsInFavoriteModules(shortcut))
				{
					if (StartupOpenMainFormTask.MainFormInstance != null
						&& StartupOpenMainFormTask.MainFormInstance.NavigationBar != null
						&& FavoritesSection != null)
					{
						StartupOpenMainFormTask.MainFormInstance.NavigationBar.AddOrUpdateModule(JumpCategory, FavoritesSection, module, true);
					}
					RecentItemManager.Instance.UpdateFavoriteModules(shortcut);
				}
				else
				{
					if (StartupOpenMainFormTask.MainFormInstance != null
						&& StartupOpenMainFormTask.MainFormInstance.NavigationBar != null
						&& RecentItemsSection != null)
					{
						StartupOpenMainFormTask.MainFormInstance.NavigationBar.AddOrUpdateModule(JumpCategory, RecentItemsSection, module);
					}
					RecentItemManager.Instance.AddOrUpdateRecentItems(string.Empty, shortcut);
				}

				RecentItemManager.Instance.AddOrUpdateRecentItems(module.ModuleID.Name, shortcut);
			}
		}

		public void RemoveFromRecentItems(LinkWrapper shortcut)
		{
			DeleteFromFavorites(shortcut);

			var module = new LinkMainFormModule(shortcut);
			if (module != null && module.ModuleID != null)
			{
				if (StartupOpenMainFormTask.MainFormInstance != null
					&& StartupOpenMainFormTask.MainFormInstance.NavigationBar != null
					&& RecentItemsSection != null)
				{
					StartupOpenMainFormTask.MainFormInstance.NavigationBar.RemoveModule(JumpCategory, RecentItemsSection, module);
				}
				RecentItemManager.Instance.RemoveFromRecentItems(string.Empty, shortcut);
				RecentItemManager.Instance.RemoveFromRecentItems(module.ModuleID.Name, shortcut);
			}
		}

		public void DeleteFromFavorites(LinkWrapper shortcut)
		{
			var module = new LinkMainFormModule(shortcut);

			if (StartupOpenMainFormTask.MainFormInstance != null && module != null && module.ModuleID != null)
			{
				StartupOpenMainFormTask.MainFormInstance.NavigationBar.RemoveModule(JumpCategory, FavoritesSection, module);
			}

			RecentItemManager.Instance.RemoveFromFavoriteModules(shortcut);
		}

		public bool IsInFavorites(LinkWrapper shortcut)
		{
			return RecentItemManager.Instance.IsInFavoriteModules(shortcut);
		}
	}
}

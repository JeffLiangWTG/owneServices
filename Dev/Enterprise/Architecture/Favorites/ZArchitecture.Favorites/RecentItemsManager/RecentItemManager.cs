using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Favorites
{
	public class RecentItemManager
	{
		public static RecentItemManager Instance
		{
			get { return instance ?? (instance = new RecentItemManager()); }
			set { instance = value; }
		}

		[ThreadStatic]
		static RecentItemManager instance;

		RecentItemManager()
		{
			factory = new BusinessObjectFactory() { NameForDebugging = "Recent Items Manager" };
			factory.SuspendValidation();
			recentItemsByModule = new Dictionary<string, StmLinkCollection>();
		}

		public static void Reset()
		{
			instance = null;
		}

		readonly BusinessObjectFactory factory;

#if DEBUG
		internal BusinessObjectFactory FactoryExposedForTest => factory;
#endif

		#region Favorite Modules & Items

		public int MaximumNumberOfFavorites => RawDataRegistry.Instance.MaximumNumberOfFavorites.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);

		public StmLinkCollection FavoriteModules
		{
			get
			{
				if (favorites == null)
				{
					favorites = new StmLinkCollection(factory, FavoritesFilter, StmLinkConstants.Favorites, MaximumNumberOfFavorites, StmLinkSchema.Constants.STL_ShortcutIndex, System.ComponentModel.ListSortDirection.Ascending);
					if (favorites.CleanUp() > 0)
					{
						Save();
					}
				}

				return favorites;
			}
		}
		StmLinkCollection favorites;

		ZQuery FavoritesFilter => favoritesFilter ?? (favoritesFilter = GetFilter(StmLinkConstants.Favorites));
		ZQuery favoritesFilter;

		public bool AddToFavoriteModules(LinkWrapper shortcut)
		{
			if (!IsInFavoriteModules(shortcut))
			{
				FavoriteModules.AddToCollection(shortcut);
				ReassignShortcuts();
				Save();

				return true;
			}

			return false;
		}

		public void UpdateFavoriteModules(LinkWrapper shortcut)
		{
			var sourceFavorite = FavoriteModules.OfType<StmLink>().FirstOrDefault(f => LinkWrapper.AreShortcutsEqual(shortcut, f));

			if (sourceFavorite != null)
			{
				var wrapperRecordDescription = shortcut.RecordDescription;

				if (wrapperRecordDescription != null && wrapperRecordDescription.Length > sourceFavorite.STL_ItemDescriptionInfo.MaxLength)
				{
					wrapperRecordDescription = wrapperRecordDescription.Substring(0, sourceFavorite.STL_ItemDescriptionInfo.MaxLength - 3) + "...";
				}

				sourceFavorite.STL_ItemDescription = wrapperRecordDescription;
				sourceFavorite.STL_ItemUrl = shortcut.RecordUrl;
				sourceFavorite.STL_LastUsedDateTimeUtc = DateTime.UtcNow;

				Save();
			}
		}

		public bool MoveFavorite(LinkWrapper shortcut, byte moveToIndex)
		{
			var movedSuccessfully = false;

			if (IsInFavoriteModules(shortcut) && moveToIndex > 0 && moveToIndex <= FavoriteModules.Count)
			{
				var sourceFavorite = FavoriteModules.OfType<StmLink>().FirstOrDefault(f => LinkWrapper.AreShortcutsEqual(shortcut, f));
				if (sourceFavorite != null && sourceFavorite.STL_ShortcutIndex != moveToIndex)
				{
					var oldIndex = (byte)sourceFavorite.STL_ShortcutIndex;
					var reorderList = new List<StmLink>();
					var reorderIndex = oldIndex < moveToIndex ? oldIndex : (moveToIndex + 1);
					var lowIndex = oldIndex < moveToIndex ? (oldIndex + 1) : moveToIndex;
					var highIndex = oldIndex < moveToIndex ? (moveToIndex + 1) : oldIndex;

					for (var i = lowIndex; i < highIndex; i++)
					{
						reorderList.Add(FavoriteModules[i - 1]);
					}

					sourceFavorite.STL_ShortcutIndex = moveToIndex;

					foreach (var item in reorderList)
					{
						item.STL_ShortcutIndex = (byte)(reorderIndex++);
					}

					Save();
					movedSuccessfully = true;
				}
			}

			return movedSuccessfully;
		}

		public void RemoveFromFavoriteModules(LinkWrapper shortcut)
		{
			FavoriteModules.RemoveFromCollection(shortcut);
			ReassignShortcuts();
			Save();
		}

		void ReassignShortcuts()
		{
			byte index = 1;
			foreach (var favorite in FavoriteModules.OfType<StmLink>())
			{
				favorite.STL_ShortcutIndex = index++;
			}
		}

		public bool IsInFavoriteModules(LinkWrapper shortcut)
		{
			return FavoriteModules.OfType<StmLink>().Any(t => LinkWrapper.AreShortcutsEqual(shortcut, t));
		}

		public void RemoveAllFavoriteModules()
		{
			FavoriteModules.DeleteAll();

			Save();
		}

		#endregion

		#region Recent Modules

		public int MaximumNumberOfRecentModules => RawDataRegistry.Instance.MaximumNumberOfRecentModules.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);

		public StmLinkCollection RecentModules
		{
			get
			{
				if (recentModules == null)
				{
					recentModules = new StmLinkCollection(factory, RecentModulesFilter, StmLinkConstants.RecentModules, MaximumNumberOfRecentModules, StmLinkSchema.Constants.STL_LastUsedDateTimeUtc, System.ComponentModel.ListSortDirection.Descending);
					if (recentModules.CleanUp() > 0)
					{
						Save();
					}
				}

				return recentModules;
			}
		}
		StmLinkCollection recentModules;

		ZQuery RecentModulesFilter => recentModulesFilter ?? (recentModulesFilter = GetFilter(StmLinkConstants.RecentModules));
		ZQuery recentModulesFilter;

		public void RemoveFromRecentModules(LinkWrapper shortcut)
		{
			RecentModules.RemoveFromCollection(shortcut);

			Save();
		}

		public void AddOrUpdateRecentModules(LinkWrapper shortcut)
		{
			factory.ThreadSentry.EnsureCurrentThreadIsOwner();

			if (IsInRecentModules(shortcut))
			{
				RecentModules.UpdateCollection(shortcut, (l, c) => l.STL_LastUsedDateTimeUtc = DateTime.UtcNow);
			}
			else
			{
				RecentModules.AddToCollection(shortcut);
			}

			Save();
		}

		public bool IsInRecentModules(LinkWrapper shortcut)
		{
			return RecentModules.OfType<StmLink>().Any(t => LinkWrapper.AreShortcutsEqual(shortcut, t));
		}

		public void RemoveAllRecentModules()
		{
			RecentModules.DeleteAll();

			Save();
		}

		#endregion

		#region Recent Items

		public int MaximumNumberOfRecentItems => RawDataRegistry.Instance.MaximumNumberOfRecentItems.GetValueWithoutFallback(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty);

		public StmLinkCollection GetRecentItems(string module)
		{
			if (string.IsNullOrEmpty(module))
			{
				if (recentItems == null)
				{
					recentItems = new StmLinkCollection(factory, RecentItemsFilter, StmLinkConstants.RecentItems, MaximumNumberOfRecentItems, StmLinkSchema.Constants.STL_LastUsedDateTimeUtc, System.ComponentModel.ListSortDirection.Descending);
					if (recentItems.CleanUp() > 0)
					{
						Save();
					}
				}

				return recentItems;
			}

			if (!recentItemsByModule.TryGetValue(module, out var items))
			{
				items = new StmLinkCollection(factory, GetRecentItemsFilter(module), StmLinkConstants.RecentItemsByModule, MaximumNumberOfRecentItems, StmLinkSchema.Constants.STL_LastUsedDateTimeUtc, System.ComponentModel.ListSortDirection.Descending);
				if (items.CleanUp() > 0)
				{
					Save();
				}
				recentItemsByModule.Add(module, items);
			}

			return items;
		}

		public void RemoveFromRecentItems(string module, LinkWrapper shortcut)
		{
			var collection = GetRecentItems(module);
			collection.RemoveFromCollection(shortcut);

			Save();

			lock (eventLock)
			{
				if (!string.IsNullOrEmpty(module))
				{
					RecentItemsChanged?.Invoke(this, new RecentItemsChangedEventArgs(module, shortcut, false));
				}
			}
		}

		public void AddOrUpdateRecentItems(string module, LinkWrapper shortcut)
		{
			var items = GetRecentItems(module);

			RemoveAllOtherShortcutsWithSameDescription(module, shortcut);

			if (IsInRecentItems(items, shortcut))
			{
				items.UpdateCollection(shortcut, (l, c) =>
				{
					string wrapperRecordDescription = null;
					if (shortcut.RecordDescription != null && shortcut.RecordDescription.Length > l.STL_ItemDescriptionInfo.MaxLength)
					{
						wrapperRecordDescription = shortcut.RecordDescription.Substring(0, l.STL_ItemDescriptionInfo.MaxLength - 3) + "...";
					}

					l.STL_ItemDescription = wrapperRecordDescription ?? shortcut.RecordDescription;
					l.STL_LastUsedDateTimeUtc = DateTime.UtcNow;
					l.STL_ItemUrl = shortcut.RecordUrl;
				});
			}
			else
			{
				items.AddToCollection(shortcut);
			}

			Save();

			lock (eventLock)
			{
				if (!string.IsNullOrEmpty(module) && RecentItemsChanged != null)
				{
					RecentItemsChanged(this, new RecentItemsChangedEventArgs(module, shortcut, true));
				}
			}
		}

		public bool IsInRecentItems(string module, LinkWrapper shortcut)
		{
			return GetRecentItems(module).OfType<StmLink>().Any(t => LinkWrapper.AreShortcutsEqual(shortcut, t));
		}

		internal bool IsInRecentItems(StmLinkCollection items, LinkWrapper shortcut)
		{
			return items.OfType<StmLink>().Any(t => LinkWrapper.AreShortcutsEqual(shortcut, t));
		}

		internal void RemoveAllOtherShortcutsWithSameDescription(string module, LinkWrapper shortcut)
		{
			foreach (StmLink item in GetRecentItems(module).OfType<StmLink>().Where(t => t.STL_ItemDescription == shortcut.RecordDescription && !LinkWrapper.AreShortcutsEqual(shortcut, t)).ToArray())
			{
				RemoveFromRecentItems(module, new LinkWrapper(item));
			}
		}

		ZQuery RecentItemsFilter => recentItemsFilter ?? (recentItemsFilter = GetFilter(StmLinkConstants.RecentItems));
		ZQuery recentItemsFilter;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Only ever accessed with a lock.")]
		public static event EventHandler<RecentItemsChangedEventArgs> RecentItemsChanged;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "IS the lock.")]
		static readonly object eventLock = new object();

		public void RemoveAllRecentItems(string module)
		{
			var collection = GetRecentItems(module);
			collection.DeleteAll();

			Save();

			lock (eventLock)
			{
				if (!string.IsNullOrEmpty(module) && RecentItemsChanged != null)
				{
					RecentItemsChanged(this, new RecentItemsChangedEventArgs(module, null, false));
				}
			}
		}

		#endregion

		#region Implementation

		void Save()
		{
			try
			{
				factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, NotificationHandler.Instance, false);
			}
		}

		ZQuery GetFilter(string type)
		{
			var filter = new ZQuery(StmLinkSchema.STL_LinkType, type);
			filter.AddToFilter(JoinCondition.And, StmLinkSchema.STL_GS_NKUser, EnvProxy.Instance.CurrentUser.Initials);
			filter.AddToFilter(JoinCondition.And, StmLinkSchema.STL_GC_LogonCompany, EnvProxy.Instance.CurrentCompany.PK);

			return filter;
		}

		ZQuery GetRecentItemsFilter(string module)
		{
			var filter = GetFilter(StmLinkConstants.RecentItemsByModule);
			filter.AddToFilter(JoinCondition.And, StmLinkSchema.STL_ModuleID, module);

			return filter;
		}

		StmLinkCollection recentItems;
		readonly Dictionary<string, StmLinkCollection> recentItemsByModule;

		#endregion

		#region Test
#if DEBUG

		public void AddToFavoriteForTest(string moduleName)
		{
			AddToFavoriteForTest(new LinkWrapper(moduleName));
		}

		public void AddToFavoriteForTest(LinkWrapper shortcut)
		{
			FavoriteModules.AddToCollectionForTest(shortcut);
			ReassignShortcuts();
			Save();
		}

		public void AddToRecentModulesForTest(LinkWrapper shortcut)
		{
			RecentModules.AddToCollectionForTest(shortcut);
			Save();
		}

		public void AddToRecentItemsForTest(string module, LinkWrapper shortcut)
		{
			var items = GetRecentItems(module);

			items.AddToCollectionForTest(shortcut);
			Save();
		}

#endif
		#endregion
	}

	#region RecentItemsChangedEventArgs

	public class RecentItemsChangedEventArgs : EventArgs
	{
		public RecentItemsChangedEventArgs(string module, LinkWrapper item, bool added)
		{
			Module = module;
			Item = item;
			Added = added;
		}

		public string Module { get; }
		public LinkWrapper Item { get; }
		public bool Added { get; }
	}

	#endregion
}

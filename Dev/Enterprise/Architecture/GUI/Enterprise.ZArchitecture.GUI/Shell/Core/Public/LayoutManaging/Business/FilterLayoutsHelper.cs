using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Internal;
using WTG.StaticAnalysis.Annotation;
using static Enterprise.ZArchitecture.GUI.StripControl;

namespace Enterprise.ZArchitecture.GUI
{
	public static class FilterLayoutsHelper
	{
		public static readonly MultilingualString MyFilterLayoutsText = ResString.GetMultilingualString("0e4a2524-732c-43d0-ab0c-1e0f0bd391bf", "My Filter Layouts");
		public static readonly MultilingualString SharedFilterLayoutsText = ResString.GetMultilingualString("2fae106e-0b54-4bb8-b540-9f93dba616db", "Shared Filter Layouts");
		public static readonly MultilingualString FavoriteFilterLayoutsText = ResString.GetMultilingualString("492AE2A3-065D-4D72-9C7C-C79D4AD715A4", "Favorites");

		public static List<ZFilterToolStripMenuItem> CalculateTree(IEnumerable<StmModuleFilter> items, bool isFavorite, Action<object, EventArgs> eventName = null, Dictionary<ZGuid, string> layoutsRenamed = null)
		{
			string GetPath(StmModuleFilter filter)
			{
				var path = layoutsRenamed?.TryGetValue(filter.PK, out var names) ?? false ? names : filter.S9_FilterNameMultilingual.ToString();
				return string.Join("/", path.Split('/').Select(p => p.Trim()));
			}

			List<(StmModuleFilter filter, string menuPath)> itemsWithName = items.Select(filter => (filter, GetPath(filter))).ToList();
			itemsWithName.Sort((p1, p2) =>
			{
				var r = p1.filter.S9_IsPublished.CompareTo(p2.filter.S9_IsPublished);
				r = r == 0 ? StringComparer.OrdinalIgnoreCase.Compare(p1.menuPath, p2.menuPath) : r;
				return r;
			});

			var res = new List<ZFilterToolStripMenuItem>();
			var lastMenuItemTreeCreated = new List<(string menuName, ZFilterToolStripMenuItem menuItem)>();
			foreach (var (filter, menuPath) in itemsWithName)
			{
				var split = menuPath.Split('/');
				PopulateTree(0);
				void PopulateTree(int depth)
				{
					var name = split[depth];

					if (lastMenuItemTreeCreated.Count > depth && StringComparer.OrdinalIgnoreCase.Equals(name, lastMenuItemTreeCreated[depth].menuName))
					{
						// Do nothing. We already created this menu item
					}
					else
					{
						if (lastMenuItemTreeCreated.Count > 0)
						{
							lastMenuItemTreeCreated.RemoveRange(depth, lastMenuItemTreeCreated.Count - depth);
						}

						var item = CreateMenuItemFromFilter(filter, name, split.Length == depth + 1, eventName, isFavorite);
						if (lastMenuItemTreeCreated.Count == 0)
						{
							res.Add(item);
						}
						else
						{
							lastMenuItemTreeCreated[lastMenuItemTreeCreated.Count - 1].menuItem.DropDownItems.Add(item);
						}
						lastMenuItemTreeCreated.Add((name, item));
					}

					if (depth + 1 < split.Length)
					{
						PopulateTree(depth + 1);
					}
				}
			}
			return res;
		}

		internal static List<ZFilterToolStripMenuItem> InsertInSubTree(List<ZFilterToolStripMenuItem> tree, StmModuleFilter filter, Stack<string> names, bool isFavorite, Action<object, EventArgs> eventName = null)
		{
			var currentLevelUntrimmed = names.Pop();
			var currentLevel = currentLevelUntrimmed.Trim();
			var item = FindItemInListByText(tree, currentLevel, filter.S9_IsPublished);
			var isLastLevel = names.Count == 0;

			if (item == null)
			{
				tree.Insert(GetNewItemInsertIndex(tree, filter, currentLevel), CreateMenuItemFromFilter(filter, currentLevel, isLastLevel, eventName, isFavorite));
			}
			else if (isLastLevel)
			{
				UpdateMenuItemFromFilter(item, filter, true, eventName);
			}

			if (!isLastLevel)
			{
				var dropdownItems = FindItemInListByText(tree, currentLevel, filter.S9_IsPublished).DropDownItems.Cast<ZFilterToolStripMenuItem>().ToList();
				var subTree = InsertInSubTree(dropdownItems, filter, names, isFavorite, eventName);

				FindItemInListByText(tree, currentLevelUntrimmed, filter.S9_IsPublished)?.DropDownItems.AddRange(subTree.ToArray());
			}

			return tree;
		}

		internal static ZFilterToolStripMenuItem FindItemInListByText(List<ZFilterToolStripMenuItem> list, string text, bool isPublished)
		{
			return list.FirstOrDefault(x => x.IsPublished == isPublished && String.Compare(x.Text.Trim(), text.Trim(), StringComparison.OrdinalIgnoreCase) == 0 || String.Compare(x.Text.Trim(), text.Trim() + " [+]", StringComparison.OrdinalIgnoreCase) == 0);
		}

		internal static int GetNewItemInsertIndex(List<ZFilterToolStripMenuItem> list, StmModuleFilter filter, string nameAtCurrentLevel)
		{
			int i;

			for (i = 0; i < list.Count; i++)
			{
				if (!list[i].IsFavorite && list[i].IsPublished && !filter.S9_IsPublished)
				{
					break;
				}

				var itemName = list[i].Text.Trim();
				var shouldFilterNameBeBeforeItemName = String.Compare(nameAtCurrentLevel, itemName, StringComparison.OrdinalIgnoreCase) <= 0;
				var isItemNameAFilter = itemName != MyFilterLayoutsText && itemName != SharedFilterLayoutsText;

				if (!list[i].IsFavorite && list[i].IsPublished == filter.S9_IsPublished && shouldFilterNameBeBeforeItemName && isItemNameAFilter)
				{
					break;
				}
			}

			return i;
		}

		internal static ZFilterToolStripMenuItem CreateMenuItemFromFilter(StmModuleFilter filter, ZString displayText, bool isAction, Action<object, EventArgs> eventName, bool isFavorite)
		{
			var item = new ZFilterToolStripMenuItem(StmModuleFilter.GetDisplayName(displayText, filter.IsUserDefinedFilter, isAction), filter.S9_IsPublished, isFavorite, isAction);
			item.Font = ZFilterStripBaseControl.UnselectedFindListItemFont; // Pre-setting the font to the font it usually is in order to avoid re-rendering later.

			return UpdateMenuItemFromFilter(item, filter, isAction, eventName);
		}

		internal static ZFilterToolStripMenuItem UpdateMenuItemFromFilter(ZFilterToolStripMenuItem item, StmModuleFilter filter, bool isAction, Action<object, EventArgs> eventName)
		{
			try
			{
				if (isAction)
				{
					if (item.IsFavorite)
					{
						item.Image = FavoriteSelected;
					}
					else
					{
						item.Image = FavoriteUnselected;
					}
					item.ToolTipText = GetToolTipText(item, filter);
					item.Tag = filter;
					if (eventName != null)
					{
						item.Click += new EventHandler(eventName);
					}
				}
			}
			catch
			{
				item.Dispose(); //This satisfies CA2000
				throw;
			}

			return item;
		}

		public static string GetToolTipText(ZFilterToolStripMenuItem item, StmModuleFilter filter)
		{
			if (filter.S9_IsSystem)
			{
				return Res.GetString("FilterStripControl|TheFilterLayoutShipsWithProduct", "The filter layout {0} ships with {1} and is published system-wide.", item.Text.Trim(), Enterprise.Core.Constants.ProductName);
			}
			else if (filter.S9_IsPublished)
			{
				return Res.GetString("FilterStripControl|TheFilterLayoutIsPublishedSystemwide", "The filter layout '{0}' is published system-wide.", item.Text.Trim());
			}
			else
			{
				return Res.GetString("FilterStripControl|ThisIsYourPrivateFilterLayout", "This is your private filter layout '{0}'", filter.S9_FilterName.Trim());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible")]
		[ThreadSafe]
		public static Image FavoriteUnselected = ZFilterControlImages.FilterImageList.Images["FavoriteUnselected"];
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible")]
		[ThreadSafe]
		public static Image FavoriteSelected = ZFilterControlImages.FilterImageList.Images["FavoriteSelected"];
	}
}

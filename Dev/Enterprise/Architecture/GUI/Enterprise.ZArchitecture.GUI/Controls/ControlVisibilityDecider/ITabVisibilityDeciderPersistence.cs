using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// Provides persistence for dynamic control visibility. If implemented, certain control visibility
	/// settings will be persisted.
	/// </summary>
	public interface ITabVisibilityDeciderPersistence
	{
		void StoreTabVisible(ZTabPage page);
		bool RetrieveTabPageVisible(ZTabPage page);
		bool HasTabVisiblePersisted { get; }
	}

	public static class TabVisibilityPersistenceExtensions
	{
		/// <summary>
		/// Retrieves tab visibility settings from a host business object.
		/// </summary>
		public static bool RetrieveTabPageVisible(this ITabVisibilityDeciderPersistence tabVisibilityDeciderPersistence, ZTabPage page, BusinessObject host, SchemaStringColumn persistenceColumn)
		{
			return RetrieveTabPageVisibleCore(tabVisibilityDeciderPersistence, page, host, persistenceColumn);
		}

		/// <summary>
		/// Retrieves tab visibility settings from a host business object.
		/// </summary>
		public static bool RetrieveTabPageVisible(this ITabVisibilityDeciderPersistence tabVisibilityDeciderPersistence, ZTabPage page, BusinessObject host, SchemaXmlColumn persistenceColumn)
		{
			return RetrieveTabPageVisibleCore(tabVisibilityDeciderPersistence, page, host, persistenceColumn);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1801:ReviewUnusedParameters")]
		static bool RetrieveTabPageVisibleCore(this ITabVisibilityDeciderPersistence tabVisibilityDeciderPersistence, ZTabPage page, BusinessObject host, SchemaColumn persistenceColumn)
		{
			var pageIdentifier = GetTabPageIdentifier(page);
			if (!string.IsNullOrEmpty(pageIdentifier))
			{
				var invisibleColumnsXml = (ZString)host[persistenceColumn];
				if (!invisibleColumnsXml.IsEmpty)
				{
					return invisibleColumnsXml.ToString().IndexOf(TabSeparator + pageIdentifier + TabSeparator, StringComparison.Ordinal) < 0;
				}
			}

			return true;
		}

		/// <summary>
		/// Sets tab visibility settings for an individual tab on a host business object.
		/// </summary>
		public static void StoreTabVisible(this ITabVisibilityDeciderPersistence tabVisibilityDeciderPersistence, ZTabPage tabPage, BusinessObject host, SchemaStringColumn persistenceColumn)
		{
			StoreTabVisibleCore(tabVisibilityDeciderPersistence, tabPage, host, persistenceColumn);
		}

		/// <summary>
		/// Sets tab visibility settings for an individual tab on a host business object.
		/// </summary>
		public static void StoreTabVisible(this ITabVisibilityDeciderPersistence tabVisibilityDeciderPersistence, ZTabPage tabPage, BusinessObject host, SchemaXmlColumn persistenceColumn)
		{
			StoreTabVisibleCore(tabVisibilityDeciderPersistence, tabPage, host, persistenceColumn);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1801:ReviewUnusedParameters")]
		static void StoreTabVisibleCore(this ITabVisibilityDeciderPersistence tabVisibilityDeciderPersistence, ZTabPage tabPage, BusinessObject host, SchemaColumn persistenceColumn)
		{
			var exisitingXml = (ZString)host[persistenceColumn];
			var tabIdentifier = GetTabPageIdentifier(tabPage);

			if (exisitingXml.IsEmpty)
			{
				if (!tabPage.TabVisible)
				{
					host[persistenceColumn] = InvisibleTabsOpenTag + TabSeparator + tabIdentifier + TabSeparator + InvisibleTabsCloseTag;
				}
			}
			else
			{
				var alreadyInvisibleTabs = exisitingXml.ToString()
					.Replace(InvisibleTabsOpenTag, string.Empty)
					.Replace(InvisibleTabsCloseTag, string.Empty)
					.Split(new[] { TabSeparator }, StringSplitOptions.RemoveEmptyEntries);

				var isTabAlreadyInvisible = alreadyInvisibleTabs.Contains(tabIdentifier);

				if (tabPage.TabVisible && isTabAlreadyInvisible)
				{
					SetInvisibleTabs(host, persistenceColumn, alreadyInvisibleTabs.Where(x => x != tabIdentifier));
				}
				else if (!tabPage.TabVisible && !isTabAlreadyInvisible)
				{
					SetInvisibleTabs(host, persistenceColumn, alreadyInvisibleTabs.Concat(new[] { tabIdentifier }));
				}
			}
		}

		static void SetInvisibleTabs(BusinessObject host, SchemaColumn persistenceColumn, IEnumerable<string> invisibleTabs)
		{
			var invisibleTabsText = invisibleTabs.Any() ? TabSeparator + string.Join(TabSeparator, invisibleTabs) + TabSeparator : "";

			if (!((IZType)host[persistenceColumn]).IsEmpty || !string.IsNullOrEmpty(invisibleTabsText))
			{
				host[persistenceColumn] = InvisibleTabsOpenTag + invisibleTabsText + InvisibleTabsCloseTag;
			}
		}

		public static string GetTabPageIdentifier(ZTabPage tab)
		{
			var result = tab.Name;
			if (string.IsNullOrEmpty(result))
			{
				result = tab.Text;
			}
			return result;
		}

		public const string InvisibleTabsOpenTag = @"<InvisibleTabs>"; // XML tag constant
		public const string InvisibleTabsCloseTag = @"</InvisibleTabs>"; // XML tag constant
		public const string TabSeparator = ";";
	}
}

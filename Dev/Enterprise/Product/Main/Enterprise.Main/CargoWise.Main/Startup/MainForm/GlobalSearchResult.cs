using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Main.Navigation;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.SearchBox;
using MenuItem = CargoWise.Main.Navigation.MenuItem;

namespace Enterprise.Startup;

public interface IGlobalSearchResult
{
	string SearchValue { get; }
	string ErrorStatus { get; }
	string ErrorMessage { get; }
	bool IsError { get; }
	IEnumerable<IGlobalSearchResultGroup> Groups { get; }
}

public interface IGlobalSearchResultGroup
{
	MultilingualString GroupName { get; }
	string EntityType { get; }
	IEnumerable<IGlobalSearchResultItem> Items { get; }
}

public interface IGlobalSearchResultItem
{
	string Key { get; }
	string EntityType { get; }
	IEnumerable<string> Data { get; }
	Action SelectAction { get; }
}

public record GlobalSearchResult : IGlobalSearchResult
{
	public string SearchValue { get; init; }
	public string ErrorStatus { get; init; }
	public string ErrorMessage { get; init; }
	public bool IsError { get; init; }
	public bool HasWarning => !string.IsNullOrEmpty(WarningMessage);
	public string WarningMessage { get; init; }

	public IEnumerable<IGlobalSearchResultGroup> Groups { get; init; }

	internal static GlobalSearchResult Create(string searchValue, IEnumerable<IGlobalSearchResultGroup> results, string warningMessage = null)
	{
		return new GlobalSearchResult
		{
			SearchValue = searchValue,
			ErrorStatus = string.Empty,
			ErrorMessage = string.Empty,
			IsError = false,
			Groups = results,
			WarningMessage = warningMessage,
		};
	}

	internal static GlobalSearchResult Empty()
	{
		return new GlobalSearchResult
		{
			SearchValue = string.Empty,
			ErrorStatus = string.Empty,
			ErrorMessage = string.Empty,
			IsError = false,
			Groups = Enumerable.Empty<IGlobalSearchResultGroup>()
		};
	}

	internal static GlobalSearchResult Error(string errorStatus, string errorMessage)
	{
		return new GlobalSearchResult
		{
			SearchValue = string.Empty,
			ErrorStatus = errorStatus,
			ErrorMessage = errorMessage,
			IsError = true,
			Groups = Enumerable.Empty<IGlobalSearchResultGroup>()
		};
	}
}

public record GlobalSearchResultGroup : IGlobalSearchResultGroup
{
	public MultilingualString GroupName { get; init; }
	public string EntityType { get; init; }
	public IEnumerable<IGlobalSearchResultItem> Items { get; init; }
}

public record GlobalSearchResultItem : IGlobalSearchResultItem
{
	/// <summary>
	/// aka: PK for the entity
	/// </summary>
	public string Key { get; init; }
	/// <summary>
	/// eg. IGlbStaff, IGlbCompany, etc.
	/// </summary>
	public string EntityType { get; init; }
	public IEnumerable<string> Data { get; init; }
	public Action SelectAction { get; init; }

	internal static IGlobalSearchResultItem CreateSearchItem(
		string key,
		string entityType,
		Action clickAction,
		params string[] data)
	{
		return new GlobalSearchResultItem
		{
			Key = key,
			EntityType = entityType,
			Data = data,
			SelectAction = clickAction,
		};
	}
}

public static class GlobalSearchResultExtensions
{
	public static IEnumerable<IDisplayItem> ToSearchBoxDisplayItems(this GlobalSearchResult result)
	{
		if (result.IsError)
		{
			return new IDisplayItem[]
			{
				DisplayItemFactory.CreateErrorItem(result.ErrorStatus, result.ErrorMessage)
			};
		}

		List<IDisplayItem> displayItems = [];
		if (result.HasWarning)
		{
			displayItems.Add(
				DisplayItemFactory.CreateErrorItem(result.WarningMessage.SplitByLine().ToArray()));
		}

		foreach (var group in result.Groups)
		{
			displayItems.Add(
				DisplayItemFactory.CreateHeadingItem(group.GroupName));

			foreach (var item in group.Items)
			{
				displayItems.Add(
					item.ToSearchBoxDisplayItem());
			}
		}
		return displayItems;
	}

	public static IDisplayItem ToSearchBoxDisplayItem(this IGlobalSearchResultItem item)
	{
		return DisplayItemFactory.CreateSearchItem
		(
			executeAction: item.SelectAction,
			data: item.Data.ToArray()
		);
	}

	public static MenuItem ToMenuItem(this IGlobalSearchResultItem item)
	{
		var text = string.Join(" - ", item.Data.ToArray());
		return new MenuItem
		(
			key: item.Key,
			multilingualString: (NoResString)text,
			action: item.SelectAction
		);
	}

	public static IEnumerable<SearchResultSection> ToSearchSections(this IGlobalSearchResult result)
	{
		if (result.IsError)
		{
			return Enumerable.Empty<SearchResultSection>();
		}

		return result.Groups.Select(g => g.ToSearchSection());
	}

	public static SearchResultSection ToSearchSection(this IGlobalSearchResultGroup group)
	{
		return new SearchResultSection
		(
			displayName: group.GroupName,
			name: group.EntityType,
			items: group.Items.Select(i => i.ToMenuItem()),
			sectionType: SectionType.GlobalSearch
		);
	}
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Billing.Integration;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using GlowIndexQueryService.Business;
using Constants = GlowIndexQueryService.Business.Constants;
using Res = CargoWise.Main.Res;

namespace Enterprise.Startup;

public class GlobalSearch
{
	public GlobalSearch(Action<ModuleOpenerInfo> moduleOpener)
	{
		GlowIndexQueryEngine = ObjectFactory.Get<IGlowIndexQueryEngine>();
		ModuleOpener = moduleOpener;
	}

	public Action<ModuleOpenerInfo> ModuleOpener;
	public IGlowIndexQueryEngine GlowIndexQueryEngine { get; set; }
	static Dictionary<Type, ZController> ZControllerMap => zControllerMap ?? (zControllerMap = new Dictionary<Type, ZController>());

	[ThreadStatic]
	static Dictionary<Type, ZController> zControllerMap;

	public GlobalSearchResult Search(string searchVal) => SearchCore(searchVal, GlowIndexQueryEngine, ModuleOpener);

	static GlobalSearchResult SearchCore(string searchValue, IGlowIndexQueryEngine searchEngine, Action<ModuleOpenerInfo> moduleOpener)
	{
		if (searchValue == null || searchEngine == null || moduleOpener == null)
		{
			ErrorReporter.ReportOnce("SearchCore_ParamtersAreNull", $"{searchValue == null} {searchEngine == null} {moduleOpener == null}");
			return GlobalSearchResult.Empty();
		}

		if (GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.Value)
		{
			var searchResults = searchEngine.Query(new GlowIndexQueryParam(searchValue, Constants.LUCENE_SEARCH_FIELD_COMMON));

			if (searchResults == null)
			{
				ErrorReporter.ReportOnce("SearchCore_ResultsAreNull", $"This should never be null. SearchValue={searchValue}");
				return GlobalSearchResult.Empty();
			}

			if (searchResults.Status == GlowIndexQueryStatus.Success)
			{
				try
				{
					var results = GlowIndexQueryResultsToDisplayItems(searchResults.Results, moduleOpener, searchValue.Split(null));
					return GlobalSearchResult.Create(searchValue, results, string.IsNullOrWhiteSpace(searchResults.WarningMessage) ? null : searchResults.WarningMessage);
				}
				catch (NullReferenceException nre)
				{
					ErrorReporter.ReportOnce("GlowIndexQueryResultsToDisplayItems_NullRefCaught", "", nre);
				}
			}
			else
			{
				return GlobalSearchResult.Error(searchResults.Status.ToString(), searchResults.ErrorMessage);
			}
		}

		return GlobalSearchResult.Empty();
	}

	internal static IEnumerable<IGlobalSearchResultGroup> GlowIndexQueryResultsToDisplayItems(IEnumerable<GlowIndexQueryResult> searchResults, Action<ModuleOpenerInfo> moduleOpener, params string[] searchKeywords)
	{
		if (searchKeywords == null || searchResults == null || moduleOpener == null)
		{
			ErrorReporter.ReportOnce("GlowIndexQueryResultsToDisplayItems_ParamtersAreNull", $"{searchKeywords == null} {searchResults == null} {moduleOpener == null}");
			return Enumerable.Empty<IGlobalSearchResultGroup>();
		}

		var displayItemCategories = new Dictionary<string, IList<IGlobalSearchResultItem>>();
		foreach (var result in searchResults)
		{
			if (GlowIndexQueryResultIsNull(result))
			{
				ErrorReporter.ReportOnce("GlowIndexQueryResultsToDisplayItems_ResultWasNull", "");
				continue;
			}

			var displayItem = GlowEntityToDisplayItem(result, moduleOpener, searchKeywords);
			if (displayItem == null)
			{
				continue;
			}

			if (!displayItemCategories.TryGetValue(result.EntityType, out var value))
			{
				displayItemCategories[result.EntityType] = value = new List<IGlobalSearchResultItem>();
			}

			if (value == null)
			{
				ErrorReporter.ReportOnce("GlowIndexQueryResultsToDisplayItems_DictionaryValueWasNull", "");
				continue;
			}

			value.Add(displayItem);
		}

		return DictionaryToEnumerable(displayItemCategories);
	}

	internal static IEnumerable<IGlobalSearchResultGroup> DictionaryToEnumerable(Dictionary<string, IList<IGlobalSearchResultItem>> displayItems)
	{
		if (displayItems == null || displayItems.Count == 0)
		{
			return Enumerable.Empty<IGlobalSearchResultGroup>();
		}

		var results = new List<IGlobalSearchResultGroup>();
		foreach (var kv in displayItems)
		{
			var entityType = kv.Key;
			if (entityType == null || kv.Value == null || kv.Value.Count <= 0)
			{
				continue;
			}

			var groupHeader = GetHumanReadableEntityName(entityType);

			var resultItems = kv.Value
				.Where(i => i != null && i.Data != null)
				.OrderByDescending(j => j.Data.Count());
			results.Add(new GlobalSearchResultGroup
			{
				EntityType = entityType,
				GroupName = groupHeader,
				Items = resultItems
			});
		}
		return results;
	}
	/// <summary>
	/// Key: EntityType
	/// Value: Human Readable Name
	/// </summary>
	readonly static ConcurrentDictionary<string, MultilingualString> EntityToReadableNamesCache = new ConcurrentDictionary<string, MultilingualString>();

	/// <param name="entityType">eg. IWorkItem, IIncidentRequest</param>
	/// <returns>eg. Work Item, </returns>
	internal static MultilingualString GetHumanReadableEntityName(string entityType)
	{
		return EntityToReadableNamesCache.GetOrAdd(entityType, GetHumanReadableEntityNameImp);
		static MultilingualString GetHumanReadableEntityNameImp(string entityType)
		{
			var prefix = GlowModuleToCW1ModuleConverter.ConvertEntityTypeToTableCode(entityType);
			if (string.IsNullOrEmpty(prefix))
			{
				return (NoResString)entityType;
			}
			var moduleIdentifier = ZModuleFactory.Instance.GetRegisteredIdentifiersByColumnNamePrefix(prefix)?.FirstOrDefault();
			return moduleIdentifier?.ExtendedDescription ?? (NoResString)entityType;
		}
	}

	// this method is not called when the search runs, it is called when the menu item is clicked
	internal static ModuleOpenerInfo GlowEntityToModuleOpenerInfo(GlowIndexQueryResult result, string matchValue = null, BusinessObjectFactory factory = null)
	{
		var bizoPk = result?.PK;
		if (GlowIndexQueryResultIsNull(result))
		{
			ErrorReporter.ReportOnce("GlowEntityToDisplayItem_ArgumentsWereNull", "Param GlowIndexQueryResult can not be null");
			return null;
		}

		var bizo = GlowModuleToCW1ModuleConverter.ConvertEntityToBizo(result.PK, result.EntityType, factory);

		if (bizo is IGlobalSearchBusinessObjectProvider provider)
		{
			bizo = provider.BusinessObjectForController;
			bizoPk = bizo?.PK.ToString();
		}

		if (bizo == null)
		{
			// we care about this case so we'd like info for the future
			Globals.Message.ShowError(Res.GetString("3EB47616-D164-47D8-9BA5-8AF81247BDEA", "Entity {0}, {1} Not Found. Please try it later.", result.EntityType, matchValue));
			return null;
		}

		var bizoType = bizo.GetType();
		if (!ZControllerMap.TryGetValue(bizoType, out var controller))
		{
			var countryCode = "";
			if (bizo is BaseJobDeclaration jobDeclaration)
			{
				var company = factory.Load<GlbCompany>(jobDeclaration.JE_GC);
				countryCode = company?.GC_RN_NKCountryCode ?? "";
			}
			controller = ZControllerFactory.Instance.GetControllerForTypeOrItsBaseTypes(bizoType, countryCode: countryCode);
			ZControllerMap[bizoType] = controller;
		}

		if (controller == null || controller.ModuleID == null || controller.ModuleID == ModuleIDs.NotAssigned)
		{
			ErrorReporter.ReportOnce("GlowEntityToDisplayItem_ArgumentsWereNull", $"Module not found. PK={bizoPk}, EntityType={result.EntityType}, BizoType={bizoType.FullName}, Controller={controller}");
			return null;
		}

		return new ModuleOpenerInfo(controller.ID, controller.ModuleID, bizoPk);
	}

	internal static IGlobalSearchResultItem GlowEntityToDisplayItem(GlowIndexQueryResult result, Action<ModuleOpenerInfo> moduleOpener, params string[] searchKeywords)
	{
		bool IsMatch(string value, string keyword) => !string.IsNullOrEmpty(keyword) && value.Contains(keyword, StringComparison.OrdinalIgnoreCase);

		if (searchKeywords == null || GlowIndexQueryResultIsNull(result) || moduleOpener == null)
		{
			var debugString = $"{searchKeywords == null}_{GlowIndexQueryResultIsNull(result)}_{moduleOpener == null}";
			ErrorReporter.ReportOnce("GlowEntityToDisplayItem_ArgumentsWereNull", debugString);
			return null;
		}

		var keyFieldsMatching = result.KeyFields.Where(kf => searchKeywords.Any(keyword => IsMatch(kf.Value, keyword))).ToArray();
		if (!keyFieldsMatching.Any())
		{
			// this can happen if glow returns some weird object that shouldn't be searchable, or
			// this can happen due to async search - returned results are for an older search term
			return null;
		}

		var matchValue = FormattableString.Invariant($"{keyFieldsMatching.First().Key} - {keyFieldsMatching.First().Value}");
		var allFields = result.KeyFields
		.Select(kv => FormattableString.Invariant($"{kv.Key} - {kv.Value}"))
		.OrderBy(kf => kf)
		.ToArray();
		string[] displayFields;

		if (Enum.TryParse(result.EntityType, out EntityType entityType))
		{
			displayFields = SelectEntityData(entityType, allFields);
			displayFields = displayFields
			.Select(f => f.Contains("-") ? f.Substring(f.IndexOf('-') + 1).Trim() : f)
			.ToArray();
		}
		else
		{
			displayFields = keyFieldsMatching
					.Select(kv => FormattableString.Invariant($"{kv.Key} - {kv.Value}"))
					.OrderBy(kf => kf)
					.ToArray();
		}

		var clickAction = new Action(() => ProcessModuleOpen(result, matchValue, moduleOpener));
		var item = GlobalSearchResultItem.CreateSearchItem(
			result.PK,
			result.EntityType,
			clickAction,
			displayFields
		);

		if (item == null)
		{
			ErrorReporter.ReportOnce("GlowEntityToDisplayItems_DisplayItemWasNull", "");
		}

		return item;
	}

	public static readonly Dictionary<EntityType, Func<string[], string[]>> EntityFilterMap = new Dictionary<EntityType, Func<string[], string[]>>
	{
		{ EntityType.IWorkItem, allFields => allFields.Where(f => f.Contains("JOBNUMBER") || f.Contains("SUMMARY")).ToArray() },
		{ EntityType.IBMNCNShape, allFields => allFields.Where(f => f.Contains("NAME")).ToArray() },
		{ EntityType.IGlbStaff, allFields => allFields.Where(f => f.Contains("CODE") || f.Contains("FULLNAME")).ToArray() },
		{ EntityType.IGlbDepartment, allFields => allFields.Where(f => f.Contains("CODE") || f.Contains("DESCRIPTION")).ToArray() },
		{ EntityType.IGlbBranch, allFields => allFields.Where(f => f.Contains("CODE") || f.Contains("BRANCHNAME")).ToArray() },
		{ EntityType.IOrgHeader, allFields => allFields.Where(f => f.Contains("CODE") || f.Contains("NAME")).ToArray() },
		{ EntityType.IIncidentRequest, allFields => allFields.Where(f => f.Contains("NUMBER") || f.Contains("SUMMARY")).ToArray() }
	};

	public static string[] SelectEntityData(EntityType entityType, string[] allFields)
	{
		if (EntityFilterMap.TryGetValue(entityType, out var filter))
		{
			return filter(allFields);
		}
		else
		{
			throw new ArgumentOutOfRangeException(nameof(entityType), $"No filter defined for entity type {entityType}.");
		}
	}

	internal static void ProcessModuleOpen(GlowIndexQueryResult result, string matchValue, Action<ModuleOpenerInfo> moduleOpener)
	{
		using (new CursorSwitcher(Cursors.WaitCursor))
		{
			var openerInfo = GlowEntityToModuleOpenerInfo(result, matchValue, new BusinessObjectFactory() { NameForDebugging = "Glow Entity To Module Id" });
			if (openerInfo?.ModuleId == null)
			{
				return;
			}

			moduleOpener(openerInfo);

			ObjectFactory.Get<ISearchPerformedUsageCollector>()?.Report(
				SearchPerformedType.GlobalSearch,
				$"{openerInfo.ModuleId.ID}({openerInfo.ModuleId.Description.GetLocalizedValue(Enterprise.Core.Constants.Languages.English)})",
				string.Empty);
		}
	}

	internal static bool GlowIndexQueryResultIsNull(GlowIndexQueryResult giqr) => giqr == null || giqr.EntityType == null || giqr.KeyFields == null || giqr.PK == null;
}

public class ModuleOpenerInfo
{
	public ModuleOpenerInfo(ControllerID controllerId, ModuleIdentifier moduleId, string bizoPk)
	{
		ControllerId = controllerId;
		BizoPk = bizoPk;
		ModuleId = moduleId;
	}
	public ControllerID ControllerId { get; }
	public string BizoPk { get; }
	public ModuleIdentifier ModuleId { get; }
}

public enum EntityType
{
	IWorkItem,
	IBMNCNShape,
	IGlbStaff,
	IGlbBranch,
	IGlbDepartment,
	IOrgHeader,
	IIncidentRequest
}

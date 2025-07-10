using System;

namespace GlowIndexQueryService.Business
{
	public static class Constants
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "API string")]
		const string ODATA4_PREFIX = "odata";
		public const string BASE_URL = ODATA4_PREFIX + "/" + "Index"; // API string

		public const string LUCENE_ENTITY_INFOS = "EntityInfos"; // API string
		public const string LUCENE_ENTITY_TYPES = "EntityTypes"; // API string
		public const string LUCENE_ENTITY_CATEGORIES = "EntityCategories"; // API string
		public const string LUCENE_SEARCH_FIELDS = "SearchFields"; // API string
		public const string LUCENE_SEARCH_GET_FIELDS_BY_TYPE = "GetSearchFieldsByType(entityType=@entityType)"; // API string

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Uri")]
		public const string CAPTION_SERVICE = "captionservice/captiondata/HTML?languageCode=";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Uri")]
		public const string LOOKUP_SERVICE = "api/indexLookups/";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Uri")]
		public const string LOOKUP_RULE_GET_SERVICE = "api/rules/IEntityInfo";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "API string")]
		public const string LUCENE_SEARCH_FIELD_COMMON = "Common";

		public const string ODATA_NEXTLINK = "@odata.nextLink";
		public const string ODATA_COUNT = "@odata.count";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Http Header")]
		public const string ODATA_PREFER_HEADER_NAME = "prefer";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Http Header")]
		public const string ODATA_PREFER_HEADER_VALUE = "odata.include-annotations=\"*\"";

		public const int MINIMUM_QUERY_RESULTS_RETURNED = 0;
		public const int DEFAULT_QUERY_RESULTS_RETURNED = 50; // page size in glow is defined here: https://devops.wisetechglobal.com/wtg/Glow/_git/Glow?path=%2FDotNet%2FService%2FCommon%2FService%2FglowService.yaml&version=GBmaster&_a=contents
		public const int MAXIMUM_QUERY_RESULTS_RETURNED = 1000;

		public const int QUERY_COUNT_OMITTED = -1;

		public static readonly TimeSpan MAXIMUM_UPDATE_TIME = TimeSpan.FromMinutes(20);
	}
}

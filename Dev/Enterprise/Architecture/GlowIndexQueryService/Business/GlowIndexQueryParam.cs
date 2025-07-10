using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using CargoWise.Common;

namespace GlowIndexQueryService.Business
{
	public class GlowIndexQueryParam
	{
		public string EntityType { get; }
#if DEBUG
		virtual
#endif
		public string CategoryType { get; }
		public int MaxQueryResults { get; set; } = Constants.DEFAULT_QUERY_RESULTS_RETURNED;
		public bool IncludeCount { get; set; }
		public bool OnlyIncludePk { get; set; }

		public List<IGlowQuery> GlowQueries { get; }
		public string SearchFieldForEntityType { get; }
		public string[] SearchFieldsForCategory { get; }
		public string Keyword { get; }

		public GlowIndexQueryParam(string keyword, string[] searchFieldsForCategory, string categoryType,
			int maxQueryResults = Constants.DEFAULT_QUERY_RESULTS_RETURNED,
			bool includeCount = false, bool onlyIncludePk = false
			)
		{
			_ = Argument.NotNull(keyword, nameof(keyword));
			_ = Argument.NotNull(searchFieldsForCategory, nameof(searchFieldsForCategory));
			_ = Argument.NotNull(categoryType, nameof(categoryType));

			// when search by category, the query looks like this
			// e.g. odata/Index/EntityInfos?$filter=(Category eq 'APA') and ((JOBNUMBER eq 'S00001001') or (MASTERBILLNUMBER eq 'S00001001') or (HOUSEBILLNUMBER eq 'S00001001') or (CONTAINERNUMBER eq 'S00001001'))
			// so we only have 1 keyword -  e.g. S00001001
			// but we have multiple search Fields, i.e, JOBNUMBER. MASTERBILLNUMBER, HOUSEBILLNUMBER, CONTAINERNUMBER
			GlowQueries = searchFieldsForCategory
				.Where(t => !string.IsNullOrWhiteSpace(t))
				.Select(sf => new Term(sf, keyword))
				.Select(term => new EqualQuery(term))
				.ToList<IGlowQuery>();

			CategoryType = categoryType;
			SearchFieldsForCategory = searchFieldsForCategory;
			MaxQueryResults = maxQueryResults;
			IncludeCount = includeCount;
			OnlyIncludePk = onlyIncludePk;

			CheckQueryCount();
		}

		public GlowIndexQueryParam(string keyword, string searchField, string entityType = null, int maxQueryResults = Constants.DEFAULT_QUERY_RESULTS_RETURNED, bool includeCount = false, bool onlyIncludePk = false)
		{
			_ = Argument.NotNull(keyword, nameof(keyword));
			_ = Argument.NotNull(searchField, nameof(searchField));

			if (!string.IsNullOrWhiteSpace(keyword))
			{
				var term = new Term(searchField, keyword);
				var glowQuery = new PrefixQuery(term);
				GlowQueries = [glowQuery];
			}

			SearchFieldForEntityType = searchField;
			Keyword = keyword;
			EntityType = entityType;

			MaxQueryResults = maxQueryResults;
			IncludeCount = includeCount;
			OnlyIncludePk = onlyIncludePk;

			CheckQueryCount();
		}

		public GlowIndexQueryParam(List<IGlowQuery> glowQueries, string entityType, int maxQueryResults = Constants.DEFAULT_QUERY_RESULTS_RETURNED, bool includeCount = false, bool onlyIncludePk = true)
		{
			_ = Argument.NotNull(entityType, nameof(entityType));

			EntityType = entityType;
			GlowQueries = glowQueries;

			MaxQueryResults = maxQueryResults;
			IncludeCount = includeCount;

			OnlyIncludePk = onlyIncludePk;

			CheckQueryCount();
		}

		protected void CheckQueryCount()
		{
			if (MaxQueryResults > Constants.MAXIMUM_QUERY_RESULTS_RETURNED)
			{
				throw new ArgumentOutOfRangeException(nameof(MaxQueryResults), MaxQueryResults, $"Max query result count cannot be > {Constants.MAXIMUM_QUERY_RESULTS_RETURNED}");
			}
			if (MaxQueryResults < Constants.MINIMUM_QUERY_RESULTS_RETURNED)
			{
				throw new ArgumentOutOfRangeException(nameof(MaxQueryResults), MaxQueryResults, $"Max query result count cannot be < {Constants.MINIMUM_QUERY_RESULTS_RETURNED}");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "API string")]
		public string GetUriHeader(int top)
		{
			var sb = new StringBuilder();
			_ = sb.Append(Constants.BASE_URL).Append('/').Append(Constants.LUCENE_ENTITY_INFOS);
			_ = sb.Append('?').Append(FormattableString.Invariant($"$top={top}"));

			if (IncludeCount)
			{
				_ = sb.Append('&').Append("$count=true");
			}
			return sb.ToString();
		}

		public GlowIndexQueryParam()
		{
			SearchFieldForEntityType = Constants.LUCENE_SEARCH_FIELD_COMMON;
			EntityType = null;
			MaxQueryResults = Constants.DEFAULT_QUERY_RESULTS_RETURNED;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "API string")]
		public string GetQueryUri(int top)
		{
			var sb = new StringBuilder();
			sb.Append(GetUriHeader(top));

			if (SearchFieldForEntityType == null && EntityType == null
				&& SearchFieldsForCategory == null && CategoryType == null)
			{
				return sb.ToString();
			}

			var filterUrl = GetFilterUri();
			if (!string.IsNullOrEmpty(filterUrl))
			{
				_ = sb.Append('&').Append("$filter=").Append(filterUrl);
			}
			return sb.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "API string")]
		string GetFilterUri()
		{
			var list = new List<string>();
			list.AddRange(GlowQueries.Where(s => !string.IsNullOrWhiteSpace(s.ToUrlComponent())).Select(s => s.ToUrlComponent()));

			if (!string.IsNullOrWhiteSpace(EntityType))
			{
				list.Add(FormattableString.Invariant($"EntityType eq '{SanitiseInput(EntityType)}'"));
			}
			else if (!string.IsNullOrWhiteSpace(CategoryType))
			{
				var subQuery = string.Format("({0})", string.Join(" or ", list));
				list.Clear();
				list.Add(subQuery);
				list.Add(FormattableString.Invariant($"Category eq '{SanitiseInput(CategoryType)}'"));
			}
			return string.Join(" and ", list);
		}

		public static string SanitiseInput(string input) => WebUtility.UrlEncode(input.Replace("'", "''"));
	}
}

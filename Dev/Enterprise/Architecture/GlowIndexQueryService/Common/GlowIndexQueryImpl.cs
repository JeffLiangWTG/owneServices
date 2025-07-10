using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using GlowIndexQueryService.Business;
using GlowIndexQueryService.Common.Dtos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WTG.Foundation.Http;
using WTG.StaticAnalysis.Annotation;

namespace GlowIndexQueryService.Common
{
	static class GlowIndexQueryImpl
	{
		internal static GlowIndexQueryResultCollection Query(GlowIndexQueryParam queryParam)
		{
			var request = ObjectFactory.Get<IGlowWebRequest>();
			request.SetAdditionalHeader(Constants.ODATA_PREFER_HEADER_NAME, Constants.ODATA_PREFER_HEADER_VALUE);
			return Query(queryParam, request);
		}

		internal static SearchFieldCollection GetSearchFields(string entityType)
			=> GetSearchFields(entityType, ObjectFactory.Get<IGlowWebRequest>());

		internal static ISet<string> GetGlowEntityTypes()
		{
			try
			{
				return GlowEntityTypes ?? new HashSet<string>();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
			}
			return new HashSet<string>();
		}

		internal static ISet<string> GetGlowEntityCategories()
		{
			try
			{
				return GlowEntityCategories ?? new HashSet<string>();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
			}
			return new HashSet<string>();
		}

		internal static IEnumerable<ZGuid> QueryPKs(GlowIndexQueryParam queryParam)
		{
			_ = Argument.NotNull(queryParam, nameof(queryParam));

			queryParam.OnlyIncludePk = true;
			var searchResults = Query(queryParam);

			return searchResults.Status == GlowIndexQueryStatus.Success
				? searchResults.Results
					.Select(r => ZGuid.TryParse(r.PK, out var zGuid) ? zGuid : ZGuid.Empty)
					.Where(r => r != ZGuid.Empty)
				: Enumerable.Empty<ZGuid>();
		}

		// Returns a dictionary with key 'entityType' and value a list of strings which are reference IDs to
		// search results that match the query.
		// Note: user must be logged in to both CW1 and Glow to call Get() from the Glow service.
		// However checking this introduces circular references so we can leave it to the consumer to do this.
		internal static GlowIndexQueryResultCollection Query(GlowIndexQueryParam queryParam, IGlowWebRequest glowRequest)
		{
			_ = Argument.NotNull(queryParam, nameof(queryParam));
			_ = Argument.NotNull(glowRequest, nameof(glowRequest));

			var results = new GlowIndexQueryResultCollection();

			try
			{
				if (!ValidateQueryParams(queryParam, out var failStatus))
				{
					results.Status = failStatus;
					return results;
				}

				var nextLink = queryParam.GetQueryUri(queryParam.MaxQueryResults);
				do
				{
					var rawGlowResponses = glowRequest.Get(nextLink);
					// Empty response - something may have gone wrong and we could throw an exception, but what is the user
					// going to do? I think it's better to just display no results if this happens.
					if (string.IsNullOrEmpty(rawGlowResponses))
					{
						return results;
					}

					nextLink = null;
					var deserialisedResponse = DeserialiseResponse(rawGlowResponses);
					if (deserialisedResponse != null)
					{
						results.Concat(ParseKeyFields(deserialisedResponse, queryParam.OnlyIncludePk));
						ParseTableUpdateTime(deserialisedResponse, results.TablesUpdateTime);
						nextLink = ParseNextLink(deserialisedResponse);
						results.MaximumResults = queryParam.IncludeCount ? ParseResultCount(deserialisedResponse) : Constants.QUERY_COUNT_OMITTED;
					}
				}
				while (!string.IsNullOrEmpty(nextLink));
			}
			catch (CargoWise.Authentication.Primitives.AuthorizationFailureException e)
			{
				results.Status = GlowIndexQueryStatus.UserNotLoggedIn;
				results.ErrorMessage = Res.GetString("45b2fc2d-a5e4-4cdd-8bea-a8fbabe22354", "User Not Logged In, ") + e.Message;
				return results;
			}
			catch (GlowHttpRequestException e)
			{
				var displayMessage = e.Message;
				var statusCode = (int)e.StatusCode;
				if (statusCode == 503)
				{
					results.Status = GlowIndexQueryStatus.ServiceUnavailable;
					displayMessage = Res.GetString("14447266-2583-4bee-b715-e721359d2404", "Glow Service is unavailable, please try again later");
				}
				else if (statusCode / 100 == 5 || statusCode == 0 || statusCode == 404)
				{
					results.Status = GlowIndexQueryStatus.DeadService;
					displayMessage = Res.GetString("056946ef-3330-4103-99ee-ac7143ee6b3b", "Please check the registry setting: GLOW > Services > GLOW Service URL");
				}
				else if (statusCode == 401)
				{
					results.Status = GlowIndexQueryStatus.UserNotLoggedIn;
					displayMessage = Res.GetString("4cca3739-6641-45b0-b816-13bee865a5b2", "Please re-login as a non-system user");
				}
				else
				{
					results.Status = GlowIndexQueryStatus.BadRequest;
					if (statusCode / 100 == 4)
					{
						var message = $"HttpRequestException {statusCode}:{e.StatusCode}";
						displayMessage = message;
						ErrorReporter.ReportOnce($"GIQS {statusCode}:{e.StatusCode}", message, e);
					}
				}
				results.ErrorMessage = displayMessage;
				return results;
			}
			catch (HttpRequestException e)
			{
				results.Status = GlowIndexQueryStatus.BadRequest;
				results.ErrorMessage = Res.GetString("ea34351d-93d8-46f4-b9f3-92cbf5d89725", "HTTP Bad Request: {0}", e.Message);
				return results;
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				results.Status = GlowIndexQueryStatus.UnknownError;
				results.Results.Add(new GlowIndexQueryResult(e.Message, string.Empty, null));
				results.ErrorMessage = Res.GetString("63bd7174-475c-4c81-911e-e7f738d2899f", "Unknown Error: {0}", e.Message);
				return results;
			}

			results.Status = GlowIndexQueryStatus.Success;

			if (results.IsOutOfDate)
			{
				string outOfDateMessage;
				if (results.LastUpdateTime.IsValid)
				{
					outOfDateMessage = Res.GetString("67644efd-e1f8-4427-ae17-765d0eb1d6e5",
@"GLOW Indexing last update at {0} minutes ago.
Recent changes may not appear in search results.", (int)(ZDateTimeOffset.Now - results.LastUpdateTime).TotalMinutes);
				}
				else
				{
					outOfDateMessage = Res.GetString("56ba73ba-801d-48d9-be12-7b927cfcf0d9",
@"GLOW Indexing is delayed.
Recent changes may not appear in search results.");
				}
				results.WarningMessage = outOfDateMessage;
			}

			return results;
		}

		public static JObject DeserialiseResponse(string rawGlowResponse)
		{
			try
			{
				return (JObject)JsonConvert.DeserializeObject(rawGlowResponse);
			}
			catch (JsonReaderException ex)
			{
				ErrorReporter.ReportOnce($"Failed to deserialise glow response. RawGlowResponse={rawGlowResponse}", ex); // dev exception message
				return null;
			}
		}

		public static bool ValidateQueryParams(GlowIndexQueryParam queryParam, out GlowIndexQueryStatus failureStatus)
		{
			if (queryParam.GlowQueries == null || queryParam.GlowQueries.Count == 0)
			{
				failureStatus = GlowIndexQueryStatus.BlankSearchTerms;
				return false;
			}

			if (GlowEntityTypes != null && queryParam.EntityType != null && !GlowEntityTypes.Any(e => e.Equals(queryParam.EntityType, StringComparison.OrdinalIgnoreCase)))
			{
				failureStatus = GlowIndexQueryStatus.UnknownEntityType;
				return false;
			}

			if (GlowEntityCategories != null && queryParam.CategoryType != null && !GlowEntityCategories.Any(e => e.Equals(queryParam.CategoryType, StringComparison.OrdinalIgnoreCase)))
			{
				failureStatus = GlowIndexQueryStatus.UnknownCategoryType;
				return false;
			}

			if (queryParam.EntityType != null && queryParam.CategoryType != null)
			{
				failureStatus = GlowIndexQueryStatus.EntityTypeAndCategoryTypeCoexist;
				return false;
			}

			if (GlowSearchFields != null && queryParam.SearchFieldForEntityType != null && !GlowSearchFields.Contains(queryParam.SearchFieldForEntityType.ToUpper()))
			{
				failureStatus = GlowIndexQueryStatus.UnknownSearchField;
				return false;
			}

			if (GlowSearchFields != null && queryParam.SearchFieldsForCategory != null && queryParam.SearchFieldsForCategory.Any(x => !GlowSearchFields.Contains(x.ToUpper())))
			{
				failureStatus = GlowIndexQueryStatus.UnknownSearchField;
				return false;
			}

			failureStatus = GlowIndexQueryStatus.Success;
			return true;
		}

		static internal ISet<string> GlowEntityTypes
		{
			get => glowEntityTypes = glowEntityTypes ?? GetGlowMetadata(Constants.LUCENE_ENTITY_TYPES)?.ToImmutableHashSet(); // URLs are not translatable
			set => glowEntityTypes = value?.ToImmutableHashSet();
		}
		[ThreadSafe]
		internal static ImmutableHashSet<string> glowEntityTypes = null;

		static internal ISet<string> GlowSearchFields
		{
			get => glowSearchFields = glowSearchFields ?? GetGlowMetadata(Constants.LUCENE_SEARCH_FIELDS)?.ToImmutableHashSet(); // URLs are not translatable
			set => glowSearchFields = value?.ToImmutableHashSet();
		}
		[ThreadSafe]
		internal static ImmutableHashSet<string> glowSearchFields = null;

		static internal ISet<string> GlowEntityCategories
		{
			get => glowEntityCategories = glowEntityCategories ?? GetGlowMetadata(Constants.LUCENE_ENTITY_CATEGORIES)?.ToImmutableHashSet(); // URLs are not translatable
			set => glowEntityCategories = value?.ToImmutableHashSet();
		}
		[ThreadSafe]
		internal static ImmutableHashSet<string> glowEntityCategories = null;

		internal static ISet<string> GetGlowMetadata(string metadataType)
		{
			string rawGlowResponse;
			var request = ObjectFactory.Get<IGlowWebRequest>();
			rawGlowResponse = request.Get($"{Constants.BASE_URL}/{metadataType}?$format=json"); // URLs are not translatable

			return ParseGlowMetadata(metadataType, rawGlowResponse);
		}

		internal static HashSet<string> ParseGlowMetadata(string metadataType, string rawGlowResponse)
		{
			if (string.IsNullOrEmpty(rawGlowResponse))
			{
				return null;
			}

			var response = (dynamic)DeserialiseResponse(rawGlowResponse);
			var localSet = new HashSet<string>();

			if (metadataType == Constants.LUCENE_ENTITY_TYPES)
			{
				foreach (var val in response.value)
				{
					localSet.Add(val.Value);
				}
			}
			else if (metadataType == Constants.LUCENE_ENTITY_CATEGORIES)
			{
				foreach (var val in response.value)
				{
					localSet.Add(val.Value);
				}
			}
			else if (metadataType == Constants.LUCENE_SEARCH_FIELDS)
			{
				foreach (var val in response.value)
				{
					localSet.Add(val.FieldName.Value.ToUpper());
				}
				// special case, glow has this search field, but doesn't tell us
				_ = localSet.Add(Constants.LUCENE_SEARCH_FIELD_COMMON.ToUpper()); // API string
			}

			return localSet;
		}

		internal static ICollection<GlowIndexQueryResult> ParseKeyFields(dynamic response, bool onlyIncludePk = false)
		{
			var results = new Collection<GlowIndexQueryResult>();
			foreach (var val in response.value)
			{
				var value = new GlowIndexQueryResult((string)val.PK, (string)val.EntityType);
				if (!onlyIncludePk)
				{
					foreach (var kv in val.KeyFields)
					{
						value.KeyFields.Add(((string)kv.FieldName, (string)kv.Value));
					}
				}
				results.Add(value);
			}
			return results;
		}

		internal static void ParseTableUpdateTime(JObject response, IDictionary<string, ZDateTimeOffset> tablesUpdateTime)
		{
			if (!response.TryGetValue("@index.updated", out var tablesUpdatedTime))
			{
				return;
			}

			foreach (var item in tablesUpdatedTime)
			{
				var tableName = (string)item["Source"];
				var updateTime = (DateTimeOffset)item["Updated"];
				tablesUpdateTime[tableName] = updateTime;
			}
		}

		internal static string ParseNextLink(JObject response)
			=> ParseResponseField(response, Constants.ODATA_NEXTLINK, JTokenAsString);

		internal static int ParseResultCount(JObject response)
			=> ParseResponseField(response, Constants.ODATA_COUNT, JTokenAsInt);

		internal static string JTokenAsString(JToken t)
			=> (string)t;

		internal static int JTokenAsInt(JToken t)
			=> int.TryParse((string)t, out var result) ? result : -1;

		internal static T ParseResponseField<T>(JObject response, string fieldName, Func<JToken, T> conversionFunc)
		{
			var jToken = response
				.Properties()
				.SingleOrDefault(p => p.Name == fieldName)?.Value;

			return conversionFunc(jToken);
		}

		internal static SearchFieldCollection GetSearchFields(string entityType, IGlowWebRequest glowRequest)
		{
			var rawGlowResponse = GetRawGlowResponse(entityType, glowRequest);
			if (rawGlowResponse == null)
			{
				return new SearchFieldCollection(GlowIndexQueryStatus.BadRequest);
			}

			var searchFieldsResponse = ParseSearchFieldsJson(rawGlowResponse);
			if (searchFieldsResponse == null)
			{
				return new SearchFieldCollection(GlowIndexQueryStatus.UnknownEntityType);
			}

			if (searchFieldsResponse.IsReponseBlank)
			{
				ReportBlankSearchFieldFromGlow(entityType, rawGlowResponse);
				return new SearchFieldCollection(GlowIndexQueryStatus.UnknownSearchField);
			}

			var lookupMetadata = GetGlowLookupMetadata(entityType, glowRequest);
			if (lookupMetadata == null)
			{
				return new SearchFieldCollection(GlowIndexQueryStatus.UnKnownLookupMetadata);
			}

			var translationDataMap = fieldsTranslationMap.GetOrAdd(Res.CurrentLanguage, lang => GenStringData(glowRequest, lang));
			var searchFields = searchFieldsResponse.Value.Select(dto => CreateSearchField(entityType, dto)).Where(s => s != null).ToArray();

			var searchFieldCollection = new SearchFieldCollection(entityType, searchFields);
			return searchFieldCollection;

			SearchField CreateSearchField(string entityType, SearchFieldDto dto)
			{
				if (dto.DataType == null)
				{
					return null;
				}

				var lookupCompositeDto = lookupMetadata.TryGetValue(dto.FieldName.ToUpperInvariant(), out var lookup) ? lookup : null;
				var description = translationDataMap.TryGetValue($"ENTITY|{dto.FieldName}", out var desc) ? desc : dto.FieldName;

				if (lookupCompositeDto?.RuleLookup != null)
				{
					var ruleLookup = new SearchFieldRuleLookup(lookupCompositeDto.RuleLookup.RuleId);
					return new SearchField(dto.FieldName, description, dto.DataType, dto.UIHidden, dto.IsUtcTime, dto.Scale, ruleLookup);
				}

				if (lookupCompositeDto?.EntityLookup != null)
				{
					var lookupInfo = lookupCompositeDto.EntityLookup;
					var entityLookup = new SearchFieldEntityLookup(lookupInfo.LookupSource, lookupInfo.ValuePath, lookupInfo.TableName, lookupInfo.HierarchyColumnName, lookupInfo.HierarchyColumnValue);
					return new SearchField(dto.FieldName, description, dto.DataType, dto.UIHidden, dto.IsUtcTime, dto.Scale, entityLookup);
				}

				return new SearchField(dto.FieldName, description, dto.DataType, dto.UIHidden, dto.IsUtcTime, dto.Scale);
			}
		}

		static string GetRawGlowResponse(string entityType, IGlowWebRequest glowRequest)
		{
			var sb = new StringBuilder();
			sb
				.Append(Constants.BASE_URL)
				.Append('/').Append(Constants.LUCENE_SEARCH_GET_FIELDS_BY_TYPE)
				.Append("?").Append($"@entityType=").Append(WebUtility.UrlEncode($"'{entityType}'"));
			var url = sb.ToString();

			try
			{
				return glowRequest.Get(url);
			}
			catch
			{
				return null;
			}
		}

		static IDictionary<string, LookupCompositeDto> GetGlowLookupMetadata(string entityType, IGlowWebRequest glowRequest)
		{
			var url = $"{Constants.LOOKUP_SERVICE}{entityType}";
			try
			{
				var rawResponseString = glowRequest.Get(url);
				return JsonConvert.DeserializeObject<IDictionary<string, LookupCompositeDto>>(rawResponseString).ToDictionary(kp => kp.Key.ToUpperInvariant(), kp => kp.Value);
			}
			catch
			{
				return null;
			}
		}

		static SearchFieldsResponse ParseSearchFieldsJson(string rawGlowResponse)
		{
			try
			{
				return JsonConvert.DeserializeObject<SearchFieldsResponse>(rawGlowResponse);
			}
			catch
			{
				return null;
			}
		}

		static void ReportBlankSearchFieldFromGlow(string entityType, string rawGlowResponse)
		{
			ErrorReporter.ReportOnce("BlankSearchFieldFromGlow",
@$"entityType:
{entityType}
rawGlowResponse:
{rawGlowResponse}");
		}

		static readonly ConcurrentDictionary<string, ImmutableDictionary<string, string>> fieldsTranslationMap = new();

		static ImmutableDictionary<string, string> GenStringData(IGlowWebRequest glowRequest, string language)
		{
			string rawGlowResponses;

			var result = new Dictionary<string, string>();

			try
			{
				var url = Constants.CAPTION_SERVICE + language;
				rawGlowResponses = glowRequest.Get(url);
			}
			catch
			{
				return result.ToImmutableDictionary();
			}

			if (string.IsNullOrEmpty(rawGlowResponses))
			{
				return result.ToImmutableDictionary();
			}
			var deserialisedResponse = DeserialiseResponse(rawGlowResponses);
			result = deserialisedResponse
				.Cast<KeyValuePair<string, JToken>>()
				.ToDictionary(d => d.Key, d => d.Value["long"]
				.ToString());
			return result.ToImmutableDictionary();
		}

		public static CodeDescriptionPairList GetListByRuleId(Guid lookupRuleId)
		{
			if (ListCache.TryGetValue(lookupRuleId, out var list))
			{
				return list;
			}

			list = GetListByRuleIdCore(lookupRuleId);
			ListCache.Add(lookupRuleId, list);
			return list;
		}

		static CodeDescriptionPairList GetListByRuleIdCore(Guid lookupRuleId)
		{
			var url = $"{Constants.LOOKUP_RULE_GET_SERVICE}/{lookupRuleId}";
			var result = new CodeDescriptionPairList();
			var glowRequest = ObjectFactory.Get<IGlowWebRequest>();
			try
			{
				var rawResponseString = glowRequest.Get(url);
				var list = JsonConvert.DeserializeObject<IList<CodeDescriptionDto>>(rawResponseString);
				foreach (var item in list)
				{
					result.AddPair(item.Code, (NoResString)item.Description);
				}
				return result;
			}
			catch
			{
				return result;
			}
		}

		internal static LRUCache<Guid, CodeDescriptionPairList> ListCache
		{
			get { return listCache ??= new(); }
		}
		static LRUCache<Guid, CodeDescriptionPairList> listCache;
	}
}

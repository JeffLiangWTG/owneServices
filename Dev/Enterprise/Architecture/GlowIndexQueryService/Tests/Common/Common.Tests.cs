using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.IO;
using Enterprise.Registry.Business;
using GlowIndexQueryService.Business;
using GlowIndexQueryService.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using WTG.Foundation.Http;
using Res = GlowIndexQueryService.Common.Res;

namespace GlowIndexQueryService.Tests.Common
{
	sealed class GlowIndexQueryServiceTestClass : TransactionedTestCase
	{
		string glowResponseString;

		protected override void SetUp()
		{
			base.SetUp();

			GlowIndexQueryImpl.GlowSearchFields = null;
			GlowIndexQueryImpl.GlowEntityTypes = null;
			GlowIndexQueryImpl.GlowEntityCategories = null;

			_ = GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				glowResponseString = resourceRetriever.GetString("GlowIndexQueryService.Tests.TestFiles.SearchFields.json");
				Assert("Glow Index Query must be enabled for this test to pass", GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.Value);
			}
		}

		public void SetUpGlowMetaData()
		{
			using (ObjectFactory.Substitute<IGlowWebRequest>(new MockGlowWebRequest(GlowTestData.TestGlowSearchFields)))
			{
				_ = GlowIndexQueryImpl.GlowSearchFields;
			}
			using (ObjectFactory.Substitute<IGlowWebRequest>(new MockGlowWebRequest(GlowTestData.GlowEntityTypesTestData)))
			{
				_ = GlowIndexQueryImpl.GlowEntityTypes;
			}
			using (ObjectFactory.Substitute<IGlowWebRequest>(new MockGlowWebRequest(GlowTestData.GlowEntityCategoriesTestData)))
			{
				_ = GlowIndexQueryImpl.GlowEntityCategories;
			}
		}

		protected override void TearDown()
		{
			GlowIndexQueryImpl.GlowSearchFields = null;
			GlowIndexQueryImpl.GlowEntityTypes = null;
			GlowIndexQueryImpl.GlowEntityCategories = null;
			GlowIndexQueryImpl.ListCache.Clear();
			ObjectFactory.DisposeSingletons();
			ObjectFactory.DisposeSubstitutions();

			base.TearDown();
		}

		public void TestGlowIndexQueryResultsConcat()
		{
			var results = new GlowIndexQueryResultCollection();
			var extra = new Collection<GlowIndexQueryResult>();

			AssertEquals(0, results.Results.Count);
			AssertEquals(0, extra.Count);

			extra.Add(new GlowIndexQueryResult("pk", "entityType", new Collection<(string Key, string Value)> { ("k1", "v1"), ("k2", "v2") }));
			AssertEquals(1, extra.Count);
			AssertEquals(2, extra.First().KeyFields.Count);

			results.Concat(extra);

			AssertEquals(1, results.Results.Count);
			AssertEquals("pk", results.Results.First().PK);
			AssertEquals("entityType", results.Results.First().EntityType);

			var kf = results.Results.First().KeyFields.ToArray();
			AssertEquals(2, kf.Length);
			AssertEquals("k1", kf[0].Key);
			AssertEquals("v1", kf[0].Value);
			AssertEquals("k2", kf[1].Key);
			AssertEquals("v2", kf[1].Value);
		}

		public void TestShouldSetAdditionalHeader()
		{
			var mockGlowWebRequest = new MockGlowWebRequest(GlowTestData.TestData);
			using (ObjectFactory.Substitute<IGlowWebRequest>(mockGlowWebRequest))
			{
				_ = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("S", Constants.LUCENE_SEARCH_FIELD_COMMON));
				Assert("Should set additional header", mockGlowWebRequest.HasAdditionalHeader);
				AssertEquals("Should set additional header", "odata.include-annotations=\"*\"", mockGlowWebRequest.AdditionalHeaders["prefer"]);
			}
		}

		public void TestSearch()
		{
			SetUpGlowMetaData();

			var results = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("JAY", Constants.LUCENE_SEARCH_FIELD_COMMON), new MockGlowWebRequest(GlowTestData.TestData));
			AssertEquals("Result status should be success", GlowIndexQueryStatus.Success, results.Status);
			AssertEquals("Result entity type should be IOrgHeader", "IOrgHeader", results.Results.First().EntityType);
			AssertEquals(1, results.Results.Count);
			foreach (var result in results.Results)
			{
				Assert("Returned item contains search term at least once", result.KeyFields.Any(kf => kf.Value.StartsWith("JAY")));
			}
		}

		public void TestSearchWithEmptyUpdateTime()
		{
			SetUpGlowMetaData();

			var mockGlowWebRequestWithEmptyUpdateTime = new MockGlowWebRequest(GlowTestData.TestData);
			var results = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("JAY", Constants.LUCENE_SEARCH_FIELD_COMMON), mockGlowWebRequestWithEmptyUpdateTime);
			AssertEquals("Result status should be success", GlowIndexQueryStatus.Success, results.Status);
			AssertEquals("Result entity type should be IOrgHeader", "IOrgHeader", results.Results.First().EntityType);
			AssertEquals(1, results.Results.Count);
			foreach (var result in results.Results)
			{
				Assert("Returned item contains search term at least once", result.KeyFields.Any(kf => kf.Value.StartsWith("JAY")));
			}

			AssertEquals("Result should no TablesUpdateTime", 0, results.TablesUpdateTime.Count);
			AssertEquals("Result should be Out Of Date", true, results.IsOutOfDate);
			AssertNullOrEmpty("Result should have no error message", results.ErrorMessage);
			AssertStartsWith("Result should have warning message", "GLOW Indexing is delayed.", results.WarningMessage);
		}

		[TestDate(2024, 1, 1, 8, 5, 0)]
		[TestUtcOffset(0, 0, 0)]
		public void TestSearchWithUpdateTime()
		{
			SetUpGlowMetaData();

			var mockGlowWebRequestWithEmptyUpdateTime = new MockGlowWebRequest(GlowTestData.TestDataWithUpdateTime);
			var results = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("JAY", Constants.LUCENE_SEARCH_FIELD_COMMON), mockGlowWebRequestWithEmptyUpdateTime);
			AssertEquals("Result status should be success", GlowIndexQueryStatus.Success, results.Status);

			AssertEquals("Result should has TablesUpdateTime", 1, results.TablesUpdateTime.Count);
			AssertEquals("Result should not Out Of Date", false, results.IsOutOfDate);
			AssertNullOrEmpty("Result should have no error message", results.ErrorMessage);
			AssertNullOrEmpty("Result should have no warning message", results.WarningMessage);
		}

		public void TestSearch_OnlyIncludePk()
		{
			SetUpGlowMetaData();

			var results = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("JAY", Constants.LUCENE_SEARCH_FIELD_COMMON, onlyIncludePk: true), new MockGlowWebRequest(GlowTestData.TestData));
			AssertEquals("Result status should be success", GlowIndexQueryStatus.Success, results.Status);
			AssertEquals("Result entity type should be IOrgHeader", "IOrgHeader", results.Results.First().EntityType);
			AssertEquals(1, results.Results.Count);
			foreach (var result in results.Results)
			{
				AssertEquals("Returned item not contains search term", 0, result.KeyFields.Count);
			}
		}

		public void TestSearchDuplicateKeyFieldsInResponse()
		{
			SetUpGlowMetaData();
			GlowIndexQueryResultCollection results = null;

			AssertNoExceptionThrown("Exception shouldn't be thrown from json response with duplicate fields",
				() => results = GlowIndexQueryImpl.Query(
					new GlowIndexQueryParam("JAY", Constants.LUCENE_SEARCH_FIELD_COMMON),
					new MockGlowWebRequest(GlowTestData.TestDataDuplicateKeyFields)));

			AssertNotNull(results);
			AssertEquals("Result status should be success", GlowIndexQueryStatus.Success, results.Status);
			AssertEquals("Result entity type should be IOrgHeader", "IOrgHeader", results.Results.First().EntityType);
			AssertEquals(1, results.Results.Count);
		}

		public void TestSearchEmpty()
		{
			var results = GlowIndexQueryImpl.Query(new GlowIndexQueryParam(string.Empty, Constants.LUCENE_SEARCH_FIELD_COMMON), new MockGlowWebRequest(GlowTestData.TestData));
			AssertEquals(GlowIndexQueryStatus.BlankSearchTerms, results.Status);
			AssertEquals(0, results.Results.Count);
		}

		public void TestSearchSpecialCharacters()
		{
			SetUpGlowMetaData();

			GlowIndexQueryResultCollection results = null;

			AssertNoExceptionThrown("Exception shouldn't be thrown from json response with special characters",
				() => results = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("' # &", Constants.LUCENE_SEARCH_FIELD_COMMON), new MockGlowWebRequest(GlowTestData.TestDataSpecialCharacters)));

			AssertNotNull(results);
			AssertEquals("Result status should be success", GlowIndexQueryStatus.Success, results.Status);
			AssertEquals("Result entity type should be IOrgHeader", "IOrgHeader", results.Results.First().EntityType);
			AssertEquals("One result should be found", 1, results.Results.Count);
			AssertEquals("Three KeyFields should be found", 3, results.Results.First().KeyFields.Count);
			AssertEquals("O'Brian", results.Results.First().KeyFields.ElementAt(0).Value);
			AssertEquals("No#000", results.Results.First().KeyFields.ElementAt(1).Value);
			AssertEquals("A&B", results.Results.First().KeyFields.ElementAt(2).Value);
		}

		public void TestSearchHandleHttpRequestExceptionBadRequest()
		{
			SetUpGlowMetaData();

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			GlowIndexQueryResultCollection results = null;

			AssertNoExceptionThrown(() => results = GlowIndexQueryImpl.Query(
				new GlowIndexQueryParam("test HttpRequestExceptionBadRequest", Constants.LUCENE_SEARCH_FIELD_COMMON),
				new MockGlowWebRequestThrows(new GlowHttpRequestException("400 (Bad Request)", System.Net.HttpStatusCode.BadRequest))));

			AssertEquals(GlowIndexQueryStatus.BadRequest, results.Status);
			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals("Error report should be sent", "GIQS 400:BadRequest", ErrorReporter.LastKeyReported);
			AssertEquals("Error report should be sent", "HttpRequestException 400:BadRequest", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestSearchHandleHttpRequestExceptionNotLoggedIn()
		{
			SetUpGlowMetaData();

			GlowIndexQueryResultCollection results = null;
			AssertNoExceptionThrown(() => results = GlowIndexQueryImpl.Query(
				new GlowIndexQueryParam("test HttpRequestExceptionNotLoggedIn", Constants.LUCENE_SEARCH_FIELD_COMMON),
				new MockGlowWebRequestThrows(new GlowHttpRequestException("401 (Not Logged In)", System.Net.HttpStatusCode.Unauthorized))));

			AssertEquals(GlowIndexQueryStatus.UserNotLoggedIn, results.Status);
			AssertEquals("Please re-login as a non-system user", results.ErrorMessage);
		}

		public void TestSearchHandleHttpRequestExceptionDeadService()
		{
			SetUpGlowMetaData();

			GlowIndexQueryResultCollection results = null;
			AssertNoExceptionThrown(() => results = GlowIndexQueryImpl.Query(
				new GlowIndexQueryParam("test HttpRequestExceptionDeadService0", Constants.LUCENE_SEARCH_FIELD_COMMON),
				new MockGlowWebRequestThrows(new GlowHttpRequestException("0 (Dead Service)"))));

			AssertEquals(GlowIndexQueryStatus.DeadService, results.Status);
			AssertEquals("Please check the registry setting: GLOW > Services > GLOW Service URL", results.ErrorMessage);

			results = null;
			AssertNoExceptionThrown(() => results = GlowIndexQueryImpl.Query(
				new GlowIndexQueryParam("test HttpRequestExceptionDeadService404", Constants.LUCENE_SEARCH_FIELD_COMMON),
				new MockGlowWebRequestThrows(new GlowHttpRequestException("404 (Dead Service)", System.Net.HttpStatusCode.NotFound))));

			AssertEquals(GlowIndexQueryStatus.DeadService, results.Status);
			AssertEquals("Please check the registry setting: GLOW > Services > GLOW Service URL", results.ErrorMessage);

			results = null;
			AssertNoExceptionThrown(() => results = GlowIndexQueryImpl.Query(
				new GlowIndexQueryParam("test HttpRequestExceptionDeadService500", Constants.LUCENE_SEARCH_FIELD_COMMON),
				new MockGlowWebRequestThrows(new GlowHttpRequestException("500 (Dead Service)", System.Net.HttpStatusCode.InternalServerError))));

			AssertEquals(GlowIndexQueryStatus.DeadService, results.Status);
			AssertEquals("Please check the registry setting: GLOW > Services > GLOW Service URL", results.ErrorMessage);
		}

		public void TestSearchHandleHttpRequestExceptionServiceUnavailable()
		{
			SetUpGlowMetaData();

			GlowIndexQueryResultCollection results = null;
			AssertNoExceptionThrown(() => results = GlowIndexQueryImpl.Query(
				new GlowIndexQueryParam("test HttpRequestExceptionServiceUnavailable503", Constants.LUCENE_SEARCH_FIELD_COMMON),
				new MockGlowWebRequestThrows(new GlowHttpRequestException("503 (Service Unavailable)", System.Net.HttpStatusCode.ServiceUnavailable))));

			AssertEquals(GlowIndexQueryStatus.ServiceUnavailable, results.Status);
			AssertEquals("Glow Service is unavailable, please try again later", results.ErrorMessage);
		}

		public void TestSearchHandleGlowClientUnknownException()
		{
			SetUpGlowMetaData();

			GlowIndexQueryResultCollection results = null;
			AssertNoExceptionThrown(() => results = GlowIndexQueryImpl.Query(
				new GlowIndexQueryParam("test", Constants.LUCENE_SEARCH_FIELD_COMMON),
				new MockGlowWebRequestThrows(new ArgumentException("generic exception")))); // throw an exception with this message
			AssertEquals(GlowIndexQueryStatus.UnknownError, results.Status);
		}

		public void TestSearchHandleGlowClientNotLoggedInException()
		{
			SetUpGlowMetaData();

			var results = GlowIndexQueryImpl.Query(
				new GlowIndexQueryParam("test", Constants.LUCENE_SEARCH_FIELD_COMMON),
				new MockGlowWebRequestThrows(new CargoWise.Authentication.Primitives.AuthorizationFailureException("You must be logged in in order to access GLOW services.")));

			AssertEquals(GlowIndexQueryStatus.UserNotLoggedIn, results.Status);
			AssertEquals("User Not Logged In, You must be logged in in order to access GLOW services.", results.ErrorMessage);
			AssertEquals(0, results.Results.Count);
		}

		public void TestCatchTheExceptionOfValidateQueryParamsThrowAndAddToErrorMessage()
		{
			using (ObjectFactory.Substitute<IGlowWebRequest>(new MockGlowWebRequestThrows(new CargoWise.Authentication.Primitives.AuthorizationFailureException("Unexpected authentication result: ContextChangeRequired"))))
			{
				var results = GlowIndexQueryImpl.Query(
				new GlowIndexQueryParam("test", Constants.LUCENE_SEARCH_FIELD_COMMON),
				new MockGlowWebRequestThrows(new GlowHttpRequestException("Unsuccessful Glow Request (Code: 401)", HttpStatusCode.Unauthorized)));

				AssertEquals(GlowIndexQueryStatus.UserNotLoggedIn, results.Status);
				AssertEquals("User Not Logged In, Unexpected authentication result: ContextChangeRequired", results.ErrorMessage);
				AssertEquals(0, results.Results.Count);
			}
		}

		public void TestSearchMultiPart()
		{
			SetUpGlowMetaData();

			var results = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("S", Constants.LUCENE_SEARCH_FIELD_COMMON), new MockGlowWebRequest(GlowTestData.TestData_Multi1, GlowTestData.TestData_Multi2));

			AssertNotNull(results);
			AssertEquals("Result status should be success", GlowIndexQueryStatus.Success, results.Status);
			AssertEquals(4, results.Results.Count);
		}

		public void TestSearchMultiPartErrorPlacedInResults()
		{
			SetUpGlowMetaData();

			var results = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("S", Constants.LUCENE_SEARCH_FIELD_COMMON), new MockGlowWebRequest(GlowTestData.TestData_Multi1));

			AssertNotNull(results);
			AssertEquals("Result status should be unknown error", GlowIndexQueryStatus.UnknownError, results.Status);
			AssertEquals("Attempted to resubmit a request to Glow service but no more requests were expected", results.Results.Last().PK);
			AssertEquals(3, results.Results.Count); // 2 legit results from TestData_Multi1, 1 error message for the missing second part
		}

		public void TestShouldUseNextLink()
		{
			SetUpGlowMetaData();

			var mockGlowWebRequest = new MockGlowWebRequest(GlowTestData.TestData_Multi1, GlowTestData.TestData_Multi2);
			var results = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("S", Constants.LUCENE_SEARCH_FIELD_COMMON), mockGlowWebRequest);

			AssertEquals("Result status should be success", GlowIndexQueryStatus.Success, results.Status);
			AssertEquals(4, results.Results.Count);

			AssertEquals("Request Glow Index Search Fields", 2, mockGlowWebRequest.CurrentRequestCount);
			AssertEquals(2, mockGlowWebRequest.RequestURLs.Count);
			AssertEquals("odata/Index/EntityInfos?$top=50&$filter=(startswith(Common,'S'))", mockGlowWebRequest.RequestURLs[0]);
			AssertEquals("EntityInfos?$filter=startswith(Common,%20'S546147')&$skiptoken=2", mockGlowWebRequest.RequestURLs[1]);
		}

		public void TestGetQueryUri()
		{
			CombineAssertions(() =>
			{
				AssertEquals("odata/Index/EntityInfos?$top=50&$filter=(startswith(" + Constants.LUCENE_SEARCH_FIELD_COMMON.ToUpper() + ",'S'))", new GlowIndexQueryParam("S", Constants.LUCENE_SEARCH_FIELD_COMMON.ToUpper(), includeCount: false).GetQueryUri(50));
				AssertEquals("odata/Index/EntityInfos?$top=50&$count=true&$filter=(startswith(" + Constants.LUCENE_SEARCH_FIELD_COMMON.ToUpper() + ",'S'))", new GlowIndexQueryParam("S", Constants.LUCENE_SEARCH_FIELD_COMMON.ToUpper(), includeCount: true).GetQueryUri(50));
				AssertEquals("odata/Index/EntityInfos?$top=23&$count=true&$filter=(startswith(" + Constants.LUCENE_SEARCH_FIELD_COMMON.ToUpper() + ",'hello+world'))", new GlowIndexQueryParam("hello world", Constants.LUCENE_SEARCH_FIELD_COMMON.ToUpper(), includeCount: true).GetQueryUri(23));
				AssertEquals("odata/Index/EntityInfos?$top=50&$count=true&$filter=(startswith(" + Constants.LUCENE_SEARCH_FIELD_COMMON.ToUpper() + ",'!+%22+%23+%24+%25+%26+%27%27+(+)+*+%2B+%2C+-+.+%2F+%5C+%40+%3F'))", new GlowIndexQueryParam("! \" # $ % & ' ( ) * + , - . / \\ @ ?", Constants.LUCENE_SEARCH_FIELD_COMMON.ToUpper(), includeCount: true).GetQueryUri(50));
				AssertEquals("odata/Index/EntityInfos?$top=50&$count=true&$filter=(startswith(ROUTINGSTATUS,'S'))", new GlowIndexQueryParam("S", "ROUTINGSTATUS", includeCount: true).GetQueryUri(50));
				AssertEquals("odata/Index/EntityInfos?$top=50&$count=true&$filter=(startswith(Common,'John++Apple'))", new GlowIndexQueryParam("John  Apple", Constants.LUCENE_SEARCH_FIELD_COMMON, includeCount: true).GetQueryUri(50));
				AssertEquals("odata/Index/EntityInfos?$top=50&$count=true&$filter=(startswith(Common,'John+Apple')) and EntityType eq 'Form'", new GlowIndexQueryParam("John Apple", Constants.LUCENE_SEARCH_FIELD_COMMON, "Form", includeCount: true).GetQueryUri(50));
				AssertEquals("odata/Index/EntityInfos?$top=50&$count=true&$filter=(startswith(Common,'John%27%27+Apple%27%27%27%27')) and EntityType eq 'Form%27%27'", new GlowIndexQueryParam("John' Apple''", Constants.LUCENE_SEARCH_FIELD_COMMON, "Form'", includeCount: true).GetQueryUri(50));
				//tests for search by category
				AssertEquals("odata/Index/EntityInfos?$top=50&$count=true&$filter=((JOBNUMBER eq 'S0001001') or (HOUSEBILLNUMBER eq 'S0001001')) and Category eq 'APA'", new GlowIndexQueryParam("S0001001", new string[] { "JOBNUMBER", "HOUSEBILLNUMBER" }, "APA", includeCount: true).GetQueryUri(50));
			});
		}

		public void TestSearchGlowIndexSearchFieldsRequestOnlyOnce()
		{
			var mockGlowWebRequest = new MockGlowWebRequest(GlowTestData.GlowEntityTypesTestData, GlowTestData.TestGlowSearchFields);

			using (ObjectFactory.Substitute<IGlowWebRequest>(mockGlowWebRequest))
			{
				_ = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("S", Constants.LUCENE_SEARCH_FIELD_COMMON), new MockGlowWebRequest(GlowTestData.TestData));
				AssertEquals("Request Glow Index Search Fields", 2, mockGlowWebRequest.CurrentRequestCount);
				_ = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("SS", Constants.LUCENE_SEARCH_FIELD_COMMON), new MockGlowWebRequest(GlowTestData.TestData_Multi1));
				_ = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("SSS", Constants.LUCENE_SEARCH_FIELD_COMMON), new MockGlowWebRequest(GlowTestData.TestData_Multi2));
				_ = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("SSSSS", Constants.LUCENE_SEARCH_FIELD_COMMON), new MockGlowWebRequest(GlowTestData.TestData));
				AssertEquals("Only request Glow Index Search Fields once", 2, mockGlowWebRequest.CurrentRequestCount);
			}
		}

		public void TestParseNextLink()
		{
			dynamic response = JsonConvert.DeserializeObject(GlowTestData.TestData_Multi1);
			var nextLink = GlowIndexQueryImpl.ParseNextLink(response);
			AssertEquals(@"EntityInfos?$filter=startswith(" + Constants.LUCENE_SEARCH_FIELD_COMMON + ",%20'S546147')&$skiptoken=2", nextLink);
		}

		public void TestParseNextLinkNonexistent()
		{
			dynamic response = JsonConvert.DeserializeObject(GlowTestData.TestData);
			var nextLink = GlowIndexQueryImpl.ParseNextLink(response);
			AssertEquals(null, nextLink);
		}

		public void TestParseKeyFields()
		{
			dynamic response = JsonConvert.DeserializeObject(GlowTestData.TestData);
			ICollection<GlowIndexQueryResult> results = GlowIndexQueryImpl.ParseKeyFields(response);
			AssertEquals(1, results.Count);

			var firstResult = results.First().KeyFields.ToArray();
			AssertEquals(5, firstResult.Length);

			AssertEquals("CODE", firstResult[0].Key);
			AssertEquals("JAYSCH", firstResult[0].Value);

			AssertEquals("DESCRIPTION", firstResult[1].Key);
			AssertEquals("JAY SCHULZ", firstResult[1].Value);

			AssertEquals("NAME", firstResult[2].Key);
			AssertEquals("JAY SCHULZ", firstResult[2].Value);

			AssertEquals("REFERENCEID", firstResult[3].Key);
			AssertEquals("JAYSCH", firstResult[3].Value);

			AssertEquals("CLOSESTPORT", firstResult[4].Key);
			AssertEquals("AUBNE", firstResult[4].Value);
		}

		public void TestParseKeyFields_OnlyIncludePK()
		{
			dynamic response = JsonConvert.DeserializeObject(GlowTestData.TestData);
			ICollection<GlowIndexQueryResult> results = GlowIndexQueryImpl.ParseKeyFields(response, true);
			AssertEquals(1, results.Count);

			var firstResult = results.First().KeyFields.ToArray();
			AssertEquals(0, firstResult.Length);
		}

		public void TestParseNonCommonIndexSearch()
		{
			dynamic response = JsonConvert.DeserializeObject(GlowTestData.TestNonCommonIndexSearch);
			ICollection<GlowIndexQueryResult> results = GlowIndexQueryImpl.ParseKeyFields(response);

			AssertEquals(1, results.Count);
			var result = results.First();

			CombineAssertions(() =>
			{
				AssertEquals("0f9eae9f-5a6c-426f-900e-f4fec569dc61", result.PK);
				AssertEquals("IHRJobApplicationDocument", result.EntityType);
				AssertEquals(0, result.KeyFields.Count);
			});
		}

		public void TestParseInlineCount()
		{
			dynamic response = JsonConvert.DeserializeObject(GlowTestData.QueryInlineCountTestData);
			int count = GlowIndexQueryImpl.ParseResultCount(response);

			AssertEquals(3, count);
		}

		public void TestValidateQueryParams_NullOrWhitespaceSearchTerms()
		{
			GlowIndexQueryStatus failureStatus;

			Assert(!GlowIndexQueryImpl.ValidateQueryParams(new GlowIndexQueryParam(), out failureStatus));
			AssertEquals(GlowIndexQueryStatus.BlankSearchTerms, failureStatus);

			Assert(!GlowIndexQueryImpl.ValidateQueryParams(new GlowIndexQueryParam(string.Empty, Constants.LUCENE_SEARCH_FIELD_COMMON), out failureStatus));
			AssertEquals(GlowIndexQueryStatus.BlankSearchTerms, failureStatus);
		}

		public void TestValidateQueryParams_UnknownEntityOrCategoryType()
		{
			SetUpGlowMetaData();

			Assert(
				"Searching with an unknown entity type should not validate.",
				!GlowIndexQueryImpl.ValidateQueryParams(new GlowIndexQueryParam("searchTerm", "INSURANCECURRENCY", "unknownEntityType"), out var failureStatus));
			AssertEquals(GlowIndexQueryStatus.UnknownEntityType, failureStatus);

			Assert(
				"Searching with an unknown entity category should not validate.",
				!GlowIndexQueryImpl.ValidateQueryParams(new GlowIndexQueryParam("searchTerm", new string[] { "INSURANCECURRENCY" }, "unknownCategoryType"), out failureStatus));
			AssertEquals(GlowIndexQueryStatus.UnknownCategoryType, failureStatus);
		}

		public void TestValidateQueryParams_UnknownSearchField()
		{
			SetUpGlowMetaData();

			Assert(
				"Searching with an unknown search field with valid entity type should not validate.",
				!GlowIndexQueryImpl.ValidateQueryParams(new GlowIndexQueryParam("searchTerm", "unknownSearchField", "IJobShipment"), out var failureStatus));
			AssertEquals(GlowIndexQueryStatus.UnknownSearchField, failureStatus);

			Assert(
				"Searching with an unknown search field with valid entity category should not validate.",
				!GlowIndexQueryImpl.ValidateQueryParams(new GlowIndexQueryParam("searchTerm", new string[] { "unknownSearchField" }, "APA"), out failureStatus));
			AssertEquals(GlowIndexQueryStatus.UnknownSearchField, failureStatus);
		}

		public void TestValidateQueryParams_Success()
		{
			SetUpGlowMetaData();

			Assert(
				"Searching with an known search field and entity type should validate.",
				GlowIndexQueryImpl.ValidateQueryParams(new GlowIndexQueryParam("food", "INSURANCECURRENCY", "IJobShipment"), out var failureStatus));
			AssertEquals(GlowIndexQueryStatus.Success, failureStatus);

			Assert(
				"Searching with an known search field and entity category should validate.",
				GlowIndexQueryImpl.ValidateQueryParams(new GlowIndexQueryParam("S0001001", new string[] { "JOBNUMBER", "MASTERBILLNUMBER", "HOUSEBILLNUMBER", "CONTAINERNUMBER" }, "APA"), out failureStatus));
			AssertEquals(GlowIndexQueryStatus.Success, failureStatus);
		}

		public void TestValidateQueryParams_EntityTypeAndCategoryTypeCoexist()
		{
			SetUpGlowMetaData();

			var queryParam = new GlowIndexQueryParamWithCategoryType("food", "INSURANCECURRENCY", "IJobShipment");
			Assert(
				"Searching with entity type together with category type should not validate.",
				!GlowIndexQueryImpl.ValidateQueryParams(queryParam, out var failureStatus));
			AssertEquals(GlowIndexQueryStatus.EntityTypeAndCategoryTypeCoexist, failureStatus);
		}

		class GlowIndexQueryParamWithCategoryType : GlowIndexQueryParam
		{
			public GlowIndexQueryParamWithCategoryType(string keyword, string searchField, string entityType = null) : base(keyword, searchField, entityType)
			{
			}
			public override string CategoryType => "APA";
		}

		public void TestDeserialiseResponse()
		{
			AssertNoExceptionThrown(() => GlowIndexQueryImpl.DeserialiseResponse(GlowTestData.TestData));
		}

		public void TestDeserialiseResponseJsonReaderException()
		{
			AssertNull(GlowIndexQueryImpl.DeserialiseResponse(GlowTestData.TestErrorNoFunctionSignature + "something extra"));
			AssertEquals($"Failed to deserialise glow response. RawGlowResponse={GlowTestData.TestErrorNoFunctionSignature}something extra", ErrorReporter.LastMessageReported);
			Assert("Last exception was expected to be JsonReaderException", ErrorReporter.LastExceptionReported is JsonReaderException);
			ErrorReporter.Clear();
		}

		public void TestGlowSearchFields()
		{
			var mockGlowWebRequest = new MockGlowWebRequest(GlowTestData.TestGlowSearchFields);

			using (ObjectFactory.Substitute<IGlowWebRequest>(mockGlowWebRequest))
			{
				Assert("SearchFields response should contain the expected search field.", GlowIndexQueryImpl.GlowSearchFields.Contains("BOOKINGPARTYCOMPANYNAME"));
			}
		}

		public void TestGlowEntityTypes()
		{
			var mockGlowWebRequest = new MockGlowWebRequest(GlowTestData.GlowEntityTypesTestData);

			using (ObjectFactory.Substitute<IGlowWebRequest>(mockGlowWebRequest))
			{
				Assert("EntityTypes response should contain the expected entity type.", GlowIndexQueryImpl.GlowEntityTypes.Any(e => e.Equals("IHRJOBAPPLICATIONDOCUMENT", StringComparison.OrdinalIgnoreCase)));
			}
		}

		public void TestGlowEntityCategories()
		{
			var mockGlowWebRequest = new MockGlowWebRequest(GlowTestData.GlowEntityCategoriesTestData);

			using (ObjectFactory.Substitute<IGlowWebRequest>(mockGlowWebRequest))
			{
				Assert("EntityCategories response should contain the expected entity type.", GlowIndexQueryImpl.GlowEntityCategories.Any(e => e.Equals("APA", StringComparison.OrdinalIgnoreCase)));
			}
		}

		public void TestGetGlowMetadata_EmptyResponse()
		{
			var mockGlowWebRequest = new MockGlowWebRequest(string.Empty);

			using (ObjectFactory.Substitute<IGlowWebRequest>(mockGlowWebRequest))
			{
				AssertNull("When something goes wrong, we should return null", GlowIndexQueryImpl.GetGlowMetadata(string.Empty));
			}
		}

		public void TestParseGlowMetaData_SearchFields()
		{
			var data = GlowTestData.TestGlowSearchFields;
			var result = GlowIndexQueryImpl.ParseGlowMetadata(Constants.LUCENE_SEARCH_FIELDS, data);
			AssertEquals(547, result.Count);
			Assert(result.Contains("APPLICATIONDOCUMENTCONTENT"));
		}

		public void TestParseGlowMetaData_EntityTypes()
		{
			var data = GlowTestData.GlowEntityTypesTestData;
			var result = GlowIndexQueryImpl.ParseGlowMetadata(Constants.LUCENE_ENTITY_TYPES, data);
			AssertEquals(41, result.Count);
			Assert("Resulting entity type hashset should contain this entity type", result.Any(e => e.Equals("IHRJOBAPPLICATIONDOCUMENT", StringComparison.OrdinalIgnoreCase)));
		}

		public void TestParseGlowMetaData_EntityCategories()
		{
			var data = GlowTestData.GlowEntityCategoriesTestData;
			var result = GlowIndexQueryImpl.ParseGlowMetadata(Constants.LUCENE_ENTITY_CATEGORIES, data);
			AssertEquals(16, result.Count);
			Assert("Resulting entity type hashset should contain this entity category", result.Any(e => e.Equals("APA", StringComparison.OrdinalIgnoreCase)));
		}

		public void TestGlowIndexQueryParamsDefaultResultCount()
		{
			var giqp = new GlowIndexQueryParam("S", Constants.LUCENE_SEARCH_FIELD_COMMON);
			AssertEquals(Constants.DEFAULT_QUERY_RESULTS_RETURNED, giqp.MaxQueryResults);
		}

		public void TestGlowIndexQueryParamsMaximumResultCount()
		{
			var userSpecifiedMaxResults = 1001;
			AssertGreaterThan(userSpecifiedMaxResults, Constants.MINIMUM_QUERY_RESULTS_RETURNED);
			var ex = AssertExceptionThrown<ArgumentOutOfRangeException>(() => new GlowIndexQueryParam("S", Constants.LUCENE_SEARCH_FIELD_COMMON, null, userSpecifiedMaxResults));
			AssertEquals("MaxQueryResults", ex.ParamName);
			AssertEquals(userSpecifiedMaxResults, ex.ActualValue);
			AssertEquals($"Max query result count cannot be > {Constants.MAXIMUM_QUERY_RESULTS_RETURNED}\r\nParameter name: MaxQueryResults\r\nActual value was 1001.", ex.Message);
		}

		public void TestGlowIndexQueryParamsMinimumResultCount()
		{
			var userSpecifiedMaxResults = -9001;
			AssertLessThan(userSpecifiedMaxResults, Constants.MINIMUM_QUERY_RESULTS_RETURNED);
			var ex = AssertExceptionThrown<ArgumentOutOfRangeException>(() => new GlowIndexQueryParam("S", Constants.LUCENE_SEARCH_FIELD_COMMON, null, userSpecifiedMaxResults));
			AssertEquals("MaxQueryResults", ex.ParamName);
			AssertEquals(userSpecifiedMaxResults, ex.ActualValue);
			AssertEquals($"Max query result count cannot be < {Constants.MINIMUM_QUERY_RESULTS_RETURNED}\r\nParameter name: MaxQueryResults\r\nActual value was -9001.", ex.Message);
		}

		public void TestGetSearchFields()
		{
			var mockGlowWebRequest = new MockGlowWebRequestWithCaption(GlowTestData.GetSearchFieldsByTypeTestData);

			using (ObjectFactory.Substitute<IGlowWebRequest>(mockGlowWebRequest))
			{
				var fields = GlowIndexQueryImpl.GetSearchFields("IGlbStaff");

				AssertEquals(3, fields.Value.Length);
				AssertEquals(3, mockGlowWebRequest.CurrentRequestCount);

				var nameField = fields.Value.FirstOrDefault(f => f.FieldName == "FULLNAME");
				AssertEquals("Name", nameField.Description);

				fields = GlowIndexQueryImpl.GetSearchFields("IGlbStaff");
				AssertEquals("not request caption when language not change", 5, mockGlowWebRequest.CurrentRequestCount);
			}
		}

		public void TestSearchFieldsNoAdditionalHeader()
		{
			var mockGlowWebRequest = new MockGlowWebRequest(GlowTestData.GlowEntityTypesTestData);

			using (ObjectFactory.Substitute<IGlowWebRequest>(mockGlowWebRequest))
			{
				Assert("EntityTypes response should contain the expected entity type.", GlowIndexQueryImpl.GlowEntityTypes.Any(e => e.Equals("IHRJOBAPPLICATIONDOCUMENT", StringComparison.OrdinalIgnoreCase)));
				Assert("Should not set additional header", !mockGlowWebRequest.HasAdditionalHeader);
			}
		}

		public void TestGetSearchFields_LanguageSwitch()
		{
			var mockGlowWebRequest = new MockGlowWebRequestWithCaption(GlowTestData.GetSearchFieldsByTypeTestData);

			using (ObjectFactory.Substitute<IGlowWebRequest>(mockGlowWebRequest))
			{
				using (Res.TemporarilySwitchLanguage("EN"))
				{
					var fields = GlowIndexQueryImpl.GetSearchFields("IGlbStaff");

					AssertEquals(3, fields.Value.Length);

					var nameFields = fields.Value.FirstOrDefault(f => f.FieldName == "FULLNAME");
					AssertEquals("Name", nameFields.Description);
				}

				using (Res.TemporarilySwitchLanguage("ZH-CN"))
				{
					var fields = GlowIndexQueryImpl.GetSearchFields("IGlbStaff");

					AssertEquals(3, fields.Value.Length);

					var nameFiled = fields.Value.FirstOrDefault(f => f.FieldName == "FULLNAME");
					AssertEquals("姓名", nameFiled.Description);
				}
			}
		}

		public void TestGetSearchFieldsWithLookup_EntityLookup()
		{
			var mockGlowWebRequest = new MockGlowWebRequestWithCaption(GlowTestData.GetSearchFieldsByTypeTestData);
			using (ObjectFactory.Substitute<IGlowWebRequest>(mockGlowWebRequest))
			{
				var fields = GlowIndexQueryImpl.GetSearchFields("IGlbStaff");
				var codeField = fields.Value.Where(v => v.FieldName == "CODE").FirstOrDefault();
				AssertNotNull(codeField?.EntityLookup);
				AssertEquals("A", codeField.EntityLookup.EntityType);
				AssertEquals("B", codeField.EntityLookup.ValuePath);
				AssertEquals("C", codeField.EntityLookup.TableName);
				AssertEquals("D", codeField.EntityLookup.HierarchyColumnName);
				AssertEquals("E", codeField.EntityLookup.HierarchyColumnValue);
			}
		}

		public void TestGetSearchFieldsWithLookup_RuleLookup()
		{
			var mockGlowWebRequest = new MockGlowWebRequestWithCaption(GlowTestData.GetSearchFieldsByTypeTestData);
			using (ObjectFactory.Substitute<IGlowWebRequest>(mockGlowWebRequest))
			{
				var fields = GlowIndexQueryImpl.GetSearchFields("IGlbStaff");
				var nameField = fields.Value.Where(v => v.FieldName == "FULLNAME").FirstOrDefault();
				var list = nameField.GetListDelegate();

				AssertNotNull(nameField?.RuleLookup);
				AssertEquals(new Guid("ccafad48-0b7a-42b5-8d07-9022f25cbe8b"), nameField.RuleLookup.RuleId);
				AssertEquals("api/rules/IEntityInfo/ccafad48-0b7a-42b5-8d07-9022f25cbe8b", mockGlowWebRequest.LastRequestUrl);

				AssertEquals(2, list.Count);
				AssertEquals("OPN", list[0].Code);
				AssertEquals("Open Pending Allocation", list[0].Description);

				AssertEquals("ASN", list[1].Code);
				AssertEquals("Assigned", list[1].Description);
			}
		}

		public void TestBlankSearchField()
		{
			var mock = new MockGlowWebRequest(GlowTestData.GetSearchFieldsWithEmptyValue);

			using (ObjectFactory.Substitute<IGlowWebRequest>(mock))
			{
				var fields = GlowIndexQueryImpl.GetSearchFields("IGlbStaff");

				AssertEquals(true, fields.Value.IsEmpty);
				AssertEquals(GlowIndexQueryStatus.UnknownSearchField, fields.Status);
				AssertEquals("BlankSearchFieldFromGlow", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			}
		}

		public void TestSearchFieldNameIsNull()
		{
			var mock = new MockGlowWebRequest(GlowTestData.GetSearchFieldsWithNullValue);

			using (ObjectFactory.Substitute<IGlowWebRequest>(mock))
			{
				var fields = GlowIndexQueryImpl.GetSearchFields("IGlbStaff");

				AssertEquals(GlowIndexQueryStatus.UnknownSearchField, fields.Status);
				AssertEquals("BlankSearchFieldFromGlow", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
			}
		}

		public void TestGetListByRuleIdWithError()
		{
			var mock = new MockGlowWebRequest(GlowTestData.LookupListErrorData);

			using (ObjectFactory.Substitute<IGlowWebRequest>(mock))
			{
				var list = GlowIndexQueryImpl.GetListByRuleId(new Guid("ccafad48-0b7a-42b5-8d07-9022f25cbe8b"));
				AssertEquals(0, list.Count);
			}
		}

		public void TestGetListByRuleId_CacheResult()
		{
			var mock = new MockGlowWebRequest(GlowTestData.LookupListData);

			using (ObjectFactory.Substitute<IGlowWebRequest>(mock))
			{
				GlowIndexQueryImpl.GetListByRuleId(new Guid("ccafad48-0b7a-42b5-8d07-9022f25cbe8b"));
				GlowIndexQueryImpl.GetListByRuleId(new Guid("ccafad48-0b7a-42b5-8d07-9022f25cbe8b"));
				AssertEquals(1, mock.CurrentRequestCount);
			}
		}

		public void TestQueryWhenGlowIndexRegistryFalse()
		{
			SetUpGlowMetaData();

			using (GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Substitute<IGlowWebRequest>(new MockGlowWebRequest(GlowTestData.TestData)))
			{
				var results = GlowIndexQueryImpl.Query(new GlowIndexQueryParam("S", Constants.LUCENE_SEARCH_FIELD_COMMON));
				AssertEquals(GlowIndexQueryStatus.Success, results.Status);
			}
		}
	}
}

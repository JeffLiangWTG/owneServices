using Nest;

namespace ZClientEDI.ElasticSearchAdaptor.Test
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using Enterprise.Integration;
	using Moq;
	using Moq.Protected;
	using Newtonsoft.Json;
	using NUnit.Framework;

	public class ElasticSearchAdaptorTest : TestCase
	{
		public void TestESNotAvailable()
		{
			var logger = new MockLogger();
			using (ElasticSearchAdaptor.SetSettingsProviderForTest(new ElasticSearchAdaptorSettingsProviderForTest()))
			{
				AssertNoExceptionThrown(() => ElasticSearchAdaptor.GetAggAlertPerDay(new DateTime(2019, 9, 17), 1, serviceLogger: logger));
				AssertEquals(LogType.Error, logger.LogType);
				Assert(!string.IsNullOrEmpty(logger.Message));
			}
		}

		public void TestESAccessible()
		{
			var logger = new MockLogger();
			using (ElasticSearchAdaptor.SetSettingsProviderForTest(new ValidElasticSearchAdaptorSettingsProviderForTest()))
			{
				AssertNotNull(ElasticSearchAdaptor.GetAggAlertPerDay(new DateTime(2019, 9, 17), 1, serviceLogger: logger));
				AssertEquals(LogType.Information, logger.LogType);
				AssertEquals(string.Empty, logger.Message);
			}
		}

		public void TestESAccessiblePerClient()
		{
			var logger = new MockLogger();
			using (ElasticSearchAdaptor.SetSettingsProviderForTest(new ValidElasticSearchAdaptorSettingsProviderForTest()))
			{
				var result = ElasticSearchAdaptor.GetAlertPerDayPerClient(DateTime.UtcNow, 1, serviceLogger: logger);
				Assert(result?.Terms("topLevelAggregation")?.Buckets.Any(systemBucket => systemBucket.Terms("SubAggregation1")?.Buckets.Any(onwerBucket => onwerBucket.Terms("SubAggregationClient")?.Buckets.Count > 0) ?? false) ?? false);
				AssertEquals(LogType.Information, logger.LogType);
				AssertEquals(string.Empty, logger.Message);
			}
		}

		public void TestAdaptorGetAlertPerDayWithCapabilityFilter()
		{
			var responseContent = "{\"responses\":[{\"took\":205,\"timed_out\":false,\"_shards\":{\"total\":2425,\"successful\":2425,\"skipped\":2420,\"failed\":0},\"hits\":{\"total\":286262,\"max_score\":0.0,\"hits\":[]},\"aggregations\":{\"topLevelAggregation\":{\"doc_count_error_upper_bound\":0,\"sum_other_doc_count\":0,\"buckets\":[{\"key\":\"Accounting\",\"doc_count\":156671,\"SubAggregation1\":{\"doc_count_error_upper_bound\":-1,\"sum_other_doc_count\":156217,\"buckets\":[{\"key\":\"F972CCF2410A6FBE\",\"doc_count\":450,\"SubAggregation2\":{\"doc_count_error_upper_bound\":0,\"sum_other_doc_count\":0,\"buckets\":[{\"key\":\"CW1\",\"doc_count\":450,\"11\":{\"value\":2.0011019001E10},\"5\":{\"value\":185037.0},\"6\":{\"value\":5.88183393E9},\"7\":{\"value\":91897.0},\"8\":{\"value\":1.606866149733E12,\"value_as_string\":\"2020-12-01T23:42:29.733Z\"},\"9\":{\"value\":1.60274886E12,\"value_as_string\":\"2020-10-15T08:01:00.000Z\"},\"10\":{\"value\":1.60638894E12,\"value_as_string\":\"2020-11-26T11:09:00.000Z\"},\"AggCPUTimeMS\":{\"value\":4941132.0}}]},\"AggCPUTimeMS\":{\"value\":4941132.0}}]},\"AggCPUTimeMS\":{\"value\":8.8159379E7}},{\"key\":\"Forwarding\",\"doc_count\":129591,\"SubAggregation1\":{\"doc_count_error_upper_bound\":-1,\"sum_other_doc_count\":129567,\"buckets\":[{\"key\":\"13322696842015950818\",\"doc_count\":22,\"SubAggregation2\":{\"doc_count_error_upper_bound\":0,\"sum_other_doc_count\":0,\"buckets\":[{\"key\":\"CW1\",\"doc_count\":22,\"11\":{\"value\":2.0011019001E10},\"5\":{\"value\":22.0},\"6\":{\"value\":3.96307631E8},\"7\":{\"value\":12421.0},\"8\":{\"value\":1.606856460717E12,\"value_as_string\":\"2020-12-01T21:01:00.717Z\"},\"9\":{\"value\":1.60069536E12,\"value_as_string\":\"2020-09-21T13:36:00.000Z\"},\"10\":{\"value\":1.60638894E12,\"value_as_string\":\"2020-11-26T11:09:00.000Z\"},\"AggCPUTimeMS\":{\"value\":2964574.0}}]},\"AggCPUTimeMS\":{\"value\":2964574.0}}]},\"AggCPUTimeMS\":{\"value\":5.1333563E7}}]}},\"status\":200}]}";
			var result = JsonConvert.DeserializeObject<SearchResponse>(responseContent);
			AssertNotNull(result);
			Assert(result.Result.Length > 0);
			var subSystemBuckets = result.Result.First().Aggregations.topLevelAggregation.Buckets;
			Assert(subSystemBuckets.Length <= 2);
			Assert(subSystemBuckets.Where(ssBucket => ssBucket.SubAggregation1.Buckets.Length > 0).All(ssBucket => ssBucket.SubAggregation1.Buckets[0]?.Key != "0"));
			Assert(subSystemBuckets.Where(ssBucket => ssBucket.SubAggregation1.Buckets.Length > 0).All(ssBucket => ssBucket.SubAggregation1.Buckets[0]?.SubAggregation2.Buckets[0]?.Key != "Customer"));
			Assert(subSystemBuckets.Where(ssBucket => ssBucket.SubAggregation1.Buckets.Length > 0).All(ssBucket =>
			{
				var currentVersionAsDouble = ssBucket.SubAggregation1.Buckets[0]?.SubAggregation2.Buckets[0]?.Slot11?.Value;
				return currentVersionAsDouble != null && string.CompareOrdinal(ElasticSearchAdaptor.ConvertDoubleToVersionString(currentVersionAsDouble.Value), new Version().ToString()) != 0;
			}));
		}

		public void TestAdaptorGetAlertDeserialization()
		{
			using (ElasticSearchAdaptor.SetSettingsProviderForTest(new ElasticSearchAdaptorSettingsProviderForTestESResponse()))
			{
				AssertNoExceptionThrown(() => ElasticSearchAdaptor.GetAggAlertPerDay(new DateTime(2019, 9, 17), 1));
			}
		}

		[DeveloperOnlyTest]
		public void TestAdaptorGetAlertWithServerInstanceNameAndQueryHash()
		{
			using (ElasticSearchAdaptor.SetSettingsProviderForTest(new ValidElasticSearchAdaptorSettingsProviderForTest()))
			{
				AssertServerInstanceNameAndQueryHashExistInAggregateDictionary(ElasticSearchAdaptor.GetAggAlertPerDay(DateTime.UtcNow, 1));
				AssertServerInstanceNameAndQueryHashExistInAggregateDictionary(ElasticSearchAdaptor.GetAlertPerDayPerClient(DateTime.UtcNow, 1));
			}
		}

		void AssertServerInstanceNameAndQueryHashExistInAggregateDictionary(AggregateDictionary aggregateDictionary)
		{
			var topLevelAggregationBuckets = aggregateDictionary.Terms("topLevelAggregation").Buckets;

			foreach (var systemBucket in topLevelAggregationBuckets)
			{
				foreach (var ownerBucket in systemBucket.Terms("SubAggregation1").Buckets)
				{
					var clientBuckets = ownerBucket.Terms("SubAggregationClient")?.Buckets;
					if (clientBuckets != null)
					{
						foreach (var clientBucket in clientBuckets)
						{
							AssertServerInstanceNameAndQueryHashExistInQueryHashBuckets(clientBucket.Terms("SubAggregation2")?.Buckets);
						}
					}
					else
					{
						AssertServerInstanceNameAndQueryHashExistInQueryHashBuckets(ownerBucket.Terms("SubAggregation2")?.Buckets);
					}
				}
			}
		}

		void AssertServerInstanceNameAndQueryHashExistInQueryHashBuckets(IReadOnlyCollection<KeyedBucket<string>> queryHashBuckets)
		{
			if (queryHashBuckets == null)
			{
				return;
			}
			foreach (var queryHashBucket in queryHashBuckets)
			{
				var maxCpuTimeHit = queryHashBucket.TopHits("MaxCpuTimeHit")?.Documents<Dictionary<string, object>>()?.FirstOrDefault();
				AssertNotNull(maxCpuTimeHit);
				Assert("The TopHits of MaxCpuTimeHit should contain the field ServerInstanceName", maxCpuTimeHit.ContainsKey("ServerInstanceName"));
				Assert("The TopHits of MaxCpuTimeHit should contain the field QueryHash", maxCpuTimeHit.ContainsKey("QueryHash"));
			}
		}
	}

	class MockLogger : ILogger
	{
		void ILogger.Log(LogType type, string message)
		{
			LogType = type;
			Message = message;
		}

		void ILogger.Log(LogType type, string message, Exception ex)
		{
			LogType = type;
			Message = message;
		}

		public LogType LogType { get; private set; } = LogType.Information;
		public string Message { get; private set; } = string.Empty;
	}

	class ElasticSearchAdaptorSettingsProviderForTest : IElasticSearchAdaptorSettingsProvider
	{
		public double RequestTimeout => 300000;

		public string RequestUrl => "https://r.prod-1.es.wtg.ws";

		public string User => "user";

		public string Pwd => "password";

		public string SingleHashRequestString => string.Empty;

		public string FullRequestString => string.Empty;

		public string FullRequestStringWithCapabilityFilter => string.Empty;
		public virtual HttpClient httpClient => new HttpClient();

		public string Index => string.Empty;

		public long LowerThreshold => 0;
	}

	class ValidElasticSearchAdaptorSettingsProviderForTest : IElasticSearchAdaptorSettingsProvider
	{
		public double RequestTimeout => 300000;

		public string RequestUrl => "https://r.prod-1.es.wtg.ws";

		public string User => "ediprodreader";

		public string Pwd => "Mh5MDAmGb4dkqAhMv8WMeSD4hZDWRHf8";

		public string SingleHashRequestString => string.Empty;

		public string FullRequestString => "{\"index\":[\"wisecloud-syd-systemusage-2*\"],\"ignore_unavailable\":true,\"preference\":1521967565949}\r\n{\"size\":0,\"_source\":{\"excludes\":[]},\"aggs\":{\"topLevelAggregation\":{\"terms\":{\"field\":\"System.keyword\",\"size\":35,\"missing\":\"__missing__\"},\"aggs\":{\"AggCPUTimeMS\":{\"sum\":{\"field\":\"CpuTimeMilliseconds\"}},\"SubAggregation1\":{\"terms\":{\"field\":\"QueryHash.keyword\",\"size\":{MaxAlertPerDayPerSystem},\"order\":{\"AggCPUTimeMS\":\"desc\"}},\"aggs\":{\"AggCPUTimeMS\":{\"sum\":{\"field\":\"CpuTimeMilliseconds\"}},\"SubAggregation2\":{\"terms\":{\"field\":\"Owner.keyword\",\"size\":1,\"order\":{\"AggCPUTimeMS\":\"desc\"},\"missing\":\"__missing__\"},\"aggs\":{\"AggCPUTimeMS\":{\"sum\":{\"field\":\"CpuTimeMilliseconds\"}},\"5\":{\"sum\":{\"field\":\"Qty\"}},\"6\":{\"sum\":{\"field\":\"LogicalReads\"}},\"7\":{\"sum\":{\"field\":\"Writes\"}},\"8\":{\"max\":{\"field\":\"CollectSystemTimeUtc\"}},\"9\":{\"min\":{\"field\":\"ExeDate\"}},\"10\":{\"max\":{\"field\":\"ExeDate\"}},\"11\":{\"max\":{\"script\":{\"lang\":\"painless\",\"source\":\"String versionString=doc['CurrentVersion.keyword'].value;double result=0;int offset=0;int next=0;for(int i=3;i>=0;i--){next=versionString.indexOf('.',offset);if(next==-1)next=versionString.length();result=result+Integer.parseInt(versionString.substring(offset,next))*Math.pow(1000,i);offset=next+1}return result;\"}}}}}}}}}},\"stored_fields\":[\"*\"],\"script_fields\":{},\"docvalue_fields\":[\"CollectSystemTimeUtc\"],\"query\":{\"bool\":{\"must\":[{\"match_all\":{}},{\"range\":{\"ExeDate\":{\"gt\":\"now-3M\"}}},{\"range\":{\"CollectSystemTimeUtc\":{\"gte\":\"{StartDate}\",\"lte\":\"{EndDate}\",\"format\":\"epoch_millis\"}}},{\"terms\":{\"System\":[{CapabilityList}]}}],\"filter\":[],\"should\":[],\"must_not\":[{\"term\":{\"QueryHash\":\"0\"}},{\"term\":{\"Owner\":\"customer\"}}]}}}\r\n";

		public string FullRequestStringWithCapabilityFilter => string.Empty;
		public virtual HttpClient httpClient => new HttpClient();

		public string Index => "idx-au2-prod-sqlcpumonitoring-prod";

		public long LowerThreshold => 0;
	}

	class ElasticSearchAdaptorSettingsProviderForTestESResponse : ElasticSearchAdaptorSettingsProviderForTest
	{
		public override HttpClient httpClient => new HttpClient(GetMockHttpMsgHandler());
		HttpMessageHandler GetMockHttpMsgHandler()
		{
			// mock: invalid response string
			var mockHttpMsgHandler = new Mock<HttpMessageHandler>();
			var responseContent = @"{""responses"":[{""took"":""invalid_string"",""timed_out"":""invalid_string"",""status"":""invalid_string"",""hits"":1.1,""aggregations"":1.1}]}";

			mockHttpMsgHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(responseContent) });

			return mockHttpMsgHandler.Object;
		}
	}
}

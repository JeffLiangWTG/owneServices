using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Enterprise.Client.EDI;

namespace ZClientEDI.ElasticSearchAdaptor
{
	public interface IElasticSearchAdaptorSettingsProvider
	{
		double RequestTimeout { get; }
		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		string RequestUrl { get; }
		string Index { get; }
		string User { get; }
		string Pwd { get; }
		long LowerThreshold { get; }
		string SingleHashRequestString { get; }
		string FullRequestString { get; }
		string FullRequestStringWithCapabilityFilter { get; }
		HttpClient httpClient { get; }
	}

	class ElasticSearchAdaptorSettingsProvider : IElasticSearchAdaptorSettingsProvider
	{
		public double RequestTimeout => 300000;

		public string RequestUrl => EDIDataRegistry.Instance.ElasticSearchAdaptorRequestUrl.Value;

		public string Index => EDIDataRegistry.Instance.ElasticSearchAdaptorIndex.Value;
		public string User => EDIDataRegistry.Instance.ElasticSearchAdaptorUsername.Value;

		public string Pwd => EDIDataRegistry.Instance.ElasticSearchAdaptorPassword.Value;

		public long LowerThreshold => EDIDataRegistry.Instance.ExternalMonitoringSqlCpuUsageLowerThreshold.Value * 1000;

		public string SingleHashRequestString => "{\"index\":[\"wisecloud-syd-systemusage-2*\"],\"ignore_unavailable\":true,\"preference\":1521967565949}\r\n{\"size\":1,\"_source\":{\"excludes\":[]},\"stored_fields\":[\"*\"],\"script_fields\":{},\"docvalue_fields\":[\"CollectSystemTimeUtc\"],\"query\":{\"bool\":{\"must\":[{\"match_all\":{}},{\"range\":{\"CollectSystemTimeUtc\":{\"gte\":\"{StartDate}\",\"lte\":\"{EndDate}\",\"format\":\"basic_date\"}}},{\"term\":{\"QueryHash\":\"{QueryHashID}\"}}],\"filter\":[],\"should\":[],\"must_not\":[]}}}\r\n";

		public string FullRequestStringWithCapabilityFilter => EDIDataRegistry.Instance.ElasticSearchAdaptorRequestStringWithFilter.Value;
		public string FullRequestString => "{\"index\":[\"wisecloud-syd-systemusage-2*\"],\"ignore_unavailable\":true,\"preference\":1521967565949}\r\n{\"size\":0,\"_source\":{\"excludes\":[]},\"aggs\":{\"topLevelAggregation\":{\"terms\":{\"field\":\"System.keyword\",\"size\":35,\"missing\":\"__missing__\"},\"aggs\":{\"AggCPUTimeMS\":{\"sum\":{\"field\":\"CpuTimeMilliseconds\"}},\"SubAggregation1\":{\"terms\":{\"field\":\"QueryHash.keyword\",\"size\":{MaxAlertPerDayPerSystem},\"order\":{\"AggCPUTimeMS\":\"desc\"}},\"aggs\":{\"AggCPUTimeMS\":{\"sum\":{\"field\":\"CpuTimeMilliseconds\"}},\"SubAggregation2\":{\"terms\":{\"field\":\"Owner.keyword\",\"size\":1,\"order\":{\"AggCPUTimeMS\":\"desc\"},\"missing\":\"__missing__\"},\"aggs\":{\"AggCPUTimeMS\":{\"sum\":{\"field\":\"CpuTimeMilliseconds\"}},\"5\":{\"sum\":{\"field\":\"Qty\"}},\"6\":{\"sum\":{\"field\":\"LogicalReads\"}},\"7\":{\"sum\":{\"field\":\"Writes\"}},\"8\":{\"max\":{\"field\":\"CollectSystemTimeUtc\"}},\"9\":{\"min\":{\"field\":\"ExeDate\"}},\"10\":{\"max\":{\"field\":\"ExeDate\"}}}}}}}}},\"stored_fields\":[\"*\"],\"script_fields\":{},\"docvalue_fields\":[\"CollectSystemTimeUtc\"],\"query\":{\"bool\":{\"must\":[{\"match_all\":{}},{\"range\":{\"CollectSystemTimeUtc\":{\"gte\":\"{StartDate}\",\"lte\":\"{EndDate}\",\"format\":\"epoch_millis\"}}}],\"filter\":[],\"should\":[],\"must_not\":[{\"term\":{\"QueryHash\":\"0\"}},{\"term\":{\"Owner\":\"customer\"}}]}}}\r\n";
		public HttpClient httpClient => new HttpClient();
	}
}

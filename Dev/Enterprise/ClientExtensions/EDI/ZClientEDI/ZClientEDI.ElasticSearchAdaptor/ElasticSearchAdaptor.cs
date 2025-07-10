using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CargoWise.Common;
using Enterprise.Integration;
using Nest;
using Newtonsoft.Json;
using WTG.StaticAnalysis.Annotation;

namespace ZClientEDI.ElasticSearchAdaptor
{
	public static class ElasticSearchAdaptor
	{
		[ThreadSafe] //Readonly except for test
		static IElasticSearchAdaptorSettingsProvider settingsProvider = new ElasticSearchAdaptorSettingsProvider();

		internal static IDisposable SetSettingsProviderForTest(IElasticSearchAdaptorSettingsProvider testSettingsProvider)
		{
			var originalSettingsProvider = settingsProvider;
			settingsProvider = testSettingsProvider;
			return new DisposableAction(() => settingsProvider = originalSettingsProvider);
		}

		public static AggregateDictionary GetAggAlertPerDay(DateTime targetDate, int maxAlertPerDayPerSystem, IEnumerable<string> capabilites = null, ILogger serviceLogger = null)
		{
			if (maxAlertPerDayPerSystem > 0)
			{
				var startDate = targetDate.AddDays(-1);
				var endDate = targetDate;
				return GetAlert(startDate, endDate, capabilites, maxAlertPerDayPerSystem, serviceLogger);
			}
			return null;
		}

		public static AggregateDictionary GetAlertPerDayPerClient(DateTime targetDate, int maxAlertPerDayPerSystem, IEnumerable<string> capabilites = null, ILogger serviceLogger = null)
		{
			if (maxAlertPerDayPerSystem > 0)
			{
				var startDate = targetDate.AddDays(-1);
				var endDate = targetDate;
				return GetAlertPerClient(startDate, endDate, capabilites, maxAlertPerDayPerSystem, serviceLogger);
			}
			return null;
		}

		[SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		static AggregateDictionary GetAlert(DateTime startDate, DateTime endDate, IEnumerable<string> capabilites, int maxAlertPerDayPerSystem, ILogger serviceLogger = null)
		{
			var settings = new ConnectionSettings(new Uri(settingsProvider.RequestUrl));
			settings.BasicAuthentication(settingsProvider.User, settingsProvider.Pwd)
				.DefaultIndex(settingsProvider.Index + "*");
			var client = new ElasticClient(settings);

			var response = client.Search<object>(search => search
				.Size(0)
				.RequestConfiguration(r => r.DisableDirectStreaming())
				.Aggregations(aggContainer => aggContainer
					.Terms("topLevelAggregation", termsAgg => termsAgg
						.Field(new Field("System.keyword"))
						.Size(35)
						.Aggregations(aggContainer1 => aggContainer1
							.Terms("SubAggregation1", termsAgg1 => termsAgg1
								.Field(new Field("Owner.keyword"))
								.Aggregations(aggContainer2 => aggContainer2
									.Terms("SubAggregation2", termsAgg2 => termsAgg2
										.Field(new Field("WtgQueryHash.keyword"))
										.Size(maxAlertPerDayPerSystem)
										.Aggregations(aggContainer3 => aggContainer3
											.Sum("AggCPUTimeMS2", sumAgg => sumAgg
												.Field(new Field("CpuTimeMilliseconds"))
											)
											.Sum("5", sumAgg => sumAgg
												.Field(new Field("Qty"))
											)
											.Sum("6", sumAgg => sumAgg
												.Field(new Field("LogicalReads"))
											)
											.Sum("7", sumAgg => sumAgg
												.Field(new Field("Writes"))
											)
											.Max("8", sumAgg => sumAgg
												.Field(new Field("CollectSystemTimeUtc"))
											)
											.Min("9", sumAgg => sumAgg
												.Field(new Field("ExeDate"))
											)
											.Max("10", sumAgg => sumAgg
												.Field(new Field("ExeDate"))
											)
											.Max("11", sumAgg => sumAgg
												.Script(script => script.Source("double result=0;if(doc.containsKey('CurrentVersion') && doc['CurrentVersion.keyword'] != null && doc['CurrentVersion.keyword'].length > 0) { String versionString=doc['CurrentVersion.keyword'].value; int offset=0; int next=0; for(int i=3;i>=0;i--) { next=versionString.indexOf('.',offset); if(next==-1) next=versionString.length(); result=result+Integer.parseInt(versionString.substring(offset,next))*Math.pow(1000,i); offset=next+1 } } return result;"))
											)
											.TopHits("MaxCpuTimeHit", topHit => topHit
												.Size(1)
												.Sort(srt => srt
													.Descending(new Field("CpuTimeMilliseconds")))
												.Source(src => src
													.Includes(filter => filter
														.Field(new Field("CpuTimeMilliseconds"))
														.Field(new Field("ServerInstanceName"))
														.Field(new Field("QueryHash"))))
											)
											.BucketSelector("cpuTimeBucketFilter", selector => selector
												.BucketsPath(path => path.Add("totalCpuTimePerQueryHash", "AggCPUTimeMS2"))
												.Script($"params.totalCpuTimePerQueryHash > {settingsProvider.LowerThreshold}")
											)
										)
										.Order(o => o.Descending("AggCPUTimeMS2"))
									)
								)
							)
						)
					)
				)
				.Query(query => query
					.Bool(boolQuery => boolQuery
						.Must(queryMust =>
							queryMust.MatchAll() &&
							queryMust
								.Bool(boolQuery => boolQuery
									.Should(shouldQuery =>
										shouldQuery.DateRange(date => date
											.Field("ExeDate")
											.GreaterThan(DateTime.UtcNow.AddMonths(-3))) ||
										!shouldQuery.Exists(field => field.Field("ExeDate"))
									)
								) &&
							queryMust.DateRange(date => date
								.Field("CollectSystemTimeUtc")
								.GreaterThanOrEquals(startDate)
								.LessThanOrEquals(endDate)) &&
							queryMust.Terms(termsQuery => termsQuery
								.Field("System.keyword")
								.Terms(capabilites ?? Array.Empty<string>()))
						)
						.MustNot(queryMustNot =>
							queryMustNot.Term("WtgQueryHash", "0") ||
							queryMustNot.Term("Owner", "customer")
						)
					)
				)
			);
			if (!response.IsValid)
			{
				serviceLogger?.Error(response.DebugInformation);
			}
			return response.Aggregations;
		}

		[SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		static AggregateDictionary GetAlertPerClient(DateTime startDate, DateTime endDate, IEnumerable<string> capabilites, int maxAlertPerDayPerSystem, ILogger serviceLogger = null)
		{
			var settings = new ConnectionSettings(new Uri(settingsProvider.RequestUrl));
			settings.BasicAuthentication(settingsProvider.User, settingsProvider.Pwd)
				.DefaultIndex(settingsProvider.Index + "*");
			settings.RequestTimeout(TimeSpan.FromMilliseconds(settingsProvider.RequestTimeout));
			var client = new ElasticClient(settings);

			var response = client.Search<object>(search => search
				.Size(0)
				.RequestConfiguration(r => r.DisableDirectStreaming())
				.Aggregations(aggContainer => aggContainer
					.Terms("topLevelAggregation", termsAgg => termsAgg
						.Field(new Field("System.keyword"))
						.Size(35)
						.Aggregations(aggContainerSum => aggContainerSum
							.Terms("SubAggregation1", termsAgg1 => termsAgg1
								.Field(new Field("Owner.keyword"))
								.Aggregations(aggContainer2 => aggContainer2
									.Terms("SubAggregationClient", termsAggClient => termsAggClient
										.Script("def dbName = doc['DatabaseName.keyword'].value.toUpperCase(); def result = null;  if (dbName != null) { def n = /(ODYSSEY[A-Z0-9]{6})/.matcher(dbName); if (n.find()) { result = n.group(1); } else { def eventName = doc['EventName.keyword'].value.toUpperCase(); if (eventName != 'DMV') { def serverPrincipalName = doc['ServerPrincipalName.keyword'].value.toUpperCase(); if (serverPrincipalName != null) { n = /(ODYSSEY[A-Z0-9]{6})/.matcher(serverPrincipalName); if (n.find()) { result = n.group(1); } } } if (result == null) { def source = doc['SqlText.keyword']; if (source.size() > 0 && source.size() < 500) { def sqlText = source.value.toUpperCase(); n = /(ODYSSEY[A-Z0-9]{6})/.matcher(sqlText); if (n.find()) { result = n.group(1); } } } if (result == null) { result = dbName; } } } return result;")
										.Order(o => o.Descending("AggCPUTimeMSClient"))
										.Size(maxAlertPerDayPerSystem)
										.Aggregations(aggContainerClientSum => aggContainerClientSum
											.Sum("AggCPUTimeMSClient", sumAggClient => sumAggClient
												.Field(new Field("CpuTimeMilliseconds"))
											)
											.Terms("SubAggregation2", termsAgg2 => termsAgg2
												.Field(new Field("WtgQueryHash.keyword"))
												.Size(1)
												.Aggregations(aggContainer3 => aggContainer3
													.Sum("AggCPUTimeMS2", sumAgg => sumAgg
														.Field(new Field("CpuTimeMilliseconds"))
													)
													.Sum("5", sumAgg => sumAgg
														.Field(new Field("Qty"))
													)
													.Sum("6", sumAgg => sumAgg
														.Field(new Field("LogicalReads"))
													)
													.Sum("7", sumAgg => sumAgg
														.Field(new Field("Writes"))
													)
													.Max("8", sumAgg => sumAgg
														.Field(new Field("CollectSystemTimeUtc"))
													)
													.Min("9", sumAgg => sumAgg
														.Field(new Field("ExeDate"))
													)
													.Max("10", sumAgg => sumAgg
														.Field(new Field("ExeDate"))
													)
													.Max("11", sumAgg => sumAgg
														.Script(script => script.Source("double result=0;if(doc.containsKey('CurrentVersion') && doc['CurrentVersion.keyword'] != null && doc['CurrentVersion.keyword'].length > 0) { String versionString=doc['CurrentVersion.keyword'].value; int offset=0; int next=0; for(int i=3;i>=0;i--) { next=versionString.indexOf('.',offset); if(next==-1) next=versionString.length(); result=result+Integer.parseInt(versionString.substring(offset,next))*Math.pow(1000,i); offset=next+1 } } return result;"))
													)
													.TopHits("MaxCpuTimeHit", topHit => topHit
														.Size(1)
														.Sort(srt => srt
															.Descending(new Field("CpuTimeMilliseconds")))
														.Source(src => src
															.Includes(filter => filter
																.Field(new Field("CpuTimeMilliseconds"))
																.Field(new Field("ServerInstanceName"))
																.Field(new Field("QueryHash"))))
													)
													.BucketSelector("cpuTimeBucketFilter", selector => selector
														.BucketsPath(path => path.Add("totalCpuTimePerQueryHash", "AggCPUTimeMS2"))
														.Script($"params.totalCpuTimePerQueryHash > {settingsProvider.LowerThreshold}")
													)
												)
												.Order(o => o.Descending("AggCPUTimeMS2"))
											)
										)
									)
								)
							)
						)
					)
				)
				.Query(query => query
					.Bool(boolQuery => boolQuery
						.Must(queryMust =>
							queryMust.MatchAll() &&
							queryMust
								.Bool(boolQuery => boolQuery
									.Should(shouldQuery =>
										shouldQuery.DateRange(date => date
											.Field("ExeDate")
											.GreaterThan(DateTime.UtcNow.AddMonths(-3))) ||
										!shouldQuery.Exists(field => field.Field("ExeDate"))
									)
								) &&
							queryMust.DateRange(date => date
								.Field("CollectSystemTimeUtc")
								.GreaterThanOrEquals(startDate)
								.LessThanOrEquals(endDate)) &&
							queryMust.Terms(termsQuery => termsQuery
								.Field("System.keyword")
								.Terms(capabilites ?? Array.Empty<string>()))
						)
						.MustNot(queryMustNot =>
							queryMustNot.Term("WtgQueryHash", "0") ||
							queryMustNot.Term("Owner", "customer")
						)
					)
				)
			);
			if (!response.IsValid)
			{
				serviceLogger?.Error(response.DebugInformation);
			}
			return response.Aggregations;
		}

		public static string ConvertDoubleToVersionString(double? versionAsDouble, ILogger serviceLogger = null)
		{
			if (versionAsDouble == null || (versionAsDouble > Math.Pow(1000, 4) && versionAsDouble < Math.Pow(1000, 3)))
			{
				serviceLogger?.Log(LogType.Warning, string.Format(CultureInfo.CurrentCulture, "versionAsDouble ({0}) cannot be parsed as product release version.", versionAsDouble));
				return new Version().ToString();
			}

			var result = new List<string>();
			var reminder = versionAsDouble;
			for (var i = 3; i >= 0; i--)
			{
				result.Add(((int)(reminder / Math.Pow(1000, i))).ToString(CultureInfo.InvariantCulture));
				reminder %= Math.Pow(1000, i);
			}
			return string.Join(".", result);
		}
	}

	public static class ElasticSearchExtensions
	{
		public static DateTime FromUnixTime(this double unixTime)
		{
			return epoch.AddMilliseconds(unixTime);
		}

		public static long ToUnixTime(this DateTime isoTime)
		{
			return Convert.ToInt64((isoTime - epoch).TotalMilliseconds);
		}

		static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	}

	#region SearchResponse

	public class SearchResponse
	{
		[JsonProperty("responses")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public SearchResult[] Result { get; set; }
	}

	public class SearchResult
	{
		[JsonProperty("took")]
		public long Took { get; set; }

		[JsonProperty("timed_out")]
		public long Timedout { get; set; }

		[JsonProperty("status")]
		public long Status { get; set; }

		[JsonProperty("hits")]
		public SearchResultHits Hits { get; set; }

		[JsonProperty("aggregations")]
		public SearchResultAggregations Aggregations { get; set; }
	}

	public class SearchResultHits
	{
		[JsonProperty("total")]
		public long Total { get; set; }

		[JsonProperty("hits")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public SearchResultHit[] Hits { get; set; }
	}

	public class SearchResultHit
	{
		[JsonProperty("_source")]
		public object Source { get; set; }
	}

	public class SearchResultAggregations
	{
		[JsonProperty(nameof(topLevelAggregation))]
		public SearchResultAggregation topLevelAggregation { get; set; }
	}

	public class SearchResultAggregation : SearchResultBucketSlot
	{
		[JsonProperty("buckets")]
		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public SearchResultBucket[] Buckets { get; set; }
	}

	public class SearchResultBucketSlot
	{
		[JsonProperty("value")]
		public double? Value { get; set; }
	}

	public class SearchResultBucket
	{
		[JsonProperty(nameof(AggCPUTimeMS))]
		public SearchResultBucketSlot AggCPUTimeMS { get; set; }

		[JsonProperty("key_as_string")]
		public string KeyAsString { get; set; }

		[JsonProperty("key")]
		public string Key { get; set; }

		[JsonProperty("doc_count")]
		public long DocCount { get; set; }

		[JsonProperty(nameof(SubAggregation1))]
		public SearchResultAggregation SubAggregation1 { get; set; }

		[JsonProperty(nameof(SubAggregation2))]
		public SearchResultAggregation SubAggregation2 { get; set; }

		[JsonProperty("5")]
		public SearchResultBucketSlot Slot5 { get; set; }

		[JsonProperty("6")]
		public SearchResultBucketSlot Slot6 { get; set; }

		[JsonProperty("7")]
		public SearchResultBucketSlot Slot7 { get; set; }

		[JsonProperty("8")]
		public SearchResultBucketSlot Slot8 { get; set; }

		[JsonProperty("9")]
		public SearchResultBucketSlot Slot9 { get; set; }

		[JsonProperty("10")]
		public SearchResultBucketSlot Slot10 { get; set; }

		[JsonProperty("11")]
		public SearchResultBucketSlot Slot11 { get; set; }
	}

	#endregion

}

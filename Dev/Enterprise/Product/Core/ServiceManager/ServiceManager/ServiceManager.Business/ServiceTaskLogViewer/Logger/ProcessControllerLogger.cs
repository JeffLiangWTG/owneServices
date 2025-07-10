using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using Elasticsearch.Net;
using Enterprise.Integration;
using Nest;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Shared
{
	public class ProcessControllerLogger : IProcessControllerLogger
	{
		[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "ConnectionSettings is used in the ElasticClient constructor")]
		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		public ProcessControllerLogger(
			string databaseHost,
			string database,
			string server,
			string basicAuthUsername,
			string basicAuthPassword,
			int maximumResultsByQuery,
			string processControllerLogIndex)
		{
			try
			{
				var settings = new ConnectionSettings(new Uri(server));
				settings.BasicAuthentication(basicAuthUsername, basicAuthPassword)
					.DefaultIndex(processControllerLogIndex + "*");
				new ProcessControllerLogger(databaseHost, database, maximumResultsByQuery, settings);
			}
			catch (UriFormatException ex)
			{
				throw new WebException(ex.Message);
			}
		}

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		public ProcessControllerLogger(
			string databaseHost,
			string database,
			int maximumResultsByQuery,
			ConnectionSettings connectionSettings)
		{
			this.databaseHost = databaseHost.ToLowerInvariant();
			this.database = database.ToLowerInvariant();
			this.maximumResultsByQuery = maximumResultsByQuery;
			elasticClient = new ElasticLogClientFactory().GetElasticClient(connectionSettings);
		}

		public ProcessControllerLogger(
			string databaseHost,
			string database,
			int maximumResultsByQuery,
			IElasticClient elasticClient)
		{
			this.databaseHost = databaseHost.ToLowerInvariant();
			this.database = database.ToLowerInvariant();
			this.maximumResultsByQuery = maximumResultsByQuery;
			this.elasticClient = elasticClient;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "CA1502:AvoidExcessiveComplexity is because of elasticClient.Search.")]
		public List<string> GetFileNames(string serviceHost, string serviceTask)
		{
			var hostName = serviceHost.ToLowerInvariant();
			var taskCode = serviceTask.ToUpperInvariant();

			var result = new List<string>();

			if (string.IsNullOrWhiteSpace(hostName) || string.IsNullOrWhiteSpace(taskCode)) { return result; }

			ISearchResponse<ProcessControllerLog> response;
			try
			{
				response = elasticClient.Search<ProcessControllerLog>(search => search
					.Query(m => m.Term(ts => ts.Field("processcontroller.hostname.keyword").Value(hostName))
								&& m.Term(ts => ts.Field("servicetask.code.keyword").Value(taskCode))
								&& m.Term(ts => ts.Field("database_host.keyword").Value(databaseHost))
								&& m.Term(ts => ts.Field("database.keyword").Value(database))
					)
					.Size(0)
					.Aggregations(aggregation => aggregation
						.DateHistogram(
							"distinct_date",
							distinctDate => distinctDate
								.Field(p => p.EventTime)
								.CalendarInterval(DateInterval.Day)
								.Format("yyyyMMdd")
								.Aggregations(subAggregation => subAggregation
									.DateHistogram(
										"distinct_time",
										distinctTime => distinctTime
											.Field(p => p.EventTime)
											.FixedInterval(new Time(TimeSpan.FromMinutes(10)))
											.Format("HHmm")))
						))
					.Sort(s => s.Ascending(p => p.EventTime))
				);
				if (!response.IsValid)
				{
					throw new ElasticsearchClientException(response.DebugInformation);
				}
			}
			catch (ElasticsearchClientException ex)
			{
				throw new WebException(ex.Message);
			}

			return ParseDateAggregations(response, taskCode);
		}

		internal List<string> ParseDateAggregations(ISearchResponse<ProcessControllerLog> response, string serviceTask)
		{
			var result = new List<string>();

			IAggregate indexAggregate;

			if (!response.Aggregations.TryGetValue("distinct_date", out indexAggregate)) { return result; }

			var indexBucketAggregate = indexAggregate as BucketAggregate;

			if (indexBucketAggregate == null) { return result; }

			foreach (var indexBucket in indexBucketAggregate.Items)
			{
				var dateHistogramBucket = indexBucket as DateHistogramBucket;

				if (dateHistogramBucket == null) { continue; }

				var utcDateLog = dateHistogramBucket.KeyAsString;

				if (dateHistogramBucket.DocCount == 0) { continue; }

				if (dateHistogramBucket.DocCount / maximumResultsByQuery <= 1)
				{
					result.Add(serviceTask + "_" + utcDateLog);
					continue;
				}

				IAggregate timeAggregate;

				if (!((AggregateDictionary)indexBucket).TryGetValue("distinct_time", out timeAggregate)) { continue; }

				var timeBucketAggregate = timeAggregate as BucketAggregate;

				if (timeBucketAggregate == null || timeBucketAggregate.Items == null || !timeBucketAggregate.Items.Any()) { continue; }

				var timeBucketResult = ParseTimeAggregations(utcDateLog, timeBucketAggregate, serviceTask);

				result.AddRange(timeBucketResult);
			}

			return result;
		}

		internal List<string> ParseTimeAggregations(string utcDateLog, BucketAggregate timeBucketAggregate, string serviceTask)
		{
			var startUtcTime = ((DateHistogramBucket)timeBucketAggregate.Items.ElementAt(0)).KeyAsString;
			var endUtcTime = string.Empty;
			long totalHits = 0;
			long nextMaximumResultsHits = maximumResultsByQuery;

			var result = new List<string>();
			for (var i = 0; i < timeBucketAggregate.Items.Count; i++)
			{
				var currentTimeBucket = timeBucketAggregate.Items.ElementAt(i) as DateHistogramBucket;
				DateHistogramBucket nextTimeBucket = null;

				if (timeBucketAggregate.Items.Count > i + 1)
				{
					nextTimeBucket = timeBucketAggregate.Items.ElementAt(i + 1) as DateHistogramBucket;
				}

				if (currentTimeBucket != null && nextTimeBucket != null && currentTimeBucket.DocCount != null)
				{
					totalHits += Convert.ToInt64(currentTimeBucket.DocCount);

					if (totalHits + nextTimeBucket.DocCount >= nextMaximumResultsHits)
					{
						endUtcTime = nextTimeBucket.KeyAsString;
						result.Add(serviceTask + "_" + utcDateLog + "_" + startUtcTime + "_" + endUtcTime);
						nextMaximumResultsHits = totalHits + maximumResultsByQuery;
					}
					else
					{
						continue;
					}
				}
				else
				{
					result.Add(serviceTask + "_" + utcDateLog + "_" + startUtcTime + "_0000"); // last file
				}

				startUtcTime = endUtcTime;
			}

			return result;
		}

		(DateTime startDateTimeQuery, DateTime endDateTimeQuery) ParseDateTimeFromFile(string fileName)
		{
			var fileParam = fileName.Split('_');

			DateTime startDateTimeQuery;
			DateTime endDateTimeQuery;

			try
			{
				var startDate = fileParam[1];
				var startTime = fileParam.Length > 2 ? fileParam[2] : "0000";
				startDateTimeQuery = DateTime.ParseExact(startDate + " " + startTime, "yyyyMMdd HHmm", CultureInfo.InvariantCulture);

				var endTime = fileParam.Length > 3 ? fileParam[3] : "0000";
				if (endTime == "0000")
				{
					var tempStartDateTimeQuery = DateTime.ParseExact(startDate + " " + endTime, "yyyyMMdd HHmm", CultureInfo.InvariantCulture);
					endDateTimeQuery = tempStartDateTimeQuery.AddDays(1);
				}
				else
				{
					endDateTimeQuery = DateTime.ParseExact(startDate + " " + endTime, "yyyyMMdd HHmm", CultureInfo.InvariantCulture);
				}
			}
			catch (Exception)
			{
				throw new FormatException("Wrong FileName format: " + fileName);
			}

			return (startDateTimeQuery, endDateTimeQuery);
		}

		public byte[] GetStream(string fileName, string serviceHost, string serviceTask)
		{
			var (startDateTimeQuery, endDateTimeQuery) = ParseDateTimeFromFile(fileName);
			return GetStream(
				new ServiceTaskLogFilters
				{
					FromDateTimeUtc = startDateTimeQuery,
					ToDateTimeUtc = endDateTimeQuery,
					HostName = serviceHost,
					ServiceTaskCode = serviceTask
				},
				true);
		}

		public byte[] GetStream(ServiceTaskLogFilters filters, bool isFetchAll)
		{
			var response = elasticClient.Search<ProcessControllerLog>(search => search
				.Query(m =>
					m.Term(ts => ts.Field("servicetask.code.keyword").Value(filters.ServiceTaskCode)) &&
					m.Term(ts => ts.Field("database_host.keyword").Value(databaseHost)) &&
					m.Term(ts => ts.Field("database.keyword").Value(database)) &&
					m.Term(ts => ts.Field("processcontroller.hostname.keyword").Value(filters.HostName)) && (
					m.Term(ts => ts.Field("severity.keyword").Value(SeverityIncludedInFilter(LogType.Error))) ||
					m.Term(ts => ts.Field("severity.keyword").Value(SeverityIncludedInFilter(LogType.Warning))) ||
					m.Term(ts => ts.Field("severity.keyword").Value(SeverityIncludedInFilter(LogType.Information))) ||
					m.Term(ts => ts.Field("severity.keyword").Value(SeverityIncludedInFilter(LogType.Debug)))) &&
					m.Term(ts => ts.Field(p => p.Process.Pid).Value(filters.ProcessId)) &&
					m.DateRange(r => r.Field(p => p.EventTime).GreaterThanOrEquals(filters.FromDateTimeUtc ?? DateTime.MinValue).LessThan(filters.ToDateTimeUtc ?? DateTime.MaxValue))
				)
				.Size(10000)
				.Sort(s =>
					s.Field(f => f.Field(p => p.EventTime).Order(SortOrder.Descending).MissingFirst())
					.Field(f => f.Field(p => p.SequenceId.SequenceId).Order(SortOrder.Ascending))
					.Field(f => f.Field(p => p.Process.Pid).Order(SortOrder.Ascending)))
				.Scroll("5m")
			);

			return CreateStreamFromElasticsearchResponse(response, isFetchAll);

			string SeverityIncludedInFilter(LogType severityFilter)
			{
				return filters.Severity.HasValue && severityFilter <= filters.Severity
					? severityFilter.ToString()
					: string.Empty;
			}
		}

		byte[] CreateStreamFromElasticsearchResponse(ISearchResponse<ProcessControllerLog> response, bool isFetchAll = false)
		{
			using var memoryStream = new MemoryStream();
			using var streamWriter = new StreamWriter(memoryStream);
			try
			{
				if (!response.IsValid)
				{
					throw new ElasticsearchClientException(response.DebugInformation);
				}

				var maxHits = Math.Min(response.Total, isFetchAll ? response.Total : maximumResultsByQuery);
				var totalHitsCount = 0;
				do
				{
					foreach (var hit in response.Hits)
					{
						if (totalHitsCount > maxHits)
						{
							break;
						}

						var processControllerLog = hit.Source;

						var dateTimeStamp = processControllerLog.EventTime.ToUniversalTime();

						var formattedLogLine = Logger.GetFormattedLogLine(processControllerLog.SequenceId.SequenceId, dateTimeStamp, processControllerLog.Severity, processControllerLog.ProcessController.Hostname, processControllerLog.Process.Pid, processControllerLog.Message, null);

						streamWriter.WriteLine(formattedLogLine);
						streamWriter.Flush();
						totalHitsCount++;
					}

					if (totalHitsCount >= maxHits)
					{
						break;
					}

					response = elasticClient.Scroll<ProcessControllerLog>("5m", response.ScrollId);
					if (!response.IsValid)
					{
						throw new ElasticsearchClientException(response.DebugInformation);
					}
				} while (response.Hits.Count > 0);
			}
			catch (ElasticsearchClientException ex)
			{
				throw new WebException(ex.Message);
			}

			return memoryStream.ToArray();
		}

		internal const string EventTimeField = "eventTime";
		readonly string databaseHost;
		readonly string database;
		readonly int maximumResultsByQuery;
		readonly IElasticClient elasticClient;

		[ElasticsearchType(RelationName = "processControllerLog")]
		public class ProcessControllerLog
		{
			[Text(Name = "database")]
			public string Database { get; set; }

			[Text(Name = "database_host")]
			public string DatabaseHost { get; set; }

			[Text(Name = "message")]
			public string Message { get; set; }

			[Object(Name = "processcontroller")]
			public ProcessControllerClass ProcessController { get; set; }

			[Object(Name = "servicetask")]
			public ServiceTaskClass ServiceTask { get; set; }

			[Object(Name = "process")]
			public ProcessClass Process { get; set; }

			[Text(Name = "severity")]
			public string Severity { get; set; }

			[Date(Name = "@timestamp")]
			public DateTime Timestamp { get; set; }

			[Date(Name = EventTimeField)]
			public DateTime EventTime { get; set; }

			[Object(Name = "sequenceId")]
			public SequenceIdClass SequenceId { get; set; }

			public class ProcessClass
			{
				[Number(NumberType.Integer, Name = "pid")]
				public int Pid { get; set; }
			}

			public class ProcessControllerClass
			{
				[Text(Name = "hostname")]
				public string Hostname { get; set; }
			}

			public class ServiceTaskClass
			{
				[Text(Name = "code")]
				public string Code { get; set; }
			}

			public class SequenceIdClass
			{
				[Number(NumberType.Integer, Name = "sequenceId")]
				public int SequenceId { get; set; }
			}
		}
	}
}

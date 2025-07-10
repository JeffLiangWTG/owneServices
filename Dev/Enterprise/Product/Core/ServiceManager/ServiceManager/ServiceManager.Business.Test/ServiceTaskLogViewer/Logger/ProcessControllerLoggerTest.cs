using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Elasticsearch.Net;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Moq;
using Nest;
using NUnit.Framework;
using static Enterprise.ServiceManager.Shared.ProcessControllerLogger;

namespace Enterprise.ServiceManager.Shared.Testing
{
	class ProcessControllerLoggerTest : TransactionedTestCase
	{
		public void TestGetFileNamesReturnEmptyFileNames()
		{
			using var dummyProcessControllerLogger = new DummyProcessControllerLogger();
			var files = dummyProcessControllerLogger.GetClient().GetFileNames(ServiceManagerHelper.GetHostName(), "TASK");
			AssertEquals(0, files.Count);
		}

		public class TestGetStream : TransactionedTestCase
		{
			public void TestGetStreamWithFileNameReturnEmptyStream()
			{
				using var dummyProcessControllerLogger = new DummyProcessControllerLogger();
				var buffer = dummyProcessControllerLogger.GetClient().GetStream("TASK_20070720", ServiceManagerHelper.GetHostName(), "TASK");
				AssertEquals(0, buffer.Length);
			}

			public void TestGetStreamWithFileNameContainsEventTime()
			{
				// Arrange
				var client = new Mock<IElasticClient>();
				var response = new Mock<ISearchResponse<ProcessControllerLog>>();
				var emptyResponse = new Mock<ISearchResponse<ProcessControllerLog>>();
				var hit = new Mock<IHit<ProcessControllerLog>>();
				var expectedDateTime = $"{new DateTime(2022, 2, 2, 13, 0, 0, DateTimeKind.Utc):yyyy-MM-dd HH:mm:ss.fff}";
				var log = new ProcessControllerLog();
				log.EventTime = new DateTime(2022, 2, 2, 13, 0, 0, DateTimeKind.Utc);
				log.Process = new ProcessControllerLog.ProcessClass() { Pid = 1 };
				log.Message = "test";
				log.SequenceId = new ProcessControllerLog.SequenceIdClass() { SequenceId = 1 };
				log.ProcessController = new ProcessControllerLog.ProcessControllerClass() { Hostname = "wtg.com" };

				hit.Setup(x => x.Source).Returns(log);

				response.Setup(x => x.IsValid).Returns(true);
				response
					.Setup(x => x.Hits)
					.Returns(Array.AsReadOnly(new IHit<ProcessControllerLog>[] { hit.Object }));

				emptyResponse.Setup(x => x.IsValid).Returns(true);
				emptyResponse
					.Setup(x => x.Hits)
					.Returns(Array.Empty<IHit<ProcessControllerLog>>());

				client
					.Setup(x => x.Search(It.IsAny<Func<SearchDescriptor<ProcessControllerLog>, ISearchRequest>>()))
					.Returns(response.Object);
				client
					.Setup(x => x.Scroll<ProcessControllerLog>("5m", It.IsAny<string>(), null))
					.Returns(emptyResponse.Object);

				// Act
				var processControllerLogger = new ProcessControllerLogger(string.Empty, string.Empty, 300000, client.Object);

				var buffer = processControllerLogger.GetStream("TASK_20220927", ServiceManagerHelper.GetHostName(), "TASK");
				var plainText = System.Text.Encoding.Default.GetString(buffer);

				// Assert
				AssertEquals(true, plainText.Contains(expectedDateTime));
			}

			public void TestGetStreamWithFileNameContainsTabAndNewLine()
			{
				// Arrange
				var client = new Mock<IElasticClient>();
				var response = new Mock<ISearchResponse<ProcessControllerLog>>();
				var emptyResponse = new Mock<ISearchResponse<ProcessControllerLog>>();
				var hit = new Mock<IHit<ProcessControllerLog>>();
				var expectedDateTime = $"{new DateTime(2022, 2, 2, 13, 0, 0, DateTimeKind.Utc):yyyy-MM-dd HH:mm:ss.fff}";
				var log = new ProcessControllerLog();
				log.EventTime = new DateTime(2022, 2, 2, 13, 0, 0, DateTimeKind.Utc);
				log.Process = new ProcessControllerLog.ProcessClass() { Pid = 1 };
				log.Message = "->↵test↵message->↵";
				log.SequenceId = new ProcessControllerLog.SequenceIdClass() { SequenceId = 1 };
				log.ProcessController = new ProcessControllerLog.ProcessControllerClass() { Hostname = "wtg.com" };

				hit.Setup(x => x.Source).Returns(log);

				response.Setup(x => x.IsValid).Returns(true);
				response
					.Setup(x => x.Hits)
					.Returns(Array.AsReadOnly(new IHit<ProcessControllerLog>[] { hit.Object }));

				emptyResponse.Setup(x => x.IsValid).Returns(true);
				emptyResponse
					.Setup(x => x.Hits)
					.Returns(Array.Empty<IHit<ProcessControllerLog>>());

				client
					.Setup(x => x.Search(It.IsAny<Func<SearchDescriptor<ProcessControllerLog>, ISearchRequest>>()))
					.Returns(response.Object);
				client
					.Setup(x => x.Scroll<ProcessControllerLog>("5m", It.IsAny<string>(), null))
					.Returns(emptyResponse.Object);

				// Act
				var processControllerLogger = new ProcessControllerLogger(string.Empty, string.Empty, 300000, client.Object);

				var buffer = processControllerLogger.GetStream("TASK_20220927", ServiceManagerHelper.GetHostName(), "TASK");
				var plainText = System.Text.Encoding.UTF8.GetString(buffer);

				// Assert
				AssertEquals(true, plainText.Contains("->↵test↵message->↵"));
			}

			public void TestGetStreamWithWrongFileNameFormat()
			{
				using var dummyProcessControllerLogger = new DummyProcessControllerLogger();
				AssertExceptionThrown<FormatException>(() => dummyProcessControllerLogger.GetClient().GetStream("TASK_", ServiceManagerHelper.GetHostName(), "TASK"));
			}

			public void TestGetStreamWithInvalidResponse()
			{
				// Arrange
				var client = new Mock<IElasticClient>();
				var response = new Mock<ISearchResponse<ProcessControllerLog>>();
				response.Setup(x => x.IsValid).Returns(false);
				client
					.Setup(x => x.Search(It.IsAny<Func<SearchDescriptor<ProcessControllerLog>, ISearchRequest>>()))
					.Returns(response.Object);
				var processControllerLogger = new ProcessControllerLogger(string.Empty, string.Empty, 300000, client.Object);

				try
				{
					// Act
					_ = processControllerLogger.GetStream("TASK_20220927", ServiceManagerHelper.GetHostName(), "TASK");
				}
				catch (Exception ex)
				{
					// Assert
					AssertEquals(typeof(WebException), ex.GetType());
				}
			}

			public void TestGetStreamWithServiceTaskLogFilters()
			{
				// Arrange
				var client = new Mock<IElasticClient>();
				var response = new Mock<ISearchResponse<ProcessControllerLog>>();
				var emptyResponse = new Mock<ISearchResponse<ProcessControllerLog>>();
				var hit = new Mock<IHit<ProcessControllerLog>>();
				var log = new ProcessControllerLog
				{
					EventTime = new DateTime(2022, 2, 2, 13, 0, 0, DateTimeKind.Utc),
					Process = new ProcessControllerLog.ProcessClass { Pid = 1 },
					Message = "->↵test↵message->↵",
					Severity = "Information",
					ProcessController = new ProcessControllerLog.ProcessControllerClass { Hostname = "wtg.com" },
					SequenceId = new ProcessControllerLog.SequenceIdClass { SequenceId = 1 },
				};

				hit.Setup(x => x.Source).Returns(log);

				response.Setup(x => x.IsValid).Returns(true);
				response
					.Setup(x => x.Hits)
					.Returns(Array.AsReadOnly(new IHit<ProcessControllerLog>[] { hit.Object }));

				emptyResponse.Setup(x => x.IsValid).Returns(true);
				emptyResponse
					.Setup(x => x.Hits)
					.Returns(Array.Empty<IHit<ProcessControllerLog>>());

				client
					.Setup(x => x.Search(It.IsAny<Func<SearchDescriptor<ProcessControllerLog>, ISearchRequest>>()))
					.Returns(response.Object);
				client
					.Setup(x => x.Scroll<ProcessControllerLog>("5m", It.IsAny<string>(), null))
					.Returns(emptyResponse.Object);

				var filters = new ServiceTaskLogFilters
				{
					ServiceTaskCode = "TASK",
					ProcessId = "1",
					HostName = ServiceManagerHelper.GetHostName(),
					FromDateTimeUtc = new DateTime(2022, 2, 2, 13, 0, 0, DateTimeKind.Utc),
					ToDateTimeUtc = new DateTime(2022, 2, 2, 13, 0, 0, DateTimeKind.Utc),
					Severity = LogType.Information
				};

				var processControllerLogger = new ProcessControllerLogger(string.Empty, string.Empty, 300000, client.Object);

				// Act
				var buffer = processControllerLogger.GetStream(filters, false);

				var plainText = System.Text.Encoding.UTF8.GetString(buffer);

				// Assert
				AssertEquals(true, plainText.Contains("test"));
			}

			public void TestGetStreamWithFetchLimit()
			{
				// Arrange
				var maximumResults = 2500;
				var resultsPerQuery = 1000;
				var totalResults = 3000;

				var client = new Mock<IElasticClient>();
				var response = new Mock<ISearchResponse<ProcessControllerLog>>();
				var hit = new Mock<IHit<ProcessControllerLog>>();
				var log = new ProcessControllerLog
				{
					EventTime = new DateTime(2022, 2, 2, 13, 0, 0, DateTimeKind.Utc),
					Process = new ProcessControllerLog.ProcessClass { Pid = 1 },
					Message = "->↵test↵message->↵",
					Severity = "Information",
					ProcessController = new ProcessControllerLog.ProcessControllerClass { Hostname = "wtg.com" },
					SequenceId = new ProcessControllerLog.SequenceIdClass { SequenceId = 1 },
				};

				hit.Setup(x => x.Source).Returns(log);

				response.Setup(x => x.IsValid).Returns(true);
				response.Setup(x => x.Total).Returns(totalResults);
				response
					.Setup(x => x.Hits)
					.Returns(Enumerable.Repeat(hit.Object, resultsPerQuery).ToList().AsReadOnly());

				client
					.Setup(x => x.Search(It.IsAny<Func<SearchDescriptor<ProcessControllerLog>, ISearchRequest>>()))
					.Returns(response.Object);
				client
					.Setup(x => x.Scroll<ProcessControllerLog>("5m", It.IsAny<string>(), null))
					.Returns(response.Object);

				var filters = new ServiceTaskLogFilters
				{
					ServiceTaskCode = "TASK"
				};

				var processControllerLogger = new ProcessControllerLogger(string.Empty, string.Empty, maximumResults, client.Object);

				// Act
				var buffer = processControllerLogger.GetStream(filters, false);
				var plainText = System.Text.Encoding.UTF8.GetString(buffer);

				// Assert
				client.Verify(x => x.Scroll<ProcessControllerLog>("5m", It.IsAny<string>(), null), Times.Exactly(2));
				var lineCount = plainText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
				AssertEquals(maximumResults + 1, lineCount); // WriteLine adds an extra line
			}

			public void TestGetStreamFiltersCorrectSeverityWithError() => TestGetStreamFiltersCorrectSeverity(LogType.Error, new[] { "Error" });
			public void TestGetStreamFiltersCorrectSeverityWithWarning() => TestGetStreamFiltersCorrectSeverity(LogType.Warning, new[] { "Error", "Warning" });
			public void TestGetStreamFiltersCorrectSeverityWithInformation() => TestGetStreamFiltersCorrectSeverity(LogType.Information, new[] { "Error", "Warning", "Information" });
			public void TestGetStreamFiltersCorrectSeverityWithDebug() => TestGetStreamFiltersCorrectSeverity(LogType.Debug, new[] { "Error", "Warning", "Information", "Debug" });

			void TestGetStreamFiltersCorrectSeverity(LogType selectedSeverity, string[] includedSeverities)
			{
				// Arrange
				var allSeverities = new string[]
				{
					nameof(LogType.Error),
					nameof(LogType.Warning),
					nameof(LogType.Information),
					nameof(LogType.Debug)
				};
				var excludedSeverities = allSeverities.Where(x => !includedSeverities.Contains(x));

				var client = new Mock<IElasticClient>();
				var response = new Mock<ISearchResponse<ProcessControllerLog>>();

				var hits = new List<IHit<ProcessControllerLog>>();
				foreach (var severity in includedSeverities)
				{
					var log = new ProcessControllerLog
					{
						EventTime = new DateTime(2022, 2, 2, 13, 0, 0, DateTimeKind.Utc).AddMinutes(includedSeverities.ToList().IndexOf(severity)),
						Process = new ProcessControllerLog.ProcessClass { Pid = includedSeverities.ToList().IndexOf(severity) + 1 },
						Message = $"->↵test{severity}↵message->↵",
						Severity = severity,
						ProcessController = new ProcessControllerLog.ProcessControllerClass { Hostname = "wtg.com" },
						SequenceId = new ProcessControllerLog.SequenceIdClass { SequenceId = includedSeverities.ToList().IndexOf(severity) + 1 },
					};
					var hit = new Mock<IHit<ProcessControllerLog>>();
					hit.Setup(x => x.Source).Returns(log);
					hits.Add(hit.Object);
				}

				response.Setup(x => x.IsValid).Returns(true);
				response.Setup(x => x.Hits).Returns(Array.AsReadOnly(hits.ToArray()));
				response.Setup(x => x.Total).Returns(hits.Count);

				client
					.Setup(x => x.Search(It.IsAny<Func<SearchDescriptor<ProcessControllerLog>, ISearchRequest>>()))
					.Returns(response.Object);

				var processControllerLogger = new ProcessControllerLogger(string.Empty, string.Empty, 100, client.Object);
				var filters = new ServiceTaskLogFilters { Severity = selectedSeverity };

				// Act
				var buffer = processControllerLogger.GetStream(filters, false);

				// Assert
				var plainText = System.Text.Encoding.UTF8.GetString(buffer);

				var includedSeveritiesNotFiltered = includedSeverities.Where(severity => !plainText.Contains($"test{severity}")).ToList();
				var excludedSeveritiesFiltered = excludedSeverities.Where(severity => plainText.Contains($"test{severity}")).ToList();
				CombineAssertions(() =>
				{
					AssertEquals("Severities that should have been filtered: " + string.Join(", ", includedSeveritiesNotFiltered), 0, includedSeveritiesNotFiltered.Count);
					AssertEquals("Severities that shouldn't have been filtered: " + string.Join(", ", excludedSeveritiesFiltered), 0, excludedSeveritiesFiltered.Count);
				});
			}
		}

		public void TestProcessControllerLoggerWhenThrowExceptionsEnabled()
		{
			using var dummyProcessControllerLogger = new DummyProcessControllerLogger();
			using var settings = new ConnectionSettings(new Uri("http://doesntexist:9200"));
			settings
				.DefaultIndex("test")
				.ThrowExceptions();
			AssertExceptionThrown<WebException>(() => dummyProcessControllerLogger.GetClient(10000, settings).GetFileNames("HOST", "TASK"));
		}

		public void TestProcessControllerLoggerConnection()
		{
			EmptySystemDataRegistry();

			AssertExceptionThrown<WebException>(() => new ProcessControllerLogger(
				"localhost",
				"dbTest",
				SystemDataRegistry.Instance.ElasticsearchServiceUri.Value,
				SystemDataRegistry.Instance.ElasticsearchServerUserName.Value,
				SystemDataRegistry.Instance.ElasticsearchServerPassword.Value,
				SystemDataRegistry.Instance.ElasticsearchMaximumResultsByQuery.Value,
				SystemDataRegistry.Instance.ElasticsearchIndex.Value));

			SystemDataRegistry.Instance.ElasticsearchServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, @"https://localhost:9200/");
			SystemDataRegistry.Instance.ElasticsearchIndex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");

			AssertNoExceptionThrown(() => new ProcessControllerLogger(
				"localhost",
				"dbTest",
				SystemDataRegistry.Instance.ElasticsearchServiceUri.Value,
				SystemDataRegistry.Instance.ElasticsearchServerUserName.Value,
				SystemDataRegistry.Instance.ElasticsearchServerPassword.Value,
				SystemDataRegistry.Instance.ElasticsearchMaximumResultsByQuery.Value,
				SystemDataRegistry.Instance.ElasticsearchIndex.Value));
		}

		public void TestFileNamesParseIndexAggregationsWithoutTimeAggregations()
		{
			var buckets = new List<IBucket>
			{
				CreateDateHistogramBucket("20160913", 1000),
				CreateDateHistogramBucket("20160914", 10000),
				CreateDateHistogramBucket("20161014", 10000),
			};

			var bucketAggregate = new BucketAggregate
			{
				Items = buckets,
			};

			var response = new Mock<ISearchResponse<ProcessControllerLog>>();
			var backingDictionary = new Dictionary<string, IAggregate>
			{
				{ "distinct_date", bucketAggregate },
			};
			response
				.Setup(searchResponse => searchResponse.Aggregations)
				.Returns(new AggregateDictionary(backingDictionary));

			const string taskName = "TASK";
			using var dummyProcessControllerLogger = new DummyProcessControllerLogger();
			var processControllerLogger = dummyProcessControllerLogger.GetClient();

			var fileName = processControllerLogger.ParseDateAggregations(response.Object, taskName);

			AssertEquals(fileName.Count, 3);
			AssertEquals(fileName[0], taskName + "_20160913");
			AssertEquals(fileName[1], taskName + "_20160914");
			AssertEquals(fileName[2], taskName + "_20161014");
		}

		public void TestFileNamesParseIndexAggregationsWithTimeAggregations()
		{
			var buckets20160913 = new List<IBucket>
			{
				CreateDateHistogramBucket("0000", 10000),
				CreateDateHistogramBucket("0030", 40000),
				CreateDateHistogramBucket("0100", 50000),
				CreateDateHistogramBucket("1000", 300000),
				CreateDateHistogramBucket("1140", 200000),
				CreateDateHistogramBucket("1500", 100000),
				CreateDateHistogramBucket("1510", 150000),
				CreateDateHistogramBucket("1900", 150000),
			};

			var timeBucketAggregate = new BucketAggregate
			{
				Items = buckets20160913,
			};

			const string taskName = "TASK";
			using var dummyProcessControllerLogger = new DummyProcessControllerLogger();
			var processControllerLogger = dummyProcessControllerLogger.GetClient(300000);

			var fileName = processControllerLogger.ParseTimeAggregations("20160913", timeBucketAggregate, taskName);

			AssertEquals(fileName.Count, 5);
			AssertEquals(fileName[0], taskName + "_20160913_0000_1000");
			AssertEquals(fileName[1], taskName + "_20160913_1000_1140");
			AssertEquals(fileName[2], taskName + "_20160913_1140_1500");
			AssertEquals(fileName[3], taskName + "_20160913_1500_1900");
			AssertEquals(fileName[4], taskName + "_20160913_1900_0000");
		}

		static DateHistogramBucket CreateDateHistogramBucket(string key, long docCount)
		{
			var dateHistogramBucket = new DateHistogramBucket(new Dictionary<string, IAggregate>()) { DocCount = docCount, KeyAsString = key };
			return dateHistogramBucket;
		}

		static void EmptySystemDataRegistry()
		{
			SystemDataRegistry.Instance.ElasticsearchServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			SystemDataRegistry.Instance.ElasticsearchServerUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			SystemDataRegistry.Instance.ElasticsearchServerPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			SystemDataRegistry.Instance.ElasticsearchMaximumResultsByQuery.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10000);
			SystemDataRegistry.Instance.ElasticsearchIndex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
		}

		class DummyProcessControllerLogger : IDisposable
		{
			public ProcessControllerLogger GetClient(int logstashMaximumResultsByQuery = 10000)
			{
				SystemDataRegistry.Instance.ElasticsearchServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost:9200/");
				SystemDataRegistry.Instance.ElasticsearchServerUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
				SystemDataRegistry.Instance.ElasticsearchServerPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
				SystemDataRegistry.Instance.ElasticsearchMaximumResultsByQuery.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, logstashMaximumResultsByQuery);
				SystemDataRegistry.Instance.ElasticsearchIndex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test-index-");

				var pool = new SingleNodeConnectionPool(new Uri("https://localhost:9200"));
				var inMemoryConnection = new InMemoryConnection();
				var connectionSettings = new ConnectionSettings(pool, inMemoryConnection);
				connectionSettings.DefaultIndex(SystemDataRegistry.Instance.ElasticsearchIndex.Value);

				disposables.Add(pool);
				disposables.Add(inMemoryConnection);
				disposables.Add(connectionSettings);

				return GetClient(
					SystemDataRegistry.Instance.ElasticsearchMaximumResultsByQuery.Value,
					connectionSettings);
			}

			public ProcessControllerLogger GetClient(
				int maximumResultsByQuery,
				ConnectionSettings connectionSettings)
			{
				return new ProcessControllerLogger(string.Empty, string.Empty, maximumResultsByQuery, connectionSettings);
			}

			public void Dispose()
			{
				foreach (var disposable in disposables)
				{
					disposable.Dispose();
				}
			}

			readonly List<IDisposable> disposables = new List<IDisposable>();
		}
	}
}

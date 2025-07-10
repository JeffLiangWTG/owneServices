using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceHostClient.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(SearchBasedLogViewer))]
	public class SearchBasedLogViewerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SearchBasedLogViewer(logViewerDataProviderFactory.Object);
		}

		public void TestGetEventList()
		{
			// Arrange
			var testFile =
				"885332                   2007-07-20 16:54:53      Information              10008                    hostname[eye.wtg.zone]   LogWalker Cycle Started\r\n" +
				"884321                   2007-07-20 18:24:50      Information              10008                    hostname[eye.wtg.zone]   LogWalker Cycle Started\r\n";

			logViewerProvider
				.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>()))
				.Returns(Encoding.UTF8.GetBytes(testFile));

			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);
			var eventList = logViewer.EventList;

			var fromDateTime = new DateTime(2007, 7, 20, 16, 54, 53);
			var toDateTime = new DateTime(2007, 7, 20, 18, 24, 50);
			logViewer.ServiceTaskCode = "PrC"; // case insensitive
			logViewer.HostName = "eye.wtg.zone ";
			logViewer.ProcessId = "10008";
			logViewer.FromDateTimeUtc = fromDateTime;
			logViewer.ToDateTimeUtc = toDateTime;
			logViewer.Severity = "Information";

			// Act
			logViewer.ReloadEvents();
			_ = logViewer.EventList;

			// Assert
			logViewerProvider.Verify(
				provider => provider.GetBytes(
					It.Is<ServiceTaskLogFilters>(filter =>
						filter.ServiceTaskCode == "PRC" && // assert upper case
						filter.HostName == "eye.wtg.zone" && // assert trimmed
						filter.ProcessId == "10008" &&
						filter.Severity == LogType.Information &&
						filter.FromDateTimeUtc == fromDateTime &&
						filter.ToDateTimeUtc == toDateTime)), Times.Once);
			AssertEquals(2, eventList.Count);
			AssertEquals(885332, eventList[0].ElasticSequenceId);
			AssertEquals(new ZDateTime(2007, 7, 20, 16, 54, 53, 0), eventList[0].DateTime);
			AssertEquals("Information", eventList[0].Type);
			AssertEquals(10008, eventList[0].ProcessId);
			AssertEquals("eye.wtg.zone", eventList[0].HostName);
			AssertEquals("LogWalker Cycle Started", eventList[0].Message);
		}

		public void TestGetEventListWithSortingInformation()
		{
			// Arrange
			var testFile =
				"5                        2007-07-20 16:54:53      Information              10008                    hostname[eye.wtg.zone]   LogWalker Cycle Started\r\n" +
				"1                        2007-07-20 18:24:50      Information              10008                    hostname[eye.wtg.zone]   LogWalker Cycle Started\r\n" +
				"4                        2007-07-20 18:24:50      Information              10008                    hostname[eye.wtg.zone]   LogWalker Cycle Started\r\n" +
				"2                        2007-07-20 18:24:50      Information              10008                    hostname[eye.wtg.zone]   LogWalker Cycle Started\r\n";

			logViewerProvider
				.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>()))
				.Returns(Encoding.UTF8.GetBytes(testFile));

			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);

			var fromDateTime = new DateTime(2007, 7, 20, 16, 54, 53);
			var toDateTime = new DateTime(2007, 7, 20, 18, 24, 50);
			logViewer.ServiceTaskCode = "PrC"; // case insensitive
			logViewer.HostName = "eye.wtg.zone ";
			logViewer.ProcessId = "10008";
			logViewer.FromDateTimeUtc = fromDateTime;
			logViewer.ToDateTimeUtc = toDateTime;
			logViewer.Severity = "Information";

			// Act
			var eventList = logViewer.EventList; // just initialise with the sort information here
			eventList.Sort("ElasticSequenceId", ListSortDirection.Ascending);

			logViewer.ReloadEvents();
			_ = logViewer.EventList;

			// Assert
			AssertEquals(4, eventList.Count);
			AssertEquals(1, eventList[0].ElasticSequenceId);
			AssertEquals(2, eventList[1].ElasticSequenceId);
			AssertEquals(4, eventList[2].ElasticSequenceId);
			AssertEquals(5, eventList[3].ElasticSequenceId);
		}

		public void TestGetEventListWhenFiltersAreNullOrEmpty()
		{
			// Arrange
			var testFile =
				"885332                   2007-07-20 16:54:53      Information              10008                    hostname[eye.wtg.zone]   LogWalker Cycle Started\r\n" +
				"884321                   2007-07-20 18:24:50      Information              10008                    hostname[eye.wtg.zone]   LogWalker Cycle Started\r\n";

			logViewerProvider
				.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>()))
				.Returns(Encoding.UTF8.GetBytes(testFile));

			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);
			logViewer.ServiceTaskCode = "PRC";

			// Act
			logViewer.ReloadEvents();
			var eventList = logViewer.EventList;

			// Assert
			logViewerProvider.Verify(
				provider => provider.GetBytes(
					It.Is<ServiceTaskLogFilters>(filter =>
						filter.ServiceTaskCode == "PRC" &&
						filter.HostName == null &&
						filter.ProcessId == null &&
						filter.Severity == null &&
						filter.FromDateTimeUtc == null &&
						filter.ToDateTimeUtc == null)), Times.Never);
			AssertEquals(2, eventList.Count);
		}

		[ExpectNoExceptions]
		public void TestGetEventListShouldCache()
		{
			// Arrange
			logViewerProvider.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>())).Returns(Array.Empty<byte>());

			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);
			logViewer.ServiceTaskCode = "PRC";

			// Act
			logViewer.ReloadEvents();
			_ = logViewer.EventList;
			_ = logViewer.EventList;
			_ = logViewer.EventList;

			// Assert
			logViewerProvider.Verify(provider => provider.GetBytes(It.IsAny<ServiceTaskLogFilters>()), Times.Exactly(1));
		}

		[ExpectNoExceptions]
		public void TestGetEventListShouldHandleWebExceptionError()
		{
			logViewerProvider.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>()))
				.Throws(new System.Net.WebException());

			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);
			logViewer.ServiceTaskCode = "PRC";

			// Act
			logViewer.ReloadEvents();
			var eventList = logViewer.EventList;

			// Assert
			AssertEquals(1, eventList.Count);
			AssertEquals("Error", eventList[0].Type);
			AssertEquals("Error", eventList[0].Type);
			AssertContains("LogViewer Exception", eventList[0].Message);
		}

		[ExpectNoExceptions]
		public void TestReloadEvents()
		{
			// Arrange
			logViewerProvider.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>())).Returns(Array.Empty<byte>());

			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);
			logViewer.ServiceTaskCode = "PRC";

			// Act
			logViewer.ReloadEvents();
			_ = logViewer.EventList;
			logViewer.ReloadEvents();
			_ = logViewer.EventList;

			// Assert
			logViewerProvider.Verify(provider => provider.GetBytes(It.IsAny<ServiceTaskLogFilters>()), Times.Exactly(2));
		}

		public void TestServiceTaskCodes()
		{
			//Arrange
			var codes = new[] { 1, 5, 8, 2, 5 };
			codes.ForEach(code =>
			{
				var task = Factory.New<ServiceTaskSchedule>();
				task.S5_ScheduleType = $"C0{code}";
				task.S5_ScheduleDescription = $"Desc {code}";
			});

			var hostCache = new Mock<IServiceHostsCache>();
			hostCache.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Substitute(hostCache.Object))
			{
				Factory.Save();

				//Act
				var viewer = new SearchBasedLogViewer(new SearchBasedLogViewerDataProviderFactory());

				//Assert
				AssertEquals(5, viewer.ServiceTaskCodes.Count);
				AssertContainsExactElementsInExactOrder(new[] { "HOST", "C01", "C02", "C05", "C08" }, viewer.ServiceTaskCodes.ToArray().Select(pair => pair.Code));
				AssertContainsExactElementsInExactOrder(new[] { "Service Host", "Desc 1", "Desc 2", "Desc 5", "Desc 8" }, viewer.ServiceTaskCodes.ToArray().Select(pair => pair.Description));
			}
		}

		public void TestNewModuleServiceTaskCodes()
		{
			//Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var codes = new[] { "C01", "C05", "C08", "C02" };
				codes.ForEach(code =>
				{
					var task = Factory.New<StmServiceTask>();
					task.SST_ServiceTaskCode = code;
					task.SST_NextRunTime = ZDateTime.Now.AddDays(7).ToOffset();
				});

				var hostCache = new Mock<IServiceHostsCache>();
				hostCache.Setup(x => x.AvailableServiceHosts).Returns(Array.Empty<IServiceHostClient>());

				var hostedServiceAttributes = new[]
				{
					new HostedServiceAttribute("C01", "Desc 1", "category", typeof(object)),
					new HostedServiceAttribute("C05", "Desc 5", "category", typeof(object)),
					new HostedServiceAttribute("C08", "Desc 8", "category", typeof(object)),
					new HostedServiceAttribute("C02", "Desc 2", "category", typeof(object))
				};

				var provider = new Mock<IClientHostedServiceAttributeProvider>();
				provider.Setup(p => p.GetClientHostedServiceAttributes()).Returns(() => hostedServiceAttributes);
				hostedServiceAttributes.ForEach(attrib => provider.Setup(p => p.GetClientHostedServiceAttribute(attrib.Code)).Returns(attrib));

				using (ObjectFactory.Substitute(hostCache.Object))
				using (ObjectFactory.Substitute(provider.Object))
				{
					Factory.Save();

					//Act
					var viewer = new SearchBasedLogViewer(new SearchBasedLogViewerDataProviderFactory());

					//Assert
					AssertEquals(5, viewer.ServiceTaskCodes.Count);
					AssertContainsExactElementsInExactOrder(new[] { "HOST", "C01", "C02", "C05", "C08" }, viewer.ServiceTaskCodes.ToArray().Select(pair => pair.Code));
					AssertContainsExactElementsInExactOrder(new[] { "Service Host", "Desc 1", "Desc 2", "Desc 5", "Desc 8" }, viewer.ServiceTaskCodes.ToArray().Select(pair => pair.Description));
				}
			}
		}

		#region FromDateTimeUtc

		[TestDate(2025, 1, 1)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestSetFromDateTimeUtcFromLocalTime()
		{
			TestDateAttribute.UseUNLOCO = true;

			// Arrange
			var dateTime = ZDateTime.Now.ToDateTime();
			var viewer = new SearchBasedLogViewer(new SearchBasedLogViewerDataProviderFactory());

			// Act
			viewer.FromDateTimeLocal = new ZDateTime(dateTime);

			// Assert
			AssertEquals(ZDateTime.UtcNow, viewer.FromDateTimeUtc);
		}

		public void TestSetEmptyFromDateTimeUtcFromLocalTime()
		{
			// Arrange
			var viewer = new SearchBasedLogViewer(new SearchBasedLogViewerDataProviderFactory());

			// Act
			viewer.FromDateTimeLocal = ZDateTime.Now.ToDateTime();
			viewer.FromDateTimeUtc = ZDateTime.Empty;

			// Assert
			AssertEquals(ZDateTime.Empty, viewer.FromDateTimeLocal);
		}

		public void TestFromDateTimeUtcAdjustsToDateTimeUtcWhenGreater()
		{
			// Arrange
			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);
			var initialToDate = new ZDateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var newFromDate = new ZDateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);

			logViewer.ToDateTimeUtc = initialToDate;

			// Act
			logViewer.FromDateTimeUtc = newFromDate;

			// Assert
			AssertEquals(newFromDate, logViewer.ToDateTimeUtc);
		}

		#endregion

		#region ToDateTimeUtc

		[TestDate(2025, 1, 1)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestSetToDateTimeUtcFromLocalTime()
		{
			TestDateAttribute.UseUNLOCO = true;

			// Arrange
			var dateTime = ZDateTime.Now.ToDateTime();
			var viewer = new SearchBasedLogViewer(new SearchBasedLogViewerDataProviderFactory());

			// Act
			viewer.ToDateTimeLocal = new ZDateTime(dateTime);

			// Assert
			AssertEquals(ZDateTime.UtcNow, viewer.ToDateTimeUtc);
		}

		public void TestSetEmptyToDateTimeUtcFromLocalTime()
		{
			// Arrange
			var viewer = new SearchBasedLogViewer(new SearchBasedLogViewerDataProviderFactory());

			// Act
			viewer.ToDateTimeLocal = ZDateTime.Now.ToDateTime();
			viewer.ToDateTimeUtc = ZDateTime.Empty;

			// Assert
			AssertEquals(ZDateTime.Empty, viewer.ToDateTimeLocal);
		}

		public void TestToDateTimeUtcAdjustsFromDateTimeUtcWhenLess()
		{
			// Arrange
			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);
			var initialFromDate = new ZDateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
			var newToDate = new ZDateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			logViewer.FromDateTimeUtc = initialFromDate;

			// Act
			logViewer.ToDateTimeUtc = newToDate;

			// Assert
			AssertEquals(newToDate, logViewer.FromDateTimeUtc);
		}

		#endregion

		#region FromDateTimeLocal

		[TestDate(2025, 1, 1)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestSetFromDateTimeLocalFromUtcTime()
		{
			TestDateAttribute.UseUNLOCO = true;

			// Arrange
			var dateTime = ZDateTime.UtcNow;
			var viewer = new SearchBasedLogViewer(new SearchBasedLogViewerDataProviderFactory());
		
			// Act
			viewer.FromDateTimeUtc = dateTime;

			// Assert
			AssertEquals(ZDateTime.Now, viewer.FromDateTimeLocal);
		}

		public void TestSetEmptyFromDateTimeLocalFromUtcTime()
		{
			// Arrange
			var viewer = new SearchBasedLogViewer(new SearchBasedLogViewerDataProviderFactory());

			// Act
			viewer.FromDateTimeUtc = ZDateTime.UtcNow.ToDateTime();
			viewer.FromDateTimeLocal = ZDateTime.Empty;

			// Assert
			AssertEquals(ZDateTime.Empty, viewer.FromDateTimeUtc);
		}

		public void TestFromDateTimeLocalAdjustsToDateTimeLocalWhenGreater()
		{
			// Arrange
			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);
			var initialToDate = new ZDateTime(2025, 1, 1, 0, 0, 0);
			var newFromDate = new ZDateTime(2025, 1, 2, 0, 0, 0);
			var expectedDate = Env.Time.GetUtcFromLocalTime(newFromDate.UtcToDateTimeOffset().ToDateTime());

			logViewer.ToDateTimeLocal = initialToDate;

			// Act
			logViewer.FromDateTimeLocal = newFromDate;

			// Assert
			AssertEquals(expectedDate, logViewer.ToDateTimeLocal);
		}

		#endregion

		#region ToDateTimeLocal

		[TestDate(2025, 1, 1)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestSetToDateTimeLocalFromUtcTime()
		{
			TestDateAttribute.UseUNLOCO = true;

			// Arrange
			var dateTime = ZDateTime.UtcNow.ToDateTime();
			var viewer = new SearchBasedLogViewer(new SearchBasedLogViewerDataProviderFactory());

			// Act
			viewer.ToDateTimeUtc = new ZDateTime(dateTime);

			// Assert
			AssertEquals(ZDateTime.Now, viewer.ToDateTimeLocal);
		}

		[TestDate(2025, 1, 1)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestSetFromDateTimeUtcWithLocalTimeDirectly()
		{
			TestDateAttribute.UseUNLOCO = true;

			// Arrange
			var dateTime = ZDateTime.Now.ToDateTime();
			var viewer = new SearchBasedLogViewer(new SearchBasedLogViewerDataProviderFactory());
	
			// Act
			viewer.FromDateTimeUtc = dateTime;

			// Assert
			AssertEquals(ZDateTime.UtcNow, viewer.FromDateTimeUtc);
			AssertEquals(ZDateTime.Now, viewer.FromDateTimeLocal);
		}

		[TestDate(2025, 1, 1)]
		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestSetToDateTimeUtcWithLocalTimeDirectly()
		{
			TestDateAttribute.UseUNLOCO = true;

			// Arrange
			var dateTime = ZDateTime.Now.ToDateTime();
			var viewer = new SearchBasedLogViewer(new SearchBasedLogViewerDataProviderFactory());

			// Act
			viewer.ToDateTimeUtc = dateTime;

			// Assert
			AssertEquals(ZDateTime.UtcNow, viewer.ToDateTimeUtc);
			AssertEquals(ZDateTime.Now, viewer.ToDateTimeLocal);
		}

		public void TestSetEmptyToDateTimeLocalFromUtcTime()
		{
			// Arrange
			var viewer = new SearchBasedLogViewer(new SearchBasedLogViewerDataProviderFactory());

			// Act
			viewer.ToDateTimeUtc = ZDateTime.UtcNow.ToDateTime();
			viewer.ToDateTimeLocal = ZDateTime.Empty;

			// Assert
			AssertEquals(ZDateTime.Empty, viewer.ToDateTimeUtc);
		}

		public void TestToDateTimeLocalAdjustsFromDateTimeLocalWhenLess()
		{
			// Arrange
			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);
			var initialFromDate = new ZDateTime(2025, 1, 2, 0, 0, 0);
			var newToDate = new ZDateTime(2025, 1, 1, 0, 0, 0);
			var expectedDate = Env.Time.GetUtcFromLocalTime(newToDate.UtcToDateTimeOffset().ToDateTime());

			logViewer.FromDateTimeLocal = initialFromDate;

			// Act
			logViewer.ToDateTimeLocal = newToDate;

			// Assert
			AssertEquals(expectedDate, logViewer.FromDateTimeLocal);
		}

		#endregion

		[ExpectNoExceptions]
		public void TestSearchBasedLogViewerIsNotReloadedWithInvalidElasticsearchConfiguration()
		{
			// Arrange
			using (SystemDataRegistry.Instance.ElasticsearchServiceUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var searchBasedLogViewerDataProviderFactoryMock = new Mock<ISearchBasedLogViewerDataProviderFactory>();
				var viewer = new SearchBasedLogViewer(searchBasedLogViewerDataProviderFactoryMock.Object);

				// Act
				viewer.ReloadEvents();

				// Assert
				searchBasedLogViewerDataProviderFactoryMock.Verify(x => x.GetProvider(), Times.Never);
			}
		}

		[ExpectNoExceptions]
		public void TestSearchBasedLogViewerIsNotReloadedWithInvalidKafkaConfiguration()
		{
			// Arrange
			using (SystemDataRegistry.Instance.KafkaElasticsearchServiceUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				var searchBasedLogViewerDataProviderFactoryMock = new Mock<ISearchBasedLogViewerDataProviderFactory>();
				var viewer = new SearchBasedLogViewer(searchBasedLogViewerDataProviderFactoryMock.Object);

				// Act
				viewer.ReloadEvents();

				// Assert
				searchBasedLogViewerDataProviderFactoryMock.Verify(x => x.GetProvider(), Times.Never);
			}
		}

		[ExpectNoExceptions]
		public void TestSearchBasedLogViewerIsNotReloadedWithFileLoggingEnabled()
		{
			var value = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.KAF, Bool = false, Bool2 = false, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.ELK, Bool = false, Bool2 = false, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, Bool = true, Bool2 = true, SystemDefined = true, },
			};
			value.SetDefaultCode(LoggingMethods.FSL, true);
			using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
			{
				var searchBasedLogViewerDataProviderFactoryMock = new Mock<ISearchBasedLogViewerDataProviderFactory>();
				var viewer = new SearchBasedLogViewer(searchBasedLogViewerDataProviderFactoryMock.Object);

				// Act
				viewer.ReloadEvents();

				// Assert
				searchBasedLogViewerDataProviderFactoryMock.Verify(x => x.GetProvider(), Times.Never);
			}
		}

		#region HostNameList

		public void TestHostNameListIsPopulatedWithNoDuplicates()
		{
			// Arrange
			var hostName1 = "eye.wtg.zone1";
			var hostName2 = "eye.wtg.zone2";
			var testFile =
				$"""
				1                        2007-07-20 16:54:53      Information              1                    hostname[{hostName1}]   Log
				2                        2007-07-20 18:24:50      Information              1                    hostname[{hostName2}]   Log
				3                        2007-07-20 18:24:51      Information              1                    hostname[{hostName2}]   Log
				""";

			logViewerProvider
				.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>()))
				.Returns(Encoding.UTF8.GetBytes(testFile));

			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);

			// Act
			logViewer.ReloadEvents();
			_ = logViewer.EventList;
			var hostNameList = logViewer.HostNameList;

			// Assert
			AssertEquals(3, hostNameList.Count);
			AssertEquals("All", hostNameList[0].Code);
			AssertEquals(hostName1, hostNameList[1].Code);
			AssertEquals(hostName2, hostNameList[2].Code);
		}

		public void TestHostNameListIsNotResetWhenValidHostNameIsSelected()
		{
			// Arrange
			var hostName1 = "eye.wtg.zone1";
			var hostName2 = "eye.wtg.zone2";
			var testFile =
				$"""
				1                        2007-07-20 16:54:53      Information              1                    hostname[{hostName1}]   Log
				2                        2007-07-20 18:24:50      Information              1                    hostname[{hostName2}]   Log
				""";

			logViewerProvider
				.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>()))
				.Returns(Encoding.UTF8.GetBytes(testFile));

			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object)
			{
				HostName = "eye.wtg.zone1"
			};
			_ = logViewer.HostNameList;

			// Act
			logViewer.ReloadEvents();
			_ = logViewer.EventList;
			var hostNameList = logViewer.HostNameList;

			// Assert
			AssertEquals(3, hostNameList.Count);
			AssertEquals("All", hostNameList[0].Code);
			AssertEquals(hostName1, hostNameList[1].Code);
			AssertEquals(hostName2, hostNameList[2].Code);
		}

		public void TestHostNameListIsSorted()
		{
			// Arrange
			var testFile =
				"""
				1                        2007-07-20 16:54:53      Information              1                    hostname[eye.wtg.zoneA]   Log
				2                        2007-07-20 16:54:53      Information              1                    hostname[eye.wtg.zoneF]   Log
				3                        2007-07-20 18:24:50      Information              1                    hostname[eye.wtg.zoneE]   Log
				""";
			logViewerProvider
				.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>()))
				.Returns(Encoding.UTF8.GetBytes(testFile));
			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);

			// Act
			logViewer.ReloadEvents();
			_ = logViewer.EventList;
			var hostNameList = logViewer.HostNameList;

			// Assert
			AssertEquals(4, hostNameList.Count);
			AssertEquals("All", hostNameList[0].Code);
			AssertEquals("eye.wtg.zoneA", hostNameList[1].Code);
			AssertEquals("eye.wtg.zoneE", hostNameList[2].Code);
			AssertEquals("eye.wtg.zoneF", hostNameList[3].Code);
		}

		#endregion

		#region ProcessIdList

		public void TestProcessIDListIsPopulatedWithNoDuplicates()
		{
			// Arrange
			var processID1 = "10008";
			var processID2 = "10009";
			var testFile =
				$"""
				1                        2007-07-20 16:54:53      Information              {processID1}                    hostname[eye.wtg.zone]   Log
				2                        2007-07-20 18:24:50      Information              {processID2}                    hostname[eye.wtg.zone]   Log
				3                        2007-07-20 18:24:51      Information              {processID2}                    hostname[eye.wtg.zone]   Log
				""";

			logViewerProvider
				.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>()))
				.Returns(Encoding.UTF8.GetBytes(testFile));

			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);

			// Act
			logViewer.ReloadEvents();
			_ = logViewer.EventList;
			var processIDList = logViewer.ProcessIdList;

			// Assert
			AssertEquals(3, processIDList.Count);
			AssertEquals("All", processIDList[0].Code);
			AssertEquals(processID1, processIDList[1].Code);
			AssertEquals(processID2, processIDList[2].Code);
		}

		public void TestProcessIDListIsNotResetWhenValidHostNameIsSelected()
		{
			// Arrange
			var processID1 = "10008";
			var processID2 = "10009";
			var testFile =
				$"""
				1                        2007-07-20 16:54:53      Information              {processID1}                    hostname[eye.wtg.zone]   Log
				2                        2007-07-20 18:24:50      Information              {processID2}                    hostname[eye.wtg.zone]   Log
				""";

			logViewerProvider
				.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>()))
				.Returns(Encoding.UTF8.GetBytes(testFile));

			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object)
			{
				ProcessId = "10008"
			};
			_ = logViewer.ProcessIdList;

			// Act
			logViewer.ReloadEvents();
			_ = logViewer.EventList;
			var processIDList = logViewer.ProcessIdList;

			// Assert
			AssertEquals(3, processIDList.Count);
			AssertEquals("All", processIDList[0].Code);
			AssertEquals(processID1, processIDList[1].Code);
			AssertEquals(processID2, processIDList[2].Code);
		}

		public void TestProcessIDListIsSorted()
		{
			// Arrange
			var testFile =
				"""
				1                        2007-07-20 16:54:53      Information              10009                    hostname[eye.wtg.zone]   Log
				2                        2007-07-20 16:54:53      Information              10014                    hostname[eye.wtg.zone]   Log
				3                        2007-07-20 18:24:50      Information              109                      hostname[eye.wtg.zone]   Log
				""";
			logViewerProvider
				.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>()))
				.Returns(Encoding.UTF8.GetBytes(testFile));
			var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);

			// Act
			logViewer.ReloadEvents();
			_ = logViewer.EventList;
			var processIDList = logViewer.ProcessIdList;

			// Assert
			AssertEquals(4, processIDList.Count);
			AssertEquals("All", processIDList[0].Code);
			AssertEquals("109", processIDList[1].Code);
			AssertEquals("10009", processIDList[2].Code);
			AssertEquals("10014", processIDList[3].Code);
		}

		#endregion

		class TestValidation : BusinessObjectValidationTestCase
		{
			[ExpectNoExceptions]
			public void TestShouldValidateServiceTaskCodeMaxLengthExceededOnSetter()
			{
				// Arrange
				logViewerProvider.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>())).Returns(Array.Empty<byte>());

				var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);

				// Act
				try
				{
					logViewer.ServiceTaskCode = "PRCCCCCCC";
				}
				catch (Exception ex)
				{
					// Assert
					AssertEquals(typeof(MaxLengthExceededException), ex.GetType());
					AssertEquals(4, ((MaxLengthExceededException)ex).MaxLength);
					ErrorReporter.Clear();
				}
			}

			[ExpectNoExceptions]
			public void TestShouldValidateServiceTaskCodeIsEmptyOnSetter()
			{
				// Arrange
				logViewerProvider.Setup(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>()))
					.Returns(Array.Empty<byte>());

				var logViewer = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);

				// Act
				logViewer.ServiceTaskCode = "";

				// Assert
				AssertHasError(logViewer.ServiceTaskCodeInfo, "Please enter a value.");
			}

			protected override void SetUp()
			{
				base.SetUp();

				logViewerProvider = new Mock<ISearchBasedLogViewerDataProvider>();

				logViewerDataProviderFactory = new Mock<ISearchBasedLogViewerDataProviderFactory>();
				logViewerDataProviderFactory.Setup(x => x.GetProvider()).Returns(logViewerProvider.Object);
			}

			Mock<ISearchBasedLogViewerDataProvider> logViewerProvider;
			Mock<ISearchBasedLogViewerDataProviderFactory> logViewerDataProviderFactory;
		}

		protected override void SetUp()
		{
			base.SetUp();

			logViewerProvider = new Mock<ISearchBasedLogViewerDataProvider>();

			logViewerDataProviderFactory = new Mock<ISearchBasedLogViewerDataProviderFactory>();
			logViewerDataProviderFactory.Setup(x => x.GetProvider()).Returns(logViewerProvider.Object);

			SystemDataRegistry.Instance.ElasticsearchServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Example");
			SystemDataRegistry.Instance.ElasticsearchServerUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Example");
			SystemDataRegistry.Instance.ElasticsearchServerPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Example");
			SystemDataRegistry.Instance.ElasticsearchIndex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Example");
			var value = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.KAF, Bool = false, Bool2 = false, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.ELK, Bool = true, Bool2 = false, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, Bool = false, Bool2 = true, SystemDefined = true, },
			};
			value.SetDefaultCode(LoggingMethods.ELK, true);
			SystemDataRegistry.Instance.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		Mock<ISearchBasedLogViewerDataProvider> logViewerProvider;
		protected Mock<ISearchBasedLogViewerDataProviderFactory> logViewerDataProviderFactory;
	}
}

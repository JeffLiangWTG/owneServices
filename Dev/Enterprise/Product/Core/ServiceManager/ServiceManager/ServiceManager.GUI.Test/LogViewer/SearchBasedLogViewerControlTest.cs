using System;
using System.Linq;
using CargoWise.Data.Testing;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.ServiceManager.GUI.Testing
{
	[TestedType(typeof(SearchBasedLogViewerControl))]
	public class SearchBasedLogViewerControlTest : TestCase
	{
		[RequiresSTA]
		public void TestShouldNotRefreshLogsWhenValidationFailed()
		{
			// Arrange
			var logViewerDataProvider = new Mock<ISearchBasedLogViewerDataProvider>();

			var logViewerDataProviderFactory = new Mock<ISearchBasedLogViewerDataProviderFactory>();
			logViewerDataProviderFactory.Setup(x => x.GetProvider()).Returns(logViewerDataProvider.Object);

			var bindingSource = new SearchBasedLogViewer(logViewerDataProviderFactory.Object);

			using var view = new SearchBasedLogViewerControl();
			view.SetDataBinding(bindingSource, "");

			// Act
			view.RefreshLogs();

			// Assert
			AssertEquals(true, bindingSource.HasErrors);
			logViewerDataProvider.Verify(x => x.GetBytes(It.IsAny<ServiceTaskLogFilters>()), Times.Never);
			AssertEquals(0, bindingSource.EventList.Count); // Should default length with 0 because GetBytes never called
		}

		[UseSnapshotProtection]
		public void TestErrorsShownWithElasticsearchLogViewingEnabledAndInvalidElasticConfiguration()
		{
			// Arrange
			var value = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.KAF, Bool = false, Bool2 = false, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.ELK, Bool = true, Bool2 = false, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, Bool = false, Bool2 = true, SystemDefined = true, },
			};
			value.SetDefaultCode(LoggingMethods.ELK, true);

			using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
			using (SystemDataRegistry.Instance.ElasticsearchServiceUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				using var view = new SearchBasedLogViewerControl();
				var bindingSource = new SearchBasedLogViewer();

				// Act
				view.SetDataBinding(bindingSource, "");

				// Assert
				var errors = UnitTestUserNotification.Instance.PreviousMessages.Where(x => !x.ToString().Trim().Equals("None"));
				AssertEquals("Error Elasticsearch logging configuration is invalid, check the registry.", errors.First().ToString());
				AssertEquals(1, errors.Count());
			}
		}

		[UseSnapshotProtection]
		public void TestErrorsShownWithKafkaLogViewingEnabledAndInvalidKafkaConfiguration()
		{
			// Arrange
			var value = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.KAF, Bool = true, Bool2 = false, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.ELK, Bool = false, Bool2 = false, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, Bool = false, Bool2 = true, SystemDefined = true, },
			};
			value.SetDefaultCode(LoggingMethods.KAF, true);

			using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
			using (SystemDataRegistry.Instance.KafkaElasticsearchServiceUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				using var view = new SearchBasedLogViewerControl();
				var bindingSource = new SearchBasedLogViewer();

				// Act
				view.SetDataBinding(bindingSource, "");

				// Assert
				var errors = UnitTestUserNotification.Instance.PreviousMessages.Where(x => !x.ToString().Trim().Equals("None"));
				AssertEquals("Error Kafka logging configuration is invalid, check the registry.", errors.First().ToString());
				AssertEquals(1, errors.Count());
			}
		}

		[UseSnapshotProtection]
		public void TestNoErrorsWithFileLoggingEnabledAndInvalidElasticConfiguration()
		{
			// Arrange
			var value = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.KAF, Bool = false, Bool2 = false, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.ELK, Bool = false, Bool2 = false, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool { Code = LoggingMethods.FSL, Bool = true, Bool2 = true, SystemDefined = true, },
			};
			value.SetDefaultCode(LoggingMethods.FSL, true);

			using (SystemDataRegistry.Instance.LoggingMethods.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, value))
			using (SystemDataRegistry.Instance.ElasticsearchServiceUri.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty))
			{
				using var view = new SearchBasedLogViewerControl();
				var bindingSource = new SearchBasedLogViewer();

				// Act
				view.SetDataBinding(bindingSource, "");

				// Assert
				var errors = UnitTestUserNotification.Instance.PreviousMessages.Where(x => !x.ToString().Trim().Equals("None")).ToList();
				AssertEquals($"There are errors: \n{string.Join("\n", errors)}", 0, errors.Count);
			}
		}
	}
}

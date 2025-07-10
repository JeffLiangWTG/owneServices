using System.IO;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.Integration.Licensing;
using Moq;
using NLog;
using NLog.Targets;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using ServiceManager.Logging.CW;

namespace Enterprise.ServiceManager.Shared.Testing.Logging
{
	public class NLogQueueMonitorTargetFactoryTest : TransactionedTestCase
	{
		public void TestGetOrCreateTargetWithConfigurationReturnsTarget()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange
				var factory = new NLogQueueMonitorTargetFactory(Mock.Of<IHostRegistrySettings>());

				// Act
				var target = factory.GetOrCreateTarget();

				// Assert
				AssertType<FileTarget>(target);
			}
		}

		public void TestGetOrCreateTargetWithNullConfigurationReturnsTarget()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange
				LogManager.Configuration = null;
				var factory = new NLogQueueMonitorTargetFactory(Mock.Of<IHostRegistrySettings>());

				// Act
				var target = factory.GetOrCreateTarget();

				// Assert
				AssertType<FileTarget>(target);
			}
		}

		public void TestGetOrCreateTargetWhenTargetAlreadyRegisteredReturnsTarget()
		{
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange
				var factory = new NLogQueueMonitorTargetFactory(Mock.Of<IHostRegistrySettings>());
				var expectedTarget = factory.GetOrCreateTarget();
				LogManager.Configuration.AddTarget(expectedTarget);

				// Act
				var target = factory.GetOrCreateTarget();

				// Assert
				AssertSame(expectedTarget, target);
			}
		}

		public void TestGetOrCreateTargetWithConfiguredRetentionPeriodSetsMaxArchiveFiles()
		{
			var hostRegistry = Mock.Of<IHostRegistrySettings>(o => o.ProcessControllerQueueMonitoringRetentionPeriodInDays == 2);

			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			{
				// Arrange
				var factory = new NLogQueueMonitorTargetFactory(hostRegistry);

				// Act
				var target = (FileTarget)factory.GetOrCreateTarget();

				// Assert
				AssertEquals(hostRegistry.ProcessControllerQueueMonitoringRetentionPeriodInDays, target.MaxArchiveFiles);
			}
		}

		public void TestGetOrCreateTargetConfiguresLogFilenameCorrectly()
		{
			// Arrange
			using (LoggerTestHelper.TemporaryLoggingConfiguration())
			using (ObjectFactory.Substitute(Mock.Of<IProductRegistration>(registration =>
						registration.Key == Mock.Of<IProductRegistrationKey>(key =>
							key.EnterpriseCode == "EDI"
							&& key.ServerCode == "SYD"))))
			{
				var fileName = Path.Combine(CommonProgramData.GetCargoWiseDirectory("Process Controller", "Queue Monitoring", string.Empty), "EDISYD_${date:format=yyyyMMdd}.log");
				var factory = new NLogQueueMonitorTargetFactory(Mock.Of<IHostRegistrySettings>());

				// Act
				var target = (FileTarget)factory.GetOrCreateTarget();

				// Assert
				var fileTarget = target;
				AssertEquals(fileName, fileTarget.FileName.ToString());
			}
		}
	}
}

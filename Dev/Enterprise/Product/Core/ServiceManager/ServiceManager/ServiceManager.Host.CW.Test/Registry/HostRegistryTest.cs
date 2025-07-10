using System;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.CW;

namespace Enterprise.ServiceManager.Host.Testing.Core
{
	class HostRegistryTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestInitialize()
		{
			// Arrange
			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(1, 2));
			versionMock
				.Setup(x => x.TransformationVersion)
				.Returns(new VersionLabel(3, 4));

			MockHostRegistryValues();
			RegistryItemDictionary.Instance.PurgeAll();
			ObjectFactory.Substitute(versionMock.Object);

			var settings = new HostRegistrySettings();
			var hostRegistry = new HostRegistry(settings);

			// Act
			hostRegistry.Initialize();

			// Assert
			AssertEquals(1234, settings.BusyRunnerWaitTimeInSeconds);

			AssertEquals(true, settings.ShowQueryStackTraceInProcessControllerEnabled);

			AssertEquals(true, settings.ElasticSearchLoggingEnabled);
			AssertEquals("https://elastic.org/test", settings.ElasticsearchServiceUri);
			AssertEquals("elasticUser", settings.ElasticsearchServerUserName);
			AssertEquals("elasticPwd", settings.ElasticsearchServerPassword);
			AssertEquals("elasticIndex", settings.ElasticsearchIndex);

			AssertEquals(true, settings.KafkaLoggingEnabled);
			AssertEquals("https://kafka.wtg.zone", string.Join(", ", settings.KafkaBrokers.GetAllCodes()));
			AssertEquals("KafkaTopic", settings.KafkaTopic);
			AssertEquals(KafkaSecurityProtocolOptions.PLAINTEXT, settings.ProcessControllerKafkaSecurity.SecurityProtocol);

			AssertEquals(13, settings.ServiceTaskHostTerminatorFrequency);
			AssertEquals(14, settings.ServiceTaskUnloadTimeoutInSeconds);

			AssertEquals(16, settings.SecondaryProcessSpinUpDelayInSeconds);
			AssertEquals(true, settings.ServiceTaskRunnerConnectionPoolingEnabled);
			AssertEquals(100, settings.ServiceTaskProcessingMaximumBatchSize);

			AssertEquals(2.5m, settings.ServiceTaskProcessingBatchSizeScalingFactor);
			AssertEquals(ProcessPriorityClass.Normal, settings.RunnerProcessPriorityValue);

			AssertEquals(TimeSpan.FromMilliseconds(1000), settings.ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary);
			AssertEquals(TimeSpan.FromMilliseconds(100), settings.ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog);
			AssertEquals(TimeSpan.FromMilliseconds(30), settings.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog);
			AssertEquals(TimeSpan.FromMilliseconds(5000), settings.ThrottlingTargetTimeToClearQueueBacklog);
			AssertEquals(1017, settings.ServiceTaskMemoryConstraint);
			AssertEquals("UPG", settings.ForcefullyDisabledTasks);
			AssertEquals(TimeSpan.FromSeconds(30), settings.ServiceTaskMaxWaitForResourceAvailability);
			AssertEquals(80, settings.ServiceTaskMaximumCpuLoadBeforeThrottlingTasks);
			AssertEquals(1m, settings.ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks);

			AssertEquals(true, settings.ProcessControllerQueueMonitoringEnabled);
			AssertEquals(TimeSpan.FromMinutes(1), settings.ProcessControllerQueueMonitoringFrequency);
			AssertEquals(7, settings.ProcessControllerQueueMonitoringRetentionPeriodInDays);

			AssertEquals(true, settings.SwitchRunnerToNetCore);
			AssertEquals("FOO", settings.ServiceTaskRunnerSpecificGroup["UPG"]);
			AssertEquals("FOO", settings.ServiceTaskRunnerSpecificGroup["FWK"]);
			AssertEquals("BAR", settings.ServiceTaskRunnerSpecificGroup["TSS"]);
			AssertEquals("BAR", settings.ServiceTaskRunnerSpecificGroup["ASD"]);
			AssertEquals("BAR", settings.ServiceTaskRunnerSpecificGroup["DSA"]);
			AssertEquals("BAZ", settings.ServiceTaskRunnerSpecificGroup["QWE"]);
		}

		[UseSnapshotProtection]
		public void TestRefresh()
		{
			// Arrange
			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(1, 2));
			versionMock
				.Setup(x => x.TransformationVersion)
				.Returns(new VersionLabel(3, 4));

			MockHostRegistryValues();
			RegistryItemDictionary.Instance.PurgeAll();

			ObjectFactory.Substitute(versionMock.Object);

			var settings = new HostRegistrySettings();
			var hostRegistry = new HostRegistry(settings);

			// Act
			hostRegistry.Refresh();

			// Assert
			AssertEquals(1234, settings.BusyRunnerWaitTimeInSeconds);
			AssertEquals(true, settings.ShowQueryStackTraceInProcessControllerEnabled);
			AssertEquals(true, settings.ElasticSearchLoggingEnabled);
			AssertEquals("https://elastic.org/test", settings.ElasticsearchServiceUri);
			AssertEquals("elasticUser", settings.ElasticsearchServerUserName);
			AssertEquals("elasticPwd", settings.ElasticsearchServerPassword);
			AssertEquals("elasticIndex", settings.ElasticsearchIndex);
			AssertEquals(true, settings.KafkaLoggingEnabled);
			AssertEquals("https://kafka.wtg.zone", string.Join(", ", settings.KafkaBrokers.GetAllCodes()));
			AssertEquals("KafkaTopic", settings.KafkaTopic);
			AssertEquals(KafkaSecurityProtocolOptions.PLAINTEXT, settings.ProcessControllerKafkaSecurity.SecurityProtocol);
			AssertEquals(13, settings.ServiceTaskHostTerminatorFrequency);
			AssertEquals(14, settings.ServiceTaskUnloadTimeoutInSeconds);
			AssertEquals(16, settings.SecondaryProcessSpinUpDelayInSeconds);
			AssertEquals(true, settings.ServiceTaskRunnerConnectionPoolingEnabled);
			AssertEquals(100, settings.ServiceTaskProcessingMaximumBatchSize);
			AssertEquals(2.5m, settings.ServiceTaskProcessingBatchSizeScalingFactor);
			AssertEquals(ProcessPriorityClass.Normal, settings.RunnerProcessPriorityValue);
			AssertEquals("UPG", settings.ForcefullyDisabledTasks);
			AssertEquals(true, settings.ProcessControllerQueueMonitoringEnabled);
			AssertEquals(TimeSpan.FromMinutes(1), settings.ProcessControllerQueueMonitoringFrequency);
			AssertEquals(7, settings.ProcessControllerQueueMonitoringRetentionPeriodInDays);

			AssertNotEquals(TimeSpan.FromMilliseconds(1000), settings.ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary);
			AssertNotEquals(TimeSpan.FromMilliseconds(100), settings.ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog);
			AssertNotEquals(TimeSpan.FromMilliseconds(30), settings.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog);
			AssertNotEquals(TimeSpan.FromMilliseconds(5000), settings.ThrottlingTargetTimeToClearQueueBacklog);
			AssertNotEquals(1017, settings.ServiceTaskMemoryConstraint);
			AssertNotEquals(TimeSpan.FromSeconds(30), settings.ServiceTaskMaxWaitForResourceAvailability);
			AssertNotEquals(80, settings.ServiceTaskMaximumCpuLoadBeforeThrottlingTasks);
			AssertNotEquals(1m, settings.ServiceTaskMaximumDiskQueueLengthBeforeThrottlingTasks);

			AssertEquals(true, settings.SwitchRunnerToNetCore);
			AssertEquals("FOO", settings.ServiceTaskRunnerSpecificGroup["UPG"]);
			AssertEquals("FOO", settings.ServiceTaskRunnerSpecificGroup["FWK"]);
			AssertEquals("BAR", settings.ServiceTaskRunnerSpecificGroup["TSS"]);
			AssertEquals("BAR", settings.ServiceTaskRunnerSpecificGroup["ASD"]);
			AssertEquals("BAR", settings.ServiceTaskRunnerSpecificGroup["DSA"]);
			AssertEquals("BAZ", settings.ServiceTaskRunnerSpecificGroup["QWE"]);
		}

		static void MockHostRegistryValues()
		{
			var systemRegistry = SystemDataRegistry.Instance;

			systemRegistry.BusyRunnerWaitTimeInSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1234);

			systemRegistry.ShowQueryStackTraceInProcessControllerEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			
			var loggingMethods = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection
			{
				new SystemDefinableCodeDescriptionBoolWithExtraBool
					{ Code = LoggingMethods.KAF, Bool = true, Bool2 = true, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool
					{ Code = LoggingMethods.ELK, Bool = true, Bool2 = true, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool
					{ Code = LoggingMethods.FSL, Bool = true, Bool2 = true, SystemDefined = true, },
				new SystemDefinableCodeDescriptionBoolWithExtraBool
					{ Code = LoggingMethods.SYS, Bool = true, Bool2 = true, SystemDefined = true, },
			};
			loggingMethods.SetDefaultCode(LoggingMethods.FSL, true);
			systemRegistry.LoggingMethods.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, loggingMethods);

			systemRegistry.ElasticsearchServiceUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://elastic.org/test");
			systemRegistry.ElasticsearchServerUserName.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "elasticUser");
			systemRegistry.ElasticsearchServerPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "elasticPwd");
			systemRegistry.ElasticsearchIndex.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "elasticIndex");
			
			var kafkaBrokers = new CodeDescriptionPairList();
			kafkaBrokers.AddPair("https://kafka.wtg.zone");
			var kafkaSecurity = new KafkaSecurity() { SecurityProtocol = KafkaSecurityProtocolOptions.PLAINTEXT };
			systemRegistry.KafkaBrokers.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, kafkaBrokers);
			systemRegistry.KafkaTopic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "KafkaTopic");
			systemRegistry.ProcessControllerKafkaSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, kafkaSecurity);

			systemRegistry.ServiceTaskHostTerminatorFrequency.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 13);
			systemRegistry.ServiceTaskUnloadTimeoutInSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 14);

			systemRegistry.SecondaryProcessSpinUpDelayInSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 16);
			systemRegistry.ServiceTaskRunnerConnectionPoolingEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			systemRegistry.ServiceTaskProcessingMaximumBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 100);
			systemRegistry.ServiceTaskProcessingBatchSizeScalingFactor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 2.5m);
			systemRegistry.RunnerProcessPriority.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				nameof(ProcessPriorityClass.Normal));

			systemRegistry.ForcefullyDisabledTasks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "UPG");
			systemRegistry.ServiceTaskMemoryConstraint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1017);

			systemRegistry.ProcessControllerQueueMonitoringEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			systemRegistry.ProcessControllerQueueMonitoringFrequencyInMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			systemRegistry.ProcessControllerQueueMonitoringRetentionPeriodInDays.SetValue(Guid.Empty, Guid.Empty, Guid.Empty,
				7);

			systemRegistry.SwitchRunnerToNetCore.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			systemRegistry.ServiceTaskRunnerSpecificGrouping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, @"{ ""FOO"": [""UPG"",""FWK""], ""BAR"": [""TSS"",""ASD"",""DSA""], ""BAZ"":[""QWE""] }");
			systemRegistry.EnableServiceTaskRunnerSpecificGrouping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		[UseSnapshotProtection]
		public void TestServiceTaskRunnerSpecificGroupJsonErrorDoesNotBreakRegistry() => AssertServiceTaskRunnerSpecificGroupInvalidSettingDoesNotCrashRegistry(@"{ ""FOO"" }");
		[UseSnapshotProtection]
		public void TestServiceTaskRunnerSpecificGroupDuplicatedTaskCodeDoesNotBreakRegistry() => AssertServiceTaskRunnerSpecificGroupInvalidSettingDoesNotCrashRegistry(@"{ ""FOO"": [""UPG"",""UPG""], ""BAR"": [""UPG"",""UPG"",""UPG""], ""BAZ"":[""UPG""] }");

		public void AssertServiceTaskRunnerSpecificGroupInvalidSettingDoesNotCrashRegistry(string registrySetting)
		{
			// Arrange
			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(1, 2));
			versionMock
				.Setup(x => x.TransformationVersion)
				.Returns(new VersionLabel(3, 4));

			MockHostRegistryValues();
			RegistryItemDictionary.Instance.PurgeAll();

			ObjectFactory.Substitute(versionMock.Object);

			var settings = new HostRegistrySettings();
			var hostRegistry = new HostRegistry(settings);
			SystemDataRegistry.Instance.EnableServiceTaskRunnerSpecificGrouping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SystemDataRegistry.Instance.ServiceTaskRunnerSpecificGrouping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting);

			// Act
			hostRegistry.Initialize();

			// Assert
			AssertEquals(true, settings.ServiceTaskRunnerSpecificGroup.ContainsKey("ERROR"));
		}

		[UseSnapshotProtection]
		public void TestEnableServiceTaskRunnerSpecificGroupFalse()
		{
			// Arrange
			var versionMock = new Mock<IDatabaseAspectVersions>();
			versionMock
				.Setup(x => x.SchemaVersion)
				.Returns(new VersionLabel(1, 2));
			versionMock
				.Setup(x => x.TransformationVersion)
				.Returns(new VersionLabel(3, 4));

			MockHostRegistryValues();
			RegistryItemDictionary.Instance.PurgeAll();

			ObjectFactory.Substitute(versionMock.Object);

			var settings = new HostRegistrySettings();
			var hostRegistry = new HostRegistry(settings);
			SystemDataRegistry.Instance.EnableServiceTaskRunnerSpecificGrouping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			SystemDataRegistry.Instance.ServiceTaskRunnerSpecificGrouping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, @"{ ""FOO"": [""UPG"",""FWK""], ""BAR"": [""TSS"",""ASD"",""DSA""], ""BAZ"":[""QWE""] }");

			// Act
			hostRegistry.Initialize();

			// Assert
			AssertEquals(0, settings.ServiceTaskRunnerSpecificGroup.Keys.Count());
		}
	}
}

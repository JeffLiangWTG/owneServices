using System;
using System.Diagnostics;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed partial class SystemDataRegistryTest
	{
		public void TestServiceTaskRunnerConnectionPoolingEnabled()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskRunnerConnectionPoolingEnabled,
				"ServiceTaskRunnerConnectionPoolingEnabled",
				"System/Process Controller",
				"Enable database connection pooling on service task runners",
				"When service tasks require a database connection, a connection pool creates a pool of warm connections to decrease connect/disconnect time between the application and the database. This option should be disabled if you are experiencing too many idle connections.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				false);
		}

		public void TestEnableStackTraceInUserContextSwitcher()
		{
			TestRegistryItem(
				ItemSet.EnableStackTraceInUserContextSwitcher,
				"EnableStackTraceInUserContextSwitcher",
				"System/Process Controller",
				"Enable Stack trace in User Context Switcher",
				"When set to Yes, a stack trace is created every time a temporary User context is created.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestRunnerProcessPriority()
		{
			TestGenericRegistryItem(
				ItemSet.RunnerProcessPriority,
				"RunnerProcessPriority",
				"System/Process Controller",
				"Service Task Process Priority",
				"Changes process priority of Service Task Runners in Windows. Lower priority allows Process Controller hosts to remain responsive in high CPU situations. Once this registry value changed, Service Task Runners will be restarted to apply new process priority.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				nameof(ProcessPriorityClass.BelowNormal)
				);
		}

		public void TestGetRunnerPriority()
		{
			CombineAssertions(() =>
			{
				TestGetRunnerPriority(ProcessPriorityClass.Normal);
				TestGetRunnerPriority(ProcessPriorityClass.BelowNormal);
				TestGetRunnerPriority(ProcessPriorityClass.Idle);
			});

			void TestGetRunnerPriority(ProcessPriorityClass input)
			{
				// Arrange
				using (SystemDataRegistry.Instance.RunnerProcessPriority.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, input.ToString()))
				{
					// Act
					var result = SystemDataRegistry.Instance.RunnerProcessPriorityValue;

					// Assert
					AssertEquals(input, result);
				}
			}
		}

		public void TestForcefullyDisabledTasks()
		{
			TestStringRegistryItem(
				ItemSet.ForcefullyDisabledTasks,
				"ForcefullyDisabledTasks",
				SystemDataRegistry.Categories.System_ProcessController,
				"Disable Service Tasks",
				"Setting a Service Task in the registry will disable it until removed. This should only be done in emergencies. The format is Service Task Code and delimiter ' | ' i.e.'ABC|DEF'.",
				RegistryStorageFlags.System,
				TextEditorType.TextBox,
				RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
				string.Empty,
				CharacterCase.Upper);
		}

		public void TestRequestedNumberOfProcessControllers()
		{
			TestRegistryItem(
				ItemSet.RequestedNumberOfProcessControllers,
				"RequestedNumberOfProcessControllers",
				RawDataRegistry.Categories.System_ProcessController,
				"Number Of Process Controllers",
				"Requested number of Process Controllers for this system in hosted environment.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				1,
				1,
				30);
		}

		public void TestServiceTaskConfigurationPollingIntervalInMinutes()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskConfigurationPollingIntervalInMinutes,
				"ServiceTaskConfigurationPollingIntervalInMinutes",
				RawDataRegistry.Categories.System_ProcessController,
				"Service Task Configuration Polling Interval",
				@"Polling interval in minutes to update the configuration of the service tasks cached in running Process Controllers with the database.
0 to disable the polling.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				15,
				0,
				1440);
		}

		public void TestServiceTaskRequirementCheckTimeLimitSeconds()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskRequirementCheckTimeLimitSeconds,
				"ServiceTaskRequirementCheckTimeLimitSeconds",
				RawDataRegistry.Categories.System_ProcessController,
				"Service Task Requirement Check Time Limit (seconds)",
				"Time limit in seconds for running requirement check methods for service tasks. If a requirement check takes longer than this to complete it will be considered failed and the service task initialization is stopped for the current task being processed.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				15,
				5,
				900);
		}

		public void TestServiceTaskProcessingMaximumBatchSize()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskProcessingMaximumBatchSize,
				"ServiceTaskProcessingMaximumBatchSize",
				RawDataRegistry.Categories.System_ProcessController,
				"Service Task processing maximum batch size",
				"Maximum number of service tasks that will be pulled from the work queue per dispatching round. Number of tasks pulled from queue will increase with each iteration according to the batch size scaling factor.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				5,
				1,
				5000);
		}

		public void TestServiceTaskProcessingBatchSizeScalingFactor()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskProcessingBatchSizeScalingFactor,
				"ServiceTaskProcessingBatchSizeScalingFactor",
				RawDataRegistry.Categories.System_ProcessController,
				"Service Task processing batch size scaling factor",
				"Scaling factor applied to service task batch size on each iteration.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				1.3m,
				1m,
				3m);
		}

		public void TestServiceTaskProcessingBatchDelayMilliseconds()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskProcessingBatchDelayMilliseconds,
				"ServiceTaskProcessingBatchDelayMilliseconds",
				RawDataRegistry.Categories.System_ProcessController,
				"Service Task processing batch delay (in milliseconds)",
				"The delay in milliseconds between each batch of service tasks to process. Recommended value is 200 milliseconds.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				1000,
				10,
				10000);
		}

		public void TestServiceTaskHttpProcessorMaxThreads()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskHttpProcessorMaxThreads,
				"ServiceTaskHttpProcessorMaxThreads",
				RawDataRegistry.Categories.System_ProcessController,
				"Service Task HTTP Processor Maximum Threads",
				"Maximum number of parallel threads that the Service Task HTTP Processor will create for processing the queue of incoming requests.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
				20,
				1,
				100);
		}

		public void TestSwitchToNewServiceTasksModule()
		{
			TestRegistryItem(
				ItemSet.SwitchToNewServiceTasksModule,
				"SwitchToNewServiceTasksModule",
				RawDataRegistry.Categories.System_ProcessController,
				"Switch to the new Service Tasks module",
				"If this is set to true, the current Service Tasks module will be replaced by the new Service Tasks module.",
				RegistryStorageFlags.System,
				RegistryOptions.IsHidden,
				true);
		}

		public void TestSwitchRunnerToNetCore()
		{
			TestRegistryItem(
				ItemSet.SwitchRunnerToNetCore,
				"SwitchRunnerToNetCore",
				RawDataRegistry.Categories.System_ProcessController,
				"Switch Runners to use Net Core (Experimental)",
				"If this is set to true, Process controller will run the experimental net core (Net 8) runner exe.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestEnableServiceTaskRunnerSpecificGrouping()
		{
			TestRegistryItem(
				ItemSet.EnableServiceTaskRunnerSpecificGrouping,
				"EnableServiceTaskRunnerSpecificGrouping",
				RawDataRegistry.Categories.System_ProcessController,
				"Enable tasks to run with specific runner groups (Experimental)",
				"If this is set to true, Process controllers will run service tasks in separate pools as defined in the ServiceTaskRunnerSpecificGrouping registry item.",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				false);
		}

		public void TestServiceTaskRunnerSpecificGrouping()
		{
			TestRegistryItem(
				ItemSet.ServiceTaskRunnerSpecificGrouping,
				"ServiceTaskRunnerSpecificGrouping",
				RawDataRegistry.Categories.System_ProcessController,
				"Define grouping for tasks to run with specific runners (Experimental)",
				@"If set then process controller will run service tasks on separate groups of runners that only run tasks in those groups. With no group tasks will fall back to generic runners
Define grouping as a string following json dictionary format, i.e.
{ ""FOO"": [""UPG"",""FWK""], ""BAR"": [""TSS"",""ASD"",""DSA""], ""BAZ"":[""QWE""] } - Defining 3 groups FOO, BAR and BAZ, containing [UPG, FWK], [TSS, ASD, DSA] and [ASD] respectively",
				RegistryStorageFlags.System,
				RegistryOptions.IsOnlyForSupport,
				TextEditorType.TextBox,
				string.Empty);
		}

		public class ProcessControllerCategoryTest : TestCase
		{
			public void TestSystem_ProcessController()
			{
				AssertEquals("System/Process Controller", RawDataRegistry.Categories.System_ProcessController);
			}

			public void TestSystem_ProcessController_Logging()
			{
				AssertEquals("System/Process Controller/Logging", SystemDataRegistry.Categories.System_ProcessController_Logging);
			}

			public void TestSystem_ProcessController_Logging_Kafka()
			{
				AssertEquals("System/Process Controller/Logging/Kafka", SystemDataRegistry.Categories.System_ProcessController_Logging_Kafka);
			}

			public void TestSystem_ProcessController_Logging_ElasticSearch()
			{
				AssertEquals("System/Process Controller/Logging/Elasticsearch", SystemDataRegistry.Categories.System_ProcessController_Logging_ElasticSearch);
			}

			public void TestSystem_ProcessController_Logging_Syslog()
			{
				AssertEquals("System/Process Controller/Logging/Syslog Logging", SystemDataRegistry.Categories.System_ProcessController_Logging_Syslog);
			}

			public void TestSystem_ProcessController_Logging_CombinedFileSystem()
			{
				AssertEquals("System/Process Controller/Logging/Combined File System Logging", SystemDataRegistry.Categories.System_ProcessController_Logging_CombinedFileSystem);
			}

			public void TestSystem_ProcessController_ResourceThrottling()
			{
				AssertEquals("System/Process Controller/Resource Throttling", SystemDataRegistry.Categories.System_ProcessController_ResourceThrottling);
			}

			public void TestSystem_ProcessController_DispatcherThrottling()
			{
				AssertEquals("System/Process Controller/Dispatcher Throttling", SystemDataRegistry.Categories.System_ProcessController_DispatcherThrottling);
			}

			public void TestSystem_ProcessController_Notifications()
			{
				AssertEquals("System/Process Controller/Notifications", SystemDataRegistry.Categories.System_ProcessController_Notifications);
			}

			public void TestSystem_ProcessController_QueueMonitoring()
			{
				AssertEquals("System/Process Controller/Queue Monitoring", SystemDataRegistry.Categories.System_ProcessController_QueueMonitoring);
			}
		}
	}
}

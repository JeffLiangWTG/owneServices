using System;
using System.Diagnostics;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	partial class SystemDataRegistry
	{
		bool ISystemDataRegistry.ServiceTaskRunnerConnectionPoolingEnabled => ServiceTaskRunnerConnectionPoolingEnabled.Value;

		public BooleanRegistryItem ServiceTaskRunnerConnectionPoolingEnabled
		{
			get =>
				GetItem("ServiceTaskRunnerConnectionPoolingEnabled", () =>
					new BooleanRegistryItem(
						"ServiceTaskRunnerConnectionPoolingEnabled",
						RawDataRegistry.Categories.System_ProcessController,
						ResString.GetMultilingualString("8e234505-3b7b-43dc-a192-c750f28746ac", "Enable database connection pooling on service task runners"),
						ResString.GetMultilingualString("f593f6d4-df93-4bb3-86aa-238c0ca60923", "When service tasks require a database connection, a connection pool creates a pool of warm connections to decrease connect/disconnect time between the application and the database. This option should be disabled if you are experiencing too many idle connections."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
						false));
		}

		public BooleanRegistryItem EnableStackTraceInUserContextSwitcher
		{
			get
			{
				return GetItem("EnableStackTraceInUserContextSwitcher", () =>
					new BooleanRegistryItem(
						"EnableStackTraceInUserContextSwitcher",
						Categories.System_ProcessController,
						ResString.GetMultilingualString("99c9714f-33f5-41ba-8d3b-30c1ad3db87c", "Enable Stack trace in User Context Switcher"),
						ResString.GetMultilingualString("88e8a3f1-9ced-440b-af69-06fe9331781c", "When set to Yes, a stack trace is created every time a temporary User context is created."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false
						));
			}
		}

		public ProcessPriorityClass RunnerProcessPriorityValue => (ProcessPriorityClass)Enum.Parse(typeof(ProcessPriorityClass), RunnerProcessPriority.Value);

		public CodePairRegistryItem RunnerProcessPriority
		{
			get => GetItem("RunnerProcessPriority", () =>
				{
					var processPriorityOptionsListProvider = new CodeDescriptionPairListProvider(() => new ProcessPriorityOptions());

					return new CodePairRegistryItem(
						"RunnerProcessPriority",
						Categories.System_ProcessController,
						ResString.GetMultilingualString("573322d7-c55e-462b-ab59-824f634c6191", "Service Task Process Priority"),
						ResString.GetMultilingualString("d8360853-50d1-4a3f-9348-5d69967f7aac", "Changes process priority of Service Task Runners in Windows. Lower priority allows Process Controller hosts to remain responsive in high CPU situations. Once this registry value changed, Service Task Runners will be restarted to apply new process priority."),
						processPriorityOptionsListProvider,
						false,
						true,
						new ComboBoxRegistryEditorInfo(processPriorityOptionsListProvider),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
						ProcessPriorityOptions.BelowNormal,
						false);
				});
		}

		public StringRegistryItem ForcefullyDisabledTasks
		{
			get =>
				GetItem("ForcefullyDisabledTasks", () =>
					new StringRegistryItem(
						"ForcefullyDisabledTasks",
						SystemDataRegistry.Categories.System_ProcessController,
						(NoResString)"Disable Service Tasks",
						(NoResString)"Setting a Service Task in the registry will disable it until removed. This should only be done in emergencies. The format is Service Task Code and delimiter ' | ' i.e.'ABC|DEF'.",
						new StringRegistryDataType(CharacterCase.Upper),
						new TextRegistryEditorInfo(TextEditorType.TextBox),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport | RegistryOptions.PreserveTestValue,
						string.Empty)
				);
		}

		public IntRegistryItem RequestedNumberOfProcessControllers
		{
			get =>
				GetItem("RequestedNumberOfProcessControllers", () =>
					new IntRegistryItem(
						"RequestedNumberOfProcessControllers",
						RawDataRegistry.Categories.System_ProcessController,
						(NoResString)"Number Of Process Controllers",
						(NoResString)"Requested number of Process Controllers for this system in hosted environment.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						1,
						1,
						30)
				);
		}

		public IntRegistryItem ServiceTaskConfigurationPollingIntervalInMinutes
		{
			get =>
				GetItem("ServiceTaskConfigurationPollingIntervalInMinutes", () =>
					new IntRegistryItem(
						"ServiceTaskConfigurationPollingIntervalInMinutes",
						RawDataRegistry.Categories.System_ProcessController,
						ResString.GetMultilingualString("{86FFCF7B-65D9-46BA-8EA4-C0944EC2AC63}", "Service Task Configuration Polling Interval"),
						ResString.GetMultilingualString(
							"{70F4EED2-9868-4E51-B4F9-D77F599C6CF5}",
							@"Polling interval in minutes to update the configuration of the service tasks cached in running Process Controllers with the database.
0 to disable the polling."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
						15,
						0,
						1440)
				);
		}

		public IntRegistryItem ServiceTaskProcessingMaximumBatchSize
		{
			get =>
				GetItem("ServiceTaskProcessingMaximumBatchSize", () =>
					new IntRegistryItem(
						"ServiceTaskProcessingMaximumBatchSize",
						RawDataRegistry.Categories.System_ProcessController,
						ResString.GetMultilingualString("{5B470B8E-1EA5-410E-8A16-AE371FC28C5B}", "Service Task processing maximum batch size"),
						ResString.GetMultilingualString("{7AB7194A-1D8B-40ED-8A46-64A456D0F58F}", "Maximum number of service tasks that will be pulled from the work queue per dispatching round. Number of tasks pulled from queue will increase with each iteration according to the batch size scaling factor."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
						5,
						1,
						5000)
				);
		}

		public DecimalRegistryItem ServiceTaskProcessingBatchSizeScalingFactor
		{
			get =>
				GetItem("ServiceTaskProcessingBatchSizeScalingFactor", () =>
					new DecimalRegistryItem(
						"ServiceTaskProcessingBatchSizeScalingFactor",
						RawDataRegistry.Categories.System_ProcessController,
						ResString.GetMultilingualString("{B65ACE3F-42FD-4250-8A0C-A93D158BC9B1}", "Service Task processing batch size scaling factor"),
						ResString.GetMultilingualString("{322592C7-484E-4B5B-9165-AACE51F0CF66}", "Scaling factor applied to service task batch size on each iteration."),
						new NumericRegistryEditorInfo(1),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
						1.3m,
						1f,
						3f)
				);
		}

		public IntRegistryItem ServiceTaskProcessingBatchDelayMilliseconds
		{
			get =>
				GetItem("ServiceTaskProcessingBatchDelayMilliseconds", () =>
					new IntRegistryItem(
						"ServiceTaskProcessingBatchDelayMilliseconds",
						RawDataRegistry.Categories.System_ProcessController,
						ResString.GetMultilingualString("{C459A554-0ED6-48FC-ADB7-E4E8C6656AEA}", "Service Task processing batch delay (in milliseconds)"),
						ResString.GetMultilingualString("{2A811D1C-54E5-42A8-8806-047BB0EA7394}", "The delay in milliseconds between each batch of service tasks to process. Recommended value is 200 milliseconds."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
						1000,
						10,
						10000)
				);
		}

		public TimeSpan ServiceTaskConfigurationPollingInterval => TimeSpan.FromMinutes(ServiceTaskConfigurationPollingIntervalInMinutes.Value);

		public IntRegistryItem ServiceTaskRequirementCheckTimeLimitSeconds
		{
			get =>
				GetItem("ServiceTaskRequirementCheckTimeLimitSeconds", () =>
					new IntRegistryItem(
						"ServiceTaskRequirementCheckTimeLimitSeconds",
						RawDataRegistry.Categories.System_ProcessController,
						ResString.GetMultilingualString("03E26B11-6F7D-4F2A-9243-18FE3582BAE2", "Service Task Requirement Check Time Limit (seconds)"),
						ResString.GetMultilingualString(
							"A64150C3-6AA0-4EC6-992B-F4E4015C39C4",
							"Time limit in seconds for running requirement check methods for service tasks. If a requirement check takes longer than this to complete it will be considered failed and the service task initialization is stopped for the current task being processed."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
						15,
						5,
						900)
				);
		}

		public TimeSpan ServiceTaskRequirementCheckTimeLimit => TimeSpan.FromSeconds(ServiceTaskRequirementCheckTimeLimitSeconds.Value);

		public IntRegistryItem ServiceTaskHttpProcessorMaxThreads
		{
			get =>
				GetItem("ServiceTaskHttpProcessorMaxThreads", () =>
					new IntRegistryItem(
						"ServiceTaskHttpProcessorMaxThreads",
						RawDataRegistry.Categories.System_ProcessController,
						ResString.GetMultilingualString("2BDC1052-8443-400A-8279-D9E531FF67F2", "Service Task HTTP Processor Maximum Threads"),
						ResString.GetMultilingualString("B40F9F29-72AA-48F0-A3C3-83C224E8E5D9", "Maximum number of parallel threads that the Service Task HTTP Processor will create for processing the queue of incoming requests."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsOnlyForController,
						20,
						1,
						100
					)
				);
		}

		public BooleanRegistryItem SwitchToNewServiceTasksModule
		{
			get =>
				GetItem("SwitchToNewServiceTasksModule", () =>
					new BooleanRegistryItem(
						"SwitchToNewServiceTasksModule",
						RawDataRegistry.Categories.System_ProcessController,
						(NoResString)"Switch to the new Service Tasks module",
						(NoResString)"If this is set to true, the current Service Tasks module will be replaced by the new Service Tasks module.",
						RegistryStorageFlags.System,
						RegistryOptions.IsHidden,
						true));
		}

		public BooleanRegistryItem SwitchRunnerToNetCore
		{
			get =>
				GetItem("SwitchRunnerToNetCore", () =>
					new BooleanRegistryItem(
						"SwitchRunnerToNetCore",
						RawDataRegistry.Categories.System_ProcessController,
						(NoResString)"Switch Runners to use Net Core (Experimental)",
						(NoResString)"If this is set to true, Process controller will run the experimental net core (Net 8) runner exe.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false));
		}

		public BooleanRegistryItem EnableServiceTaskRunnerSpecificGrouping
		{
			get =>
				GetItem("EnableServiceTaskRunnerSpecificGrouping", () =>
					new BooleanRegistryItem(
						"EnableServiceTaskRunnerSpecificGrouping",
						RawDataRegistry.Categories.System_ProcessController,
						(NoResString)"Enable tasks to run with specific runner groups (Experimental)",
						(NoResString)"If this is set to true, Process controllers will run service tasks in separate pools as defined in the ServiceTaskRunnerSpecificGrouping registry item.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false));
		}

		public StringRegistryItem ServiceTaskRunnerSpecificGrouping
		{
			get =>
				GetItem(nameof(ServiceTaskRunnerSpecificGrouping), () =>
					new StringRegistryItem(
						nameof(ServiceTaskRunnerSpecificGrouping),
						RawDataRegistry.Categories.System_ProcessController,
						(NoResString)"Define grouping for tasks to run with specific runners (Experimental)",
						(NoResString)@"If set then process controller will run service tasks on separate groups of runners that only run tasks in those groups. With no group tasks will fall back to generic runners
Define grouping as a string following json dictionary format, i.e.
{ ""FOO"": [""UPG"",""FWK""], ""BAR"": [""TSS"",""ASD"",""DSA""], ""BAZ"":[""QWE""] } - Defining 3 groups FOO, BAR and BAZ, containing [UPG, FWK], [TSS, ASD, DSA] and [ASD] respectively",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						string.Empty));
		}

		internal StringRegistryItem RegistryIndex
		{
			get
			{
				return GetItem(nameof(RegistryIndex),
					() => new StringRegistryItem(nameof(RegistryIndex), null, null, null, RegistryStorageFlags.System, RegistryOptions.IsHidden));
			}
		}
	}
}

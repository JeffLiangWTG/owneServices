using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public sealed class BMSRegistry : RegistryItemSet
	{
		#region Singleton pattern

		BMSRegistry()
		{
		}

		public static BMSRegistry Instance
		{
			get { return instance ?? (instance = new BMSRegistry()); }
		}

		[ThreadStatic]
		static BMSRegistry instance;

		#endregion

		public override bool IsForProductivityWise => true;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString WorkflowManager_BufferManagement => CombineCategories(WorkflowManager, ResString.GetMultilingualString("8055da27-3687-4c0c-a91f-9ea1363e543c", "Buffer Management"));
			public static MultilingualString WorkflowManager_BufferManagement_BehaviourManagement => CombineCategories(WorkflowManager, ResString.GetMultilingualString("89E2FA1F-37C3-4A34-9204-E32AC18A6D3B", "Behavior Management"));
			public static MultilingualString WorkflowManager_BufferManagement_ReleaseGate => CombineCategories(WorkflowManager_BufferManagement, ResString.GetMultilingualString("eda6fab6-70cb-4663-af0b-4cf89d413003", "Release Gate"));
			public static MultilingualString WorkflowManager_BufferManagement_ServiceTasks => CombineCategories(WorkflowManager_BufferManagement, ResString.GetMultilingualString("090423a9-0638-4cd6-ae19-469fa32352a6", "Service Tasks"));
			public static MultilingualString WorkflowManager_BufferManagement_Tags => CombineCategories(WorkflowManager_BufferManagement, ResString.GetMultilingualString("11C1FB4A-6D59-4345-899A-EB12EB336948", "Tags"));
			public static MultilingualString WorkflowManager_BufferManagement_AcceptabilityBands => CombineCategories(WorkflowManager_BufferManagement, ResString.GetMultilingualString("F4C6F42A-033A-4E1F-BD84-A206E8182D2E", "Acceptability Bands"));
			public static MultilingualString WorkflowManager_BufferManagement_VisualBoards => CombineCategories(WorkflowManager_BufferManagement, ResString.GetMultilingualString("75A8D81E-A273-4D72-8AB7-BFEBE23A1C00", "Visual Boards"));
			public static MultilingualString WorkflowManager_BufferManagement_ServerPerformance => CombineCategories(WorkflowManager_BufferManagement, ResString.GetMultilingualString("4e4fbfaf-413c-4d28-abd5-9287ad041f83", "Server Performance"));
			public static MultilingualString WorkflowManager_BufferManagement_SystemSchematic => CombineCategories(WorkflowManager_BufferManagement, ResString.GetMultilingualString("18BC383E-A293-4919-9E67-4B1EA597EF51", "System Schematic"));
			public static MultilingualString WorkflowManager_BufferManagement_Notification => CombineCategories(WorkflowManager_BufferManagement, ResString.GetMultilingualString("75315DC6-7286-4899-B8D0-813F5B16DC15", "Notification"));
			public static MultilingualString WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing => CombineCategories(WorkflowManager_BufferManagement, ResString.GetMultilingualString("D3173878-24A0-4F6A-A687-A344A08648E6", "Responsive PAVE Data Processing"));
			public static MultilingualString WorkflowManager_BufferManagement_Experimental => CombineCategories(WorkflowManager_BufferManagement, ResString.GetMultilingualString("9fa7fed3-99ab-49fa-b4da-a4976625833f", "Experimental"));
			public static MultilingualString WorkflowManager_BufferManagement_Experimental_Kafka => CombineCategories(WorkflowManager_BufferManagement_Experimental, ResString.GetMultilingualString("8e05a47b-98f0-4300-8b7b-8b27a4b4e67f", "Kafka"));
			public static MultilingualString WorkflowManager_BufferManagement_ReleaseSequences => CombineCategories(WorkflowManager_BufferManagement, ResString.GetMultilingualString("5FA6B16E-FB34-42FC-8325-864091B3B20E", "Release Sequences"));
			public static MultilingualString WorkflowManager_NetworkDiagrams => CombineCategories(WorkflowManager, ResString.GetMultilingualString("68140886-0e97-45c0-a87d-7ea83684bfa2", "Network Diagrams"));
		}

		#endregion

		#region Workflow Management Mode (previously Buffer Management Enabled)

		public CodePairRegistryItem WorkflowManagementMode
		{
			get => GetItem("WorkflowManagementMode", () => new CodePairRegistryItem(
				"WorkflowManagementMode",
				Categories.WorkflowManager_BufferManagement,
				ResString.GetMultilingualString("6b559069-68ef-4743-90dc-ceac5c17982d", "Workflow Management Mode"),
				ResString.GetMultilingualString("c186776a-5da2-4953-ab97-a14a335594ba",
					"Select the level of complexity and configuration suitable for your business. Advanced workflow, buffer, and planning management features are opt-in. Each of these settings need configuration to support the processes, steps, and flow of your business. Consult documentation to determine which setting is best for you."),
				new CodeDescriptionPairListProvider(() => new WorkflowManagementModes()),
				RegistryStorageFlags.System,
				defaultValue: GetDefaultWorkflowManagementMode())
			{
				OnUpdateAction = OnUpdateActionForWorkflowManagementToggle
			});
		}

		static string GetDefaultWorkflowManagementMode()
		{
			return DataRegistry.Instance.ProductivityWiseModeEnabled
				? WorkflowManagementModes.Codes.PlanningManagement
				: WorkflowManagementModes.Codes.BasicWorkflow;
		}

		static void OnUpdateActionForWorkflowManagementToggle(Guid companyPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			if (newValue is string mode)
			{
				BMSRegistryServiceTaskHelper.EnableAndDisableServiceTasksWhenWorkflowModeChanges(mode);

				if (BMSRegistry.Instance.DisableCapacityCalculations.Value)
				{
					BMSRegistryServiceTaskHelper.DisableCapacityDependentServiceTasks();
				}
			}
		}

		#endregion

		#region Require Resource to Close Task

		public BooleanRegistryItem RequireResourceToCloseTask
		{
			get
			{
				return GetItem("RequireResourceToCloseTask", () =>
				{
					return new BooleanRegistryItem(
						"RequireResourceToCloseTask",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("afd61989-87f3-4208-a081-e20a5e597356", "Require Resource to Close Task"),
						ResString.GetMultilingualString("c4737308-6392-42d5-b1c3-05fa9f7c9be9", "When checked, a task cannot be closed until a staff or resource is assigned, rather than allowing closure with a group."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Release Groups

		public BooleanRegistryItem RequireReleaseGroup
		{
			get
			{
				MultilingualString hint = ResString.GetMultilingualString("751862e9-9c6b-44f6-a89b-741d1d832475", "Specify whether Release Groups are mandatory on workflows. If activated, a validation error will be applied if no Release Group is present and groups are defined, otherwise a warning will be applied.");

				return GetItem("RequireReleaseGroup", delegate
				{
					return new BooleanRegistryItem(
						"RequireReleaseGroup",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("796b3e44-7848-41f0-94b8-beec2d39d2f4", "Require Release Group"),
						hint,
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public BooleanRegistryItem ReleaseGroupsCanBeAssignedFromOtherWorkflowsWhenApplyingTemplates
		{
			get
			{
				MultilingualString hint = ResString.GetMultilingualString("a048331f-49f4-42f3-b525-6d4fe858db28", "When enabled, workflows defined on workflow templates which do not have a release group specified will attempt to find a release group from other workflows in the job when being applied to that job. If this setting is disabled, release groups will remain blank when the workflow is applied to the job unless a release group can be otherwise assigned (by Release Group Rules, for example).");

				return GetItem("ReleaseGroupsCanBeAssignedFromOtherWorkflowsWhenApplyingTemplates", delegate
				{
					return new BooleanRegistryItem(
						"ReleaseGroupsCanBeAssignedFromOtherWorkflowsWhenApplyingTemplates",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("d225a8c5-7038-4d48-8ee0-c2de0eca9d0d", "Release Groups Can Be Assigned From Other Workflows When Applying Templates"),
						hint,
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Release Gate

		#region ReleaseGateBatchSize

		public IntRegistryItem ReleaseGateBatchSize
		{
			get
			{
				return GetItem("ReleaseGateBatchSize", () =>
				{
					return new IntRegistryItem(
						"ReleaseGateBatchSize",
						Categories.WorkflowManager_BufferManagement_ReleaseGate,
						ResString.GetMultilingualString("bcde1715-aa6f-4e07-ac80-e4a78788302c", "Release Gate Batch Size"),
						ResString.GetMultilingualString("bef705f8-354f-4d78-8db2-152ea1609e7e", "The number of workflows to process in one batch by the Release Gate."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						defaultValue: ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION,
						minValue: 10,
						maxValue: ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION);
				});
			}
		}

		#endregion

		#region ReleaseGateStaleTime

		public IntRegistryItem ReleaseGateStaleTime
		{
			get
			{
				return GetItem("ReleaseGateStaleTime", () =>
				{
					return new IntRegistryItem(
						"ReleaseGateStaleTime",
						Categories.WorkflowManager_BufferManagement_ReleaseGate,
						ResString.GetMultilingualString("f57acb30-5b43-4b0b-be19-44fb9cee9862", "Release Gate Stale Time in Minutes"),
						ResString.GetMultilingualString("92fc0fd6-6c03-4fd0-8f2d-0b46ad0ec12a", "The maximum number of minutes the Release Gate will spend on a single run before it refreshes the items in the queue to be released."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 20,
						minValue: 5,
						maxValue: 60 * 8);
				});
			}
		}

		#endregion

		#region MaximumCapabilityTaskOverloadLimit

		public DecimalRegistryItem MaximumCapabilityTaskOverloadLimit
		{
			get
			{
				return GetItem("MaximumCapabilityTaskOverloadLimit", () =>
				{
					return new DecimalRegistryItem(
						"MaximumCapabilityTaskOverloadLimit",
						Categories.WorkflowManager_BufferManagement_ReleaseGate,
						ResString.GetMultilingualString("503ABD43-7553-41B0-9D25-346AC7CB6D5A", "Maximum Capability Task Overload Limit"),
						ResString.GetMultilingualString("1CCF82FC-E08B-4966-B399-2B13056FD3B4", @"This limit is used when considering releasing a workflow which contains tasks requiring a capability and no assigned resource. The Release Gate requires that across all the relevant resources with the capability, there is at least as much available capacity as the Standard Estimate of those tasks. This number can be negative when resources have negative available capacity. The specified limit will cap how far into the negative any single resource's capacity can go.
In the case that a resource has negative available capacity, when summing capacities to get the capabilities total capacity, the amount of negative capacity of an individual resource will be capped at the limit, which is a factor of their Total Capacity.
Consider increasing this value when: Some resources having available capacity is causing other resources with the capability to become overloaded.

Consider lowering this value when: Some resources being overloaded is undesirably throttling down the release of work, and you have already considered other alternatives such as reassigning some of the work to other resources, deferring work that will realistically not be completed, and contacting a Wise Service Partner for assistance."),
						new NumericRegistryEditorInfo(2),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 1.20m,
						lowerBound: 1.00,
						upperBound: 2.00);
				});
			}
		}

		#endregion

		#region ResourceLeaveWindowforReleasingWork

		public IntRegistryItem ResourceLeaveWindowforReleasingWork
		{
			get
			{
				return GetItem("ResourceLeaveWindowforReleasingWork", () =>
				{
					return new IntRegistryItem(
						"ResourceLeaveWindowforReleasingWork",
						Categories.WorkflowManager_BufferManagement_ReleaseGate,
						ResString.GetMultilingualString("5717FD2F-0603-4E53-8021-1E4DC4CDB114", "Resource Leave Window for Releasing Work"),
						ResString.GetMultilingualString("583C2DF3-DC44-4066-BAB3-25DA5DDD9CE9", @"The size of the window (as a percentage of a buffer) during which work can be released to a resource who is away on leave.
For example, with a 6 day buffer and a 'Resource Leave Window for Releasing Work' of 40%, work involving this resource will only be released when the resource is returning from leave within 2.4 days, and normal Release Gate rules are met.
A value of 0 indicates no work can be released to a resource whilst they are on leave.
A value of 100 indicates that work can always be released to a resource whilst they are on leave, contingent upon normal Release Gate rules."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 40,
						minValue: 0,
						maxValue: 100);
				});
			}
		}

		#endregion

		#region CacheCalculatedCapacity

		public BooleanRegistryItem CacheCalculatedCapacity
		{
			get
			{
				return GetItem("CacheCalculatedCapacity", () =>
				{
					return new BooleanRegistryItem(
						"CacheCalculatedCapacity",
						Categories.WorkflowManager_BufferManagement_ReleaseGate,
						ResString.GetMultilingualString("1f0bcb69-657c-43aa-9f4c-8981aa0a807a", "Cache Calculated Capacity"),
						ResString.GetMultilingualString("106a91d5-6a76-48f4-9e43-ca6f004d3e51", "Cache the capacity for all resources involved with each buffer so that all other processes can use the cached capacity. This improves performance of various service tasks and Visual Boards. Resource capacity as seen from Visual Boards may be out of date between runs of the {0} service task.", ReleaseGateRunnerServiceTask.Code),
						RegistryStorageFlags.System,
						defaultValue: true);
				});
			}
		}

		#endregion

		#region BoardsAutoCalculateCapacityWhenNotInCache

		public BooleanRegistryItem BoardsAutoCalculateCapacityWhenNotInCache
		{
			get
			{
				return GetItem("BoardsAutoCalculateCapacityWhenNotInCache", () =>
				{
					return new BooleanRegistryItem(
						"BoardsAutoCalculateCapacityWhenNotInCache",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						(NoResString)"Boards Auto-Calculate Non-cached Capacity",
						(NoResString)"When enabled, a board will calculate capacity for any resource channel that needs capacity calculation but does not have an existing cached value. This can be disabled to reduce load on database servers, however some channels may not have capacity data to show until the Release Gate runs for them.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: true);
				});
			}
		}

		#endregion

		#region ReleaseGateConsiderParentWorkflowForCcpmRtrTag

		public BooleanRegistryItem ReleaseGateConsiderParentWorkflowForCcpmRtrTag
		{
			get
			{
				return GetItem("ReleaseGateConsiderParentWorkflowForCcpmRtrTag", () =>
				{
					return new BooleanRegistryItem(
						"ReleaseGateConsiderParentWorkflowForCcpmRtrTag",
						Categories.WorkflowManager_BufferManagement_ReleaseGate,
						ResString.GetMultilingualString("bc86fe60-1515-4acf-83d3-d3ddb7e6bdd0", "Consider Parent Workflow for CCPM RTR Tag"),
						ResString.GetMultilingualString("fe8f26da-1650-4267-adab-0c17d83ebea2", "Child workflows inherit CCPM RTR tag from their parent workflows when considering whether to release according to an approved CCPM schedule. Select No if child workflows should not inherit the RTR tag from their parent."),
						RegistryStorageFlags.System,
						defaultValue: true);
				});
			}
		}

		#endregion

		#region CachedCapacityStaleTimeMultiplier

		public IntRegistryItem CachedCapacityStaleTimeMultiplier
		{
			get
			{
				return GetItem("CachedCapacityStaleTimeMultiplier", () =>
				{
					return new IntRegistryItem(
						"CachedCapacityStaleTimeMultiplier",
						Categories.WorkflowManager_BufferManagement_ReleaseGate,
						ResString.GetMultilingualString("598126b7-d477-4656-94cb-8f9441125d53", "Cached Capacity Stale Time Multiplier"),
						ResString.GetMultilingualString("71c946dd-269e-4f10-8c2c-a466fb9b96fc", "A multiplier applied to the running frequency of the {0} service task used for calculating the time after initial calculation when the cached resource capacity becomes stale and is no longer used. Consider increasing this when the {0} service task is often not running for extended periods, or takes longer to run than its schedule calls for.", ReleaseGateRunnerServiceTask.Code),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 5,
						minValue: 2,
						maxValue: 10);
				});
			}
		}

		#endregion

		#region AutoAssignTasksRegardlessCapacity

		public BooleanRegistryItem AutoAssignTasksRegardlessCapacity
		{
			get
			{
				return GetItem("AutoAssignTasksRegardlessCapacity", () =>
				{
					return new BooleanRegistryItem(
						"AutoAssignTasksRegardlessCapacity",
						Categories.WorkflowManager_BufferManagement_ServiceTasks,
						ResString.GetMultilingualString("5D710FD8-454E-4157-A78F-7F9887A89FA7", "Auto Assign Tasks Regardless of Resource Capacity"),
						ResString.GetMultilingualString("3734020B-3342-436E-A5B8-A85787D86752", "Auto-Assignment Task Rule: Auto-assign tasks even if resources do not have sufficient capacity."),
						RegistryStorageFlags.System,
						defaultValue: true);
				});
			}
		}

		#endregion

		#region AutoAssignmentCapabilityTasksFailure

		public TemplateRegistryItem AutoAssignmentCapabilityTasksFailure
		{
			get
			{
				return GetItem("AutoAssignmentCapabilityTasksFailure", () =>
				{
					return new TemplateRegistryItem(
						"AutoAssignmentCapabilityTasksFailure",
						Categories.WorkflowManager_BufferManagement_ServiceTasks,
						ResString.GetMultilingualString("61188F0A-E083-42C6-B803-C92B07353F62", "Auto Assignment Capability Tasks Failure"),
						ResString.GetMultilingualString("B32770A9-96DD-4E2E-9620-091DDBA18FA3", "Automatically create a Work Item (matching the specified template) to aid the visibility of tasks auto-assignment failure due to capability / release group misconfiguration."),
						RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#region Ordering

		public BooleanRegistryItem UseDateOrderingForReleaseGate
		{
			get
			{
				return GetItem("UseDateOrderingForReleaseGate", () =>
				{
					return new BooleanRegistryItem(
						"UseDateOrderingForReleaseGate",
						Categories.WorkflowManager_BufferManagement_ReleaseGate,
						ResString.GetMultilingualString("7C4CCEA8-A2B0-463C-B2E5-CB4ADE55960F", "Use Date Ordering for Release Gate"),
						ResString.GetMultilingualString("B977140C-33EC-4424-A406-685A9588EE2F",
@"When enabled, the Release Gate will release workflows in order of Agreed Delivery Date, then workflows without an Agreed Delivery Date in order of effective nudge, last transfer time, and create time. 

When disabled, Agreed Delivery Date is not considered by the Release Gate for the purpose of the order of workflow release. Effective nudge, last transfer time, and create time are used alone to determine the order to release workflows."),
						RegistryStorageFlags.System,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region DisableCapacityCalculations

		public BooleanRegistryItem DisableCapacityCalculations
		{
			get
			{
				return GetItem("DisableCapacityCalculations", () => new BooleanRegistryItem(
					"DisableCapacityCalculations",
					Categories.WorkflowManager_BufferManagement_ReleaseGate,
					ResString.GetMultilingualString("5df9905b-fa69-481f-a75f-9a955312d66b",
						"Disable Capacity Calculations"),
					ResString.GetMultilingualString("65b4e0c8-ceb9-41c7-8d66-1370403f2cd1",
						@"When enabled, the BMG, BMC, BMD, and BMT service tasks will be disabled. Release Gate, staggered release, CCR, and auto-assignment features will be unavailable. Channel headers will not show capacity.
Enabling this setting can provide a significant performance boost for organizations that do not use these features.
Please note: Enabling this setting will automatically disable the listed service tasks. Disabling this setting will not automatically re-enable them--this must be done manually."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted,
					defaultValue: false
				)
				{ OnUpdateAction = OnUpdateActionForDisableCapacityCalculationsToggle });
			}
		}

		static void OnUpdateActionForDisableCapacityCalculationsToggle(Guid companyPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			if (newValue is bool isCapacityDisabled && isCapacityDisabled)
			{
				BMSRegistryServiceTaskHelper.DisableCapacityDependentServiceTasks();
			}
		}

		#endregion

		#region DisplayResponsiveReleaseGateUiSettings
		public BooleanRegistryItem DisplayResponsiveReleaseGateUiSettings
		{
			get
			{
				return GetItem("DisplayResponsiveReleaseGateUiSettings", () => new BooleanRegistryItem(
					"DisplayResponsiveReleaseGateUiSettings",
					Categories.WorkflowManager_BufferManagement_ReleaseGate,
					ResString.GetMultilingualString("95120a8f-99b9-4e30-b7cb-e4a167b150ca", "Display Responsive Release Gate UI Settings"),
					ResString.GetMultilingualString("bc709bfd-a52c-4a21-b538-e30cfd59c079", "When enabled, the UI settings for the responsive release gate will be displayed."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted,
					defaultValue: false
				));
			}
		}
		#endregion

		#endregion

		#region TransferRuleBatchSize

		public IntRegistryItem TransferRuleBatchSize
		{
			get
			{
				return GetItem("TransferRuleBatchSize", () =>
				{
					return new IntRegistryItem(
						"TransferRuleBatchSize",
						Categories.WorkflowManager_BufferManagement_ServiceTasks,
						ResString.GetMultilingualString("fb567e63-3bf6-40da-9f8e-5f235373d06c", "Transfer Rule Batch Size"),
						ResString.GetMultilingualString("6dd0bb5e-631a-4b65-a7cc-3af58615f94d", "The number of workflows to process in one batch by the Transfer Rule Runner."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						defaultValue: 200,
						minValue: 10,
						maxValue: 1000);
				});
			}
		}

		#endregion

		#region PerformanceLogsWarningThreshold

		public IntRegistryItem PerformanceLogsWarningThreshold
		{
			get
			{
				return GetItem("PerformanceLogsWarningThreshold", () =>
				{
					return new IntRegistryItem(
						"PerformanceLogsWarningThreshold",
						Categories.WorkflowManager_BufferManagement_ServiceTasks,
						ResString.GetMultilingualString("d2d2a939-75b2-4315-8176-fde81b052537", "Performance Logs Warning Threshold"),
						ResString.GetMultilingualString("ba323ed6-bb5d-4467-9421-613b012b9c13", "If the time taken for a batch to be processed exceeds this value (in milliseconds), it will be logged as a Warning. Otherwise it just gets logged as Debug."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						defaultValue: 5000,
						minValue: 0,
						maxValue: 60000);
				});
			}
		}

		#endregion

		#region TimeBeforeDeletingOldScheduledTasks

		public IntRegistryItem TimeBeforeDeletingOldScheduledTasks
		{
			get
			{
				return GetItem("TimeBeforeDeletingOldScheduledTasks", () =>
				{
					return new IntRegistryItem(
						"TimeBeforeDeletingOldScheduledTasks",
						Categories.WorkflowManager_BufferManagement_ServiceTasks,
						ResString.GetMultilingualString("3be869c7-d488-4a34-bbc7-562f35b33a10", "Time Before Deleting Old Scheduled Tasks"),
						ResString.GetMultilingualString("e418fc87-207a-47c9-899f-084a840eca34", "Any closed tasks in the Time Action Schedule that are at least this old (in months) will be deleted whenever TAS is executed. If set to 0, executed actions will be deleted immediately and not saved as Closed."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 0,
						minValue: 0,
						maxValue: 36);
				});
			}
		}

		#endregion

		#region TaskActionSchedulerBatchSize

		public IntRegistryItem TaskActionSchedulerBatchSize
		{
			get
			{
				return GetItem("TaskActionSchedulerBatchSize", () =>
				{
					return new IntRegistryItem(
						"TaskActionSchedulerBatchSize",
						Categories.WorkflowManager_BufferManagement_ServiceTasks,
						ResString.GetMultilingualString("4dbc3ea5-5809-4b7f-af67-47d567a4c769", "Task Action Scheduler Batch Size"),
						ResString.GetMultilingualString("592b6416-faf4-4322-b189-d543fef87f0a", "Maximum number of records to be processed in each batch by the Task Action Scheduler Service Task."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: 100,
						minValue: 1,
						maxValue: 100000);
				});
			}
		}

		#endregion

		#region ReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer

		public BooleanRegistryItem ReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer
		{
			get
			{
				return GetItem("ReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer", () =>
				{
					return new BooleanRegistryItem(
						"ReportWorkflowDeletionCallstackWhenLoadingWorkflowsToTransfer",
						Categories.WorkflowManager_BufferManagement_ServiceTasks,
						(NoResString)"Report Workflow Deletion Callstack When Loading Workflows To Transfer", // Non localised registry item
						(NoResString)"Report workflow deletion callstack when it happens that a loaded workflow to transfer is already deleted.", // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region StaggeredReleaseCalculatorBatchSize

		public IntRegistryItem StaggeredReleaseCalculatorBatchSize
		{
			get
			{
				return GetItem("StaggeredReleaseCalculatorBatchSize", () =>
				{
					return new IntRegistryItem(
						"StaggeredReleaseCalculatorBatchSize",
						Categories.WorkflowManager_BufferManagement_ServiceTasks,
						ResString.GetMultilingualString("fce25155-f975-41e4-a320-d8d690d10c38", "Staggered Release Calculator Batch Size"),
						ResString.GetMultilingualString("4be9390f-beba-427b-aa8c-cc7189f64059", "The number of workflows to process in each batch by the Staggered Release Calculator."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						defaultValue: 200,
						minValue: 10,
						maxValue: 1000);
				});
			}
		}

		#endregion

		#region TagRuleRunnerBatchSize

		public IntRegistryItem TagRuleRunnerBatchSize
		{
			get
			{
				return GetItem("TagRuleRunnerBatchSize", () =>
				{
					return new IntRegistryItem(
						"TagRuleRunnerBatchSize",
						Categories.WorkflowManager_BufferManagement_ServiceTasks,
						ResString.GetMultilingualString("f4c2deaf-61df-4d11-a9d2-6c567a75649f", "Work Queue Tag Rule Runner Batch Size"),
						ResString.GetMultilingualString("24e50ee5-3f94-4b83-ab20-e7d646c95a84", "The number of workflows to process in each batch by the Tag Rule Runner for Work Queues."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						defaultValue: 200,
						minValue: 10,
						maxValue: 1000);
				});
			}
		}

		public IntRegistryItem TagRuleRunnerPrimarySecondaryServerTransferBatchSize
		{
			get
			{
				return GetItem("TagRuleRunnerPrimarySecondaryServerTransferBatchSize", () =>
				{
					return new IntRegistryItem(
						"TagRuleRunnerPrimarySecondaryServerTransferBatchSize",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("50b7c11a-11e4-40b6-8ce7-727fe89b2099", "Tag Rule Runner Primary to Secondary Server Transfer Batch Size"),
						ResString.GetMultilingualString("554503d8-5abc-4000-a31a-65bc636ba03e", "The number of items to process in each batch by the Tag Rule Runner when transferring between the primary and secondary servers if available."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForController,
						defaultValue: 1000,
						minValue: 1,
						maxValue: 5000);
				});
			}
		}

		#endregion

		#region CapabilityAutoAssignmentBatchSize

		public IntRegistryItem CapabilityAutoAssignmentBatchSize
		{
			get
			{
				return GetItem("CapabilityAutoAssignmentBatchSize", () =>
				{
					return new IntRegistryItem(
						"CapabilityAutoAssignmentBatchSize",
						Categories.WorkflowManager_BufferManagement_ServiceTasks,
						ResString.GetMultilingualString("b1ecd6e7-87f0-4673-b8c8-aadc83189355", "Capability Task Auto-Assignment Batch Size"),
						ResString.GetMultilingualString("438d5dfb-26fb-45fc-adf5-621e95c31a2b", "The number of workflows to process in each batch by the Capability Task Auto-Assignment service task."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						defaultValue: 200,
						minValue: 10,
						maxValue: 1000);
				});
			}
		}

		#endregion

		#region LowSwitchCostStartableTaskDurationFactor

		public IntRegistryItem LowSwitchCostStartableTaskDurationFactor
		{
			get
			{
				return GetItem("LowSwitchCostStartableTaskDurationFactor", () =>
					new IntRegistryItem(
						"LowSwitchCostStartableTaskDurationFactor",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("e4b3d2d3-addf-44bf-94e0-f187d75c5d4a", "Low Switch Cost Startable Task Duration Percentage"),
						ResString.GetMultilingualString("878a126f-53f7-4f20-ad8e-ae42091e4f65", "The percentage a non-interruptible task's standard estimated duration must be of the time until a critical handover in order to be classified as 'startable', giving it a highlighted border."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 50,
						minValue: 1,
						maxValue: 100)
				);
			}
		}

		#endregion

		#region PersistentlyOverloadedBufferFactor

		public DecimalRegistryItem PersistentlyOverloadedBufferFactor
		{
			get
			{
				return GetItem("PersistentlyOverloadedBufferFactor", () =>
					new DecimalRegistryItem(
						"PersistentlyOverloadedBufferFactor",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("ee95c5e7-94a2-4b3b-8b2c-b0a4d7b3264c", "Persistently Overloaded Buffer Factor"),
						ResString.GetMultilingualString("66aca3f0-7070-46ed-9e42-bf7a7b5b6c91", "The factor of a buffer component's Buffer Timespan which is used to consider whether a resource has been persistently overloaded for the purpose of classifying them as a candidate Capacity Constrained Resource."),
						new NumericRegistryEditorInfo(2),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 1m,
						lowerBound: 1.0,
						upperBound: double.MaxValue)
				);
			}
		}

		#endregion

		#region LogWorkflowLoadTime

		public BooleanRegistryItem LogWorkflowLoadTime
		{
			get
			{
				return GetItem("LogWorkflowLoadTime", () =>
				{
					return new BooleanRegistryItem(
						"LogWorkflowLoadTime",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("6bcd2132-97a9-4f2e-ab66-f0e31032fbe3", "Log Workflow Load Time"),
						ResString.GetMultilingualString("002c4f84-7ce5-4892-b1b7-6af837672052", "Log workflow load time on the {0} and PVE service tasks for each component link.", TransferRuleRunnerServiceTask.Code),
						RegistryStorageFlags.System,
						defaultValue: true);
				});
			}
		}

		#endregion

		#region LogWorkflowInfoOnTransfer

		public BooleanRegistryItem LogWorkflowInfoOnTransfer
		{
			get
			{
				return GetItem("LogWorkflowInfoOnTransfer", () =>
				{
					return new BooleanRegistryItem(
						"LogWorkflowInfoOnTransfer",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("23ffbe69-4d89-467c-8e9e-d2e25dcb7423", "Log Workflow Info on Transfer"),
						ResString.GetMultilingualString("6eef7b48-87b8-4e88-8cb3-06f16987f6c6", "When enabled, the description and job number of workflows will be logged to the {0} and PVE service task log as they are moved between components.", TransferRuleRunnerServiceTask.Code),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region Visual Boards

		#region MaxNumberOfItemsOnBoards

		public IntRegistryItem MaxNumberOfItemsOnBoards
		{
			get
			{
				return GetItem("MaxNumberOfTicketsOnBoards", () =>
				{
					return new IntRegistryItem(
						"MaxNumberOfTicketsOnBoards",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("ecdae5c0-002f-497f-900d-1ce1c9505d65", "Maximum Number of Items on Visual Boards"),
						ResString.GetMultilingualString("a704e926-5317-4c48-b2ba-c79966b37238", "The maximum number of items to show on a Visual Board section before the section is not shown on the board"),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						defaultValue: 1000,
						minValue: 100,
						maxValue: 10000);
				});
			}
		}

		#endregion

		#region DefaultBoardRefreshIntervalMinutes

		public IntRegistryItem DefaultBoardRefreshIntervalMinutes
		{
			get
			{
				return GetItem("DefaultBoardRefreshIntervalMinutes", () =>
				{
					return new IntRegistryItem(
						"DefaultBoardRefreshIntervalMinutes",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("eba3e069-442f-48c8-b9b9-1a65fce4a26d", "Default Board Refresh Interval (in minutes)"),
						ResString.GetMultilingualString("44d1138e-6c9e-4200-a2f9-dd2b6247d589", "The default interval between auto-refresh of Visual Boards, and the default duration of slide show frames."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.All,
						RegistryOptions.Default,
						defaultValue: 10,
						minValue: 10,
						maxValue: 99999);
				});
			}
		}

		#endregion

		#region ShowAcceptabilityBandsOnBoards

		public BooleanRegistryItem ShowAcceptabilityBandsOnBoards
		{
			get
			{
				return GetItem("ShowAcceptabilityBandsOnBoards", () =>
				{
					return new BooleanRegistryItem(
						"ShowAcceptabilityBandsOnBoards",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("db167d36-09c6-4e5b-a8e1-57fa4e612277", "Show Acceptability Bands on Visual Boards"),
						ResString.GetMultilingualString("9f75888c-6477-40e9-b8d6-b7bd187d04b2", "When set to No, do not show Acceptability Band tiles and section heading status on all Visual Boards."),
						RegistryStorageFlags.All,
						defaultValue: true);
				});
			}
		}

		#endregion

		#region CapacityCalculatorStaffBatchSize

		public IntRegistryItem CapacityCalculatorStaffBatchSize
		{
			get
			{
				return GetItem("CapacityCalculatorStaffBatchSize", () =>
				{
					return new IntRegistryItem(
						"CapacityCalculatorStaffBatchSize",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("3052168D-4042-49B4-8912-68C35A8BC578", "Capacity calculator staff batch size"),
						ResString.GetMultilingualString("7C784308-A29C-410D-AAED-E2B990D0E9D0", "The capacity calculation when it occurs through the service task can be executed for many staff causing an execution timeout. This registry item controls the maximum number of staff codes that will be included per database call."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						defaultValue: 200,
						minValue: 20,
						maxValue: 1000);
				});
			}
		}

		#endregion

		#region UpdateTicketsWithDataRefresh

		public BooleanRegistryItem UpdateTicketsWithDataRefresh
		{
			get
			{
				return GetItem("UpdateTicketsWithDataRefresh", () =>
				{
					return new BooleanRegistryItem(
						"UpdateTicketsWithDataRefresh",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("a93bca9a-adcd-4177-ada8-286a24024835", "Update tickets with Data Refresh"), // Developer only string
						ResString.GetMultilingualString("bf37b350-531a-4d17-9d24-59cb0532c4ac", "Update tickets when tasks and workflows are saved outside Visual Boards."), // Developer only string
						RegistryStorageFlags.System,
						defaultValue: true);
				});
			}
		}

		#endregion

		#region ShowIdleTimeOnBoards

		public BooleanRegistryItem ShowIdleTimeOnBoards
		{
			get
			{
				return GetItem("ShowIdleTimeOnBoards", () =>
				{
					return new BooleanRegistryItem(
						"ShowIdleTimeOnBoards",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("d2207298-2394-4b62-a6a2-7a3175bfc121", "Show Idle Time on Visual Boards"),
						ResString.GetMultilingualString("0f69c2c6-82dd-49c1-a5df-c5e37fd29a0d", "When set to No, do not show the Idle time on all Visual Board channels."),
						RegistryStorageFlags.All,
						defaultValue: true);
				});
			}
		}

		#endregion

		#region MaxNumberOfItemsOnBoardsForCache

		public IntRegistryItem MaxNumberOfItemsAllowedToCacheBoardInSlideShow
		{
			get
			{
				return GetItem("MaxNumberOfItemsAllowedToCacheBoardInSlideShow", () =>
				{
					return new IntRegistryItem(
						"MaxNumberOfItemsAllowedToCacheBoardInSlideShow",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("A8738272-1E4D-40C9-9146-33E5AB1DB7D6", "Maximum Number of Items To Cache Board In Slide Show"),
						ResString.GetMultilingualString("6E79456F-E25E-4F76-91D9-DDBC2EE11756", "Slide shows can consist of multiple boards which can each display many tickets. Since the system retains each slide in memory even when it is not being shown, when there are many tickets per board the system may run out of memory. This registry item controls the maximum number of tickets per board before the slide is re-loaded each time it is switched to rather than retaining it in memory."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						defaultValue: 300,
						minValue: 100,
						maxValue: 1000);
				});
			}
		}

		#endregion

		#region MaxNumberOfBoardsAllowedToCacheInSlideShow

		public IntRegistryItem MaxNumberOfBoardsAllowedToCacheInSlideShow
		{
			get
			{
				return GetItem("MaxNumberOfBoardsAllowedToCacheInSlideShow", () =>
				{
					return new IntRegistryItem(
						"MaxNumberOfBoardsAllowedToCacheInSlideShow",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("75D0E1DF-A994-4E0A-8ECF-1E892F20E213", "Maximum Number Of Boards Allowed To Cache In Slide Show"),
						ResString.GetMultilingualString("016B5DFE-3F66-47C5-A948-2862BF69D751", "Slide shows can consist of multiple boards. Since the system retains each board in memory even when it is not being shown, when there are many boards the system may run out of memory. This registry item controls the maximum number of boards that are retained in memory."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						defaultValue: 5,
						minValue: 0,
						maxValue: 30);
				});
			}
		}

		#endregion

		#region SectionProgressStatisticsHistoryRange

		public IntRegistryItem SectionProgressStatisticsHistoryRange
		{
			get
			{
				return GetItem("SectionProgressStatisticsHistoryRange", () =>
				{
					return new IntRegistryItem(
						"SectionProgressStatisticsHistoryRange",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("F8EF3EFE-54F8-48F7-ACCF-3BFFA6CA7910", "Section Progress Statistics History Range"),
						ResString.GetMultilingualString("75646F3E-9B9B-42D2-8040-04F6081FB88B", "The number of hours into the past to consider board loading time statistics when estimating how long a board section will take to load. This is used when displaying the loading percentage next to the section name."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						defaultValue: 72,
						minValue: 0,
						maxValue: 168); // 7 days
				});
			}
		}

		#endregion

		#region Open Board in Browser

		public BooleanRegistryItem PAVEOnTheWeb
		{
			get
			{
				return GetItem("PAVEOnTheWeb", () =>
				{
					return new BooleanRegistryItem(
						"PAVEOnTheWeb",
						Categories.WorkflowManager_BufferManagement,
						(NoResString)"Enable PAVE On The Web", // Non localised registry item
						(NoResString)"Allows users to perform PAVE operations on the Web using GLOW.", // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region FullRefreshTimer

		public IntRegistryItem NumberOfRefreshesBeforeFullRefresh
		{
			get
			{
				return GetItem("NumberOfRefreshesBeforeFullRefresh", () =>
				{
					return new IntRegistryItem(
						"NumberOfRefreshesBeforeFullRefresh",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("EAB9E50C-78DC-4AFE-8DA2-A3C0DC614AB2", "Number Of Refreshes Before Full Refresh"),
						ResString.GetMultilingualString("9BA1CD72-0011-452C-8FDB-AD1D56DBCA99", "The number of board refreshes which only refresh tickets that changed before a full refresh of all tickets will be done."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 10,
						minValue: 0, // disable full-refresh-delay-countdown
						maxValue: 1000);
				});
			}
		}

		#endregion

		#region Channel Header Uses Preferred Name

		public BooleanRegistryItem ChannelHeadingsUsePreferredName
		{
			get
			{
				return GetItem("ChannelHeadingsUsePreferredName", () =>
				{
					return new BooleanRegistryItem(
						"ChannelHeadingsUsePreferredName",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("9381C676-9C47-4BE4-B3C4-121F169975C6", "Channel Headers use Preferred Name"),
						ResString.GetMultilingualString("5CA8C627-0237-46A4-8058-A527BC425DC8", "Channel Headers on Visual Boards use the resource's Preferred Name rather than their full name."),
						RegistryStorageFlags.System,
						defaultValue: true);
				});
			}
		}

		#endregion

		#region Enable MENT Sections

		public BooleanRegistryItem EnableMENTSections
		{
			get
			{
				return GetItem("EnableMENTSections", () =>
				{
					return new BooleanRegistryItem(
						"EnableMENTSections",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("43130494-E5F1-4453-912B-73FCCB80B41E", "Enable MENT Sections on Visual Boards"),
						ResString.GetMultilingualString("930C36EA-C145-4183-8E7A-A1C30A18AB5A", "Enable MENT Sections on Visual Boards."),
						RegistryStorageFlags.System,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region BoardOnSecondaryServer

		public BooleanRegistryItem BoardOnSecondaryServer
		{
			get
			{
				return GetItem("BoardOnSecondaryServer", () =>
				{
					return new BooleanRegistryItem(
						"BoardOnSecondaryServer",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("78d36421-7b48-4245-8ccb-8bfee9e56937", "When enabled boards try to load data using secondary server"),
						ResString.GetMultilingualString("45477289-3e05-4574-975c-606f5165d63a", "When enabled boards load data using secondary server when the System->Reports->Reporting databases full server names registry item is configured."),
						RegistryStorageFlags.System,
						EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForController,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region SynchroniseBufferPenetration

		public BooleanRegistryItem SynchroniseBufferPenetration
		{
			get
			{
				return GetItem("SynchroniseBufferPenetration", () =>
				{
					return new BooleanRegistryItem(
						"SynchroniseBufferPenetration",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						(NoResString)"Synchronise Buffer Penetration and enable NCN buffers",
						(NoResString)"When enable will consider parent and child workflows to calculate buffer penetration and approved NCN buffers.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: true);
				});
			}
		}

		#endregion

		#region BoardShowsTaskTags

		public BooleanRegistryItem BoardShowTaskTags
		{
			get
			{
				return GetItem("BoardShowTaskTags", () =>
				{
					return new BooleanRegistryItem(
						"BoardShowTaskTags",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						ResString.GetMultilingualString("C230C739-6F3E-40B4-8E95-955FF3E45FCF", "Show task tags"),
						ResString.GetMultilingualString("A7B4669B-68CB-430A-A41A-EE5124C71409", "When enabled Visual Boards shows task tags on tickets."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: false);
				});
			}
		}

		#endregion

#if DEBUG

		#region DisallowDBHitsOnBoardGUIThread

		public BooleanRegistryItem DisallowDBHitsOnBoardGUIThread
		{
			get
			{
				return GetItem("DisallowDBHitsOnBoardGUIThread", () =>
				{
					return new BooleanRegistryItem(
						"DisallowDBHitsOnBoardGUIThread",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						(NoResString)"Disallow database hits on Visual Board GUI thread", // Non localised registry item
						(NoResString)"Disallow database hits on Visual Board GUI thread after the data source is built.", // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region AllowedTablesDBHitsOnBoardGUIThread

		public StringArrayRegistryItem AllowedTablesDBHitsOnBoardGUIThread
		{
			get
			{
				return GetItem("AllowedTablesDBHitsOnBoardGUIThread", () =>
				{
					return new StringArrayRegistryItem(
						"AllowedTablesDBHitsOnBoardGUIThread",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						(NoResString)"Allowed tables to be hit on Visual Board GUI thread", // Non localised registry item
						(NoResString)"Allowed tables to be hit on Visual Board GUI thread when 'Disallow database hits on Visual Board GUI thread' is activated", // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		#endregion

#endif

		#region BoardBatchTagDBHits

		public BooleanRegistryItem BoardBatchTagDBHits
		{
			get
			{
				return GetItem("BoardBatchTagDBHits", () =>
				{
					return new BooleanRegistryItem(
						"BoardBatchTagDBHits",
						Categories.WorkflowManager_BufferManagement_VisualBoards,
						(NoResString)"Enable tags batch database hits on Visual Board", // Non localised registry item
						(NoResString)"When enabled tags database hits will be executed in batches", // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false);
				});
			}
		}

		#endregion

		#endregion

		#region WorkshopJobTypes

		public CodeDescriptionPairListRegistryItem WorkshopJobTypes
		{
			get
			{
				return GetItem("WorkshopJobTypes", () =>
				{
					return new CodeDescriptionPairListRegistryItem(
						name: "WorkshopJobTypes",
						category: Categories.WorkflowManager_BufferManagement,
						caption: ResString.GetMultilingualString("62f342f8-dd57-49ee-b170-d5a21269c5f7", "Workshop Job Types"),
						hint: ResString.GetMultilingualString("aa026251-b2b3-4729-8e67-21e70b1223fc", "Valid job types which can be used for creating workflow templates."),
						maxCodeLength: 3,
						storage: RegistryStorageFlags.System);
				});
			}
		}

		#endregion

		#region WorkflowCategories

		public WorkflowCategoriesRegistryItem WorkflowCategories
		{
			get
			{
				return GetItem("WorkflowCategories", () =>
					new WorkflowCategoriesRegistryItem(
						name: "WorkflowCategories",
						category: Categories.WorkflowManager_BufferManagement,
						caption: ResString.GetMultilingualString("16382cac-27d9-4dad-855a-b71229ef2b1a", "Workflow Categories"),
						hint: ResString.GetMultilingualString("d21a30b1-5039-4785-93fe-6e864c9ede65", "This is the list of Workflow Categories for Workflow Manager."),
						storage: RegistryStorageFlags.System,
						CategorisedWorkflowCategoriesCollection.GetDefault()));
			}
		}

		#endregion

		#region DeferralReasons

		public CodeDescriptionPairListRegistryItem DeferralReasons
		{
			get
			{
				var list = new WorkflowDeferralReasonsList();
				return GetItem("DeferralReasons", () =>
				{
					return new CodeDescriptionPairListRegistryItem(
						name: "DeferralReasons",
						category: Categories.WorkflowManager_BufferManagement,
						caption: ResString.GetMultilingualString("38869151-2bdc-49e0-a97d-893c9e99f6c8", "Deferral Reasons"),
						hint: ResString.GetMultilingualString("e7c99195-aa57-4856-8edb-cd4d6950c481", "Reasons for deferring a workflow."),
						maxCodeLength: 3,
						storage: RegistryStorageFlags.System,
						defaultValue: list);
				});
			}
		}

		#endregion

		#region Enable Task Countdowns

		public BooleanRegistryItem EnableTaskCountdowns
		{
			get
			{
				return GetItem("EnableTaskCountdowns", () =>
				{
					return new BooleanRegistryItem(
						"EnableTaskCountdowns",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("365B4150-1D8E-4159-B5AF-BBB5573135D3", "Enable Task Countdowns"),
						ResString.GetMultilingualString("1F057553-1E62-46D8-B53F-08658F1B9E91", "When enabled, task cards on visual boards will display estimated number of hours until countdowns when estimated handover time is set on the preceding tasks."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: true);
				});
			}
		}

		#endregion

		#region LogReleaseIntoBufferDetails

		public BooleanRegistryItem LogReleaseIntoBufferDetails
		{
			get
			{
				return GetItem("LogSuccessfulReleaseDetails", () =>
				{
					return new BooleanRegistryItem(
						"LogSuccessfulReleaseDetails",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("bf434abb-164e-4846-ae2d-d9058e9a7e64", "Log Release into Buffer Capacity Details"),
						ResString.GetMultilingualString("6140765d-7e1e-4557-9545-73d9e7430f7d", "Logs capacity details for resources involved when a workflow is successfully released to a buffer by the Release Gate."),
						RegistryStorageFlags.System,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region LogTransferIntoBufferDetails

		public BooleanRegistryItem LogTransferIntoBufferDetails
		{
			get
			{
				return GetItem("LogTransferIntoBufferDetails", () =>
				{
					return new BooleanRegistryItem(
						"LogTransferIntoBufferDetails",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("8d94a3b7-954d-4b36-9482-72066219efae", "Log Transfer into Buffer Capacity Details"),
						ResString.GetMultilingualString("1744bfd7-8558-4763-ac50-85458bae30f1", "Logs capacity details for resources involved when a workflow is transferred into a buffer either manually by a user or by a non-Release Gate component link."),
						RegistryStorageFlags.System,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region Tag Rule Throttling

		public TagRuleThrottlingRegistryItem TagRuleThrottling
		{
			get
			{
				return GetItem("TagRuleThrottling",
					() => new TagRuleThrottlingRegistryItem("TagRuleThrottling",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("11e9e6ad-1307-4871-bf55-52a9c8210da6", "Tag Rule Frequency Throttling"),
						ResString.GetMultilingualString("3304809b-845c-4401-8dd3-b90057138112", "Tag rules that run for more than a minimum run time will be run less frequently, at an interval specified here."),
						RegistryStorageFlags.System,
						TagRuleThrottlingHeader.GetDefaultThresholds()));
			}
		}

		#endregion

		#region MaximumDepthOfAnalyzedWorkflowHierarchy

		public IntRegistryItem MaximumDepthOfAnalyzedWorkflowHierarchy
		{
			get
			{
				return GetItem("MaximumDepthOfAnalyzedWorkflowHierarchy", () =>
				{
					return new IntRegistryItem(
						"MaximumDepthOfAnalyzedWorkflowHierarchy",
						Categories.WorkflowManager_BufferManagement_ServiceTasks,
						ResString.GetMultilingualString("69205031-e8be-44f0-a969-3ffe308962a0", "Maximum Depth Of Analyzed Workflow Hierarchy"),
						ResString.GetMultilingualString("6b812edb-eddd-4e27-844b-b45b30fbf749", "This is the maximum depth of analyzed workflow hierarchy when considering ancestor/descendant workflows for capacity calculations."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 30,
						minValue: 10,
						maxValue: 200);
				});
			}
		}

		#endregion

		#region AllowAutomaticUnmarkingOfCCRs

		public BooleanRegistryItem AllowAutomaticUnmarkingOfCCRs
		{
			get
			{
				return GetItem("AllowAutomaticUnmarkingOfCCRs", () =>
				{
					return new BooleanRegistryItem(
						"AllowAutomaticUnmarkingOfCCRs",
						Categories.WorkflowManager_BufferManagement_ServiceTasks,
						ResString.GetMultilingualString("a551626a-bcb6-4d23-8313-7698111e73b7", "Allow Automatic Un-marking of CCRs"),
						ResString.GetMultilingualString("57f1768a-8f63-446e-981a-1a8f8adee6cb", "Specifies whether the BMC service task can un-mark resources as Designated CCRs when they are detected as no longer being persistently overloaded."),
						RegistryStorageFlags.System,
						defaultValue: false);
				});
			}
		}

		#endregion

		#region AllowAsynchronousModuleGridFilterQueries

		public BooleanRegistryItem AllowAsynchronousModuleGridSearch
		{
			get
			{
				return GetItem("AllowAsynchronousModuleGridSearch", () =>
				{
					return new BooleanRegistryItem(
						"AllowAsynchronousModuleGridSearch",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("26b0cbc6-80b8-4656-87fb-75922a7d9551", "Allow Asynchronous Module Grid Search"),
						ResString.GetMultilingualString("697735ec-6dca-42e1-9f0b-28f3b3cb3ae4", "When enabled, supporting module grids will perform searches in parallel to improve board load speed."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Tags

		#region TagRuleChurnDetectionDepth

		public IntRegistryItem TagRuleChurnDetectionDepth
		{
			get
			{
				return GetItem("TagRuleChurnDetectionDepth", () =>
				{
					return new IntRegistryItem(
						"TagRuleChurnDetectionDepth",
						Categories.WorkflowManager_BufferManagement_Tags,
						ResString.GetMultilingualString("F28A3477-BFCD-4191-95E6-1D91A8E998E8", "Tag Rule Churn Detection Depth"),
						ResString.GetMultilingualString("9C21C0B0-2EC7-44CE-8F48-F3A3DD8FBA20", @"The Tag Rule Monitor service task determines if a tag rule is applying or removing a tag from an item too many times within a set time frame, and deactivates it if this is the case.
This defines the number of minutes into the past that will be considered when looking for fighting rules.
Tag Churn Detection Depth is now a support-only field for systems hosted on WiseCloud. If your system is hosted on WiseCloud and you believe that your detection depth should be changed, please create a CR9 service request for the PAVE (PAV) team explaining why you believe this is necessary, including details of the applicable tag configuration."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: 240,
						minValue: 60,
						maxValue: 480);
				});
			}
		}

		#endregion

		#region TagRuleChurnDetectionLimit

		public IntRegistryItem TagRuleChurnDetectionLimit
		{
			get
			{
				return GetItem("TagRuleChurnDetectionLimit", () =>
				{
					return new IntRegistryItem(
						"TagRuleChurnDetectionLimit",
						Categories.WorkflowManager_BufferManagement_Tags,
						ResString.GetMultilingualString("E4061302-9276-4FBA-92A6-44C6CCCDBAF8", "Tag Rule Churn Detection Limit"),
						ResString.GetMultilingualString("F937A5BB-D895-4592-BA22-CBE1BE3B785B", @"The Tag Rule Monitor service task determines if a tag rule is applying or removing a tag from an item too many times within a set time frame, and deactivates it if this is the case.
This defines the number of times something must be tagged or untagged by the same Tag Rule within the time period specified in the Tag Rule Churn Detection Depth registry item, before the Tag Rule Monitor service task will deactivate the Tag Rule.
Tag Churn Detection Limit is now a support-only field for systems hosted on WiseCloud. If your system is hosted on WiseCloud and you believe that your detection limit should be changed, please create a CR9 service request for the PAVE (PAV) team explaining why you believe this is necessary, including details of the applicable tag configuration."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: 3,
						minValue: 2,
						maxValue: 10);
				});
			}
		}

		#endregion

		#region TagRuleLogOutputLimit

		public IntRegistryItem TagRuleLogOutputLimit
		{
			get
			{
				return GetItem("TagRuleLogOutputLimit", () =>
				{
					return new IntRegistryItem(
						"TagRuleLogOutputLimit",
						Categories.WorkflowManager_BufferManagement_Tags,
						ResString.GetMultilingualString("EC650601-C5CB-43A6-B2AA-1A71FC8A7862", "Tag Rule Log Output Limit"),
						ResString.GetMultilingualString("8BA99899-4918-462D-9F2E-3F50511836E9", @"The Tag Rule Monitor service task reports which items caused tag fighting.
This defines the maximum amount of items to report regarding every set of fighting tag rules."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 3,
						minValue: 1,
						maxValue: 50);
				});
			}
		}

		#endregion

		#region TagRuleLogOutputLimit

		public GuidRegistryItem DefectTagPK
		{
			get
			{
				return GetItem("DefectTagPK", () =>
				{
					return new GuidRegistryItem(
						"DefectTagPK",
						Categories.WorkflowManager_BufferManagement_Tags,
						ResString.GetMultilingualString("d3993c3e-1002-4b3e-9bb6-17734137555e", "Defect Tag"),
						ResString.GetMultilingualString("9ea5544a-d5dd-4cee-8607-c67f1c516b01", "The Tag which signifies that an item is a defect fix."),
						RegistryStorageFlags.System,
						RegistryOptions.Default)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.TagMagnitude)
					};
				});
			}
		}

		#endregion

		#endregion

		#region Acceptability Bands

		#region Acceptability Band Server Cache Time Period

		public IntRegistryItem AcceptabilityBandServerCacheTimePeriod
		{
			get
			{
				return GetItem("AcceptabilityBandServerCacheTimePeriod", () =>
				{
					return new IntRegistryItem(
						"AcceptabilityBandServerCacheTimePeriod",
						Categories.WorkflowManager_BufferManagement_AcceptabilityBands,
						ResString.GetMultilingualString("9A362AAA-9BC8-4251-8255-ECEB228EB128", "Cache Time Period Centralized"),
						ResString.GetMultilingualString("680EE8BF-1712-4011-8CED-E20D028CB516", @"This Registry Value defines the number of seconds to store the results of Acceptability Band’s calculation on centralized cache for all users.
The time period is measured in seconds. A value of zero indicates no caching."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 10 * 60,
						minValue: 0,
						maxValue: 30 * 60);
				});
			}
		}

		#endregion

		#region Acceptability Band Client Cache Time Period

		public IntRegistryItem AcceptabilityBandClientCacheTimePeriod
		{
			get
			{
				return GetItem("AcceptabilityBandClientCacheTimePeriod", () =>
				{
					return new IntRegistryItem(
						"AcceptabilityBandClientCacheTimePeriod",
						Categories.WorkflowManager_BufferManagement_AcceptabilityBands,
						ResString.GetMultilingualString("DF4861C4-31C4-4012-9E71-C401A55D6448", "Cache Time Period Locally"),
						ResString.GetMultilingualString("D65E014D-5FDD-4B77-9AA7-8B9160253E5A", @"This Registry Value defines the number of seconds to store the results of Acceptability Band’s calculation on local cache for a single user.
The time period is measured in seconds. A value of zero indicates no caching."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 10 * 60,
						minValue: 0,
						maxValue: 30 * 60);
				});
			}
		}

		#endregion

		#region Acceptability Band Calculation Service Execution Timeout

		public IntRegistryItem AcceptabilityBandCalculationServiceExecutionTimeout
		{
			get
			{
				return GetItem("AcceptabilityBandCalculationServiceExecutionTimeout", () =>
				{
					return new IntRegistryItem(
						"AcceptabilityBandCalculationServiceExecutionTimeout",
						Categories.WorkflowManager_BufferManagement_AcceptabilityBands,
						ResString.GetMultilingualString("22CFA08C-F450-4D73-B69B-A3E80863B69E", "Calculation Execution Timeout on Web Service"),
						ResString.GetMultilingualString("004D684E-D761-43F4-AF35-DD7E2E82F4A5", $@"This Registry Value defines the timeout for executing the SQL query that calculates the Acceptability Band’s Value on the Web.
The timeout is measured in seconds. A value of zero indicates the default of the current system SQL timeout."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 30,
						minValue: 0,
						maxValue: 10 * 60);
				});
			}
		}

		#endregion

		#region Acceptability Band Local Calculation Execution Timeout

		public IntRegistryItem AcceptabilityBandLocalCalculationExecutionTimeout
		{
			get
			{
				return GetItem("AcceptabilityBandLocalCalculationExecutionTimeout", () =>
				{
					return new IntRegistryItem(
						"AcceptabilityBandLocalCalculationExecutionTimeout",
						Categories.WorkflowManager_BufferManagement_AcceptabilityBands,
						ResString.GetMultilingualString("2F4F60F0-CFE6-457F-BDB0-191118608AC6", "Calculation Execution Timeout on Local System"),
						ResString.GetMultilingualString("02C517F7-F79D-4737-8FDB-F35AC3AA5390", $@"This Registry Value defines the timeout for executing the SQL query that calculates the Acceptability Band’s Value locally.
The timeout is measured in seconds. A value of zero indicates the default of the current system SQL timeout."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 30,
						minValue: 0,
						maxValue: 10 * 60);
				});
			}
		}

		#endregion

		#region Acceptability Band Local Calculation Execution Timeout

		public StringRegistryItem AcceptabilityBandForTimeRecording
		{
			get
			{
				return GetItem("AcceptabilityBandForTimeRecording", () =>
				{
					return new StringRegistryItem(
						name: "AcceptabilityBandForTimeRecording",
						category: Categories.WorkflowManager_BufferManagement_AcceptabilityBands,
						caption: ResString.GetMultilingualString("73F001E1-2230-432C-A287-17E855A99346", "Name of the Acceptability Band Used for Time Recording"),
						hint: ResString.GetMultilingualString("36A10884-60B6-42A2-B573-8CAAD21DE59F", "This Registry Value defines the name of the acceptability band used for time recording (required by WAVE)."),
						storage: RegistryStorageFlags.System,
						options: RegistryOptions.Default,
						defaultValue: (NoResString)"Operational Metric: Time Recorded - Last Week");
				});
			}
		}

		#endregion

		#endregion

		#region SystemSchematic

		#region WorkflowLoopingDetectionDepth

		public IntRegistryItem WorkflowLoopingDetectionDepth
		{
			get
			{
				return GetItem("WorkflowLoopingDetectionDepth", () =>
				{
					return new IntRegistryItem(
						"WorkflowLoopingDetectionDepth",
						Categories.WorkflowManager_BufferManagement_SystemSchematic,
						ResString.GetMultilingualString("CAE0B51E-47D0-4CE7-BB09-EB69148C7F4A", "Workflow Looping Detection Depth"),
						ResString.GetMultilingualString("0A51D605-9DD8-428A-BDA2-84CEBF35E585", @"The Schematic Transfer Loop Monitor service task determines if a workflow has been transferred between components too many times within a set time frame, and deactivates it if this is the case.
This defines the number of minutes into the past that will be considered when looking for looping workflows.
Workflow Looping Detection Depth is now a support-only field for systems hosted on WiseCloud. If your system is hosted on WiseCloud and you believe that your detection depth should be changed, please create a CR9 service request for the PAVE (PAV) team explaining why you believe this is necessary, including details of the applicable buffer management system configuration."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: 60 * 5,
						minValue: 60,
						maxValue: 60 * 10);
				});
			}
		}

		#endregion

		#region WorkflowLoopingDetectionLimit

		public IntRegistryItem WorkflowLoopingDetectionLimit
		{
			get
			{
				return GetItem("WorkflowLoopingDetectionLimit", () =>
				{
					return new IntRegistryItem(
						"WorkflowLoopingDetectionLimit",
						Categories.WorkflowManager_BufferManagement_SystemSchematic,
						ResString.GetMultilingualString("BD1FC116-D8E8-4CE0-AA2C-B5D8A9AB1EEA", "Workflow Looping Detection Limit"),
						ResString.GetMultilingualString("3C4EED2D-5E23-4858-82D5-A6AD61C6DBBB", @"The Schematic Transfer Loop Monitor service task determines if a workflow has been transferred between components too many times within a set time frame, and deactivates it if this is the case.
This defines the number of times a workflow must be moved between components within the time period specified in the Workflow Looping Detection Depth registry item, before the Schematic Transfer Loop Monitor service task will deactivate the workflow.
Workflow Looping Detection Limit is now a support-only field for systems hosted on WiseCloud. If your system is hosted on WiseCloud and you believe that your detection limit should be changed, please create a CR9 service request for the PAVE (PAV) team explaining why you believe this is necessary, including details of the applicable buffer management system configuration."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: 3,
						minValue: 2,
						maxValue: 10);
				});
			}
		}

		#endregion

		#endregion

		#region Threaded Query Slowness Threshold Factor

		public DecimalRegistryItem ThreadedQuerySlownessThresholdFactor
		{
			get
			{
				return GetItem("ThreadedQuerySlownessThresholdFactor", () =>
				{
					return new DecimalRegistryItem(
						"ThreadedQuerySlownessThresholdFactor",
						Categories.WorkflowManager_BufferManagement_Tags,
						ResString.GetMultilingualString("8408ee6a-15ac-445e-a9d5-40049b11234d", "Threaded Query Slowness Threshold Factor"),
						ResString.GetMultilingualString("74960cc9-3af2-4da5-8fc2-5443d7efcb25", "When checking tag rule performance, this factor is used to determine whether or not the tag rule should be parallelized."),
						new NumericRegistryEditorInfo(1),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						defaultValue: 1.5m,
						lowerBound: 1.0,
						upperBound: 5.0);
				});
			}
		}

		#endregion

		#region Tag Rule Performance Check Frequency

		public IntRegistryItem TagRulePerformanceCheckFrequency
		{
			get
			{
				return GetItem("TagRulePerformanceCheckFrequency", () =>
				{
					return new IntRegistryItem(
					 "TagRulePerformanceCheckFrequency",
					 Categories.WorkflowManager_BufferManagement_Tags,
					 ResString.GetMultilingualString("90d9c17c-e846-49da-804f-fe73e7aec5e5", "Tag Rule Performance Checking Frequency"),
					 ResString.GetMultilingualString("9cd86f0b-7d60-4004-9ea1-914a844bf938", "The number of days that elapse between checking made by the Tag Rule Runner to see if tag rules should be parallelized or not. To disable checking, enter 0."),
					 new NumericRegistryEditorInfo(0),
					 RegistryStorageFlags.System,
					 RegistryOptions.IsOnlyForController,
					 defaultValue: 7,
					 minValue: 0,
					 maxValue: 28);
				});
			}
		}
		#endregion

		#region Consecutive Tag Rule Timeouts Before Deactivation
		public IntRegistryItem ConsecutiveTagRuleTimeoutsBeforeDeactivation
		{
			get
			{
				return GetItem("ConsecutiveTagRuleTimeoutsBeforeDeactivation", () =>
				{
					return new IntRegistryItem(
					 "ConsecutiveTagRuleTimeoutsBeforeDeactivation",
					 Categories.WorkflowManager_BufferManagement_Tags,
					 ResString.GetMultilingualString("374b0c63-f4e6-4647-a0e0-f042291123c5", "Number Of Consecutive Tag Rule Timeouts Before Deactivation"),
					 ResString.GetMultilingualString("54dc5412-78e3-4803-b9c3-b0a6273f6d3c", "The number of times a tag rule can consecutively fail due to timing out before the Tag Rule Runner deactivates it - preventing it from running any longer."),
					 new NumericRegistryEditorInfo(0),
					 RegistryStorageFlags.System,
					 RegistryOptions.IsOnlyForController,
					 defaultValue: 5,
					 minValue: 3,
					 maxValue: 10);
				});
			}
		}
		#endregion

		#region Network Diagram

		public BooleanRegistryItem NCNRibbonEnabled
		{
			get
			{
				return GetItem("NCNRibbon_Enabled", delegate
				{
					return new BooleanRegistryItem(
						"NCNRibbon_Enabled",
						Categories.WorkflowManager_NetworkDiagrams,
						(NoResString)"NCN Ribbon Enabled", // Non localised registry item
						(NoResString)"Specify whether NCN Ribbon is enabled and visible while working with network diagrams.", // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						true);
				});
			}
		}

		#endregion

		#region Allow Company Filters in Tag Rules

		public BooleanRegistryItem AllowCompanyFiltersInTagRules
		{
			get
			{
				return GetItem("AllowCompanyFiltersInTagRules", () =>
				{
					return new BooleanRegistryItem(
						"AllowCompanyFiltersInTagRules",
						Categories.WorkflowManager_BufferManagement_Tags,
						ResString.GetMultilingualString("FBFC8459-F2DF-42A8-A273-042DBEE4B3A2", "Allow Company Filters in Tag Rules"),
						ResString.GetMultilingualString("636EE8D3-25FB-448B-893C-D5094C1DAB6C", "When enabled, workflows may be excluded from being tagged based on the company of the Branch selected on the Tag Rule. For example, workflows belonging to Customs Declarations may not be tagged if the declaration was created in a different company to the one the Tag Rule's Branch belongs to. When this registry item is set to No, this company filtering will not be applied, so more workflows may be tagged than are shown in the Tag Rule's Preview popup. When set to Yes, this filtering will be applied."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Use Glow Indexing For Tag Rule Filters

		public BooleanRegistryItem UseGlowIndexingForTagRuleFilters
		{
			get
			{
				return GetItem("UseGlowIndexingForTagRuleFilters", () =>
					new BooleanRegistryItem(
						"UseGlowIndexingForTagRuleFilters",
						Categories.WorkflowManager_BufferManagement_Tags,
						ResString.GetMultilingualString("e54610d0-50db-4940-b22d-62ddd32aed1c", "Use GLOW Indexing For Tag Rule Filters"),
						ResString.GetMultilingualString("811dc610-b8c9-4710-91df-16c81f2560a6", "Use GLOW indexing for tag rule filters"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						false
					));
			}
		}

		#endregion

		#region Notification

		#region NotificationGroup

		public GuidRegistryItem NotificationGroup
		{
			get
			{
				return GetItem("BMS" + nameof(NotificationGroup), delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"BMS" + nameof(NotificationGroup),
						Categories.WorkflowManager_BufferManagement_Notification,
						ResString.GetMultilingualString("3ab8d610-9a14-4ece-9a5d-4a75f0e1ef8d", "System Notification Group"),
						ResString.GetMultilingualString("98558f88-bae7-47c9-991e-93780d1af7cb", "The staff group that will be notified about issues relating to Buffer Management Systems."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.Default,
						Core.Constants.Groups.PostMastersGroupPK
					)
					{
						EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup)
					};
					return result;
				});
			}
		}

		#endregion

		#region BMSNotificationPeriod

		public IntRegistryItem NotificationPeriod
		{
			get
			{
				return GetItem("BMS" + nameof(NotificationPeriod), () =>
				{
					return new IntRegistryItem(
						"BMS" + nameof(NotificationPeriod),
						Categories.WorkflowManager_BufferManagement_Notification,
						ResString.GetMultilingualString("D2D0444F-1185-43C5-9770-B16DF7A11FB8", "System Notification Period"),
						ResString.GetMultilingualString("057E0A65-B7F0-4B9F-846E-4669DB84395C", @"This defines the time period in minutes to accumulate errors of the same kind before they will be sent to the Buffer Management System Notification Group in a form of a digest notification email.
Errors of a kind which have been registered a long time ago will be ignored. 
Entering a value of zero means immediate notification without accumulating."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 60,
						minValue: 0,
						maxValue: 48 * 60);
				});
			}
		}

		#endregion

		#region Threshold
		public IntRegistryItem NotificationThreshold
		{
			get
			{
				return GetItem("BMS" + nameof(NotificationThreshold), () =>
				{
					return new IntRegistryItem(
						"BMS" + nameof(NotificationThreshold),
						Categories.WorkflowManager_BufferManagement_Notification,
						ResString.GetMultilingualString("EABF2DE7-DBE7-46F5-A4FF-A7B8521B198E", "System Notification Threshold"),
						ResString.GetMultilingualString("0BD1DA97-079E-4853-9A36-ECD74AF40F46", @"This defines how many times an error must occur before the system sends a notification email.
If the number of accumulated errors is lower than this value when the notification period elapses, then a notification email will not be reported. 
If the error occurred a long time ago it will be ignored. Entering a value 1 means email notifications will be sent immediately without accumulating."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 30,
						minValue: 1,
						maxValue: int.MaxValue);
				});
			}
		}

		#endregion

		#endregion

		#region Secondary Server

		public BooleanRegistryItem UseSecondaryServerForTransferRules
		{
			get
			{
				return GetItem("UseSecondaryServerForTransferRules", () =>
					new BooleanRegistryItem(
						"UseSecondaryServerForTransferRules",
						Categories.WorkflowManager_BufferManagement_ServerPerformance,
						ResString.GetMultilingualString("11345cb0-4e2c-4f33-8a2f-10b6fb5903dc", "Use Secondary Server for Transfer Rules"),
						ResString.GetMultilingualString("6bee76ca-5767-41d0-886e-23cdd374ab4f", "When enabled, transfer rules will be executed on a secondary database server, if one is configured."),
						RegistryStorageFlags.System,
						EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForController,
						defaultValue: false)
				);
			}
		}

		#endregion

		#region AlwaysViewWorkflowManagementTab

		public BooleanRegistryItem AlwaysViewWorkflowManagementTab
		{
			get
			{
				return GetItem("AlwaysViewWorkflowManagementTab", () =>
				{
					return new BooleanRegistryItem(
						"AlwaysViewWorkflowManagementTab",
						Categories.WorkflowManager,
						ResString.GetMultilingualString("EBDD759C-AFFB-43CD-9FF5-E95C10155ADB", "Always View Workflow Management Tab"),
						ResString.GetMultilingualString("9695D2E5-F32F-4366-8AD8-C3BBE65BC7FC", "When enabled, Workflow Management tab is visible."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region Validation

		public BooleanRegistryItem AutomaticallyValidateWorkflowLoopsOnSave
		{
			get
			{
				return GetItem("AutomaticallyValidateWorkflowLoopsOnSave", () =>
				{
					return new BooleanRegistryItem(
						"AutomaticallyValidateWorkflowLoopsOnSave",
						Categories.WorkflowManager_BufferManagement,
						ResString.GetMultilingualString("AFBB0111-F340-4A80-B68C-0BF141AC690B", "Automatically Validate Workflow Loops on Save"),
						ResString.GetMultilingualString("7FD4D4AB-837F-4383-86BA-A1DA64B66BD2", "When enabled, validation to check for workflow dependency and logical loops will occur every time a workflow is saved. This can be slow for large workflow networks, even if the workflow doesn't appear on a network diagram. When disabled, this validation will not occur on save. It can be run manually using the 'Validate Workflow Loops' action on a Network Diagram or from the context menu of the Workflows grid in Workflow & Tracking -> Management."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		#region RELEASE SEQUENCES

		#region Release Sequences Module Enabled

		public BooleanRegistryItem ReleaseSequencesModuleEnabled
		{
			get
			{
				return GetItem("ReleaseSequencesModuleEnabled", delegate
				{
					var registryItem = new BooleanRegistryItem(
						"ReleaseSequencesModuleEnabled",
						Categories.WorkflowManager_BufferManagement_ReleaseSequences,
						(NoResString)"Release Sequences Module Enabled", // Non localised registry item
						(NoResString)"Specify whether Release Sequences Module is enabled.", // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false
					);

					return registryItem;
				});
			}
		}

		#endregion

		#region Release Sequence Default Nudge

		public IntRegistryItem ReleaseSequenceDefaultNudge
		{
			get
			{
				return GetItem("ReleaseSequenceDefaultNudge", delegate
				{
					var registryItem = new IntRegistryItem(
						"ReleaseSequenceDefaultNudge",
						Categories.WorkflowManager_BufferManagement_ReleaseSequences,
						(NoResString)"Release Sequence Default Nudge", // Non localised registry item
						(NoResString)"Specify Release Sequence Default Nudge.", // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: 1000
					);

					return registryItem;
				});
			}
		}

		#endregion

		#endregion

		#region Responsive PAVE Data Processing

		#region EnableSynchronousPAVEDataProcessing

		public BooleanRegistryItem EnableSynchronousPAVEDataProcessing
		{
			get
			{
				return GetItem("EnableSynchronousPAVEDataProcessing", delegate
				{
					var registryItem = new BooleanRegistryItem(
						"EnableSynchronousPAVEDataProcessing",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("81B0F60A-E845-4DC0-A3E6-29D671E8CE90", "Enable Synchronous PAVE Data Processing"),
						ResString.GetMultilingualString("02BE0028-1734-4EFB-AE3C-AAF21CF1507A", "Specify whether workflows should be transferred synchronously when saving."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						false);

					return registryItem;
				});
			}
		}

		#endregion

		#region EnableResponsivePAVEDataProcessing

		public BooleanRegistryItem EnableResponsivePAVEDataProcessing
		{
			get
			{
				return GetItem("EnableResponsivePAVEDataProcessing", delegate
				{
					var registryItem = new BooleanRegistryItem(
						"EnableResponsivePAVEDataProcessing",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("B11CDFFA-BD8F-4E37-9882-05F416D4EE0A", "Enable Responsive PAVE Data Processing"),
						ResString.GetMultilingualString("8D22B13A-C599-45CF-A837-69801284F085", "Specify whether to enable responsive PAVE data processing (such as responsive workflow transfer and alike)."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);

					return registryItem;
				});
			}
		}

		#endregion

		#region TRANSFER

		#region DynamicallyFilterTransferRules

		public BooleanRegistryItem DynamicallyFilterTransferRules
		{
			get
			{
				return GetItem("DynamicallyFilterTransferRules", () =>
				{
					return new BooleanRegistryItem(
						"DynamicallyFilterTransferRules",
						new[] { Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing },
						ResString.GetMultilingualString("69C1DD4E-CAB6-4B39-8FD6-A73EB7CEC4FE", "Dynamically filter transfer rules for BMS service Task"),
						ResString.GetMultilingualString("A717F63B-DE86-443F-BC80-D183E17CA634", @"When enabled transfer rules will be dynamically filtered and workflow related filters won't be run through the BMS service task. Only through Responsive Data Processing.
When disabled the BMS service task will not dynamically filter which Transfer Rules to run and will instead run all transfer rules. This will increase server resource consumption."),
						new DynamicallyFilterTransferRulesRegistryEditorInfo(),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
				});
			}
		}

		public BooleanRegistryItem ProcessAllTransferRulesLinksOnNextBMSRun
		{
			get
			{
				return GetItem("ProcessAllTransferRulesLinksOnNextBMSRun", () =>
					new BooleanRegistryItem(
						"ProcessAllTransferRulesLinksOnNextBMSRun",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						(NoResString)"ProcessAllTransferRulesLinksOnNextBMSRun", // Hidden registry item
						(NoResString)"Ignore filtering and process all transfer rules next time BMS runs.", // Hidden registry item
						RegistryStorageFlags.System,
						options: RegistryOptions.IsHidden | RegistryOptions.NotCached,
						false)
				);
			}
		}

		#endregion

		#region TransferWorkflowComponentOnChanges

		public BooleanRegistryItem TransferWorkflowComponentOnChanges
		{
			get
			{
				return GetItem("TransferWorkflowComponentOnChanges", () =>
				{
					return new BooleanRegistryItem(
						"TransferWorkflowComponentOnChanges",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("44993043-47F9-4CFF-94FE-7FA3400774E0", "Transfer Workflow Component On Changes"),
						ResString.GetMultilingualString("214845C1-D393-42BE-86BA-7CC0F6AB7918", @"When enabled, the system will track changes in workflows and transfer workflows between components immediately."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
				});
			}
		}

		#endregion

		#region TransferWorkflowComponentMaximumNumberOfCDCChanges

		public IntRegistryItem TransferWorkflowComponentMaximumNumberOfCDCChanges
		{
			get
			{
				return GetItem("TransferWorkflowComponentMaximumNumberOfCDCChanges", () =>
				{
					return new IntRegistryItem(
						"TransferWorkflowComponentMaximumNumberOfCDCChanges",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("F21170CC-C48A-4410-A63B-5A8F8BFC601C", "Transfer Workflow Component Maximum Number of Changes to Process through BI CDC"),
						ResString.GetMultilingualString("8D68B5C5-297A-4B39-96D6-009C2478CF06", @"Specifies a maximum number of changes to processed through CDC for workflow transfer. If the number of changes is greater than this value the changes will be ignored and will be processed through the Buffer Management Schematic Transfer Runner service task instead.

Please, before changing it, contact the PAVE team for a better evaluation."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						1000,
						10,
						int.MaxValue);
				});
			}
		}

		#endregion

		#region ResetDedicatedBufferOnWorkflowsNotMatchingComponentLinks

		public BooleanRegistryItem ResetDedicatedBufferOnWorkflowsNotMatchingComponentLinks
		{
			get
			{
				return GetItem("ResetDedicatedBufferOnWorkflowsNotMatchingComponentLinks", () =>
				{
					return new BooleanRegistryItem(
						"ResetDedicatedBufferOnWorkflowsNotMatchingComponentLinks",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("37B18DAD-966C-485E-AC65-0F9DF2781C74", "Reset Dedicated Buffer On Workflows Not Matching Component Links"),
						ResString.GetMultilingualString("410B4797-7BB0-4301-AC32-30B69BE17053", @"When enabled, the PVE service task will reset dedicated buffer on active workflows not matching component links during responsive transfer."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
				});
			}
		}

		#endregion

		#region EnableNudgingBMSServiceTaskByThePVEServiceTask

		public BooleanRegistryItem EnableNudgingBMSServiceTaskByThePVEServiceTask
		{
			get
			{
				return GetItem("EnableNudgingBMSServiceTaskByThePVEServiceTask", () =>
				{
					return new BooleanRegistryItem(
						"EnableNudgingBMSServiceTaskByThePVEServiceTask",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("2BA8E4D2-6121-4D83-B97A-68FC8AB56B9F", "Enable Nudging BMS Service Task By The PVE Service Task"),
						ResString.GetMultilingualString("4A870822-7782-4C02-BEDF-48876F5169A6", @"When enabled, the PVE service task will nudge the {0} service task when encountering too many changes that require responsive transfer to process. The maximum number of changes to process is defined by the Transfer Workflow Component Maximum Number of Changes to Process through BI CDC registry item.", TransferRuleRunnerServiceTask.Code),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						false);
				});
			}
		}

		#endregion

		#region DelayForNudgingTheBMSServiceTaskByThePVEServiceTask

		public IntRegistryItem DelayForNudgingTheBMSServiceTaskByThePVEServiceTask
		{
			get
			{
				return GetItem("DelayForNudgingTheBMSServiceTaskByThePVEServiceTask", () =>
				{
					return new IntRegistryItem(
						"DelayForNudgingTheBMSServiceTaskByThePVEServiceTask",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("2DEE4823-7697-4C53-BBD4-7494B4AA9876", "Delay For Nudging The BMS Service Task By The PVE Service Task"),
						ResString.GetMultilingualString("AB24E2D0-A240-423B-BC91-E5F2FC83EF0C", "The delay in minutes for nudging the {0} service task when the PVE service task encounters too many changes that require responsive transfer to process. A value of zero indicates immediate nudging with no delay.", TransferRuleRunnerServiceTask.Code),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 3,
						minValue: 0,
						maxValue: 5);
				});
			}
		}

		#endregion

		#endregion

		#region CAPABILITY TASK AUTO ASSIGNMENT

		#region AutoAssignCapabilityTasksOnChanges

		public BooleanRegistryItem AutoAssignCapabilityTasksOnChanges
		{
			get
			{
				return GetItem("AutoAssignCapabilityTasksOnChanges", () =>
				{
					return new BooleanRegistryItem(
						"AutoAssignCapabilityTasksOnChanges",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("AutoAssignCapabilityTasksOnChangesCaption", "Auto Assign Capability Tasks On Changes"),
						ResString.GetMultilingualString("AutoAssignCapabilityTasksOnChangesHint", @"When enabled, the system will track changes in workflows and auto assign capability tasks immediately if needed (or schedule the auto assignment for later if it is too early for auto assignment)."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
				});
			}
		}

		#endregion

		#region AutoAssignCapabilityTasksMaxNumberOfCDCChanges

		public IntRegistryItem AutoAssignCapabilityTasksMaxNumberOfCDCChanges
		{
			get
			{
				return GetItem("AutoAssignCapabilityTasksMaxNumberOfCDCChanges", () =>
				{
					return new IntRegistryItem(
						"AutoAssignCapabilityTasksMaxNumberOfCDCChanges",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("AutoAssignCapabilityTasksMaxNumberOfCDCChangesCaption", "Auto Assign Capability Tasks Maximum Number of Changes to Process through CDC"),
						ResString.GetMultilingualString("AutoAssignCapabilityTasksMaxNumberOfCDCChangesHint", @"Specifies a maximum number of changes to be processed through CDC for capability task auto assignment. If the number of changes is greater than this value the changes will be ignored and will be processed through the Capability Task Auto-Assignment service task instead.

Please, before changing it, contact the PAVE team for a better evaluation."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						1000,
						10,
						int.MaxValue);
				});
			}
		}

		#endregion

		#region EnableNudgingBMTServiceTaskByThePVEServiceTask

		public BooleanRegistryItem EnableNudgingBMTServiceTaskByThePVEServiceTask
		{
			get
			{
				return GetItem("EnableNudgingBMTServiceTaskByThePVEServiceTask", () =>
				{
					return new BooleanRegistryItem(
						"EnableNudgingBMTServiceTaskByThePVEServiceTask",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("9B5FB7F2-3629-48C4-A346-097F9F277024", "Enable Nudging BMT Service Task By The PVE Service Task"),
						ResString.GetMultilingualString("96D5F67E-AE10-4164-93E3-3935D7AFA7F5", @"When enabled, the PVE service task will nudge the {0} service task when encountering too many changes that require responsive capability task auto-assignment to process. The maximum number of changes to process is defined by the Auto Assign Capability Tasks Maximum Number of Changes to Process through CDC registry item.", CapabilityTaskAutoAssignmentServiceTask.Code),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						false);
				});
			}
		}

		#endregion

		#region DelayForNudgingTheBMTServiceTaskByThePVEServiceTask

		public IntRegistryItem DelayForNudgingTheBMTServiceTaskByThePVEServiceTask
		{
			get
			{
				return GetItem("DelayForNudgingTheBMTServiceTaskByThePVEServiceTask", () =>
				{
					return new IntRegistryItem(
						"DelayForNudgingTheBMTServiceTaskByThePVEServiceTask",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("2B9DE26D-42E9-458F-B7B7-9A479EADC608", "Delay For Nudging The BMT Service Task By The PVE Service Task"),
						ResString.GetMultilingualString("A9D460C7-CB0A-4332-BA05-028C9BB383A5", "The delay in minutes for nudging the {0} service task when the PVE service task encounters too many changes that require responsive capability task auto-assignment to process. A value of zero indicates immediate nudging with no delay.", CapabilityTaskAutoAssignmentServiceTask.Code),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: 3,
						minValue: 0,
						maxValue: 5);
				});
			}
		}

		#endregion

		#endregion

		#region RESPONSIVE WORKFLOW UPDATES ON RELATED OBJECT CHANGES

		#region EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges

		public BooleanRegistryItem EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges
		{
			get
			{
				return GetItem("EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges", () =>
				{
					return new BooleanRegistryItem(
						"EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("8360DDDE-F55D-40F4-82FD-339EAE361F75", "Enable Responsive Workflow Updates On Related Object Changes"),
						ResString.GetMultilingualString("606E624A-6CDB-4A5F-868E-1FA15D51166F", "When enabled, the system will responsively update dependent properties on workflows in response to changes in related objects (such as buffer management systems, tags groups, etc.)"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
				});
			}
		}

		#region DelayForResponsiveWorkflowUpdatesOnRelatedObjectChanges

		public IntRegistryItem DelayForResponsiveWorkflowUpdatesOnRelatedObjectChanges
		{
			get
			{
				return GetItem("DelayForResponsiveWorkflowUpdatesOnRelatedObjectChanges", () =>
				{
					return new IntRegistryItem(
						"DelayForResponsiveWorkflowUpdatesOnRelatedObjectChanges",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("2EBA9CEA-0DBE-4612-85A2-3E90894E3602", "Delay For Responsive Workflow Updates On Related Object Changes"),
						ResString.GetMultilingualString("ACA28D09-D544-42F8-9B6E-968A1C7DB09E", "The delay in minutes for responsive updates of dependent properties on workflows in response to changes in related objects (such as buffer management systems, tags groups, etc.). A value of zero indicates immediate updates with no delay."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: 1,
						minValue: 0,
						maxValue: 5);
				});
			}
		}

		#endregion

		#endregion

		#region ResponsiveWorkflowUpdatesBatchSize

		public IntRegistryItem ResponsiveWorkflowUpdatesBatchSize
		{
			get
			{
				return GetItem("ResponsiveWorkflowUpdatesBatchSize", () =>
				{
					return new IntRegistryItem(
						"ResponsiveWorkflowUpdatesBatchSize",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("0EEEC90C-CBFF-4A8C-9626-F77F0BD90539", "Responsive Workflow Updates Batch Size"),
						ResString.GetMultilingualString("065C6310-8343-4261-AAF7-B367D9657FB3", "The number of workflows to process in one batch when updating workflows in response to changes in related objects."),
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION,
						minValue: 10,
						maxValue: ZSQLInFilter.MAXIMUM_ELEMENTS_FOR_PARAMETERISATION);
				});
			}
		}

		#endregion

		#region LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges

		public BooleanRegistryItem LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges
		{
			get
			{
				return GetItem("LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges", () =>
				{
					return new BooleanRegistryItem(
						"LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("8DF49AFA-3F9D-468E-AEA2-26CE699D97DD", "Log Workflow Info On Responsive Workflow Updates On Related Object Changes"),
						ResString.GetMultilingualString("0B37A687-BAB5-4430-9CCD-85348B9C7E67", "When enabled, the description and job number of workflows will be logged to the PVE and TAS service task logs as they are responsively updated on related object changes (such as buffer management system configuration changes, workflow tag application, etc)."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: false);
				});
			}
		}

		#endregion

		#endregion

		#region RESPONSIVE RELEASE GATE

		#region ReleaseWorkflowsOnChanges

		public BooleanRegistryItem ReleaseWorkflowsOnChanges
		{
			get
			{
				return GetItem("ReleaseWorkflowsOnChanges", () =>
				{
					return new BooleanRegistryItem(
						"ReleaseWorkflowsOnChanges",
						Categories.WorkflowManager_BufferManagement_ResponsivePAVEDataProcessing,
						ResString.GetMultilingualString("ADFEA300-4CC1-4AA3-939A-6DA2C081B1EF", "Release Workflows On Changes"),
						ResString.GetMultilingualString("13D03E96-7759-44DE-A855-D4E80B967059", @"When enabled, the system will track changes in workflows and release eligible workflows immediately."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);
				});
			}
		}

		#endregion

		#endregion

		#endregion

		#region EXPERIMENTAL

		#region EnablePaveExperimentalFeatures

		public BooleanRegistryItem EnablePaveExperimentalFeatures
		{
			get
			{
				return GetItem("EnablePaveExperimentalFeatures", delegate
				{
					var registryItem = new BooleanRegistryItem(
						"EnablePaveExperimentalFeatures",
						Categories.WorkflowManager_BufferManagement_Experimental,
						(NoResString)"Enable PAVE Experimental Features (Global)", // Non localised registry item
						(NoResString)@"Specify whether to enable the new PAVE experimental features. This is a global setting that affects the ability to specify PAVE experimental settings per individual BM System / Board.

Please contact the PAVE team before enabling this.", // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);

					return registryItem;
				});
			}
		}

		#endregion

		#region EnableSimpleCapacityCalculation

		public BooleanRegistryItem EnableSimpleCapacityCalculationForAllSystems
		{
			get
			{
				return GetItem("EnableSimpleCapacityCalculation", delegate
				{
					var registryItem = new BooleanRegistryItem(
						"EnableSimpleCapacityCalculation",
						Categories.WorkflowManager_BufferManagement_ReleaseGate,
						ResString.GetMultilingualString("EnableCapacityCalculatorSimpleQuery", "Enable Capacity Calculator Simple Query"),// Non localised registry item
						ResString.GetMultilingualString("SpecifyWhetherToEnableCapacityCalculatorsSimpleQueryForAllSystems", @"Specify whether to enable capacity calculator's simple query for all systems.
Please, before enabling it, contact the PAVE team for a better evaluation."), // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);

					return registryItem;
				});
			}
		}

		#endregion

		#region DisableZoneMultipliersForAllSystems

		public BooleanRegistryItem DisableZoneMultipliersForAllSystems
		{
			get
			{
				return GetItem("DisableZoneMultipliersForAllSystems", delegate
				{
					var registryItem = new BooleanRegistryItem(
						"DisableZoneMultipliersForAllSystems",
						Categories.WorkflowManager_BufferManagement_Experimental,
						(NoResString)"Disable Component Capacity Throttles", // Non localised registry item
						(NoResString)@"Specify whether to disable Component Capacity Throttles when calculating capacity for all systems.

Please, before enabling it, contact the PAVE team for a better evaluation.", // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);

					return registryItem;
				});
			}
		}

		#endregion

		#region AcceptabilityBandUseMENT

		public BooleanRegistryItem AcceptabilityBandUseMENT
		{
			get
			{
				return GetItem("AcceptabilityBandUseMENT", delegate
				{
					var registryItem = new BooleanRegistryItem(
						"AcceptabilityBandUseMENT",
						Categories.WorkflowManager_BufferManagement_Experimental,
						(NoResString)"Acceptability Band Calculation uses MENT", // Non localised registry item
						(NoResString)@"When enabled calculation of Acceptability Bands will try to get results from MENT tables.

Please contact the PAVE team before enabling this.", // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);

					return registryItem;
				});
			}
		}

		#endregion

		#region AcceptabilityBandUseMentExpirationMinutes

		public IntRegistryItem AcceptabilityBandUseMentExpirationMinutes
		{
			get
			{
				return GetItem("AcceptabilityBandUseMentExpirationMinutes", () =>
				{
					return new IntRegistryItem(
						"AcceptabilityBandUseMentExpirationMinutes",
						Categories.WorkflowManager_BufferManagement_Experimental,
						(NoResString)"Acceptability Band Use MENT Expiration Time in Minutes", // Non localised registry item
						(NoResString)@"When enabled only get results from MENT calculated X minutes ago.

Please contact the PAVE team before enabling this.", // Non localised registry item
						new NumericRegistryEditorInfo(0),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: 300,
						minValue: 30,
						maxValue: int.MaxValue);
				});
			}
		}

		#endregion

		#region IgnoreIterationsWhenCalculatingStartability

		public BooleanRegistryItem IgnoreIterationsWhenCalculatingStartability
		{
			get
			{
				return GetItem("IgnoreIterationsWhenCalculatingStartability", delegate
				{
					var registryItem = new BooleanRegistryItem(
						"IgnoreIterationsWhenCalculatingStartability",
						Categories.WorkflowManager_BufferManagement_Experimental,
						(NoResString)"Ignore iterations when calculating startability", // Non localised registry item
						(NoResString)"When enabled, tasks in the original workflow that has containment barrier task will become startable when the containment barrier task is closed, irrespective whether a QI sub-workflow is created or not. This will present startable tasks that belong to the original and iterated workflows.", // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);

					return registryItem;
				});
			}
		}

		#endregion

		#endregion

		#region TEMPORARY

		public StringArrayRegistryItem DebuggingStringsForProcessHeaderLinks
		{
			get
			{
				return GetItem("DebuggingStringsForProcessHeaderLinks", () =>
				{
					return new StringArrayRegistryItem(
						"DebuggingStringsForProcessHeaderLinks",
						Categories.WorkflowManager_BufferManagement,
						(NoResString)"Debugging Strings For Process Header Links", // Non localised registry item
						(NoResString)"Process Header Links From/To Descriptions for Debugging (temporary)", // Non localised registry item
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		#endregion
	}
}

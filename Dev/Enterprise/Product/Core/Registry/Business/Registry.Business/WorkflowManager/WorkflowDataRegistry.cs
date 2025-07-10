using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class WorkflowDataRegistry : RegistryItemSet, IWorkflowRegistry
	{
		WorkflowDataRegistry()
		{
		}

		#region Instance

		public static WorkflowDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new WorkflowDataRegistry();
				}

				return fInstance;
			}
		}
		[ThreadStatic]
		static WorkflowDataRegistry fInstance;

		#endregion

		public override bool IsForProductivityWise => true;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString WorkflowManager_Exceptions => CombineCategories(WorkflowManager, ResString.GetMultilingualString("85D2C524-81C6-43D3-8465-F137C8D911FD", "Exceptions"));

			public static MultilingualString WorkflowManager_ResourceCapabilities => CombineCategories(WorkflowManager, ResString.GetMultilingualString("BCE58FB8-3AD8-4797-8FD1-03AFE3FDF3A1", "Resource Capabilities"));

			public static MultilingualString WorkflowManager_TaskAssignment => CombineCategories(WorkflowManager, ResString.GetMultilingualString("0666B129-2780-4240-A9A2-26F611B45141", "Task Assignment"));

			public static MultilingualString WorkflowManager_WorkflowTemplates => CombineCategories(WorkflowManager, ResString.GetMultilingualString("833B67E6-6030-4E96-983D-6C8B2B156637", "Workflow Templates"));

			public static MultilingualString WorkflowManager_WorkflowTemplates_ValidationTools => CombineCategories(WorkflowManager_WorkflowTemplates, ResString.GetMultilingualString("1370BD47-4465-4D2C-813E-AE90673D8E8F", "Validation Tools"));
		}

		#endregion

		#region IWorkflowRegistry Members

		bool IWorkflowRegistry.AreUniversalTemplatesEnabled => EnableUniversalTemplates.Value;

		#endregion

		#region Quality Iterations

		public WorkflowIterationReasonsRegistryItem IterationReasons
		{
			get
			{
				MultilingualString hint = ResString.GetMultilingualString("1780029a-a5dd-4d19-afe3-813cdf83eb48", "The list of reasons that can be given for triggering a quality iteration.");

				return GetItem("ProcessManagerIterationReasons", delegate
				{
					return new WorkflowIterationReasonsRegistryItem(
						"ProcessManagerIterationReasons",
						RawDataRegistry.Categories.WorkflowManager_QualityIteration,
						ResString.GetMultilingualString("075a5eef-f0f7-4172-87f4-636722dd68d7", "Quality Iteration Reasons"),
						hint,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						CategorisedWorkflowIterationReasonsCollection.GetDefault());
				});
			}
		}

		public BooleanRegistryItem CreateNewWorkflowsForQualityIterationsByDefault
		{
			get
			{
				var isBufferManagementEnabled = ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled;
				var options = isBufferManagementEnabled ? RegistryOptions.Default : RegistryOptions.IsOnlyForSupport | RegistryOptions.IsReadOnly;
				var defaultValue = isBufferManagementEnabled; // cannot create workflows if BM isn't enabled. Note that this will just change to 'false' when feature is complete.
				var hint = ResString.GetMultilingualString("08f14aa6-45b5-4662-8b47-a9bdcd28e71e", "When enabled, Quality Iteration tasks will be created in a new child workflow of the Containment Barrier task's workflow by default. This setting can be overridden for each task type in the Task Types registry setting. Users may also be able to override this setting each time they create a Quality Iteration dependent on the 'Allow users to change iteration workflow creation options.' registry value. Automated processes that create Quality Iterations without user input will use this value to determine whether or not to create a new workflow for Quality Iterations.");
				return GetItem("CreateNewWorkflowsForQualityIterationsByDefault", () => new BooleanRegistryItem(
					"CreateNewWorkflowsForQualityIterationsByDefault",
					RawDataRegistry.Categories.WorkflowManager_QualityIteration,
					ResString.GetMultilingualString("8f700ab5-5bb8-4408-8819-727716afe4cc", "Create New Workflows For Quality Iterations By Default"),
					hint,
					RegistryStorageFlags.System,
					options,
					defaultValue));
			}
		}

		public BooleanRegistryItem TurnOnFunctionalityUnderDevelopment
		{
			get
			{
				return GetItem("TurnOnFunctionalityUnderDevelopment", () => new BooleanRegistryItem(
					"TurnOnFunctionalityUnderDevelopment",
					RawDataRegistry.Categories.WorkflowManager,
					(NoResString)"Functionality Under Development",
					(NoResString)"FOR UAT TEST PURPOSES ONLY: Turn on functionality that is currently under development",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false));
			}
		}

		public BooleanRegistryItem AllowUsersToChangeIterationWorkflowCreationOptions
		{
			get
			{
				return GetItem("AllowUsersToChangeIterationWorkflowCreationOptions", () => new BooleanRegistryItem(
					"AllowUsersToChangeIterationWorkflowCreationOptions",
					RawDataRegistry.Categories.WorkflowManager_QualityIteration,
					ResString.GetMultilingualString("b93b19b1-0ab9-45c2-9cb9-2120cf7c6f3d", "Allow users to change iteration workflow creation options."),
					ResString.GetMultilingualString("e49c1a08-7690-40aa-8769-214e0f9813da", "When enabled, a checkbox will appear on the Quality Iteration form that allows users to select whether or not they want the quality iteration to be created in a new workflow."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					false));
			}
		}

		public BooleanRegistryItem PrefillResourceUnderReview
		{
			get
			{
				MultilingualString hint = ResString.GetMultilingualString("861ACB57-C010-4CE1-9276-57994EDAD94E", "When possible, the system will attempt to pre-fill the User Under Review field on the Containment Barrier Outcome form. This will be the resource in preceding tasks or workflows which has recorded the largest amount of Actual Duration time. If disabled, the User Under Review field will be left blank (unless there is only one resource to choose from) when the Containment Barrier form is shown. Please note that the system will always attempt to pre-fill the field when automated processes close containment barrier tasks (Trigger Completion Actions, for example).");

				return GetItem("PrefillResourceUnderReview", delegate
				{
					return new BooleanRegistryItem(
						"PrefillResourceUnderReview",
						RawDataRegistry.Categories.WorkflowManager_QualityIteration,
						ResString.GetMultilingualString("1D5BCE76-524B-4F77-A832-10CDF0F541B7", "Pre-fill User Under Review"),
						hint,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem RequireResourceUnderReview
		{
			get
			{
				MultilingualString hint = ResString.GetMultilingualString("D2EABD54-0019-4E2D-A35F-0FE7D8E3E1D4", "When enabled, a value must be chosen in the User Under Review field on the Containment Barrier Outcome form.");

				return GetItem("RequireResourceUnderReview", delegate
				{
					return new BooleanRegistryItem(
						"RequireResourceUnderReview",
						RawDataRegistry.Categories.WorkflowManager_QualityIteration,
						ResString.GetMultilingualString("BCD089DB-BEDA-49A3-882B-5100F22243D9", "Require User Under Review"),
						hint,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);
				});
			}
		}

		#endregion

		#region Template Company Filtering

		public BooleanRegistryItem CalculateTemplateUsingCurrentCompany
		{
			get
			{
				MultilingualString hint = ResString.GetMultilingualString("7AFBBA86-98DE-49BB-9CFA-16ECFFC64F71", "Sets whether the system calculates appropriate company details from context, or from logged in user.");

				return GetItem("CalculateTemplateUsingCurrentCompany", delegate
				{
					return new BooleanRegistryItem(
						"CalculateTemplateUsingCurrentCompany",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("A618ADC0-8B5A-4075-995A-E4C8C90E67CC", "Calculate template using current company"),
						hint,
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Task Types

		public WorkflowTaskTypesRegistryItem TaskTypes
		{
			get
			{
				MultilingualString hint = ResString.GetMultilingualString("bf9c98b1-c5f8-4e12-b6da-1a8807905241", "This is the list of task types for Workflow Manager. You may select whether a task type creates an appointment with the assigned staff member. You may also specify whether a task type restricts user from closing tasks that are not assigned to them. It is also possible to link the task types to specific Resource Capabilities, for Buffer Management purposes. \'Allow Working Status Changes in Buckets\' works in conjunction with the \'Allow working on any task in any component\' security setting.");

				return GetItem("ProcessManagerTaskTypes", delegate
				{
					return new WorkflowTaskTypesRegistryItem(
						"ProcessManagerTaskTypes",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("32048411-ce7e-41a3-83ba-f917189c3464", "Task Types"),
						hint,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						CategorisedWorkflowTaskTypesCollection.GetDefault());
				});
			}
		}

		#endregion

		#region Assist With This Task

		public AssistWithThisTaskRegistryItem AssistWithThisTask
		{
			get
			{
				return GetItem("AssistWithThisTask", () => new AssistWithThisTaskRegistryItem(
					"AssistWithThisTask",
					RawDataRegistry.Categories.WorkflowManager,
					ResString.GetMultilingualString("bc63b1f0-c3c7-4b98-a576-b16f67aba1057", "Assist With This Task / Add Assistance Task For"),
					ResString.GetMultilingualString("57bf3f83-b0b0-49e3-8157-f9940c6c2f0c", "For each workflow type, a task type can be designated to be used when the 'Assist With This Task' or 'Add Assistance Task For' menu item is used on task grids and Visual Board task cards."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					CategorisedAssistWithThisTaskSettingCollection.GetDefault()));
			}
		}

		#endregion

		#region TaskAssignmentRestrictions

		public TaskAssignmentRestrictionsRegistryItem TaskAssignmentRestrictions
		{
			get
			{
				MultilingualString hint = ResString.GetMultilingualString("E99C3F07-9615-4437-9082-F4FCD157E75F", "Defines restrictions on whether the same or different resources should be assigned to tasks of the specified task types.");

				return GetItem("ProcessManagerTaskTypeRestrictions", delegate
				{
					return new TaskAssignmentRestrictionsRegistryItem(
						"ProcessManagerTaskTypeRestrictions",
						Categories.WorkflowManager_TaskAssignment,
						ResString.GetMultilingualString("E2D9BD63-DE3A-4D5B-81D0-7F56CEEA34E6", "Task Assignment Restrictions"),
						hint,
						RegistryStorageFlags.System | RegistryStorageFlags.Company);
				});
			}
		}

		#endregion

		#region ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment

		public BooleanRegistryItem ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment
		{
			get
			{
				return GetItem("ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment", () =>
				new BooleanRegistryItem(
					"ResetTaskPenetrationOnNewTaskAdditionAssignmentOrReassignment",
					Categories.WorkflowManager_TaskAssignment,
					ResString.GetMultilingualString("E296362A-6407-470A-A5AB-568B174BB3E8", "Reset Task Penetration on New Task Addition Assignment or Re-assignment"),
					ResString.GetMultilingualString("2D4A6DD8-EF57-48AE-BC97-8166388B8D66", @"When checked, tasks that are assigned or re-assigned to a resource after release to a buffer, will temporarily reset their buffer penetration to provide a brief reprieve for the resource.
Tasks when reset will appear visually earlier in the buffer and then move quickly until they are shown to be in the same position as the original workflow.
This feature is optional for inter and intra team task assignments and set up in the Buffer Management System Release Group settings."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					defaultValue: false));
			}
		}

		#endregion

		#region TaskAssignmentAutoAssign

		public BooleanRegistryItem TaskAssignmentAutoAssignStaff
		{
			get
			{
				return GetItem("TaskAssignmentAutoAssignStaff", delegate
				{
					var result = new BooleanRegistryItem("TaskAssignmentAutoAssignStaff",
					Categories.WorkflowManager_TaskAssignment,
					ResString.GetMultilingualString("91714885-35f8-4e99-a536-1141c6cf7d8c", "Auto Assign Staff or Group Code"),
					ResString.GetMultilingualString("07d4aea5-6e0e-4c65-bd30-805563fccadf", "By default, succeeding tasks that are created have the staff code or group code applied from the preceding task. Turning this setting off will leave the fields blank."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					defaultValue: true);

					return result;
				});
			}
		}

		#endregion

		#region Task DurationTrackingOutOfHoursLimit

		public IntRegistryItem TaskDurationTrackingOutOfHoursLimit
		{
			get
			{
				return GetItem("TaskDurationTrackingOutOfHoursLimit", () =>
				new IntRegistryItem(
					"TaskDurationTrackingOutOfHoursLimit",
					RawDataRegistry.Categories.WorkflowManager,
					ResString.GetMultilingualString("ba1b4cff-0660-448f-ac6e-7394686e59cd", "Task Duration Tracking Out Of Work Time Limit (in hours)"),
					ResString.GetMultilingualString("861bed2d-dbbb-4f7c-be49-595136df2194", "The number of hours before task duration ignores time outside of working hours, (when a task is started, suspend or closed outside of working hours)."),
					RegistryStorageFlags.System,
					10));
			}
		}

		#endregion

		#region TaskDefaultOpeningBehaviour

		public CodePairRegistryItem TaskDefaultOpeningBehaviour
		{
			get
			{
				return GetItem("TaskDefaultOpeningBehaviour", () =>
					new CodePairRegistryItem(
						"TaskDefaultOpeningBehaviour",
						Categories.WorkflowManager,
						ResString.GetMultilingualString("f2c972a6-a836-423d-b744-491eeb6f8a0b", "Task List Default Opening Behavior"),
						ResString.GetMultilingualString("d33646a4-c57a-4f27-a95c-31e5a5ef1bd2", "Determines which form is opened by default when double-clicking on a task in the Task List module."),
						new CodeDescriptionPairListProvider(() => new TaskDefaultOpeningBehaviourOptions()),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						TaskDefaultOpeningBehaviourOptions.Codes.Job));
			}
		}

		#endregion

		#region MaximumNumberOfWorkflowItemsInTemplateApplication

		public IntRegistryItem MaximumNumberOfWorkflowItemsInTemplateApplication
		{
			get
			{
				return GetItem("MaximumNumberOfWorkflowItemsInTemplateApplication", () =>
				new IntRegistryItem(
					"MaximumNumberOfWorkflowItemsInTemplateApplication",
					Categories.WorkflowManager_WorkflowTemplates,
					ResString.GetMultilingualString("e4534315-bef4-46ac-bfa3-b622e629bc2d", "Maximum Workflow Items from Template Application"),
					ResString.GetMultilingualString("8c028598-e32a-4ced-aff3-7f6ad2fa45e4", "If more template items apply, an error has occurred and should be reported."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyEditableBySupportIfHosted,
					500));
			}
		}

		#endregion

		#region RelatedWorkflowItemDisplayLimit

		public IntRegistryItem RelatedWorkflowItemDisplayLimit
		{
			get
			{
				return GetItem("RelatedWorkflowItemDisplayLimit", () =>
				new IntRegistryItem(
					"RelatedWorkflowItemDisplayLimit",
					RawDataRegistry.Categories.WorkflowManager,
					ResString.GetMultilingualString("08431d1b-907e-4125-89f5-0c3b63669bec", "Related Workflow Display Limit"),
					ResString.GetMultilingualString("0e4ee82c-7e5a-4f28-9921-62f416dc6fd4", "When a job has related jobs (such as a Shipments to Consolidations) then Tasks, Triggers, Milestones and Exceptions from those related jobs are displayed in the Workflow & Tracking tab. This registry item will stop related Tasks, Triggers, Milestones and Exceptions from being displayed when there are an excessive number of related jobs."),
					RegistryStorageFlags.System,
					60));
			}
		}

		#endregion

		#region Notification Group

		public GuidRegistryItem WorkflowManagerNotificationGroup
		{
			get
			{
				return GetItem("WorkflowManagerNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"WorkflowManagerNotificationGroup",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("f67aa7b3-1d7d-4e09-8525-2c5f292bff3a", "Workflow Manager Notification Group"),
						ResString.GetMultilingualString("d697da5b-eacf-459b-81ec-db767e96db2e", "The staff group that will be notified about Workflow Manager failures, warnings and other information."),
						RegistryStorageFlags.All,
						RegistryOptions.Default,
						RegistryFactory.Instance.GetGroupPK("ALL")
					);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		#endregion
		#region ClientInTemplateSelection

		public ClientInTemplateSelectionCriteriaCollectionRegistryItem ClientInTemplateSelection
		{
			get
			{
				ResourceString hint = ResString.GetMultilingualString("291c03ea-aff1-4c77-be63-8586628c656f", "Some Workflow Templates can be set for a specific \"Client\".\r\n\r\n"
					+ "Use this registry to match the \"Client\" on a specified Workflow Template Type to an organization on a job the template is to be applied for. For certain Template types the \"Client\" may be interpreted in several different roles – this registry allows you to set up the order in which organizations on the job should be matched to the \"Client\".\r\n\r\n"
					+ "For example, Shipment Workflow Template Type will try to match the \"Client\" field first to the \"Consignor/Consignee\" organization on a Shipment (depending on the job direction), if no Consignor (or Consignee) present, it will try to match \"Client\" to the \"Local Client\" organization, etc.");

				return GetItem("ClientInTemplateSelectionCriteria", delegate
				{
					ClientInTemplateSelectionCriteriaCollectionRegistryItem result = new ClientInTemplateSelectionCriteriaCollectionRegistryItem(
						"ClientInTemplateSelectionCriteria",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("0142bd68-05d6-4fca-ba1a-5f609c3c8d08", "Client in Template Selection Criteria"),
						hint,
						RegistryStorageFlags.System,
						ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue);

					return result;
				});
			}
		}

		#endregion

		#region EDICommunicationModeFallbackToOrganizationEmail

		public BooleanRegistryItem EDICommunicationModeFallbackToOrganizationEmail
		{
			get
			{
				return GetItem("EDICommunicationModeFallbackToOrganizationEmail", delegate
				{
					var result = new BooleanRegistryItem(
						"EDICommunicationModeFallbackToOrganizationEmail",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("3d8f8cfd-727a-4ff1-9ac6-524efea8ccb2", "EDI Communication mode fallback to organization email"),
						ResString.GetMultilingualString("b571632b-dbee-40b2-917c-b3cf436d8db6", "Select this option to enable fallback to available email address on organization if related EDI Communication mode is not configured. Fallback email address will be selected in following order: email of selected Contact, email of selected organization Address, email of main organization Address."),
						RegistryStorageFlags.System,
						false);

					return result;
				});
			}
		}

		#endregion

		#region EnableCheckpointsForWorkflowSecurity

		public BooleanRegistryItem EnableCheckpointsForWorkflowSecurity
		{
			get
			{
				return GetItem("EnableCheckpointsForWorkflowSecurity", delegate
				{
					var result = new BooleanRegistryItem(
						"EnableCheckpointsForWorkflowSecurity",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("40cbfb24-720b-4744-b4b9-9395b1680c48", "Enable Workflow -> Edit -> Add/Delete Security rights"),
						ResString.GetMultilingualString("95f8e928-7bfb-45d5-ac5e-0c91e62aa9dd", "When enabled, the Workflow -> Edit -> Add / Delete Security Rights will take effect. Disable this registry item to bypass these Security rights"),
						RegistryStorageFlags.System,
						true);

					return result;
				});
			}
		}

		#endregion

		#region EnableDateLimitsOnWorkflowTemplates

		public BooleanRegistryItem EnableDateLimitsOnWorkflowTemplates
		{
			get
			{
				return GetItem("EnableDateLimitsOnWorkflowTemplates", () => new BooleanRegistryItem(new EnableLimitsOnWorkflowTemplates()));
			}
		}

#if DEBUG
		public
#endif
		class EnableLimitsOnWorkflowTemplates : RegistryItemImpl
		{
			public EnableLimitsOnWorkflowTemplates()
				: base("EnableDateLimitsOnWorkflowTemplates",
					 WorkflowDataRegistry.Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("47d56330-49b8-4ec8-b5c8-fbc0abd2a297", "Enable Date Limits on Workflow Templates"),
						ResString.GetMultilingualString("8436ac17-0bde-4763-901f-3571780a90aa", "When enabled, Workflow Templates can have an Effective Start Date against them, specifying the oldest create date of jobs to which those templates can be applied."),
						new BooleanWithCheck(),
						null,
						RegistryStorageFlags.System,
						RegistryOptions.Default | RegistryOptions.MustOverrideDefaultValue,
						true)
			{
			}

#if DEBUG
			public
#endif
			class BooleanWithCheck : BooleanRegistryDataType
			{
				protected override void ValidateCore(IRegistryItem registryItem, bool proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
				{
					if (!proposedValue)
					{
						try
						{
							var check = @"select count(*) from dbo.ProcessTaskTemplate where (P0_EffectiveEndDateUtc is not null or P0_EffectiveStartDateUtc is not null) and P0_IsActive = 1";
							using (var command = Db.Connection.Command(check))
							{
								if (0 < (int)command.ExecuteScalar())
								{
									throw new RegistryValidationException(Res.GetString("2c2f39f5-2bd1-4beb-93e2-221e95da9524", "There are still active templates with Effective Dates configured."));
								}
							}
						}
						catch (SqlException e) when (!e.IsCriticalException())
						{
							ErrorReporter.ReportOnce("Error validating.", e);
						}
					}

					base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
				}

				protected override bool IsValidatedOnSetEvenIfEqualDefaultValueCore => true;
			}
		}

		#endregion

		#region Enable Workflow Template Scope Restriction

		public BooleanRegistryItem EnableWorkflowTemplateScopeRestrictions
		{
			get
			{
				return GetItem("EnableWorkflowTemplateScopeRestrictions", () =>
					new BooleanRegistryItem(
						"EnableWorkflowTemplateScopeRestrictions",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("92b1c284-405c-4262-9462-e4e1563ec9d9", "Enable Workflow Template Scope Restrictions"),
						ResString.GetMultilingualString("44aedda7-6ac2-4ab3-b47b-560433a87d31", "When enabled, Workflow Templates will not apply to unmodified jobs unless the job is in an open form or service task. Special rules also apply in the LWK service task."),
						RegistryStorageFlags.System,
						true
					));
			}
		}

		#endregion

		#region Enable User Defined Condition Fast Failing

		public BooleanRegistryItem EnableUserDefinedConditionFastFailing
		{
			get
			{
				return GetItem("EnableUserDefinedConditionFastFailing", () =>
					new BooleanRegistryItem(
						"EnableUserDefinedConditionFastFailing",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("5DE0C1CE-70B8-4D7D-A7A4-B99143B8F2CB", "Enable User Defined Condition Fast Failing"),
						ResString.GetMultilingualString("A26C2AA4-1981-48E8-9F39-B69C8861AA53", "When enabled, user defined conditions will be broken into separate conditions by the '&&' logical operator and evaluation will only continue if the previous condition is true. For example in <Condition1>&&<Condition2>, <Condition2> will be only be evaluated if <Condition1> is true. This can reduce the time taken for template application with UDF conditions."),
						RegistryStorageFlags.System,
						true
					));
			}
		}

		#endregion

		#region Validation Rule Timeout

		public IntRegistryItem ValidationRuleTimeout => GetItem(nameof(ValidationRuleTimeout), () => new IntRegistryItem(
			nameof(ValidationRuleTimeout),
			Categories.WorkflowManager_WorkflowTemplates_ValidationTools,
			ResString.GetMultilingualString("9590cb16-3b27-47bf-b6ca-63de59ee9dc9", "Validation Rule Timeout"),
			ResString.GetMultilingualString("dd2d7160-7600-42e9-b7d8-5763303229b3", "The maximum number of seconds a workflow validation rule will run before timing out. Any rule that breaches this threshold will be considered as failing and handled accordingly."),
			RegistryStorageFlags.System,
			RegistryOptions.Default,
			defaultValue: 5,
			minValue: 1,
			maxValue: int.MaxValue));

		#endregion

		#region EnableWorkflowValidation

		public WorkflowValidationProcessTypeCollectionRegistryItem EnableWorkflowValidation
		{
			get
			{
				return GetItem((NoResString)"EnableWorkflowValidation", delegate
				{
					return new WorkflowValidationProcessTypeCollectionRegistryItem(
						(NoResString)"EnableWorkflowValidation",
						Categories.WorkflowManager_WorkflowTemplates_ValidationTools,
						ResString.GetMultilingualString("7c465764-5ec0-4e46-928e-99eab68abcb6", "Enable Workflow Validation"),
						ResString.GetMultilingualString("a8709cc8-3800-4705-a14e-94137a2296d9", "Use this registry to manage what workflow templates can access the custom validation rules. Process types listed below will have the validation tab available and will cause all applicable jobs to search for existing rules. "),
						RegistryStorageFlags.Company,
						RegistryOptions.Default,
						WorkflowValidationProcessTypeCollection.DefaultValue);
				});
			}
		}

		#endregion

		#region Enable Template Potential Loop Validation

		public BooleanRegistryItem EnableTemplatePotentialLoopValidation
		{
			get
			{
				return GetItem("EnableTemplatePotentialLoopValidation", () => new BooleanRegistryItem(
					"EnableTemplatePotentialLoopValidation",
					Categories.WorkflowManager_WorkflowTemplates,
					(NoResString)"Enable Template Potential Loop Validation",
					(NoResString)"When enabled, perform potential loop detection validation when saving Workflow Templates.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					true));
			}
		}

		#endregion

		#region EnableUniversalTemplates

		public BooleanRegistryItem EnableUniversalTemplates
		{
			get
			{
				return GetItem("EnableUniversalTemplates", () =>
					new BooleanRegistryItem(
						"EnableUniversalTemplates",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("dbfc6913-b12c-4105-8cd6-f6533bbf7282", "Enable Universal Templates"),
						ResString.GetMultilingualString("6071f125-9e9f-4acb-86fc-77f90559e417", "When enabled, Workflow Templates can be defined as Universal. Any triggers defined on these templates will automatically appear on jobs that match the Template Selection Criteria. Matching events will fire these triggers. When disabled, a warning on Universal Templates will indicate that triggers defined on the template will neither appear on jobs nor be fired by matching events."),
						RegistryStorageFlags.System,
						true
					));
			}
		}

		#endregion

		#region EnableWorkflowTemplateAppliedEvent

		public BooleanRegistryItem EnableWorkflowTemplateAppliedEvent
		{
			get
			{
				return GetItem("EnableWorkflowTemplateAppliedEvent", () =>
					new BooleanRegistryItem(
						"EnableWorkflowTemplateAppliedEvent",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("721ff400-4e12-4730-a0af-312ed939bb19", "Enable Workflow Template Applied Event"),
						ResString.GetMultilingualString("edb46034-6026-4882-b48f-91b38594545d", "When Enabled, Workflow Template Applied (WTA) events will be created whenever Tasks, Triggers or Milestones are created by a Workflow Template."),
						RegistryStorageFlags.System,
						true
					));
			}
		}

		#endregion

		#region EnableWorkflowManualChangeEvent

		public BooleanRegistryItem EnableWorkflowManualChangeEvent
		{
			get
			{
				return GetItem("EnableWorkflowManualChangeEvent", () =>
					new BooleanRegistryItem(
						"EnableWorkflowManualChangeEvent",
						Categories.WorkflowManager,
						ResString.GetMultilingualString("871598d1-6d7d-4658-bae0-7e9e5cce0905", "Enable Workflow Manual Change Event"),
						ResString.GetMultilingualString("b58e805c-54e1-4d7a-8ce6-dffb2154d50c", "When Enabled, Workflow Items Deleted (WTD), Workflow Items Manually Added (WTM) and Reapply Workflow Templates Requested (WTR) events will be created whenever Tasks, Triggers or Milestones are manually deleted or added."),
						RegistryStorageFlags.System,
						true
					));
			}
		}

		#endregion

		#region EnableEnvironmentLoggingOnWorkflowTemplateAppliedEvent

		public BooleanRegistryItem EnableEnvironmentLoggingOnWorkflowTemplateAppliedEvent
		{
			get
			{
				return GetItem("EnableEnvironmentLoggingOnWorkflowTemplateAppliedEvent", () =>
					new BooleanRegistryItem(
						"EnableEnvironmentLoggingOnWorkflowTemplateAppliedEvent",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("5d011358-1190-4e0a-ae37-619d1c6b8600", "Enable Environmental Logging on Workflow Template Applied Event"),
						ResString.GetMultilingualString("6e9d8c40-4f14-403c-9913-8c6e84671813", "When Enabled, Workflow Template Applied (WTA) events will include information about the name of the server on which the application or service task is running, the process ID and the thread ID."),
						RegistryStorageFlags.System,
						defaultValue: false
					));
			}
		}

		#endregion

		#region EnableTemplateApplicationConcurrencyProtection

		public CodePairRegistryItem EnableTemplateApplicationConcurrencyProtection
		{
			get
			{
				return GetItem("EnableTemplateApplicationConcurrencyProtection", () =>
					new CodePairRegistryItem(
						"EnableTemplateApplicationConcurrencyProtection",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("b9c70dc8-a3fa-4832-a132-e6442213c1ba", "Template Concurrency Protection"),
						ResString.GetMultilingualString("8be6d201-26e9-4e57-bfb0-7dcfd8fb8b27", "When enabled race conditions in template application will be detected and handled during save. When enabled in the user interface you may lose changes if you apply a template, make changes and save if another process applies the template at the same time."),
						new CodeDescriptionPairListProvider(() => new TemplateApplicationRaceConditionHandlerOptions()),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						TemplateApplicationRaceConditionHandlerOptions.Codes.ServiceTasksOnly));
			}
		}

		#endregion

		#region EnableTemplateApplicationRaceConditionHandlerProcessTasksLock

		public BooleanRegistryItem EnableTemplateApplicationRaceConditionHandlerProcessTasksLock
		{
			get
			{
				return GetItem("EnableTemplateApplicationRaceConditionHandlerProcessTasksLock", () =>
					new BooleanRegistryItem(
						"EnableTemplateApplicationRaceConditionHandlerProcessTasksLock",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("5f94e3dd-a313-4fc0-91a4-5c1c96b8a927", "Template Application Race Condition Handler Process Tasks Lock"),
						ResString.GetMultilingualString("a52864ae-74d8-47cc-bd4e-8ef81cb8417d", "When enabled race conditions in template application will have locks on related Process Tasks."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true));
			}
		}

		#endregion

		#region EnableTemplateApplicationRaceConditionHandlerProcessHeaderLock

		public BooleanRegistryItem EnableWorkflowTemplateApplicationConcurrencyProtection
		{
			get
			{
				return GetItem("EnableWorkflowTemplateApplicationConcurrencyProtection", () =>
					new BooleanRegistryItem(
						"EnableWorkflowTemplateApplicationConcurrencyProtection",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("13E34012-EB4B-4B8D-A340-826B907C37DA", "Workflow Template Concurrency Protection"),
						ResString.GetMultilingualString("AAD30D48-1DF2-43E1-90BA-71393E01FF28", "When enabled race conditions in workflow template application will be detected and handled during save."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true));
			}
		}

		#endregion

		#region EnableTemplateApplicationInMemoryFiltering

		public BooleanRegistryItem EnableTemplateApplicationInMemoryFiltering
		{
			get
			{
				return GetItem("EnableTemplateApplicationInMemoryFiltering", () =>
					new BooleanRegistryItem(
						"EnableTemplateApplicationInMemoryFiltering",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("CA74C879-82A2-46E0-9D92-D83C66A819E7", "Workflow Template In Memory Filtering"),
						ResString.GetMultilingualString("4C797CBC-81B0-41FF-92C6-EE0C895E4E1D", "Workflow Templates are cached and then filtered in memory for application. Disabling this registry item uses the database to do the filtering, resulting in more database reads and less in memory caching."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true));
			}
		}

		#endregion

		#region ApplyTemplatesWhenEventsAreAddedToAJob

		public BooleanRegistryItem ApplyTemplatesWhenEventsAreAddedToAJob
		{
			get
			{
				return GetItem("ApplyTemplatesWhenEventsAreAddedToAJob", () =>
					new BooleanRegistryItem(
						"ApplyTemplatesWhenEventsAreAddedToAJob",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("F09D34C4-504C-4423-A88D-FF54E799263E", "Apply Workflow Template When Only Events Are Added"),
						ResString.GetMultilingualString("13E91AC8-25AD-43E1-8DC5-94C6927E0DCD",
						"If only an Event is added to a Job, Workflow Templates are not applied.\r\n" +
						"The reason being an Event is unlikely to change the Template matching criteria or Template Conditions of the Job.\r\n" +
						"If the Event causes a Trigger or Milestone to fire, Templates are not applied unless a Completion Trigger Action makes additional changes on the Job.\r\n" +
						"Any other edits to the Job will result in Template application.\r\n" +
						"Checking Template conditions on every edit is an expensive operation and this registry setting is introduced in response to performance issues.\r\n\r\n" +
						"Alternative solutions should be sought before enabling this registry."),
						RegistryStorageFlags.System,
						false));
			}
		}

		#endregion

		#region EnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLock

		public BooleanRegistryItem EnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLock
		{
			get
			{
				return GetItem("EnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLock", () =>
					new BooleanRegistryItem(
						"EnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLock",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("7ED08834-98CA-4E3D-9361-BFD10D998C2E", "Workflow Template Application Race Condition Handler Process Header Lock"),
						ResString.GetMultilingualString("EA9127A5-1CB7-43AF-8F92-A16F4D9A71D6", "When enabled race conditions in template application will have locks on related Process Headers (workflows)."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true));
			}
		}

		#endregion

		#region AlwaysCreateWorkflowLinksBetweenJobsGeneratedAsTheResultOfApplyingPartialWorkflowTemplates

		public BooleanRegistryItem AlwaysCreateWorkflowLinksBetweenJobsGeneratedAsTheResultOfApplyingPartialWorkflowTemplates
		{
			get
			{
				return GetItem("AlwaysCreateWorkflowLinksBetweenJobsGeneratedAsTheResultOfApplyingPartialWorkflowTemplates", () =>
					new BooleanRegistryItem(
						"AlwaysCreateWorkflowLinksBetweenJobsGeneratedAsTheResultOfApplyingPartialWorkflowTemplates",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("625015A8-5D1F-44D3-BD68-0279B3665450", "Always Create Workflow Links between Jobs Generated as the Result of Applying Partial Workflow Templates"),
						ResString.GetMultilingualString("515FCD5A-7230-4F18-AEC5-A356E1F3B170", @"Application of a partial workflow template triggered by a milestone or a trigger may cause creating workflow links between jobs (e.g. dependency links between workflows in a consolidation and workflows in attached shipments).

When this registry item is enabled, such links are always created (without requiring any confirmation). When disabled, such links are not created.

This registry item does not affect creating links within the same job or workflow links created by templates which are not partial and which are generated as the immediate result of a user action."),
						RegistryStorageFlags.System,
						defaultValue: true));
			}
		}

		#endregion

		#region EnableWorkflowEstimateMeasurement

		public BooleanRegistryItem EnableWorkflowEstimateMeasurement
		{
			get
			{
				return GetItem("EnableWorkflowEstimateMeasurement", () =>
					new BooleanRegistryItem(
						"EnableWorkflowEstimateMeasurement",
						RawDataRegistry.Categories.WorkflowManager,
						(NoResString)"Enable Workflow Task Estimate to Actual Measurement",
						(NoResString)"When enabled, task estimates and actuals will be measured showing changes in estimates from when work production starts.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false));
			}
		}

		#endregion

		#region EnableTriggerUserContextConfiguration

		public BooleanRegistryItem EnableTriggerUserContextConfiguration
		{
			get
			{
				return GetItem("EnableTriggerUserContextConfiguration", () =>
					new BooleanRegistryItem(
						"EnableTriggerUserContextConfiguration",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("6132a51f-ece0-446f-99c6-82ddcfb0f589", "Enable Controlling Trigger User Context"),
						ResString.GetMultilingualString("4b850426-d61e-456d-b5d8-386fb39974b4", "Allow Triggers and Milestones to configure the user context used when processing Completion Trigger Actions. When this is disabled, any existing Triggers and Milestones with a non-default user context will be ignored."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						defaultValue: true));
			}
		}

		#endregion

		#region EnableUserContextChangeLogging

		public BooleanRegistryItem EnableUserContextChangeLogging
		{
			get
			{
				return GetItem("EnableUserContextChangeLogging", () =>
					new BooleanRegistryItem(
						"EnableUserContextChangeLogging",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("2067c078-9b88-49ce-b82a-50a7b014aaa0", "Log Trigger User Context changes"),
						ResString.GetMultilingualString("0e7de8a1-5332-48e5-a9e9-5c7059d1dbde", "Log User Context changes when processing Completion Trigger Actions as a part of the LWK service task"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: false));
			}
		}

		#endregion

		#region RequireHasCapability

		public BooleanRegistryItem RequireResourceToHaveCapability
		{
			get
			{
				return GetItem("RequireResourceToHaveCapability", () =>
					new BooleanRegistryItem(
						"RequireResourceToHaveCapability",
						Categories.WorkflowManager_ResourceCapabilities,
						ResString.GetMultilingualString("0813E58B-7EEE-4954-8565-22416E9B6E84", "Require Resource To Have Capability"),
						ResString.GetMultilingualString("5E4C748A-E9BB-41C9-AE46-3A46B3A40F36", "Specify whether a resource must have the capability of a task to work on it. If activated, a validation error will be applied if the resource does not have the capability on a task in an applicable Buffer Management System, otherwise a warning will be applied."),
						RegistryStorageFlags.System,
						false
					));
			}
		}

		#endregion

		#region RequireCapabilityTasksToBeAbleToBeAssignedToResources

		public BooleanRegistryItem RequireCapabilityTasksToBeAbleToBeAssignedToResources
		{
			get
			{
				return GetItem("RequireCapabilityTasksToBeAbleToBeAssignedToResources", () =>
					new BooleanRegistryItem(
						"RequireCapabilityTasksToBeAbleToBeAssignedToResources",
						Categories.WorkflowManager_ResourceCapabilities,
						ResString.GetMultilingualString("6EBFD520-FE3A-4B65-817D-DA80EA2DB535", "Require Capability Tasks to Be Able to Be Assigned to Resources"),
						ResString.GetMultilingualString("FBAF0ADA-07A2-4914-83C2-2D69EE842036", @"When changing a task capability, a task group or workflow release group that leads to having a capability task that cannot be assigned to any resource as the cross section of the capability and task/release group is empty, a warning or an error appears depending on the value of this item.
By default a warning appears and the described changes are allowed. When the value is set to Yes, the system will show errors in the above mentioned cases disallowing the changes."),
						RegistryStorageFlags.System,
						false
					));
			}
		}

		#endregion

		#region PreventMilestoneFutureActualStart

		public BooleanRegistryItem PreventMilestoneFutureActualStart
		{
			get
			{
				return GetItem("PreventMilestoneFutureActualStart", () =>
					new BooleanRegistryItem(
						"PreventMilestoneFutureActualStart",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("39BDB16F-9BCA-4EC0-9724-681DD2884FEE", "Prevent Milestone Future Actual Start"),
						ResString.GetMultilingualString("17DB0B82-9686-4EFC-90B5-FB6BC3039C9F", "Prevent users from entering future dates and times for a milestone's Actual Start field. A validation error will require the Actual Start field is updated. For services where there is no user, a 'Future Event' Exception will be raised against the milestone."),
						RegistryStorageFlags.System,
						defaultValue: false
					));
			}
		}

		#endregion

		#region Defer Firing Workflow And Template Application During Consolidation Planning Board Save

		public BooleanRegistryItem DeferFiringWorkflowAndTemplateApplicationDuringConsolidationPlanningBoardSave
		{
			get
			{
				return GetItem("DeferFiringWorkflowAndTemplateApplicationDuringConsolidationPlanningBoardSave", () =>
					new BooleanRegistryItem(
						"DeferFiringWorkflowAndTemplateApplicationDuringConsolidationPlanningBoardSave",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("8A4F16F9-6B03-4685-98E4-CC8DE15CD5AC", "Defer Firing Workflow on Consolidation Planning Board"),
						ResString.GetMultilingualString("77A8AE8E-44B0-47CA-BF17-1A9AA2890489", "When enabled, the Consolidation Planning Board will be saved without waiting for Workflow Actions and Template application to be completed."),
						RegistryStorageFlags.System,
						true
					));
			}
		}

		#endregion

		#region Defer Firing Workflow And Template Application During Consolidation AWB Action Save

		public BooleanRegistryItem DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave
		{
			get
			{
				return GetItem("DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave", () =>
					new BooleanRegistryItem(
						"DeferFiringWorkflowAndTemplateApplicationDuringConsolAWBActionsSave",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("804de9cd-99ec-4823-8fbd-5c794cd1b0a8", "Defer Firing Workflow on Consolidation AWB Action"),
						ResString.GetMultilingualString("abb7a731-ab5c-44c0-ada3-bab703818a96", "When enabled, the Consolidation's Print Final Master Action will be executed without waiting for Workflow Actions and Template application to be completed."),
						RegistryStorageFlags.System,
						false
					));
			}
		}

		#endregion

		#region AllowTriggersToFireForExistingEvents

		public BooleanRegistryItem AllowTriggersToFireForExistingEvents
		{
			get
			{
				return GetItem("AllowTriggersToFireForExistingEvents", () =>
					new BooleanRegistryItem(
						"AllowTriggersToFireForExistingEvents",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("ec0a0fef-05cf-4484-b21a-37651525ff71", "Allow New Triggers to Fire for Existing Events"),
						ResString.GetMultilingualString("08502f9a-dd94-40ab-bf21-b510be49fa8f", @"When set to Yes, triggers from workflow templates will fire against the latest existing event that meets the condition of each type.
When set to No, triggers will not fire when the corresponding template trigger is created after an event was raised.
This registry item affects triggers only, and does not affect milestones.
Milestones from templates will fire against existing events."),
						RegistryStorageFlags.System,
						defaultValue: false
					));
			}
		}

		#endregion

		#region AllowFieldChangeTriggersToFireForNewJobs

		public BooleanRegistryItem AllowFieldChangeTriggersToFireForNewJobs
		{
			get
			{
				return GetItem("AllowFieldChangeTriggersToFireForNewJobs", () =>
					new BooleanRegistryItem(
						"AllowFieldChangeTriggersToFireForNewJobs",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("68B734B4-8B6C-4212-9A60-D12BB3FA8637", "Allow Field Change Triggers to Fire for New Jobs"),
						ResString.GetMultilingualString("E551BBBB-2CD2-4B93-A8A3-E489A1B59433", @"When disabled, modifying fields on a new job will not fire Field Change Triggers.
Once the job has been saved, any subsequent changes will fire Field Change Triggers."),
						RegistryStorageFlags.System,
						defaultValue: true
					));
			}
		}

		#endregion

		#region ShouldFindLogsOnRelatedJobsDuringLWKProcessing

		public BooleanRegistryItem ShouldFindLogsOnRelatedJobsDuringLWKProcessing
		{
			get
			{
				return GetItem("ShouldFindLogsOnRelatedJobsDuringLWKProcessing", () =>
					new BooleanRegistryItem(
						"ShouldFindLogsOnRelatedJobsDuringLWKProcessing",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("093f64b1-0e9e-45a5-9adf-a8c61237b802", "Should Find Logs On Related Jobs in LWK"),
						ResString.GetMultilingualString("ba1a5cbe-ebb0-4e19-a7eb-9d4d653344af", "Toggles whether or not the LWK service task looks for Events that have fired Triggers/Milestones on related jobs. This is just here as an emergency stop button to avoid certain performance issues."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true));
			}
		}

		#endregion

		#region EstimateCycleLimitBeforeDisable

		public IntRegistryItem EstimateCalculationCycleLimitBeforeDisable
		{
			get
			{
				return GetItem("EstimateCalculationCycleLimitBeforeDisable", () =>
					new IntRegistryItem(
						"EstimateCalculationCycleLimitBeforeDisable",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("d90788a1-ddca-4d12-918d-fa2101883f97", "Task and Milestone Estimate Recalculation Cycle Limit"),
						ResString.GetMultilingualString("73db317d-9f06-468a-831d-dbc7a9376b41", "The number of times an estimate can fluctuate in a repeated pattern before estimate recalculation is disabled on the given task or milestone in order to avoid an infinite loop."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						6,
						1,
						9999));
			}
		}

		#endregion

		#region MillisecondsBeforeLoggingTriggerActions

		public IntRegistryItem MillisecondsBeforeLoggingTriggerActions
		{
			get
			{
				return GetItem("MillisecondsBeforeLoggingTriggerActions", () =>
					new IntRegistryItem(
						"MillisecondsBeforeLoggingTriggerActions",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("99e39cd4-b2c4-44ba-ba1e-eca09a7bd978", "Milliseconds Before Logging Long Running Trigger Actions"),
						ResString.GetMultilingualString("0e369999-3fe6-4b06-bbda-e81e511cf84d", "The amount of time, in milliseconds, before the Log Walker Service Task logs long-running trigger actions. Minimum value: 0. Maximum value: 10,000. Default: 0 (disabled)."),
						RegistryStorageFlags.System,
						RegistryOptions.Default,
						0,
						0,
						10000));
			}
		}

		#endregion

		#region AllowTriggersToFireForExistingEvents

		public BooleanRegistryItem AlwaysApplyTasksFromTemplateWhenFirstSavingJob
		{
			get
			{
				return GetItem("AlwaysApplyTasksFromTemplateWhenFirstSavingJob", () =>
					new BooleanRegistryItem(
						"AlwaysApplyTasksFromTemplateWhenFirstSavingJob",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("85f05d1a-24e0-4f60-a7aa-8109f4cff230", "Always Apply Tasks from Template when First Saving Jobs"),
						ResString.GetMultilingualString("d3b3694e-9d37-4538-a5aa-f78b06e76ac7", "When set to Yes, tasks will always be applied from relevant workflow templates even if tasks have been manually created before saving the job for the first time. Subsequent saves will not apply tasks from matching templates when tasks exist on the job.\r\n\r\nWhen set to No, tasks will not be applied from workflow templates if tasks have been manually created before saving the job for the first time."),
						RegistryStorageFlags.All,
						defaultValue: true
					));
			}
		}

		#endregion

		#region ShowCustomFieldsFromCurrentTemplateOnly

		public BooleanRegistryItem ShowCustomFieldsFromCurrentTemplateOnly
		{
			get
			{
				return GetItem("ShowCustomFieldsFromCurrentTemplateOnly", () =>
					new BooleanRegistryItem(
						"ShowCustomFieldsFromCurrentTemplateOnly",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("0492c44c-a4c6-4e3e-8a5e-cfb422cfd56e", "Show Custom Fields only from current Workflow Template"),
						ResString.GetMultilingualString("7af30c7a-98ce-4c74-a080-2c8b7037c1e2", "When enabled, the list of Custom Fields on job forms will contain only items from the currently applied Workflow Template (and those defined in other sources).\r\nWhen disabled, the Custom Fields list will also contain any custom field with non-empty value."),
						RegistryStorageFlags.System,
						false
					));
			}
		}

		#endregion

		#region EnableTemplateReleaseGroupRules
		public BooleanRegistryItem EnableTemplateReleaseGroupRules
		{
			get
			{
				return GetItem("EnableTemplateReleaseGroupRules", () =>
					new BooleanRegistryItem(
						"EnableTemplateReleaseGroupRules",
						Categories.WorkflowManager_WorkflowTemplates,
						ResString.GetMultilingualString("8a4c7095-3c40-46eb-8a0b-4d95a00c76ac", "Enable Template Release Group Rules"),
						ResString.GetMultilingualString("f9cdc4a9-580a-40e3-a539-b07c96bc193c", "When set to Yes, the Release Group Rules tab will be shown on Workflow Templates, allowing the Release Group of workflows on jobs which match the template's selection criteria to be defaulted according to the rules."),
						RegistryStorageFlags.System,
						true
						));
			}
		}
		#endregion

		#region AlwaysCheckForExistingJobLevelWorkflowsInTheDatabaseWhenCreatingJobLevelWorkflows

		public BooleanRegistryItem AlwaysCheckForExistingJobLevelWorkflowsInTheDatabaseWhenCreatingJobLevelWorkflows
		{
			get
			{
				return GetItem("AlwaysCheckForExistingJobLevelWorkflowsInTheDatabaseWhenCreatingJobLevelWorkflows", () =>
					new BooleanRegistryItem(
						"AlwaysCheckForExistingJobLevelWorkflowsInTheDatabaseWhenCreatingJobLevelWorkflows",
						RawDataRegistry.Categories.WorkflowManager,
						(NoResString)"Always check for existing job-level workflows in the database when creating job-level workflows",
						(NoResString)"When enabled, the system will always check for existing job-level workflows in the database when creating job-level workflows. Otherwise, the system will check for existing job-level workflows in local factory cache only when the related job is not saved.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false));
			}
		}

		#endregion

		#region CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff

		public BooleanRegistryItem CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff
		{
			get
			{
				return GetItem("CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff", () =>
					new BooleanRegistryItem(
						"CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff",
						Categories.WorkflowManager_ResourceCapabilities,
						ResString.GetMultilingualString("ae99fe09-eccb-4965-8582-5695d1e04763", "Check only non-security groups on removal of last staff"),
						ResString.GetMultilingualString("0b776e30-9070-4e91-9e6c-d70e70826efe", "When removing a staff member from a GRP-scoped capability or a group, if this removal leaves the capability-group intersection without any staff, a confirmation message is shown. By default, the confirmation is shown for security and non-security groups. When this value is set to YES, the message is only shown for non-security groups."),
						RegistryStorageFlags.System,
						defaultValue: true));
			}
		}
		#endregion

		#region ExceptionCategories

		public CodeDescriptionPairListRegistryItem ExceptionCategories
		{
			get
			{
				return GetItem("ExceptionCategories", () =>
				{
					var dataType = new CodeDescriptionPairListRegistryDataType(3);
					dataType.AllowDuplicateCodes = false;
					dataType.AllowDuplicateDescriptions = false;
					dataType.AllowEmptyCodes = false;
					dataType.AllowEmptyDescriptions = false;

					var item = new CodeDescriptionPairListRegistryItem(
						name: "ExceptionCategories",
						category: Categories.WorkflowManager_Exceptions,
						caption: ResString.GetMultilingualString("4F58BA10-EB31-4A20-9B64-7F020C0B810E", "Exception Categories"),
						hint: ResString.GetMultilingualString("40507D68-F23D-4334-9536-7687EA878DF2", "Categories that the exceptions can be organized into"),
						maxCodeLength: 3,
						storage: RegistryStorageFlags.System,
						defaultValue: new ReadOnlyCodeDescriptionPairList())
					{
						DataType = dataType
					};

					item.EditorInfo = new CodeDescriptionPairListEditorInfo(
												true,
												true,
												CodeDescriptionPairListEditorInfo.CharacterCasing.Upper,
												CodeDescriptionPairListEditorInfo.CharacterCasing.Normal,
												ResString.GetMultilingualString("60B0ECD9-9BB2-4A12-8EDC-5A76243F8D01", "Category Code"),
												ResString.GetMultilingualString("97262D7D-D22A-459F-915F-D9A3D4201EBC", "Category Description"));

					return item;
				});
			}
		}

		#endregion

		#region ExceptionRequireType

		public BooleanRegistryItem ExceptionRequireType
		{
			get
			{
				return GetItem("ExceptionRequireType", () =>
					new BooleanRegistryItem(
						name: "ExceptionRequireType",
						category: Categories.WorkflowManager_Exceptions,
						caption: ResString.GetMultilingualString("05DF9B4F-9921-4B6F-9AF5-C07D350A5FF2", "Exception Require Type"),
						hint: ResString.GetMultilingualString("2D9ACCB9-1B4A-43A2-A16A-16D71D085502", "When enabled exception require type to be saved."),
						storage: RegistryStorageFlags.System,
						defaultValue: false));
			}
		}

		#endregion

		#region ExceptionEXRReference

		public BooleanRegistryItem ExceptionEXRReference
		{
			get
			{
				return GetItem("ExceptionEXRReference", delegate
				{
					var result = new BooleanRegistryItem(
						"ExceptionEXRReference",
						Categories.WorkflowManager_Exceptions,
						ResString.GetMultilingualString("3fe38fc4-6a2b-4e29-a500-4b456443d6e4", "Exception EXR Reference"),
						ResString.GetMultilingualString("1ad6dce4-34af-4e3c-97c7-eaf072cb7db6", @"When set to Yes, the Reference for EXR events will be a list of parameters in the form ""|TYP=Exception Type|EVT=Event Code|DES=Milestone Description"".
When set to No, the Reference will be a string in the form ""Type: [Exception Type]; Event: [Event Code]""."),
						RegistryStorageFlags.System,
						false);

					return result;
				});
			}
		}

		#endregion

		#region ExceptionRequireStaffGroup

		public BooleanRegistryItem ExceptionRequireStaffGroup
		{
			get
			{
				return GetItem("ExceptionRequireStaffGroup", delegate
				{
					var result = new BooleanRegistryItem(
						"ExceptionRequireStaffGroup",
						Categories.WorkflowManager_Exceptions,
						ResString.GetMultilingualString("5b8535ea-9679-4a0b-8d0d-0489d88ab3d1", "Exception Requires Staff and/or Group"),
						ResString.GetMultilingualString("5c8bb576-72c1-4692-9f55-1d7e5f6df26c", @"When enabled, Staff and/or Group are required to be saved."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);

					return result;
				});
			}
		}

		#endregion

		#region ExceptionAutoAssignStaff

		public BooleanRegistryItem ExceptionAutoAssignStaff
		{
			get
			{
				return GetItem("ExceptionAutoAssignStaff", delegate
				{
					var result = new BooleanRegistryItem(
						"ExceptionAutoAssignStaff",
						Categories.WorkflowManager_Exceptions,
						ResString.GetMultilingualString("82a292e8-5320-4374-8feb-2587b9e9bcd5", "Exception Staff Auto-Assignment"),
						ResString.GetMultilingualString("51c36712-dbe9-4785-8cfc-ce7edcc2f086", @"When enabled, the current User is assigned if no other Staff and/or Group field(s) are assigned when Exception(s) is selected as Actioned."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						true);

					return result;
				});
			}
		}

		#endregion

		#region FilterCustomFieldsByParentTable

		public BooleanRegistryItem FilterCustomFieldsByParentTable
		{
			get
			{
				return GetItem("FilterCustomFieldsByParentTable", () =>
					new BooleanRegistryItem(
						"FilterCustomFieldsByParentTable",
						RawDataRegistry.Categories.WorkflowManager,
						(NoResString)"Filter custom fields by parent table",
						(NoResString)"When set to Yes, custom fields will be filtered by parent id and parent table, instead of just parent id.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: true
					));
			}
		}

		#endregion

		#region Completion Milestone Outcome

		public CodePairRegistryItem CompletionMilestoneOutcome
		{
			get => GetItem("CompletionMilestoneOutcome", () => new CodePairRegistryItem(
				"CompletionMilestoneOutcome",
				RawDataRegistry.Categories.WorkflowManager,
				ResString.GetMultilingualString("26c19be2-70ea-46e5-93e0-3cdb91238234", "Completion Milestone Outcome"),
				ResString.GetMultilingualString("36d4ed41-6faa-4cbc-a30c-e76265c3e086", @"This controls what happens when a Completion Milestone occurs. Options are:
- (Code: CAN) The system attempts to change the task status to canceled (""CAN"").
- (Code: CLS) The system attempts to change the task status to closed (""CLS"") if it has a non-Zero Actual Duration and it is assigned to a Staff or Group; otherwise, the status will remain unchanged.
- (Code: FBK) The system attempts to change the task status to closed (""CLS"") if it has a non-zero Actual Duration and it is assigned to a Staff or Group; otherwise, the system will attempt to change the task status to canceled (""CAN"") (default option).

Tasks have mandatory and optional conditions that need to be met in order to have a CLS status. This includes tasks requiring a staff or group being assigned to them. Optionally, if a task is set to require an actual duration greater than zero, this can prevent a CLS status. Failure to meet these or other validation conditions will mean that users will be presented with errors to resolve, or an automated process will be unsuccessful in setting the status of tasks to CLS."),
				new CodeDescriptionPairListProvider(() => new CompletionMilestoneOutcomeModes()),
				RegistryStorageFlags.System,
				defaultValue: CompletionMilestoneOutcomeModes.Codes.AttemptToClose));
		}

		#endregion

		#region FeatureFlagMacroEnhancements

		public BooleanRegistryItem FeatureFlagMacroEnhancements
		{
			get
			{
				return GetItem("FeatureFlagMacroEnhancements", () =>
					new BooleanRegistryItem(
						"FeatureFlagMacroEnhancements",
						RawDataRegistry.Categories.WorkflowManager,
						(NoResString)"Feature Flag: Macro Enhancements",
						(NoResString)"Feature Flag: Macro Enhancements",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						defaultValue: false
					));
			}
		}

		#endregion

		#region McrDataFieldMapEnhancements

		public BooleanRegistryItem McrDataFieldMapEnhancements
		{
			get
			{
				return GetItem("McrDataFieldMapEnhancements", () =>
					new BooleanRegistryItem(
						"McrDataFieldMapEnhancements",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("038E32FA-4F83-4CE8-B25D-22B4A865E867", "MCR Data Field Map Enhancements"),
						ResString.GetMultilingualString("8A8C277C-DFCC-4015-AC1D-1CFA0E833629", "Enhanced MCR Data Field Map window for milestone/trigger conditions which shows all supported data models, variables and additional macro functions."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						defaultValue: true
					));
			}
		}

		#endregion

		#region EnhancedWorkflowStatusUpdateMode

		public BooleanRegistryItem EnhancedWorkflowStatusUpdateMode
		{
			get
			{
				return GetItem("EnhancedWorkflowStatusUpdateMode", delegate
				{
					var result = new BooleanRegistryItem(
						"EnhancedWorkflowStatusUpdateMode",
						RawDataRegistry.Categories.WorkflowManager,
						ResString.GetMultilingualString("841C9964-5245-4409-8599-78F71E03EC98", "Enhanced Workflow Status Update Mode"),
						ResString.GetMultilingualString("2069AB24-1DF4-4D76-A934-5E776E2F1A44", @"When enabled, the enhanced workflow status update logic is used to ensure correct status update on save in a multi-user environment:
1) tasks are reloaded from the database on save;
2) statuses of child workflows are updated prior to updating statuses of their parent workflows."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyEditableBySupportIfHosted,
						true);

					return result;
				});
			}
		}

		#endregion
	}
}

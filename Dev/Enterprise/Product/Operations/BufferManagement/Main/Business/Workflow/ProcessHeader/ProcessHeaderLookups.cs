using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderLookups : AutoProcessHeaderLookups, IProcessHeaderLookups
	{
		public ProcessHeaderLookups(AutoProcessHeader parent)
			: base(parent)
		{
		}

		#region Dates Defaults From List

		public CodeDescriptionPairList DatesDefaultsFromList => Parent?.WorkflowDescriptor?.EstimateDefaultedFromList ?? new CodeDescriptionPairList();

		#endregion

		#region CurrentComponents

		public virtual BMComponentCollection CurrentComponents
		{
			get { return new BMComponentCollection(Factory); }
		}

		#endregion

		#region ParentHeaders

		public virtual ProcessHeaderCollection ParentHeaders
		{
			get { return new ProcessHeaderCollection(Factory); }
		}

		#endregion

		new ProcessHeader Parent
		{
			get { return (ProcessHeader)base.Parent; }
		}

		#region HeadersWithDefaultFilter

		public ProcessHeaderCollection HeadersWithDefaultFilters
		{
			get
			{
				var collection = new ProcessHeaderCollection(Factory);
				AddFilterDefaults(Parent.JobHeader, collection);
				return collection;
			}
		}

		public static void AddFilterDefaults(ProcessJobHeader jobHeader, ProcessHeaderCollection headers)
		{
			if (jobHeader != null)
			{
				var parent = jobHeader.Parent;
				if (parent != null)
				{
					var code = CodePropertyAttribute.CodeFromBusinessObject((BusinessObject)(parent));
					if (!code.IsEmpty)
					{
						headers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ProcessHeader.ModuleFilterConstants.JobCode, "Property", code, true));
						headers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ProcessHeader.ModuleFilterConstants.JobCode, "WorkflowTypeCode", parent.WorkflowType, true));
					}
					else
					{
						headers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ProcessHeader.ModuleFilterConstants.WorkflowType, "Property", parent.WorkflowType, true));
					}
					headers.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(ProcessHeader.ModuleFilterConstants.JobOrWorkflow, "Property2", ZBool.True));
				}
			}
		}

		#endregion

		#region DateAcceptabilities

		public CodeDescriptionPairList DateAcceptabilities
		{
			get { return Factory.GetCachedValue<DateAcceptabilityList>(); }
		}

		#endregion

		#region DeadlineTypes

		public CodeDescriptionPairList DeadlineTypes
		{
			get { return Factory.GetCachedValue<DeadlineTypeList>(); }
		}

		#endregion

		#region Release Groups

		public ActiveBusinessObjectCollection<GlbGroup> AllReleaseGroups => new GlbGroupActiveBusinessObjectCollection(Factory, staffGroupOnly: true);

		#endregion

		#region Transfer Type

		public CodeDescriptionPairList TransferTypeList
		{
			get
			{
				return Factory.GetCachedValue("ProcessHeaderLookups.TransferTypeList",
				() =>
				{
					var list = new TransferTypeList();
					list.AddPair("NA", string.Empty);
					return list;
				});
			}
		}

		#endregion

		#region Workflow Prerequisite Status

		public CodeDescriptionPairList WorkflowPrerequisiteStatusList
		{
			get { return Factory.GetCachedValue<WorkflowPrerequisiteStatusList>(); }
		}

		#endregion

		#region CompletionMilestones

		public CodeDescriptionPairList CompletionMilestones
		{
			get
			{
				var parent = Parent;

				if (parent == null)
				{
					return new CodeDescriptionPairList();
				}

				ProcessTaskCollection workflowItems = parent.IsTemplate ? parent.Template?.WorkflowItems : parent.Parent?.WorkflowItems;

				return workflowItems?.CompletionMilestoneCodeDescriptionPairList ?? new CodeDescriptionPairList();
			}
		}

		#endregion

		#region WorkflowStatus

		public CodeDescriptionPairList WorkflowStatusList
		{
			get { return Factory.GetCachedValue<WorkflowStatusList>(); }
		}

		#endregion

		#region WorkflowCategories

		public WorkflowCategoryCollection WorkflowCategories
		{
			get
			{
				var workflowType = ResolvedWorkflowType;

				return Factory.GetCachedValue("WorkflowCategoriesLookups-" + workflowType, () =>
					BMSRegistry.Instance.WorkflowCategories.Value.GetCategoriesFromWorkflowCode(workflowType));
			}
		}

		ZString ResolvedWorkflowType => Parent.FH_WorkflowType == "P0" ? Parent.Template.P0_ProcessType : Parent.FH_WorkflowType;

		#endregion

		#region Iteration Reasons

		public WorkflowIterationReasonCollection IterationReasons
		{
			get { return Factory.GetCachedValue("IterationReasons", () => WorkflowDataRegistry.Instance.IterationReasons.Value.GetIterationReasonsFromWorkflowCode(Parent.Parent.WorkflowType)); }
		}

		#endregion

		#region Systems

		public BMSystemCollection Systems
		{
			get { return new BMSystemCollection(Factory); }
		}

		#endregion

		#region Buffer Timespans

		public BMBufferTimespanCollection BufferTimespans => new BMBufferTimespanCollection(Factory);

		#endregion

		#region IProcessHeaderLookups Members

		ICodeDescriptionPairList IProcessHeaderLookups.DateAcceptabilityList
		{
			get { return DateAcceptabilities; }
		}

		ICodeDescriptionPairList IProcessHeaderLookups.DeadlineTypeList
		{
			get { return DeadlineTypes; }
		}

		#endregion
	}
}

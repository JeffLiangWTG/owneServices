namespace Enterprise.Client.EDI.MasterFiles.ProcessManagement
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using CargoWise.Integration;
	using CargoWise.Types;
	using Enterprise.Client.EDI.IncidentManager.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Module;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Schema;

	public class EDIProcessTaskFilterBusinessObject : ProcessTaskFilterBusinessObject
	{
		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = base.GetModuleFiltersCore();
			AddTextFilters(filters);

			return filters;
		}

		#region Text

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Work Item Type", GetWorkItemTypeQuery, Lookups.ActiveActivitySubtypes)
				.WithMaxLengthOf<ModuleTextFilter>(WorkItemSchema.WKI_ActivitySubtype);
		}

		ZQuery GetWorkItemTypeQuery(SQLComparisonOperator comparisonOperator, ZString type)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(ProcessTask));
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(NewWorkItem), ProcessTasksSchema.P9_ParentID);
			subQuery.AddToFilter(WorkItemSchema.WKI_ActivitySubtype, comparisonOperator, type);

			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		protected override void AddAdditionalTextFilters(List<ZString> currentWorkflowTypes, CodeDescriptionPairList taskTypeList)
		{
			if (currentWorkflowTypes.Contains(JobInvoicingConsumerTypes.WorkItem.Code) && taskTypeList.Count > 0)
			{
				taskTypeList.AddPair("NOR", "Not Review");
			}
		}

		protected override ZQuery GetTaskTypeQuery(SQLComparisonOperator comparisonOperator, ZString type)
		{
			ZQuery query = new ZQuery();

			if (type == "NOR") // Not Review
			{
				foreach (ICodeDescription reviewTask in EDIDataRegistry.Instance.ReviewTasks.Value)
				{
					query.AddToFilter(ProcessTasksSchema.P9_Type, SQLComparisonOperator.NotEqual, reviewTask.Code);
				}
			}
			else
			{
				query.AddToFilter(ProcessTasksSchema.P9_Type, comparisonOperator, type);
			}

			return query;
		}

		#endregion

		#endregion

		#region Lookups

		protected NewWorkItemLookups Lookups
		{
			get { return lookups ?? (lookups = GetNewLookups()); }
		}

		protected NewWorkItemLookups GetNewLookups()
		{
			return new NewWorkItemLookups(Factory);
		}

		NewWorkItemLookups lookups;

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class TaskStatusFilter : ModuleFilter, ITaskStatusFilter
	{
		public TaskStatusFilter(BusinessObjectFactory factory)
			: base(Schema.Identifier, factory)
		{
			MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|TaskStatusFilter", "Task Status");
		}

		#region Schema

		public static class Schema
		{
			public const string Identifier = "Aggregated Task Status";
			public const string TaskTypeCheckList = "TaskTypeCheckList";
			public const string TaskStatusCheckList = "TaskStatusCheckList";
			public const string TaskAggregator = "TaskAggregator";
			public const string TaskAggregatorList = "TaskAggregatorList";
		}

		#endregion

		#region Properties

		#region Task Status Aggregate Flag

		[List(Schema.TaskAggregatorList)]
		public ZString TaskAggregator
		{
			get { return taskStatusAggregator; }
			set
			{
				if (TaskAggregator != value)
				{
					InvalidateCachedQuery();
				}

				SetNonPersistentPropertyValue(TaskAggregatorInfo, ref taskStatusAggregator, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTaskAggregator();
				}
			}
		}
		ZString taskStatusAggregator;

		public ZPropertyInfo TaskAggregatorInfo
		{
			get { return GetZPropertyInfo(Schema.TaskAggregator); }
		}

		#endregion

		#region Task Type

		[ResourceStringData("TaskStatusFilter.TaskTypeCheckList", Caption = "Task Type")]
		public ZBoolDescriptionPairList TaskTypeCheckList
		{
			get { return taskTypeCheckList ?? (taskTypeCheckList = GetNewTaskTypesList()); }
			set { taskTypeCheckList = GetTaskTypes().SynchroniseInto(value); }
		}

		ZBoolDescriptionPairList GetNewTaskTypesList()
		{
			var list = GetTaskTypes().ConvertToBoolDescriptionPairList();
			list.OnPairChanged += args => InvalidateCachedQuery();
			list.OnListChanged += args => InvalidateCachedQuery();

			return list;
		}

		ZBoolDescriptionPairList taskTypeCheckList;

		#endregion

		#region Task Status

		public ZBoolDescriptionPairList TaskStatusCheckList
		{
			get { return taskStatusCheckList ?? (taskStatusCheckList = GetNewTaskStatusCheckList()); }
			set { taskStatusCheckList = GetTaskStatuses().SynchroniseInto(value); }
		}

		ZBoolDescriptionPairList GetNewTaskStatusCheckList()
		{
			var list = GetTaskStatuses().ConvertToBoolDescriptionPairList();
			list.OnPairChanged += args => InvalidateCachedQuery();
			list.OnListChanged += args => InvalidateCachedQuery();

			return list;
		}

		ZBoolDescriptionPairList taskStatusCheckList;

		#endregion

		#endregion

		#region Query Implementation

		enum JobOrWorkflowFilterState { JobAndWorkflow, JobOnly, WorkflowOnly }

		JobOrWorkflowFilterState GetJobOrWorkflowState()
		{
			if (BMFilterStripsHelper.IsJobOrWorkflowFilterPresentInGroupWithJobOnlySelected(this))
			{
				return JobOrWorkflowFilterState.JobOnly;
			}

			if (BMFilterStripsHelper.ShouldOptimiseQueryForWorkflowOnly(this))
			{
				return JobOrWorkflowFilterState.WorkflowOnly;
			}

			return JobOrWorkflowFilterState.JobAndWorkflow;
		}

		int currentFilterID;

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var taskStatuses = TaskStatusCheckList.Where(pair => pair.Value).Select(pair => pair.Description.ToString()).ToArray();
			var taskTypeCodeAndDescriptions = new HashSet<ZString>(TaskTypeCheckList.Where(pair => pair.Value).Select(pair => pair.Description));
			var taskTypes = TaskTypeList.Cast<CodeDescriptionPair>().Where(pair => taskTypeCodeAndDescriptions.Contains(pair.CodeAndDescription)).Select(t => t.Code).ToArray();

			var otherTaskStatusFilters = FilterBusinessObject.ModuleFilters.OfType<TaskStatusFilter>();
			var largestExistingID = otherTaskStatusFilters.Max(f => f.currentFilterID);
			currentFilterID = largestExistingID + 1;

			return GetFilter(TaskAggregator, taskStatuses, taskTypes, GetJobOrWorkflowState(), currentFilterID);
		}

		public static ZQuery GetFilter(string taskAggregator, params string[] taskStatuses)
		{
			return GetFilter(taskAggregator, taskStatuses, Array.Empty<string>(), JobOrWorkflowFilterState.JobAndWorkflow, 0); //It's safe to use an ID of zero as this is always called with no taskTypes
		}

		static ZQuery GetFilter(string taskAggregator, string[] taskStatuses, string[] taskTypes, JobOrWorkflowFilterState jobOrWorkflowState, int currentFilterID)
		{
			var statusParameters = Parameterise("TaskStatus_QQZ", taskStatuses);
			var parameterisedStatuses = string.Join(",", statusParameters.Select(p => p.Item1));

			var typeParameters = Parameterise("TaskType_AAN", taskTypes);
			var parameterisedTypes = string.Join(",", typeParameters.Select(p => p.Item1));

			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			var subQuery = ZString.Empty;

			if (taskStatuses.Length > 0 || taskTypes.Length > 0)
			{
				switch (taskAggregator)
				{
					case TaskStatusAggregatorList.Codes.All:
						subQuery = MakeHeaders(parameterisedStatuses, parameterisedTypes, jobOrWorkflowState, currentFilterID, shouldExcludeTaskQueryPart: true);
						break;

					case TaskStatusAggregatorList.Codes.Any:
						subQuery = MakeHeaders(parameterisedStatuses, parameterisedTypes, jobOrWorkflowState, currentFilterID, shouldExcludeTaskQueryPart: false, determiner: "IN");
						break;

					case TaskStatusAggregatorList.Codes.None:
						subQuery = MakeHeaders(parameterisedStatuses, parameterisedTypes, jobOrWorkflowState, currentFilterID, shouldExcludeTaskQueryPart: false, determiner: (NoResString)"NOT IN"); // SQL isn't res text
						break;

					default:
						break;
				}
			}

			if (subQuery != ZString.Empty)
			{
				var parameterCollection = new ZSqlParameterCollection();

				if (taskTypes.Length > 0)
				{
					var actualColumn = ProcessTasksSchema.P9_Type;
					var isLiteralOnly = false; // This will allow the use of Table Valued Parameters on this column
					var columnToAllowTVP = new SchemaStringColumn(actualColumn.TableSchema, actualColumn.Name, actualColumn.Ordinal, actualColumn.SqlDbType, actualColumn.SqlDbDefault, actualColumn.IsNullable, actualColumn.MaxLength, isLiteralOnly, actualColumn.IsNonBlankFilteredIndexParticipant, actualColumn.TVPName);

					//Appending a different int value to each @TaskStatusFilterTypes parameter will prevent collisions when using multiple filter strips
					parameterCollection.Add(ZSqlParameter.New(string.Format(CultureInfo.InvariantCulture, "@TaskStatusFilterTypes{0}", currentFilterID), taskTypes, columnToAllowTVP, isTableValued: true)); // SQL Param isn't res text
				}

				foreach (var tuple in statusParameters)
				{
					parameterCollection.Add(tuple.Item1, tuple.Item2, ProcessTasksSchema.P9_Status);
				}

				query.AddFilterAndZSQLParameterCollection(subQuery, parameterCollection);
			}

			return query;
		}

		#region Build sql

		const string baseTaskConditional = "FROM dbo.ProcessTasks WHERE P9_Type <> 'MIL' AND P9_Type <> 'TRG' AND P9_Type <> 'EXC' AND P9_FH_ProcessHeader is not null "; // SQL isn't res text
		const string baseWorkflowQuery = "SELECT P9_FH_ProcessHeader " + baseTaskConditional;
		const string baseJobQuery = "SELECT P9_ParentID " + baseTaskConditional;

		static string MakeHeaders(string parameterisedStatuses, string parameterisedTypes, JobOrWorkflowFilterState jobOrWorkflowState, int currentFilterID, bool shouldExcludeTaskQueryPart, string determiner = "NOT IN")  // SQL isn't res text
		{
			var isJobOnlyFilterPresent = jobOrWorkflowState == JobOrWorkflowFilterState.JobOnly;
			var baseSelector = isJobOnlyFilterPresent ? "FH_ParentID" : "FH_PK";
			var baseQuery = isJobOnlyFilterPresent ? baseJobQuery : baseWorkflowQuery;

			if (shouldExcludeTaskQueryPart)
			{
				var statusQuery = !string.IsNullOrEmpty(parameterisedStatuses) ? string.Format(CultureInfo.InvariantCulture, " {0} AND P9_Status NOT IN ({1}) ", baseQuery, parameterisedStatuses) : string.Empty; // SQL isn't res text
				var typeQuery = !string.IsNullOrEmpty(parameterisedTypes) ? string.Format(CultureInfo.InvariantCulture, " {0} AND P9_Type NOT IN (SELECT Value FROM @TaskStatusFilterTypes{1}) ", baseQuery, currentFilterID) : string.Empty; // SQL isn't res text
				var groupedTaskQueries = string.Join(shouldExcludeTaskQueryPart ? (NoResString)" UNION ALL " : " ", new[] { statusQuery, typeQuery }.Where(s => !string.IsNullOrEmpty(s))); // SQL isn't res text
				var unionGroupedTaskQueries = GetUnionGroupedTaskQueries(groupedTaskQueries, jobOrWorkflowState);
				return string.Format(CultureInfo.InvariantCulture, (NoResString)@"{0} NOT IN ({1} {2})", baseSelector, groupedTaskQueries, unionGroupedTaskQueries); // SQL isn't res text
			}
			else
			{
				var statusQuery = !string.IsNullOrEmpty(parameterisedStatuses) ? string.Format(CultureInfo.InvariantCulture, "AND P9_Status IN ({0})", parameterisedStatuses) : string.Empty; // SQL isn't res text
				var typeQuery = !string.IsNullOrEmpty(parameterisedTypes) ? string.Format(CultureInfo.InvariantCulture, "AND P9_Type IN (SELECT Value FROM @TaskStatusFilterTypes{0})", currentFilterID) : string.Empty; // SQL isn't res text
				var groupedTaskQueries = string.Join(" ", new[] { baseQuery, statusQuery, typeQuery }.Where(s => !string.IsNullOrEmpty(s)));
				var unionGroupedTaskQueries = GetUnionGroupedTaskQueries(groupedTaskQueries, jobOrWorkflowState);
				return string.Format(CultureInfo.InvariantCulture, @"{0} {1} ({2} {3})", baseSelector, determiner, groupedTaskQueries, unionGroupedTaskQueries); // SQL isn't res text
			}
		}

		static string GetUnionGroupedTaskQueries(ZString groupedTaskQueries, JobOrWorkflowFilterState jobOrWorkflowFilter)
		{
			// The line below includes a check for FH_FH_ParentHeader is not null, this is done because the query optimiser attempts to help by running a query where because of the outer not in it tries to find a row with null which will invalidate the not in.
			return jobOrWorkflowFilter != JobOrWorkflowFilterState.JobAndWorkflow ?
				string.Empty :
				string.Format(CultureInfo.InvariantCulture, @"UNION ALL SELECT FH_FH_ParentHeader FROM dbo.ProcessHeader WHERE FH_PK IN ({0}) AND FH_FH_ParentHeader IS NOT NULL", groupedTaskQueries); // SQL isn't res text
		}

		#endregion

		#region Build parameters

		static Tuple<string, string>[] Parameterise(string name, string[] parameters)
		{
			var parameterPairs = new List<Tuple<string, string>>();

			for (int i = 0; i < parameters.Length; ++i)
			{
				parameterPairs.Add(Tuple.Create(string.Format(CultureInfo.InvariantCulture, "@{0}{1}", name, i), parameters[i]));
			}

			return parameterPairs.ToArray();
		}

		#endregion

		protected override bool IsEmptyCore => false;

		public override bool IsExpensiveQuery => false;

		#endregion

		#region Filter Implementation

		protected override FilterCategory DefaultCategory => ProcessHeaderFilterBusinessObject.TasksFilterCategory;

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new TaskStatusFilter(Factory) { FilterBusinessObject = FilterBusinessObject };
		}

		protected override object[] QueryDelegateParameters => new object[] { TaskTypeCheckList, TaskStatusCheckList, TaskAggregator };

		protected override void ClearCore()
		{
			taskTypeCheckList = null;
			taskStatusCheckList = null;
			TaskAggregator = ZString.Empty;
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var taskFilter = filterToCopyFrom as TaskStatusFilter;
			if (taskFilter != null)
			{
				TaskTypeCheckList = taskFilter.TaskTypeCheckList;
				TaskStatusCheckList = taskFilter.TaskStatusCheckList;
				TaskAggregator = taskFilter.TaskAggregator;
			}
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString(Schema.TaskAggregator, TaskAggregator);

			foreach (var code in TaskStatusCheckList.Where(pair => pair.Value).Select(pair => pair.Description))
			{
				writer.WriteElementString(Schema.TaskStatusCheckList, code);
			}

			foreach (var code in TaskTypeCheckList.Where(pair => pair.Value).Select(pair => pair.Description))
			{
				writer.WriteElementString(Schema.TaskTypeCheckList, code);
			}
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == Schema.TaskAggregator)
			{
				TaskAggregator = reader.ReadElementContentAsString();
			}

			while (reader.Name == Schema.TaskStatusCheckList)
			{
				var code = reader.ReadElementContentAsString();
				var taskStatus = TaskStatusCheckList.SingleOrDefault(pair => pair.Description == code);
				if (taskStatus != null)
				{
					taskStatus.Value = true;
				}
			}

			while (reader.Name == Schema.TaskTypeCheckList)
			{
				var code = reader.ReadElementContentAsString();
				var taskType = TaskTypeCheckList.SingleOrDefault(pair => pair.Description == code);
				if (taskType != null)
				{
					taskType.Value = true;
				}
			}
		}

		#endregion

		#region Related Business Objects

		public CodeDescriptionPairList TaskStatusList
		{
			get { return Factory.GetCachedValue<ProcessTaskStatusCodeList>(); }
		}

		IEnumerable<string> GetTaskStatuses()
		{
			return TaskStatusList.Cast<CodeDescriptionPair>().Select(p => p.Code);
		}

		public CodeDescriptionPairList TaskAggregatorList
		{
			get { return Factory.GetCachedValue<TaskStatusAggregatorList>(); }
		}

		public CodeDescriptionPairList TaskTypeList
		{
			get
			{
				var workflowTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
				var pairList = new CodeDescriptionPairList();

				var taskTypes = workflowTaskTypes.Cast<CategorisedWorkflowTaskTypes>().SelectMany(cwtt => cwtt.TaskTypes.Cast<WorkflowTaskType>());

				foreach (var pair in taskTypes.Select(tt => new CodeDescriptionPair((string)tt.Code, tt.Description)).Distinct().OrderBy(pair => pair.CodeAndDescription).ToList())
				{
					pairList.Add(pair);
				}

				return pairList;
			}
		}

		IEnumerable<string> GetTaskTypes()
		{
			return TaskTypeList.Cast<CodeDescriptionPair>().Select(p => p.CodeAndDescription);
		}

		#endregion

		#region Validation

		public new TaskStatusFilterValidation Validation
		{
			get { return (TaskStatusFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new TaskStatusFilterValidation(this);
		}

		public class TaskStatusFilterValidation : ModuleFilterValidation
		{
			public TaskStatusFilterValidation(TaskStatusFilter parent)
				: base(parent)
			{
				this.parent = parent;
			}

			readonly TaskStatusFilter parent;

			public void ValidateTaskAggregator()
			{
				ValidateCalculatedProperty(parent.TaskAggregatorInfo);
			}

			protected virtual void CheckTaskAggregator()
			{
				MandatoryValidation.CheckEntered(parent.TaskAggregatorInfo);
				ListValidation.ErrorIfInvalidCode(parent.TaskAggregatorInfo);
			}

			public override void ValidateAll()
			{
				ValidateTaskAggregator();
			}

			public override Type AutoValidationType
			{
				get { return GetType(); }
			}
		}

		#endregion

		#region Test Data Setup
#if DEBUG

		protected override void FillWithValidTestFilterValueCore()
		{
			TaskAggregator = RandomString(MaxLength);
		}

#endif
		#endregion
	}
}

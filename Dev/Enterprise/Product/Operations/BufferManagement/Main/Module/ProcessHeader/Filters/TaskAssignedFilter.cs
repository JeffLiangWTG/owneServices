using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.BufferManagement.Business.ProcessHeader;

namespace Enterprise.BufferManagement.Module
{
	public class TaskAssignedFilter : ModuleFilter
	{
		public TaskAssignedFilter(BusinessObjectFactory factory)
			: base(Schema.Identifier, factory)
		{
			MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|TaskAssignedFilter", "Task Assigned");
		}

		#region Schema

		public static class Schema
		{
			public const string Identifier = ModuleFilterConstants.TaskAssigned;
			public const string TaskOrdinality = "TaskOrdinality";
			public const string TaskOrdinalityList = "TaskOrdinalityList";
			public const string TaskAssignmentType = "TaskAssignmentType";
			public const string TaskAssignmentTypeList = "TaskAssignmentTypeList";
		}

		#endregion

		#region Properties

		#region Task Ordinality

		[List(Schema.TaskOrdinalityList)]
		public ZString TaskOrdinality
		{
			get { return taskOrdinality; }
			set
			{
				SetNonPersistentPropertyValue(TaskOrdinalityInfo, ref taskOrdinality, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTaskOrdinality();
				}
			}
		}

		ZString taskOrdinality;

		public ZPropertyInfo TaskOrdinalityInfo
		{
			get { return GetZPropertyInfo(Schema.TaskOrdinality); }
		}

		#endregion

		#region Task Assignement Type

		[List(Schema.TaskAssignmentTypeList)]
		public ZString TaskAssignmentType
		{
			get { return taskAssignmentType; }
			set
			{
				SetNonPersistentPropertyValue(TaskAssignmentTypeInfo, ref taskAssignmentType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTaskAssignmentType();
				}
			}
		}

		public ZPropertyInfo TaskAssignmentTypeInfo
		{
			get { return GetZPropertyInfo(Schema.TaskAssignmentType); }
		}

		ZString taskAssignmentType;

		#endregion

		#endregion

		#region Query Implementation

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));

			if (!IsEmpty)
			{
				var isWorkflowOnly = BMFilterStripsHelper.ShouldOptimiseQueryForWorkflowOnly(this);
				var subQuery = GetSubQuery(TaskOrdinality, TaskAssignmentType, isWorkflowOnly);

				if (subQuery != null)
				{
					query.AddFilterAndZSQLParameterCollection(subQuery.Item1, subQuery.Item2);
				}
			}

			return query;
		}

		static Tuple<string, ZSqlParameterCollection> GetSubQuery(string taskAssigned, string taskAssignmentType, bool isWorkflowOnly)
		{
			return GetSubQueryAll(taskAssigned, taskAssignmentType, isWorkflowOnly) ??
				GetSubQueryAny(taskAssigned, taskAssignmentType, isWorkflowOnly) ??
				GetSubQueryNone(taskAssigned, taskAssignmentType, isWorkflowOnly);
		}

		static Tuple<string, ZSqlParameterCollection> GetSubQueryAll(string taskAssigned, string taskAssignmentType, bool isWorkflowOnly)
		{
			switch (taskAssigned + taskAssignmentType)
			{
				case TaskAssignedOrdinalityFilterList.Codes.All + TaskAssignmentTypeFilterList.Codes.Both:
					return GenerateAllBothTaskSubQuery(isWorkflowOnly);

				case TaskAssignedOrdinalityFilterList.Codes.All + TaskAssignmentTypeFilterList.Codes.Either:
					return GenerateEitherTaskSubQuery(Assignments.Resources | Assignments.Capabilities, isWorkflowOnly);

				case TaskAssignedOrdinalityFilterList.Codes.All + TaskAssignmentTypeFilterList.Codes.Capabilities:
					return GenerateEitherTaskSubQuery(Assignments.Capabilities, isWorkflowOnly);

				case TaskAssignedOrdinalityFilterList.Codes.All + TaskAssignmentTypeFilterList.Codes.Resources:
					return GenerateEitherTaskSubQuery(Assignments.Resources, isWorkflowOnly);

				case TaskAssignedOrdinalityFilterList.Codes.All + TaskAssignmentTypeFilterList.Codes.Unassigned:
					return GenerateAllTaskUnassignedSubQuery(isWorkflowOnly);

				default:
					return null;
			}
		}

		static Tuple<string, ZSqlParameterCollection> GetSubQueryAny(string taskAssigned, string taskAssignmentType, bool isWorkflowOnly)
		{
			switch (taskAssigned + taskAssignmentType)
			{
				case TaskAssignedOrdinalityFilterList.Codes.Any + TaskAssignmentTypeFilterList.Codes.Both:
					return GenerateAnyTaskSubQuery(Assignments.Resources | Assignments.Capabilities, isWorkflowOnly);

				case TaskAssignedOrdinalityFilterList.Codes.Any + TaskAssignmentTypeFilterList.Codes.Either:
					return GenerateAnyEitherTaskSubQuery(Assignments.Resources | Assignments.Capabilities, isWorkflowOnly);

				case TaskAssignedOrdinalityFilterList.Codes.Any + TaskAssignmentTypeFilterList.Codes.Capabilities:
					return GenerateAnyTaskSubQuery(Assignments.Capabilities, isWorkflowOnly);

				case TaskAssignedOrdinalityFilterList.Codes.Any + TaskAssignmentTypeFilterList.Codes.Resources:
					return GenerateAnyTaskSubQuery(Assignments.Resources, isWorkflowOnly);

				case TaskAssignedOrdinalityFilterList.Codes.Any + TaskAssignmentTypeFilterList.Codes.Unassigned:
					return GenerateAnyTaskUnassignedSubQuery(isWorkflowOnly);

				default:
					return null;
			}
		}

		static Tuple<string, ZSqlParameterCollection> GetSubQueryNone(string taskAssigned, string taskAssignmentType, bool isWorkflowOnly)
		{
			switch (taskAssigned + taskAssignmentType)
			{
				case TaskAssignedOrdinalityFilterList.Codes.None + TaskAssignmentTypeFilterList.Codes.Both:
					return GenerateNoBothTaskSubQuery(Assignments.Resources | Assignments.Capabilities, isWorkflowOnly);

				case TaskAssignedOrdinalityFilterList.Codes.None + TaskAssignmentTypeFilterList.Codes.Either:
					return GenerateNoTaskSubQuery(Assignments.Resources | Assignments.Capabilities, isWorkflowOnly);

				case TaskAssignedOrdinalityFilterList.Codes.None + TaskAssignmentTypeFilterList.Codes.Capabilities:
					return GenerateNoTaskSubQuery(Assignments.Capabilities, isWorkflowOnly);

				case TaskAssignedOrdinalityFilterList.Codes.None + TaskAssignmentTypeFilterList.Codes.Resources:
					return GenerateNoTaskSubQuery(Assignments.Resources, isWorkflowOnly);

				case TaskAssignedOrdinalityFilterList.Codes.None + TaskAssignmentTypeFilterList.Codes.Unassigned:
					return GenerateEitherTaskSubQuery(Assignments.Resources | Assignments.Capabilities, isWorkflowOnly);

				default:
					return null;
			}
		}

		[Flags]
		enum Assignments
		{
			None = 0,
			Resources = 1,
			Capabilities = 2,
		}

		static Tuple<string, ZSqlParameterCollection> GenerateAllBothTaskSubQuery(bool isWorkflowOnly)
		{
			var queryParts = GetInclusiveQueryParts(Assignments.Resources | Assignments.Capabilities);
			return CreateWorkflowAndTaskQuery(unionParts: true, inWorkflow: false, queryParts: queryParts, isWorkflowOnly: isWorkflowOnly);
		}

		static Tuple<string, ZSqlParameterCollection> GenerateEitherTaskSubQuery(Assignments assignments, bool isWorkflowOnly)
		{
			var queryParts = GetInclusiveQueryParts(assignments);
			return CreateWorkflowAndTaskQuery(unionParts: false, inWorkflow: false, queryParts: queryParts, isWorkflowOnly: isWorkflowOnly);
		}

		static Tuple<string, ZSqlParameterCollection> GenerateAllTaskUnassignedSubQuery(bool isWorkflowOnly)
		{
			var queryParts = GetExclusiveQueryParts(Assignments.Resources | Assignments.Capabilities);
			return CreateWorkflowAndTaskQuery(unionParts: true, inWorkflow: false, queryParts: queryParts, isWorkflowOnly: isWorkflowOnly);
		}

		static Tuple<string, ZSqlParameterCollection> GenerateAnyTaskSubQuery(Assignments assignments, bool isWorkflowOnly)
		{
			var queryParts = GetExclusiveQueryParts(assignments);
			return CreateWorkflowAndTaskQuery(unionParts: false, inWorkflow: true, queryParts: queryParts, isWorkflowOnly: isWorkflowOnly);
		}

		static Tuple<string, ZSqlParameterCollection> GenerateAnyEitherTaskSubQuery(Assignments assignments, bool isWorkflowOnly)
		{
			var queryParts = GetExclusiveQueryParts(assignments);
			return CreateWorkflowAndTaskQuery(unionParts: true, inWorkflow: true, queryParts: queryParts, isWorkflowOnly: isWorkflowOnly);
		}

		static Tuple<string, ZSqlParameterCollection> GenerateAnyTaskUnassignedSubQuery(bool isWorkflowOnly)
		{
			var queryParts = GetInclusiveQueryParts(Assignments.Resources | Assignments.Capabilities);
			return CreateWorkflowAndTaskQuery(unionParts: false, inWorkflow: true, queryParts: queryParts, isWorkflowOnly: isWorkflowOnly);
		}

		static Tuple<string, ZSqlParameterCollection> GenerateNoTaskSubQuery(Assignments assignments, bool isWorkflowOnly)
		{
			var queryParts = GetExclusiveQueryParts(assignments);
			return CreateWorkflowAndTaskQuery(unionParts: true, inWorkflow: false, queryParts: queryParts, isWorkflowOnly: isWorkflowOnly);
		}

		static Tuple<string, ZSqlParameterCollection> GenerateNoBothTaskSubQuery(Assignments assignments, bool isWorkflowOnly)
		{
			var queryParts = GetExclusiveQueryParts(assignments);
			return CreateWorkflowAndTaskQuery(unionParts: false, inWorkflow: false, queryParts: queryParts, isWorkflowOnly: isWorkflowOnly);
		}

		static string[] GetInclusiveQueryParts(Assignments assignments)
		{
			return new[] { assignments.HasFlag(Assignments.Resources) ? "P9_GS_NKAssignedStaffMember = ''" : string.Empty, assignments.HasFlag(Assignments.Capabilities) ? "P9_G4_RequiredCapability is null" : string.Empty }; // Sql isn't res string.
		}

		static string[] GetExclusiveQueryParts(Assignments assignments)
		{
			return new[] { assignments.HasFlag(Assignments.Resources) ? "P9_GS_NKAssignedStaffMember != ''" : string.Empty, assignments.HasFlag(Assignments.Capabilities) ? "P9_G4_RequiredCapability is not null" : string.Empty }; // Sql isn't res string.
		}

		const string baseTaskQuery = @"
select * from dbo.ProcessTasks
	where {0}
		and P9_Status in ({1}) 
		and P9_Type <> 'MIL' 
		and P9_Type <> 'TRG'
		and P9_Type <> 'EXC'
";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Sql isn't res string.")]
		static Tuple<string, ZSqlParameterCollection> CreateWorkflowAndTaskQuery(bool unionParts, bool inWorkflow, IEnumerable<string> queryParts, bool isWorkflowOnly)
		{
			var statusParameters = Parameterise("@TaskStatusLT", ProcessTasks.GetOpenTaskStatuses().ToArray());
			var statusSql = string.Join(", ", statusParameters.Select(s => s.Item1));
			var taskSubQuery = string.Empty;

			if (!inWorkflow && !unionParts)
			{
				return Tuple.Create(GenerateWorkflowSubQueryForNotInAndNonUnion_ForPerformance(queryParts, isWorkflowOnly), CreateStatusParameterCollection(statusParameters));
			}
			else
			{
				if (unionParts)
				{
					const string unionAll = " union all ";
					taskSubQuery = string.Join(unionAll, queryParts.Where(s => !string.IsNullOrEmpty(s)).Select(queryPart => string.Format(CultureInfo.InvariantCulture, baseTaskQuery, queryPart, statusSql)));
				}
				else
				{
					const string and = " AND ";
					taskSubQuery = string.Format(CultureInfo.InvariantCulture, baseTaskQuery, string.Join(and, queryParts.Where(s => !string.IsNullOrEmpty(s))), statusSql);
				}

				var predicate = inWorkflow ? (NoResString)"in" : (NoResString)"not in";

				return Tuple.Create(GenerateWorkflowSubQuery(taskSubQuery, predicate, isWorkflowOnly), CreateStatusParameterCollection(statusParameters));
			}
		}

		static ZSqlParameterCollection CreateStatusParameterCollection(Tuple<string, string>[] statusParameters)
		{
			var parameterCollection = new ZSqlParameterCollection();

			foreach (var statusParameter in statusParameters)
			{
				parameterCollection.Add(statusParameter.Item1, statusParameter.Item2, ProcessTasksSchema.P9_Status);
			}
			return parameterCollection;
		}

		const string baseTaskForNotInAndNonUnionQuery = @"
SELECT 1 
FROM dbo.ProcessTasks
{0}
WHERE {1}
";

		static string GenerateWorkflowSubQueryForNotInAndNonUnion_ForPerformance(IEnumerable<string> queryParts, bool isWorkflowOnly)
		{
			var statusParameters = Parameterise("@TaskStatusLT", ProcessTasks.GetOpenTaskStatuses().ToArray());
			var openStatuses = string.Join(", ", statusParameters.Select(s => s.Item1));

			var nonTaskTypes = string.Join(", ", ProcessTasks.GetNonTaskTypes().Select(s => string.Format(CultureInfo.InvariantCulture, "'{0}'", s)).ToArray());

			var queryPartsWithWorkflowFilter = queryParts.ToList();

			queryPartsWithWorkflowFilter.Add(string.Format(CultureInfo.InvariantCulture, (NoResString)" {0} IN ({1}) ", ProcessTasksSchema.P9_Status.Name, openStatuses)); // Sql string
			queryPartsWithWorkflowFilter.Add(string.Format(CultureInfo.InvariantCulture, (NoResString)" {0} NOT IN ({1}) ", ProcessTasksSchema.P9_Type.Name, nonTaskTypes)); // Sql string
			queryPartsWithWorkflowFilter.Add(string.Format(CultureInfo.InvariantCulture, " {0} = {1} ", ProcessTasksSchema.P9_FH_ProcessHeader.Name, ProcessHeaderSchema.PK.Name)); // Sql string

			var workflowTaskForExclusion = string.Format(
				CultureInfo.InvariantCulture,
				baseTaskForNotInAndNonUnionQuery,
				string.Empty,
				string.Join(" AND ", queryPartsWithWorkflowFilter.Where(s => !string.IsNullOrEmpty(s))));

			var queryAsString = string.Format(CultureInfo.InvariantCulture, (NoResString)"NOT EXISTS ( {0} )", workflowTaskForExclusion); // Sql string
			if (!isWorkflowOnly)
			{
				var queryPartsWithJobFilter = queryParts.ToList();
				queryPartsWithJobFilter.Add(string.Format(CultureInfo.InvariantCulture, (NoResString)" {0} IN ({1}) ", ProcessTasksSchema.P9_Status.Name, openStatuses)); // Sql string
				queryPartsWithJobFilter.Add(string.Format(CultureInfo.InvariantCulture, (NoResString)" {0} NOT IN ({1}) ", ProcessTasksSchema.P9_Type.Name, nonTaskTypes)); // Sql string
				queryPartsWithJobFilter.Add(string.Format(CultureInfo.InvariantCulture, " {0} = {1} ", ViewProcessHeaderSchema.VFH_FH_ParentHeader.Name, ProcessHeaderSchema.PK.Name)); // Sql string

				var jobTaskForExclusion = string.Format(
					CultureInfo.InvariantCulture,
					baseTaskForNotInAndNonUnionQuery,
					string.Format(CultureInfo.InvariantCulture, (NoResString)" JOIN {0} ON {1} = {2}", ViewProcessHeader.Schema.TableName, ViewProcessHeaderSchema.PK.Name, ProcessTasksSchema.P9_FH_ProcessHeader.Name), // Sql string
					string.Join(" AND ", queryPartsWithJobFilter.Where(s => !string.IsNullOrEmpty(s))));

				queryAsString = string.Format(CultureInfo.InvariantCulture, (NoResString)"{0} AND NOT EXISTS ({1})", queryAsString, jobTaskForExclusion); // Sql string
			}

			return queryAsString;
		}

		static string GenerateWorkflowSubQuery(string taskSubQuery, string predicate, bool isWorkflowOnly)
		{
			string subHeaderQuery = isWorkflowOnly ?
				")" :
				@"
				UNION ALL
				SELECT FH_FH_ParentHeader FROM ({0}) task
					JOIN dbo.ProcessHeader ON FH_PK = P9_FH_ProcessHeader
				)";

			string baseHeaderQuery = @"
			FH_PK {1} (
				SELECT FH_PK FROM ({0}) task
					JOIN dbo.ProcessHeader ON FH_PK = P9_FH_ProcessHeader
					" + subHeaderQuery;

			return string.Format(CultureInfo.InvariantCulture, baseHeaderQuery, taskSubQuery, predicate);
		}

		protected override bool IsEmptyCore => string.IsNullOrEmpty(TaskOrdinality) || string.IsNullOrEmpty(TaskAssignmentType);

		public override bool IsExpensiveQuery => false;

		#endregion

		#region Build parameters

		static Tuple<string, string>[] Parameterise(string name, string[] parameters)
		{
			var parameterPairs = new List<Tuple<string, string>>();

			for (int i = 0; i < parameters.Length; ++i)
			{
				parameterPairs.Add(Tuple.Create(string.Format(CultureInfo.InvariantCulture, "{0}{1}", name, i), parameters[i]));
			}

			return parameterPairs.ToArray();
		}

		#endregion

		#region Filter Implementation

		protected override FilterCategory DefaultCategory
		{
			get { return ProcessHeaderFilterBusinessObject.TasksFilterCategory; }
		}

		protected override ModuleFilter GetNewCommonModuleFilter(FilterCategory category, ModuleFilterCollection parentCollection)
		{
			return new TaskAssignedFilter(Factory) { FilterBusinessObject = FilterBusinessObject };
		}

		protected override object[] QueryDelegateParameters
		{
			get { return new object[] { TaskAssignmentType, TaskOrdinality }; }
		}

		protected override void ClearCore()
		{
			TaskAssignmentType = ZString.Empty;
			TaskOrdinality = ZString.Empty;
		}

		protected override void CopyPersistantValuesFromFilter(ModuleFilter filterToCopyFrom)
		{
			var taskFilter = filterToCopyFrom as TaskAssignedFilter;
			if (taskFilter != null)
			{
				TaskAssignmentType = taskFilter.TaskAssignmentType;
				TaskOrdinality = taskFilter.TaskOrdinality;
			}
		}

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			writer.WriteElementString(Schema.TaskOrdinality, TaskOrdinality);
			writer.WriteElementString(Schema.TaskAssignmentType, TaskAssignmentType);
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			if (reader.Name == Schema.TaskOrdinality)
			{
				TaskOrdinality = reader.ReadElementContentAsString();
			}

			if (reader.Name == Schema.TaskAssignmentType)
			{
				TaskAssignmentType = reader.ReadElementContentAsString();
			}
		}

		#endregion

		#region Related Business Objects

		public CodeDescriptionPairList TaskOrdinalityList
		{
			get { return Factory.GetCachedValue<TaskAssignedOrdinalityFilterList>(); }
		}

		public CodeDescriptionPairList TaskAssignmentTypeList
		{
			get { return Factory.GetCachedValue<TaskAssignmentTypeFilterList>(); }
		}

		#endregion

		#region Validation

		public new TaskAssignedFilterValidation Validation
		{
			get { return (TaskAssignedFilterValidation)base.Validation; }
		}

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new TaskAssignedFilterValidation(this);
		}

		public class TaskAssignedFilterValidation : ModuleFilterValidation
		{
			public TaskAssignedFilterValidation(TaskAssignedFilter parent)
				: base(parent)
			{
				this.parent = parent;
			}

			readonly TaskAssignedFilter parent;

			public void ValidateTaskOrdinality()
			{
				ValidateCalculatedProperty(parent.TaskOrdinalityInfo);
			}

			public void ValidateTaskAssignmentType()
			{
				ValidateCalculatedProperty(parent.TaskAssignmentTypeInfo);
			}

			protected virtual void CheckTaskOrdinality()
			{
				MandatoryValidation.CheckEntered(parent.TaskOrdinalityInfo);
				ListValidation.ErrorIfInvalidCode(parent.TaskOrdinalityInfo);
			}

			protected virtual void CheckTaskAssignmentType()
			{
				MandatoryValidation.CheckEntered(parent.TaskAssignmentTypeInfo);
				ListValidation.ErrorIfInvalidCode(parent.TaskAssignmentTypeInfo);
			}

			public override void ValidateAll()
			{
				ValidateTaskOrdinality();
				ValidateTaskAssignmentType();
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
			TaskOrdinality = RandomString(MaxLength);
		}

#endif
		#endregion
	}
}

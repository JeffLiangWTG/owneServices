using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Module;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.NetworkVisualisation.Module
{
	public class NetworkDiagramFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var diagramTypeFilter = filters.AddTextFilter(BMNCNShape.ModuleFilterConstants.DiagramType, BMNCNShapeSchema.BNS_ShapeType, () => new DiagramShapeTypeList());
			diagramTypeFilter.MultilingualDescription = ResString.GetMultilingualString("NetworkVisualisation|NetworkDiagramFilterBusinessObject|DiagramType", "Shape Type");
			diagramTypeFilter.Visibility = ParentModule == null || ParentModule.AllowNew ? FilterVisibility.AlwaysApplied : FilterVisibility.Visible;
			diagramTypeFilter.DefaultProperty = ParentModule == null || ParentModule.AllowNew ? DiagramShapeTypeList.Codes.Diagram : DiagramShapeTypeList.Codes.Shape;

			filters.AddTextFilter(BMNCNShape.ModuleFilterConstants.Name, BMNCNShapeSchema.BNS_Name).MultilingualDescription = ResString.GetMultilingualString("NetworkVisualisation|NetworkDiagramFilterBusinessObject|Name", "Name");
			filters.AddTextFilter(BMNCNShape.ModuleFilterConstants.JobType, BMNCNShapeSchema.BNS_JobType, () => ObjectFactory.Get<IWorkflowDescriptorList>()).MultilingualDescription = ResString.GetMultilingualString("NetworkVisualisation|NetworkDiagramFilterBusinessObject|JobType", "Job Type");
			filters.AddTextFilter(BMNCNShape.ModuleFilterConstants.CompletionStatements, BMNCNShapeSchema.BNS_CompletionStatements).MultilingualDescription = ResString.GetMultilingualString("NetworkVisualisation|NetworkDiagramFilterBusinessObject|CompletionStatements", "Completion Statements");
			filters.AddTextFilter(BMNCNShape.ModuleFilterConstants.Status, AddStatusQuery, CreateStatusFilterList).MultilingualDescription = ResString.GetMultilingualString("NetworkVisualisation|NetworkDiagramFilterBusinessObject|Status", "Status");
			filters.AddGuidFilter(BMNCNShape.ModuleFilterConstants.LinkedEntity, ModuleIDs.ProcessHeader, BMNCNShapeSchema.BNS_RelatedEntityID, () => new ProcessJobHeaderCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("NetworkVisualisation|NetworkDiagramFilterBusinessObject|LinkedEntity", "Linked Entity");
			filters.AddGuidFilter(BMNCNShape.ModuleFilterConstants.ChildEntity, ModuleIDs.ProcessHeader, ChildEntityQuery, () => new ProcessHeaderCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("NetworkVisualisation|NetworkDiagramFilterBusinessObject|ChildEntity", "Contains Child Entity");
			filters.AddTextFilter(BMNCNShape.ModuleFilterConstants.ChildName, ChildNameQuery).MultilingualDescription = ResString.GetMultilingualString("NetworkVisualisation|NetworkDiagramFilterBusinessObject|ChildName", "Contains Child Name");
			filters.AddNkFilter(BMNCNShape.ModuleFilterConstants.ApprovedBy, BMNCNShapeSchema.BNS_GS_NKApprovedBy, ModuleIDs.GlbStaff, new GlbStaffCollection(Factory)).MultilingualDescription = ResString.GetMultilingualString("NetworkVisualisation|NetworkDiagramFilterBusinessObject|ApprovedBy", "Approved By");
			filters.AddTextFilter(BMNCNShape.ModuleFilterConstants.Scaled, ScaledQuery, () => new NetworkDiagramScaledFilterOptions()).MultilingualDescription = ResString.GetMultilingualString("NetworkVisualisation|NetworkDiagramFilterBusinessObject|Scaled", "Scaled");
			return filters;
		}

		#region Queries

		ZQuery AddStatusQuery(ZString value)
		{
			if (!Factory.GetCachedValue<ShapeStatusList>().ContainsCode(value) && !Factory.GetCachedValue<NetworkDiagramStatusFilterList>().ContainsCode(value))
			{
				return new ZQuery();
			}
			else
			{
				return GetQueryForValidStatus(value);
			}
		}

		static ZQuery GetQueryForValidStatus(ZString value)
		{
			var statusOfShapeQuery = GetShapeStatusQuery(value);

			if (ShapeStatusList.Codes.Unknown != value)
			{
				var statusOfHeaderQuery = GetWorkflowStatusQuery(value);

				if (statusOfHeaderQuery != null)
				{
					var statusOfShapesWithHeaderQuery = new ZDBOnlySubQuery(typeof(BMNCNShape), BMNCNShapeSchema.PK);
					statusOfShapesWithHeaderQuery.AddSubQuery(statusOfHeaderQuery, JoinCondition.And);
					statusOfShapeQuery.AddAsUnionQuery(statusOfShapesWithHeaderQuery);
				}
			}

			var query = new ZDBOnlyQuery(typeof(BMNCNShape));
			query.AddSubQuery(statusOfShapeQuery, JoinCondition.And);

			return query;
		}

		static ZDBOnlySubQuery GetShapeStatusQuery(ZString value)
		{
			var statusOfShapeQuery = new ZDBOnlySubQuery(typeof(BMNCNShape), BMNCNShapeSchema.PK);

			switch (value)
			{
				case NetworkDiagramStatusFilterList.Codes.Complete:
					statusOfShapeQuery.AddToFilter(BMNCNShapeSchema.BNS_Status, new[] { ShapeStatusList.Codes.Closed, ShapeStatusList.Codes.Cancelled });
					break;

				case NetworkDiagramStatusFilterList.Codes.NotComplete:
					statusOfShapeQuery.AddToFilter(BMNCNShapeSchema.BNS_Status, new[] { ShapeStatusList.Codes.Open, ShapeStatusList.Codes.Assigned, ShapeStatusList.Codes.Working, ShapeStatusList.Codes.Suspended });
					break;

				case NetworkDiagramStatusFilterList.Codes.NotSpecified:
					statusOfShapeQuery.AddToFilter(BMNCNShapeSchema.BNS_Status, ShapeStatusList.Codes.Unknown);
					break;

				default:
					statusOfShapeQuery.AddToFilter(BMNCNShapeSchema.BNS_Status, value);
					break;
			}

			statusOfShapeQuery.AddToFilter(BMNCNShapeSchema.BNS_RelatedEntityID, null);
			return statusOfShapeQuery;
		}

		static ZDBOnlySubQuery GetWorkflowStatusQuery(ZString value)
		{
			var statusOfHeaderQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), BMNCNShapeSchema.BNS_RelatedEntityID);

			switch (value)
			{
				case NetworkDiagramStatusFilterList.Codes.Complete:
					statusOfHeaderQuery.AddToFilter(ProcessHeaderSchema.FH_Status, WorkflowStatusList.Codes.Closed);
					break;

				case NetworkDiagramStatusFilterList.Codes.NotComplete:
					statusOfHeaderQuery.AddToFilter(ProcessHeaderSchema.FH_Status, new[] { WorkflowStatusList.Codes.Open, WorkflowStatusList.Codes.Blocked });

					break;

				case NetworkDiagramStatusFilterList.Codes.NotSpecified:
					statusOfHeaderQuery = null;
					break;

				case ShapeStatusList.Codes.Assigned:
					statusOfHeaderQuery.AddToFilter(TaskStatusFilter.GetFilter(TaskStatusAggregatorList.Codes.None, ProcessTaskStatusCodeList.Codes.Open));
					statusOfHeaderQuery.AddToFilter(TaskStatusFilter.GetFilter(TaskStatusAggregatorList.Codes.Any, ProcessTaskStatusCodeList.Codes.Assigned, ProcessTaskStatusCodeList.Codes.Suspended, ProcessTaskStatusCodeList.Codes.Working));
					break;

				case ShapeStatusList.Codes.Open:
					statusOfHeaderQuery.AddToFilter(TaskStatusFilter.GetFilter(TaskStatusAggregatorList.Codes.Any, ProcessTaskStatusCodeList.Codes.Open));
					break;

				case ShapeStatusList.Codes.Suspended:
					statusOfHeaderQuery.AddToFilter(TaskStatusFilter.GetFilter(TaskStatusAggregatorList.Codes.Any, ProcessTaskStatusCodeList.Codes.Suspended));
					break;

				case ShapeStatusList.Codes.Working:
					statusOfHeaderQuery.AddToFilter(TaskStatusFilter.GetFilter(TaskStatusAggregatorList.Codes.Any, ProcessTaskStatusCodeList.Codes.Working));
					break;

				case ShapeStatusList.Codes.Cancelled:
					statusOfHeaderQuery.AddToFilter(TaskStatusFilter.GetFilter(TaskStatusAggregatorList.Codes.All, ProcessTaskStatusCodeList.Codes.Cancelled));
					break;

				case ShapeStatusList.Codes.Closed:
					statusOfHeaderQuery.AddToFilter(TaskStatusFilter.GetFilter(TaskStatusAggregatorList.Codes.All, ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Cancelled));
					break;
			}

			return statusOfHeaderQuery;
		}

		ZQuery ChildEntityQuery(ZGuid entityPK)
		{
			var query = new ZDBOnlyQuery(typeof(BMNCNShape));
			var childShapeSubQuery = new ZDBOnlySubQuery(typeof(BMNCNShape), BMNCNShapeSchema.BNS_BNS_ParentShape);
			childShapeSubQuery.AddToFilter(BMNCNShapeSchema.BNS_RelatedEntityID, entityPK);

			query.AddSubQuery(childShapeSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery ChildNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(BMNCNShape));
			var childShapeSubQuery = new ZDBOnlySubQuery(typeof(BMNCNShape), BMNCNShapeSchema.BNS_BNS_ParentShape);
			var workflowSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), BMNCNShapeSchema.BNS_RelatedEntityID);

			childShapeSubQuery.AddToFilter(BMNCNShapeSchema.BNS_Name, comparisonOperator, value);
			workflowSubQuery.AddToFilter(ProcessHeaderSchema.FH_CompletionStatement, comparisonOperator, value);
			childShapeSubQuery.AddSubQuery(workflowSubQuery, JoinCondition.Or);

			query.AddSubQuery(childShapeSubQuery, JoinCondition.And);

			return query;
		}

		static ZQuery ScaledQuery(ZString value)
		{
			if (value.Equals(NetworkDiagramScaledFilterOptions.Codes.Both))
			{
				return new ZQuery();
			}

			var query = new ZDBOnlyQuery(typeof(BMNCNShape));
			var shouldUseNotInSubQuery = value == NetworkDiagramScaledFilterOptions.Codes.NonScaled;
			var subQuery = new ZDBOnlySubQuery(typeof(BMNCNSchedule), BMNCNScheduleSchema.BNC_BNS_Shape, notIn: shouldUseNotInSubQuery);
			subQuery.AddToFilter(BMNCNScheduleSchema.BNC_IsScaled, true);

			query.AddSubQuery(BMNCNShapeSchema.PK, BMNCNScheduleSchema.BNC_BNS_Shape, subQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Utils

		CodeDescriptionPairList CreateStatusFilterList()
		{
			return MergeInto(new NetworkDiagramStatusFilterList(), MakeEmptyItemList(), new ShapeStatusList());
		}

		CodeDescriptionPairList MakeEmptyItemList()
		{
			var emptyItemList = new CodeDescriptionPairList();
			emptyItemList.Add(new CodeDescriptionPair(string.Empty, ZString.Empty));

			return emptyItemList;
		}

		CodeDescriptionPairList MergeInto(CodeDescriptionPairList list, params CodeDescriptionPairList[] lists)
		{
			for (int i = 0; i < lists.Length; i++)
			{
				list.AddRange(lists[i]);
			}

			return list;
		}

		#endregion
	}
}

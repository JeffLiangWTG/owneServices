using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class AssignedResourceFilter : ModuleNkFilter
	{
		public AssignedResourceFilter(ZString description, ModuleIdentifier id, IBusinessObjectCollection list, FilterCategory category)
			: base(description, EmptyQuery, id, list)
		{
			this.Category = category;
			this.MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|ResourceOnAnyTask", "Resource Assigned To Any Task");
			EditOperatorList();
		}

		static ZQuery EmptyQuery(SQLComparisonOperator op, ZString nk)
		{
			throw new InvalidOperationException();
		}

		void EditOperatorList()
		{
			ComparisonOperator_List.RemoveCode(ComparisonConstants.NotEqual);
		}

		protected override ZQuery GetQuery()
		{
			var processHeaderQuery = new ZDBOnlyQuery(typeof(ProcessHeader));
			var processTaskSubQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_FH_ProcessHeader);
			processTaskSubQuery.AddToFilter(ProcessTasksSchema.P9_GS_NKAssignedStaffMember, SqlComparisonOperator, Property);

			if (BMFilterStripsHelper.ShouldOptimiseQueryForWorkflowOnly(this))
			{
				processHeaderQuery.AddSubQuery(processTaskSubQuery, JoinCondition.And);
			}
			else
			{
				var processHeaderSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.FH_FH_ParentHeader);
				processHeaderSubQuery.AddSubQuery(processTaskSubQuery, JoinCondition.And);
				processHeaderSubQuery.AddAsUnionQuery(processTaskSubQuery, addAsUnionAll: true);
				processHeaderQuery.AddSubQuery(processHeaderSubQuery, JoinCondition.And);
			}

			return processHeaderQuery;
		}
	}
}

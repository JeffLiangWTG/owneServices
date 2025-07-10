using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class AssignedResourceOnCurrentTaskFilter : ModuleNkFilter
	{
		public AssignedResourceOnCurrentTaskFilter(ZString description, ModuleIdentifier id, IBusinessObjectCollection list, FilterCategory category)
			: base(description, EmptyQuery, id, list)
		{
			Category = category;
			MultilingualDescription = ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|ResourceOnCurrentTask", "Resource Assigned To Startable Task");
			SupportsFiltersMatchComparisonOperator = true;
		}

		static ZQuery EmptyQuery(ZString nk)
		{
			throw new NotSupportedException();
		}

		public override bool HasComparisonOperator => true;

		protected override ZQuery GetQuery()
		{
			var parameters = new ZSqlParameterCollection();
			var staffSql = GetStaffSql(parameters);
			var staffOperator = GetStaffOperator();

			var getCurrentTasksInWorkflows = ObjectFactory.Get<IBMSQLFunctionHelper>().GetCurrentTasksInWorkflows;
			var additionalJobWorfklowsSql = BMFilterStripsHelper.ShouldOptimiseQueryForWorkflowOnly(this)
				? string.Empty
				: string.Format(CultureInfo.InvariantCulture, JobWorkflowsSql, getCurrentTasksInWorkflows, staffOperator, staffSql);
			var sql = string.Format(CultureInfo.InvariantCulture, BaseSql, getCurrentTasksInWorkflows, staffOperator, staffSql, additionalJobWorfklowsSql);

			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			query.AddFilterAndZSQLParameterCollection(sql, parameters);

			return query;
		}

		string GetStaffOperator()
		{
			switch (ComparisonOperator)
			{
				case ComparisonConstants.FiltersMatch:
					return "IN";
				case ComparisonConstants.CurrentUser:
				case ComparisonConstants.Exact:
				case ComparisonConstants.IsBlank:
					return "=";
				case ComparisonConstants.IsNotBlank:
				case ComparisonConstants.NotEqual:
					return "<>";
				default:
					return "=";
			}
		}

		string GetStaffSql(ZSqlParameterCollection parameters)
		{
			if (ComparisonOperator == ComparisonConstants.FiltersMatch)
			{
				return GetFiltersMatchSql(parameters);
			}

			var parameterName = "@StaffCode" + ZGuid.NewZGuid().ToString().Replace("-", string.Empty);
			parameters.Add(parameterName, Property, ProcessTasksSchema.P9_GS_NKAssignedStaffMember);

			return parameterName;
		}

		string GetFiltersMatchSql(ZSqlParameterCollection parameters)
		{
			var sql = BMFilterStripsHelper.GetFiltersMatchSql(this, "@StaffValue", parameters);

			return string.Format(CultureInfo.InvariantCulture, "SELECT {0} FROM {1} {2}{3}", GlbStaffSchema.Constants.GS_Code, GlbStaffSchema.Constants.TableName, sql.Item1, sql.Item2);
		}

		const string BaseSql = @"
FH_PK IN
(
    SELECT P9_FH_ProcessHeader
    FROM   {0}()
    WHERE  P9_GS_NKAssignedStaffMember {1} ({2}) {3}
)";

		const string JobWorkflowsSql = @"
    UNION ALL
    SELECT  FH_FH_ParentHeader
    FROM    {0}()
    JOIN    dbo.ProcessHeader ON FH_PK = P9_FH_ProcessHeader
    WHERE   P9_GS_NKAssignedStaffMember {1} ({2})
      AND   FH_FH_ParentHeader IS NOT NULL";
	}
}

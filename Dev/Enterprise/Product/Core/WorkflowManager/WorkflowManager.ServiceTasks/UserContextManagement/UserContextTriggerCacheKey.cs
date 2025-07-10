using System;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	class UserContextTriggerCacheKey : IEquatable<UserContextTriggerCacheKey>
	{
		public UserContextTriggerCacheKey(ZGuid parentId, ITriggerUserContextConditions conditions, WorkflowTriggerEventData eventData)
		{
			this.parentId = parentId;
			triggerContext = conditions.TriggerContextCode;
			branch = conditions.TriggerBranch;
			company = conditions.TriggerCompany;
			department = conditions.TriggerDepartment;
			staffCode = conditions.TriggerStaffCode;
			logStaff = eventData.ContextStaffCode;
			logBranch = eventData.ContextBranchCode;
			logDepartment = eventData.ContextDepartmentCode;
			hashCode = parentId.GetHashCode() ^ branch.GetHashCode() ^ company.GetHashCode() ^ department.GetHashCode() ^ triggerContext.GetHashCode() ^ staffCode.GetHashCode() ^ logStaff.GetHashCode() ^ logBranch.GetHashCode() ^ logDepartment.GetHashCode();
		}

		readonly ZGuid parentId, branch, company, department;
		readonly ZString triggerContext, staffCode, logStaff, logBranch, logDepartment;
		readonly int hashCode;

		public bool Equals(UserContextTriggerCacheKey other)
		{
			return parentId.Equals(other.parentId) &&
				branch.Equals(other.branch) &&
				company.Equals(other.company) &&
				department.Equals(other.department) &&
				triggerContext.Equals(other.triggerContext) &&
				staffCode.Equals(other.staffCode) &&
				logStaff.Equals(other.logStaff) &&
				logBranch.Equals(other.logBranch) &&
				logDepartment.Equals(other.logDepartment);
		}

		public override bool Equals(object obj) => obj is UserContextTriggerCacheKey other && Equals(other);
		public override int GetHashCode() => hashCode;
	}
}

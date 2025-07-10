using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker
{
	public class UserContextSwitchingLogBatcher : LogBatcher<UserContextSwitcher>
	{
		public UserContextSwitchingLogBatcher(INotifications notifications, string subscriberName)
		{
			this.notifications = notifications;
			this.subscriberName = subscriberName;
		}

		readonly string subscriberName;

		readonly INotifications notifications;

		protected override void AddGroupingFetchHints(IEnumerable<IQueuedLog> queuedItems)
		{
			var factory = queuedItems.First().Factory;
			foreach (var queueItem in queuedItems)
			{
				factory.AddFetchHint(typeof(GlbStaff), GetUserQuery(queueItem.SJ_GS_NKUser));
				factory.AddFetchHint(typeof(GlbBranch), GetBranchQuery(queueItem.SJ_GB_NKBranch));
				factory.AddFetchHint(typeof(GlbDepartment), GetDepartmentQuery(queueItem.SJ_GE_NKDepartment));
			}
		}

		protected override LogsGroupContext SetContextForLogsGroup(UserContextSwitcher userContextSwitcher, IEnumerable<IQueuedLog> queuedLogs)
		{
			return new LogsGroupContext(false, userContextSwitcher.Set(notifications));
		}

		protected override UserContextSwitcher GetGroupLogKey(IQueuedLog queueItem)
		{
			var factory = queueItem.Factory;
			var user = factory.LoadTop1<GlbStaff>(GetUserQuery(queueItem.SJ_GS_NKUser)) ?? GlbStaff.GetCurrentUser(factory);
			var branch = factory.LoadTop1<GlbBranch>(GetBranchQuery(queueItem.SJ_GB_NKBranch)) ?? GlbBranch.GetCurrentBranch(factory);
			var dept = factory.LoadTop1<GlbDepartment>(GetDepartmentQuery(queueItem.SJ_GE_NKDepartment)) ?? GlbDepartment.GetCurrentDepartment(factory);
			return UserContextSwitcher.Build(user, branch, dept, subscriberName);
		}

		static ZQuery GetUserQuery(ZString staffCode)
		{
			return new ZQuery(GlbStaffSchema.GS_Code, staffCode);
		}

		static ZQuery GetBranchQuery(ZString branchCode)
		{
			return new ZQuery(GlbBranchSchema.GB_Code, branchCode);
		}

		static ZQuery GetDepartmentQuery(ZString deptCode)
		{
			return new ZQuery(GlbDepartmentSchema.GE_Code, deptCode);
		}
	}
}

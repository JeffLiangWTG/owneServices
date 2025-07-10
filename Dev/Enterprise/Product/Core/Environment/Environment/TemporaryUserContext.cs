using System;
using System.Runtime.CompilerServices;
using CargoWise.Common;

namespace Enterprise.Environment
{
	public class TemporaryUserContext
	{
		public TemporaryUserContext()
		{
			StaffLoginName = CurrentUserLoginName;
			BranchPK = CurrentBranchPK;
			DepartmentPK = CurrentDepartmentPK;
		}

		public string StaffLoginName { get; set; }
		public Guid BranchPK { get; set; }
		public Guid DepartmentPK { get; set; }

		public IDisposable Set([CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			return
				StaffLoginName != CurrentUserLoginName || BranchPK != CurrentBranchPK || DepartmentPK != CurrentDepartmentPK
					? Env.Instance.SetTemporaryUserContext(new UserContext(StaffLoginName, BranchPK, DepartmentPK), callerFilePath, callerMemberName, callerLineNumber)
					: DisposableAction.NoAction;
		}

		static string CurrentUserLoginName
		{
			get { return Env.CurrentUser != null ? Env.CurrentUser.LoginName : string.Empty; }
		}

		static Guid CurrentBranchPK
		{
			get { return Env.CurrentBranch != null ? Env.CurrentBranch.PK : Guid.Empty; }
		}

		static Guid CurrentDepartmentPK
		{
			get { return Env.CurrentDepartment != null ? Env.CurrentDepartment.PK : Guid.Empty; }
		}
	}
}

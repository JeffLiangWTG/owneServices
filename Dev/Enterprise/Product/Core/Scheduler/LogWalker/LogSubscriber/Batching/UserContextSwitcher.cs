using System;
using System.Runtime.CompilerServices;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.LogWalker
{
	[Immutable]
	public class UserContextSwitcher : IEquatable<UserContextSwitcher>
	{
		#region Statics

		public static UserContextSwitcher Build(WorkflowUserContext workflowUserContext, string message)
		{
			return Build(workflowUserContext.Staff, workflowUserContext.Branch, workflowUserContext.Department, message);
		}

		public static UserContextSwitcher Build(GlbStaff staff, GlbBranch branch, GlbDepartment department, string message)
		{
			bool Exists(BusinessObject bizo) => bizo?.IsInDatabase ?? false;
			if (Exists(staff) && Exists(branch) && Exists(department))
			{
				return new UserContextSwitcher(staff.PK, branch, department.PK, message);
			}
			else
			{
				return NullSwitcher;
			}
		}

		public static UserContextSwitcher NullSwitcher { get; } = new UserContextSwitcher();

		#endregion

		UserContextSwitcher(ZGuid staff, GlbBranch branch, ZGuid department, string message)
		{
			this.staffPk = staff.IsValid ? staff : throw new ArgumentException("ZGuid must be valid", nameof(staff));
			this.branchPk = branch?.PK ?? throw new ArgumentNullException(nameof(branch));
			this.BranchCode = branch.GB_Code;
			this.departmentPk = department.IsValid ? department : throw new ArgumentException("ZGuid must be valid", nameof(department));
			this.message = !string.IsNullOrEmpty(message) ? message : throw new ArgumentNullException(nameof(message));
			hashCode = staff.GetHashCode() ^ branch.GetHashCode() ^ department.GetHashCode();
		}

		UserContextSwitcher()
		{
			isNull = true;
		}

		readonly string message;
		readonly ZGuid staffPk;
		readonly ZGuid branchPk;
		readonly ZGuid departmentPk;
		readonly int hashCode;
		readonly bool isNull;

		public ZGuid BranchPK => branchPk;

		public ZString BranchCode { get; }
		public ZGuid StaffPK => staffPk;

		public ZGuid DepartmentPK => departmentPk;

		public override bool Equals(object obj) => obj is UserContextSwitcher s && Equals(s);
		public bool Equals(UserContextSwitcher other) => staffPk.Equals(other.staffPk) && branchPk.Equals(other.branchPk) && departmentPk.Equals(other.departmentPk);
		public override int GetHashCode() => hashCode;

		public IDisposable Set(INotifications notifier,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1)
		{
			if (!string.IsNullOrEmpty(message) && notifier != null && WorkflowDataRegistry.Instance.EnableUserContextChangeLogging.Value)
			{
				notifier.Add(new InfoNotification(message));
			}

			if (!isNull)
			{
				using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
				{
					return Env.Instance.SetTemporaryUserContext(staffPk.ToGuid(), branchPk.ToGuid(), departmentPk.ToGuid(), callerFilePath, callerMemberName, callerLineNumber);
				}
			}
			else
			{
				return null;
			}
		}
	}
}

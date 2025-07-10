using System;
using System.Runtime.CompilerServices;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Environment
{
	public interface IEnvironment
	{
		event EventHandler<IUserContextChangingEventArgs> UserContextChanging;

		string ApplicationStartupPath { get; }
		string CurrentNKUNLOCO { get; }
		string ServiceTaskCode { get; }
		IBranch CurrentBranch { get; }
		ICompany CurrentCompany { get; }
		IDepartment CurrentDepartment { get; }
		IUser CurrentUser { get; }
		bool IsLoggedIn { get; }
		bool IsAuthenticated { get; }
		bool IsValidLogon { get; }
		string GlobalFormTopCaption { get; }
		bool IsWeb { get; }
		bool IsWebService { get; }
		bool IsProductionSystem { get; }
		ILicenceProxy Licence { get; }
		IUserLoginController LoginController { get; }
		NumberFountains NumberFountains { get; }
		IOutgoingMailManager OutgoingMailManager { get; }
		IOutgoingCustomsMailManager OutgoingCustomsMailManager { get; }
		DataRegistry Registry { get; }
		ISecurityProxy Security { get; }
		string TempPath { get; }
		TimeFactory Time { get; }
		ISemaphoreProvider SemaphoreProvider { get; }
		IUserContext CurrentUserContext { get; }
		Guid CurrentBranchPK { get; }
		Guid CurrentDepartmentPK { get; }
		IDbUpgradeCaptions DbUpgradeCaptions { get; }

		void SetUserContext(
			IUserContext userContext,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1);
		void ClearUserContext(
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1);
		IUserContext NewUserContext(
			string staffLoginName,
			Guid branchPK,
			Guid departmentPK);
		IDisposable SetTemporaryUserContext(
			IUserContext userContext,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1);
		IDisposable SetTemporaryUserContext(
			string staffLoginName,
			Guid branchPK,
			Guid departmentPK,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1);
		IDisposable SetTemporaryUserContext(
			Guid staffPK,
			Guid branchPK,
			Guid departmentPK,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1);
		IDisposable SetTemporaryMasterUserContext(
			string staffLoginName,
			Guid branchPK,
			Guid departmentPK,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1);
		IDisposable TemporaryServiceTaskContext(
			string code,
			bool canRunInAnyBranch,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1);

		IDisposable EnsureUserContextIsRestoredAfterThis();
		IDisposable SuppressSwitchContextCheck(bool ensureContextIsRestoredAfterSuppression = true);

		IDisposable SuspendBranchAccessError();

		string GetTempFileName();
		string GetTempFileName(string directoryName);
		string GetTempFileName(string directoryName, string extension);

		void CleanupUserContextOnCurrentThread();
		void ExitApplication();
	}

#if DEBUG
	public interface IEnvironmentForTest
	{
		bool IsSecurityCreatedForTest { get; }
		void ResetSecurityForTest();
		IDisposable SetTemporaryMasterUserContext(
			IUserContext userContext,
			[CallerFilePath] string callerFilePath = "",
			[CallerMemberName] string callerMemberName = "",
			[CallerLineNumber] int callerLineNumber = -1);
	}
#endif

	public interface IUserContext
	{
		IBranch Branch { get; }
		ICompany Company { get; }
		IDepartment Department { get; }
		IUser User { get; }
		ILicenceProxy Licence { get; }
		LoginAuthenticationInfo LoginAuthenticationInfo { get; }

		IUserContext ThreadSafeClone();

		void EnsureCurrentThreadIsOwner();
	}

	public interface IUserContextChangingEventArgs
	{
		IUserContext OldUserContext { get; }
		IUserContext NewUserContext { get; }
		bool SetCurrentThreadContext { get; }
		bool IsRevert { get; }
		string CallerFilePath { get; }
		string CallerMemberName { get; }
		int CallerLineNumber { get; }
	}

	public struct UserContextInfo
	{
		public UserContextInfo(IUserContext context)
		{
			BranchCode = context.Branch?.Code;
			CompanyCode = context.Company?.Code;
			UserName = context.User?.LoginName;
		}

		public UserContextInfo(string branchCode, string companyCode, string userName)
		{
			BranchCode = branchCode;
			CompanyCode = companyCode;
			UserName = userName;
		}

		public string BranchCode { get; }
		public string CompanyCode { get; }
		public string UserName { get; }

		public bool Equals(UserContextInfo rhs)
		{
			return (BranchCode?.Equals(rhs.BranchCode, StringComparison.OrdinalIgnoreCase) ?? rhs.BranchCode == null) &&
					(CompanyCode?.Equals(rhs.CompanyCode, StringComparison.OrdinalIgnoreCase) ?? rhs.CompanyCode == null) &&
					(UserName?.Equals(rhs.UserName, StringComparison.OrdinalIgnoreCase) ?? rhs.UserName == null);
		}

		public override bool Equals(object obj)
		{
			return obj is UserContextInfo rhs && Equals(rhs);
		}

		public override int GetHashCode()
		{
			unchecked // Overflow is fine, just wrap
			{
				var hash = 17;
				if (BranchCode != null)
				{
					hash = hash * 23 + BranchCode.GetHashCode();
				}

				if (CompanyCode != null)
				{
					hash = hash * 23 + CompanyCode.GetHashCode();
				}

				if (UserName != null)
				{
					hash = hash * 23 + UserName.GetHashCode();
				}

				return hash;
			}
		}

		public static bool operator ==(UserContextInfo lhs, UserContextInfo rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(UserContextInfo lhs, UserContextInfo rhs)
		{
			return !lhs.Equals(rhs);
		}
	}
}

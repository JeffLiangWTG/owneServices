using System;
using System.Diagnostics;
using CargoWise.Common;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Environment
{
	class UserContextSecurity
	{
		int currentThreadContextRefCount;

		public UserContextSecurity()
		{
			SetMasterUserContext(new UserContext());
		}

		public IUserContext Context
		{
			get
			{
				return context;
			}
		}
		IUserContext context;

		public void SetMasterUserContext(IUserContext userContext)
		{
			SetContext(userContext);
		}

		public void SetCurrentThreadUserContext(IUserContext userContext, bool isRevert)
		{
			SetContext(userContext);
			currentThreadContextRefCount = isRevert ? currentThreadContextRefCount - 1 : currentThreadContextRefCount + 1;

			if (currentThreadContextRefCount < 0)
			{
				currentThreadContextRefCount = 0;
				var key = string.Format(Culture.Invariant, (NoResString)"Tried to revert UserContext more times than it was set.  CompanyCode: {0}, BranchCode: {1}, UserInitials: {2}.", context.Company?.Code, context.Branch?.Code, context.User?.Initials);
				ErrorReporter.ReportOnce(key, lastContextUpdateStackTrace.ToString());
			}
		}

		void SetContext(IUserContext userContext)
		{
			Argument.NotNull(userContext, nameof(userContext));
			userContext.EnsureCurrentThreadIsOwner();
			context = userContext;
			Enterprise.ZArchitecture.Core.Culture.fCurrentCompanyCountryCulture = null;
			if (security != null)
			{
				security.ResetData(null,
					context.User,
					context.Branch?.PK ?? Guid.Empty,
					context.Department?.PK ?? Guid.Empty,
					context.Company?.PK ?? Guid.Empty);

				security.LockDownUserCompanyDepartmentBranch();
			}

			lastContextUpdateStackTrace = new StackTrace();
		}

		public bool ContextIsOverriden => currentThreadContextRefCount > 0;
		public StackTrace LastContextUpdateStackTrace => lastContextUpdateStackTrace;

		public SecurityCore Security
		{
			get
			{
				if (security == null && !isCurrentlyCreatingSecurity)
				{
					isCurrentlyCreatingSecurity = true;

					try
					{
						security = GetSecurity(Context);
					}
					finally
					{
						isCurrentlyCreatingSecurity = false;
					}
				}

				security?.EnsureCurrentThreadIsOwner();
				Context.EnsureCurrentThreadIsOwner();
				return security;
			}
			set
			{
				security = Argument.NotNull(value, nameof(value));
				security.EnsureCurrentThreadIsOwner();
			}
		}
		SecurityCore security;
		bool isCurrentlyCreatingSecurity;
		StackTrace lastContextUpdateStackTrace;

		static SecurityCore GetSecurity(IUserContext userContext)
		{
			var userPk = userContext.User?.PK ?? Guid.Empty;
			var branchPk = userContext.Branch?.PK ?? Guid.Empty;
			var departmentPk = userContext.Department?.PK ?? Guid.Empty;
			var companyPk = userContext.Company?.PK ?? Guid.Empty;

			SecurityCore result;
			using (PerformanceStatisticsCollector.StartMonitoring("SetSecurity"))
			{
				result = new SecurityCore(null, userPk, branchPk, departmentPk, companyPk);
			}
#if DEBUG
			result.HookClientHookChanged();
#endif
			result.LockDownUserCompanyDepartmentBranch();
			return result;
		}

#if DEBUG
		public bool IsSecurityCreatedForTest => security != null;

		public void ResetSecurityForTest()
		{
			security = null;
		}
#endif
	}
}

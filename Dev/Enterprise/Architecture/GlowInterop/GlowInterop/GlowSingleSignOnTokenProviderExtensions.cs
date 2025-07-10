using System;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GlowInterop
{
	public static class GlowSingleSignOnTokenProviderExtensions
	{
		public static string CreateLimitedToken(this IGlowSingleSignOnTokenProvider provider)
			=> CreateLimitedTokenCore(provider, Env.CurrentUserPK, UserType.Staff);

		public static string CreateLimitedTokenForContact(this IGlowSingleSignOnTokenProvider provider, Guid contactPK)
			=> CreateLimitedTokenCore(provider, contactPK, UserType.Contact);

		public static string CreateCaptiveLimitedToken(this IGlowSingleSignOnTokenProvider provider, Guid? userPK = null, Guid? branch = null, Guid? department = null)
			=> CreateLimitedTokenCore(provider, userPK ?? Env.CurrentUserPK, UserType.Staff, isCaptiveSession: true, branch: branch, department: department);

		public static string CreateLimitedTokenSpecificBranch(this IGlowSingleSignOnTokenProvider provider, Guid? branch)
			=> CreateLimitedTokenCore(provider, Env.CurrentUserPK, UserType.Staff, isCaptiveSession: true, branch: branch);

		static string CreateLimitedTokenCore(IGlowSingleSignOnTokenProvider provider, Guid userPK, UserType userType, bool isCaptiveSession = false, Guid? branch = null, Guid? department = null)
		{
			var tablePrefix = userType == UserType.Contact ? OrgContactSchema.Constants.Prefix : GlbStaffSchema.Constants.Prefix;
			var currentUserLoginToken = Env.CurrentUser?.LoginToken;
			var tokenOptions = new GlowSingleSignOnTokenOptions
			{
				BranchPK = branch ?? Env.CurrentBranchPK,
				DepartmentPK = department ?? Env.CurrentDepartmentPK,
				IsCaptiveSession = isCaptiveSession,
				LoggedInWithSupportToken = currentUserLoginToken?.LoggedInWithSupportToken ?? false,
				SupportUserCode = currentUserLoginToken?.SupportTokenUserCode,
				SupportUserName = currentUserLoginToken?.SupportTokenUserName,
			};
			return provider.CreateLimitedToken(userPK, tablePrefix, tokenOptions);
		}

		enum UserType
		{
			Contact,
			Staff,
		}
	}
}

using System;
using CargoWise.Application;
using CargoWise.Authentication.Primitives;
using CargoWise.Definitions.Authentication;
using Newtonsoft.Json;

namespace Enterprise.ZArchitecture.GlowInterop
{
	public interface IGlowSingleSignOnTokenProvider
	{
		string CreateLimitedToken(Guid userPK, string parentTableCode, GlowSingleSignOnTokenOptions options);
	}

	public class GlowSingleSignOnTokenProvider : IGlowSingleSignOnTokenProvider
	{
		public GlowSingleSignOnTokenProvider()
		{
			tokenAccessControl = ObjectFactory.Get<ITokenizedAccessControl>();
		}

		readonly ITokenizedAccessControl tokenAccessControl;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Token")]
		public string CreateLimitedToken(Guid userPK, string parentTableCode, GlowSingleSignOnTokenOptions options)
		{
			var scope = new LocalIdentityTokenScope();
			scope.BranchKey = options.BranchPK;
			scope.DepartmentKey = options.DepartmentPK;
			if (options.IsCaptiveSession)
			{
				scope.IsCaptiveSession = options.IsCaptiveSession;
			}
			if (!string.IsNullOrEmpty(options.IPRestriction))
			{
				scope.IPAddressRestriction = options.IPRestriction;
			}

			if (options.LoggedInWithSupportToken)
			{
				scope.ExternalUserInfo = new ExternalUserInfo
				{
					UserCode = options.SupportUserCode,
					UserName = options.SupportUserName,
				};
			}

			return tokenAccessControl.CreateLimitedToken(AccessTokenTypes.LocalIdentity, new AccessTokenInfo(JsonConvert.SerializeObject(scope), userPK, parentTableCode), TimeSpan.FromMinutes(5), 1);
		}
	}

	public struct GlowSingleSignOnTokenOptions
	{
		public Guid BranchPK;
		public Guid DepartmentPK;
		public bool IsCaptiveSession;
		public string IPRestriction;
		public bool LoggedInWithSupportToken;
		public string SupportUserCode;
		public string SupportUserName;
	}
}

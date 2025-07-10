using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public static class UserSessionHelper
	{
		public static string GenerateSessionString(OrgContactWebUser user)
		{
			var result = "";
			if (user != null)
			{
				var secureQueryString = new SecureQueryString();
				var webContact = user.LoggedInWebContact;
				secureQueryString[LoggedInOrganisationKey] = webContact?.OC_OH.ToString() ?? "";
				secureQueryString[LoggedInUserKey] = webContact?.PK.ToString() ?? "";
				secureQueryString[IsSuperUserKey] = user.IsSuperUser.ToString();
				secureQueryString[SupportStaffCodeKey] = user.SupportStaffCode;
				result = secureQueryString.ToString();
			}

			return result;
		}

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults")]
		public static UserSession DecodeSessionString(string sessionString)
		{
			var session = new UserSession();
			if (!string.IsNullOrEmpty(sessionString))
			{
				var secureQueryString = new SecureQueryString(sessionString);
				ZGuid.TryParse(secureQueryString[LoggedInOrganisationKey], out session.LoggedInOrganisationPK);
				ZGuid.TryParse(secureQueryString[LoggedInUserKey], out session.LoggedInUserPK);
				ZBool.TryParse(secureQueryString[IsSuperUserKey], out session.IsSuperUser);
				session.SupportStaffCode = secureQueryString[SupportStaffCodeKey];
			}

			return session;
		}

		const string LoggedInOrganisationKey = "LoggedInOrganisation";
		const string LoggedInUserKey = "LoggedInUser";
		const string IsSuperUserKey = "IsSuperUser";
		const string SupportStaffCodeKey = "SupportStaffCode";

		[SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "Not (yet) needed.")]
		public struct UserSession
		{
			public ZGuid LoggedInOrganisationPK;
			public ZGuid LoggedInUserPK;
			public ZBool IsSuperUser;
			public ZString SupportStaffCode;
		}
	}
}

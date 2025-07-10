using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions.Authentication;
using Enterprise.Environment;
using static CargoWise.Definitions.Authentication.SupportLogonRole;
using static Enterprise.Client.EDI.EDISecurityCheckpoints.Constants;

namespace Enterprise.Client.EDI
{
	public static class EDISecurityCheckPointRoleMapper
	{
		static readonly IReadOnlyDictionary<SupportLogonRole, string> userRoleToCheckpointMap = new Dictionary<SupportLogonRole, string>
		{
			{ new SupportLogonRole(UserType.CW1, DatabaseType.Test, SystemType.Internal), GlowSupportLogonToInternalNonProductionSystemAsSuperUser },
			{ new SupportLogonRole(UserType.CW1, DatabaseType.Production, SystemType.Internal), GlowSupportLogonToInternalProductionSystemAsSuperUser },
			{ new SupportLogonRole(UserType.Diagnostic, DatabaseType.Test, SystemType.Internal), GlowSupportLogonToInternalNonProductionSystemAsDiagnosticsUser },
			{ new SupportLogonRole(UserType.Diagnostic, DatabaseType.Production, SystemType.Internal), GlowSupportLogonToInternalProductionSystemAsDiagnosticsUser },
			{ new SupportLogonRole(UserType.CW1, DatabaseType.Test, SystemType.External), GlowSupportLogonToExternalNonProductionSystemAsSuperUser },
			{ new SupportLogonRole(UserType.CW1, DatabaseType.Production, SystemType.External),GlowSupportLogonToExternalProductionSystemAsSuperUser },
			{ new SupportLogonRole(UserType.Diagnostic, DatabaseType.Test, SystemType.External), GlowSupportLogonToExternalNonProductionSystemAsDiagnosticsUser },
			{ new SupportLogonRole(UserType.Diagnostic, DatabaseType.Production, SystemType.External),  GlowSupportLogonToExternalProductionSystemAsDiagnosticsUser },
		};

		public static string[] GetInternalUserRoles()
		{
			return GetUserRoles(null, null, SystemType.Internal);
		}

		public static string[] GetExternalSuperUserRoles()
		{
			return GetUserRoles(UserType.CW1, null, SystemType.External);
		}

		public static string[] GetExternalDiagnosticUserRoles()
		{
			return GetUserRoles(UserType.Diagnostic, null, SystemType.External);
		}

		static string[] GetUserRoles(UserType? userType, DatabaseType? databaseType, SystemType? systemType)
		{
			var filteredMap = userRoleToCheckpointMap
				.Where(pair =>
					(userType == null || userType.Equals(pair.Key.User)) &&
					(databaseType == null || databaseType.Equals(pair.Key.Database)) &&
					(systemType == null || systemType.Equals(pair.Key.System))
				);

			var roles = new List<string>();

			foreach (var kvp in filteredMap)
			{
				var checkPoint = Env.Security.FindCheckPoint(kvp.Value);
				if (checkPoint != null && checkPoint.IsAllowed)
				{
					roles.Add(kvp.Key.GetRole());
				}
			}
			return roles.ToArray();
		}
	}
}

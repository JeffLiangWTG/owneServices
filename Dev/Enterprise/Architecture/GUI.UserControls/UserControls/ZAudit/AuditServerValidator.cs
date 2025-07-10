using System;

namespace Enterprise.ZArchitecture.GUI
{
	public static class AuditServerValidator
	{
		public static bool AuditHasValidServerHost(string auditServer)
		{
			var baseUrl = auditServer?.Split('\\')[0].Split(',')[0] ?? string.Empty;

			return !string.IsNullOrWhiteSpace(auditServer) && Uri.CheckHostName(baseUrl) != UriHostNameType.Unknown;
		}
	}
}

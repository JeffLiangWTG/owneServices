using SV = Enterprise.DbUpgrader.Resource.SchemaVersion;

namespace Enterprise.DbUpgrader.Resource.Version
{
	public static class SchemaVersion
	{
		public static readonly VersionLabel Application = new VersionLabel(SV.ApplicationMajor, SV.ApplicationMinor);
		public static readonly VersionLabel DocManager = new VersionLabel(SV.DocumentMajor, SV.DocumentMinor);
	}
}

using SV = Enterprise.DbUpgrader.Resource.ScriptVersion;

namespace Enterprise.DbUpgrader.Resource.Version
{
	public static class ScriptVersion
	{
		public static readonly VersionLabel Application = new VersionLabel(SV.Major, SV.Minor);
	}
}

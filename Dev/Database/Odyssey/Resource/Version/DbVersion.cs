using CargoWise.Database.Abstractions;

namespace Enterprise.DbUpgrader.Resource.Version
{
	public sealed class DbVersion : IDatabaseAspectVersions
	{
		public VersionLabel SchemaVersion => Version.SchemaVersion.Application;
		public VersionLabel ScriptVersion => Version.ScriptVersion.Application;
		public VersionLabel TransformationVersion => Version.TransformationVersion.ApplicationNumber;
	}
}

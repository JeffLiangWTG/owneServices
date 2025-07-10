using Enterprise.DbUpgrader.Resource.Version;

namespace CargoWise.Database.Abstractions
{
	public interface IDatabaseAspectVersions
	{
		VersionLabel SchemaVersion { get; }
		VersionLabel ScriptVersion { get; }
		VersionLabel TransformationVersion { get; }
	}
}

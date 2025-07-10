using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;

namespace Enterprise.DbUpgrader.Startup
{
	class ForceSchemaScriptUpgradeVersionInfo : IVersionChangeInfo
	{
		public ForceSchemaScriptUpgradeVersionInfo()
		{
			DbReferenceVersion_Schema = new VersionLabel(Env.Registry.DatabaseMajorSchemaVersion, Env.Registry.DatabaseMinorSchemaVersion);
			DbReferenceVersion_Script = new VersionLabel(Env.Registry.DatabaseMajorScriptVersion, Env.Registry.DatabaseMinorScriptVersion);
			DbReferenceVersion_Transformation = new VersionLabel(Env.Registry.DatabaseMajorTransformationVersion, Env.Registry.DatabaseMinorTransformationVersion);
			DbReferenceVersion_Data = new VersionLabel(Env.Registry.DatabaseSystemDataVersionMajor, Env.Registry.DatabaseSystemDataVersionMinor);
			DbReferenceVersion_Clr = new VersionLabel(Env.Registry.DatabaseMajorClrAssembliesVersion, Env.Registry.DatabaseMinorClrAssembliesVersion);
		}

		public VersionLabel DbReferenceVersion_Data { get; }
		public VersionLabel DbReferenceVersion_Schema { get; }
		public VersionLabel DbReferenceVersion_Script { get; }
		public VersionLabel DbReferenceVersion_Transformation { get; }
		public VersionLabel DbReferenceVersion_Clr { get; }

		public virtual bool IsRequired_Data => (DataVersion.Application.CompareTo(DbReferenceVersion_Data) != 0);
		public bool IsRequired_Transformation => (TransformationVersion.ApplicationNumber.CompareTo(DbReferenceVersion_Transformation) != 0);
		public bool IsRequired_Schema => true;
		public bool IsRequired_Script => true;
		public bool IsRequired_ClientDocuments => false;
	}
}

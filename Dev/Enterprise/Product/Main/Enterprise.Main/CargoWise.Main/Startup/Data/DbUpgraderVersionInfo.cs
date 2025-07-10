using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Startup
{
	class DbUpgraderVersionInfo : IVersionChangeInfo
	{
		public DbUpgraderVersionInfo()
			: this(null)
		{ }

		public DbUpgraderVersionInfo(UpgradeInfo softwareUpgrade)
		{
			InitialiseDbReferenceVersion(
				Env.Registry.DatabaseMajorSchemaVersion, Env.Registry.DatabaseMinorSchemaVersion,
				Env.Registry.DatabaseMajorScriptVersion, Env.Registry.DatabaseMinorScriptVersion,
				Env.Registry.DatabaseMajorTransformationVersion, Env.Registry.DatabaseMinorTransformationVersion,
				Env.Registry.DatabaseSystemDataVersionMajor, Env.Registry.DatabaseSystemDataVersionMinor,
				Env.Registry.DatabaseMajorClrAssembliesVersion, Env.Registry.DatabaseMinorClrAssembliesVersion);

			InitializeAvailableClientDocuments(softwareUpgrade);
			ResetTranformationVersionIfRequired(softwareUpgrade);
		}

		protected void InitialiseDbReferenceVersion(
			int schemaMajorFromDb, int schemaMinorFromDb,
			int scriptMajorFromDb, int scriptMinorFromDb,
			int transformationMajorVersionFromDb, int transformationMinorVersionFromDb,
			int dataVersionMajorFromDb, int dataVersionMinorFromDb,
			int clrVersionMajorFromDb, int clrVersionMinorFromDb)
		{
			dbReferenceVersion_Schema = new VersionLabel(schemaMajorFromDb, schemaMinorFromDb);
			dbReferenceVersion_Script = new VersionLabel(scriptMajorFromDb, scriptMinorFromDb);
			dbReferenceVersion_Transformation = new VersionLabel(transformationMajorVersionFromDb, transformationMinorVersionFromDb);
			dbReferenceVersion_Data = new VersionLabel(dataVersionMajorFromDb, dataVersionMinorFromDb);
			dbReferenceVersion_Clr = new VersionLabel(clrVersionMajorFromDb, clrVersionMinorFromDb);

			appToDbVersionComparison_Schema = SchemaVersion.Application.CompareTo(dbReferenceVersion_Schema);
			isMajoDiff_Schema = SchemaVersion.Application.IsMajorDiff(dbReferenceVersion_Schema.Major);
		}

		void InitializeAvailableClientDocuments(UpgradeInfo softwareUpgrade)
		{
			IsRequired_ClientDocuments = softwareUpgrade != null && !string.IsNullOrEmpty(DataRegistry.Instance.ClientDocumentName);
		}

		void ResetTranformationVersionIfRequired(UpgradeInfo softwareUpgrade)
		{
			if (softwareUpgrade != null && softwareUpgrade.Version > ReleaseInfo.Instance.VersionNumber.ToVersion() && TransformationVersion.ApplicationNumber.CompareTo(dbReferenceVersion_Transformation) < 0)
			{
				dbReferenceVersion_Transformation = new VersionLabel(0, 0);
				Env.Registry.DatabaseMajorTransformationVersion = 0;
				Env.Registry.DatabaseMinorTransformationVersion = 0;
			}
		}

		protected VersionLabel dbReferenceVersion_Data;
		protected VersionLabel dbReferenceVersion_Schema;
		protected VersionLabel dbReferenceVersion_Script;
		protected VersionLabel dbReferenceVersion_Transformation;
		protected VersionLabel dbReferenceVersion_Clr;

		int? appToDbVersionComparison_Schema;
		bool? isMajoDiff_Schema;

		public bool IsMajorVersionDowngradeAttempt
		{
			get
			{
				bool result = (
					appToDbVersionComparison_Schema < 0
					&& (bool)isMajoDiff_Schema);
				return result;
			}
		}

		internal bool IsCurrentMajorVersionTooOld => dbReferenceVersion_Schema.Major < DataRegistry.MinUpgradableDbMajorSchemaVersion
																									&& dbReferenceVersion_Schema.CompareTo(0, 0) > 0;

		internal bool IsCurrentTransformVersionTooOld => dbReferenceVersion_Transformation.Major < DataRegistry.MinDatabaseMajorTransformationVersion
																										 && dbReferenceVersion_Transformation.CompareTo(0, 0) > 0;

		#region IUpgradeStartupInfo Members

		VersionLabel IVersionChangeInfo.DbReferenceVersion_Data => dbReferenceVersion_Data;
		VersionLabel IVersionChangeInfo.DbReferenceVersion_Schema => dbReferenceVersion_Schema;
		VersionLabel IVersionChangeInfo.DbReferenceVersion_Script => dbReferenceVersion_Script;
		VersionLabel IVersionChangeInfo.DbReferenceVersion_Transformation => dbReferenceVersion_Transformation;
		public VersionLabel DbReferenceVersion_Clr => dbReferenceVersion_Clr;

		bool IVersionChangeInfo.IsRequired_Data => DataVersion.Application.CompareTo(dbReferenceVersion_Data) != 0;

		/// <summary>
		/// if NOT downgrading
		/// AND AppTransformationVersion > DbReferenceTransformationVersion
		/// </summary>
		bool IVersionChangeInfo.IsRequired_Transformation
			=> TransformationVersion.ApplicationNumber.CompareTo(dbReferenceVersion_Transformation) != 0;

		/// <summary>
		/// If upgrading
		/// OR downgrading minor version only
		/// </summary>
		bool IVersionChangeInfo.IsRequired_Schema
			=> appToDbVersionComparison_Schema > 0
			|| (appToDbVersionComparison_Schema < 0 && !(bool)isMajoDiff_Schema);

		bool IVersionChangeInfo.IsRequired_Script
			=> ScriptVersion.Application.CompareTo(dbReferenceVersion_Script) != 0
			|| ((IVersionChangeInfo)this).IsRequired_Schema;

		public bool IsRequired_ClientDocuments { get; private set; }

		#endregion
	}
}

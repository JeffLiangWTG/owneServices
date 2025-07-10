namespace Enterprise.Startup.Testing
{
	sealed class DbUpgraderVersionInfoForTesting : DbUpgraderVersionInfo
	{
		public void ReInitialiseDbReferenceVersion_Schema(int schemaMajorFromDb, int schemaMinorFromDb)
		{
			InitialiseDbReferenceVersion(
				schemaMajorFromDb, schemaMinorFromDb,
				dbReferenceVersion_Script.Major, dbReferenceVersion_Script.Minor,
				dbReferenceVersion_Transformation.Major, dbReferenceVersion_Transformation.Minor,
				dbReferenceVersion_Data.Major, dbReferenceVersion_Data.Minor,
				dbReferenceVersion_Clr.Major, dbReferenceVersion_Clr.Minor);
		}

		public void ReInitialiseDbReferenceVersion_Script(int scriptMajorFromDb, int scriptMinorFromDb)
		{
			InitialiseDbReferenceVersion(
				dbReferenceVersion_Schema.Major, dbReferenceVersion_Schema.Minor,
				scriptMajorFromDb, scriptMinorFromDb,
				dbReferenceVersion_Transformation.Major, dbReferenceVersion_Transformation.Minor,
				dbReferenceVersion_Data.Major, dbReferenceVersion_Data.Minor,
				dbReferenceVersion_Clr.Major, dbReferenceVersion_Clr.Minor);
		}

		public void ReInitialiseDbReferenceVersion_Data(int dataVersionMajorFromDb, int dataVersionMinorFromDb)
		{
			this.InitialiseDbReferenceVersion(
				dbReferenceVersion_Schema.Major, dbReferenceVersion_Schema.Minor,
				dbReferenceVersion_Script.Major, dbReferenceVersion_Script.Minor,
				dbReferenceVersion_Transformation.Major, dbReferenceVersion_Transformation.Minor,
				dataVersionMajorFromDb, dataVersionMinorFromDb,
				dbReferenceVersion_Clr.Major, dbReferenceVersion_Clr.Minor);
		}

		public void ReInitialiseDbReferenceVersion_Transformation(int transformationMajorFromDb, int transformationMinorFromDb)
		{
			this.InitialiseDbReferenceVersion(
				dbReferenceVersion_Schema.Major, dbReferenceVersion_Schema.Minor,
				dbReferenceVersion_Script.Major, dbReferenceVersion_Script.Minor,
				transformationMajorFromDb, transformationMinorFromDb,
				dbReferenceVersion_Data.Major, dbReferenceVersion_Data.Minor,
				dbReferenceVersion_Clr.Major, dbReferenceVersion_Clr.Minor);
		}

		public void ReInitialiseDbReferenceVersion_Clr(int clrVersionMajorFromDb, int clrVersionMinorFromDb)
		{
			this.InitialiseDbReferenceVersion(
				dbReferenceVersion_Schema.Major, dbReferenceVersion_Schema.Minor,
				dbReferenceVersion_Script.Major, dbReferenceVersion_Script.Minor,
				dbReferenceVersion_Transformation.Major, dbReferenceVersion_Transformation.Minor,
				dbReferenceVersion_Data.Major, dbReferenceVersion_Data.Minor,
				clrVersionMajorFromDb, clrVersionMinorFromDb);
		}
	}
}

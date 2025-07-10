namespace Enterprise.Builder.Generator
{
	public static class CommandLineOptions
	{
		public const string SetupNewSchemaAutoRegen = "-AUTOREGEN";
		public const string CommandLineSetupNewSchemaAutoRegen = "-SILENTAUTOREGEN";

		internal const string GenerateModelViewObjects = "-MODELVIEW";
		internal const string GenerateOneBizObj = "-BIZOBJ";
		internal const string GenerateBizObjectsForSolution = "-BIZOBJFORSOLUTION";
		internal const string GenerateSchemaColumnList = "-ZSCHEMA";
		internal const string RestoreDatabase = "-RESTOREDB";
		internal const string CommandLineRestoreDatabase = "-SILENTRESTOREDB";
	}
}

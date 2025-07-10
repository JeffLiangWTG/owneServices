namespace Enterprise.Builder.Generator
{
	sealed class SingleNonBizObjTableSchemaGeneratorForTesting : SingleNonBizObjTableSchemaGenerator
	{
		public SingleNonBizObjTableSchemaGeneratorForTesting(string dbName, string subFolder, string nonBusinessObjectTableName, GeneratorOutputDirectory outputDirectory)
			: base(dbName, subFolder, nonBusinessObjectTableName, outputDirectory)
		{
		}

		public string FullTableName_Exposed
		{
			get { return FullTableName; }
		}

		public string SubFolderNameForSchemaClasses_Exposed
		{
			get { return SubFolderNameForSchemaClasses; }
		}

		public string ActualDatabaseNameDuringRegen_Exposed
		{
			get { return ActualDatabaseNameDuringRegen; }
		}
	}
}

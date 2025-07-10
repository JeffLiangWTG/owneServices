using CargoWise.Data;

namespace Enterprise.DbUpgrader.Schema
{
	sealed class MetadataScriptRunnerForTesting : MetadataScriptRunner
	{
		public MetadataScriptRunnerForTesting(DbConnection upgConnection, string dbBeingUpgraded, string templateDb)
			: base(upgConnection, dbBeingUpgraded, templateDb)
		{
		}

		public string GetScriptReplacingDbNames_Exposed(string rawScript)
		{
			return GetScriptReplacingDbNames(rawScript);
		}
	}
}

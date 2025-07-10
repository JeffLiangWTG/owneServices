using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.Schema
{
	/// <summary>
	/// Gets/Runs metadata scripts to synchronise database objects based on the TemplateDb.
	/// Tags {0} and {1} are replaced by the actual DbBeingUpgraded and TemplateDb names.
	/// </summary>
	public abstract class MetadataScriptRunner
	{
		public MetadataScriptRunner(DbConnection upgConnection, string dbBeingUpgraded, string templateDb, IUpgradeTaskWorkflowLogger taskLogger = null)
		{
			this.dbBeingUpgraded = dbBeingUpgraded;
			this.templateDb = templateDb;
			this.upgConnection = upgConnection;
			this.taskLogger = taskLogger;
		}

		protected readonly string dbBeingUpgraded;
		protected readonly string templateDb;
		protected readonly DbConnection upgConnection;
		protected readonly IUpgradeTaskWorkflowLogger taskLogger;

		protected DataTable GetDataTableFromQueryReplacingDbNames(string rawSqlScript)
		{
			string sqlText = GetScriptReplacingDbNames(rawSqlScript);
			DataTable result = DataUtils.GetDataTableFromQuery(upgConnection, sqlText);

			return result;
		}

		protected string GetScriptReplacingDbNames(string rawScript)
		{
			string result = String.Format(rawScript, dbBeingUpgraded, templateDb);
			return result;
		}
	}
}

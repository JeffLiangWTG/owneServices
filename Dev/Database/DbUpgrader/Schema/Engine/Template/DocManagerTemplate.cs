using CargoWise.Data;
using Enterprise.DbUpgrader.Resource;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema
{
	/// <summary>
	/// Manage creation of DocManager template databases
	/// </summary>
	public class DocManagerTemplate : LatestSchemaDbTemplate
	{
		public DocManagerTemplate(IUpgradeManager manager, string templateDbName)
			: base(manager, templateDbName)
		{
		}

		protected override void CreateAllDbObjects(DbConnection conn)
		{
			var scriptManager = new ScriptManager();
			conn.ExecuteNonQuery(scriptManager.DocManagerSchemaScript);
		}
	}
}

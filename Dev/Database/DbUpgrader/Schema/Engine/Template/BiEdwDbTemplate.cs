using CargoWise.Data;
using Enterprise.DbUpgrader.Resource;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Template
{
	public class BiEdwDbTemplate : LatestSchemaDbTemplate
	{
		public BiEdwDbTemplate(IUpgradeManager manager, string templateDbName, string serverName)
			: base(manager, templateDbName, serverName)
		{
		}

		protected override void CreateAllDbObjects(DbConnection conn)
		{
			var manager = new ScriptManager();
			conn.ExecuteNonQuery(manager.BiEdwDbSchemaScript);
		}
	}
}

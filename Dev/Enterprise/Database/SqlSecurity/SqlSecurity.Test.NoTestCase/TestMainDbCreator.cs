using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.SqlSecurity.Test.NoTestCase
{
	public class TestMainDbCreator : MainDbTemplate, IAuxiliaryDbCreator
	{
		public TestMainDbCreator(IUpgradeManager manager, string templateDbName) : base(manager, templateDbName)
		{
		}

		protected override void SetupDatabaseAfterCreation(DbConnection conn)
		{
			var allScriptsInIndexingOrder = CoreScriptIndex.GetScripts();
			foreach (var script in allScriptsInIndexingOrder)
			{
				if (script.Name == "DataRegGetValueNOD")
				{
					conn.ExecuteNonQuery(script.Text);

					if (script is IIndexedViewDbScript)
					{
						conn.ExecuteNonQuery(((IIndexedViewDbScript)script).IndexCreateScript);
					}
				}
			}

			CreateAllDbObjects(conn);
		}
	}
}

using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	public class TemplateDbCreatorForTest : LatestSchemaDbTemplate
	{
		public TemplateDbCreatorForTest(IUpgradeManager manager, string templateDbName)
			: this(manager, templateDbName, null)
		{
		}

		public TemplateDbCreatorForTest(IUpgradeManager manager, string templateDbName, params string[] createDbObjectsScripts)
			: base(manager, templateDbName)
		{
			this.createDbObjectsScripts = createDbObjectsScripts;
		}

		protected override void SetupDatabaseAfterCreation(DbConnection conn)
		{
			((IDbLoginRepair)conn).EnsureDbLoginsHaveRightsToCurrentDatabase();
			base.SetupDatabaseAfterCreation(conn);
		}

		protected override void CreateAllDbObjects(DbConnection conn)
		{
			if (createDbObjectsScripts != null)
			{
				foreach (var script in createDbObjectsScripts)
				{
					if (!String.IsNullOrEmpty(script))
					{
						conn.ExecuteNonQuery(script);
					}
				}
			}
		}

		protected override string BaseDbName
		{
			get { return Db.DatabaseName; }
		}

		public string DbName_Exposed
		{
			get { return dbName; }
		}

		readonly string[] createDbObjectsScripts;
	}
}

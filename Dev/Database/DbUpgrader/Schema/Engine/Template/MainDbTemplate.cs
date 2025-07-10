using CargoWise.Data;
using Enterprise.DbUpgrader.Resource;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema
{
	/// <summary>
	/// Manage creation of the application main database template
	/// </summary>
	public class MainDbTemplate : LatestSchemaDbTemplate
	{
		public MainDbTemplate(IUpgradeManager manager, string templateDbName)
			: base(manager, templateDbName)
		{
		}

		protected override void CreateAllDbObjects(DbConnection conn)
		{
			conn.RunInTransaction(() =>
			{
				ShowInfoMessage("Creating additional schemas");
				CreateDbSchemas(conn);

				ShowInfoMessage("Creating xml schemas");
				CreateXmlSchemaObjects(conn);

				ShowInfoMessage("Creating objects (e.g. tables)");
				CreateDbSchemaObjects(conn);

				ShowInfoMessage("All inside-transaction template database objects are done");
			});

			ShowInfoMessage("Creating extra objects (e.g. client-specific tables)");
			CreateExtraDbObjects(conn);
		}

		void CreateDbSchemas(DbConnection conn)
		{
			foreach (var schema in Db.CW1AdditionalSchemas)
			{
				conn.ExecuteNonQuery($"CREATE SCHEMA {schema}");
			}
		}

		void CreateXmlSchemaObjects(DbConnection conn)
		{
			using (var cmd = conn.Command(Scripts.MaindDbXmlSchemaScript))
			{
				cmd.ExecuteNonQuery();
			}
		}

		void CreateDbSchemaObjects(DbConnection conn)
		{
			using (var cmd = conn.Command(Scripts.MaindDbSchemaScript))
			{
				cmd.ExecuteNonQuery();
			}
		}

		ScriptManager scripts;
		ScriptManager Scripts
		{
			get
			{
				if (scripts == null)
				{
					scripts = new ScriptManager();
				}

				return scripts;
			}
		}
		protected virtual void CreateExtraDbObjects(DbConnection conn)
		{
			CreateClientSpecificTables(conn);
		}

		void CreateClientSpecificTables(DbConnection conn)
		{
			new ClientSpecificTableCreator().Create(conn);
		}
	}

	#region MainDbTemplateFactory

	public static class MainDbTemplateFactory
	{
		public static MainDbTemplate New(IUpgradeManager manager, string templateDbName)
		{
#if DEBUG
			return new MainDbTemplateWithExtraDevelopmentObjects(manager, templateDbName);
#else
			return new MainDbTemplate(manager, templateDbName);
#endif
		}
	}

	#endregion
}

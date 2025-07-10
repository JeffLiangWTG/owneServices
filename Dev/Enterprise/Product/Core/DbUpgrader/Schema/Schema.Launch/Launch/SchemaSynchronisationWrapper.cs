using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Schema
{
	public partial class SchemaSynchronisationWrapper
	{
		public SchemaSynchronisationWrapper(IUpgradeManager manager, string dbToUpgrade, DbConnection upgConnection)
		{
			Manager = manager;
			DbBeingUpgraded = dbToUpgrade;
			UpgConnection = upgConnection;
		}

		public void Run()
		{
			using (((ICurrentDbControl)UpgConnection).UseDatabase(DbBeingUpgraded))
			{
				try
				{
					DoSynchroniseSchema();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Manager.ShowTaskError(ex.Message);
					var exceptionMessageBuilder = new StringBuilder(FormattableString.Invariant($"Failed to synchronise database [{DbBeingUpgraded}]."));
					if (ex is SqlException sqlException && new DbErrorMatch(sqlException).ExceptionType == DbErrorType.CouldNotObtainExclusiveLock)
					{
						exceptionMessageBuilder.AppendLine("Current queries to [model] database:");
						UpgConnection.ExecuteReader(
							@"
SELECT
	dm_tran_locks.request_session_id,
	dm_exec_sessions.login_name,
	text
FROM
	sys.dm_tran_locks
	JOIN sys.databases ON dm_tran_locks.resource_database_id = databases.database_id
		AND databases.name=@name
	JOIN sys.dm_exec_sessions ON dm_tran_locks.request_session_id = dm_exec_sessions.session_id
	JOIN sys.dm_exec_requests ON dm_tran_locks.request_session_id = dm_exec_requests.session_id
	CROSS APPLY sys.dm_exec_sql_text(dm_exec_requests.sql_handle)
",
							command => { command.AddParameter("@name", SqlDbType.VarChar, "model"); },
							reader =>
							{
								exceptionMessageBuilder.AppendLine(FormattableString.Invariant($"Session id: {reader["request_session_id"]}"));
								exceptionMessageBuilder.AppendLine(FormattableString.Invariant($"Login name: {reader["login_name"]}"));
								exceptionMessageBuilder.AppendLine(FormattableString.Invariant($"Query: {reader["text"]}"));
							});
					}

					throw new Exception(exceptionMessageBuilder.ToString(), ex);
				}
			}
		}

		protected virtual IAuxiliaryDbCreator GetTemplateDbCreator()
		{
			return MainDbTemplateFactory.New(Manager, TemplateDb);
		}

		protected virtual string GetTemplateDbName() => UpgUtils.GetTemplateDbName(DbBeingUpgraded);

		#region Implementation

		#region Constants and Attributes

		protected string DbBeingUpgraded { get; }
		protected IUpgradeManager Manager { get; }
		protected DbConnection UpgConnection { get; }

		protected IAuxiliaryDbCreator TemplateCreator => templateDbCreator ?? (templateDbCreator = GetTemplateDbCreator());

		IAuxiliaryDbCreator templateDbCreator;

		protected string TemplateDb => templateDbName ?? (templateDbName = GetTemplateDbName());

		string templateDbName;

		#endregion // Constants and Attributes

		#region Upgrade Scripts

		protected void DoSynchroniseSchema()
		{
			Manager.StartTask(string.Format(CultureInfo.InvariantCulture, "*** Database: {0} ***", DbBeingUpgraded));

			Manager.StartTask("Creating template database if needed");
			CreateTemplate();

			RunSynchronisationActions();

			Manager.ShowInfoMessage(".");
		}

		protected virtual void RunSynchronisationActions()
		{
			SchemaSynchroniser.SynchroniseDatabaseSchema();
		}

		internal void CreateAndValidateCheckConstraints()
		{
			using (((ICurrentDbControl)UpgConnection).UseDatabase(DbBeingUpgraded))
			{
				SchemaSynchroniser.CreateAndValidateCheckConstraints();
			}
		}

		internal void CreateTemporaryTransformationIndexes(ITransformationIndexProvider indexProvider)
		{
			var provider = indexProvider.IndexProvider;
			if (provider.Any())
			{
				Manager.StartTask("Creating temporary transformation indexes");
				using (((ICurrentDbControl)UpgConnection).UseDatabase(DbBeingUpgraded))
				{
					provider.CreateIndexes(UpgConnection, Manager);
				}
			}
		}

		internal void DropTemporaryTransformationIndexes(ITransformationIndexProvider indexProvider)
		{
			var provider = indexProvider.IndexProvider;
			if (provider.Any())
			{
				Manager.StartTask("Dropping temporary Pre-Upgrade transformation indexes");
				using (((ICurrentDbControl)UpgConnection).UseDatabase(DbBeingUpgraded))
				{
					provider.DropIndexes(UpgConnection, Manager);
				}
			}
		}

		internal void SetSelectiveXMLIndexesChangedFilter()
		{
			SchemaSynchroniser.IndexMetadataRunner.SetSelectiveXMLIndexesChangedFilter();
		}

		protected SchemaSynchroniser SchemaSynchroniser => schemaSynchroniser ?? (schemaSynchroniser = new SchemaSynchroniser(UpgConnection, Manager, DbBeingUpgraded, TemplateDb));

		SchemaSynchroniser schemaSynchroniser;

		#endregion // Upgrade Scripts

		#region Miscellaneous Methods

		void CreateTemplate()
		{
			TemplateCreator.CreateDropExisting();
		}

		#endregion // Miscellaneous Methods

		#endregion // Implementation
	}
}

#region Test
#if DEBUG

namespace Enterprise.DbUpgrader.Schema
{
	partial class SchemaSynchronisationWrapper
	{
		internal void DropTemplateDb()
		{
			templateDbCreator.Drop();
		}
	}
}

#endif
#endregion

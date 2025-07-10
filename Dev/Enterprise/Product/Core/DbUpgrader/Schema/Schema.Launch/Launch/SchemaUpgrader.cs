using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.ReferenceDatabases;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations;
using Enterprise.Environment;

namespace Enterprise.DbUpgrader.Schema
{
	public class SchemaUpgrader : BaseUpgrader
	{
		public SchemaUpgrader(IUpgradeManager manager, DbConnection upgConnection, DbConnection auditConnection, DbConnection dataWarehouseConnection)
			: base(manager, upgConnection, manager.SchemaVersionBeforeUpgrade)
		{
			this.auditConnection = auditConnection;
			this.dataWarehouseConnection = dataWarehouseConnection;
		}

		readonly DbConnection auditConnection;
		readonly DbConnection dataWarehouseConnection;

		protected override void DoUpgrade()
		{
			UpgradeReferenceDbs();

			UpgradeOperationalDatabaseStructure();

			UpdateSchemaVersion();
		}

		public void RefreshDependentScripts(IDependentScriptRefresher dependentScriptRefresher = null)
			=> MainDbSynchroniser.RefreshDependentScripts(dependentScriptRefresher ?? new DependentScriptRefresher());

		void UpgradeOperationalDatabaseStructure()
		{
			// Prepare data to comply with new structure if required
			RunPreUpgradeTransformationTasks();

			// Upgrade databases
			UpgradeMainDbSchema();
			CdcSynchroniseAfterSchemaChanges();
			UpgradeDocManagerDatabases();
			UpgradeBusinessIntelligenceDatabases();
		}

		public override int EstimatedNumberOfTasks
		{
			get
			{
				if (estimatedNumberOfTasks == null)
				{
					estimatedNumberOfTasks = NumberOfGeneralSchemaUpgradeTasks
						+ NumberOfDbSynchronisingTasks * (DocManagerDBsToUpgrade.Count() + 1);
				}

				return estimatedNumberOfTasks.Value;
			}
		}

		int? estimatedNumberOfTasks;

		public override IEnumerable<string> SecondaryDatabasesToUpgrade
		{
			get { return DocManagerDBsToUpgrade; }
		}

		public override string Name
		{
			get { return "Database Schema Upgrade"; }
		}

		protected override VersionLabel LatestVersion
		{
			get { return SchemaVersion.Application; }
		}

		#region Implementation

		#region Upgrade Scripts

		#region CDC Support

		CdcSupport CdcUpgrader
		{
			get
			{
				return cdcUpgrader_UsePropertyInstead ?? (cdcUpgrader_UsePropertyInstead = new CdcSupport(Manager, Db.DatabaseName, (AdminConnection)upgConnection));
			}
		}
		CdcSupport cdcUpgrader_UsePropertyInstead;

		/// <summary>
		/// Apply schema changes to CDC
		/// </summary>
		void CdcSynchroniseAfterSchemaChanges()
		{
			CdcUpgrader.SynchroniseCdcSchema();
		}

		#endregion

		#region Upgrade Transformation Scripts

		public TransformationDirector TransformationDirector
		{
			get
			{
				if (fTransformationDirector == null)
				{
					fTransformationDirector = new TransformationDirector(Manager, auditConnection, dataWarehouseConnection);
				}

				return fTransformationDirector;
			}
		}

		protected TransformationDirector fTransformationDirector;

		#region Before Upgrade

		protected void RunPreUpgradeTransformationTasks()
		{
			RunPreUpgradeTransformationScripts();
			RemovePreSchemaUpgradeTemporaryDatabaseTables();
		}

		protected void RunPreUpgradeTransformationScripts()
		{
			StartTask("Running pre-upgrade transformation scripts");
			try
			{
				TransformationDirector.OfflinePreUpgradeRun();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				string errorMessage = "Failed to run pre-upgrade transformation scripts." + System.Environment.NewLine + e.Message;
				throw new Exception(errorMessage, e);
			}
			ShowInfoMessage(".");
		}

		void RemovePreSchemaUpgradeTemporaryDatabaseTables()
		{
			OnlineUpgrade.TablePreSynchroniser.CleanupAllResources(upgConnection);
		}

		#endregion

		#endregion

		#region DocManager DB Upgrade

		/// <summary>
		/// Made virtual for UnitTest only
		/// </summary>
		protected virtual void UpgradeDocManagerDatabases()
		{
			foreach (var docManagerDb in DocManagerDBsToUpgrade)
			{
				var syncSchema = new DocManagerSchemaSynchronisationWrapper(Manager, docManagerDb, upgConnection);
				syncSchema.Run();
			}
		}

		/// <summary>
		/// Runs DocManager upgrade if:
		///  - DocManager version is greater than the DB version before the upgrade
		///  - Application version is less than the DB version before the upgrade (downgrade)
		///    It might happen that the DocManager database has changed in the upgrade that is been reversed
		/// </summary>
		protected virtual IEnumerable<string> DocManagerDBsToUpgrade
		{
			get
			{
				if (SchemaVersion.DocManager.CompareTo(versionBeforeUpgrade) > 0
				|| SchemaVersion.Application.CompareTo(versionBeforeUpgrade) < 0)
				{
					return upgConnection.GetDatabases(DatabaseType.SD);
				}
				else
				{
					return Array.Empty<string>();
				}
			}
		}

		#endregion

		#region Business Intelligence Databases

		void UpgradeBusinessIntelligenceDatabases()
		{
			if (BiRequirementChecker.Instance.IsBiDatabaseUpgradeRequired(upgConnection, auditConnection))
			{
				UpgradeAuditDatabase();
			}

			if (BiRequirementChecker.Instance.IsBiDatabaseUpgradeRequired(upgConnection, dataWarehouseConnection))
			{
				UpgradeEdwDatabase();
			}
		}

		void UpgradeAuditDatabase()
		{
			var syncSchema = AuditDatabaseSynchronisationWrapper.New(Manager, auditConnection);
			syncSchema.Run();
		}

		void UpgradeEdwDatabase()
		{
			var syncSchema = EdwDatabaseSynchronisationWrapper.New(Manager, dataWarehouseConnection);
			syncSchema.Run();
		}

		#endregion

		/// <summary>
		/// Made virtual for UnitTest only
		/// </summary>
		protected virtual void UpgradeMainDbSchema()
			=> MainDbSynchroniser.Run();

		public void CreateAndValidateCheckConstraints()
			=> MainDbSynchroniser.CreateAndValidateCheckConstraints();

		public void CreateTemporaryTransformationIndexes()
			=> MainDbSynchroniser.CreateTemporaryTransformationIndexes(TransformationDirector);

		public void DropTemporaryTransformationIndexes()
			=> MainDbSynchroniser.DropTemporaryTransformationIndexes(TransformationDirector);

		public void SetSelectiveXMLIndexesChangedFilter()
			=> MainDbSynchroniser.SetSelectiveXMLIndexesChangedFilter();

		protected virtual MainDbSchemaSynchronisationWrapper MainDbSynchroniser
			=> mainDbSynchroniser ??
			  (mainDbSynchroniser = new MainDbSchemaSynchronisationWrapper(
					Manager,
					Db.DatabaseName,
					upgConnection));

		MainDbSchemaSynchronisationWrapper mainDbSynchroniser;

		protected void UpdateSchemaVersion()
		{
			StartTask("Updating schema version");

			try
			{
				Env.Registry.DatabaseMajorSchemaVersion = SchemaVersion.Application.Major;
				Env.Registry.DatabaseMinorSchemaVersion = SchemaVersion.Application.Minor;
			}
			catch (Exception e)
			{
				throw new Exception("Failed to update schema version.\r\n" + e.Message, e);
			}
		}

		#endregion

		#region Create Customs Reference DBs

		/// <summary>
		/// REFERENCE FILE DATABASE UPGRADES go in the DbUpgrader.ReferenceDatabases solution.
		/// </summary>
		protected void UpgradeReferenceDbs()
		{
			DoReferenceDbUpgrade(upgConnection);

			DoReferenceDbUpgradeForEdw();
		}

		protected void DoReferenceDbUpgradeForEdw()
		{
			if (dataWarehouseConnection == null)
			{
				return;
			}
			using ((dataWarehouseConnection as ICurrentDbControl).UseDatabase(Db.EdwDatabaseName))
			{
				if (UpgradeDatawarehouseReferenceDbsRequired)
				{
					DoReferenceDbUpgrade(dataWarehouseConnection);
				}
				else
				{
					ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Begin synonym synchronization for {0} to {1}", Db.EdwDatabaseName, RefDbTableNameResolver.SingleRefDatabaseName));
					var creator = new RefDatabaseSynonymRecreator(dataWarehouseConnection);
					creator.RecreateSynonym();
					ShowInfoMessage(string.Format(CultureInfo.InvariantCulture, "Finish synonym synchronization for {0} to {1}", Db.EdwDatabaseName, RefDbTableNameResolver.SingleRefDatabaseName));
				}
			}
		}

		public virtual bool UpgradeDatawarehouseReferenceDbsRequired
		{
			get
			{
				return !string.Equals(dataWarehouseConnection.ServerNameReportedByDatabase, upgConnection.ServerNameReportedByDatabase, StringComparison.OrdinalIgnoreCase);
			}
		}

		void DoReferenceDbUpgrade(DbConnection connection)
		{
			StartTask($"Creating/Updating reference databases on server {connection.ServerName}");

			try
			{
				var director = new ReferenceDbUpgradeDirector(Manager, connection, Manager);
				director.UpgradeReferenceDbs();
				director.UpgradeSingleRefDatabase();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var errorMessage = $"Failed to create/update reference databases on server {connection.ServerName}.\r\n" + ex.Message;
				throw new Exception(errorMessage, ex);
			}

			ShowInfoMessage(".");
		}

		#endregion

		const int NumberOfGeneralSchemaUpgradeTasks = 9;
		const int NumberOfDbSynchronisingTasks = 11;

		#endregion
	}
}

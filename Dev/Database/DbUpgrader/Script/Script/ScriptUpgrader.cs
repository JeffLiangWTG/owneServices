using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Script
{
	public class ScriptUpgrader : BaseUpgrader
	{
		public ScriptUpgrader(IUpgradeManager manager, DbConnection upgConnection, DbConnection auditConnection, DbConnection dataWarehouseConnection, VersionLabel versionBeforeUpgrade, IEnumerable<string> triggerTransformations = null)
			: base(manager, upgConnection, versionBeforeUpgrade)
		{
			this.auditConnection = auditConnection;
			this.dataWarehouseConnection = dataWarehouseConnection;
			this.triggerTransformations = triggerTransformations;
		}
		readonly DbConnection auditConnection;
		readonly DbConnection dataWarehouseConnection;
		readonly IEnumerable<string> triggerTransformations;

		protected override void DoUpgrade()
		{
			DoUpgradeCore();
		}

		public override int EstimatedNumberOfTasks
		{
			get
			{
				if (estimatedNumberOfTasks == null)
				{
					var numOfTasks = 9;
					estimatedNumberOfTasks = numOfTasks + DocManagerDBsToUpgrade.Count();
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
			get { return "Database View, Procedure, Function & Trigger Upgrade"; }
		}

		protected override VersionLabel LatestVersion
		{
			get { return ScriptVersion.Application; }
		}

		void DoUpgradeCore()
		{
			RecreateMainDbViewsAndRoutines();
			RecreateDocManagerViewsAndRoutines();
			RecreateBiViewsAndRoutines();
			UpdateScriptVersion();
		}

		public void CheckTriggersAreEnabledAndSync()
		{
			MainDbScriptCreator.CheckTriggersAreEnabledAndSync();
		}

		#region Main DB

		public void RecreateMainDbViewsAndRoutines()
		{
			MainDbScriptCreator.Run();
			var mainDbTableCodeNameScriptCreator = new TableCodeNameMappingCreator(Manager, upgConnection);
			mainDbTableCodeNameScriptCreator.Run();
			Manager.ShowInfoMessage(".");
		}

		/// <summary>
		/// Made virtual for UnitTest only
		/// </summary>
		internal virtual ViewAndRoutineCreator GetNewMainDbViewAndRoutineCreator(string upgradingDb)
		{
			return new ViewAndRoutineCreator(Manager, upgConnection, upgradingDb, triggerTransformations);
		}

		ViewAndRoutineCreator MainDbScriptCreator => mainDbScriptCreator ?? (mainDbScriptCreator = GetNewMainDbViewAndRoutineCreator(MainDbBeingUpgraded));
		ViewAndRoutineCreator mainDbScriptCreator;

		/// <summary>
		/// Made virtual for UnitTest only
		/// </summary>
		protected virtual string MainDbBeingUpgraded
		{
			get { return Db.DatabaseName; }
		}

		#endregion

		#region DocManager DB Upgrade

		protected void RecreateDocManagerViewsAndRoutines()
		{
			foreach (var docManagerDb in DocManagerDBsToUpgrade)
			{
				var creator = GetNewViewAndRoutineCreator_DocManagerDbs(docManagerDb);
				creator.Run();
				Manager.ShowInfoMessage(".");
			}
		}

		/// <summary>
		/// Made virtual for UnitTest only
		/// </summary>
		internal virtual DocManagerViewAndRoutineCreator GetNewViewAndRoutineCreator_DocManagerDbs(string upgradingDb)
		{
			return new DocManagerViewAndRoutineCreator(Manager, upgConnection, upgradingDb);
		}

		/// <summary>
		/// Runs DocManager upgrade if:
		///  - DocManager version is greater than the DB version before the upgrade
		///  - Application version is less than the DB version before the upgrade (downgrade)
		///    It might happen that the DocManager database has changed in the upgrade that is been reversed
		///
		/// Made virtual for UnitTest only
		/// </summary>
		protected virtual IEnumerable<string> DocManagerDBsToUpgrade
		{
			get
			{
				if (ScriptVersion.Application.CompareTo(versionBeforeUpgrade) < 0)
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

		protected void RecreateBiViewsAndRoutines()
		{
			if (BiRequirementChecker.Instance.IsBiDatabaseUpgradeRequired(upgConnection, auditConnection))
			{
				RecreateAuditDbViewsAndRoutines();
			}

			if (BiRequirementChecker.Instance.IsBiDatabaseUpgradeRequired(upgConnection, dataWarehouseConnection))
			{
				RecreateEdwDbViewsAndRoutines();
			}
		}

		void RecreateEdwDbViewsAndRoutines()
		{
			RunViewAndRoutineCreator(GetNewViewAndRoutineCreator_EDW(MainDbBeingUpgraded));
		}

		void RecreateAuditDbViewsAndRoutines()
		{
			RunViewAndRoutineCreator(AuditViewAndRoutineCreator.New(Manager, auditConnection, MainDbBeingUpgraded));
		}

		void RunViewAndRoutineCreator(BiViewAndRoutineCreator creator)
		{
			if (creator.DoesBiDatabaseExist())
			{
				creator.Run();
				Manager.ShowInfoMessage(".");
			}
			else
			{
				Manager.ShowInfoMessage(Name + " skipped for non-existent BI database.");
			}
		}

		internal virtual EdwViewAndRoutineCreator GetNewViewAndRoutineCreator_EDW(string upgradingDb)
		{
			return EdwViewAndRoutineCreator.New(Manager, dataWarehouseConnection, upgradingDb);
		}

		#endregion

		protected void UpdateScriptVersion()
		{
			StartTask("Updating script version information");

			try
			{
				DbRegistry.DatabaseMajorScriptVersion.SaveValue(ScriptVersion.Application.Major, upgConnection);
				DbRegistry.DatabaseMinorScriptVersion.SaveValue(ScriptVersion.Application.Minor, upgConnection);
			}
			catch (Exception e)
			{
				throw new Exception("Failed to update script version information.\r\n" + e.Message, e);
			}
		}

		public void CreateTemporaryScriptIndexes()
		{
			MainDbScriptCreator.CreateTemporaryScriptIndexes();
		}
	}
}

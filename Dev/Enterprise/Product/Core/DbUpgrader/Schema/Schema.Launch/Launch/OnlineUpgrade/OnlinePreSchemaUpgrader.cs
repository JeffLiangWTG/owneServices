using System;
using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Schema.OnlineUpgrade;
using Enterprise.DbUpgrader.Shared;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;

namespace Enterprise.DbUpgrader.Schema
{
	public class OnlinePreSchemaUpgrader : BaseUpgrader
	{
		public OnlinePreSchemaUpgrader(IUpgradeManager manager, DbConnection upgConnection, DbConnection auditConnection, DbConnection dataWarehouseConnection, VersionLabel versionBeforeUpgrade)
			: base(manager, upgConnection, versionBeforeUpgrade)
		{
			this.auditConnection = auditConnection;
			this.dataWarehouseConnection = dataWarehouseConnection;
		}
		readonly DbConnection auditConnection;
		readonly DbConnection dataWarehouseConnection;

		protected override void DoUpgrade()
		{
			bool disableAutoStatistics = DataUtils.ShouldDisableAutoStatisticsDuringUpgrade(upgConnection, Db.DatabaseName);
			if (disableAutoStatistics)
			{
				TurnOffStatistics(StatisticsSwitch.Instance, new RetryPolicy<ErrorDetectionStrategy>(new FixedInterval(4, TimeSpan.FromSeconds(15))));
			}

			try
			{
				UpgradeMainDbSchema();
				OnlineUpgradeBusinessIntelligenceDatabases();
				RunUpgradeActions(UpgradeActions);
			}
			finally
			{
				if (disableAutoStatistics)
				{
					StatisticsSwitch.Instance.CheckAndTurnStatistics(Db.DatabaseName, isON: true);
				}
			}
		}

		internal static void TurnOffStatistics(IStatisticsSwitch statisticsSwitch, RetryPolicy retryPolicy)
		{
			try
			{
				retryPolicy.ExecuteAction(() => statisticsSwitch.CheckAndTurnStatistics(Db.DatabaseName, isON: false));
			}
			catch (SqlException sqlException) when (ErrorDetectionStrategy.IsTransient(sqlException))
			{
				throw new OnlinePreSchemaUpgraderEnvironmentException(OnlinePreSchemaUpgraderEnvironmentException.DisableStatisticsExceptionMessage, sqlException);
			}
		}

		public override int EstimatedNumberOfTasks => 41;
		public override IEnumerable<string> SecondaryDatabasesToUpgrade => Array.Empty<string>();
		public override string Name => "Database Schema Online Upgrade";
		protected override VersionLabel LatestVersion => SchemaVersion.Application;
		protected override bool RequiresTransaction => false;
		public UpgradeActionList UpgradeActions => upgradeActions ?? (upgradeActions = new UpgradeActionList());

		void RunUpgradeActions(UpgradeActionList actionList)
		{
			foreach (var action in actionList)
			{
				Manager.ShowInfoMessage(".");
				Manager.StartTask("--- " + action.Name + " - START ---");

				action.Run();

				Manager.StartTask("--- " + action.Name + " - END   ---");
				Manager.ShowInfoMessage(".");
			}
		}

		#region Implementation

		protected virtual void UpgradeMainDbSchema()
		{
			new OnlineMainDatabaseSchemaSynchronisationWrapper(Manager, Db.DatabaseName, upgConnection).Run();
		}

		protected virtual void OnlineUpgradeBusinessIntelligenceDatabases()
		{
			if (auditConnection != null)
			{
				if (auditConnection.DatabaseExists(Db.AuditDatabaseName) && BiRequirementChecker.Instance.IsBiDatabaseUpgradeRequired(upgConnection, auditConnection))
				{
					new OnlineAuditDatabaseSynchronisationWrapper(Manager, Db.AuditDatabaseName, auditConnection).Run();
				}
			}

			if (dataWarehouseConnection != null)
			{
				if (dataWarehouseConnection.DatabaseExists(Db.EdwDatabaseName) && BiRequirementChecker.Instance.IsBiDatabaseUpgradeRequired(upgConnection, dataWarehouseConnection))
				{
					new OnlineEdwDatabaseSynchronisationWrapper(Manager, Db.EdwDatabaseName, dataWarehouseConnection).Run();
				}
			}
		}

		UpgradeActionList upgradeActions;

		class ErrorDetectionStrategy : ITransientErrorDetectionStrategy
		{
			public bool IsTransient(Exception ex)
			{
				if (ex is SqlException sqlException)
				{
					return IsTransient(sqlException);
				}

				return false;
			}

			static public bool IsTransient(SqlException sqlException)
			{
				var exceptionType = new DbErrorMatch(sqlException).ExceptionType;
				return
					exceptionType == DbErrorType.CannotAlterDbWhileInUse
					|| exceptionType == DbErrorType.BackupAndFileManipulationOperationsMustBeSerialized
					|| exceptionType == DbErrorType.TimeoutExpired;
			}
		}

		#endregion // Implementation
	}
}

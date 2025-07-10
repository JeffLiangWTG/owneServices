using System;
using System.IO;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Enterprise.DbUpgrader.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DbUpgrader.Startup
{
	class ChangeTrackingStatusManager
	{
		public void ReEnableChangeTrackToEnsureDataCleanup(AdminConnection connection, IUpgradeManager upgradeManager, ref string exceptionMessage)
		{
			var cdcSupport = new CdcSupport(upgradeManager, Db.DatabaseName, connection);
			var isGlowEnabled = ObjectFactory.Get<IGlowRegistry>().GlowServiceUri != "/";
			var shouldDisableChangeTracking = !isGlowEnabled || (cdcSupport.AreTablesTrackedByCdc && !ObjectFactory.Get<ISystemDataRegistry>().AllowCTWhenCDCEnabled);

			if (shouldDisableChangeTracking)
			{
				DisableChangeTracking(connection, upgradeManager, ref exceptionMessage);
			}
			if (shouldDisableChangeTracking)
			{
				if (!isGlowEnabled)
				{
					upgradeManager.StartTask("Skipping [Enable Change Tracking] as Glow Service Uri is not set.");
				}
				else
				{
					upgradeManager.StartTask("Skipping [Enable Change Tracking] as it is not allowed by the key [AllowCTWhenCDCEnabled].");
				}
			}
			else
			{
				EnableChangeTracking(connection, upgradeManager, ref exceptionMessage);
			}
		}

		void DisableChangeTracking(AdminConnection connection, IUpgradeTaskWorkflowLogger logger, ref string exceptionMessage) => SetChangeTrackingStatus(connection, logger, false, ref exceptionMessage);
		void EnableChangeTracking(AdminConnection connection, IUpgradeTaskWorkflowLogger logger, ref string exceptionMessage) => SetChangeTrackingStatus(connection, logger, true, ref exceptionMessage);

		protected void SetChangeTrackingStatus(AdminConnection connection, IUpgradeTaskWorkflowLogger logger, bool isEnable, ref string exceptionMessage)
		{
			var systemDataRegistry = ObjectFactory.Get<ISystemDataRegistry>();
			var hasLogBeenCalled = false;
			string message = null;
			SetChangeTrackingStatusCore(connection, isEnable, (logString, exception) =>
			{
				if (!hasLogBeenCalled)
				{
					hasLogBeenCalled = true;
					logger.ShowInfoMessage(".");
					logger.StartTask(logString);
				}
				else if (exception == null)
				{
					logger.StartTask(logString);
					logger.ShowInfoMessage(".");
				}
				else
				{
					logger.ShowInfoMessage(logString);
					message = logString;
				}
			});
			exceptionMessage += message;
		}

		void SetChangeTrackingStatusCore(AdminConnection connection, bool isEnable, Action<string, Exception> logAction)
		{
			var actionName = $"{(isEnable ? "Enable" : "Disable")} Change Tracking";
			logAction?.Invoke("--- " + actionName + " - START ---", null);

			try
			{
				ExecuteGlowSetChangeTrackingStatusScript(isEnable, connection);
				logAction?.Invoke("--- " + actionName + " - END   ---", null);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var message = $"{(isEnable ? "Enabling" : "Disabling")} Change Tracking encountered error : " + ex.Message;
				logAction?.Invoke(message, ex);
			}
		}

		internal virtual void ExecuteGlowSetChangeTrackingStatusScript(bool isEnable, AdminConnection connection)
		{
			using (var stream = Assembly.Load("CargoWise.Glow.Model.CW.Resources").GetManifestResourceStream($"CargoWise.Glow.Model.CW1.Resources.{(isEnable ? "Enable" : "Disable")}ChangeTracking.sql"))
			{
				if (stream == null)
				{
					throw new InvalidOperationException($"{(isEnable ? "Enable" : "Disable")}ChangeTracking.SQL Resource Stream is null");
				}

				using (var reader = new StreamReader(stream))
				{
					var sql = reader.ReadToEnd();
					using (var command = connection.Command(sql))
					{
						command.ExecuteNonQuery();
					}
				}
			}
		}
	}
}

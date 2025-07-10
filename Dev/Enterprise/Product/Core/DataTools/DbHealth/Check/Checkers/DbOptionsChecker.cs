using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.DbHealth.Check
{
	internal class DbOptionsChecker : IChecker, IDbOptionsChecker
	{
		public string Description => "Check that database options and features are compatible";

		public string DelayedDurabilityErrorMessage = " database has Delayed Durability enabled, contact " + Core.Constants.ProductSupportName + " to disable Delayed Durability on your database. Delayed Durability is not compatible with Change Data Capture.";

		public const string DelayedDurabilityOptionName = "delayed_durability";
#if DEBUG
		public
#endif
			string IsDelayedDurabilityEnabledWithCDCQuery = @"
				IF EXISTS (
					SELECT name
					FROM sys.databases
					WHERE name = @dbname
						AND delayed_durability > 0		--checks for delayed durability
						AND is_cdc_enabled = 1			--checks for cdc enablement
				)
				SELECT 1
				ELSE
				SELECT 0
			";

		public void Check(DbConnection connection, DbHealthWarningList warningList, ILogger logger)
		{
			CheckDelayedDurabilityOff(connection, logger);
		}

		public void CheckDelayedDurabilityOff(DbConnection connection, ILogger logger)
		{
			if (IsDelayedDurabilityEnabledWithCDC(connection, Db.DatabaseName, DelayedDurabilityOptionName))
			{
				var message = Db.DatabaseName + DelayedDurabilityErrorMessage;
				logger.Log(
					LogType.Error,
					message
				);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Db maintenance check")]
		public virtual bool IsDelayedDurabilityEnabledWithCDC(DbConnection connection, string dbName, string dbOption)
		{
			using (var cmd = connection.Command(IsDelayedDurabilityEnabledWithCDCQuery))
			{
				cmd.AddParameter("@dbName", SqlDbType.VarChar, 128, dbName);
				var result = cmd.ExecuteScalar();
				return Convert.ToBoolean(result);
			}
		}
	}
}

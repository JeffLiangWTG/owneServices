using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.Accounting.ServiceTasks.JobCostingReport
{
	public class JCDDependentObjectSynchronizer
	{
		public JCDDependentObjectSynchronizer(DbConnection connection, ILogger logger, JCDDBObjectVersionManager versionManager)
		{
			this.connection = connection;
			this.logger = logger;
			this.versionManager = versionManager;
		}

		readonly DbConnection connection;
		readonly ILogger logger;
		readonly JCDDBObjectVersionManager versionManager;

		public bool Synchronize()
		{
			bool result = false;

			if (!versionManager.IsUpToDate)
			{
				var currentVersionInRegistry = versionManager.CurrentVersion;

				if (connection != null)
				{
					try
					{
						var hasRunPartition = false;

						foreach (var dbObject in versionManager.DBObjects.Where(x => x.ShouldDropOnSynchronize).Reverse())
						{
							RunScriptToDropDBObject(dbObject);
							hasRunPartition = true;
						}

						foreach (var dbObject in versionManager.DBObjects.Where(x => x.ShouldCreateOnSynchronize))
						{
							RunScriptToCreateDBObject(dbObject);
							hasRunPartition = true;
						}

						versionManager.ChangeVersion(versionManager.LatestVersion);

						result = true;

						if (hasRunPartition)
						{
							logger.Log(LogType.Information, FormattableString.Invariant($"{versionManager.Description} are up-to-date."));
						}
					}
					catch
					{
						if (connection != null && connection.IsInTransaction)
						{
							versionManager.ChangeVersion(currentVersionInRegistry);
						}
						throw;
					}
				}
			}

			return result;
		}

		public bool DropObjects(int changeToVersion)
		{
			bool result = false;

			if (connection != null)
			{
				var currentVersionInRegistry = versionManager.CurrentVersion;

				try
				{
					foreach (var dbObject in versionManager.DBObjects.Reverse())
					{
						RunScriptToDropDBObject(dbObject);
					}

					versionManager.ChangeVersion(changeToVersion);
					result = true;
				}
				catch
				{
					if (connection != null && connection.IsInTransaction)
					{
						versionManager.ChangeVersion(currentVersionInRegistry);
					}
					throw;
				}
			}

			return result;
		}

		protected virtual void RunScriptToCreateDBObject(JCDDependentDBObject dbObject)
		{
			if (!string.IsNullOrEmpty(dbObject.CreateSQLText))
			{
				logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Creating {0}", dbObject.Name));
				connection.ExecuteNonQuery(dbObject.CreateSQLText);
			}
		}

		protected virtual void RunScriptToDropDBObject(JCDDependentDBObject dbObject)
		{
			if (!string.IsNullOrEmpty(dbObject.CheckAndDropSQLText))
			{
				logger.Log(LogType.Debug, string.Format(CultureInfo.InvariantCulture, "Dropping {0}", dbObject.Name));
				connection.ExecuteNonQuery(dbObject.CheckAndDropSQLText);
			}
		}
	}
}

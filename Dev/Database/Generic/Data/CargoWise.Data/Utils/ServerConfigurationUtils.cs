using System;
using System.Collections.Generic;
using CargoWise.Data.SqlServer;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Data
{
	[ThreadSafe]
	public class ServerConfigurationUtils
	{
		const int SQL_EXCEPTION_SP_NOT_FOUND = 2812;
		readonly string linkedServerName;
		readonly Func<string, string, AdminConnection> replicaConnectionFactory;

		public ServerConfigurationUtils() : this("LOOPBACK")
		{
		}

		internal ServerConfigurationUtils(string linkedServerName) : this(linkedServerName, Db.NewAdminConnection)
		{
		}

		internal ServerConfigurationUtils(string linkedServerName, Func<string, string, AdminConnection> replicaConnectionFactory)
		{
			this.linkedServerName = linkedServerName;
			this.replicaConnectionFactory = replicaConnectionFactory;
		}

		/// <summary>
		/// Executes ConfigureServer stored proc from current database
		/// </summary>
		public virtual void EnsureServerIsConfigured(AdminConnection connection)
		{
			ExecuteConfigureServer(connection, string.Empty);
		}

		/// <summary>
		/// Executes ConfigureServer stored proc from current database On the main replica and tries 
		/// </summary>
		public virtual void EnsureServerIsConfiguredAndTryConfiguringOtherReplicas(AdminConnection connection, out List<Exception> replicaExceptions)
		{
			replicaExceptions = null;
			EnsureServerIsConfigured(connection);

			if (AlwaysOn.IsDbPartOfAlwaysOn(connection, connection.CurrentDatabase))
			{
				var dbName = connection.CurrentDatabase;
				var replicas = AlwaysOn.GetAlwaysOnReplicaInfos(connection, connection.CurrentDatabase);
				foreach (var replica in replicas)
				{
					if (replica.ReplicaServerName != connection.ServerName)
					{
						try
						{
							using (var replicaConnection = replicaConnectionFactory(replica.ReplicaServerName, "Master"))
							{
								ConfigureServerFromDatabase(replicaConnection, dbName);
							}
						}
						catch (Exception ex)
						{
							if (replicaExceptions is null)
							{
								replicaExceptions = new List<Exception>();
							}
							replicaExceptions.Add(ex);
						}
					}
				}
			}
		}

		/// <summary>
		/// Executes the ConfigureServer stored proc from the specified database.
		/// </summary>
		public virtual void ConfigureServerFromDatabase(AdminConnection connection, string targetDatabaseName)
		{
			if (string.IsNullOrWhiteSpace(targetDatabaseName))
			{
				throw new ArgumentNullException(nameof(targetDatabaseName));
			}
			try
			{
				ExecuteConfigureServer(connection, $"{targetDatabaseName}.dbo.");
			}
			catch (SqlException ex) when (ex.Number == SQL_EXCEPTION_SP_NOT_FOUND)
			{
				// If the database does not have a ConfigureServer stored proc, it doesn't need any configuration to be done on the server.
			}
		}

		void ExecuteConfigureServer(AdminConnection connection, string prefix)
		{
			connection.ExecuteNonQuery($@"{prefix}ConfigureServer",
				cmd =>
				{
					cmd.CommandType = System.Data.CommandType.StoredProcedure;
					cmd.AddParameter("@LINKEDSERVERNAME", System.Data.SqlDbType.NVarChar, 128, linkedServerName);
				}
			);
		}
	}
}

using System;
using System.Diagnostics;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.Loader
{
	public class EnterpriseDatabaseHelper
	{
		public EnterpriseDatabaseHelper(EnterpriseConfiguration configuration)
		{
			this.configuration = configuration;
		}
		readonly EnterpriseConfiguration configuration;

		public SqlConnection Connection
		{
			get;
			private set;
		}

		public SqlException ConnectionException
		{
			get;
			private set;
		}

		public bool OpenConnection(bool isAdmin = false)
		{
			return OpenConnectionCore(isAdmin);
		}

#if DEBUG
		protected internal virtual
#endif
		bool OpenConnectionCore(bool isAdmin)
		{
			if (Connection != null)
			{
				Connection.Close();
			}

			Connection = null;

			const int maxMillisecondsToConnect = 15 * 1000;
			int startTicks = Environment.TickCount;

			while ((Connection == null) && ((Environment.TickCount - startTicks) < maxMillisecondsToConnect))
			{
				try
				{
					SqlTrace($"Connecting to Server:{configuration.ServerName}, Database: {configuration.DatabaseName}, Admin={isAdmin}");

					if (isAdmin)
					{
						Connection = OpenNewAdminConnection();
					}
					else
					{
						Connection = OpenNewApplicationLoginConnection();
					}

					SqlTrace("Opened");
				}
				catch (SqlException ex)
				{
					ConnectionException = ex;
					SqlTrace("Caught exception: " + ex.ToString());
				}
			}

			return (Connection != null);
		}

		protected internal virtual SqlConnection OpenNewAdminConnection()
		{
			var connectionProvider = Application.ServiceProvider.GetRequiredService<ISqlConnectionProvider>();
			return connectionProvider.OpenNewAdminSqlConnection(configuration.ServerName, ConfigureConnectionString);
		}

		protected internal virtual SqlConnection OpenNewApplicationLoginConnection()
		{
			var connectionProvider = Application.ServiceProvider.GetRequiredService<ISqlConnectionProvider>();
			var pdsFactory = Application.ServiceProvider.GetRequiredService<IProtectedDataServiceFactory>();
			var protectedDataService = pdsFactory.CreateSystemService(configuration.ServerName, configuration.DatabaseName);
			return connectionProvider.OpenNewSqlConnection<RestrictedWriterLoginCredentials>(protectedDataService, ConfigureConnectionString);
		}

		void ConfigureConnectionString(SqlConnectionStringBuilder builder)
		{
			builder.DataSource = configuration.ServerName;
			builder.InitialCatalog = configuration.DatabaseName;
			builder.IntegratedSecurity = false;
			builder.Pooling = false;
			builder.ApplicationName = "CargoWiseOneLoader";
		}

		[Conditional("SQLTRACE")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Baseline")]
		static void SqlTrace(string traceString)
		{
			Trace.WriteLine(traceString, "ediLoad");
		}
	}
}

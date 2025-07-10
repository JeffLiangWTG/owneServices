using System;
using System.Globalization;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Data
{
	public abstract class DatabaseLogin<TCredentials> : DatabaseLogin where TCredentials : DBCredentials
	{
		protected IServiceProvider pdsServiceProvider;

		protected DatabaseLogin(AdminConnection connection) : this(ProtectedDataService.GlobalServiceProvider, connection)
		{
		}

		protected DatabaseLogin(IServiceProvider pdsServiceProvider, AdminConnection connection) : base(connection)
		{
			this.pdsServiceProvider = pdsServiceProvider;
		}

		public override void EnableLogin(Action<string> logMessage)
		{
			var loginRepairService = ActivatorUtilities.CreateInstance<LoginRepairService>(pdsServiceProvider);
			var caps = pdsServiceProvider.GetRequiredService<ProtectedDataServiceCapabilities>();
			if (connection.ServerName == Db.ServerName) // target server and main server are the same.
			{
				loginRepairService.ReviveApplicationLogin<TCredentials>(connection, baseDbName, connection);
			}
			else
			{
				using (var adminConnection = Db.NewAdminConnection(Db.ServerName, Db.SqlMasterDb))
				{
					loginRepairService.ReviveApplicationLogin<TCredentials>(adminConnection, baseDbName, connection);
				}
			}
			logMessage(string.Format(CultureInfo.InvariantCulture, $"Login type [{caps.GetSecretTypeName<TCredentials>()}] was checked on server."));
		}
	}
}

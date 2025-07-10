using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DataProtection;
using Microsoft.Extensions.Options;

namespace CargoWise.ProductRegistration.Service
{
	public interface IConnectionManager
	{
		void Reset();
		string TestConnect();
	}

	public sealed class ConnectionManager : IConnectionManager, IDisposable
	{
		public ConnectionManager(ISqlConnectionProvider provider, IOptions<DatabaseSettings> settings)
		{
			sqlConnectionProvider = provider;
			isConnectionOwner = true;
			this.settings = settings.Value;
		}

		public ConnectionManager(System.Data.Common.DbConnection connection, System.Data.Common.DbTransaction transaction)
		{
			this.Connection = connection;
			this.transaction = transaction;
			isConnectionOwner = false;
		}

		readonly ISqlConnectionProvider sqlConnectionProvider;
		readonly DatabaseSettings settings;

		public System.Data.Common.DbCommand CreateCmd()
		{
			CreateAndOpenConnectionIfNeeded();
			var cmd = Connection.CreateCommand();
			cmd.Transaction = transaction;
			return cmd;
		}

		public async Task<System.Data.Common.DbDataReader> ExecuteReaderAsync(System.Data.Common.DbCommand cmd)
		{
			if (isConnectionOwner)
			{
				return await cmd.ExecuteReaderAsync();
			}
			else
			{
				return cmd.ExecuteReader();
			}
		}

		public async Task<int> ExecuteNonQueryAsync(System.Data.Common.DbCommand cmd)
		{
			if (isConnectionOwner)
			{
				return await cmd.ExecuteNonQueryAsync();
			}
			else
			{
				return cmd.ExecuteNonQuery();
			}
		}

		public System.Data.Common.DbConnection CreateAndOpenConnectionIfNeeded()
		{
			if (Connection != null)
			{
				if (Connection.State == ConnectionState.Open)
				{
					return Connection;
				}
			}

			try
			{
				Connection = OpenConnection(settings.PrimaryConnectionString);
			}
			catch (System.Data.Common.DbException)
			{
				if (string.IsNullOrEmpty(settings.SecondaryConnectionString))
				{
					throw;
				}

				Connection = OpenConnection(settings.SecondaryConnectionString);
			}

			return Connection;
		}

		SqlConnection OpenConnection(string connectionString)
		{
			var serverName = new SqlConnectionStringBuilder(connectionString).DataSource;
			var conn = sqlConnectionProvider.OpenNewAdminSqlConnection(serverName, builder => builder.ConnectionString = connectionString);
			if (IsDbLockedOut(conn))
			{
				throw new DatabaseUpgradeInProgressException();
			}
			return conn;
		}

		bool IsDbLockedOut(SqlConnection connection)
		{
			var sqlText = "SELECT value FROM sys.extended_properties WITH (NOLOCK) WHERE class = 0 AND name = 'DbIsLockedOutFor'";
			using (var cmd = connection.CreateCommand())
			{
				cmd.CommandText = sqlText;
				var lockoutValue = cmd.ExecuteScalar();
				return lockoutValue != null && lockoutValue != DBNull.Value;
			}
		}

		public string TestConnect()
		{
			var primary = settings.PrimaryConnectionString;
			var secondary = settings.SecondaryConnectionString;
			var test1 = TestConnection(primary);
			var test2 = TestConnection(secondary);

			return "Primary: " + test1 + " - Secondary: " + test2;
		}

		string TestConnection(string connectionString)
		{
			if (string.IsNullOrEmpty(connectionString))
			{
				return "None";
			}

			var watch = Stopwatch.StartNew();

			try
			{
				using (var conn = OpenConnection(connectionString))
				{
					var connectionTime = watch.Elapsed;
					watch.Restart();

					using (var cmd = conn.CreateCommand())
					{
						cmd.CommandText = "select top 1 1 from dbo.LicenceDatabase";
						cmd.ExecuteScalar();
					}

					return "Connection Time: " + connectionTime.TotalSeconds.ToString("#.00", CultureInfo.InvariantCulture) + "s"
						+ ", Query Time: " + watch.Elapsed.TotalSeconds.ToString("#.00", CultureInfo.InvariantCulture) + "s";
				}
			}
			catch (System.Data.Common.DbException ex)
			{
				return "Fail Time: " + watch.Elapsed.TotalSeconds.ToString("#.00", CultureInfo.InvariantCulture) + "s - " + ex.ToString();
			}
		}

		public System.Data.Common.DbConnection Connection { get; private set; }
		readonly System.Data.Common.DbTransaction transaction;
		readonly bool isConnectionOwner;

		public void Dispose()
		{
			Reset();
		}

		public void Reset()
		{
			if (isConnectionOwner && Connection != null)
			{
				var tmp = Connection;
				Connection = null;
				tmp.Dispose();
			}
		}
	}

	public static class SqlCommandExtensions
	{
		public static System.Data.Common.DbParameter AddParameterWithValue(
			this System.Data.Common.DbCommand @this,
			string parameterName,
			object value)
		{
			Argument.NotNull(@this, nameof(@this));
			Argument.NotNullOrEmpty(parameterName, nameof(parameterName));

			var parameter = @this.CreateParameter();
			parameter.ParameterName = parameterName;
			parameter.Value = value;
			@this.Parameters.Add(parameter);
			return parameter;
		}
	}
}

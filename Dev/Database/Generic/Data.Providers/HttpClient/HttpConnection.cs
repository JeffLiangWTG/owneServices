using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data.SqlProxy.Interface.Models;

namespace CargoWise.Data.HttpClient
{
	public class HttpConnection : DbConnection, IDbConnectionInfo
	{
		public HttpConnection(string serverName, string databaseName, string applicationName, int connectTimeout)
			: this(serverName, databaseName, null, null, applicationName, connectTimeout)
		{
		}

		public HttpConnection(string serverName, string databaseName, string userName, string userPwd, string applicationName, int connectTimeout)
		{
			if (!HttpLoaderFactory.IsHttpServerConfigured)
			{
				throw new InvalidOperationException("HTTP server address must be set before using HttpConnection");
			}

			currentDatabase = databaseName;
			ServerName = serverName;
			UserName = userName;
			Password = userPwd;
			SuffixedApplicationName = applicationName;
			ConnectTimeout = connectTimeout;
		}

		string currentDatabase;
		public string ServerName { get; }
		public override string Database => currentDatabase;
		public string UserName { get; }
		public string Password { get; }
		public string SuffixedApplicationName { get; }
		public int ConnectTimeout { get; }

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
		{
			if (activeTransaction == null || !activeTransaction.IsPending)
			{
				activeTransaction = new HttpTransaction(this, isolationLevel);
			}

			return activeTransaction;
		}

		internal HttpTransaction HttpTransaction
		{
			get { return activeTransaction; }
		}
		public Guid TransactionId { get { return (activeTransaction == null) ? Guid.Empty : activeTransaction.HttpTransactionId; } }
		HttpTransaction activeTransaction;

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public override void ChangeDatabase(string databaseName)
		{
			using (var command = CreateCommand())
			{
				command.CommandText = "USE [" + databaseName + "];";
				command.ExecuteNonQuery();
			}

			currentDatabase = databaseName;
		}

		public override void Close()
		{
			if (activeTransaction != null)
			{
				activeTransaction.Dispose();
				activeTransaction = null;
			}
			state = ConnectionState.Closed;
		}

		public override string DataSource => ServerName;

		public override string ConnectionString
		{
			get { return string.Empty; }
			set { throw new InvalidOperationException("Connection string on HttpConnection may not be set"); }
		}

		public override int ConnectionTimeout => DbEnv.Instance.ConnectionTimeout;

		protected override DbCommand CreateDbCommand()
		{
			return new HttpCommand(this);
		}

		public override string ServerVersion
		{
			get
			{
				if (productVersion == null)
				{
					using var command = CreateCommand();
					command.CommandText = "SELECT SERVERPROPERTY('ProductVersion')"; // It is a SQL command
					var commandResult = command.ExecuteScalar();
					if (commandResult != null)
					{
						productVersion = commandResult.ToString();
					}
					else
					{
						throw new InvalidOperationException("Unable to get product version");
					}
				}
				return productVersion;
			}
		}
		string productVersion;

		public override void Open()
		{
			var testOpenCommand = CreateDbCommand();
			testOpenCommand.CommandText = "SELECT 0";
			testOpenCommand.ExecuteNonQuery();
			state = ConnectionState.Open;
		}

		public override ConnectionState State
		{
			get { return state; }
		}

		ConnectionState state;

		public IDisposable NewConnectingSplashFormManager()
		{
			return DbEnv.Instance.ConnectionGuiPlugin.NewConnectingSplashFormManager();
		}

		public void HandleCommandException(Exception ex)
		{
		}

		#region Dispose

		[SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", Justification = "It is already handled in Close")]
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					Close();
				}

				disposed = true;
			}
			base.Dispose(disposing);
		}

		bool disposed;

		#endregion
	}
}

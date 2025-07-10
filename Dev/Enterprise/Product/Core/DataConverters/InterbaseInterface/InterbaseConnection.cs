using System;
using System.Data;

namespace Enterprise.DataConverters.InterbaseInterface
{
	public enum ConnectionProviderType
	{
		Odbc,
		Firebird
	}

	public abstract class InterbaseConnection
	{
		public InterbaseConnection(string connectionString)
		{
			ConnectionString = !string.IsNullOrWhiteSpace(connectionString) ? connectionString : DefaultConnectionString;

			CreateAndOpenConnection();
		}

		public abstract IDbCommand GetNewCommand(string cmdText);
		public abstract IDataAdapter GetNewDataAdapter(IDbCommand command);
		protected abstract IDbConnection GetConnection();
		protected abstract string DefaultConnectionString { get; }

		#region Implementation

		void CreateAndOpenConnection()
		{
			var success = false;
			try
			{
				Connection.Close();
				Connection.Open();
				success = true;
			}
			finally
			{
				if (!success)
				{
					fConnection = null;
				}
			}
		}

		public void CloseConnection()
		{
			try
			{
				Connection.Close();
			}
			catch (Exception)
			{
				throw;
			}
		}

		protected IDbConnection Connection
		{
			get
			{
				if (fConnection == null)
				{
					fConnection = GetConnection();
				}
				return fConnection;
			}
		}

		IDbConnection fConnection;
		protected readonly string ConnectionString;

		#endregion
	}
}


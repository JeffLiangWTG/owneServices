using System.Data;
using CargoWise.Common;

namespace Enterprise.DataConverters.InterbaseInterface
{
	public class InterbaseCommandRunner
	{
		public InterbaseCommandRunner(string connectionString, ConnectionProviderType providerType)
		{
			ConnectionString = connectionString;
			ProviderType = providerType;
		}

		InterbaseConnection GetConnection()
		{
			InterbaseConnection conn = null;

			switch (ProviderType)
			{
				case ConnectionProviderType.Firebird:
					conn = new InterbaseFirebirdConnection(ConnectionString); break;
				case ConnectionProviderType.Odbc:
					conn = new InterbaseOdbcConnection(ConnectionString); break;
			}

			return conn;
		}

		public DataTable GetTableFromQuery(string text)
		{
			Argument.NotNullOrEmpty(text, nameof(text));

			InterbaseConnection connection = GetConnection();

			DataSet data = new DataSet();
			DataTable result = new DataTable();

			IDbCommand cmd = connection.GetNewCommand(text);
			var success = false;
			try
			{
				IDataAdapter adapter = connection.GetNewDataAdapter(cmd);
				adapter.Fill(data);
				success = true;
			}
			finally
			{
				if (!success)
				{
					data.Clear();
				}
			}

			connection.CloseConnection();

			if (data.Tables.Count > 0)
			{
				result = data.Tables[0];
			}
			return result;
		}

		readonly string ConnectionString;
		readonly ConnectionProviderType ProviderType;
	}
}

using System.Data;
using FirebirdSql.Data.FirebirdClient;

namespace Enterprise.DataConverters.InterbaseInterface
{
	public class InterbaseFirebirdConnection : InterbaseConnection
	{
		public InterbaseFirebirdConnection(string connectionString) : base(connectionString)
		{
		}

		public override IDbCommand GetNewCommand(string cmdText)
		{
			IDbCommand result = new FbCommand(cmdText, (FbConnection)Connection);
			return result;
		}

		public override IDataAdapter GetNewDataAdapter(IDbCommand command)
		{
			IDataAdapter result = new FbDataAdapter((FbCommand)command);
			return result;
		}

		protected override IDbConnection GetConnection()
		{
			return new FbConnection(ConnectionString);
		}

		protected override string DefaultConnectionString
		{
			get
			{
				string defaultConn = @"Database=Dbcyber2.gdb;User=SYSDBA;Password=masterkey;Dialect=3;Server=" + System.Environment.MachineName;
				return defaultConn;
			}
		}
	}
}


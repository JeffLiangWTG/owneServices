using System.Data;
#if NETFRAMEWORK
using System.Data.Odbc;
#else
using System.Data.OleDb;
#endif

namespace Enterprise.DataConverters.InterbaseInterface
{
	public class InterbaseOdbcConnection  : InterbaseConnection
	{
		public InterbaseOdbcConnection(string connectionString) : base(connectionString)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "Baseline")]
		public override IDbCommand GetNewCommand(string cmdText)
		{
#if NETFRAMEWORK
			var command = new OdbcCommand(cmdText);
#else
			var command = new OleDbCommand(cmdText);
#endif
			return command;
		}

		public override IDataAdapter GetNewDataAdapter(IDbCommand command)
		{
#if NETFRAMEWORK
			var adapter = new OdbcDataAdapter((OdbcCommand)command);
			adapter.SelectCommand.Connection = (OdbcConnection)ODBCConnection;
#else
			var adapter = new OleDbDataAdapter((OleDbCommand)command);
			adapter.SelectCommand.Connection = (OleDbConnection)ODBCConnection;
#endif
			return adapter;
		}

		protected override IDbConnection GetConnection()
		{
			return ODBCConnection;
		}

		IDbConnection ODBCConnection
		{
			get
			{
				if (fConnection == null)
				{
#if NETFRAMEWORK
					fConnection = new OdbcConnection(ConnectionString);
#else
					fConnection = new OleDbConnection(ConnectionString);
#endif
				}
				return fConnection;
			}
		}
		IDbConnection fConnection;

		protected override string DefaultConnectionString
		{
			get {	return @"Driver={INTERSOLV InterBase ODBC Driver (*.gdb)};Server=Cyber2;Database=Dbcyber2.gdb;Uid=SYSDBA;Pwd=masterkey" ;	}
		}
	}
}

using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using Enterprise.Integration;

namespace CargoWise.Bi.Product.DataLoad.Helper
{
	/// <summary>
	/// Forwards sql server message logs to the given logger
	/// The log level is determined by severityMapper from the sql server error severity, which is 0 to 10 for non-error messages
	/// </summary>
	[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Full name needed to identify SqlClient types")]
	internal class SqlMessagePassthroughLogger : IDisposable
	{
		public SqlMessagePassthroughLogger(DbConnection connection, ILogger logger, Func<int, LogType> severityMapper = null)
		{
			_connection = connection;
			_logger = logger;
			_severityMapper = severityMapper ?? DefaultSeverityMapper;

			var dbConnection = ((IDbConnectionInternals)_connection).ADOConnection;
			if (dbConnection is System.Data.SqlClient.SqlConnection sysConnection)
			{
				sysConnection.InfoMessage += HandleMessageSys;
				return;
			}
#if NET
			if (dbConnection is Microsoft.Data.SqlClient.SqlConnection msConnection)
			{
				msConnection.InfoMessage += HandleMessageMS;
				return;
			}
#endif
		}

		void HandleMessageSys(object sender, System.Data.SqlClient.SqlInfoMessageEventArgs messages)
		{
			foreach (var message_obj in messages.Errors)
			{
				if (message_obj is System.Data.SqlClient.SqlError message)
				{
					_logger.Log(_severityMapper(message.Class), message.Message);
				}
			}
		}

#if NET
		void HandleMessageMS(object sender, Microsoft.Data.SqlClient.SqlInfoMessageEventArgs messages)
		{
			foreach (var message_obj in messages.Errors)
			{
				if (message_obj is Microsoft.Data.SqlClient.SqlError message)
				{
					_logger.Log(_severityMapper(message.Class), message.Message);
				}
			}
		}
#endif

		LogType DefaultSeverityMapper(int severityNum)
		{
			switch (severityNum)
			{
				case 0:
					return LogType.Debug;
				case 1:
					return LogType.Information;
				case 2:
					return LogType.Warning;
				case 3:
					return LogType.Error;
				default:
					return LogType.Debug;
			}
		}

		public void Dispose()
		{
			var dbConnection = ((IDbConnectionInternals)_connection).ADOConnection;
			if (dbConnection is System.Data.SqlClient.SqlConnection sysConnection)
			{
				sysConnection.InfoMessage -= HandleMessageSys;
				return;
			}
#if NET
			if (dbConnection is Microsoft.Data.SqlClient.SqlConnection msConnection)
			{
				msConnection.InfoMessage -= HandleMessageMS;
				return;
			}
#endif
		}

		readonly DbConnection _connection;
		readonly ILogger _logger;
		readonly Func<int, LogType> _severityMapper;
	}
}

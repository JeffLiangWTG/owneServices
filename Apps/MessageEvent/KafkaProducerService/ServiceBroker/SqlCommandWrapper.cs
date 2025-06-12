using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.ServiceBroker
{
	public class SqlCommandWrapper : ISqlCommandWrapper
	{
		private readonly SqlCommand _command;

		public SqlCommandWrapper(SqlCommand command)
		{
			_command = command;
		}

		public async Task<DbDataReader> ExecuteReaderAsync()
		{
			return await _command.ExecuteReaderAsync();
		}

		#region delegate methods

		public void Dispose()
		{
			_command.Dispose();
		}

		public void Cancel()
		{
			_command.Cancel();
		}

		public IDbDataParameter CreateParameter()
		{
			return ((IDbCommand)_command).CreateParameter();
		}

		public int ExecuteNonQuery()
		{
			return _command.ExecuteNonQuery();
		}

		public IDataReader ExecuteReader()
		{
			return ((IDbCommand)_command).ExecuteReader();
		}

		public IDataReader ExecuteReader(CommandBehavior behavior)
		{
			return ((IDbCommand)_command).ExecuteReader(behavior);
		}

		public object? ExecuteScalar()
		{
			return _command.ExecuteScalar();
		}

		public void Prepare()
		{
			_command.Prepare();
		}

		[AllowNull]
		public string CommandText
		{
			get => _command.CommandText;
			set => _command.CommandText = value;
		}

		public int CommandTimeout
		{
			get => _command.CommandTimeout;
			set => _command.CommandTimeout = value;
		}

		public CommandType CommandType
		{
			get => _command.CommandType;
			set => _command.CommandType = value;
		}

		public IDbConnection? Connection
		{
			get => ((IDbCommand)_command).Connection;
			set => ((IDbCommand)_command).Connection = value;
		}

		public IDataParameterCollection Parameters => ((IDbCommand)_command).Parameters;

		public IDbTransaction? Transaction
		{
			get => ((IDbCommand)_command).Transaction;
			set => ((IDbCommand)_command).Transaction = value;
		}

		public UpdateRowSource UpdatedRowSource
		{
			get => _command.UpdatedRowSource;
			set => _command.UpdatedRowSource = value;
		}

		#endregion
	}
}

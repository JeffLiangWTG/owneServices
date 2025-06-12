using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Logging;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Options;
using CargoWise.eHub.MessageEvent.KafkaProducerService.sql;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.ServiceBroker
{
	public class MessageEventServiceBroker : IMessageEventServiceBroker
	{
		private readonly ServiceBrokerOptions _options;
		private IDbTransaction? _transaction;
		private readonly IDbConnection _connection;
		private readonly ILogger<MessageEventServiceBroker> _logger;

		public MessageEventServiceBroker(IOptions<ServiceBrokerOptions> options, IDbConnection dbConnection, ILogger<MessageEventServiceBroker> logger)
		{
			_options = options.Value;
			_connection = dbConnection;
			_logger = logger;
		}

		public void BeginTransaction()
		{
			if (_transaction != null)
			{
				throw new InvalidOperationException("Transaction is already exists.");
			}

			if (_connection.State == ConnectionState.Closed)
			{
				_connection.Open();
			}

			_transaction = _connection.BeginTransaction();
		}

		public void Commit()
		{
			if (_transaction == null)
			{
				throw new InvalidOperationException("Transaction is not exists.");
			}

			_transaction.Commit();
			_transaction = null;
		}

		public void Dispose()
		{
			_connection.Dispose();
		}

		public async IAsyncEnumerable<XmlDocument> GetNextBatchMessagesAsync()
		{
			_logger.eHubLog(LogLevel.Trace, "", "", "", "{className} starting", nameof(GetNextBatchMessagesAsync));
			using var command = GetCommandCore(_connection);
			command.Transaction = _transaction;
			command.CommandText = sql_command.receive_next_batch_messages;
			command.Parameters.Add(new SqlParameter("@batchSize", SqlDbType.Int) { Value = _options.BatchSize });

			await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
			while (await reader.ReadAsync().ConfigureAwait(false))
			{
				var content = reader.GetString(0);
				var message = new XmlDocument();
				message.LoadXml(content);
				yield return message;
			}
			_logger.eHubLog(LogLevel.Trace, "", "", "", "{className} finished", nameof(GetNextBatchMessagesAsync));
		}

		internal virtual ISqlCommandWrapper GetCommandCore(IDbConnection connection)
		{
			if (!(connection is SqlConnection con))
			{
				throw new Exception($"Expect {nameof(connection)} to be {nameof(SqlConnection)}.");
			}

			var command = con.CreateCommand();
			return new SqlCommandWrapper(command);
		}

		public void Rollback()
		{
			if (_transaction == null)
			{
				throw new InvalidOperationException("Transaction is not exists.");
			}

			_transaction.Rollback();
			_transaction = null;
		}
	}
}

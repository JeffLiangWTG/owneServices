using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Logging;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Options;
using CargoWise.eHub.MessageEvent.KafkaProducerService.ServiceBroker;
using CargoWise.eHub.MessageEvent.KafkaProducerService.sql;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Serilog;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.Logging
{
	public class MessageEventServiceBrokerLoggingTests
	{
		private StringWriter _writer;
		private Mock<IDbConnection> _connectionMock;
		private Mock<DbDataReader> _readerMock;
		private Mock<IDataParameterCollection> _parameterCollectionMock;
		private Mock<IOptions<ServiceBrokerOptions>> _optionsMock;
		private MessageEventServiceBroker _serviceBroker;
		private Mock<ISqlCommandWrapper> _sqlCommandWrapperMock;
		private Mock<IDbTransaction> _transactionMock;
		private List<string> _messages;
		private List<string>.Enumerator _messagesEnumerator;

		[SetUp]
		public void SetUp()
		{
			_writer = new StringWriter();
			Console.SetOut(_writer);

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Verbose()
				.Enrich.FromLogContext()
				.Enrich.WithMachineName()
				.Enrich.WithProcessId()
				.Enrich.WithThreadId()
				.Enrich.With<LogLevelEnricher>()
				.Enrich.With<ContextSourceReplaceEnricher>()
				.WriteTo.Console(
					formatter: new IndentationMessageTemplateTextFormatter()
				)
				.CreateBootstrapLogger();

			var loggerFactory = new LoggerFactory().AddSerilog();
			var _logger = loggerFactory.CreateLogger<MessageEventServiceBroker>();

			const int batchSize = 5;
			_messages = new List<string> { "<Message>1</Message>", "<Message>2</Message>", "<Message>3</Message>" };
			_messagesEnumerator = _messages.GetEnumerator();

			_connectionMock = new Mock<IDbConnection>();
			_readerMock = new Mock<DbDataReader>();
			_parameterCollectionMock = new Mock<IDataParameterCollection>();
			_optionsMock = new Mock<IOptions<ServiceBrokerOptions>>();
			_sqlCommandWrapperMock = new Mock<ISqlCommandWrapper>();
			_transactionMock = new Mock<IDbTransaction>();

			_connectionMock.Setup(x => x.BeginTransaction()).Returns(_transactionMock.Object);
			_sqlCommandWrapperMock.Setup(x => x.ExecuteReaderAsync()).ReturnsAsync(_readerMock.Object);
			_sqlCommandWrapperMock.SetupGet(x => x.Parameters).Returns(_parameterCollectionMock.Object);
			_optionsMock.Setup(x => x.Value).Returns(new ServiceBrokerOptions { BatchSize = batchSize, TimeoutSecond = 15 });
			_parameterCollectionMock.Setup(x => x.Add(It.Is<SqlParameter>(p => p.ParameterName == "@batchSize" && p.SqlDbType == SqlDbType.Int && (int)(p.Value) == batchSize))).Verifiable();

			_readerMock.Setup(x => x.ReadAsync(It.IsAny<CancellationToken>())).Returns<CancellationToken>(token => Task.FromResult(_messagesEnumerator.MoveNext()));
			_readerMock.Setup(x => x.GetString(0)).Returns<int>(position => _messagesEnumerator.Current);

			var serviceBrokerMock = new Mock<MessageEventServiceBroker>(_optionsMock.Object, _connectionMock.Object, _logger) { CallBase = true };
			serviceBrokerMock.Setup(x => x.GetCommandCore(It.IsAny<IDbConnection>())).Returns(_sqlCommandWrapperMock.Object);
			_serviceBroker = serviceBrokerMock.Object;
		}

		[TearDown]
		public void TearDown()
		{
			_messagesEnumerator.Dispose();
		}

		[Test]
		public async Task TestGetNextBatchMessagesAsync_AllServiceCalled()
		{
			_serviceBroker.BeginTransaction();
			var count = await _serviceBroker.GetNextBatchMessagesAsync().CountAsync();

			Mock.Get(_serviceBroker).Verify(x => x.GetCommandCore(_connectionMock.Object), Times.Once);
			_sqlCommandWrapperMock.VerifySet(x => x.Transaction = _transactionMock.Object, Times.Once);
			_sqlCommandWrapperMock.VerifySet(x => x.CommandText = sql_command.receive_next_batch_messages, Times.Once);
			_parameterCollectionMock.VerifyAll();
			_sqlCommandWrapperMock.Verify(x => x.ExecuteReaderAsync(), Times.Once);
			_readerMock.Verify(x => x.ReadAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce);
			_readerMock.Verify(x => x.GetString(0), Times.AtLeastOnce);
			_sqlCommandWrapperMock.Verify(x => x.Dispose(), Times.Once);
			_readerMock.Verify(x => x.DisposeAsync(), Times.Once);

			Assert.That(TestLog.Message.SplitAndCheck(_writer.ToString()), Is.True);

		}
	}

	
}

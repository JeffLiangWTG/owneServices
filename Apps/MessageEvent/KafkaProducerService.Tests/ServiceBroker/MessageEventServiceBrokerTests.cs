using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Options;
using CargoWise.eHub.MessageEvent.KafkaProducerService.ServiceBroker;
using CargoWise.eHub.MessageEvent.KafkaProducerService.sql;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.Tests.ServiceBroker
{
	public class MessageEventServiceBrokerTests
	{
		private Mock<IDbConnection> _connectionMock;
		private Mock<ILogger<MessageEventServiceBroker>> _loggerMock;
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
			const int batchSize = 5;
			_messages = new List<string> { "<Message>1</Message>", "<Message>2</Message>", "<Message>3</Message>" };
			_messagesEnumerator = _messages.GetEnumerator();

			_connectionMock = new Mock<IDbConnection>();
			_loggerMock = new Mock<ILogger<MessageEventServiceBroker>>();
			_loggerMock.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
			_readerMock = new Mock<DbDataReader>();
			_parameterCollectionMock = new Mock<IDataParameterCollection>();
			_optionsMock = new Mock<IOptions<ServiceBrokerOptions>>();
			_sqlCommandWrapperMock = new Mock<ISqlCommandWrapper>();
			_transactionMock = new Mock<IDbTransaction>();

			_connectionMock.Setup(x => x.BeginTransaction()).Returns(_transactionMock.Object);
			_sqlCommandWrapperMock.Setup(x => x.ExecuteReaderAsync()).ReturnsAsync(_readerMock.Object);
			_sqlCommandWrapperMock.SetupGet(x => x.Parameters).Returns(_parameterCollectionMock.Object);
			_optionsMock.Setup(x => x.Value).Returns(new ServiceBrokerOptions { BatchSize = batchSize, TimeoutSecond = 15 });
			_parameterCollectionMock.Setup(x  => x.Add(It.Is<SqlParameter>(p => p.ParameterName == "@batchSize" && p.SqlDbType == SqlDbType.Int && (int)(p.Value) == batchSize))).Verifiable();

			_readerMock.Setup(x => x.ReadAsync(It.IsAny<CancellationToken>())).Returns<CancellationToken>(token => Task.FromResult(_messagesEnumerator.MoveNext()));
			_readerMock.Setup(x => x.GetString(0)).Returns<int>(position => _messagesEnumerator.Current);

			var serviceBrokerMock = new Mock<MessageEventServiceBroker>(_optionsMock.Object, _connectionMock.Object, _loggerMock.Object) { CallBase = true };
			serviceBrokerMock.Setup(x => x.GetCommandCore(It.IsAny<IDbConnection>())).Returns(_sqlCommandWrapperMock.Object);
			_serviceBroker = serviceBrokerMock.Object;
		}

		[TearDown]
		public void TearDown()
		{
			_messagesEnumerator.Dispose();
		}

		[Test]
		public void TestBeginTransaction()
		{
			_serviceBroker.BeginTransaction();

			_connectionMock.Verify(x => x.Open(), Times.Once);
			_connectionMock.Verify(x => x.BeginTransaction(), Times.Once);
		}

		[Test]
		public void TestBeginTransaction_SkipOpen()
		{
			_connectionMock.SetupGet(x => x.State).Returns(ConnectionState.Open);
			_serviceBroker.BeginTransaction();

			_connectionMock.Verify(x => x.Open(), Times.Never);
		}

		[Test]
		public void TestCommit()
		{
			_serviceBroker.BeginTransaction();
			_serviceBroker.Commit();

			_transactionMock.Verify(x => x.Commit(), Times.Once);
		}

		[Test]
		public void TestCommit_NoExistingTransaction_ThrowsException()
		{
			Assert.That(() => _serviceBroker.Commit(), Throws.InstanceOf<Exception>().With.Message.EqualTo("Transaction is not exists."));
		}

		[Test]
		public void TestRollback()
		{
			_serviceBroker.BeginTransaction();
			_serviceBroker.Rollback();

			_transactionMock.Verify(x => x.Rollback(), Times.Once);
		}

		[Test]
		public void TestRollback_NoExistingTransaction_ThrowsException()
		{
			Assert.That(() => _serviceBroker.Rollback(), Throws.InstanceOf<Exception>().With.Message.EqualTo("Transaction is not exists."));
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

			_loggerMock.Verify(x => x.Log(
					LogLevel.Trace,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((o, t) => o.ToString() == "GetNextBatchMessagesAsync starting"),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);
			_loggerMock.Verify(x => x.Log(
					LogLevel.Trace,
					It.IsAny<EventId>(),
					It.Is<It.IsAnyType>((o, t) => o.ToString() == "GetNextBatchMessagesAsync finished"),
					It.IsAny<Exception>(),
					It.IsAny<Func<It.IsAnyType, Exception, string>>()),
				Times.Once);

			Assert.That(count, Is.EqualTo(_messages.Count));
		}
	}
}

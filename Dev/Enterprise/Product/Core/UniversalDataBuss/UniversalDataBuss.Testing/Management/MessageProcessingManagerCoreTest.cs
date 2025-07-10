using System;
using System.IO;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.UniversalDataBuss.Testing.Management
{
	class MessageProcessingManagerCoreTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestMessageStatusRejectedCalledInternallyDontSendNotification()
		{
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			var testManager = new TestMessageProcessingManagerCore(messageLogger, MessageStatus.Rejected, null);

			var message = GetQueuedUniversalShipmentMessage("", false);
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;

			testManager.Process(message);

			AssertEquals(message, messageLogger.SourceMessage);
			AssertEquals(EDIMessageStatusList.Codes.Rejected, message.EM_Status);
		}

		public void TestMessageStatusRejected()
		{
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			var testManager = new TestMessageProcessingManagerCore(messageLogger, MessageStatus.Rejected, null);

			var message = GetQueuedUniversalShipmentMessage("", false);
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			testManager.Process(message);

			AssertEquals(message, messageLogger.SourceMessage);
			AssertEquals(EDIMessageStatusList.Codes.Rejected, message.EM_Status);
		}

		public void TestMessageStatusDiscarded()
		{
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			var testManager = new TestMessageProcessingManagerCore(messageLogger, MessageStatus.Discarded, null);

			var message = GetQueuedUniversalShipmentMessage("", false);
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			testManager.Process(message);

			AssertEquals(message, messageLogger.SourceMessage);
			AssertEquals(EDIMessageStatusList.Codes.Discarded, message.EM_Status);
		}

		public void TestMessageStatusError()
		{
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			var testManager = new TestMessageProcessingManagerCore(messageLogger, MessageStatus.Processed, null);

			var message = GetQueuedUniversalShipmentMessage("", false);
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			messageLogger.Log(LogType.Error, "Some error.");

			testManager.Process(message);

			AssertEquals(message, messageLogger.SourceMessage);
			AssertEquals(EDIMessageStatusList.Codes.Error, message.EM_Status);
		}

		public void TestMessageStatusWarning()
		{
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			var testManager = new TestMessageProcessingManagerCore(messageLogger, MessageStatus.Processed, null);

			var message = GetQueuedUniversalShipmentMessage("", false);
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			messageLogger.Log(LogType.Warning, "Some warning.");

			testManager.Process(message);

			AssertEquals(message, messageLogger.SourceMessage);
			AssertEquals(EDIMessageStatusList.Codes.Warning, message.EM_Status);
		}

		public void TestMessageStatusProcessedOK()
		{
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);

			var testManager = new TestMessageProcessingManagerCore(messageLogger, MessageStatus.Processed, null);

			var message = GetQueuedUniversalShipmentMessage("", false);
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			testManager.Process(message);

			AssertEquals(message, messageLogger.SourceMessage);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
		}

		public void TestMessageStatusNullException()
		{
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			var testManager = new TestMessageProcessingManagerCore(messageLogger, MessageStatus.Processed, null);

			AssertExceptionThrown(typeof(ArgumentNullException), () => testManager.Process(null));
		}

		#region TestDeadlockExceptionThrowsMessageProcessingBusinessFailureException

		public void TestDeadlockExceptionThrowsMessageProcessingBusinessFailureException()
		{
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			var testManager = new TestMessageProcessingManagerCore(messageLogger, MessageStatus.Rejected, CreateZSaveExceptionIncludingDeadlock());

			var message = GetQueuedUniversalEventMessage("", false);
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Internal;

			AssertExceptionThrown(typeof(MessageProcessingBusinessFailureException), () => testManager.Process(message));
		}

		public void TestNotifyUsersWhenException()
		{
			var serviceLogger = new ServiceTaskLogForTesting();
			var messageLogger = new XmlSessionTracker(serviceLogger);
			var testManager = new TestMessageProcessingManagerCore(messageLogger, MessageStatus.Rejected, new IOException());

			var message = GetQueuedUniversalEventMessage("", false);
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;

			AssertExceptionThrown(typeof(IOException), () => testManager.Process(message));
		}

		public void TestXmlSessionTrackerLoggerNotNullable()
		{
			ServiceTaskLogForTesting serviceLogger = null;
			AssertExceptionThrown<ArgumentException>(() =>
			{
				new XmlSessionTracker(serviceLogger);
			});
		}

		#endregion

		#region CreateZSaveExceptionIncludingDeadlock

		ZSaveException CreateZSaveExceptionIncludingDeadlock()
		{
			var error = SqlExceptionBuilder.CreateSqlError(1205, 1, 1, Db.ServerName, "Transaction (Process ID 102) was deadlocked on lock resources with another process and has been chosen as the deadlock victim.", "", 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var sqlEx = SqlExceptionBuilder.CreateSqlException(errors);
			var dataEx = new ZDataException(sqlEx, null, null);
			return new ZSaveException(dataEx, new BusinessObjectFactory());
		}

		#endregion
	}

	class TestUniversalObjectFactoryLocator : IEDIMessageUniversalObjectFactoryLocator
	{
		public IUniversalObjectFactory GetFactory(IEDIMessage message, IXmlSessionTracker logger)
		{
			var factory = new UniversalObjectFactory();
			factory.SetDataRefresh(message.Factory.RefreshEnabled);
			return factory;
		}
	}

	class TestMessageProcessingManagerCore : IMessageProcessingManager
	{
		readonly MessageProcessingManagerCore messageProcessingManager;

		public MessageStatus ReturnStatus { get; private set; }
		public Exception ReturnException { get; private set; }

		public IXmlSessionTracker Logger => messageProcessingManager.Logger;

		public IDataWritingManager OutboundSessionTracker
		{
			set => messageProcessingManager.OutboundSessionTracker = value;
		}

		public TestMessageProcessingManagerCore(XmlSessionTracker logger, MessageStatus returnStatus, Exception returnException)
		{
			ReturnStatus = returnStatus;
			ReturnException = returnException;
			messageProcessingManager = new MessageProcessingManagerCore(logger, GetMessageProcessor, (_) => new TestUniversalObjectFactoryLocator());
		}

		IDataMessageProcessor GetMessageProcessor(string messageSubType)
		{
			return ReturnException == null ? new TestDataMessageProcessor(ReturnStatus) : new TestDataMessageProcessor(ReturnException);
		}

		public IXmlSessionTracker Process(IEDIMessage message, ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager mapper = null, IDelayedTransactionManager transaction = null)
		{
			return messageProcessingManager.Process(message, topLevelDataObject, recordBillingInformation, mapper, transaction);
		}
	}

	class TestDataMessageProcessor : IDataMessageProcessor
	{
		public MessageStatus ReturnStatus { get; private set; }
		public Exception ReturnException { get; private set; }

		public TestDataMessageProcessor(MessageStatus status)
		{
			ReturnStatus = status;
			ReturnException = null;
		}

		public TestDataMessageProcessor(Exception exception)
		{
			ReturnException = exception;
		}

		public MessageStatus Process(IUniversalObjectFactory factory, IEDIMessage message, IXmlSessionTracker logger, ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager mapper = null)
		{
			if (ReturnException != null)
			{
				throw ReturnException;
			}

			return ReturnStatus;
		}
	}
}

using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Management;

namespace Enterprise.UniversalDataBuss.Management.Testing
{
	class MessageProcessingManagerTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestInvalidInboundUniversalShipment()
		{
			var message = GetQueuedUniversalShipmentMessage("<UniversalShipment></UniversalShipment>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Rejected, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Failed to parse XML. Errors found:-
Error - Line 1: Root element <UniversalShipment> must contain one <Shipment> element. </UniversalShipment> is not valid in this scope.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Error - Line 1: Root element <UniversalShipment> must contain one <Shipment> element. </UniversalShipment> is not valid in this scope.
Message Rejected.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestInvalidInboundUniversalEvent()
		{
			var message = GetQueuedUniversalEventMessage("<UniversalEvent></UniversalEvent>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Rejected, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Failed to parse XML. Errors found:-
Error - Line 1: Root element <UniversalEvent> must contain one <Event> element. </UniversalEvent> is not valid in this scope.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Error - Line 1: Root element <UniversalEvent> must contain one <Event> element. </UniversalEvent> is not valid in this scope.
Message Rejected.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestInvalidInboundUniversalSchedule()
		{
			var message = GetQueuedUniversalScheduleMessage("<UniversalSchedule></UniversalSchedules>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Rejected, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Failed to parse XML. Errors found:-
Error - Line 1: Root element <UniversalSchedule> must contain one <Schedule> element. </UniversalSchedules> is not valid in this scope.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Error - Line 1: Root element <UniversalSchedule> must contain one <Schedule> element. </UniversalSchedules> is not valid in this scope.
Message Rejected.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestInvalidInboundUniversalTransactionBatch()
		{
			var message = GetQueuedUniversalTransactionBatchMessage("<UniversalTransactionBatch></UniversalTransactionBatch>");

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);

			CombineAssertions(delegate
			{
				AssertEquals(EDIMessageStatusList.Codes.Rejected, message.EM_Status);

				AssertMultilineASCIIEquals("Service Task Log", @"
ERROR - Failed to parse XML. Errors found:-
Error - Line 1: Root element <UniversalTransactionBatch> must contain one <TransactionBatch> element. </UniversalTransactionBatch> is not valid in this scope.
".Trim(), serviceTaskLog.ToString());

				AssertMultilineASCIIEquals("Message Log Note", @"
Error - Line 1: Root element <UniversalTransactionBatch> must contain one <TransactionBatch> element. </UniversalTransactionBatch> is not valid in this scope.
Message Rejected.
".Trim(), message.GetLogNoteText());
			});
		}

		public void TestUnknownMessageTypeDoesNotThrowException()
		{
			var message = GetQueuedUniversalEventMessage("<RandomStuff></RandomStuff>");
			message.EM_MessageSubType = "XOX";

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			AssertNoExceptionThrown("Unhandled Message Sub Type [XOX].", delegate { manager.Process(message); });
		}

		public void TestWrapExceptionToMessageProcessingBusinessFailureException()
		{
			var message = GetQueuedUniversalShipmentMessage("<UniversalShipment></UniversalShipment>");
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManagerForTest(new NullReferenceException("Object reference not set to an instance of this object."));
			AssertExceptionThrown<NullReferenceException>(() => manager.Process(message));

			manager.ExceptionToThrow = new InvalidOperationException();
			AssertExceptionThrown<InvalidOperationException>(() => manager.Process(message));

			manager.ExceptionToThrow = new MessageProcessingBusinessFailureException();
			AssertExceptionThrown<MessageProcessingBusinessFailureException>(() => manager.Process(message));

			manager.ExceptionToThrow = new ZCannotSaveException(string.Empty, string.Empty);
			AssertExceptionThrown<MessageProcessingBusinessFailureException>(() => manager.Process(message));
		}

		class TestStream : Stream
		{
			long totalBytes;

			public override bool CanRead => true;

			public override bool CanSeek => false;

			public override bool CanWrite => false;

			public override long Length => throw new NotSupportedException();

			public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

			public override void Flush()
			{
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				totalBytes += count;

				if (totalBytes >= 3221225472)
				{
					return 0;
				}

				return count;
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotSupportedException();
			}

			public override void SetLength(long value)
			{
				throw new NotSupportedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException();
			}
		}

		public void TestParseGreaterThan2GB()
		{
			TextReader reader = new StreamReader(new TestStream());
			AssertNoExceptionThrown(() => reader.Parse<Activity>());
		}

		public void TestGetMessageProcessor_ShouldHandleUniversalActivity()
		{
			var message = GetQueuedUniversalActivityMessage("<UniversalActivity><Activity></Activity></UniversalActivity>");
			var logger = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(logger);

			AssertNoExceptionThrown("Universal Activity message should be able to be processed. SAD!", () => manager.Process(message));
		}

		#region Helper Class
		class UniversalMessageProcessingManagerForTest : IMessageProcessingManager
		{
			readonly TestMessageProcessorForExceptionThrowing messageProcessor;
			readonly MessageProcessingManagerCore messageProcessingManagerCore;

			public Exception ExceptionToThrow
			{
				get { return messageProcessor.ExceptionToThrow; }
				set { messageProcessor.ExceptionToThrow = value; }
			}

			public IXmlSessionTracker Logger => throw new NotImplementedException();

			public IDataWritingManager OutboundSessionTracker { set => throw new NotImplementedException(); }

			public UniversalMessageProcessingManagerForTest(Exception exceptionToThrow)
			{
				messageProcessor = new TestMessageProcessorForExceptionThrowing(exceptionToThrow);
				this.messageProcessingManagerCore = new MessageProcessingManagerCore(new XmlSessionTracker(new ServiceTaskLogForTesting()), GetMessageProcessor, (_) => new TestUniversalObjectFactoryLocator());
			}

			IDataMessageProcessor GetMessageProcessor(string messageSubType)
			{
				return messageProcessor;
			}

			public IXmlSessionTracker Process(IEDIMessage message, ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager mapper = null, IDelayedTransactionManager transaction = null)
			{
				return messageProcessingManagerCore.Process(message, topLevelDataObject, recordBillingInformation, mapper, transaction);
			}
		}

		class TestMessageProcessorForExceptionThrowing : IDataMessageProcessor
		{
			public Exception ExceptionToThrow { get; set; }

			public bool ThrowConcurrencyExceptionOnSave { get; set; }

			public TestMessageProcessorForExceptionThrowing() : this(null) { }

			public TestMessageProcessorForExceptionThrowing(Exception exceptionToThrow)
			{
				ExceptionToThrow = exceptionToThrow;
			}

			public MessageStatus Process(IUniversalObjectFactory factory, IEDIMessage message, IXmlSessionTracker logger, ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager mapper = null)
			{
				if (ExceptionToThrow != null)
				{
					throw ExceptionToThrow;
				}

				return MessageStatus.Processed;
			}
		}
		#endregion
	}
}

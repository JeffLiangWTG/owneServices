using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal abstract class JXCMessageProcessorTestCase : JXCMessageProcessorBaseTest
	{
		public void TestConstructor()
		{
			JXCRecord[] records = System.Array.Empty<JXCRecord>();
			JXCMessageProcessor messageProcessor = GetNewMessageProcessor(records);
			AssertEquals("Should be assigned in the constructor", records, messageProcessor.Records);
			AssertEquals("Should be empty array if there is not enough records", 0, messageProcessor.BodyRecords.Length);
			records = new JXCRecord[] { new DummyRecord("", ""), new DummyRecord("", ""), new DummyRecord("", ""), new DummyRecord("", "") };
			messageProcessor = GetNewMessageProcessor(records);
			AssertEquals("Should be assigned in the constructor", records, messageProcessor.Records);
			AssertEquals("Should not include header and trailer", 2, messageProcessor.BodyRecords.Length);
			AssertEquals(messageProcessor.Records[1], messageProcessor.BodyRecords[0]);
			AssertEquals(messageProcessor.Records[2], messageProcessor.BodyRecords[1]);
		}

		public void TestProcessRecords_NotEnoughRecordLines()
		{
			JXCRecord[] records = System.Array.Empty<JXCRecord>();
			JXCMessageProcessor messageProcessor = GetNewMessageProcessor(records);
			bool result = messageProcessor.ProcessRecords(FactoryProvider, NotificationBuffer);
			Assert("Should be false as there is not enough record lines", !result);
			AssertErrorNotification(ErrorType.InvalidFileFormat, "Invalid JXC file");
		}

		public void TestProcessRecords_NoHeaderAndOrTrailer()
		{
			JXCRecord[] noHeaderAndTrailerRecords = new JXCRecord[] { new DummyRecord("TEST", "123"), new DummyRecord("TEST", "345"), new DummyRecord("TEST", "567") };
			JXCMessageProcessor messageProcessor = GetNewMessageProcessor(noHeaderAndTrailerRecords);
			bool result = messageProcessor.ProcessRecords(FactoryProvider, NotificationBuffer);
			Assert("Should be false, no header and trailer", !result);
			AssertErrorNotification(ErrorType.InvalidFileFormat, "HEAD and/or TRLR does not exist.");
			NotificationBuffer.Clear();
			JXCRecord[] noTrailerRecords = new JXCRecord[] { new HEADRecord("HEAD", "123"), new DummyRecord("TEST", "345"), new DummyRecord("TEST", "567") };
			messageProcessor = GetNewMessageProcessor(noTrailerRecords);
			result = messageProcessor.ProcessRecords(FactoryProvider, NotificationBuffer);
			Assert("Should be false, no trailer", !result);
			AssertErrorNotification(ErrorType.InvalidFileFormat, "HEAD and/or TRLR does not exist.");
			NotificationBuffer.Clear();
			JXCRecord[] noHeaderRecord = new JXCRecord[] { new TRLRRecord("TRLR", "123"), new TRLRRecord("TRLR", "345"), new TRLRRecord("TRLR", "567") };
			messageProcessor = GetNewMessageProcessor(noHeaderRecord);
			result = messageProcessor.ProcessRecords(FactoryProvider, NotificationBuffer);
			Assert("Should be false, no header", !result);
			AssertErrorNotification(ErrorType.InvalidFileFormat, "HEAD and/or TRLR does not exist.");
		}

		public void TestProcessRecords_InvalidFirstLineType()
		{
			JXCRecord[] records = new JXCRecord[] { new HEADRecord("HEAD", ""), new DummyRecord("TEST", "345"), new TRLRRecord("TRLR", "") };
			JXCMessageProcessor messageProcessor = GetNewMessageProcessor(records);
			bool result = messageProcessor.ProcessRecords(FactoryProvider, NotificationBuffer);
			Assert("Should be false, invalid first line type", !result);
			AssertErrorNotification(ErrorType.InvalidFileFormat, "First body line (TEST) is invalid");
		}

		public void TestMessageType()
		{
			JXCRecord[] records = new JXCRecord[] { new HEADRecord("HEAD", ""), new DummyRecord("TEST", "345"), new TRLRRecord("TRLR", "") };
			JXCMessageProcessor messageProcessor = GetNewMessageProcessor(records);
			AssertEquals("TEST", messageProcessor.MessageType);
			records = System.Array.Empty<JXCRecord>();
			messageProcessor = GetNewMessageProcessor(records);
			AssertEquals("", messageProcessor.MessageType);
			records = new JXCRecord[] { new HEADRecord("HEAD", "") };
			messageProcessor = GetNewMessageProcessor(records);
			AssertEquals("", messageProcessor.MessageType);
			records = new JXCRecord[] { new HEADRecord("HEAD", ""), new DummyRecord("SOMETHINGNEW", "345") };
			messageProcessor = GetNewMessageProcessor(records);
			AssertEquals("SOMETHINGNEW", messageProcessor.MessageType);
		}

		#region Implementation
		protected BusinessObjectFactoryProvider FactoryProvider
		{
			get
			{
				if (fFactoryProvider == null)
				{
					fFactoryProvider = new BusinessObjectFactoryProvider(Factory);
				}

				return fFactoryProvider;
			}
		}

		protected NotificationBuffer NotificationBuffer
		{
			get
			{
				if (fNotificationBuffer == null)
				{
					fNotificationBuffer = new NotificationBuffer();
				}

				return fNotificationBuffer;
			}
		}

		protected void AssertErrorNotification(ErrorType expectedErrorType, string expectedErrorMessage)
		{
			AssertEquals(1, NotificationBuffer.Events.Length);
			AssertEquals("Incorrect error type", expectedErrorType, ((ErrorNotification)NotificationBuffer.Events[0]).ErrorType);
			AssertEquals("Incorrect error message", expectedErrorMessage, ((ErrorNotification)NotificationBuffer.Events[0]).AdditionalInfo);
		}

		protected void AssertContainNotification(string failureMessage, NotificationBuffer notificationBuffer, NotificationSubscriberType notificationType, string expectedMessage)
		{
			INotification[] notifications = notificationBuffer.GetEventsByType(notificationType);
			foreach (INotification notification in notifications)
			{
				INotificationSubscriberNotification subscriberNotification = notification as INotificationSubscriberNotification;
				if (subscriberNotification != null && subscriberNotification.AdditionalInfo == expectedMessage)
				{
					Assert(true);
					return;
				}
			}

			Fail(failureMessage);
		}

		protected abstract JXCMessageProcessor GetNewMessageProcessor(JXCRecord[] records);
		BusinessObjectFactoryProvider fFactoryProvider;
		NotificationBuffer fNotificationBuffer;
		#endregion
	}
}

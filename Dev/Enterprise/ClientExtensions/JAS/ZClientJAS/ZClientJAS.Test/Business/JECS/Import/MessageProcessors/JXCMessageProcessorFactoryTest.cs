using System;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class JXCMessageProcessorFactoryTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestNewProcessor_PassingInNull()
		{
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			JXCMessageProcessor processor = MessageProcessorFactory.NewProcessor(null, notificationBuffer);
			AssertNull("Should return Null when JXCRecord array is null", processor);
			AssertEquals(1, notificationBuffer.Events.Length);
			AssertEquals(ErrorType.InvalidFileFormat, ((ErrorNotification)notificationBuffer.Events[0]).ErrorType);
			AssertEquals("JXC file is invalid", ((ErrorNotification)notificationBuffer.Events[0]).AdditionalInfo);
		}

		[ExpectNoExceptions]
		public void TestNewProcessor_ArrayHasLessThanTwoElements()
		{
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			JXCRecord[] records = Array.Empty<JXCRecord>();
			JXCMessageProcessor processor = MessageProcessorFactory.NewProcessor(records, notificationBuffer);
			AssertNull("Not enough elements in the array, should return null", processor);
			records = new JXCRecord[] { new HEADRecord(JXCConstants.LineTypes.HEAD, ""), new TRLRRecord(JXCConstants.LineTypes.TRLR, "") };
			processor = MessageProcessorFactory.NewProcessor(records, notificationBuffer);
			AssertNull("Not enough elements in the array, should return null", processor);
		}

		public void TestNewProcessor()
		{
			JXCRecord[] records = new JXCRecord[3];
			records[0] = new HEADRecord(JXCConstants.LineTypes.HEAD, "");
			records[2] = new TRLRRecord(JXCConstants.LineTypes.TRLR, "");
			AssertNewProcessor(records, new MAWBRecord(JXCConstants.LineTypes.MAWB, ""), typeof(MAWBMessageProcessor));
			AssertNewProcessor(records, new MAWBRecord(JXCConstants.LineTypes.DAWB, ""), typeof(DAWBMessageProcessor));
			AssertNewProcessor(records, new HAWBRecord(JXCConstants.LineTypes.CHAB, ""), typeof(CHABMessageProcessor));
			AssertNewProcessor(records, new HAWBRecord(JXCConstants.LineTypes.PSAB, ""), typeof(PSABMessageProcessor));
			AssertNewProcessor(records, new OMANRecord(JXCConstants.LineTypes.OMAN, ""), typeof(OMANMessageProcessor));
			AssertNewProcessor(records, new OHBLRecord(JXCConstants.LineTypes.PSBL, ""), typeof(PSBLMessageProcessor));
			AssertNewProcessor(records, new OHBLRecord(JXCConstants.LineTypes.COHB, ""), typeof(COHBMessageProcessor));
		}

		public void TestNewProcessor_UnrecognisedMessageTypeIsIgnored()
		{
			JXCRecord[] records = new JXCRecord[3];
			records[0] = new HEADRecord(JXCConstants.LineTypes.HEAD, "");
			records[2] = new TRLRRecord(JXCConstants.LineTypes.TRLR, "");
			AssertNewProcessor_UnrecognisedMessageTypeIsIgnored(records, JXCConstants.LineTypes.DHAB);
			AssertNewProcessor_UnrecognisedMessageTypeIsIgnored(records, "ASDF");
			AssertNewProcessor_UnrecognisedMessageTypeIsIgnored(records, ")(*#$)(*");
		}

		#region Implementation
		void AssertNewProcessor(JXCRecord[] records, JXCRecord firstBodyRecord, Type expectedMessageProcessorType)
		{
			records[1] = firstBodyRecord;
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			JXCMessageProcessor messageProcessor = MessageProcessorFactory.NewProcessor(records, notificationBuffer);
			AssertEquals("Wrong type is returned", expectedMessageProcessorType, messageProcessor.GetType());
			AssertEquals("Should be passed in through the constructor", records, messageProcessor.Records);
			AssertEquals("There should be no notifications", 0, notificationBuffer.Events.Length);
		}

		void AssertNewProcessor_UnrecognisedMessageTypeIsIgnored(JXCRecord[] records, ZString firstBodyLineType)
		{
			records[1] = new JXCRecordForTest(firstBodyLineType);
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			JXCMessageProcessor messageProcessor = MessageProcessorFactory.NewProcessor(records, notificationBuffer);
			AssertNull("Should be returning null if MessageType is not recognised", messageProcessor);
			AssertEquals("There should be a warning", firstBodyLineType + " message is not supported. The system is ignoring this message.", ((WarningNotification)notificationBuffer.Events[0]).AdditionalInfo);
		}

		JXCMessageProcessorFactory MessageProcessorFactory
		{
			get
			{
				if (fMessageProcessorFactory == null)
				{
					fMessageProcessorFactory = new JXCMessageProcessorFactory();
				}

				return fMessageProcessorFactory;
			}
		}

		JXCMessageProcessorFactory fMessageProcessorFactory;
		#region JXCRecordForTest
		class JXCRecordForTest : JXCRecord
		{
			public JXCRecordForTest(ZString lineType) : base(lineType, "")
			{
			}
		}
		#endregion
		#endregion
	}
}

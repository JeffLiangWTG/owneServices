using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.MessagePurgeProcessor.Testing
{
	[TestedType(typeof(MessagePurgeServiceTask))]
	class MessagePurgeServiceTaskTest : ServiceTaskTestCase<MessagePurgeServiceTask>
	{
		public void TestAllowedReferences()
		{
			var allowedReferences = new List<string> {
				"EDIMessage_EM_EM_RequestMessage_FK2_EDIMessage_RRR_120N",
				"EDIMessage_EM_EI_FK2_EDIInterchange_RRR_120N",
				"EDIMessageAttach_EG_EM_FK2_EDIMessage_CRR_120N",
				"EDIMessageLogPivot_EML_EM_FK2_EDIMessage_CRR_120N",
				"EDIMessageQueueState_EQS_EM_FK2_EDIMessage_CRR_120N"
			};
			var actualReferences = new List<string>();
			var query = @"
SELECT DISTINCT name FROM sys.objects WHERE object_id in
(
	SELECT fk.constraint_object_id FROM sys.foreign_key_columns as fk
	WHERE fk.referenced_object_id IN (SELECT object_id from sys.tables WHERE name IN ('EDIMessage', 'EDIInterchange'))
)
";
			using (var cmd = Db.Connection.Command(query))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					actualReferences.Add(reader.GetString(0));
				}
			}
			AssertContainsExactElementsInAnyOrder(
@"When this unit test fails you need to:
1.  Change MessagePurgeServiceTaskTest.TestRunTask to generate the data required to test purging with your reference
2.  Then change MessagePurgeServiceTask to handle your reference
3.  Then add the name of your reference to the allowedReferences list in this test.",
				allowedReferences, actualReferences
			);
		}

		[TestDate(2012, 01, 01)]
		public void TestRunTask_NoExceptionThrowWhenInterchangeInNull()
		{
			var message = Factory.New<XmlEDIMessage>();
			message.EM_ApplicationCode = ApplicationCodeList.Codes.XMS;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.AgencyBillsOfLading;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;

			Factory.Save();
			TestDateAttribute.Date = ZDateTime.Now.AddMonths(2).AddMinutes(1).ToDateTime();

			var task = new MessagePurgeServiceTask();
			InitialiseTaskSchedule(task);
			SetUpRegistry();
			AssertNoExceptionThrown(task.RunTask);
			AssertMessageAndMessageNoteDeleted(message.PK, new BusinessObjectFactory());
		}

		[TestDate(2012, 01, 01)]
		public void TestRunTask()
		{
			#region SetUp

			var interchange11 = CreateInterchange(ApplicationCodeList.Codes.XMS, ReceiveTransmitList.Codes.Transmit);
			var note11 = interchange11.Notes.AddNew(true, "Blah", "test11");

			var interchange1 = CreateInterchange(ApplicationCodeList.Codes.XMS, ReceiveTransmitList.Codes.Transmit);
			var note1 = interchange1.Notes.AddNew(true, "Blah", "test1");
			var message1 = CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.AgencyBillsOfLading);
			var message2 = CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.Events);
			var message3 = CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.Products);
			var message4 = CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.Shipments);
			var message5 = CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.Organizations);

			var interchange2 = CreateInterchange(ApplicationCodeList.Codes.XMS, ReceiveTransmitList.Codes.Transmit);
			var note2 = interchange2.Notes.AddNew(true, "Blah", "test2");
			var message6 = CreateMessageWithGenPivotRecordAndNote(interchange2, EDIMessageSubTypeList.Codes.Events);
			var message7 = CreateMessageWithGenPivotRecordAndNote(interchange2, EDIMessageSubTypeList.Codes.AgencyBillsOfLading);

			var interchange31 = CreateInterchange(ApplicationCodeList.Codes.UniversalDataMessaging, ReceiveTransmitList.Codes.Transmit);
			var note31 = interchange31.Notes.AddNew(true, "Blah", "test31");

			var interchange3 = CreateInterchange(ApplicationCodeList.Codes.UniversalDataMessaging, ReceiveTransmitList.Codes.Transmit);
			var note3 = interchange3.Notes.AddNew(true, "Blah", "test3");
			var message8 = CreateMessageWithGenPivotRecordAndNote(interchange3, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
			var message9 = CreateMessageWithGenPivotRecordAndNote(interchange3, EDIMessageSubTypeList.Codes.XmlUniversalShipment);

			var interchange4 = CreateInterchange(ApplicationCodeList.Codes.UniversalDataMessaging, ReceiveTransmitList.Codes.Transmit);
			var note4 = interchange4.Notes.AddNew(true, "Blah", "test4");
			var message10 = CreateMessageWithGenPivotRecordAndNote(interchange4, EDIMessageSubTypeList.Codes.XmlUniversalEvent);

			var interchange51 = CreateInterchange(ApplicationCodeList.Codes.SYS, ReceiveTransmitList.Codes.Transmit);
			var note51 = interchange51.Notes.AddNew(true, "Blah", "test51");

			var interchange5 = CreateInterchange(ApplicationCodeList.Codes.SYS, ReceiveTransmitList.Codes.Transmit);
			var note5 = interchange5.Notes.AddNew(true, "Blah", "test5");
			var message11 = CreateMessageWithGenPivotRecordAndNote(interchange5, "CVR");
			var message12 = CreateMessageWithGenPivotRecordAndNote(interchange5, "CVR");
			var message13 = CreateMessageWithGenPivotRecordAndNote(interchange5, "CVR");

			var interchange6 = CreateInterchange(ApplicationCodeList.Codes.SYS, ReceiveTransmitList.Codes.Receive);
			var note6 = interchange6.Notes.AddNew(true, "Blah", "test6");
			var message14 = CreateMessageWithGenPivotRecordAndNote(interchange6, "CVR");
			var message15 = CreateMessageWithGenPivotRecordAndNote(interchange6, "CVR");
			var message16 = CreateMessageWithGenPivotRecordAndNote(interchange6, "CVR");

			var interchangeErd1 = CreateInterchange(ApplicationCodeList.Codes.SYS, ReceiveTransmitList.Codes.Transmit);
			var noteErd1 = interchangeErd1.Notes.AddNew(true, "Blah", "testErd1");
			var messageErd1 = CreateMessageWithGenPivotRecordAndNote(interchangeErd1, "ERD");
			var messageErd2 = CreateMessageWithGenPivotRecordAndNote(interchangeErd1, "ERD");
			var messageErd3 = CreateMessageWithGenPivotRecordAndNote(interchangeErd1, "ERD");

			var interchangeErd2 = CreateInterchange(ApplicationCodeList.Codes.SYS, ReceiveTransmitList.Codes.Receive);
			var noteErd2 = interchangeErd2.Notes.AddNew(true, "Blah", "testErd2");
			var messageErd4 = CreateMessageWithGenPivotRecordAndNote(interchangeErd2, "ERD");
			var messageErd5 = CreateMessageWithGenPivotRecordAndNote(interchangeErd2, "ERD");
			var messageErd6 = CreateMessageWithGenPivotRecordAndNote(interchangeErd2, "ERD");

			var interchangeUar1 = CreateInterchange(ApplicationCodeList.Codes.SYS, ReceiveTransmitList.Codes.Transmit);
			var noteUar1 = interchangeUar1.Notes.AddNew(true, "Blah", "testUar1");
			var messageUar1 = CreateMessageWithGenPivotRecordAndNote(interchangeUar1, "UAR");
			var messageUar2 = CreateMessageWithGenPivotRecordAndNote(interchangeUar1, "UAR");
			var messageUar3 = CreateMessageWithGenPivotRecordAndNote(interchangeUar1, "UAR");

			var interchangeUar2 = CreateInterchange(ApplicationCodeList.Codes.SYS, ReceiveTransmitList.Codes.Receive);
			var noteUar2 = interchangeUar2.Notes.AddNew(true, "Blah", "testUar2");
			var messageUar4 = CreateMessageWithGenPivotRecordAndNote(interchangeUar2, "UAR");
			var messageUar5 = CreateMessageWithGenPivotRecordAndNote(interchangeUar2, "UAR");
			var messageUar6 = CreateMessageWithGenPivotRecordAndNote(interchangeUar2, "UAR");

			var interchange71 = CreateInterchange(ApplicationCodeList.Codes.NativeDataMessaging, ReceiveTransmitList.Codes.Transmit);
			var note71 = interchange71.Notes.AddNew(true, "Blah", "test71");

			var interchange7 = CreateInterchange(ApplicationCodeList.Codes.NativeDataMessaging, ReceiveTransmitList.Codes.Transmit);
			var note7 = interchange7.Notes.AddNew(true, "Blah", "test7");
			var message17 = CreateMessageWithGenPivotRecordAndNote(interchange7, EDIMessageSubTypeList.Codes.XmlNativeStaff);
			var message18 = CreateMessageWithGenPivotRecordAndNote(interchange7, EDIMessageSubTypeList.Codes.XmlNativeOrder);
			var message19 = CreateMessageWithGenPivotRecordAndNote(interchange7, EDIMessageSubTypeList.Codes.XmlNativeAirline);

			var interchange8 = CreateInterchange(ApplicationCodeList.Codes.XMS, ReceiveTransmitList.Codes.Transmit);
			var message20 = CreateMessageWithGenPivotRecordAndNote(interchange8, EDIMessageSubTypeList.Codes.AgencyBillsOfLading);

			Factory.Save();
			TestDateAttribute.Date = ZDateTime.Now.AddMonths(2).AddMinutes(1).ToDateTime();

			#endregion

			var initialMessageCount = Factory.Load<EDIMessage>(new ZQuery()).Length;
			var task = new MessagePurgeServiceTask(4);
			InitialiseTaskSchedule(task);
			SetUpRegistry();
			var defaultSettings = eHubMessagingRegistry.GetDefaultPurgeSettings();
			var allDefaultApplicationCodes = defaultSettings.ApplicationCodes.Cast<ApplicationCodeObj>();
			var allDefaultMessageTypes = allDefaultApplicationCodes
				.Where(ac => ac.MessageTypes != null)
				.SelectMany(ac => ac.MessageTypes.Cast<MessageTypeObj>())
				.ToArray();
			var settings = eHubMessagingRegistry.Instance.PurgeSettingsItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var allApplicationCodes = settings.ApplicationCodes.Cast<ApplicationCodeObj>();
			var allMessageTypes = allApplicationCodes
				.Where(ac => ac.MessageTypes != null)
				.SelectMany(ac => ac.MessageTypes.Cast<MessageTypeObj>())
				.ToArray();
			AssertEquals("PRECONDITION: No message types should be missing", allDefaultMessageTypes.Length, allMessageTypes.Length);

			task.RunTask();
			var newFactory = new BusinessObjectFactory();

			var interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange11.PK);
			AssertNotNull(interchangeAfterPurge);
			var noteAfterPurge = newFactory.Load<StmNote>(note11.PK);
			AssertNotNull(noteAfterPurge);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange1.PK);
			AssertNotNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(note1.PK);
			AssertNotNull(noteAfterPurge);
			AssertEquals(1, interchangeAfterPurge.ContainedMessages.Count);
			Assert(interchangeAfterPurge.ContainedMessages.Select(a => a.PK).Contains(message4.PK));
			AssertMessageAndMessageNoteDeleted(message1.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(message2.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(message3.PK, newFactory);
			AssertMessageNotDeleted(message4, newFactory);
			AssertMessageAndMessageNoteDeleted(message5.PK, newFactory);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange2.PK);
			AssertNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(note2.PK);
			AssertNull(noteAfterPurge);
			AssertMessageAndMessageNoteDeleted(message6.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(message7.PK, newFactory);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange31.PK);
			AssertNotNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(note31.PK);
			AssertNotNull(noteAfterPurge);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange3.PK);
			AssertNotNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(note3.PK);
			AssertNotNull(noteAfterPurge);
			AssertEquals(1, interchangeAfterPurge.ContainedMessages.Count);
			Assert(interchangeAfterPurge.ContainedMessages.Select(a => a.PK).Contains(message9.PK));
			AssertMessageAndMessageNoteDeleted(message8.PK, newFactory);
			AssertMessageNotDeleted(message9, newFactory);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange4.PK);
			AssertNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(note4.PK);
			AssertNull(noteAfterPurge);
			AssertMessageAndMessageNoteDeleted(message10.PK, newFactory);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange51.PK);
			AssertNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(note51.PK);
			AssertNull(noteAfterPurge);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange5.PK);
			AssertNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(note5.PK);
			AssertNull(noteAfterPurge);
			AssertMessageAndMessageNoteDeleted(message11.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(message12.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(message13.PK, newFactory);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange6.PK);
			AssertNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(note6.PK);
			AssertNull(noteAfterPurge);
			AssertMessageAndMessageNoteDeleted(message14.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(message15.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(message16.PK, newFactory);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchangeErd1.PK);
			AssertNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(noteErd1.PK);
			AssertNull(noteAfterPurge);
			AssertMessageAndMessageNoteDeleted(messageErd1.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(messageErd2.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(messageErd3.PK, newFactory);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchangeErd2.PK);
			AssertNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(noteErd2.PK);
			AssertNull(noteAfterPurge);
			AssertMessageAndMessageNoteDeleted(messageErd4.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(messageErd5.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(messageErd6.PK, newFactory);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchangeUar1.PK);
			AssertNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(noteUar1.PK);
			AssertNull(noteAfterPurge);
			AssertMessageAndMessageNoteDeleted(messageUar1.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(messageUar2.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(messageUar3.PK, newFactory);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchangeUar2.PK);
			AssertNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(noteUar2.PK);
			AssertNull(noteAfterPurge);
			AssertMessageAndMessageNoteDeleted(messageUar4.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(messageUar5.PK, newFactory);
			AssertMessageAndMessageNoteDeleted(messageUar6.PK, newFactory);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange71.PK);
			AssertNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(note71.PK);
			AssertNull(noteAfterPurge);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange7.PK);
			AssertNotNull(interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(note7.PK);
			AssertNotNull(noteAfterPurge);
			AssertEquals(2, interchangeAfterPurge.ContainedMessages.Count);
			AssertMessageNotDeleted(message17, newFactory);
			AssertMessageAndMessageNoteDeleted(message18.PK, newFactory);
			AssertMessageNotDeleted(message19, newFactory);

			AssertMessageAndMessageNoteDeleted(message20.PK, newFactory);

			AssertEquals("Deleted messages count", initialMessageCount - newFactory.Load<EDIMessage>(new ZQuery()).Length, 28);
			AssertContains("Starting purge", "Starting purge.", task.Buffer.AsString);
			AssertContains("Task Log: " + task.Buffer.AsString, @"
0 EDI Messages and their associated entities have been purged.
4 EDI Messages and their associated entities have been purged.
4 EDI Messages and their associated entities have been purged.
4 EDI Messages and their associated entities have been purged.
4 EDI Messages and their associated entities have been purged.
4 EDI Messages and their associated entities have been purged.
4 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
4 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
1 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
1 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.", task.Buffer.AsString);
			AssertContains("Finished purge", "Finished purge.", task.Buffer.AsString);

			settings = eHubMessagingRegistry.Instance.PurgeSettingsItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			allApplicationCodes = settings.ApplicationCodes.Cast<ApplicationCodeObj>();
			allMessageTypes = allApplicationCodes.SelectMany(ac => ac.MessageTypes.Cast<MessageTypeObj>()).ToArray();
			CombineAssertions("Registry was updated", () =>
			{
				AssertContainsExactElementsInAnyOrder("Missing message types were restored from defaults",
					allDefaultMessageTypes.Select(mt => mt.MessageSubType),
					allMessageTypes.Select(mt => mt.MessageSubType));
				foreach (var messageType in allMessageTypes.Where(mt => mt.Selected))
				{
					AssertEquals(messageType.MessageSubType, GetMaxMessageCreationTimeToPurge(messageType), messageType.LatestPurgedMessageTimeUtc);
				}
			});
		}

		[TestDate(2012, 01, 01)]
		public void TestPurgeWithRegistryBatchSizing()
		{
			#region Setup
			var interchange1 = CreateInterchange(ApplicationCodeList.Codes.XMS, ReceiveTransmitList.Codes.Transmit);

			for (var i = 0; i < 200; i++)
			{
				CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.Organizations);
			}

			Factory.Save();
			TestDateAttribute.Date = ZDateTime.Now.AddMonths(2).AddMinutes(1).ToDateTime();
			#endregion

			var newBatchSize = 150;
			var settings = eHubMessagingRegistry.Instance.PurgeSettingsItem.DefaultValue;
			settings.BatchSize = newBatchSize;
			using (eHubMessagingRegistry.Instance.PurgeSettingsItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, settings))
			{
				var task = new MessagePurgeServiceTask();
				InitialiseTaskSchedule(task);
				SetUpRegistry();

				task.RunTask();
				var log = task.Buffer.AsString;

				Assert(log.Contains("150 EDI Messages and their associated entities have been purged."));
				Assert(log.Contains("50 EDI Messages and their associated entities have been purged."));
			}
		}

		[TestDate(2012, 01, 01)]
		public void TestPurgeUDQMessages()
		{
			#region SetUp

			var interchange0 = CreateInterchange(ApplicationCodeList.Codes.UniversalDataQuery, ReceiveTransmitList.Codes.Transmit);
			var note0 = interchange0.Notes.AddNew(true, "Blah", "test1");

			var interchange1 = CreateInterchange(ApplicationCodeList.Codes.UniversalDataQuery, ReceiveTransmitList.Codes.Transmit);
			var note1 = interchange1.Notes.AddNew(true, "Blah", "test2");

			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlUniversalActivity);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlUniversalActivityRequest);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlUniversalDocumentRequest);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlUniversalEvent);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlUniversalInterchangeRequeueRequest);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlUniversalResponse);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlUniversalSchedule);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlUniversalShipment);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlUniversalShipmentRequest);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlUniversalTransaction);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatchRequest);

			Factory.Save();

			#endregion

			var initialMessageCount = Factory.Load<EDIMessage>(new ZQuery()).Length;

			var task = new MessagePurgeServiceTask(4);
			InitialiseTaskSchedule(task);
			task.RunTask();
			var newFactory = new BusinessObjectFactory();

			AssertEquals("Shouldn't purge new data", initialMessageCount, newFactory.Load<EDIMessage>(new ZQuery()).Length);

			var settings = eHubMessagingRegistry.GetDefaultPurgeSettings();
			var appCode = settings.ApplicationCodes.OfType<ApplicationCodeObj>().Single(a => a.ApplicationCode == ApplicationCodeList.Codes.UniversalDataQuery);
			var messageTypes = appCode.MessageTypes.Cast<MessageTypeObj>().ToArray();
			Array.ForEach(messageTypes, m => m.Selected = false);
			eHubMessagingRegistry.Instance.PurgeSettingsItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
			TestDateAttribute.Date = ZDateTime.Now.AddMonths(24).AddMinutes(1).ToDateTime();

			task = new MessagePurgeServiceTask(4);
			InitialiseTaskSchedule(task);
			task.RunTask();
			newFactory = new BusinessObjectFactory();

			AssertEquals("Shouldn't purge message types that are not selected", initialMessageCount, newFactory.Load<EDIMessage>(new ZQuery()).Length);

			Array.ForEach(messageTypes, m => m.Selected = true);
			eHubMessagingRegistry.Instance.PurgeSettingsItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			task = new MessagePurgeServiceTask(4);
			InitialiseTaskSchedule(task);
			task.RunTask();
			newFactory = new BusinessObjectFactory();

			var interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange0.PK);
			AssertNull("This interchange has no messages so should get purged", interchangeAfterPurge);
			var noteAfterPurge = newFactory.Load<StmNote>(note0.PK);
			AssertNull(noteAfterPurge);
			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange1.PK);
			AssertNull("This interchange should be purged with all it's messages", interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(note1.PK);
			AssertNull(noteAfterPurge);

			AssertEquals("Deleted messages count", 12, initialMessageCount - newFactory.Load<EDIMessage>(new ZQuery()).Length);
			AssertContains("Task Log: " + task.Buffer.AsString, @"Starting purge.
4 EDI Messages and their associated entities have been purged.
4 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
1 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
Finished purge.", task.Buffer.AsString);
		}

		[TestDate(2012, 01, 01)]
		public void TestPurgeNDQMessages()
		{
			#region SetUp

			var interchange0 = CreateInterchange(ApplicationCodeList.Codes.NativeDataQuery, ReceiveTransmitList.Codes.Transmit);
			var note0 = interchange0.Notes.AddNew(true, "Blah", "test1");

			var interchange1 = CreateInterchange(ApplicationCodeList.Codes.NativeDataQuery, ReceiveTransmitList.Codes.Transmit);
			var note1 = interchange1.Notes.AddNew(true, "Blah", "test2");

			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeAcceptabilityBand);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeAddOnRule);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeAirline);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeBMControlCustomisation);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeBMSystem);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeCommodityCode);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeCommunication);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeCompany);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeContainer);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeCountry);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeCurrencyExchangeRate);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeCusStatement);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeDangerousGood);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeDeclaration);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeOrder);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeOrganization);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeProduct);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeRate);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeServiceLevel);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeShipment);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeStaff);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeTag);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeTagRule);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeUNLOCO);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeVessel);
			CreateMessageWithGenPivotRecordAndNote(interchange1, EDIMessageSubTypeList.Codes.XmlNativeWorkflowTemplate);

			Factory.Save();
			TestDateAttribute.Date = ZDateTime.Now.AddMonths(24).AddMinutes(1).ToDateTime();

			#endregion

			var initialMessageCount = Factory.Load<EDIMessage>(new ZQuery()).Length;
			var task = new MessagePurgeServiceTask(4);
			InitialiseTaskSchedule(task);

			task.RunTask();
			var newFactory = new BusinessObjectFactory();

			var interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange0.PK);
			AssertNull("This interchange has no messages so should gets purged", interchangeAfterPurge);
			var noteAfterPurge = newFactory.Load<StmNote>(note0.PK);
			AssertNull(noteAfterPurge);
			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchange1.PK);
			AssertNull("This interchange should be purged with all it's messages", interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(note1.PK);
			AssertNull(noteAfterPurge);

			AssertEquals("Deleted messages count", 26, initialMessageCount - newFactory.Load<EDIMessage>(new ZQuery()).Length);
			AssertContains("Task Log: " + task.Buffer.AsString, @"Starting purge.
4 EDI Messages and their associated entities have been purged.
4 EDI Messages and their associated entities have been purged.
4 EDI Messages and their associated entities have been purged.
4 EDI Messages and their associated entities have been purged.
4 EDI Messages and their associated entities have been purged.
4 EDI Messages and their associated entities have been purged.
2 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
1 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
Finished purge.", task.Buffer.AsString);
		}

		[TestDate(2012, 01, 01)]
		public void TestPurgeTelematicsXmlMessages()
		{
			var telematicsApplicationCode = eHubMessagingRegistry.Instance.PurgeSettingsItem.DefaultValue.ApplicationCodes.GetApplicationCodeObj(ApplicationCodeList.Codes.Telematics);

			AssertEquals(2, telematicsApplicationCode.MessageTypes.Count);
			AssertCollectionContains(TelematicsMessageList.Codes.TelematicsXmlData, telematicsApplicationCode.MessageTypes.Select(m => ((MessageTypeObj)m).MessageSubType));
			AssertCollectionContains(TelematicsMessageList.Codes.ProtobufData, telematicsApplicationCode.MessageTypes.Select(m => ((MessageTypeObj)m).MessageSubType));

			#region SetUp

			var interchangeEmptyTransmit = CreateInterchange(ApplicationCodeList.Codes.Telematics, ReceiveTransmitList.Codes.Transmit);
			var noteInterchangeEmptyTransmit = interchangeEmptyTransmit.Notes.AddNew(true, "Blah", "testInterchangeEmptyTransmit");

			var interchangeEmptyReceive = CreateInterchange(ApplicationCodeList.Codes.Telematics, ReceiveTransmitList.Codes.Receive);
			var noteInterchangeEmptyReceive = interchangeEmptyReceive.Notes.AddNew(true, "Blah", "testInterchangeEmptyReceive");

			var interchangeTXDTransmit = CreateInterchange(ApplicationCodeList.Codes.Telematics, ReceiveTransmitList.Codes.Transmit);
			var noteInterchangeTXDTransmit = interchangeTXDTransmit.Notes.AddNew(true, "Blah", "testInterchangeTXDTransmit");

			var interchangeTXDReceive = CreateInterchange(ApplicationCodeList.Codes.Telematics, ReceiveTransmitList.Codes.Receive);
			var noteInterchangeTXDReceive = interchangeTXDReceive.Notes.AddNew(true, "Blah", "testInterchangeTXDReceive");

			var interchangeProtobuff = CreateInterchange(ApplicationCodeList.Codes.Telematics, ReceiveTransmitList.Codes.Transmit);

			CreateMessageWithGenPivotRecordAndNote(interchangeTXDTransmit, TelematicsMessageList.Codes.TelematicsXmlData);
			CreateMessageWithGenPivotRecordAndNote(interchangeTXDReceive, TelematicsMessageList.Codes.TelematicsXmlData);
			CreateMessageWithGenPivotRecordAndNote(interchangeProtobuff, TelematicsMessageList.Codes.ProtobufData);

			Factory.Save();
			TestDateAttribute.Date = ZDateTime.Now.AddDays(14).AddMinutes(1).ToDateTime();

			#endregion

			var initialMessageCount = Factory.Load<EDIMessage>(new ZQuery()).Length;
			var task = new MessagePurgeServiceTask(4);
			InitialiseTaskSchedule(task);

			task.RunTask();
			var newFactory = new BusinessObjectFactory();

			var interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchangeEmptyTransmit.PK);
			AssertNull("This interchange has no messages so should gets purged", interchangeAfterPurge);
			var noteAfterPurge = newFactory.Load<StmNote>(noteInterchangeEmptyTransmit.PK);
			AssertNull(noteAfterPurge);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchangeEmptyReceive.PK);
			AssertNull("This interchange has no messages so should gets purged", interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(noteInterchangeEmptyReceive.PK);
			AssertNull(noteAfterPurge);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchangeTXDTransmit.PK);
			AssertNull("This interchange should be purged with all it's messages", interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(noteInterchangeTXDTransmit.PK);
			AssertNull(noteAfterPurge);

			interchangeAfterPurge = newFactory.Load<XmlEDIInterchange>(interchangeTXDReceive.PK);
			AssertNull("This interchange should be purged with all it's messages", interchangeAfterPurge);
			noteAfterPurge = newFactory.Load<StmNote>(noteInterchangeTXDReceive.PK);
			AssertNull(noteAfterPurge);

			AssertEquals("Deleted messages count", 3, initialMessageCount - newFactory.Load<EDIMessage>(new ZQuery()).Length);
			AssertContains("Task Log: " + task.Buffer.AsString, @"Starting purge.
0 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
2 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
Finished purge.", task.Buffer.AsString);
		}

		[TestDate(2012, 01, 01)]
		public void TestPurgeUsageMessages()
		{
			var usageApplicationCode = eHubMessagingRegistry.Instance.PurgeSettingsItem.DefaultValue.ApplicationCodes[7];

			AssertEquals(1, usageApplicationCode.MessageTypes.Count);
			AssertEquals(string.Empty, usageApplicationCode.MessageTypes[0].MessageSubType);

			#region SetUp

			var usagemessage = Factory.New<IUsageEDIMessage>();
			usagemessage.EM_ApplicationCode = ApplicationCodeList.Codes.UsageData;
			usagemessage.EM_MessageType = EDIMessageTypeList.Codes.UsageData;
			var usageSummaryMessage = Factory.New<IUsageEDIMessage>();
			usageSummaryMessage.EM_ApplicationCode = ApplicationCodeList.Codes.UsageDataToSummarise;
			usageSummaryMessage.EM_MessageType = EDIMessageTypeList.Codes.UsageDataToSummarise;
			Factory.Save();
			TestDateAttribute.Date = ZDateTime.Now.AddDays(14).AddMinutes(1).ToDateTime();

			#endregion

			var task = new MessagePurgeServiceTask(4);
			InitialiseTaskSchedule(task);

			task.RunTask();
			var newFactory = new BusinessObjectFactory();

			var messageAfterPurge = newFactory.Load<IUsageEDIMessage>(usagemessage.PK);
			AssertNotNull("The usage message should not have been purged, not yet 1 month old", messageAfterPurge);
			messageAfterPurge = newFactory.Load<IUsageEDIMessage>(usageSummaryMessage.PK);
			AssertNotNull("The usage summary message should not have been purged, not yet 1 month old", messageAfterPurge);

			TestDateAttribute.Date = ZDateTime.Now.AddMonths(1).ToDateTime();

			task = new MessagePurgeServiceTask(4);
			InitialiseTaskSchedule(task);

			task.RunTask();
			newFactory = new BusinessObjectFactory();

			messageAfterPurge = newFactory.Load<IUsageEDIMessage>(usagemessage.PK);
			AssertNull("The usage message should now have been purged, it is older than a month old", messageAfterPurge);
			messageAfterPurge = newFactory.Load<IUsageEDIMessage>(usageSummaryMessage.PK);
			AssertNull("The usage summary message should now have been purged, it is older than a month old", messageAfterPurge);
			AssertContains("Task Log: " + task.Buffer.AsString, @"Starting purge.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
2 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
Finished purge.", task.Buffer.AsString);
		}

		[TestDate(2012, 01, 01)]
		public void TestPurgeENEGEIGEPMessages()
		{
			var eNEApplicationCode = eHubMessagingRegistry.Instance.PurgeSettingsItem.DefaultValue.ApplicationCodes.GetApplicationCodeObj(ApplicationCodeList.Codes.eNett);
			var gEIApplicationCode = eHubMessagingRegistry.Instance.PurgeSettingsItem.DefaultValue.ApplicationCodes.GetApplicationCodeObj(ApplicationCodeList.Codes.GlobalElectronicInvoice);
			var gEPApplicationCode = eHubMessagingRegistry.Instance.PurgeSettingsItem.DefaultValue.ApplicationCodes.GetApplicationCodeObj(ApplicationCodeList.Codes.GlobalElectronicPayment);

			var eNEMessageSubTypeList = new List<string>()
			{
				"CIN",
				"CNI",
				"GNC",
				"GNI",
				"GNP",
				"OFP",
				"PCC",
				"PDD",
				"RES",
			};
			var gEIMessageSubTypeList = new List<string>()
			{
				"ARN",
				"APR",
				"CBI",
				"CRN",
				"CRC",
				"RCN",
				"CAN",
				"GED",
				"GEX",
				"GEN",
				"GRN",
				"GEL",
				"GDO",
				"GDD",
				"PIL",
				"GSU",
				"GEQ",
				"RIN",
				"RDN",
				"RCP",
				"RRO",
				"PAP",
				"SIB",
				"SRN",
				"PRJ",
				"PST",
				"RST",
				"SUB",
				"REQ",
				"PDF",
			};
			var gEPMessageSubTypeList = new List<string>()
			{
				"GRT",
				"GAQ",
				"PAY",
				"SBN"
			};

			AssertEquals(9, eNEApplicationCode.MessageTypes.Count);
			AssertContainsExactElementsInAnyOrder(eNEMessageSubTypeList, eNEApplicationCode.MessageTypes.Cast<MessageTypeObj>().Select(x => x.MessageSubType));
			AssertEquals(30, gEIApplicationCode.MessageTypes.Count);
			AssertContainsExactElementsInAnyOrder(gEIMessageSubTypeList, gEIApplicationCode.MessageTypes.Cast<MessageTypeObj>().Select(x => x.MessageSubType));
			AssertEquals(4, gEPApplicationCode.MessageTypes.Count);
			AssertContainsExactElementsInAnyOrder(gEPMessageSubTypeList, gEPApplicationCode.MessageTypes.Cast<MessageTypeObj>().Select(x => x.MessageSubType));

			#region SetUp

			var eNEEDIMessagePks = new List<ZGuid>();
			foreach (var eNEMessageSubType in eNEMessageSubTypeList)
			{
				var eNEInterchange = CreateInterchange(ApplicationCodeList.Codes.eNett, ReceiveTransmitList.Codes.Transmit);
				var eNEEDIMessage = CreateMessageWithGenPivotRecordAndNote(eNEInterchange, eNEMessageSubType);
				eNEEDIMessagePks.Add(eNEEDIMessage.PK);
			}

			var gEIEDIMessagePks = new List<ZGuid>();
			foreach (var gEIMessageSubType in gEIMessageSubTypeList)
			{
				var gEIInterchange = CreateInterchange(ApplicationCodeList.Codes.GlobalElectronicInvoice, ReceiveTransmitList.Codes.Transmit);
				var gEIEDIMessage = CreateMessageWithGenPivotRecordAndNote(gEIInterchange, gEIMessageSubType);
				gEIEDIMessagePks.Add(gEIEDIMessage.PK);
			}

			var gEPEDIMessagePks = new List<ZGuid>();
			foreach (var gEPMessageSubType in gEPMessageSubTypeList)
			{
				var gEPInterchange = CreateInterchange(ApplicationCodeList.Codes.GlobalElectronicPayment, ReceiveTransmitList.Codes.Transmit);
				var gEPEDIMessage = CreateMessageWithGenPivotRecordAndNote(gEPInterchange, gEPMessageSubType);
				gEPEDIMessagePks.Add(gEPEDIMessage.PK);
			}

			Factory.Save();

			#endregion

			var task = new MessagePurgeServiceTask(3);
			InitialiseTaskSchedule(task);

			task.RunTask();
			var newFactory = new BusinessObjectFactory();

			var messageAfterPurge = newFactory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.PK, eNEEDIMessagePks));
			AssertEquals("The ENE message should not have been purged, not yet 6 month old", 9, messageAfterPurge.Length);
			messageAfterPurge = newFactory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.PK, gEIEDIMessagePks));
			AssertEquals("The GEI message should not have been purged, not yet 6 month old", 30, messageAfterPurge.Length);
			messageAfterPurge = newFactory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.PK, gEPEDIMessagePks));
			AssertEquals("The GEP message should not have been purged, not yet 6 month old", 4, messageAfterPurge.Length);

			TestDateAttribute.Date = ZDateTime.Now.AddMonths(6).ToDateTime();

			task = new MessagePurgeServiceTask(3);
			InitialiseTaskSchedule(task);

			task.RunTask();
			newFactory = new BusinessObjectFactory();

			messageAfterPurge = newFactory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.PK, eNEEDIMessagePks));
			AssertEquals("The ENE message should now have been purged, it is older than 6 month old", false, messageAfterPurge.Any());
			messageAfterPurge = newFactory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.PK, gEIEDIMessagePks));
			AssertEquals("The GEI message should now have been purged, it is older than 6 month old", false, messageAfterPurge.Any());
			messageAfterPurge = newFactory.Load<XmlEDIMessage>(new ZQuery(EDIMessageSchema.PK, gEPEDIMessagePks));
			AssertEquals("The GEP message should now have been purged, it is older than 6 month old", false, messageAfterPurge.Any());
			AssertContains("Task Log: " + task.Buffer.AsString, @"Starting purge.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
3 EDI Messages and their associated entities have been purged.
1 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Messages and their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
0 EDI Interchanges with their associated entities have been purged.
Finished purge.", task.Buffer.AsString);
		}

		public void TestPurgeMessagesMarkedAsDiscarded()
		{
			#region SetUp

			var interchangeDiscardedHavingNoMessageWithApplicableCode = CreateInterchange(ApplicationCodeList.Codes.KRCustoms, ReceiveTransmitList.Codes.Transmit);
			interchangeDiscardedHavingNoMessageWithApplicableCode.EI_Status = EDIInterchangeStatusList.Codes.Discarded;
			var note0 = interchangeDiscardedHavingNoMessageWithApplicableCode.Notes.AddNew(true, "Blah", "note0");

			var interchangeSentHavingNoMessage = CreateInterchange(ApplicationCodeList.Codes.KRCustoms, ReceiveTransmitList.Codes.Transmit);
			interchangeSentHavingNoMessage.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			var note1 = interchangeSentHavingNoMessage.Notes.AddNew(true, "Blah", "note1");

			var interchangeDiscardedHavingMessage = CreateInterchange(ApplicationCodeList.Codes.KRCustoms, ReceiveTransmitList.Codes.Transmit);
			interchangeDiscardedHavingMessage.EI_Status = EDIInterchangeStatusList.Codes.Discarded;
			var note2 = interchangeDiscardedHavingMessage.Notes.AddNew(true, "Blah", "note2");
			var messageDiscardedWithApplicableCode = CreateMessageWithGenPivotRecordAndNote(interchangeDiscardedHavingMessage, string.Empty);
			messageDiscardedWithApplicableCode.EM_Status = EDIMessageStatusList.Codes.Discarded;

			var interchangeSentHavingMessage = CreateInterchange(ApplicationCodeList.Codes.KRCustoms, ReceiveTransmitList.Codes.Transmit);
			interchangeSentHavingMessage.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			var note3 = interchangeSentHavingMessage.Notes.AddNew(true, "Blah", "note3");
			var messageSent = CreateMessageWithGenPivotRecordAndNote(interchangeSentHavingMessage, string.Empty);
			messageSent.EM_Status = EDIMessageStatusList.Codes.Sent;

			var interchangeDiscardedHavingNoMessageWithoutApplicableCode = CreateInterchange(ApplicationCodeList.Codes.NZCustoms, ReceiveTransmitList.Codes.Transmit);
			interchangeDiscardedHavingNoMessageWithoutApplicableCode.EI_Status = EDIInterchangeStatusList.Codes.Discarded;
			var note4 = interchangeDiscardedHavingNoMessageWithoutApplicableCode.Notes.AddNew(true, "Blah", "note4");

			var interchangeDiscardedHavingMessageWithoutApplicableCode = CreateInterchange(ApplicationCodeList.Codes.NZCustoms, ReceiveTransmitList.Codes.Transmit);
			interchangeDiscardedHavingMessageWithoutApplicableCode.EI_Status = EDIInterchangeStatusList.Codes.Discarded;
			var note5 = interchangeDiscardedHavingMessageWithoutApplicableCode.Notes.AddNew(true, "Blah", "note5");
			var messageDiscardedWithoutApplicableCode = CreateMessageWithGenPivotRecordAndNote(interchangeDiscardedHavingMessageWithoutApplicableCode, string.Empty);
			messageDiscardedWithoutApplicableCode.EM_Status = EDIMessageStatusList.Codes.Discarded;
			Factory.Save();

			#endregion

			var initialMessageCount = Factory.Load<EDIMessage>(new ZQuery()).Length;
			AssertEquals("Pre-Condition: 3 messages exist.", 3, initialMessageCount);

			var initialInterchangeCount = Factory.Load<EDIInterchange>(new ZQuery()).Length;
			AssertEquals("Pre-Condition: 6 interchanges exist.", 6, initialInterchangeCount);

			var task = new MessagePurgeServiceTask(10);
			InitialiseTaskSchedule(task);
			task.RunTask();
			var newFactory = new BusinessObjectFactory();

			CombineAssertions(() => { 
				AssertNull("The interchange has been deleted as EI_Status and EI_AppilcationCode is in the list allowing deletion.", newFactory.Load<XmlEDIInterchange>(interchangeDiscardedHavingNoMessageWithApplicableCode.PK));
				AssertNull("A note attached to the deleted interchange also has been deleted.", newFactory.Load<StmNote>(note0.PK));

				AssertNotNull("The interchange has not been deleted as EI_Status is not in the list allowing deletion.", newFactory.Load<XmlEDIInterchange>(interchangeSentHavingNoMessage.PK));
				AssertNotNull("A note attached to the interchange still exists.", newFactory.Load<StmNote>(note1.PK));

				AssertNull("The interchange has been deleted as EI_Status and EI_AppilcationCode is in the list allowing deletion.", newFactory.Load<XmlEDIInterchange>(interchangeDiscardedHavingMessage.PK));
				AssertNull("A note attached to the deleted interchange also has been deleted.", newFactory.Load<StmNote>(note2.PK));
				AssertMessageAndMessageNoteDeleted(interchangeDiscardedHavingMessage.PK, newFactory);

				AssertNotNull("The interchange has not been deleted as EI_Status is not in the list allowing deletion.", newFactory.Load<XmlEDIInterchange>(interchangeSentHavingMessage.PK));
				AssertNotNull("A note attached to the interchange still exists.", newFactory.Load<StmNote>(note3.PK));
				AssertMessageNotDeleted(messageSent, newFactory);

				AssertNotNull("The interchange has not been deleted as EI_AppilcationCode is not in the list allowing deletion.", newFactory.Load<XmlEDIInterchange>(interchangeDiscardedHavingNoMessageWithoutApplicableCode.PK));
				AssertNotNull("A note attached to the interchange still exists.", newFactory.Load<StmNote>(note4.PK));

				AssertNotNull("The interchange has not been deleted as EI_AppilcationCode is not in the list allowing deletion.", newFactory.Load<XmlEDIInterchange>(interchangeDiscardedHavingMessageWithoutApplicableCode.PK));
				AssertNotNull("A note attached to the interchange still exists.", newFactory.Load<StmNote>(note5.PK));
				AssertMessageNotDeleted(messageDiscardedWithoutApplicableCode, newFactory);

				AssertEquals("Deleted messages count", 1, initialMessageCount - newFactory.Load<EDIMessage>(new ZQuery()).Length);
				AssertEquals("Deleted Interchanges count", 2, initialInterchangeCount - newFactory.Load<EDIInterchange>(new ZQuery()).Length);
				AssertContains("Task Log for DCD messages and interchanges" ,
@"1 EDI Messages marked as discarded and their associated entities have been purged based on application codes.
[Application Codes: KRC]
1 EDI Interchanges marked as discarded and their associated entities have been purged based on application codes.
[Application Codes: KRC]", task.Buffer.AsString);
			});
		}

		[TestDate(2012, 01, 01)]
		public void TestLaststPurgeTime()
		{
			var message = Factory.New<IHttpXmlEDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataQuery;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_IsActive = true;
			message.EM_MessageNum = "00001";

			var message1 = Factory.New<IHttpXmlEDIMessage>();
			message1.EM_Status = EDIMessage.Status.Queued;
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataQuery;
			message1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalActivity;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_IsActive = true;
			message1.EM_MessageNum = "00002";

			Factory.Save();
			TestDateAttribute.Date = ZDateTime.Now.AddMonths(24).ToDateTime();

			var settings = eHubMessagingRegistry.GetDefaultPurgeSettings();
			var appCode = settings.ApplicationCodes.OfType<ApplicationCodeObj>().Single(a => a.ApplicationCode == ApplicationCodeList.Codes.UniversalDataQuery);
			var messageTypes = appCode.MessageTypes.Cast<MessageTypeObj>().ToArray();
			var xuaSubType = messageTypes.Where(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalActivity).First();
			var xusSubType = messageTypes.Where(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment).First();
			xuaSubType.Selected = false;
			xusSubType.Selected = true;
			eHubMessagingRegistry.Instance.PurgeSettingsItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var task = new MessagePurgeServiceTask(4);
			InitialiseTaskSchedule(task);
			task.RunTask();

			Factory.Save();
			settings = eHubMessagingRegistry.Instance.PurgeSettingsItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			appCode = settings.ApplicationCodes.OfType<ApplicationCodeObj>().Single(a => a.ApplicationCode == ApplicationCodeList.Codes.UniversalDataQuery);
			messageTypes = appCode.MessageTypes.Cast<MessageTypeObj>().ToArray();
			xuaSubType = messageTypes.Where(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalActivity).First();
			xusSubType = messageTypes.Where(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment).First();
			AssertEquals("Selected=False, purge time should be empty", ZDateTime.Empty, xuaSubType.LatestPurgedMessageTimeUtc);
			AssertEquals("Selected=True, purge time should be updated", ZDateTime.Now.AddMonths(-3), xusSubType.LatestPurgedMessageTimeUtc);

			xuaSubType.Selected = true;
			xusSubType.Selected = true;
			eHubMessagingRegistry.Instance.PurgeSettingsItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
			TestDateAttribute.Date = ZDateTime.Now.AddMonths(12).ToDateTime();

			task = new MessagePurgeServiceTask(4);
			InitialiseTaskSchedule(task);
			task.RunTask();

			settings = eHubMessagingRegistry.Instance.PurgeSettingsItem.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			appCode = settings.ApplicationCodes.OfType<ApplicationCodeObj>().Single(a => a.ApplicationCode == ApplicationCodeList.Codes.UniversalDataQuery);
			messageTypes = appCode.MessageTypes.Cast<MessageTypeObj>().ToArray();
			xuaSubType = messageTypes.Where(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalActivity).First();
			xusSubType = messageTypes.Where(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalShipment).First();
			AssertEquals("Selected=True, purge time should be updated", ZDateTime.Now.AddMonths(-3), xuaSubType.LatestPurgedMessageTimeUtc);
			AssertEquals("Selected=True, purge time should be updated", ZDateTime.Now.AddMonths(-3), xusSubType.LatestPurgedMessageTimeUtc);
			AssertEquals(0, new BusinessObjectFactory().Load<EDIMessage>(new ZQuery()).Length);
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1day", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestRunTask_LowPriorityPauser_RetriesOnExceptions()
		{
			var pauserFactory = new Mock<ILowPriorityProcessPauserFactory>();
			var pauser = new Mock<ILowPriorityProcessPauser>();
			pauserFactory.Setup(f => f.Create()).Returns(pauser.Object);
			pauser.SetupSequence(p => p.Wait(It.IsAny<ILogger>(), It.IsAny<TimeSpan?>(), null)).Throws(new AlwaysOnDelayProviderException()).Throws(new AlwaysOnDelayProviderException()).Returns(TimeSpan.Zero);
			var task = new MessagePurgeServiceTask(recordsToPurge: 4, pauseWaitBackoff: TimeSpan.FromSeconds(5), pauseWaitRetries: 3, pauserFactory.Object);
			InitialiseTaskSchedule(task);
			SetUpRegistry();
			var stopwatch = Stopwatch.StartNew();
			AssertNoExceptionThrown(task.RunTask);
			stopwatch.Stop();
			AssertGreaterThanOrEqualTo("Task should have backed off between retries", stopwatch.Elapsed, TimeSpan.FromSeconds(7.5));
		}

		public void TestRunTask_LowPriorityPauser_NoRetriesOnCriticalExceptions()
		{
			var pauserFactory = new Mock<ILowPriorityProcessPauserFactory>();
			var pauser = new Mock<ILowPriorityProcessPauser>();
			pauserFactory.Setup(f => f.Create()).Returns(pauser.Object);
			pauser.SetupSequence(p => p.Wait(It.IsAny<ILogger>(), It.IsAny<TimeSpan?>(), null)).Throws(new OutOfMemoryException()).Returns(TimeSpan.Zero);
			var task = new MessagePurgeServiceTask(recordsToPurge: 4, pauseWaitBackoff: TimeSpan.FromSeconds(10), pauseWaitRetries: 3, pauserFactory.Object);
			InitialiseTaskSchedule(task);
			SetUpRegistry();
			AssertExceptionThrown(typeof(OutOfMemoryException), task.RunTask);
		}

		public void TestRunTask_LowPriorityPauser_ExitsBackOffWhenSignaled()
		{
			var pauserFactory = new Mock<ILowPriorityProcessPauserFactory>();
			var pauser = new Mock<ILowPriorityProcessPauser>();
			pauserFactory.Setup(f => f.Create()).Returns(pauser.Object);
			pauser.SetupSequence(p => p.Wait(It.IsAny<ILogger>(), It.IsAny<TimeSpan?>(), null)).Throws(new AlwaysOnDelayProviderException()).Returns(TimeSpan.Zero);
			var task = new MessagePurgeServiceTask(recordsToPurge: 4, pauseWaitBackoff: TimeSpan.FromSeconds(10), pauseWaitRetries: 3, pauserFactory.Object);
			InitialiseTaskSchedule(task);
			SetUpRegistry();
			var cancelationTokenSource = new CancellationTokenSource();
			var signalTask = Task.Run(() =>
			{
				Thread.Sleep(TimeSpan.FromSeconds(2.5));
				cancelationTokenSource.Cancel();
			});
			var stopwatch = Stopwatch.StartNew();
			AssertExceptionThrown(typeof(OperationCanceledException), () => task.RunTask(cancelationTokenSource.Token));
			stopwatch.Stop();
			AssertLessThanOrEqualTo("Task shouldn't have completed the full backoff", stopwatch.Elapsed, TimeSpan.FromSeconds(5));
			signalTask.Wait();
		}

		public void TestRunTask_LowPriorityPauser_ThrowsOnLastRetry()
		{
			var pauserFactory = new Mock<ILowPriorityProcessPauserFactory>();
			var pauser = new Mock<ILowPriorityProcessPauser>();
			pauserFactory.Setup(f => f.Create()).Returns(pauser.Object);
			pauser.SetupSequence(p => p.Wait(It.IsAny<ILogger>(), It.IsAny<TimeSpan?>(), null)).Throws(new AlwaysOnDelayProviderException()).Throws(new AlwaysOnDelayProviderException()).Throws(new AlwaysOnDelayProviderException());
			var task = new MessagePurgeServiceTask(recordsToPurge: 4, pauseWaitBackoff: TimeSpan.Zero, pauseWaitRetries: 3, pauserFactory.Object);
			InitialiseTaskSchedule(task);
			SetUpRegistry();
			AssertExceptionThrown(typeof(AlwaysOnDelayProviderException), task.RunTask);
		}

		[TestDate(2012, 01, 01)]
		public void TestDeleteRelatedMessagesSucceeds()
		{
			var message1 = Factory.New<IXmlEDIMessage>();
			message1.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			message1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
			Factory.Save();
			var message2 = Factory.New<IXmlEDIMessage>();
			message2.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
			message2.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
			message2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message2.EM_MessageText = "Test";
			message2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(1);
			message2.EM_EM_RequestMessage = message1.PK;
			Factory.Save();

			TestDateAttribute.Date = ZDateTime.Now.AddMonths(2).ToDateTime();
			SetUpRegistry();
			var task = new MessagePurgeServiceTask(1);
			InitialiseTaskSchedule(task);
			task.RunTask();

			var newFactory = new BusinessObjectFactory();
			AssertNull("The message has been deleted", newFactory.Load<IXmlEDIMessage>(message1.PK));
			Assert("The EM_EM_RequestMessage field in the related message should be empty", newFactory.Load<IXmlEDIMessage>(message2.PK).EM_EM_RequestMessage.IsEmpty);
		}

		class DummyMessagePurgeSettingsConfig : PurgeSettingsConfig
		{
			public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
			{
				yield return AddApplicationCodePurgeType("..A", new InterchangeObjCollection() { NewInterchangeConfigObj(1, TimeUnit.Month) }, 1, TimeUnit.Month);

				yield return AddApplicationCodeMessageTypePurgeType("..B", new InterchangeObjCollection() { NewInterchangeConfigObj(1, TimeUnit.Month) }, new MessageTypePurgeTypeObjCollection()
					.Add("..1", "..1", 1, TimeUnit.Month)
				);

				yield return AddApplicationCodeMessageSubTypePurgeType("..C", new InterchangeObjCollection() { NewInterchangeConfigObj(1, TimeUnit.Month) }, new MessageSubTypePurgeTypeObjCollection()
					.Add("..2", "..2", 1, TimeUnit.Month)
				);

				yield return AddApplicationCodeMessageTypeAndSubTypePurgeType("..D", new InterchangeObjCollection() { NewInterchangeConfigObj(1, TimeUnit.Month) }, new MessageTypeAndSubTypePurgeTypeObjCollection()
					.Add("..3", "..3", "..4", "..4", 1, TimeUnit.Month)
				);

				yield return AddUnpurgableApplicationCode("..E");
			}
		}

		[TestDate(2012, 01, 01)]
		public void TestPurgingByDifferentPurgeTypes()
		{
			var mockConfigList = new List<PurgeSettingsConfig> { new DummyMessagePurgeSettingsConfig() };

			using (ObjectFactory.Substitute("MessagePurgeSettingsConfigList", mockConfigList))
			{
				var message1 = Factory.New<IXmlEDIMessage>();
				message1.EM_ApplicationCode = "..A";
				message1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

				var message2 = Factory.New<IXmlEDIMessage>();
				message2.EM_ApplicationCode = "..B";
				message2.EM_MessageType = "..1";
				message2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

				var message3 = Factory.New<IXmlEDIMessage>();
				message3.EM_ApplicationCode = "..C";
				message3.EM_MessageSubType = "..2";
				message3.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

				var message4 = Factory.New<IXmlEDIMessage>();
				message4.EM_ApplicationCode = "..D";
				message4.EM_MessageType = "..3";
				message4.EM_MessageSubType = "..4";
				message4.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

				var message5 = Factory.New<IXmlEDIMessage>();
				message5.EM_ApplicationCode = "..E";
				message5.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);

				Factory.Save();

				var task = new MessagePurgeServiceTask();
				InitialiseTaskSchedule(task);
				task.RunTask();

				AssertEquals(5, new BusinessObjectFactory().Load<EDIMessage>(new ZQuery()).Length);

				TestDateAttribute.Date = ZDateTime.Now.AddMonths(1).ToDateTime();

				task = new MessagePurgeServiceTask();
				InitialiseTaskSchedule(task);
				task.RunTask();

				AssertEquals(1, new BusinessObjectFactory().Load<EDIMessage>(new ZQuery()).Length);
			}
		}

		[TestDate(2012, 01, 01)]
		public void TestPurgeMessagesSeeksNonClusteredIndex()
		{
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var message1 = Factory.New<IXmlEDIMessage>();
				message1.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
				message1.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
				message1.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				message1.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(-1);
				Factory.Save();
				var message2 = Factory.New<IXmlEDIMessage>();
				message2.EM_ApplicationCode = ApplicationCodeList.Codes.UniversalDataMessaging;
				message2.EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalEvent;
				message2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
				message2.EM_MessageText = "Test";
				message2.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddDays(1);
				message2.EM_EM_RequestMessage = message1.PK;
				Factory.Save();

				TestDateAttribute.Date = ZDateTime.Now.AddMonths(2).ToDateTime();
				SetUpRegistry();
				var task = new MessagePurgeServiceTask(1);
				InitialiseTaskSchedule(task);
				task.RunTask();

				var queries = TestConnection.ExecutedCommandsAndQueryPlans.Where(t => t.Item1.Contains("UPDATE dbo.EDIMessage SET EM_EM_RequestMessage = NULL")).ToArray();
				Assert(queries.Length > 0);
				var queryPlanXml = queries[0].Item2.First(t => t.Contains("UPDATE dbo.EDIMessage SET EM_EM_RequestMessage = NULL"));
				var planalyzer = new QueryPlanalyzer(queryPlanXml);

				CombineAssertions(() =>
				{
					var indexDetails = planalyzer.IndexSeeks.First(x => x.IndexName == "FK_RX__EM_EM_RequestMessage");
					AssertEquals("EDIMessage", indexDetails.TableName);
					AssertEquals("NonClustered", indexDetails.IndexKind);
					Assert(!planalyzer.IndexScans.Any(x => x.IndexName == "NR_RC__EM_SystemCreateTimeUtc"));
					Assert(!planalyzer.IndexScans.Any(x => x.IndexName == "FK_RX__EM_EM_RequestMessage"));
				});
			}
		}

		[TestDate(2012, 01, 01)]
		public void TestServiceTaskDeletesApplicationCodesAddedToObjectFactory()
		{
			var message = Factory.New<IXmlEDIMessage>();
			message.EM_ApplicationCode = "TST";
			message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message.EM_MessageText = "Test";
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(-2);
			Factory.Save();

			TestDateAttribute.Date = ZDateTime.Now.AddMonths(2).ToDateTime();
			SetUpRegistry();
			var mockConfigList = new List<PurgeSettingsConfig> { new DummyApplicationCodeSetting() };
			using (ObjectFactory.Substitute("MessagePurgeSettingsConfigList", mockConfigList))
			{
				var task = new MessagePurgeServiceTask(1);
				InitialiseTaskSchedule(task);
				task.RunTask();
			}
			var newFactory = new BusinessObjectFactory();
			AssertNull("The message with dummy ApplicationCode should be deleted", newFactory.Load<IXmlEDIMessage>(message.PK));
		}

		class DummyApplicationCodeSetting : PurgeSettingsConfig
		{
			public override IEnumerable<ApplicationCodeObj> GetPurgeSettings()
			{
				yield return AddApplicationCodePurgeType("TST", new InterchangeObjCollection() { NewInterchangeConfigObj(1, TimeUnit.Month) }, 1, TimeUnit.Month);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		static ZDateTime GetMaxMessageCreationTimeToPurge(MessageTypeObj messageTypeObj)
		{
			if (messageTypeObj.PurgeTimeUnit == TimeUnit.Week)
			{
				return ZDateTime.UtcNow.AddDays(-messageTypeObj.PurgeTime * 7);
			}
			if (messageTypeObj.PurgeTimeUnit == TimeUnit.Month)
			{
				return ZDateTime.UtcNow.AddMonths(-messageTypeObj.PurgeTime);
			}
			if (messageTypeObj.PurgeTimeUnit == TimeUnit.Year)
			{
				return ZDateTime.UtcNow.AddYears(-messageTypeObj.PurgeTime);
			}
			throw new Exception("Unknown purge time unit");
		}

		static void SetUpRegistry()
		{
			var settings = eHubMessagingRegistry.GetDefaultPurgeSettings();

			var xmsApplicationCodeObj = settings.ApplicationCodes.OfType<ApplicationCodeObj>().Single(a => a.ApplicationCode == ApplicationCodeList.Codes.XMS);

			xmsApplicationCodeObj.Interchanges.RemoveAll();

			var xmsMessageTypes = xmsApplicationCodeObj.MessageTypes.Cast<MessageTypeObj>().ToArray();
			Array.ForEach(xmsMessageTypes, m => m.Selected = false);

			xmsApplicationCodeObj.MessageTypes.Remove(xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.Consols));
			xmsApplicationCodeObj.MessageTypes.Remove(xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.CFSLoadList));

			xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.AgencyBillsOfLading).Selected = true;
			xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.AgencyBillsOfLading).PurgeTime = 6;
			xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.AgencyBillsOfLading).PurgeTimeUnit = TimeUnit.Week;

			xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.Organizations).Selected = true;
			xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.Organizations).PurgeTime = 6;
			xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.Organizations).PurgeTimeUnit = TimeUnit.Week;

			xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.Events).Selected = true;
			xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.Events).PurgeTime = 2;
			xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.Events).PurgeTimeUnit = TimeUnit.Month;

			xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.Products).Selected = true;
			xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.Products).PurgeTime = 2;
			xmsMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.Products).PurgeTimeUnit = TimeUnit.Month;

			var udmApplicationCodeObj = settings.ApplicationCodes.OfType<ApplicationCodeObj>().Single(a => a.ApplicationCode == ApplicationCodeList.Codes.UniversalDataMessaging);

			Array.ForEach(udmApplicationCodeObj.Interchanges.Cast<InterchangeObj>().ToArray(), _ => _.Selected = false);

			var udmMessageTypes = udmApplicationCodeObj.MessageTypes.Cast<MessageTypeObj>().ToArray();
			Array.ForEach(udmMessageTypes, m => m.Selected = false);

			udmApplicationCodeObj.MessageTypes.Remove(udmMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalSchedule));
			udmApplicationCodeObj.MessageTypes.Remove(udmMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalTransaction));

			udmMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent).Selected = true;
			udmMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent).PurgeTime = 2;
			udmMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlUniversalEvent).PurgeTimeUnit = TimeUnit.Month;

			var ndmApplicationCodeObj = settings.ApplicationCodes.OfType<ApplicationCodeObj>().Single(a => a.ApplicationCode == ApplicationCodeList.Codes.NativeDataMessaging);

			Array.ForEach(ndmApplicationCodeObj.Interchanges.Cast<InterchangeObj>().ToArray(), _ => _.PurgeTime = 2);

			var ndmMessageTypes = ndmApplicationCodeObj.MessageTypes.Cast<MessageTypeObj>().ToArray();
			Array.ForEach(ndmMessageTypes, m => m.Selected = false);

			ndmApplicationCodeObj.MessageTypes.Remove(ndmMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlNativeDeclaration));
			ndmApplicationCodeObj.MessageTypes.Remove(ndmMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlNativeUNLOCO));

			ndmMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlNativeOrder).Selected = true;
			ndmMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlNativeOrder).PurgeTime = 2;
			ndmMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlNativeOrder).PurgeTimeUnit = TimeUnit.Month;

			ndmMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlNativeAirline).Selected = true;
			ndmMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlNativeAirline).PurgeTime = 1;
			ndmMessageTypes.Single(m => m.MessageSubType == EDIMessageSubTypeList.Codes.XmlNativeAirline).PurgeTimeUnit = TimeUnit.Year;

			var sysApplicationCodeObj = settings.ApplicationCodes.OfType<ApplicationCodeObj>().Single(a => a.ApplicationCode == ApplicationCodeList.Codes.SYS);

			Array.ForEach(sysApplicationCodeObj.Interchanges.Cast<InterchangeObj>().ToArray(), _ => _.PurgeTime = 2);

			var sysMessageTypes = sysApplicationCodeObj.MessageTypes.Cast<MessageTypeObj>().ToArray();
			sysMessageTypes.Single(m => m.MessageSubType == SystemMessageList.Codes.CurrentVersionReport).Selected = true;
			sysMessageTypes.Single(m => m.MessageSubType == SystemMessageList.Codes.CurrentVersionReport).PurgeTime = 2;
			sysMessageTypes.Single(m => m.MessageSubType == SystemMessageList.Codes.CurrentVersionReport).PurgeTimeUnit = TimeUnit.Month;

			sysMessageTypes.Single(m => m.MessageSubType == SystemMessageList.Codes.ERequestDocument).Selected = true;
			sysMessageTypes.Single(m => m.MessageSubType == SystemMessageList.Codes.ERequestDocument).PurgeTime = 2;
			sysMessageTypes.Single(m => m.MessageSubType == SystemMessageList.Codes.ERequestDocument).PurgeTimeUnit = TimeUnit.Month;

			sysMessageTypes.Single(m => m.MessageSubType == SystemMessageList.Codes.UserAccountReport).Selected = true;
			sysMessageTypes.Single(m => m.MessageSubType == SystemMessageList.Codes.UserAccountReport).PurgeTime = 2;
			sysMessageTypes.Single(m => m.MessageSubType == SystemMessageList.Codes.UserAccountReport).PurgeTimeUnit = TimeUnit.Month;
			eHubMessagingRegistry.Instance.PurgeSettingsItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
		}

		static void AssertMessageNotDeleted(XmlEDIMessage initialMessage, BusinessObjectFactory factory)
		{
			var messageAfterPurge = factory.Load<XmlEDIMessage>(initialMessage.PK);
			var pivots = factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation2ID, initialMessage.PK));
			AssertNotNull(messageAfterPurge);
			AssertEquals(1, pivots.Length);

			var query = new ZQuery().AddToFilter(StmNoteSchema.ST_ParentID, initialMessage.PK).AddToFilter(StmNoteSchema.ST_Table, "EDIMessage");
			var notesAfterPurge = factory.Load<StmNote>(query);
			Assert(notesAfterPurge.Length == 1);
		}

		static void AssertMessageAndMessageNoteDeleted(ZGuid pk, BusinessObjectFactory factory)
		{
			var messageAfterPurge = factory.Load<XmlEDIMessage>(pk);
			var pivots = factory.Load<GenPivot>(new ZQuery(GenPivotSchema.XX_Relation2ID, pk));
			AssertNull(messageAfterPurge);
			AssertEquals(0, pivots.Length);

			var query = new ZQuery().AddToFilter(StmNoteSchema.ST_ParentID, pk).AddToFilter(StmNoteSchema.ST_Table, "EDIMessage");
			var notesAfterPurge = factory.Load<StmNote>(query);
			Assert(notesAfterPurge.Length == 0);
		}

		XmlEDIInterchange CreateInterchange(string applicationCode, string direction)
		{
			var interchange = Factory.New<XmlEDIInterchange>();
			interchange.EI_ApplicationCode = applicationCode;
			interchange.EI_From = "A";
			interchange.EI_To = "B";
			interchange.EI_ReceiveTransmit = direction;

			return interchange;
		}

		XmlEDIMessage CreateMessageWithGenPivotRecordAndNote(XmlEDIInterchange parent, string messageSubType)
		{
			var message = parent.ContainedMessages.AddNew();
			message.FillWithValidTestData();
			message.EM_ApplicationCode = parent.EI_ApplicationCode;
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = parent.EI_ReceiveTransmit;

			var note = Factory.New<StmNote>();
			note.ST_ParentID = message.PK;
			note.ST_Table = "EDIMessage";

			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation2ID = message.PK;
			pivot.XX_Relation2TableCode = EDIMessageSchema.Constants.Prefix;
			pivot.XX_Relation1TableCode = StmALogSchema.Constants.Prefix;
			pivot.XX_RelationType = "XEM";

			return message;
		}
	}
}

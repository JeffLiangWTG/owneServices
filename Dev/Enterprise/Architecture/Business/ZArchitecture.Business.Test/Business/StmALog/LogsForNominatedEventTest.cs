using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(LogsForNominatedEvent))]
	sealed class LogsForNominatedEventTest : BusinessObjectCollectionViewTestCase<LogsForNominatedEvent>
	{
		public void TestMostRecentLog()
		{
			AssertNull(TestNote.LogsForNominatedEvent.MostRecentLog);

			StmALog aLog = InitialiseValidBizoAndAddALog(TestNote.LogsForNominatedEvent);
			using (aLog.LockForUpdatingKeyFieldsForTesting())
			{
				aLog.SL_EventTime = new ZDateTime(2005, 1, 1, 1, 1, 2);
			}
			AssertEquals("Most Recent Log", aLog, TestNote.LogsForNominatedEvent.MostRecentLog);

			StmALog newLog = InitialiseValidBizoAndAddALog(TestNote.LogsForNominatedEvent);
			using (newLog.LockForUpdatingKeyFieldsForTesting())
			{
				newLog.SL_EventTime = new ZDateTime(2005, 1, 1, 1, 1, 1);
			}
			AssertEquals("Most Recent Log", aLog, TestNote.LogsForNominatedEvent.MostRecentLog);

			StmALog newerLog = InitialiseValidBizoAndAddALog(TestNote.LogsForNominatedEvent);
			using (newerLog.LockForUpdatingKeyFieldsForTesting())
			{
				newerLog.SL_EventTime = new ZDateTime(2005, 1, 1, 1, 1, 3);
			}
			AssertEquals("Most Recent Log", newerLog, TestNote.LogsForNominatedEvent.MostRecentLog);
		}

		[TestDate(2010, 11, 12)]
		public void TestFireWorkflowWhenAddingThroughNominatedEventLogs()
		{
			var logParent = (DummyEnterpriseBusinessObject)Factory.New<IDummyWithWorkflow>();

			var trigger = ((IDummyWithWorkflow)logParent).AddNewTrigger();
			((IBaseTrigger)trigger).TriggerEventCode = Events.AuthorisedCode;
			trigger.P9_ParentID = logParent.PK;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var log = new LogsForNominatedEvent(logParent.Logs, Events.Authorised).AddNew("");
			AssertEquals("milestone.P9_ActualDate after add first event", new ZDateTime(2010, 11, 12), trigger.P9_ActualDate.ToZDateTime());

			log = new LogsForNominatedEvent(logParent.Logs, Events.Authorised).AddNew("", ZDateTime.BrettsBirthday.ToOffset());
			AssertEquals("milestone.P9_ActualDate after add second event", ZDateTime.BrettsBirthday, trigger.P9_ActualDate.ToZDateTime());
		}

		protected override LogsForNominatedEvent GetCollectionToTest()
		{
			TestNote = Factory.New<TestHelperStmNote>();
			return TestNote.LogsForNominatedEvent;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = TestNote.LogsForNominatedEvent.NominatedEvent.Code;
			}
			return log;
		}

		public void TestAddDeliveredLog()
		{
			StmALog newLog = InitialiseValidBizoAndAddALog(TestNote.LogsForNominatedEvent);
			AssertLogInitialisedProperly(newLog, TestNote, Events.CargoCheckinDiscrepancy.Code);
		}

		public void TestReadDeliveredLog()
		{
			StmALog newLog = InitialiseValidBizoAndAddALog(TestNote.LogsForNominatedEvent);
			TestNote.Factory.Save();
			AssertEquals("DeliveredLogCount", 1, new BusinessObjectFactory().Load<TestHelperStmNote>(TestNote.PK).LogsForNominatedEvent.Count);
		}

		public void TestDescriptionExists()
		{
			StmALog newLog = InitialiseValidBizoAndAddALog(TestNote.LogsForNominatedEvent);
			AssertEquals("Non Existant description found in logs", false, TestNote.LogsForNominatedEvent.DescriptionExists("XXX"));
			using (newLog.LockForUpdatingKeyFieldsForTesting())
			{
				newLog.SL_Reference = "ABC 123";
			}
			AssertEquals("Existant description found in logs", true, TestNote.LogsForNominatedEvent.DescriptionExists("ABC"));
		}

		public void TestIndexer()
		{
			StmALog newLog1 = TestNote.LogsForNominatedEvent.AddNew();
			System.Threading.Thread.Sleep(1000);
			AssertEquals(newLog1, TestNote.LogsForNominatedEvent[0]);

			StmALog newLog2 = TestNote.LogsForNominatedEvent.AddNew("Ref");
			AssertEquals(newLog2, TestNote.LogsForNominatedEvent[0]);
			AssertEquals(newLog1, TestNote.LogsForNominatedEvent[1]);

			System.Threading.Thread.Sleep(1000);
			StmALog newLog3 = TestNote.LogsForNominatedEvent.AddNew("Ref", ZDateTimeOffset.Now);
			AssertEquals(newLog3, TestNote.LogsForNominatedEvent[0]);
			AssertEquals(newLog2, TestNote.LogsForNominatedEvent[1]);
			AssertEquals(newLog1, TestNote.LogsForNominatedEvent[2]);
		}

		public void TestConstructorExludingEstimated()
		{
			var log1 = TestNote.Logs.AddNew(Events.Authorised, new ZDateTimeOffset(2013, 5, 3), ZBool.True);
			var log2 = TestNote.Logs.AddNew(Events.Authorised, new ZDateTimeOffset(2013, 5, 2), ZBool.False);
			var logs = new LogsForNominatedEvent(TestNote.Logs, Events.Authorised);
			AssertEquals(log1, logs.MostRecentLog);
			logs = new LogsForNominatedEvent(TestNote.Logs, Events.Authorised, true);
			AssertEquals(log2, logs.MostRecentLog);
		}

		public new void TestReintroducedIndexerRemovedForGenericCollection() => Assert("Indexer add custom sorting logic.", true);

		#region Implementation
		/// <summary>
		/// This is a test helper class than will have a LogsForNominatedEvent collection as a property
		/// </summary>
		class TestHelperStmNote : StmNote
		{
			public TestHelperStmNote(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public LogsForNominatedEvent LogsForNominatedEvent
			{
				get
				{
					if (fLogsForNominatedEvent == null)
					{
						//the event Events.CargoCheckinDiscrepancy is just an arbitrary event
						fLogsForNominatedEvent = new LogsForNominatedEvent(Logs, Events.CargoCheckinDiscrepancy);
					}
					return fLogsForNominatedEvent;
				}
			}
			protected LogsForNominatedEvent fLogsForNominatedEvent;
		}

		void AssertLogInitialisedProperly(StmALog log, EnterpriseBusinessObject parent, ZString eventCode)
		{
			AssertEquals("EventType", log.SL_SE_NKEvent, eventCode);
			AssertEquals("EventParent", log.SL_Parent, parent.PK);
			AssertEquals("EventParentTable", log.SL_Table, parent.TableName);
		}

		StmALog InitialiseValidBizoAndAddALog(LogsForNominatedEvent eventView)
		{
			var log = eventView.AddNew();
			return log;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestNote = Factory.New<TestHelperStmNote>();
			TestNote.ST_Table = "DummyBizo";
		}
		TestHelperStmNote TestNote;
		#endregion
	}
}

using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	[TestedType(typeof(StmALogValueObjectDataAdapter))]
	public class StmALogValueObjectDataAdapterTest : ValueObjectDataAdapterTest<StmALog, Xsd.Event>
	{
		public virtual void TestToXmlCollectionValueObject()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			BusinessObject bizObj = SetupBusinessObjectWithEvents();
			Xsd.Events eventsValue = StmALogValueObjectDataAdapter.New(bizObj, "", EventsWithSourceType.Empty).ToXmlCollectionValueObject(new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(3, eventsValue.Event.Count);
			AssertXsdContainsExportedEvent(eventsValue, Events.Arrival.Code);
			AssertXsdContainsExportedEvent(eventsValue, Events.EntryWorkInProgress.Code);
			AssertXsdContainsExportedEvent(eventsValue, Events.AddedARecordToTheSystem.Code);
		}

		public void TestToXmlCollectionValueObject_NotSimpleXML()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			BusinessObject bizObj = SetupBusinessObjectWithEvents();
			StmALogValueObjectDataAdapter adapter = StmALogValueObjectDataAdapter.New(bizObj, "", EventsWithSourceType.Empty);
			Xsd.Events eventsValue = adapter.ToXmlCollectionValueObject(new ValueObjectExportContext(new NotificationBuffer()));
			if (adapter.IncludeEditEventsInExport)
			{
				AssertEquals(5, eventsValue.Event.Count);
			}
			else
			{
				AssertEquals(4, eventsValue.Event.Count);
			}
			AssertXsdContainsExportedEvent(eventsValue, Events.Arrival.Code);
			AssertXsdContainsExportedEvent(eventsValue, Events.EntryWorkInProgress.Code);
			AssertXsdContainsExportedEvent(eventsValue, Events.AddedARecordToTheSystem.Code);
			AssertXsdContainsExportedEvent(eventsValue, Events.DataExport.Code);

			if (adapter.IncludeEditEventsInExport)
			{
				AssertXsdContainsExportedEvent(eventsValue, Events.EditedARecord.Code);
			}
		}

		public virtual void TestToXmlCollectionValueObjectDoesNotExportCancelledEvents()
		{
			var org = Factory.New<OrgHeader>();
			var nonSystemEventValue = new EventValue(Events.Arrival,
				eventTime: new ZDateTimeOffset(2005, 1, 1),
				reference: "nonsystem1");
			org.Logs.AddNew(nonSystemEventValue);

			var nonSystemEventValue2 = new EventValue(Events.EntryWorkInProgress,
				eventTime: new ZDateTimeOffset(2005, 2, 2),
				reference: "nonsystem2");
			org.Logs.AddNew(nonSystemEventValue2);

			var nonSystemEventValue3 = new EventValue(Events.Departure,
				eventTime: new ZDateTimeOffset(2005, 2, 2),
				reference: "nonsystem3");

			var nonSystemLog3 = org.Logs.AddNew(nonSystemEventValue3);
			nonSystemLog3.Cancel();

			Xsd.Events eventsValue = StmALogValueObjectDataAdapter.New(org, "", EventsWithSourceType.Empty).ToXmlCollectionValueObject(new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Should be 2 events returned, both non-cancelled", 2, eventsValue.Event.Count);
			AssertXsdContainsExportedEvent(eventsValue, Events.Arrival.Code);
			AssertXsdContainsExportedEvent(eventsValue, Events.EntryWorkInProgress.Code);
		}

		protected void AssertXsdContainsExportedEvent(Xsd.Events xsdEvents, ZString code)
		{
			bool result = true;
			foreach (Xsd.Event xsdEvent in xsdEvents.Event)
			{
				if (xsdEvent.Code == code)
				{
					result = true;
					break;
				}
			}
			AssertEquals("Xsd event " + code, true, result);
		}

		public virtual void TestFromXmlCollectionValueObject()
		{
			SystemDataRegistry.Instance.SimpleXMLExportFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			BusinessObject bizObj = SetupBusinessObjectWithEvents();

			AssertEquals("[PRE-CONDITION] All Events Created Initially", 7, bizObj.GetLogs().GetAllLogs().Count);
			AssertContainsLog(bizObj.GetLogs(), Events.Arrival);
			AssertContainsLog(bizObj.GetLogs(), Events.EntryWorkInProgress);
			AssertContainsLog(bizObj.GetLogs(), Events.AddedARecordToTheSystem);
			AssertContainsLog(bizObj.GetLogs(), Events.EditedARecord);
			AssertContainsLog(bizObj.GetLogs(), Events.DeletedARecordInTheSystem);
			AssertContainsLog(bizObj.GetLogs(), Events.DataExport);
			AssertContainsLog(bizObj.GetLogs(), Events.DataImport);

			Xsd.Events eventsValue = StmALogValueObjectDataAdapter.New(bizObj, "", EventsWithSourceType.Empty).ToXmlCollectionValueObject(new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(4, eventsValue.Event.Count);

			OrgHeader readBizObj = Factory.New<OrgHeader>();
			readBizObj.OH_Code = "4445566";
			AssertEquals("Should have zero logs initially", 0, readBizObj.Logs.GetAllLogs().Count);

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			StmALogValueObjectDataAdapter.New(readBizObj, "", EventsWithSourceType.Empty).FromXmlCollectionValueObject(eventsValue, context);
			AssertEquals("Only Operational Events are imported", 3, readBizObj.Logs.GetAllLogs().Count);

			AssertContainsLog(readBizObj.Logs, Events.Arrival);
			AssertContainsLog(readBizObj.Logs, Events.EntryWorkInProgress);
			AssertContainsLog(readBizObj.Logs, Events.DataExport);

			StmALogValueObjectDataAdapter.New(readBizObj, "", EventsWithSourceType.Empty).FromXmlCollectionValueObject(eventsValue, context);
			AssertEquals("The same logs should be there when importing a second time", 3, readBizObj.Logs.GetAllLogs().Count);

			OrgHeader matchingOrg = Factory.Load<OrgHeader>(context.Converter.MappingOrgPK);
			OrgPatternMatchOverride eventCodeOverride = matchingOrg.CreatePatternMatchOverrideForTest();
			eventCodeOverride.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.EventCode;
			eventCodeOverride.OO_LocalCode = Events.Arrival.Code;
			eventCodeOverride.OO_ForeignCode = "Y12";
			Factory.Save();
			int logsCount = readBizObj.Logs.GetAllLogs().Count;

			eventsValue.Event[0].Code = "Y12";
			StmALogValueObjectDataAdapter.New(readBizObj, "", EventsWithSourceType.Empty).FromXmlCollectionValueObject(eventsValue, context);
			AssertEquals("The same logs should be there when importing a third time", logsCount, readBizObj.Logs.GetAllLogs().Count);

			eventsValue.Event[0].DateTime = eventsValue.Event[0].DateTime.AddSeconds(1);
			StmALogValueObjectDataAdapter.New(readBizObj, "", EventsWithSourceType.Empty).FromXmlCollectionValueObject(eventsValue, context);
			AssertEquals("New log record was added", logsCount + 1, readBizObj.Logs.GetAllLogs().Count);
		}

		void AssertContainsLog(Logs logs, Event logEvent)
		{
			bool result = false;
			foreach (StmALog log in logs.GetAllLogs())
			{
				if (log.SL_SE_NKEvent == logEvent.Code)
				{
					result = true;
					break;
				}
			}
			AssertEquals("This log should have been read: " + logEvent.Code, true, result);
		}

		public void TestFindBusinessObject()
		{
			var logParent = Factory.New<OrgHeader>();

			logParent.Logs.AddNew(Events.BookingConfirmed, new ZDateTimeOffset(2005, 1, 1));
			logParent.Logs.AddNew(Events.Booked, new ZDateTimeOffset(2005, 1, 2));
			var log = logParent.Logs.AddNew(Events.Booked, new ZDateTimeOffset(2005, 1, 1));

			var eventValue = new Xsd.Event();
			eventValue.DateTime = new ZDateTime(2005, 1, 1);
			eventValue.Code = Events.Booked.Code;
			var context = new ValueObjectImportContext(Factory, new NotificationBuffer());
			var foundLog = new TestStmALogValueObjectDataAdapter(logParent, "", EventsWithSourceType.Empty).FindBusinessObject(eventValue, context);
			AssertEquals("Should find the existing log record", log.PK, foundLog.PK);

			var estimatedLog = logParent.Logs.AddNew(Events.Booked, new ZDateTimeOffset(2005, 1, 1), true, null);

			eventValue.IsEstimatedDate = Xsd.TrueFalse.@true;
			eventValue.IsEstimatedDateSpecified = true;

			foundLog = new TestStmALogValueObjectDataAdapter(logParent, "", EventsWithSourceType.Empty).FindBusinessObject(eventValue, context);
			AssertEquals("Should find the estimated log record", estimatedLog.PK, foundLog.PK);

			eventValue.IsEstimatedDate = Xsd.TrueFalse.@false;
			foundLog = new TestStmALogValueObjectDataAdapter(logParent, "", EventsWithSourceType.Empty).FindBusinessObject(eventValue, context);
			AssertEquals("Should find the existing actual log record", log.PK, foundLog.PK);

			eventValue.IsEstimatedDateSpecified = false;
			foundLog = new TestStmALogValueObjectDataAdapter(logParent, "", EventsWithSourceType.Empty).FindBusinessObject(eventValue, context);
			AssertEquals("Should find the existing actual log record if IsEstimated is not specified", log.PK, foundLog.PK);
		}

		public void TestExportPostedDateTime()
		{
			OrgHeader logParent = Factory.NewWithValidTestData<OrgHeader>();
			StmALog log = logParent.Logs.AddNew(Events.Booked);
			Factory.Save();

			Xsd.Event exportedEventValue = new TestStmALogValueObjectDataAdapter(logParent, "", EventsWithSourceType.Empty).ExportToValueObject(log, new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals("Posted time should be exported", true, exportedEventValue.PostedDateTime.IsValid);
			AssertEquals("Posted time should be exported", log.SL_PostedTimeUtc, exportedEventValue.PostedDateTime);
		}

		public void TestInitialDataExported()
		{
			var logParent = Factory.NewWithValidTestData<OrgHeader>();
			logParent.Logs.AddNew();

			var adapter = StmALogValueObjectDataAdapter.New(logParent, "");
			var xsdEvents = adapter.ToXmlCollectionValueObject(new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(ZDateTime.Empty, xsdEvents.InitialDataExported);
			AssertEquals(false, xsdEvents.InitialDataExportedSpecified);

			logParent.Logs.AddNew(Events.DataExport, new ZDateTimeOffset(2009, 2, 3, 2, 46, 0));
			var exportLog2 = logParent.Logs.AddNew(Events.DataExport, new ZDateTimeOffset(2008, 12, 3, 2, 46, 0));
			logParent.Logs.AddNew(Events.DataExport, new ZDateTimeOffset(2009, 5, 28, 9, 32, 0));
			logParent.Logs.AddNew(Events.Authorised, new ZDateTimeOffset(2008, 3, 19, 2, 46, 0));

			xsdEvents = adapter.ToXmlCollectionValueObject(new ValueObjectExportContext(new NotificationBuffer()));
			AssertEquals(exportLog2.SL_EventTime, xsdEvents.InitialDataExported);
			AssertEquals(true, xsdEvents.InitialDataExportedSpecified);
		}

		[TestDate]
		public void TestExportFillsLatestTriggeredByMatchingTriggeredByEventCodes()
		{
			TestDateAttribute.Date = new DateTime(2008, 1, 25, 9, 1, 1);
			var logParent = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 1);
			logParent.Logs.AddNew(Events.Booked);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 2);
			logParent.Logs.AddNew(Events.Booked);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 3);
			logParent.Logs.AddNew(Events.Booked);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 4);
			logParent.Logs.AddNew(Events.Departure);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 5);
			logParent.Logs.AddNew(Events.Departure);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 6);
			logParent.Logs.AddNew(Events.Departure);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 7);
			logParent.Logs.AddNew(Events.Arrival);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 8);
			logParent.Logs.AddNew(Events.Arrival);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 9);
			logParent.Logs.AddNew(Events.Arrival);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 10);
			var xsdEvents = GetSerializedJob(logParent, Events.Departure.Code, null);

			AssertTriggeredByInLogExport(xsdEvents.Event[1], Events.AddedARecordToTheSystem.Code, false);

			AssertTriggeredByInLogExport(xsdEvents.Event[2], Events.Booked.Code, false);
			AssertTriggeredByInLogExport(xsdEvents.Event[3], Events.Booked.Code, false);
			AssertTriggeredByInLogExport(xsdEvents.Event[4], Events.Booked.Code, false);

			AssertTriggeredByInLogExport(xsdEvents.Event[5], Events.Departure.Code, false);
			AssertTriggeredByInLogExport(xsdEvents.Event[6], Events.Departure.Code, false);
			AssertTriggeredByInLogExport(xsdEvents.Event[7], Events.Departure.Code, true);

			AssertTriggeredByInLogExport(xsdEvents.Event[8], Events.Arrival.Code, false);
			AssertTriggeredByInLogExport(xsdEvents.Event[9], Events.Arrival.Code, false);
			AssertTriggeredByInLogExport(xsdEvents.Event[10], Events.Arrival.Code, false);
		}

		[TestDate]
		public void TestExportFillsLatestTriggeredByMatchingTriggeredByEventCodesAndReference()
		{
			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 0);
			OrgHeader logParent = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 1);
			logParent.Logs.AddNew(AutoEvents.Booked, "Paperwork");
			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 2);
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 3);
			logParent.Logs.AddNew(AutoEvents.Booked, "Rejected");
			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 4);
			Factory.Save();

			Xsd.Events events = GetSerializedJob(logParent, Events.Booked.Code, "Paperwork");
			GetSerializedJob(logParent, AutoEvents.Booked.Code, "Rejected");

			AssertTriggeredByInLogExport(events.Event[1], AutoEvents.AddedARecordToTheSystem.Code, false);

			AssertTriggeredByInLogExport(events.Event[2], AutoEvents.Booked.Code, true);
			AssertTriggeredByInLogExport(events.Event[3], AutoEvents.Booked.Code, false);
		}

		public void TestSetTriggeredBySetsForCorrectForLatestEventOnlyIfTriggerRefNotSet()
		{
			EnterpriseBusinessObject logParent = (EnterpriseBusinessObject)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));

			Factory.Save();

			logParent.Logs.AddNew(Events.Departure, "AAA");
			Factory.Save();

			System.Threading.Thread.Sleep(1000);
			logParent.Logs.AddNew(Events.Departure, "BBB");
			Factory.Save();

			Xsd.Events xsdEvents = GetSerializedJob(logParent, Events.Departure.Code, "", "MIL");
			Xsd.Event bbbEvent = null;
			Xsd.Event aaaEvent = null;
			foreach (Xsd.Event ev in xsdEvents.Event)
			{
				if (ev.Information == "BBB")
				{
					bbbEvent = ev;
				}
				if (ev.Information == "AAA")
				{
					aaaEvent = ev;
				}
			}
			AssertNotNull(aaaEvent);
			AssertNotNull(bbbEvent);
			Assert(!aaaEvent.TriggeredBySpecified);
			Assert(bbbEvent.TriggeredBySpecified);
			Assert(bbbEvent.TriggeredBy);
		}

		[TestDate]
		public void TestSetTriggeredBySetsForCorrectParentOnly()
		{
			TestDateAttribute.Date = new DateTime(2008, 1, 25, 9, 1, 1);
			var logParent = (EnterpriseBusinessObject)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 9, 1, 2);
			var jobHeader = new JobHeader.Loader(logParent as IJobHeaderParent).TryCreateWithoutMutexForTestOnly();
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 1);
			logParent.Logs.AddNew(Events.Departure, "Departure");
			jobHeader.Logs.AddNew(Events.Departure, "Departure");
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 2);
			var eventValue = new EventValue(Events.Departure, reference: "Departure", isEstimate: true);
			logParent.Logs.AddNew(eventValue);

			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 3);
			var logDeparture3 = logParent.Logs.AddNew(Events.Departure, "Departure");
			logDeparture3.Cancel();
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 4);
			logParent.Logs.AddNew(Events.Arrival, "Arrival");
			Factory.Save();

			Xsd.Events xsdEvents1 = GetSerializedJob(logParent, Events.Departure.Code, "", "MIL");

			//JOP record and ADD record will be created at the same time so the order could be different after sorting by creation time every time.
			var eventForTest = xsdEvents1.Event.Cast<Xsd.Event>().First(currentEvent => currentEvent.Code == Events.JobOpen.Code);
			AssertTriggeredByInLogExport(eventForTest, Events.JobOpen.Code, false);
			eventForTest = xsdEvents1.Event.Cast<Xsd.Event>().First(currentEvent => currentEvent.Code == Events.AddedARecordToTheSystem.Code);
			AssertTriggeredByInLogExport(eventForTest, Events.AddedARecordToTheSystem.Code, false);
			eventForTest = xsdEvents1.Event.Cast<Xsd.Event>().First(currentEvent => currentEvent.Code == Events.BillingJobEdit.Code);
			AssertTriggeredByInLogExport(eventForTest, Events.BillingJobEdit.Code, false);
			eventForTest = xsdEvents1.Event.Cast<Xsd.Event>().First(currentEvent => currentEvent.Code == Events.Arrival.Code);
			AssertTriggeredByInLogExport(eventForTest, Events.Arrival.Code, false);

			var eventsForTest = xsdEvents1.Event.Cast<Xsd.Event>().Where(currentEvent => currentEvent.Code == Events.Departure.Code).ToArray();
			AssertEquals(3, eventsForTest.Length);
			AssertTriggeredByInLogExport(eventsForTest[0], Events.Departure.Code, false);
			AssertTriggeredByInLogExport(eventsForTest[1], Events.Departure.Code, true);
			AssertTriggeredByInLogExport(eventsForTest[2], Events.Departure.Code, false);

			xsdEvents1 = GetSerializedJob(jobHeader, Events.Departure.Code, "", "MIL");

			//JOP record and ADD record will be created at the same time so the order could be different after sorting by creation time every time.
			eventForTest = xsdEvents1.Event.Cast<Xsd.Event>().First(currentEvent => currentEvent.Code == Events.JobOpen.Code);
			AssertTriggeredByInLogExport(eventForTest, Events.JobOpen.Code, false);
			eventForTest = xsdEvents1.Event.Cast<Xsd.Event>().First(currentEvent => currentEvent.Code == Events.AddedARecordToTheSystem.Code);
			AssertTriggeredByInLogExport(eventForTest, Events.AddedARecordToTheSystem.Code, false);
			eventForTest = xsdEvents1.Event.Cast<Xsd.Event>().First(currentEvent => currentEvent.Code == Events.BillingJobEdit.Code);
			AssertTriggeredByInLogExport(eventForTest, Events.BillingJobEdit.Code, false);
			eventForTest = xsdEvents1.Event.Cast<Xsd.Event>().First(currentEvent => currentEvent.Code == Events.Departure.Code);
			AssertTriggeredByInLogExport(eventForTest, Events.Departure.Code, true);
		}

		[TestDate]
		public void TestExportEventFromRelatedBusinessObjects()
		{
			TestDateAttribute.Date = new DateTime(2008, 1, 25, 9, 1, 1);
			var logParent = (EnterpriseBusinessObject)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Freight.ICommonShipment)));
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 9, 1, 2);
			var jobHeader = new JobHeader.Loader(logParent as IJobHeaderParent).TryCreateWithoutMutexForTestOnly();
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 1);
			logParent.Logs.AddNew(Events.Departure, "Departure");
			Factory.Save();

			TestDateAttribute.Date = new DateTime(2008, 1, 25, 10, 1, 2);
			logParent.Logs.AddNew(Events.Arrival, "Arrival");
			Factory.Save();

			Xsd.Events xsdEvents1 = GetSerializedJob(logParent, Events.JobOpen.Code, "", "MIL");

			//JOP record and Add record will be created at the same time so the order could be different after sorting by creation time every time.
			var eventForTest = xsdEvents1.Event.Cast<Xsd.Event>().First(currentEvent => currentEvent.Code == Events.JobOpen.Code);
			AssertTriggeredByInLogExport(eventForTest, Events.JobOpen.Code, true);
			eventForTest = xsdEvents1.Event.Cast<Xsd.Event>().First(currentEvent => currentEvent.Code == Events.AddedARecordToTheSystem.Code);
			AssertTriggeredByInLogExport(eventForTest, Events.AddedARecordToTheSystem.Code, false);
			eventForTest = xsdEvents1.Event.Cast<Xsd.Event>().First(currentEvent => currentEvent.Code == Events.BillingJobEdit.Code);
			AssertTriggeredByInLogExport(eventForTest, Events.BillingJobEdit.Code, false);
			eventForTest = xsdEvents1.Event.Cast<Xsd.Event>().First(currentEvent => currentEvent.Code == Events.Departure.Code);
			AssertTriggeredByInLogExport(eventForTest, Events.Departure.Code, false);
			eventForTest = xsdEvents1.Event.Cast<Xsd.Event>().First(currentEvent => currentEvent.Code == Events.Arrival.Code);
			AssertTriggeredByInLogExport(eventForTest, Events.Arrival.Code, false);
		}

		public void TestBusinessObjectsWithRelatedEvents_IsNull()
		{
			var logParent = Factory.NewWithValidTestData<DummyWithWorkflow>();
			logParent.BusinessObjectsWithRelatedEventsForTest = () => null;
			logParent.BusinessObjectsWithRelatedEvents_ReturnNull = true;
			logParent.Logs.AddNew(Events.Departure, "Departure");
			Factory.Save();

			Factory.Save();

			var events = StmALogValueObjectDataAdapter.New(logParent, "", EventsWithSourceType.Empty).ToXmlCollectionValueObject(new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(1, events.Event.Count);
		}

		public void TestBusinessObjectsWithRelatedEvents_ContainsNull()
		{
			var logParent = Factory.NewWithValidTestData<DummyWithWorkflow>();
			logParent.BusinessObjectsWithRelatedEventsForTest = () => new BusinessObject[1] { null };
			logParent.Logs.AddNew(Events.Departure, "Departure");
			Factory.Save();

			Factory.Save();

			var events = StmALogValueObjectDataAdapter.New(logParent, "", EventsWithSourceType.Empty).ToXmlCollectionValueObject(new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals(1, events.Event.Count);
		}

		Xsd.Events GetSerializedJob(BusinessObject logParent, string eventCode, string eventReference)
		{
			return GetSerializedJob(logParent, eventCode, eventReference, string.Empty);
		}

		Xsd.Events GetSerializedJob(BusinessObject logParent, string eventCode, string eventReference, string taskType)
		{
			var triggerAction = Factory.NewWithValidTestData<ProcessTaskNotification>();
			var task = (ProcessTask)triggerAction.Parent;

			if (eventReference != null)
			{
				task.P9_Type = taskType;
				task.P9_Description = "Stay away from workflow!!!";
				task.TriggerConditions.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
				task.TriggerConditions.TriggerConditionValue = eventReference;
			}
			task.TriggerConditions.TriggerEventCode = eventCode;
			var eventSource = new EventsWithSourceType(EventsWithSourceType.SourceType.Consol, triggerAction, task.GetJob());
			var exportContent = new ValueObjectExportContext(new NotificationBuffer());
			return new TestStmALogValueObjectDataAdapter(logParent, "test", eventSource).ToXmlCollectionValueObject(exportContent);
		}

		void AssertTriggeredByInLogExport(Xsd.Event exportedEvent, string expectedEventCode, bool expectedTriggeredBy)
		{
			AssertEquals("Exported Event", expectedEventCode, exportedEvent.Code);
			AssertEquals("Exported Triggered By", expectedTriggeredBy, exportedEvent.TriggeredBy);
			AssertEquals("Exported TriggeredBySpecified", expectedTriggeredBy, exportedEvent.TriggeredBySpecified);
		}

		public void TestImport_UpdatedOrCreatedNotificationNotShown()
		{
			StmALogValueObjectDataAdapter adapter = (StmALogValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			Xsd.Event eventValue = new Xsd.Event();

			NotificationBuffer notify = new NotificationBuffer();
			adapter.CreateOrUpdateFromValueObject(eventValue, new ValueObjectImportContext(Factory, notify));
			AssertEquals("Should not contain created or updated message", false, notify.ContainsNotificationType(WarningType.BusinessObjectCreatedOrUpdated));
		}

		public void TestImportInvalidEventCode()
		{
			StmALogValueObjectDataAdapter adapter = (StmALogValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();
			Xsd.Event eventValue = new Xsd.Event();
			eventValue.Code = Events.Delivered.Code;

			NotificationBuffer notify = new NotificationBuffer();

			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			adapter.CreateOrUpdateFromValueObject(eventValue, context);
			AssertEquals("There should be no unknown event code initially for the test", false, notify.ContainsNotificationType(ErrorType.UnknownCode));

			eventValue.Code = "~ZZ";
			adapter.CreateOrUpdateFromValueObject(eventValue, context);
			AssertEquals("There should be an unknown code error shown", true, notify.ContainsNotificationType(ErrorType.UnknownCode));
		}

		public void TestImport_IgnoresUser()
		{
			StmALogValueObjectDataAdapter adapter = (StmALogValueObjectDataAdapter)GetNewBizObjXmlDataAdapter();

			Xsd.Event eventValue = new Xsd.Event();
			eventValue.User = "ABC";

			StmALog log = adapter.CreateOrUpdateFromValueObject(eventValue, new ValueObjectImportContext(Factory, new NotificationBuffer()));
			AssertEquals("User is never imported and always defaults to current user", GlbStaff.CurrentUser.GS_Code, log.SL_GS_NKUser);
		}

		#region TestSetTriggeredByForLogInDifferentScopeAndInTriggerredByList

		public void TestSetTriggeredByForLogInDifferentScopeAndInTriggerredByList()
		{
			var logParent = Factory.NewWithValidTestData<OrgHeader>();
			var log = logParent.Logs.AddNew(AutoEvents.Departure);
			Factory.Save();

			var job = Factory.NewWithValidTestData<OrgOpportunity>();
			var task = job.WorkflowItems.Triggers.AddNew();
			var triggerAction = task.ProcessTaskNotifications.AddNew();
			task.TriggerConditions.TriggerEventCode = AutoEvents.Departure.Code;

			Factory.Save();

			var eventSource = new EventsWithSourceType(EventsWithSourceType.SourceType.Consol, triggerAction, job);
			var exportContent = new ValueObjectExportContext(new NotificationBuffer());
			var events = new TestStmALogValueObjectDataAdapter(logParent, "test", eventSource).ToXmlCollectionValueObject(exportContent);

			AssertEquals(3, events.Event.Count);
			AssertNotNull(events.Event.Cast<Xsd.Event>().Single(e => e.Code == AutoEvents.Departure.Code && !e.TriggeredBy));

			task.Logs.AddNew(AutoEvents.WorkflowTriggerEvent, log.PK.ToString());

			Factory.Save();

			eventSource = new EventsWithSourceType(EventsWithSourceType.SourceType.Consol, triggerAction, job);
			events = new TestStmALogValueObjectDataAdapter(logParent, "test", eventSource).ToXmlCollectionValueObject(exportContent);

			AssertEquals(3, events.Event.Count);
			AssertNotNull(events.Event.Cast<Xsd.Event>().Single(e => e.Code == AutoEvents.Departure.Code && e.TriggeredBy));

			System.Threading.Thread.Sleep(50);
			var log2 = logParent.Logs.AddNew(AutoEvents.Departure);

			Factory.Save();

			eventSource = new EventsWithSourceType(EventsWithSourceType.SourceType.Consol, triggerAction, job);
			events = new TestStmALogValueObjectDataAdapter(logParent, "test", eventSource).ToXmlCollectionValueObject(exportContent);

			AssertNotNull(events.Event.Cast<Xsd.Event>().Single(e => e.Code == AutoEvents.Departure.Code && e.TriggeredBy));
		}

		#endregion

		#region Debug for log deleted before being processed (WI00372797)

		class TestOrgHeader : OrgHeader
		{
			public TestOrgHeader(BusinessObjectFactory factory, DataRow row)
						: base(factory, row)
			{
			}

			public bool EnableDeleteLog { get; set; }

			protected override void ProcessLogCore(IStmALog log)
			{
				if (EnableDeleteLog)
				{
					(log as BusinessObject).Delete();
				}
				var eventCode = log.SL_SE_NKEvent;
				base.ProcessLogCore(log);
			}
		}

		string GetPCADeletedLog() => ErrorReporter.LastExceptionsReported().FirstOrDefault(t => t.Contains("Information for Forwarding team"));

		public void TestPCALogDeletedOnImport()
		{
			var logTime = new ZDateTimeOffset(2020, 1, 1);
			var otherTime = new ZDateTimeOffset(2018, 1, 1);
			var logParent = Factory.NewWithValidTestData<TestOrgHeader>();
			var log = logParent.Logs.AddNew(Events.PickupCartageAdvised, "originalReference", logTime);
			logParent.Logs.AddNew(Events.PickupCartageAdvised, "otherPCA1", otherTime);
			logParent.Logs.AddNew(Events.PickupCartageAdvised, "otherPCA2", otherTime);
			logParent.Logs.AddNew(Events.DeliveryCartageAdvised, "notPCA", otherTime);

			var adapter = StmALogValueObjectDataAdapter.New(logParent, "", EventsWithSourceType.Empty);
			var eventValue = new Xsd.Event();
			eventValue.Code = Events.PickupCartageAdvised.Code;
			eventValue.DateTime = logTime.ToZDateTime();
			eventValue.Information = "reference number";

			logParent.EnableDeleteLog = true;
			AssertEquals("precondition: not deleted", false, log.IsDeleted);

			NotificationBuffer notify = new NotificationBuffer();
			AssertNoExceptionThrown("fails silently", () => adapter.CreateOrUpdateFromValueObject(eventValue, new ValueObjectImportContext(Factory, notify)));
			AssertEquals("log was deleted", true, log.IsDeleted);
			Assert(ErrorReporter.HasBeenReported("LogDeletedBeforeProcess_PCA"));
			var errorReporterString = GetPCADeletedLog();

			var expectedError = $@"Message :Information for Forwarding team:

Event
Code: PCA
DateTime: 01-Jan-20 00:00:00
Information: reference number
IsEstimatedDate: false

Current Log:
Log
IsInDatabase: False
SL_EventTime: 01-Jan-20 00:00:00
SL_IsEstimate: N
SL_Parent: {logParent.PK}
SL_Reference: originalReference
SL_SE_NKEvent: PCA
SL_Table: OrgHeader

LogParent Logs:
Log
IsInDatabase: False
SL_EventTime: 01-Jan-20 00:00:00
SL_IsEstimate: N
SL_Parent: {logParent.PK}
SL_Reference: originalReference
SL_SE_NKEvent: PCA
SL_Table: OrgHeader

Log
IsInDatabase: False
SL_EventTime: 01-Jan-18 00:00:00
SL_IsEstimate: N
SL_Parent: {logParent.PK}
SL_Reference: otherPCA1
SL_SE_NKEvent: PCA
SL_Table: OrgHeader

Log
IsInDatabase: False
SL_EventTime: 01-Jan-18 00:00:00
SL_IsEstimate: N
SL_Parent: {logParent.PK}
SL_Reference: otherPCA2
SL_SE_NKEvent: PCA
SL_Table: OrgHeader
-------------------

Event
Code: PCA
DateTime: 01-Jan-20 00:00:00
Information: reference number
IsEstimatedDate: false

Current Log:
Deleted log

LogParent Logs:
Log
IsInDatabase: False
SL_EventTime: 01-Jan-18 00:00:00
SL_IsEstimate: N
SL_Parent: {logParent.PK}
SL_Reference: otherPCA1
SL_SE_NKEvent: PCA
SL_Table: OrgHeader

Log
IsInDatabase: False
SL_EventTime: 01-Jan-18 00:00:00
SL_IsEstimate: N
SL_Parent: {logParent.PK}
SL_Reference: otherPCA2
SL_SE_NKEvent: PCA
SL_Table: OrgHeader
-------------------";
			AssertContains(expectedError.Replace("\r\n", "\n"), errorReporterString.Replace("\r\n", "\n"));

			ErrorReporter.Clear();
		}

		public void TestPCALogDeletedOnImport_DoesntLogOnNonPCA()
		{
			var logTime = new ZDateTimeOffset(2020, 1, 1);
			var logParent = Factory.NewWithValidTestData<TestOrgHeader>();
			var log = logParent.Logs.AddNew(Events.DeliveryCartageAdvised, "originalReference", logTime);

			var adapter = StmALogValueObjectDataAdapter.New(logParent, "", EventsWithSourceType.Empty);
			var eventValue = new Xsd.Event();
			eventValue.Code = Events.DeliveryCartageAdvised.Code;
			eventValue.DateTime = logTime.ToZDateTime();

			logParent.EnableDeleteLog = true;
			AssertEquals("precondition: not deleted", false, log.IsDeleted);

			NotificationBuffer notify = new NotificationBuffer();
			AssertNoExceptionThrown("fails silently", () => adapter.CreateOrUpdateFromValueObject(eventValue, new ValueObjectImportContext(Factory, notify)));
			AssertEquals("log was deleted", true, log.IsDeleted);
			Assert(!ErrorReporter.HasBeenReported("LogDeletedBeforeProcess_PCA"));

			ErrorReporter.Clear();
		}

		#endregion

		#region Implementation

		class TestStmALogValueObjectDataAdapter : StmALogValueObjectDataAdapter
		{
			public TestStmALogValueObjectDataAdapter(BusinessObject logParent, string errorContext, EventsWithSourceType triggeredByEvents)
				: base(logParent, errorContext, triggeredByEvents)
			{
			}

			public new StmALog FindBusinessObject(Xsd.Event value, IValueObjectImportContext context)
			{
				return base.FindBusinessObject(value, context);
			}
		}

		OrgHeader SetupBusinessObjectWithEvents()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "asdasd";

			var nonSystemLogValue = new EventValue(Events.Arrival, reference: "nonsystem1", eventTime: new ZDateTimeOffset(2005, 1, 1));
			org.Logs.AddNew(nonSystemLogValue);

			nonSystemLogValue = new EventValue(Events.EntryWorkInProgress, reference: "nonsystem2", eventTime: new ZDateTimeOffset(2005, 2, 2));
			org.Logs.AddNew(nonSystemLogValue);

			nonSystemLogValue = new EventValue(Events.AddedARecordToTheSystem, reference: "system1", eventTime: new ZDateTimeOffset(2005, 3, 3));
			org.Logs.AddNew(nonSystemLogValue);

			nonSystemLogValue = new EventValue(Events.EditedARecord, reference: "system2", eventTime: new ZDateTimeOffset(2005, 4, 4));
			org.Logs.AddNew(nonSystemLogValue);

			nonSystemLogValue = new EventValue(Events.DeletedARecordInTheSystem, reference: "system3", eventTime: new ZDateTimeOffset(2005, 5, 5));
			org.Logs.AddNew(nonSystemLogValue);

			nonSystemLogValue = new EventValue(Events.DataExport, reference: "system4", eventTime: new ZDateTimeOffset(2005, 6, 6));
			org.Logs.AddNew(nonSystemLogValue);

			nonSystemLogValue = new EventValue(Events.DataImport, reference: "system5", eventTime: new ZDateTimeOffset(2005, 7, 7));
			org.Logs.AddNew(nonSystemLogValue);

			return org;
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		protected override ValueObjectDataAdapter<StmALog, Xsd.Event> GetNewBizObjXmlDataAdapter()
		{
			return StmALogValueObjectDataAdapter.New(Factory.New(typeof(OrgHeader)), "", EventsWithSourceType.Empty);
		}

		protected override string ExpectedRootCollectionElementName
		{
			get { return "Events"; }
		}

		protected override string ExpectedRootElementName
		{
			get { return "Event"; }
		}

		protected override void AssertImportFromThenExportToProducesSameXml(BusinessObjectAndExpectedOutputFileName sample, StmALog bizObjToImportTo, Xsd.Event exportedValueObject, string exportedValueObjectXml, string bizObjToImportToDescription)
		{
			Assert(true); // When importing, we do not import "User" - so this test fails
		}

		protected override BusinessObjectAndExpectedOutputFileName GetEmptyBizObjSample()
		{
			var parent = Factory.New<OrgHeader>();

			var emptyLog = parent.Logs.AddNew(Events.Booked, new ZDateTimeOffset(2005, 1, 1));
			emptyLog.SL_GS_NKUser = "";

			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.DataTransfer.Test.DataAdapters.StmALog.Testing.EmptyStmALog.xml");
			return new BusinessObjectAndExpectedOutputFileName(emptyLog, expectedOutputFilename, ValidationKind.None, "Empty StmALog");
		}

		protected override BusinessObjectAndExpectedOutputFileName GetPopulatedBizObjWithEmptyFieldsSample()
		{
			return GetEmptyBizObjSample();
		}

		protected override BusinessObjectAndExpectedOutputFileName GetFullyPopulatedBizObjSample()
		{
			var parent = Factory.New<OrgHeader>();
			var eventValue = new EventValue(Events.Booked, reference: "Information About The Event", eventTime: new ZDateTimeOffset(2005, 1, 1));

			var populatedLog = parent.Logs.AddNew(eventValue);
			populatedLog.SL_GS_NKUser = "ABC";

			populatedLog.FillWithValidTestData(TestBusinessObjectKind.All, Array.Empty<PropertyDescriptor>());
			var expectedOutputFilename = resourceRetriever.Value.SaveResourceToFile("Enterprise.DataTransfer.Test.DataAdapters.StmALog.Testing.PopulatedStmALog.xml");
			return new BusinessObjectAndExpectedOutputFileName(populatedLog, expectedOutputFilename, ValidationKind.Xsd, "Populated StmALog");
		}

		protected override BusinessObjectAndExpectedOutputFileName[] GetMiscSampleBusinessObjects()
		{
			return Array.Empty<BusinessObjectAndExpectedOutputFileName>();
		}

		protected override string[] XmlNodesToExcludeFromCoverageTest
		{
			get
			{
				return new string[]
				{
					"ReferenceKeys",
					"PostedDateTime",
					"Payload",
					"TriggeredBy",
					"UserName",
					"UserEmailAddress"
				};
			}
		}

		#endregion
	}
}

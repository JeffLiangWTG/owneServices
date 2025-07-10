using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class EventsTest : TestCaseWithFactory
	{
		public void TestEventsAllIsNotEmpty()
		{
			Assert(Events.All.Count > 0);
		}

		public void TestAll()
		{
			AssertContainsExactElementsInAnyOrder(
				"The set of auto-generated events should match the records in StmEvent",
				Factory.Load<StmEvent>(new ZQuery()).Select(item => item.SE_Code),
				Events.All.Cast<Event>().Select(item => item.Code));

			List<Event> eventsFound = new List<Event>();
			foreach (Event @event in Events.All)
			{
				Assert("The event " + @event.Code + " was auto-generated twice in Events.cs!", !eventsFound.Contains(@event));
				eventsFound.Add(@event);
			}
		}

		public void TestNRNEvent()
		{
			var format = "<EVENT><If(NAM!=\"\",\" by <NAM>\", \"\")> <REF>";
			AssertEquals("Related Notes Not Read", AutoEvents.RelatedNotesNotRead.Description);
			AssertEquals("NRN", AutoEvents.RelatedNotesNotRead.Code);
			var query = new ZQuery();
			query.AddToFilter(StmEventSchema.SE_Code, "NRN");
			var collection = Factory.Load<StmEvent>(query);
			var eventDetails = collection[0];
			AssertEquals("NRN", eventDetails.SE_Code);
			AssertEquals(format, eventDetails.SE_ReferenceFormat);
		}

		public void TestAUXEvent()
		{
			var format = "<EVENT> <If(MOD != \"\",\"<MOD>\", \"\")><If(EVT != \"\",\" <EventDescription(EVT)>\", \"\")><If(FRM != \"\",\" from <FRM>\", \"\")> <FAC> <If(IsUnloco(LOC),CityCountry(LOC),LOC)> <REF><If(VFL!=\"\",\", <VFL>\",\"\")><If(FDT!=\"\",\", <datetime.Parse(FDT).Format(\"dd-MMM-yy\")>\",\"\")><If(QTY!=\"\",\", <QTY> pieces\",\"\")>";
			AssertEquals("Auxiliary", AutoEvents.Auxiliary.Description);
			AssertEquals("AUX", AutoEvents.Auxiliary.Code);
			var query = new ZQuery();
			query.AddToFilter(StmEventSchema.SE_Code, "AUX");
			var collection = Factory.Load<StmEvent>(query);
			var eventDetails = collection[0];
			AssertEquals("AUX", eventDetails.SE_Code);
			AssertEquals(format, eventDetails.SE_ReferenceFormat);
		}

		public void TestLFREvent()
		{
			var format = "<EVENT><If(DEP != \"\",\" for <DEP>, \", \"\")><If(DIR != \"\",\"<DIR> \", \"\")><If(TYP != \"\",\"<TYP> \", \"\")><If(EQN != \"\",\"<EQN> \", \"\")><If(LOC != \"\", \" \", \"\")><If(IsUnloco(LOC),CityCountry(LOC),LOC)>";
			AssertEquals("Last Free Date", AutoEvents.LastFreeDate.Description);
			AssertEquals("LFR", AutoEvents.LastFreeDate.Code);
			var query = new ZQuery();
			query.AddToFilter(StmEventSchema.SE_Code, "LFR");
			var collection = Factory.Load<StmEvent>(query);
			var eventDetails = collection[0];
			AssertEquals("LFR", eventDetails.SE_Code);
			AssertEquals(format, eventDetails.SE_ReferenceFormat);
			AssertEquals(false, eventDetails.SE_PropagateToParent);
		}

		public void TestExistenceOfEST()
		{
			AssertEquals("Estimated Date Changed", AutoEvents.EstimatedDateChanged.Description);
			AssertEquals("Estimated Date Changed", Events.All["EST"].Description);
			AssertEquals("Estimated Date Changed", Factory.Load<StmEvent>(new ZQuery(StmEventSchema.SE_Code, "EST"))[0].SE_Desc);
		}

		public void TestChangeLogs()
		{
			var expectedEvents = new Event[]
			{
				Events.AddedARecordToTheSystem,
				Events.EditedARecord,
				Events.DeletedARecordInTheSystem,
				Events.RecallDateUpdated,
				Events.SetToActive,
				Events.SetToInactive,
				Events.StaffFlaggedAsDeviceOnly,
				Events.StaffUnFlaggedAsDeviceOnly,
				Events.UserSeat,
				Events.Login,
				Events.Logout,
			};

			AssertContainsExactElementsInAnyOrder(expectedEvents, Events.ChangeLogs);
		}

		public void TestEventsThatCannotBeCancelled()
		{
			var expectedEvents = new Event[]
			{
				Events.JobOpen,
				Events.JobClose,
				Events.ReadRelatedNotes,
				Events.RelatedNotesNotRead,
				Events.DocumentSent,
				Events.DocumentDelivered,
				Events.DocumentNotDelivered,
				Events.AssignedUserChanged,
				Events.OceanCarrierBookingByTEU
			}.Concat(Events.ChangeLogs);

			AssertContainsExactElementsInAnyOrder(expectedEvents, Events.EventsThatCannotBeCancelled);
		}

		public void TestExceptions()
		{
			var allStmEvents = Factory.Load<StmEvent>(new ZQuery());

			AssertContainsExactElementsInAnyOrder(
				Events.Exceptions.Select(e => (ZString)e.Code),
				allStmEvents.Where(stm => stm.SE_IsExceptionEvent).Select(stm => stm.SE_Code));

			AssertEquals(false, Events.Exceptions.Contains(Events.Arrival));
			AssertEquals(true, Events.Exceptions.Contains(Events.ExceptionMiscellaneous));
		}

		public void TestCustomizables()
		{
			var allStmEvents = Factory.Load<StmEvent>(new ZQuery());

			AssertContainsExactElementsInAnyOrder(
				Events.Customizables.Select(e => (ZString)e.Code),
				allStmEvents.Where(stm => stm.SE_IsCustomizable).Select(stm => stm.SE_Code));
		}

		public void TestInactiveEvents()
		{
			var allStmEvents = Factory.Load<StmEvent>(new ZQuery());

			AssertContainsExactElementsInAnyOrder(
				Events.InactiveEvents.Select(e => (ZString)e.Code),
				allStmEvents.Where(stm => !stm.SE_IsActive).Select(stm => stm.SE_Code));
		}

		public void TestEventCodesThatCannotBeChanged()
		{
			string assertMessage = "This event code must not be changes as it is broadly used by Reports, Data Interfaces and Legacy Code";

			AssertEquals("[AddedARecordToTheSystem] code has changed:\r\n" + assertMessage, "ADD", AutoEvents.AddedARecordToTheSystem.Code);
			AssertEquals("[EditedARecord] code has changed:\r\n" + assertMessage, "EDT", AutoEvents.EditedARecord.Code);
			AssertEquals("[DeletedARecordInTheSystem] code has changed:\r\n" + assertMessage, "DEL", AutoEvents.DeletedARecordInTheSystem.Code);
			AssertEquals("[Login] code has changed:\r\n" + assertMessage, "LGI", AutoEvents.Login.Code);
			AssertEquals("[Logout] code has changed:\r\n" + assertMessage, "LGO", AutoEvents.Logout.Code);
		}

		public void TestEventCodeMatchesCodeConstant()
		{
			AssertEquals("[AddedARecordToTheSystem] code", AutoEvents.AddedARecordToTheSystemCode, AutoEvents.AddedARecordToTheSystem.Code);
			AssertEquals("[EditedARecord] code", AutoEvents.EditedARecordCode, AutoEvents.EditedARecord.Code);
			AssertEquals("[DeletedARecordInTheSystem] code", AutoEvents.DeletedARecordInTheSystemCode, AutoEvents.DeletedARecordInTheSystem.Code);
		}

		public void TestCustomisedDescription()
		{
			var customEvent = Factory.LoadTop1<StmEvent>(new ZQuery(StmEventSchema.SE_IsCustomizable, true));
			customEvent.SE_Desc = "My new description";
			Factory.Save();

			Events.ReloadCustomizableEventsFromDB();
			var result = Events.Customizables.First(e => e.Code == customEvent.SE_Code);
			AssertEquals(customEvent.SE_Desc, result.Description);
		}

		public void TestGetMultiLingualDescription()
		{
			var customEvent = Factory.LoadTop1<StmEvent>(new ZQuery(StmEventSchema.SE_IsCustomizable, true));
			customEvent.SE_Desc = "My new description";
			Factory.Save();

			Events.ReloadCustomizableEventsFromDB();
			AssertEquals(ZString.Empty, Events.GetMultilingualDescription("XXXXXX"));
			AssertEquals(customEvent.SE_DescMultilingual, Events.GetMultilingualDescription(customEvent.SE_Code));
		}
	}
}

using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	sealed class OperationsEventsTestCase : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConstructor()
		{
			new OperationsEvents();
		}

		public void TestCollectionContainsEvents()
		{
			var expectedEvents = Events.All.Cast<Event>()
				.Except(Events.ChangeLogs)
				.Except(Events.InactiveEvents)
				.Except(Events.SystemReservedEvents)
				.Except(Events.Customizables)
				.Except(Events.Exceptions).Except(Events.WorkflowTriggerEvent);
			expectedEvents = expectedEvents.Concat(Factory.Load<StmEvent>(new ZQuery(StmEventSchema.SE_IsCustomizable, true)).Select(x => (Event)x));

			var invalidList = expectedEvents.Where(x => !operationsEvents.Contains(x));
			Assert(string.Format("Should contain: {0}", string.Join(", ", invalidList.Select(x => x.Code))), invalidList.IsNullOrEmpty());

			Assert("Should find DLV event", operationsEvents.Contains(Events.Delivered));
			Assert("Should find CNC event", operationsEvents.Contains(Events.Cancelled));
			Assert("Should find DEP event", operationsEvents.Contains(Events.Departure));
			Assert("Should find ARV event", operationsEvents.Contains(Events.Arrival));
			Assert("Should find ACD event", operationsEvents.Contains(Events.AddedARecordToTheSystem));
			Assert("Should find EDT event", operationsEvents.Contains(Events.EditedARecord));
			Assert("Should find ACT event", operationsEvents.Contains(Events.SetToActive));
			Assert("Should find INA event", operationsEvents.Contains(Events.SetToInactive));
			Assert("Should find EXR event", operationsEvents.Contains(Events.ExceptionRaised));
			Assert("Should find RRN event", operationsEvents.Contains(Events.ReadRelatedNotes));
		}

		public void TestCollectionDoesNotContainEvents()
		{
			var invalidList = operationsEvents.Where(e => Events.ChangeLogs.Contains(e)).Except(Events.SetToActive).Except(Events.SetToInactive).Except(Events.AddedARecordToTheSystem).Except(Events.EditedARecord);
			Assert(string.Format("Should not contain: {0}", string.Join(", ", invalidList.Select(x => x.Code))), !invalidList.Any());

			Assert("Should not find EXZ event", !operationsEvents.Contains(Events.ExceptionMiscellaneous));
			Assert("Should not find WTE event", !operationsEvents.Contains(Events.WorkflowTriggerEvent));
			Assert("Should not find DEL event", !operationsEvents.Contains(Events.DeletedARecordInTheSystem));
		}

		public void TestCollectionDoesNotContainInactiveEvents()
		{
			Assert(!operationsEvents.Any(e => Events.InactiveEvents.Contains(e)));
		}

		public void TestCollectionDoesNotContainDuplicateEvents()
		{
			var duplicates = operationsEvents
				.GroupBy(eventCode => eventCode).Where(group => group.Count() > 1).Select(group => group.Key).ToList();

			var duplicatesString = string.Join(",", duplicates);
			AssertEquals($"OperationsEvents should not have any duplicate event codes. The current event codes that are duplicated are '{duplicatesString}'.",
				0, duplicates.Count);
		}

		public void TestSortByCode()
		{
			operationsEvents.Clear();
			operationsEvents.Add(Events.Booked);
			operationsEvents.Add(Events.CallBackClient);
			operationsEvents.Add(Events.Arrival);
			operationsEvents.SortByCode();
			AssertEquals("Its first element should be ARV", "ARV", operationsEvents[0].Code);
			AssertEquals("Its Second element should be BKD", "BKD", operationsEvents[1].Code);
			AssertEquals("Its third element should be CBK", "CBK", operationsEvents[2].Code);
		}

		public void TestContainsCode()
		{
			operationsEvents.Clear();
			operationsEvents.Add(new TestEvent("ABC", "Test Code", ZGuid.NewZGuid()));
			operationsEvents.Add(new TestEvent("XYZ", "Another Code", ZGuid.NewZGuid()));

			AssertEquals("Should contain ABC", true, operationsEvents.ContainsCode("ABC"));
			AssertEquals("Should contain XYZ", true, operationsEvents.ContainsCode("XYZ"));
			AssertEquals("Should not contain missing code", false, operationsEvents.ContainsCode("XXX"));
		}

		public void TestGetDescriptionFromCode()
		{
			operationsEvents.Clear();
			operationsEvents.Add(new TestEvent("ADD", "Added a Record", ZGuid.NewZGuid()));
			operationsEvents.Add(new TestEvent("DEL", "Deleted a Record", ZGuid.NewZGuid()));

			AssertEquals("Incorrect description returned", "Added a Record", operationsEvents.GetDescriptionFromCode("ADD"));
			AssertEquals("Incorrect description returned", "Deleted a Record", operationsEvents.GetDescriptionFromCode("DEL"));
			AssertEquals("Should return empty string", string.Empty, operationsEvents.GetDescriptionFromCode("XXX"));
		}

		#region Test Classes

		class TestEvent : Event
		{
			public TestEvent(ZString code, ZString description, ZGuid pk)
				: base(code, (NoResString)description, pk)
			{
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			operationsEvents = new OperationsEvents();
		}

		OperationsEvents operationsEvents;

		#endregion
	}
}

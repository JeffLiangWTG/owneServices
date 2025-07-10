using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class EventCollectionTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestConstructor()
		{
			new EventCollection();
		}

		public void TestAdd()
		{
			AssertEquals("Precondition (Events.Count)", 0, EventCollection.Count);

			EventCollection.Add(Events.AvailableTo);
			AssertEquals("Events.Count", 1, EventCollection.Count);
			AssertEquals("Events[Code]", Events.AvailableTo, EventCollection[Events.AvailableTo.Code]);

			EventCollection.Add(Events.AvailableTo); // ensure add twice works
			AssertEquals("Events.Count", 1, EventCollection.Count);
			AssertEquals("Events[Code]", Events.AvailableTo, EventCollection[Events.AvailableTo.Code]);
		}

		public void TestAddRange()
		{
			AssertEquals("Precondition (Events.Count)", 0, EventCollection.Count);

			EventCollection.AddRange(Events.AvailableTo);
			AssertEquals("Events.Count", 1, EventCollection.Count);
			AssertEquals("Events[Code]", Events.AvailableTo, EventCollection[Events.AvailableTo.Code]);

			EventCollection.AddRange(Events.AvailableTo); // ensure add twice works
			AssertEquals("Events.Count", 1, EventCollection.Count);
			AssertEquals("Events[Code]", Events.AvailableTo, EventCollection[Events.AvailableTo.Code]);

			EventCollection.AddRange(Events.Arrival, Events.ContainerPack);
			AssertEquals("Events.Count", 3, EventCollection.Count);
			AssertEquals("Events[Code]", Events.Arrival, EventCollection[Events.Arrival.Code]);
			AssertEquals("Events[Code]", Events.ContainerPack, EventCollection[Events.ContainerPack.Code]);
		}

		public void TestRemove()
		{
			AssertEquals("Precondition (Events.Count)", 0, EventCollection.Count);

			EventCollection.Remove(Events.Arrival);
			AssertEquals("Precondition (Events.Count)", 0, EventCollection.Count);

			EventCollection.Add(Events.Arrival);
			Assert("EventCollection.Contains(Events.Arrival)", EventCollection.Contains(Events.Arrival));

			EventCollection.Remove(Events.Arrival);
			Assert("EventCollection.Contains(Events.Arrival)", !EventCollection.Contains(Events.Arrival));
		}

		public void TestContainsByCode()
		{
			Assert("Precondition (EventCollection.Contains(Events.Arrival))", !EventCollection.Contains(Events.Arrival));

			EventCollection.Add(Events.Arrival);
			Assert("EventCollection.Contains(Events.Arrival)", EventCollection.Contains(Events.Arrival.Code));
		}

		public void TestContainsByEvent()
		{
			Assert("Precondition (EventCollection.Contains(Events.Arrival))", !EventCollection.Contains(Events.Arrival));

			EventCollection.Add(Events.Arrival);
			Assert("EventCollection.Contains(Events.Arrival)", EventCollection.Contains(Events.Arrival));
		}

		public void TestCount()
		{
			AssertEquals("Precondition (Events.Count)", 0, EventCollection.Count);

			EventCollection.AddRange(Events.Arrival, Events.Delivered);
			AssertEquals("Precondition (Events.Count)", 2, EventCollection.Count);
		}

		public void TestIndexer()
		{
			EventCollection.Add(Events.Arrival);
			AssertEquals(Events.Arrival, EventCollection[Events.Arrival.Code]);
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			EventCollection = new EventCollection();
		}

		EventCollection EventCollection;

		#endregion
	}
}

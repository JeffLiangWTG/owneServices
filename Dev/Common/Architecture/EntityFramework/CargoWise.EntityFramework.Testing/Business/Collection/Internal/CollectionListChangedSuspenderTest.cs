using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class CollectionListChangedSuspenderTest : TestCaseWithFactory
	{
		public void TestShouldFireAllListChangedEventsAfterRunDelayListChangedEvents_IndexOutOfRangeException()
		{
			var handler1Result = 0;
			var handler2Result = 0;
			var handler3Result = 0;
			var handler4Result = 0;

			using (CollectionListChangedSuspender.GetInstance(Factory).DelayListChangedEvents())
			{
				ListChangedEventHandler handler1 = (s, e) => handler1Result++;
				ListChangedEventHandler handler3 = (s, e) => handler3Result++;
				ListChangedEventHandler handler4 = (s, e) => handler4Result++;

				ListChangedEventHandler handler2 = (s, e) =>
				{
					if (e.ListChangedType != ListChangedType.Reset && e.NewIndex != -1)
					{
						throw new IndexOutOfRangeException("Just for test");
					}

					handler2Result++;
				};

				var list = new List<object>();
				RunDelayableListChanged(handler1, list, new ListChangedEventArgs(ListChangedType.ItemChanged, 0));
				RunDelayableListChanged(handler2, list, new ListChangedEventArgs(ListChangedType.ItemChanged, 0));
				RunDelayableListChanged(handler3, list, new ListChangedEventArgs(ListChangedType.ItemMoved, -1));
				RunDelayableListChanged(handler4, list, new ListChangedEventArgs(ListChangedType.ItemMoved, -1));
			}

			AssertEquals("Should Invoke handler1 once.", 1, handler1Result);
			AssertEquals("Should Invoke handler2 once.", 1, handler2Result);
			AssertEquals("Should Invoke handler3 once.", 1, handler3Result);
			AssertEquals("Should Invoke handler4 once.", 1, handler4Result);
		}

		[ExpectNoExceptions]
		public void TestGetInstanceMultithreadingException()
		{
			for (int i = 0; i < 100; ++i) //100 iterations gives it about a 20% chance of failing without the lock.
			{
				var factory = new BusinessObjectFactory();
				CollectionListChangedSuspender suspender = null;
				CollectionListChangedSuspender otherSuspender = null;
				var thread = new Thread(() => { if (i % 4 == 0) { Thread.Sleep(i % 2); } otherSuspender = CollectionListChangedSuspender.GetInstance(Factory); });
				thread.Start();
				if (i % 16 < 8)
				{ Thread.Sleep(i % 8 < 4 ? 0 : 1); }
				suspender = CollectionListChangedSuspender.GetInstance(Factory);
				thread.Join();
				AssertEquals(false, thread.IsAlive);
				AssertEquals(suspender, otherSuspender);
				AssertNotEquals(null, suspender);
			}
		}

		public void TestRunDelayableListChanged()
		{
			ListChangedEventArgs firedWithEvent = null;
			using (CollectionListChangedSuspender.GetInstance(Factory).DelayListChangedEvents())
			{
				ListChangedEventHandler handler = delegate(object sender, ListChangedEventArgs e)
				{ firedWithEvent = e; };
				List<object> list = new List<object>();
				RunDelayableListChanged(handler, list, new ListChangedEventArgs(ListChangedType.ItemAdded, 0));
				AssertEquals("Event not fired while suspended", null, firedWithEvent);
			}
			AssertEquals("Event fired after resume", ListChangedType.ItemAdded, firedWithEvent.ListChangedType);
		}

		public void TestRunDelayableListChanged_WithMultipleEvents()
		{
			ListChangedEventArgs firedWithEvent = null;
			using (CollectionListChangedSuspender.GetInstance(Factory).DelayListChangedEvents())
			{
				ListChangedEventHandler handler = delegate(object sender, ListChangedEventArgs e)
				{ firedWithEvent = e; };
				List<object> list = new List<object>();
				RunDelayableListChanged(handler, list, new ListChangedEventArgs(ListChangedType.ItemAdded, 0));
				RunDelayableListChanged(handler, list, new ListChangedEventArgs(ListChangedType.ItemAdded, 1));
				RunDelayableListChanged(handler, list, new ListChangedEventArgs(ListChangedType.ItemDeleted, 0));
				AssertEquals("Event not fired while suspended", null, firedWithEvent);
			}
			AssertEquals("Event fired with Reset if multiple events fired", ListChangedType.Reset, firedWithEvent.ListChangedType);
		}

		public void TestRunDelayableListChanged_WithMultipleAdds()
		{
			ListChangedEventArgs firedWithEvent = null;
			using (CollectionListChangedSuspender.GetInstance(Factory).DelayListChangedEvents())
			{
				ListChangedEventHandler handler = delegate(object sender, ListChangedEventArgs e)
				{ firedWithEvent = e; };
				List<object> list = new List<object>();
				list.Add(new object());
				RunDelayableListChanged(handler, list, new ListChangedEventArgs(ListChangedType.ItemAdded, 0));
				list.Add(new object());
				RunDelayableListChanged(handler, list, new ListChangedEventArgs(ListChangedType.ItemAdded, 1));
				AssertEquals("Event not fired while suspended", null, firedWithEvent);
			}
			AssertEquals("Event fired with Reset if multiple events fired", ListChangedType.ItemAdded, firedWithEvent.ListChangedType);
		}

		public void TestRunDelayableListChanged_WithMultipleDeletes()
		{
			ListChangedEventArgs firedWithEvent = null;
			ListChangedEventArgs firedWithEvent2 = null;
			List<object> list = new List<object>();

			ListChangedEventHandler handler = delegate(object sender, ListChangedEventArgs e)
			{ firedWithEvent = e; };
			ListChangedEventHandler handler2 = delegate(object sender, ListChangedEventArgs e)
			{ firedWithEvent2 = e; };
			using (CollectionListChangedSuspender.GetInstance(Factory).DelayListChangedEvents())
			{
				RunDelayableListChanged(handler, list, new ListChangedEventArgs(ListChangedType.ItemDeleted, 2));
				RunDelayableListChanged(handler2, list, new ListChangedEventArgs(ListChangedType.ItemDeleted, 2));
				RunDelayableListChanged(handler, list, new ListChangedEventArgs(ListChangedType.ItemDeleted, 1));
				RunDelayableListChanged(handler2, list, new ListChangedEventArgs(ListChangedType.ItemDeleted, 1));
			}
			AssertEquals("Delete event fired if done in the correct order", ListChangedType.ItemDeleted, firedWithEvent.ListChangedType);
			AssertEquals("Delete event fired if done in the correct order", ListChangedType.ItemDeleted, firedWithEvent2.ListChangedType);

			using (CollectionListChangedSuspender.GetInstance(Factory).DelayListChangedEvents())
			{
				RunDelayableListChanged(handler, list, new ListChangedEventArgs(ListChangedType.ItemDeleted, 1));
				RunDelayableListChanged(handler2, list, new ListChangedEventArgs(ListChangedType.ItemDeleted, 1));
				RunDelayableListChanged(handler, list, new ListChangedEventArgs(ListChangedType.ItemDeleted, 2));
				RunDelayableListChanged(handler2, list, new ListChangedEventArgs(ListChangedType.ItemDeleted, 2));
			}
			AssertEquals("Event escalated if Deletes done in an incompatible order", ListChangedType.Reset, firedWithEvent.ListChangedType);
			AssertEquals("Event escalated if Deletes done in an incompatible order", ListChangedType.Reset, firedWithEvent2.ListChangedType);
		}

		[ExpectNoExceptions]
		public void TestSuppressIndexOutOfRangeException()
		{
			List<object> list = new List<object>();

			bool reset = false;

			ListChangedEventHandler handler =
				delegate(object sender, ListChangedEventArgs e)
				{
					if (e.ListChangedType == ListChangedType.Reset)
					{
						reset = true;
					}
					else if (e.NewIndex >= ((List<object>)sender).Count)
					{
						throw new IndexOutOfRangeException();
					}
					else
					{
						((List<object>)sender).RemoveAt(e.NewIndex); // Due to some logic, e.g. active filter
					}
				};

			using (CollectionListChangedSuspender.GetInstance(Factory).DelayListChangedEvents())
			{
				list.Add(new object());
				RunDelayableListChanged(handler, list, new ListChangedEventArgs(ListChangedType.ItemAdded, 0));
				list.Add(new object());
				RunDelayableListChanged(handler, list, new ListChangedEventArgs(ListChangedType.ItemAdded, 1));
			}

			Assert("List reset event shoud be fired", reset);
		}

		public void TestRunDelayableListChanged_DelayListChangedDelayerIsNull()
		{
			var collection = CollectionListChangedSuspender.GetInstance(Factory);
			collection.MakeListNullForTest = true;

			var action = collection.DelayListChangedEvents();
			AssertEquals(1, collection.DelayListChangedEventsIndex);

			action.Dispose();
			AssertEquals(0, collection.DelayListChangedEventsIndex);
		}

		void RunDelayableListChanged(ListChangedEventHandler handler, IList list, ListChangedEventArgs e)
		{
			CollectionListChangedSuspender.GetInstance(Factory).RunDelayableListChanged(handler, list, e);
		}
	}
}

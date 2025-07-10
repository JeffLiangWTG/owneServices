using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using static CargoWise.EntityFramework.SubscriptionCollection;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SubscriptionCollectionTest : TestCaseWithFactory
	{
		[NUnit.Framework.DeveloperOnlyTest] //because I don't know how to make it deterministic - but it does seem to get faster after my fix
		public void TestPerformanceWithManyFactoriesAndSubscriptions()
		{
			var sw = new Stopwatch();
			sw.Start();
			var factories = new List<BusinessObjectFactory>();
			const int exponent = 100;
			for (var i = 0; i < exponent; ++i)
			{
				var factory = new BusinessObjectFactory() { RefreshEnabled = true };
				factories.Add(factory);
				for (var j = 0; j < exponent; ++j)
				{
					factory.New<DummyBusinessObject>();
				}
			}
			sw.Stop();
			AssertEquals(exponent * exponent, factories.Select(x => ((IBusinessObjectFactoryInternals)x).NumberOfBusinessObjects).Aggregate((x, y) => x + y));
			AssertLessThan(sw.ElapsedMilliseconds, 1500);
		}

		public void TestContainsParticipantsWithDuplicateKeysInRedBlackTree()
		{
			var a = new DummyObject(1);
			var b = new DummyObject(1);
			var c = new DummyObject(1);
			var d = new DummyObject(1);
			AssertEquals(a.GetHashCode(), b.GetHashCode());
			AssertEquals(a.GetHashCode(), c.GetHashCode());
			AssertEquals(a.GetHashCode(), d.GetHashCode());

			AssertNotEquals(a, b);
			AssertNotEquals(a, c);
			AssertNotEquals(a, d);

			var set = new SubscriptionSet();
			set.Add("A", new TestSubscription(Factory, a));
			set.Add("A", new TestSubscription(Factory, b));
			set.Add("A", new TestSubscription(Factory, c));

			AssertEquals(true, set.ContainsParticipant("A", a));
			AssertEquals(true, set.ContainsParticipant("A", b));
			AssertEquals(true, set.ContainsParticipant("A", c));
			AssertEquals(false, set.ContainsParticipant("A", d));
		}

		void AssertContainsItemPerformance(Func<SubscriptionSet, string, TestSubscription, bool> containsMethodToTest)
		{
			var set = new SubscriptionSet();

			var random = new Random();
			var firstSubscription = new TestSubscription(Factory, new DummyObject(random.Next()));
			set.Add("A", firstSubscription);

			CombineAssertions(() =>
			{
				var sw = new Stopwatch();
				for (var i = 0; i < 50_000; i++)
				{
					set.Add("A", new TestSubscription(Factory, new DummyObject(random.Next())));
				}
				AssertLessThan("Insertion should be quick.", sw.ElapsedMilliseconds, 200);

				var contains = false;
				sw.Restart();
				for (var i = 0; i < 50_000; i++)
				{
					contains = containsMethodToTest(set, "A", firstSubscription);
				}

				AssertLessThan("Should be able to find first item quickly.", sw.ElapsedMilliseconds, 200);
				AssertEquals(true, contains);

				var lastSubscription = new TestSubscription(Factory, new DummyObject(random.Next()));
				set.Add("A", lastSubscription);

				sw.Restart();
				for (var i = 0; i < 50_000; i++)
				{
					contains = containsMethodToTest(set, "A", lastSubscription);
				}

				AssertLessThan("Should be able to find last item quickly.", sw.ElapsedMilliseconds, 200);
				AssertEquals(true, contains);

				var otherSubscription = new TestSubscription(Factory, new DummyObject(random.Next()));
				sw.Restart();
				for (var i = 0; i < 50_000; i++)
				{
					contains = containsMethodToTest(set, "A", otherSubscription);
				}

				AssertLessThan("Should know an item is not in the list quickly.", sw.ElapsedMilliseconds, 200);
				AssertEquals(false, contains);
			});
		}

		[StressTest]
		public void TestSubscriptionSetContainsParticipantPerformance()
			=> AssertContainsItemPerformance((set, subKey, subscription) => set.ContainsParticipant(subKey, subscription.Participant));

		[StressTest]
		public void TestSubscriptionSetContainsSubscriptionPerformance()
			=> AssertContainsItemPerformance((set, subKey, subscription) => set.ContainsSubscription(subKey, subscription));

		class DummyObject
		{
			public DummyObject(int i)
			{
				this.i = i;
			}

			readonly int i;

			public override int GetHashCode()
			{
				return i.GetHashCode();
			}
		}

		public void TestCollection()
		{
			Collection.Add(new DataRefreshBus.SubscriptionSubject("Key1", ""), Subscription1_Factory1);
			Collection.Add(new DataRefreshBus.SubscriptionSubject("Key2", ""), Subscription2_Factory2);
			Collection.Add(new DataRefreshBus.SubscriptionSubject("Key1", ""), Subscription1_Factory2);
			AssertEquals(3, Collection.Count);
			AssertContains(true, new DataRefreshBus.SubscriptionSubject("Key1", ""), Subscription1_Factory1);
			AssertContains(false, new DataRefreshBus.SubscriptionSubject("Key2", ""), Subscription2_Factory1);
			AssertContains(true, new DataRefreshBus.SubscriptionSubject("Key1", ""), Subscription1_Factory2);
			AssertContains(true, new DataRefreshBus.SubscriptionSubject("Key2", ""), Subscription2_Factory2);

			Collection.Remove(new DataRefreshBus.SubscriptionSubject("Key2", ""), new[] { Subscription2_Factory2 });
			AssertEquals(2, Collection.Count);
			AssertContains(true, new DataRefreshBus.SubscriptionSubject("Key1", ""), Subscription1_Factory1);
			AssertContains(false, new DataRefreshBus.SubscriptionSubject("Key2", ""), Subscription2_Factory1);
			AssertContains(true, new DataRefreshBus.SubscriptionSubject("Key1", ""), Subscription1_Factory2);
			AssertContains(false, new DataRefreshBus.SubscriptionSubject("Key2", ""), Subscription2_Factory2);
		}

		public void TestCollectionWhenFactoryNoService()
		{
			Collection.Add(new DataRefreshBus.SubscriptionSubject("Key2", ""), Subscription2_Factory2);

			Factory2.ServiceContainer.RemoveService<SubscriptionSetService>();
			AssertNoExceptionThrown(() => Collection.Remove(new DataRefreshBus.SubscriptionSubject("Key1", ""), new[] { Subscription2_Factory2 }));
		}

		void AssertContains(bool expectContains, DataRefreshBus.SubscriptionSubject subscriptionSubject, DataRefreshBus.Subscription subscription)
		{
			AssertEquals("Contains", expectContains, Collection.Contains(subscriptionSubject, subscription));
			AssertEquals("ContainsParticipant", expectContains, Collection.ContainsParticipant(subscriptionSubject, subscription.Participant, subscription.Factory));
		}

		public void TestWeakReferencedByFactory()
		{
			(var factoryRef, var subscriptionRef) = SubscribeUnreferenced();
			GC.Collect();
			AssertEquals("Factory should be collected along with the factory", false, factoryRef.IsAlive);
			AssertEquals("Subscription should be collected along with the factory", false, subscriptionRef.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		(WeakReference factoryRef, WeakReference subscriptionRef) SubscribeUnreferenced()
		{
			var factory = new BusinessObjectFactory();
			var factoryRef = new WeakReference(factory);
			var subscription = new TestSubscription(factory, new object());
			var subscriptionRef = new WeakReference(subscription);
			Collection.Add(new DataRefreshBus.SubscriptionSubject("Key", ""), subscription);
			return (factoryRef, subscriptionRef);
		}

		public void TestConcurrencyGetSubscriptions()
		{
			var service = new SubscriptionSetService { CurrentThreadName = "MainThreadForTest" };
			var key = new SubscriptionCollection();

			ThreadPool.QueueUserWorkItem(_ =>
			{
				Thread.CurrentThread.Name = "ThisIsANewThreadThatDoesNotExistInCurrentProcess";
				while (!service.AnotherThreadGo)
				{
					Thread.Sleep(10);
				}
				service.GetSubscriptions(key).TryAdd("Key1", new SubscriptionSet());
				service.GetSubscriptions(key).TryAdd("Key2", new SubscriptionSet());
			});

			try
			{
				Thread.CurrentThread.Name = "MainThreadForTest";
			}
			catch (InvalidOperationException)
			{
				service.CurrentThreadName = Thread.CurrentThread.Name;
			}
			AssertEquals(2, service.GetSubscriptions(key).Count);
		}

		#region Test Classes

		class TestSubscription : DataRefreshBus.Subscription
		{
			public TestSubscription(BusinessObjectFactory factory, object participant)
				: base(factory, participant)
			{
			}

			internal override void DoAction(IEnumerable<BusinessObject> publishedObjects)
			{
				OnSubscriptionUpdate(false);
			}
		}

		#endregion

		#region Implementation

		SubscriptionCollection Collection
		{
			get { return collection ?? (collection = new SubscriptionCollection()); }
		}
		SubscriptionCollection collection;

		TestSubscription Subscription1_Factory1
		{
			get { return subscription1_Factory1 ?? (subscription1_Factory1 = new TestSubscription(Factory1, new object())); }
		}
		TestSubscription subscription1_Factory1;

		TestSubscription Subscription2_Factory1
		{
			get { return subscription2_Factory1 ?? (subscription2_Factory1 = new TestSubscription(Factory1, new object())); }
		}
		TestSubscription subscription2_Factory1;

		TestSubscription Subscription1_Factory2
		{
			get { return subscription1_Factory2 ?? (subscription1_Factory2 = new TestSubscription(Factory2, new object())); }
		}
		TestSubscription subscription1_Factory2;

		TestSubscription Subscription2_Factory2
		{
			get { return subscription2_Factory2 ?? (subscription2_Factory2 = new TestSubscription(Factory2, new object())); }
		}
		TestSubscription subscription2_Factory2;

		BusinessObjectFactory Factory1
		{
			get { return factory1 ?? (factory1 = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory1;

		BusinessObjectFactory Factory2
		{
			get { return factory2 ?? (factory2 = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory2;

		#endregion
	}
}

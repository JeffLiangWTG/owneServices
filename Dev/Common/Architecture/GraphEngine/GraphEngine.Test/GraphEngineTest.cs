using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.GraphEngine.Test
{
	class GraphEngineTest : TestCase
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			LazyEngine = new Lazy<DummyGraphEngine>(() => new DummyGraphEngine());
			LazyFactory = new Lazy<DummyEntityFactory>(() => new DummyEntityFactory());
		}

		Lazy<DummyGraphEngine> LazyEngine { get; set; }
		Lazy<DummyEntityFactory> LazyFactory { get; set; }
		DummyGraphEngine Engine => LazyEngine.Value;
		DummyEntityFactory Factory => LazyFactory.Value;

		const int largeNumber = 10000;

		#endregion

		#region Block

		public void TestBlock_SingleKey()
		{
			Engine.Dummies.AddRange(Factory.New("1").New("1"));
			Engine.LoadEntities();
			AssertEquals(1, Engine.FetchUnblocked().Count());
		}

		public void TestBlock_DoubleKey()
		{
			Engine.Dummies.AddRange(Factory.New("1", "2").New("1", "2"));
			Engine.LoadEntities();
			AssertEquals(1, Engine.FetchUnblocked().Count());
		}

		public void TestBlock_DoubleKey_Order()
		{
			Engine.Dummies.AddRange(
				Factory.New("1", "2").New("1").New("2") // Chain1
				.New("A").New("B").New("A", "B"));

			Engine.LoadEntities();

			CombineAssertions("Expect that the entities that are created first are the first to appear in the Grengine",
			() =>
			{
				var unblocked = Engine.FetchUnblocked().ToArray();
				AssertEquals(3, unblocked.Length);
				AssertContains(unblocked, "1", "2");
				AssertContains(unblocked, "A");
				AssertContains(unblocked, "B");
			});
		}

		public void TestBlock_WithoutLoad()
		{
			Engine.Dummies.AddRange(Factory.New("!").New("A"));
			AssertEquals(0, Engine.FetchUnblocked().Count());
		}

		public void TestBlock_Wide()
		{
			Engine.Dummies.AddRange(Enumerable.Range(0, largeNumber)
				.Select(index => Factory.New(index.ToString())));
			Engine.LoadEntities();

			AssertEquals(largeNumber, Engine.FetchUnblocked().Count());
		}

		public void TestBlock_Deep()
		{
			var key = "m";

			Engine.Dummies.AddRange(Enumerable.Range(0, largeNumber)
				.Select(index => Factory.New(key)));
			Engine.LoadEntities();

			AssertEquals(1, Engine.FetchUnblocked().Count());
		}

		public void TestBlock_WideAndDeep()
		{
			var elementsPerKey = (int)Math.Sqrt(largeNumber);

			Engine.Dummies.AddRange(Enumerable.Range(0, largeNumber)
				.Select(index => Factory.New((index % elementsPerKey).ToString())));
			Engine.LoadEntities();

			AssertEquals(elementsPerKey, Engine.FetchUnblocked().Count());
		}

		public void TestBlock_WideAndDeepWithCollisions()
		{
			var elementsPerKey = (int)Math.Sqrt(largeNumber);
			var keysPerElement = (int)Math.Sqrt(elementsPerKey);

			Func<int, string[]> genKeys = i => Enumerable.Range(0, elementsPerKey)
			.Select(j => ((i + j) % elementsPerKey).ToString()).ToArray();

			Engine.Dummies.AddRange(Enumerable.Range(0, largeNumber)
				.Select(index => Factory.New(genKeys(index))));
			Engine.LoadEntities();

			AssertEquals("For now, there is a bug. No logic for queue jumping exists yet.", 1, Engine.FetchUnblocked().Count());
		}

		public void TestNoKeys()
		{
			Engine.Dummies.AddRange(Factory.New().New().New());
			Engine.LoadEntities();

			AssertEquals(3, Engine.FetchUnblocked().Count());
		}

		#endregion

		#region Test we can handle inconsistencies caused by chaining.

		public void TestRemoveMiddleRow()
		{
			Engine.Dummies.AddRange(Factory.New("1").New("1").New("1").New("1").New("1"));
			Engine.LoadEntities();
			Engine.FindChains().Single().Skip(2).First().Process(Engine);
			Engine.AssertNumberOfTimesBlockedDuringProcessing("We expect the engine to be blocked 4 times, even though the middle item was removed", 4);
		}

		public void TestRemoveMiddleRows()
		{
			Engine.Dummies.AddRange(Enumerable.Range(0, 15).Select(_ => Factory.New("1")));
			Engine.LoadEntities();
			foreach (var c in Engine.FindChains().Single().Skip(5).Take(5))
			{
				c.Process(Engine);
			}

			var result = Engine.LoadEntities();
			AssertEquals(5, result.OldEntitiesWithPrerequisitesMoved);
		}

		public void TestRemoveLast()
		{
			Engine.Dummies.AddRange(Factory.New("1").New("1").New("1").New("1").New("1"));
			Engine.LoadEntities();
			Engine.FindChains().Single().Last().Process(Engine);
			Engine.AssertNumberOfTimesBlockedDuringProcessing("We expect the engine to be blocked 4 times, even though the last item was removed", 4);
		}

		#endregion

		#region Test Load

		public void TestLoad()
		{
			Engine.Dummies.AddRange(Factory.New().New().New());
			AssertEquals(0, Engine.TestTracker.Loads);

			Engine.LoadEntities();
			AssertEquals(3, Engine.TestTracker.Loads);

			Engine.LoadEntities();
			AssertEquals("No new loads.", 3, Engine.TestTracker.Loads);
		}

		public void TestLoad_DuplicatesAdded()
		{
			var set = Factory.New().New().New();
			Engine.Dummies.AddRange(set);
			Engine.LoadEntities();
			AssertEquals(3, Engine.TestTracker.Loads);
			AssertEquals(3, Engine.TestTracker.VertexAdds);

			Engine.Dummies.AddRange(set);
			Engine.LoadEntities();
			AssertEquals(6, Engine.TestTracker.Loads);
			AssertEquals(3, Engine.TestTracker.VertexAdds);
		}

		[ExpectNoExceptions]
		public void TestLoad_ProcessedEntity()
		{
			var item = Factory.New();
			item.Process(Engine);
			Engine.LoadEntities();
		}

		#endregion

		#region Test Process

		public void TestProcess_Basic()
		{
			var dummy = Factory.New();
			Engine.Dummies.Add(dummy);
			Engine.LoadEntities();

			dummy.Process(Engine);
			Engine.LoadEntities();
			AssertEquals(0, Engine.FetchUnblocked().Count());
		}

		public void TestProcess_Unblock()
		{
			var dummy1 = Factory.New("!");
			var dummy2 = Factory.New("!");
			Engine.Dummies.AddRange(new[] { dummy1, dummy2 });

			Engine.AssertProcessAndUnblock(dummy1, dummy2);
		}

		public void TestProcess_Deep()
		{
			TestBlock_Deep();
			Engine.AssertProcessAll(largeNumber);
		}

		public void TestProcess_Wide()
		{
			TestBlock_Wide();
			Engine.AssertProcessAll(largeNumber);
		}

		public void TestProcess_WideAndDeep()
		{
			TestBlock_WideAndDeep();
			Engine.AssertProcessAll(largeNumber);
		}

		public void TestProcess_WideAndDeepWithCollisions()
		{
			TestBlock_WideAndDeepWithCollisions();
			Engine.AssertProcessAll(largeNumber);
		}

		#endregion

		#region Chain Identification

		public void TestChain_Sequence()
		{
			var dummies = Factory.New("1").New("1").New("1").New("1");
			Engine.AssertChains("Single", dummies);
		}

		public void TestChain_Sequences()
		{
			var chain1 = Factory.New("1").New("1");
			var chain2 = Factory.New("2").New("2");
			Engine.AssertChains("Two chains will be", chain1, chain2);
		}

		public void TestChain_Distinct()
		{
			var distinct = Factory.New("The Pan Man")
				.New("Sat on his Can")
				.New("Waving his Fan")
				.New("Eating his Flan")
				.New("Acting his Plan")
				.New("Using his Scan")
				.New("Biggotry.")
				.Select(n => new[] { n })
				.ToArray();

			Engine.AssertChains("Each of these unique chains should be happy", distinct);
		}

		public void TestChain_Empty()
		{
			Engine.AssertChains("When the engine is empty everything is ok.");
		}

		public void TestChain_Split()
		{
			var chain1 = Factory.New("1").New("1").New("1", "2").New("1");
			Engine.Dummies.Add(Factory.New("2", "3"));

			Engine.AssertChains("Do not take items after splits in the graph", chain1);
		}

		public void TestChain_Merge()
		{
			var chain1 = Factory.New("1").New("1");
			var chain2 = Factory.New("2").New("2");
			var extra = Factory.New("1", "2").New("1").New("1").New("2").New("2");
			Engine.Dummies.AddRange(extra);

			Engine.AssertChains("Do not take items after merge in the graph", chain1, chain2);
		}

		public void TestBigChain()
		{
			foreach (var line in TestDataProvider.LargeSetOfKeys.Split(System.Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
			{
				Engine.Dummies.Add(Factory.New(line.Split(',')));
			}

			Engine.LoadEntities();
			var unblocked = Engine.FindChains();
			AssertNotNull(unblocked.Single(c => c.Count > 1000));
		}

		#endregion

		#region Assertion Helpers

		void AssertContains(IEnumerable<DummyEntity> entities, params string[] keys)
		{
			AssertContains($"Expected entity with keys [{string.Join(", ", keys)}]", entities, keys);
		}

		void AssertContains(string message, IEnumerable<DummyEntity> entities, params string[] keys)
		{
			var set = new HashSet<string>(keys);
			Assert(message, entities.Any(e => e.Keys.All(k => set.Contains(k.ToString()))));
		}

		#endregion
	}

	static class TestExtensions
	{
		public static void AssertProcessAndUnblock(this DummyGraphEngine engine, DummyEntity entity, params DummyEntity[] unblocked)
		{
			engine.LoadEntities();
			var currentlyUnblocked = new HashSet<DummyEntity>(engine.FetchUnblocked());
			entity.Process(engine);
			engine.LoadEntities();
			var newlyUnblocked = new HashSet<DummyEntity>(engine.FetchUnblocked());
			newlyUnblocked.ExceptWith(currentlyUnblocked);

			Assertion.AssertContainsExactElementsInAnyOrder("Expected elements to be unblocked by operation.", unblocked, newlyUnblocked);
		}

		public static void AssertProcessAll(this DummyGraphEngine engine, int expectedRemoves)
		{
			DummyEntity[] entities;

			do
			{
				engine.LoadEntities();
				entities = engine.FetchUnblocked().ToArray();
				foreach (var entity in entities)
				{
					entity.Process(engine);
				}
			} while (entities.Length > 0);

			Assertion.AssertEquals(expectedRemoves, engine.TestTracker.VertexRemoves);
		}

		public static void AssertNumberOfTimesBlockedDuringProcessing(this DummyGraphEngine engine, string message, int expectedTimesBlocked)
		{
			int numberOfLoops = 0;

			while (engine.Count > 0)
			{
				numberOfLoops++;
				var entities = engine.FetchUnblocked().ToArray();
				foreach (var entity in entities)
				{
					entity.Process(engine);
				}
				engine.LoadEntities();
			}

			Assertion.AssertEquals(message, expectedTimesBlocked, numberOfLoops);
		}

		public static void AssertChains(this DummyGraphEngine engine, string message, params IEnumerable<DummyEntity>[] expectedChains)
		{
			engine.Dummies.AddRange(expectedChains.SelectMany(s => s).Distinct());
			engine.LoadEntities();

			var expected = expectedChains.Select(s => new Chain<DummyEntity>(s.Reverse().ToArray())); // Reverse is required due to cons creating reverse sequence.
			var unblocked = engine.FindChains();

			Assertion.AssertContainsExactElementsInAnyOrder(message, expected, unblocked);
		}
	}
}

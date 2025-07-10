using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace CargoWise.Common.Collections.Testing
{
	class IEnumerableExtensionsTest : TestCase
	{
		class ItemWithList
		{
			public int Number { get; set; }

			public List<ItemWithList> Children { get; } = new List<ItemWithList>();
			internal ItemWithList AddChild()
			{
				var child = new ItemWithList();
				Children.Add(child);
				return child;
			}
		}

		public void TestExcept()
		{
			var a11 = new A { I = 1 };
			var a12 = new A { I = 1 };
			var a2 = new A { I = 2 };
			var a51 = new A { I = 5 };
			var a52 = new A { I = 5 };
			var a3 = new A { I = 3 };
			var a4 = new A { I = 4 };
			var a53 = new A { I = 5 };
			var list = new List<A> { a11, a12, a2, a51, a52, a3, a4, a53, };
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Null case", list, list.Except(Enumerable.Empty<A>(), new AComparer()));
				AssertContainsExactElementsInAnyOrder("Miss a2 case.", new[] { a11, a12, a51, a52, a3, a4, a53, }, list.Except(new[] { a2 }, new AComparer()));
				AssertContainsExactElementsInAnyOrder("Miss both a1 cases.", new[] { a2, a51, a52, a3, a4, a53, }, list.Except(new[] { a11 }, new AComparer()));
				AssertContainsExactElementsInAnyOrder("Nothing", Array.Empty<A>(), list.Except(list, new AComparer()));
			});
		}

		public void TestDistinctBy_IsStable()
		{
			var set = new List<A> { new A { I = 5 }, new A { I = 3 }, new A { I = 1 }, new A { I = 4 }, new A { I = 2 }, };
			AssertArrayEqualsByElements(set.ToArray(), set.DistinctBy(s => s.I).ToArray());
			AssertArrayEqualsByElements(set.ToArray(), set.Concat(set).DistinctBy(s => s.I).ToArray());
		}

		public void TestDistinctDepthFirstHeirarchyTraversal()
		{
			var item1 = new ItemWithList();
			var item1_1 = item1.AddChild();
			var item1_2 = item1.AddChild();
			var item1_1_1 = item1_1.AddChild();
			var item1_2_1 = item1_2.AddChild();
			var items = new[] { item1 }.DistinctDepthFirstHeirarchyTraversal(yeah => yeah.Children).ToArray();
			AssertArrayEqualsByElements(new[] { item1, item1_1, item1_1_1, item1_2, item1_2_1 }, items);
		}

		public void TestContainsSameElementsInAnyOrder()
		{
			var random = new Random(32);
			var list = new[] { 0, 1, 2, 3, 4 };
			var same = list.OrderBy(_ => random.Next()).ToList();
			var different = same.Append(0).ToList();
			Assert("Has the same elements, so should return true", list.ContainsSameElementsInAnyOrder(same));
			Assert("The different collection has two '0's, and so the collections are not equal", !list.ContainsSameElementsInAnyOrder(different));
			Assert("The rule is commutative", !different.ContainsSameElementsInAnyOrder(list));
			var l1 = new[] { 0, 0, 1 };
			var l2 = new[] { 0, 1, 1 };
			Assert("The lists are not the same", !l1.ContainsSameElementsInAnyOrder(l2));
		}

		public void TestContainsSameElementsInAnyOrder_WithPredicate()
		{
			var random = new Random(32);
			var list = new[] { 0, 1, 2, 3, 4 };
			var same = list.OrderBy(_ => random.Next()).ToList();
			var different = same.Append(0).ToList();
			Func<int, int, bool> areEqual = (l, r) => l == r;
			Assert("Has the same elements, so should return true", list.ContainsSameElementsInAnyOrder(same, areEqual));
			Assert("The different collection has two '0's, and so the collections are not equal", !list.ContainsSameElementsInAnyOrder(different, areEqual));
			Assert("The rule is commutative", !different.ContainsSameElementsInAnyOrder(list, areEqual));
			var l1 = new[] { 0, 0, 1 };
			var l2 = new[] { 0, 1, 1 };
			Assert("The lists are not the same", !l1.ContainsSameElementsInAnyOrder(l2, areEqual));
		}

		public void TestIterateLinkedListNodes()
		{
			var list = new LinkedList<int>(new[] { 1, 2, 3 });
			var item = 0;
			foreach (var node in list.Nodes())
			{
				AssertEquals(++item, node.Value);
			}

			AssertEquals(3, item);
		}

		public void TestIterateLinkedListNodes_ForEmptyList()
		{
			var list = new LinkedList<int>();
			AssertEquals(0, list.Nodes().Count());
		}

		public void TestIterateLinkedListNodes_InterruptingEnumeration()
		{
			var list = new LinkedList<int>(new[] { 1, 2, 3 });
			var hitCount = 0;
			foreach (var node in list.Nodes())
			{
				list.Remove(node);
				hitCount++;
			}

			AssertEquals("Should not move past first node, since the Next property is cleared when removing node from the list", 1, hitCount);
			AssertEquals(2, list.Count);
		}

		public void TestAppend()
		{
			Assert(new[] { 1, 2, 3 }.SequenceEqual(new[] { 1, 2 }.Append(3)));
		}

		public void TestIsNullOrEmpty()
		{
			IEnumerable<int> source;
			source = null;
			Assert("Source is null", source.IsNullOrEmpty());
			source = Enumerable.Empty<int>();
			Assert("Source is empty", source.IsNullOrEmpty());
			source = new List<int>()
			{ 1 };
			Assert("Source is not empty", !source.IsNullOrEmpty());
			source = GetCollection(0);
			Assert("Source is empty", source.IsNullOrEmpty());
			source = GetCollection(2);
			Assert("Source is not empty", !source.IsNullOrEmpty());
		}

		#region Count
		public void TestIsCountEqualTo()
		{
			IEnumerable<int> source = new List<int>();
			Assert("Count is 0", source.IsCountEqualTo(0));
			Assert("Count is not 1", !source.IsCountEqualTo(1));
			Assert("Count is 0", source.IsCountEqualTo(0, number => true));
			Assert("Count is not 1", !source.IsCountEqualTo(1, number => true));
			source = new List<int>()
			{ 1, 2 };
			Assert("Source has exactly 2 elements", source.IsCountEqualTo(2));
			Assert("Source doesn't have exactly 1 element", !source.IsCountEqualTo(1));
			Assert("Source has exactly 2 elements", source.IsCountEqualTo(2, number => true));
			Assert("Source doesn't have exactly 1 element", !source.IsCountEqualTo(1, number => true));
			Assert("Source has exactly 1 even element", source.IsCountEqualTo(1, number => number % 2 == 0));
			source = GetCollection(0);
			Assert("Count is 0", source.IsCountEqualTo(0));
			Assert("Count is not 1", !source.IsCountEqualTo(1));
			Assert("Count is 0", source.IsCountEqualTo(0, number => true));
			Assert("Count is not 1", !source.IsCountEqualTo(1, number => true));
			source = GetCollection(2);
			Assert("Source has exactly 2 elements", source.IsCountEqualTo(2));
			Assert("Source doesn't have exactly 1 element", !source.IsCountEqualTo(1));
			Assert("Source has exactly 2 elements", source.IsCountEqualTo(2, number => true));
			Assert("Source doesn't have exactly 1 element", !source.IsCountEqualTo(1, number => true));
			Assert("Source has exactly 1 even element", source.IsCountEqualTo(1, number => number % 2 == 0));
		}

		public void TestIsCountLessThan()
		{
			IEnumerable<int> source;
			source = null;
			AssertExceptionThrown("Source is null", typeof(ArgumentNullException), () => source.IsCountLessThan(0));
			source = new List<int>();
			AssertExceptionThrown("Predicate is null", typeof(ArgumentNullException), () => source.IsCountLessThan(0, null));
			Assert("Count is less than 1", source.IsCountLessThan(1));
			Assert("Count is not less than 0", !source.IsCountLessThan(0));
			Assert("Count is less than 2", source.IsCountLessThan(2, number => true));
			Assert("Count is not less than 0", !source.IsCountLessThan(0, number => true));
			source = new List<int>()
			{ 1, 2, 3, 4 };
			Assert("Source has less than 5 elements", source.IsCountLessThan(5));
			Assert("Source doesn't have less than 3 elements", !source.IsCountLessThan(3));
			Assert("Source has less than 5 elements", source.IsCountLessThan(5, number => true));
			Assert("Source doesn't have less than 3 elements", !source.IsCountLessThan(3, number => true));
			Assert("Source has less than 4 even elements", source.IsCountLessThan(4, number => number % 2 == 0));
			Assert("Source doesn't have less than 2 even elements", !source.IsCountLessThan(2, number => number % 2 == 0));
			source = GetCollection(0);
			Assert("Count is less than 1", source.IsCountLessThan(1));
			Assert("Count is not less than 0", !source.IsCountLessThan(0));
			Assert("Count is less than 2", source.IsCountLessThan(2, number => true));
			Assert("Count is not less than 0", !source.IsCountLessThan(0, number => true));
			source = GetCollection(4);
			Assert("Source has less than 5 elements", source.IsCountLessThan(5));
			Assert("Source doesn't have less than 3 elements", !source.IsCountLessThan(3));
			Assert("Source has less than 5 elements", source.IsCountLessThan(5, number => true));
			Assert("Source doesn't have less than 3 elements", !source.IsCountLessThan(3, number => true));
			Assert("Source has less than 4 even elements", source.IsCountLessThan(4, number => number % 2 == 0));
			Assert("Source doesn't have less than 2 even elements", !source.IsCountLessThan(2, number => number % 2 == 0));
		}

		public void TestIsCountMoreThan()
		{
			IEnumerable<int> source;
			source = null;
			AssertExceptionThrown("Source is null", typeof(ArgumentNullException), () => source.IsCountMoreThan(0));
			source = new List<int>();
			AssertExceptionThrown("Predicate is null", typeof(ArgumentNullException), () => source.IsCountMoreThan(0, null));
			Assert("Count is not more than 0", !source.IsCountMoreThan(0));
			Assert("Count is not more than 0", !source.IsCountMoreThan(0, number => true));
			source = new List<int>()
			{ 1, 2, 3, 4 };
			Assert("Source has more than 3 elements", source.IsCountMoreThan(3));
			Assert("Source doesn't have more than 5 elements", !source.IsCountMoreThan(5));
			Assert("Source has more than 3 elements", source.IsCountMoreThan(3, number => true));
			Assert("Source doesn't have less than 4 elements", !source.IsCountMoreThan(4, number => true));
			Assert("Source has more than 1 even element", source.IsCountMoreThan(1, number => number % 2 == 0));
			Assert("Source doesn't have more than 2 even elements", !source.IsCountMoreThan(2, number => number % 2 == 0));
			source = GetCollection(0);
			Assert("Count is not more than 0", !source.IsCountMoreThan(0));
			Assert("Count is not more than 0", !source.IsCountMoreThan(0, number => true));
			source = GetCollection(4);
			Assert("Source has more than 3 elements", source.IsCountMoreThan(3));
			Assert("Source doesn't have more than 5 elements", !source.IsCountMoreThan(5));
			Assert("Source has more than 3 elements", source.IsCountMoreThan(3, number => true));
			Assert("Source doesn't have less than 4 elements", !source.IsCountMoreThan(4, number => true));
			Assert("Source has more than 1 even element", source.IsCountMoreThan(1, number => number % 2 == 0));
			Assert("Source doesn't have more than 2 even elements", !source.IsCountMoreThan(2, number => number % 2 == 0));
		}

		#endregion
		#region IndexOf
		public void TestIndexOf()
		{
			var a11 = new A { I = 1 };
			var a12 = new A { I = 1 };
			var a2 = new A { I = 2 };
			var a51 = new A { I = 5 };
			var a52 = new A { I = 5 };
			var a3 = new A { I = 3 };
			var a4 = new A { I = 4 };
			var a53 = new A { I = 5 };
			var list = new List<A> { a11, a12, a2, a51, a52, a3, a4, a53 };
			AssertEquals(0, list.IndexOf(a => a.I == 1));
			AssertEquals(2, list.IndexOf(a => a.I == 2));
			AssertEquals(5, list.IndexOf(a => a.I == 3));
			AssertEquals(6, list.IndexOf(a => a.I == 4));
			AssertEquals(3, list.IndexOf(a => a.I == 5));
			AssertEquals(-1, list.IndexOf(a => a.I == 6));
			AssertEquals(-1, Enumerable.Empty<A>().IndexOf(a => a.I == 1));
		}

		#endregion
		#region Max/Min
		public void TestAllMaxAndMinByAndOnObjects()
		{
			var one = new
			{
				Name = "One",
				Value = 1
			}

			;
			var two1 = new
			{
				Name = "Two",
				Value = 2
			}

			;
			var two2 = new
			{
				Name = "Too",
				Value = 2
			}

			;
			var two3 = new
			{
				Name = "Tew",
				Value = 2
			}

			;
			var three = new
			{
				Name = "Three",
				Value = 3
			}

			;
			AssertContainsExactElementsInAnyOrder(new[] { three }, new[] { one, two1, two2, two3, three }.CollectMaxBy(x => x.Value));
			AssertContainsExactElementsInAnyOrder(new[] { two1, two2, two3 }, new[] { one, two1, two2, two3 }.CollectMaxBy(x => x.Value));
			AssertContainsExactElementsInAnyOrder(new[] { one }, new[] { one, two1, two2, two3, three }.CollectMinBy(x => x.Value));
			AssertContainsExactElementsInAnyOrder(new[] { two1, two2, two3 }, new[] { two1, two2, two3, three }.CollectMinBy(x => x.Value));
		}

		public void TestAllMaxAndMinByDefaults()
		{
			var one = new
			{
				Name = "Inky Plinky Plonky",
				Value = default(int)
			}

			;
			var two = new
			{
				Name = "Daddy Bought a Donkey",
				Value = default(int)
			}

			;
			AssertContainsExactElementsInAnyOrder(new[] { one, two }, new[] { one, two }.CollectMaxBy(x => x.Value));
			AssertContainsExactElementsInAnyOrder(new[] { one, two }, new[] { one, two }.CollectMinBy(x => x.Value));
			AssertEquals(0, Enumerable.Empty<int>().CollectMaxBy(x => x).Count);
			AssertEquals(0, Enumerable.Empty<int>().CollectMinBy(x => x).Count);
		}

		public void TestMaxByOnInts()
		{
			AssertExceptionThrown<InvalidOperationException>(() => Array.Empty<int>().MaxBy(i => i));
			AssertNoExceptionThrown(() => Array.Empty<int>().MaxBySafe(i => i));
			AssertEquals(1, new[] { 1 }.MaxBy(i => i));
			AssertEquals(5, new[] { 1, 2, 3, 5, 4 }.MaxBy(i => i));
		}

		public void TestMinByOnInts()
		{
			AssertExceptionThrown<InvalidOperationException>(() => Array.Empty<int>().MinBy(i => i));
			AssertNoExceptionThrown(() => Array.Empty<int>().MinBySafe(i => i));
			AssertEquals(1, new[] { 1 }.MinBy(i => i));
			AssertEquals(-1, new[] { 1, 2, -1, 5, 4 }.MinBy(i => i));
		}

		public void TestMaxAndMinByOnObjects()
		{
			var one = new
			{
				Name = "One",
				Value = 1
			}

			;
			var two = new
			{
				Name = "Two",
				Value = 2
			}

			;
			var three = new
			{
				Name = "Three",
				Value = 3
			}

			;
			AssertEquals(three, new[] { one, three, two }.MaxBy(x => x.Value));
			AssertEquals(one, new[] { one, three, two }.MinBy(x => x.Value));
		}

		public void TestMinOrDefault()
		{
			AssertEquals(0, Array.Empty<int>().MinOrDefault(n => n));
			AssertEquals(1, new int[] { 1, 2, 3 }.MinOrDefault(n => n));
		}

		public void TestMaxOrDefault_ShouldNotHitSelectorMultipleTimes()
		{
			var hit = false;
			var selector = new Func<int, int>(i =>
			{
				if (hit)
				{
					throw new InvalidOperationException("Hit selector multiple times");
				}
				else
				{
					hit = true;
					return i;
				}
			});
			AssertEquals(1, new[] { 1 }.MaxOrDefault(selector));
		}

		#endregion
		#region TakeUntil
		public void TestTakeUntil()
		{
			var seq = new[] { 1, 2, 3 };
			Assert(seq.TakeUntil(i => i == 1).SequenceEqual(new[] { 1 }));
			Assert(seq.TakeUntil(i => i == 2).SequenceEqual(new[] { 1, 2 }));
			Assert(seq.TakeUntil(i => i == 3).SequenceEqual(new[] { 1, 2, 3 }));
			Assert(seq.TakeUntil(i => i == 4).SequenceEqual(new[] { 1, 2, 3 }));
		}

		#endregion
		#region Chunk
		public void TestChunk()
		{
			var chunks = Enumerable.Range(1, 20);
			var list = new List<string>();
			foreach (var chunk in chunks.Chunk(5))
			{
				list.Add($"[{string.Join(", ", chunk)}]");
			}

			AssertEquals(list.Count, 4);
			AssertEquals("[1, 2, 3, 4, 5]", list[0]);
			AssertEquals("[6, 7, 8, 9, 10]", list[1]);
			AssertEquals("[11, 12, 13, 14, 15]", list[2]);
			AssertEquals("[16, 17, 18, 19, 20]", list[3]);
		}

		#endregion
		#region Dictionary
		public void TestToKeyListDictionary()
		{
			var a11 = new A { I = 1 };
			var a12 = new A { I = 1 };
			var a2 = new A { I = 2 };
			var a51 = new A { I = 5 };
			var a52 = new A { I = 5 };
			var a3 = new A { I = 3 };
			var a4 = new A { I = 4 };
			var a53 = new A { I = 5 };
			var list = new List<A> { a12, a2, a52, a3, a51, a4, a53, a11, };
			var dict = list.ToKeyListDictionary(x => x.I);
			List<A> listValue;
			Assert(dict.TryGetValue(1, out listValue));
			AssertContainsExactElementsInAnyOrder(new[] { a11, a12 }, listValue);
			Assert(dict.TryGetValue(2, out listValue));
			AssertContainsExactElementsInAnyOrder(new[] { a2 }, listValue);
			Assert(dict.TryGetValue(3, out listValue));
			AssertContainsExactElementsInAnyOrder(new[] { a3 }, listValue);
			Assert(dict.TryGetValue(4, out listValue));
			AssertContainsExactElementsInAnyOrder(new[] { a4 }, listValue);
			Assert(dict.TryGetValue(5, out listValue));
			AssertContainsExactElementsInAnyOrder(new[] { a51, a52, a53 }, listValue);
			AssertEquals(5, dict.Count);
		}

		#endregion
		#region Test Utils
		static IEnumerable<int> GetCollection(int count)
		{
			for (int i = 1; i <= count; i++)
			{
				yield return i;
			}
		}

		#endregion
		#region Test In
		public void TestInOnString()
		{
			var apple = "apple";
			var fruits = new[] { "apple", "mango", "banana" };
			var dolphin = "dolphin";
			var animals = new[] { "dolphin", "shark", "octopus" };
			Assert("apple should be 'in' fruits", apple.In(fruits));
			Assert("dolphin should be 'in' animals", dolphin.In(animals));
			Assert("apple should not 'in' animals", !apple.In(animals));
			Assert("dolphin should not 'in' fruits", !dolphin.In(fruits));
		}

		#endregion
		#region Select Distinct Recursive
		public void TestSelectDistinctRecursive_AllowCircular()
		{
			var node1 = new TreeNode();
			var node2 = new TreeNode();
			var node3 = new TreeNode();
			var node4 = new TreeNode();
			node1.Children.AddRange(new[] { node1 });
			AssertEquals(1, node1.Children.SelectDistinctRecursive(n => n.Children).Count());
		}

		public void TestSelectDistinctRecursive_Distinct()
		{
			var node1 = new TreeNode();
			var node2 = new TreeNode();
			var node3 = new TreeNode();
			var node4 = new TreeNode();
			node1.Children.AddRange(new[] { node2, node2, node2 });
			node2.Children.Add(node3);
			node3.Children.Add(node4);
			AssertEquals(4, new[] { node1 }.SelectDistinctRecursive(n => n.Children).Count());
			AssertContainsExactElementsInAnyOrder(new[] { node1, node2, node3, node4 }, new[] { node1 }.SelectDistinctRecursive(n => n.Children));
		}

		class TreeNode
		{
			public List<TreeNode> Children
			{
				get
				{
					return children;
				}
			}

			readonly List<TreeNode> children = new List<TreeNode>();
		}

		#endregion
		#region Before/After
		public void TestElementAfterValue()
		{
			AssertEquals(default, Array.Empty<int>().ElementAfter(5));
			var noDuplicates = new[] { 0, 1, 452343, 2, 534, 3, 4 };
			AssertEquals(1, noDuplicates.ElementAfter(0));
			AssertEquals(default, noDuplicates.ElementAfter(4));
			AssertEquals(452343, noDuplicates.ElementAfter(1));
			AssertEquals(4, noDuplicates.ElementAfter(3));
			AssertEquals(2, noDuplicates.ElementAfter(452343));
			AssertEquals(534, noDuplicates.ElementAfter(2));
			AssertEquals(default, noDuplicates.ElementAfter(3248022));
			var duplicates = new[] { 0, 1, 234, 2, 234, 1, 44 };
			AssertEquals(1, duplicates.ElementAfter(0));
			AssertEquals(234, duplicates.ElementAfter(1));
			AssertEquals(2, duplicates.ElementAfter(234));
			AssertEquals(234, duplicates.ElementAfter(2));
			AssertEquals(default, duplicates.ElementAfter(44));
			AssertEquals(default, duplicates.ElementAfter(3248022));
		}

		public void TestElementInFrontOf()
		{
			AssertEquals(default, Array.Empty<int>().ElementInFrontOf(10));
			var noDuplicates = new[] { 0, 1, 452343, 2, 534, 3, 4 };
			AssertEquals(default, noDuplicates.ElementInFrontOf(0));
			AssertEquals(3, noDuplicates.ElementInFrontOf(4));
			AssertEquals(0, noDuplicates.ElementInFrontOf(1));
			AssertEquals(534, noDuplicates.ElementInFrontOf(3));
			AssertEquals(1, noDuplicates.ElementInFrontOf(452343));
			AssertEquals(452343, noDuplicates.ElementInFrontOf(2));
			AssertEquals(default, noDuplicates.ElementInFrontOf(3248022));
			var duplicates = new[] { 0, 1, 234, 2, 234, 1, 44 };
			AssertEquals(default, duplicates.ElementInFrontOf(0));
			AssertEquals(0, duplicates.ElementInFrontOf(1));
			AssertEquals(1, duplicates.ElementInFrontOf(234));
			AssertEquals(234, duplicates.ElementInFrontOf(2));
			AssertEquals(1, duplicates.ElementInFrontOf(44));
			AssertEquals(default, duplicates.ElementInFrontOf(3248022));
		}

		public void TestElementAfterRef()
		{
			var a1 = new A();
			var a2 = new A();
			var a3 = new A();
			var a4 = new A();
			var a5 = new A();
			var a6 = new A();
			var aMissing = new A();
			var noDuplicates = new[] { a2, a1, null, a6, a4, a3, a5 };
			AssertEquals(a1, noDuplicates.ElementAfter(a2));
			AssertNull(noDuplicates.ElementAfter(a1));
			AssertEquals(a6, noDuplicates.ElementAfter(null));
			AssertEquals(a4, noDuplicates.ElementAfter(a6));
			AssertEquals(a3, noDuplicates.ElementAfter(a4));
			AssertEquals(a5, noDuplicates.ElementAfter(a3));
			AssertNull(noDuplicates.ElementAfter(a5));
			AssertNull(noDuplicates.ElementAfter(aMissing));
			AssertNotEquals(a2, noDuplicates.ElementAfter(a1));
			var duplicates = new[] { a2, a1, a2, a3, a4, a3, a6 };
			AssertEquals(a1, duplicates.ElementAfter(a2));
			AssertEquals(a2, duplicates.ElementAfter(a1));
			AssertEquals(a4, duplicates.ElementAfter(a3));
			AssertEquals(a3, duplicates.ElementAfter(a4));
			AssertNull(duplicates.ElementAfter(a6));
			AssertNull(duplicates.ElementAfter(aMissing));
		}

		public void TestElementInFrontOfRef()
		{
			var a1 = new A();
			var a2 = new A();
			var a3 = new A();
			var a4 = new A();
			var a5 = new A();
			var a6 = new A();
			var aMissing = new A();
			var noDuplicates = new[] { a2, a1, a6, null, a4, a3, a5 };
			AssertNull(noDuplicates.ElementInFrontOf(a2));
			AssertEquals(a2, noDuplicates.ElementInFrontOf(a1));
			AssertEquals(a1, noDuplicates.ElementInFrontOf(a6));
			AssertEquals(a6, noDuplicates.ElementInFrontOf(null));
			AssertNull(noDuplicates.ElementInFrontOf(a4));
			AssertEquals(a4, noDuplicates.ElementInFrontOf(a3));
			AssertEquals(a3, noDuplicates.ElementInFrontOf(a5));
			AssertNull(noDuplicates.ElementInFrontOf(aMissing));
			AssertNotEquals(a6, noDuplicates.ElementInFrontOf(a1));
			var duplicates = new[] { a2, a1, a2, a3, a4, a3, a6 };
			AssertNull(duplicates.ElementInFrontOf(a2));
			AssertEquals(a2, duplicates.ElementInFrontOf(a1));
			AssertEquals(a2, duplicates.ElementInFrontOf(a3));
			AssertEquals(a3, duplicates.ElementInFrontOf(a4));
			AssertEquals(a3, duplicates.ElementInFrontOf(a6));
			AssertNull(duplicates.ElementInFrontOf(aMissing));
		}

		public void TestElementAfterCustom()
		{
			var a1 = new A { I = 3 };
			var a2 = new A { I = 4 }; //4
			var a3 = new A { I = 1 }; //1
			var a4 = new A { I = 1 }; //1
			var a5 = new A { I = 9 };
			var a6 = new A { I = 4 }; //4
			var aMissing = new A { I = 100 };
			var elements = new[] { a2, a1, a5, null, null, a2, a3, a4, a3, a6 };
			var comparer = new AEqualityComparer();
			AssertEquals(a1, elements.ElementAfter(a2, comparer));
			AssertEquals(a1, elements.ElementAfter(a6, comparer));
			AssertEquals(a4, elements.ElementAfter(a3, comparer));
			AssertEquals(a4, elements.ElementAfter(a4, comparer));
			AssertEquals(a5, elements.ElementAfter(a1, comparer));
			AssertNull(elements.ElementAfter(a5, comparer));
			AssertNull(elements.ElementAfter(null, comparer));
			AssertNull(elements.ElementAfter(aMissing, comparer));
		}

		public void TestElementInFrontOfCustom()
		{
			var a1 = new A { I = 3 };
			var a2 = new A { I = 4 }; //4
			var a3 = new A { I = 1 }; //1
			var a4 = new A { I = 1 }; //1
			var a5 = new A { I = 9 };
			var a6 = new A { I = 4 }; //4
			var aMissing = new A { I = 100 };
			var elements = new[] { a2, a1, a5, null, null, a2, a3, a4, a3, a6 };
			var comparer = new AEqualityComparer();
			AssertNull(elements.ElementInFrontOf(a2, comparer));
			AssertNull(elements.ElementInFrontOf(a6, comparer));
			AssertEquals(a2, elements.ElementInFrontOf(a3, comparer));
			AssertEquals(a2, elements.ElementInFrontOf(a4, comparer));
			AssertEquals(a2, elements.ElementInFrontOf(a1, comparer));
			AssertEquals(a1, elements.ElementInFrontOf(a5, comparer));
			AssertEquals(a5, elements.ElementInFrontOf(null, comparer));
			AssertNull(elements.ElementInFrontOf(aMissing, comparer));
		}

		#endregion
		#region ForEach
		public void TestForEach_NoMultiEnumeration()
		{
			var executions = 0;
			var dummy = AddString("A", "B", "C");
			dummy.ForEach(d => d = d.ToLower());
			AssertEquals("Should execute 3 times", 3, executions);
			IEnumerable<string> AddString(params string[] values)
			{
				foreach (var value in values)
				{
					executions++;
					yield return value;
				}
			}
		}

		public void TestForEach_ByIncorporatingIndex()
		{
			var arr = new[] { "A", "B", "C", "D" };
			var b = new StringBuilder();
			arr.ForEach((a, i) => b.Append($"{i}-{a}, "));
			AssertEquals("0-A, 1-B, 2-C, 3-D, ", b.ToString());
		}

		#endregion
		#region Betweenies
		public void TestForEachWithBetween()
		{
			var arr = new[] { 1, 2, 3, 4, 5 };
			var b = new StringBuilder();
			arr.ForEachWithBetween(a => b.Append(a), () => b.Append(", "));
			AssertEquals("1, 2, 3, 4, 5", b.ToString());
		}

		public void TestSelectWithBetween()
		{
			AssertArrayEqualsByElements(new[] { 1, 0, 2, 0, 3, 0, 4, 0, 5 }, new[] { 1, 2, 3, 4, 5 }.SelectWithBetween(a => a, () => 0).ToArray());
			AssertArrayEqualsByElements(new[] { 1 }, new[] { 1 }.SelectWithBetween(a => a, () => 0).ToArray());
		}

		#endregion
		#region Fallback Search
		public void TestSearchWithFallback()
		{
			var arr = new[] { 1, 1, 2, 2, 3, 3, 4 };
			AssertArrayEqualsByElements(new[] { 1, 1 }, arr.SearchWithFallback(k => k, 1, 2).ToArray());
			AssertArrayEqualsByElements(new[] { 2, 2 }, arr.SearchWithFallback(k => k, 2, 3).ToArray());
			AssertArrayEqualsByElements(new[] { 4 }, arr.SearchWithFallback(k => k, 7, 99, 1234324, 4).ToArray());
		}
		#endregion
		#region Test util classes

		public class A
		{
			public int I { get; set; }
			public override string ToString() => "A" + I;
		}

		public class AComparer : IComparer<A>
		{
			public int Compare(A x, A y)
			{
				if (x != null && y != null)
				{
					return x.I.CompareTo(y.I);
				}
				else
				{
					return (x != null).CompareTo(y != null);
				}
			}
		}

		public class AEqualityComparer : IEqualityComparer<A>
		{
			public bool Equals(A x, A y) => x == y || (x?.I.Equals(y?.I) ?? false);

			public int GetHashCode(A obj) => obj.I.GetHashCode();
		}

		#endregion
		#region ConsecutivePairs

		public void TestConsecutivePairs_MultiPrereqWorks()
		{
			var set = new [] {
				("a", 10),
				("b", 10),
				("c", 20),
				("d", 20),
				("e", 30),
				("f", 30),
			};
			var pairs = set.ConsecutivePairs(f => f.Item2).ToArray();

			AssertCollectionContains((set[0], set[2]), pairs);
			AssertCollectionContains((set[0], set[3]), pairs);
			AssertCollectionContains((set[1], set[2]), pairs);
			AssertCollectionContains((set[1], set[3]), pairs);
			AssertCollectionContains((set[2], set[4]), pairs);
			AssertCollectionContains((set[2], set[5]), pairs);
			AssertCollectionContains((set[3], set[4]), pairs);
			AssertCollectionContains((set[3], set[5]), pairs);

			AssertEquals(8, pairs.Length);
		}

		public void TestConsecutivePairs_StraightLine()
		{
			var set = new [] {
				("a", 10),
				("b", 20),
				("c", 30),
			};

			var pairs = set.ConsecutivePairs(f => f.Item2).ToArray();

			AssertCollectionContains((set[0], set[1]), pairs);
			AssertCollectionContains((set[1], set[2]), pairs);
			AssertEquals(2, pairs.Length);
		}

		#endregion
	}
}

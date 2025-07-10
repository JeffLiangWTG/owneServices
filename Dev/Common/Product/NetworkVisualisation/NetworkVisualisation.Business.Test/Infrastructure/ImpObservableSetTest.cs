using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	class ImpObservableSetTest : TestCase
	{
		#region Test Data

		protected override void SetUp()
		{
			base.SetUp();
			LazyIntSet = new Lazy<ImpObservableSet<int>>(() => new ImpObservableSet<int>());
			LazyBoxSet = new Lazy<ImpObservableSet<Box>>(() => new ImpObservableSet<Box>());
		}

		Lazy<ImpObservableSet<int>> LazyIntSet { get; set; }
		Lazy<ImpObservableSet<Box>> LazyBoxSet { get; set; }
		public ImpObservableSet<int> IntSet => LazyIntSet.Value;
		public ImpObservableSet<Box> BoxSet => LazyBoxSet.Value;

		#endregion

		public void TestBox_Distinct()
		{
			var adds = BoxSet.TrackAddRemoved();

			BoxSet.Add(new Box(0));

			AssertEquals(1, adds.Added.Count);
			AssertEquals(0, adds.Removed.Count);
			AssertCollectionContains(0, adds.Added.Select(s => s.BoxedInt));

			BoxSet.Add(new Box(0));

			AssertEquals(1, adds.Added.Count);
			AssertEquals(0, adds.Removed.Count);
			AssertCollectionContains(0, adds.Added.Select(s => s.BoxedInt));

			BoxSet.Add(new Box(1));

			AssertEquals(2, adds.Added.Count);
			AssertEquals(0, adds.Removed.Count);
			AssertCollectionContains(1, adds.Added.Select(s => s.BoxedInt));

			BoxSet.Remove(new Box(1));

			AssertEquals(2, adds.Added.Count);
			AssertEquals(1, adds.Removed.Count);
			AssertCollectionContains(1, adds.Removed.Select(s => s.BoxedInt));
		}

		public void TestInt_RemovesDistinct()
		{
			var providerList = new List<int>(new[] { 1, 2, 3, 4 });
			LazyIntSet = new Lazy<ImpObservableSet<int>>(() => new ImpObservableSet<int>(() => providerList));

			var adds = IntSet.TrackAddRemoved();
			AssertEquals(4, IntSet.Count);

			IntSet.Reload();
			AssertEquals(4, IntSet.Count);
			AssertContainsExactElementsInAnyOrder(providerList, IntSet);

			IntSet.Remove(1);
			IntSet.Remove(1);
			IntSet.Remove(1);
			AssertContainsExactElementsInAnyOrder(providerList.Where(t => t != 1), IntSet);

			AssertCollectionContains(1, adds.Removed);
			AssertEquals(0, adds.Added.Count);
		}

		public void TestInt_ReloadAddsAreDistinct()
		{
			var providerList = new List<int>(new[] { 1, 2, 3, 4 });
			LazyIntSet = new Lazy<ImpObservableSet<int>>(() => new ImpObservableSet<int>(() => providerList));

			var changed = IntSet.TrackAddRemoved();
			AssertEquals(4, IntSet.Count);

			providerList.RemoveRange(2, 2);
			AssertEquals(4, IntSet.Count);

			IntSet.Reload();
			AssertEquals(2, IntSet.Count);
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2 }, IntSet);
			AssertEquals(2, changed.Removed.Count);
			AssertContainsExactElementsInAnyOrder(new[] { 3, 4 }, changed.Removed);
		}

		public void TestInt_MultipleReloads_NoAdditionalEvents()
		{
			var providerList = new List<int>(new[] { 1, 2, 3, 4 });
			LazyIntSet = new Lazy<ImpObservableSet<int>>(() => new ImpObservableSet<int>(() => providerList));

			var changed = IntSet.TrackAddRemoved();
			AssertEquals(4, IntSet.Count);

			providerList.RemoveRange(2, 2);
			providerList.Add(99);
			AssertEquals(4, IntSet.Count);

			IntSet.Reload();
			IntSet.Reload();
			IntSet.Reload();
			AssertEquals(3, IntSet.Count);
			AssertContainsExactElementsInAnyOrder(new[] { 1, 2, 99 }, IntSet);
			AssertEquals(2, changed.Removed.Count);
			AssertEquals(1, changed.Added.Count);
			AssertContainsExactElementsInAnyOrder(new[] { 3, 4 }, changed.Removed);
			AssertContainsExactElementsInAnyOrder(new[] { 99 }, changed.Added);
		}

		public void TestInt_AddAndRemoveSameElements_NoDisfunction_ReloadAndUpdate()
		{
			var set1 = new[] { 1, 2 };
			var providerList = new List<int>(set1);
			LazyIntSet = new Lazy<ImpObservableSet<int>>(() => new ImpObservableSet<int>(() => providerList));

			var chanaged = IntSet.TrackAddRemoved();
			AssertEquals(2, IntSet.Count);

			providerList.Clear();
			IntSet.Reload();
			AssertEquals(0, IntSet.Count);
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), chanaged.Added);
			AssertContainsExactElementsInAnyOrder(set1, chanaged.Removed);
			chanaged.Clear();

			providerList.AddRange(set1);
			IntSet.Reload();
			AssertEquals(2, IntSet.Count);
			AssertContainsExactElementsInAnyOrder(set1, chanaged.Added);
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), chanaged.Removed);
			chanaged.Clear();

			providerList.Clear();
			IntSet.Update();
			AssertEquals(0, IntSet.Count);
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), chanaged.Added);
			AssertContainsExactElementsInAnyOrder(set1, chanaged.Removed);
			chanaged.Clear();

			providerList.AddRange(set1);
			IntSet.Update();
			AssertEquals(2, IntSet.Count);
			AssertContainsExactElementsInAnyOrder(set1, chanaged.Added);
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), chanaged.Removed);
		}

		public void TestInt_Changed_ReloadAndUpdate()
		{
			var set1 = new[] { 1, 2 };
			var providerList = new List<int>(set1);
			LazyIntSet = new Lazy<ImpObservableSet<int>>(() => new ImpObservableSet<int>(() => providerList));

			var changed = IntSet.TrackChanged();
			AssertEquals(2, IntSet.Count);

			providerList.Clear();
			IntSet.Reload();
			AssertEquals(0, IntSet.Count);
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), changed.Added);
			AssertContainsExactElementsInAnyOrder("Reloads do not trigger remove.", Array.Empty<int>(), changed.Removed);
			changed.Clear();

			providerList.AddRange(set1);
			IntSet.Reload();
			AssertEquals(2, IntSet.Count);
			AssertContainsExactElementsInAnyOrder(set1, changed.Added);
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), changed.Removed);
			changed.Clear();

			providerList.Clear();
			IntSet.Update();
			AssertEquals(0, IntSet.Count);
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), changed.Added);
			AssertContainsExactElementsInAnyOrder("Updates do though", set1, changed.Removed);
			changed.Clear();

			providerList.AddRange(set1);
			IntSet.Update();
			AssertEquals(2, IntSet.Count);
			AssertContainsExactElementsInAnyOrder(set1, changed.Added);
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), changed.Removed);
		}

		public void TestInt_Changed_UpdateMultipleTimes()
		{
			var set1 = new[] { 1, 2 };
			var providerList = new List<int>(set1);
			LazyIntSet = new Lazy<ImpObservableSet<int>>(() => new ImpObservableSet<int>(() => providerList));

			var changed = IntSet.TrackChanged();
			AssertEquals(2, IntSet.Count);

			IntSet.Update();
			IntSet.Update();
			IntSet.Update();
			IntSet.Update();
			AssertContainsExactElementsInAnyOrder("Only new items are added?", Array.Empty<int>(), changed.Added);
			AssertContainsExactElementsInAnyOrder("Reloads do not trigger remove.", Array.Empty<int>(), changed.Removed);
		}
	}

	static class ImpObservableTestExtensions
	{
		public static ItemsChanged<T> TrackAddRemoved<T>(this ImpObservableCollection<T> collection)
		{
			var items = new ItemsChanged<T>();
			collection.ItemsAdded += (s, e) => items.Added.AddRange(e.Items?.Cast<T>());
			collection.ItemsRemoved += (s, e) => items.Removed.AddRange(e.Items?.Cast<T>());
			return items;
		}

		public static ItemsChanged<T> TrackChanged<T>(this ImpObservableCollection<T> collection)
		{
			var items = new ItemsChanged<T>();
			collection.CollectionChanged += (s, e) =>
			{
				if (e.OldItems != null)
				{
					items.Removed.AddRange(e.OldItems.Cast<T>());
				}

				if (e.NewItems != null)
				{
					items.Added.AddRange(e.NewItems.Cast<T>());
				}
			};
			return items;
		}

		public class ItemsChanged<T>
		{
			public List<T> Added { get; } = new List<T>();
			public List<T> Removed { get; } = new List<T>();

			public void Clear()
			{
				Added.Clear();
				Removed.Clear();
			}
		}
	}
}

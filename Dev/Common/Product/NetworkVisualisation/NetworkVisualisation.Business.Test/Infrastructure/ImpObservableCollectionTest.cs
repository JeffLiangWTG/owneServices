using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.Business.Test
{
	class ImpObservableCollectionTest : TestCase
	{
		public void TestReload_ShouldRestoreOriginalItemsFromClosure()
		{
			var collection = new ImpObservableCollection<int>(() => new[] { 1, 2, 3 });
			AssertEquals(3, collection.Count);
			Assert(collection.SequenceEqual(new[] { 1, 2, 3 }));

			collection.Clear();
			AssertEquals(0, collection.Count);

			collection.Reload();
			AssertEquals(3, collection.Count);
			Assert(collection.SequenceEqual(new[] { 1, 2, 3 }));
		}

		public void TestReload_RaiseAddEventsForNewEntities()
		{
			var baseCollection = new HashSet<int>();
			var itemsAdded = new HashSet<int>();

			var collection = new ImpObservableCollection<int>(() => baseCollection);

			var firedCount = 0;
			collection.ItemsAdded += (s, e) =>
			{
				firedCount++;
				foreach (int item in e.Items)
				{
					itemsAdded.Add(item);
				}
			};

			Assert(baseCollection.SetEquals(itemsAdded));

			baseCollection.Add(1);
			baseCollection.Add(2);
			baseCollection.Add(3);

			Assert(!baseCollection.SetEquals(itemsAdded));

			collection.Reload();

			Assert(baseCollection.SetEquals(itemsAdded));
			AssertEquals(1, firedCount);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using NUnit.Framework;

namespace CargoWise.Common.Collections.Testing
{
	internal class ObservableDictionaryTest : TestCase
	{
		#region Add
		public void TestAdd_ItemsWithSuchKeyExists_ThrowException()
		{
			var dictionary = new ObservableDictionary<string, string>();
			dictionary["LOC"] = "UAIEV";
			AssertExceptionThrown<ArgumentException>(() => dictionary.Add("LOC", "ZEK"));
		}

		public void TestICollection_Add_ItemsWithSuchKeyExists_ThrowException()
		{
			ICollection<KeyValuePair<string, string>> dictionary = new ObservableDictionary<string, string>();
			dictionary.Add(new KeyValuePair<string, string>("LOC", "UAIEV"));
			AssertExceptionThrown<ArgumentException>(() => dictionary.Add(new KeyValuePair<string, string>("LOC", "ZEK")));
		}

		public void TestAdd_KeyIsUnique_AddItem()
		{
			var eventIsFired = false;
			var dictionary = new ObservableDictionary<string, string>();
			var item = new KeyValuePair<string, string>("LOC", "LEZOK");
			dictionary.CollectionChanged += (s, e) =>
			{
				CheckEventArgs(e, NotifyCollectionChangedAction.Add, new[] { item }, null);
				eventIsFired = true;
			};
			dictionary.Add(item.Key, item.Value);
			AssertEquals("Value in dictionary", dictionary[item.Key], item.Value);
			AssertEquals("Event has been fired", true, eventIsFired);
		}

		public void TestICollection_Add_KeyIsUnique_AddItem()
		{
			var eventIsFired = false;
			var dictionary = new ObservableDictionary<string, string>();
			var item = new KeyValuePair<string, string>("LOC", "LEZOK");
			dictionary.CollectionChanged += (s, e) =>
			{
				CheckEventArgs(e, NotifyCollectionChangedAction.Add, new[] { item }, null);
				eventIsFired = true;
			};
			dictionary.Add(item);
			AssertEquals("Value in dictionary", dictionary[item.Key], item.Value);
			AssertEquals("Event has been fired", true, eventIsFired);
		}

		#endregion
		#region Remove
		public void TestRemove_ItemsWithSuchKeyDoesntExist_ReturnFalse()
		{
			var dictionary = new ObservableDictionary<string, string>();
			dictionary["LOC"] = "UAIEV";
			var removed = dictionary.Remove("LOC2");
			AssertEquals("The value has been removed", false, removed);
		}

		public void TestICollection_Remove_ItemsWithSuchKeyDoesntExist_ReturnFalse()
		{
			ICollection<KeyValuePair<string, string>> dictionary = new ObservableDictionary<string, string>();
			dictionary.Add(new KeyValuePair<string, string>("LOC", "UAIEV"));
			var removed = dictionary.Remove(new KeyValuePair<string, string>("LOC2", "UAIEV"));
			AssertEquals("The value has been removed", false, removed);
		}

		public void TestRemove_ItemsWithSuchKeyExist_RemoveItemAndReturnTrue()
		{
			var eventIsFired = false;
			var dictionary = new ObservableDictionary<string, string>();
			var item = new KeyValuePair<string, string>("LOC", "UAIEV");
			dictionary[item.Key] = item.Value;
			dictionary.CollectionChanged += (s, e) =>
			{
				CheckEventArgs(e, NotifyCollectionChangedAction.Remove, null, new[] { item });
				eventIsFired = true;
			};
			var removed = dictionary.Remove(item.Key);
			AssertEquals("The value has been removed", true, removed);
			AssertEquals("The key exists in dictionary", false, dictionary.ContainsKey("LOC"));
			AssertEquals("Event has been fired", true, eventIsFired);
		}

		public void TestICollection_Remove_ItemsWithSuchKeyExist_RemoveItemAndReturnTrue()
		{
			var eventIsFired = false;
			var dictionary = new ObservableDictionary<string, string>();
			var item = new KeyValuePair<string, string>("LOC", "UAIEV");
			dictionary.Add(item);
			dictionary.CollectionChanged += (s, e) =>
			{
				CheckEventArgs(e, NotifyCollectionChangedAction.Remove, null, new[] { item });
				eventIsFired = true;
			};
			var removed = dictionary.Remove(item);
			AssertEquals("The value has been removed", true, removed);
			AssertEquals("The key exists in dictionary", false, dictionary.ContainsKey("LOC"));
			AssertEquals("Event has been fired", true, eventIsFired);
		}

		#endregion
		#region Clear
		public void TestClear_DictionaryContainsSomeItems_RemoveAllItems()
		{
			var eventIsFired = false;
			var dictionary = new ObservableDictionary<string, string>();
			dictionary["LOC"] = "SARADIP";
			dictionary["VORAZA"] = "LOH";
			dictionary.CollectionChanged += (s, e) =>
			{
				AssertEquals("Action type in event", NotifyCollectionChangedAction.Reset, e.Action);
				eventIsFired = true;
			};
			dictionary.Clear();
			AssertEquals("Items count in dictionary", 0, dictionary.Count);
			AssertEquals("Event has been fired", true, eventIsFired);
		}

		#endregion
		#region Indexer
		public void TestIndexer_ItemsWithSuchKeyExists_ReplaceValue()
		{
			var eventIsFired = false;
			var dictionary = new ObservableDictionary<string, string>();
			dictionary["LOC"] = "UAIEV";
			dictionary.CollectionChanged += (s, e) =>
			{
				var oldItem = (KeyValuePair<string, string>)e.OldItems[0];
				var newItem = (KeyValuePair<string, string>)e.NewItems[0];
				AssertEquals("Action type in event", NotifyCollectionChangedAction.Replace, e.Action);
				AssertEquals("Removed item's key in event", "LOC", oldItem.Key);
				AssertEquals("Removed item's value in event", "UAIEV", oldItem.Value);
				AssertEquals("Added item's key in event", "LOC", newItem.Key);
				AssertEquals("Added item's value in event", "ZEK", newItem.Value);
				eventIsFired = true;
			};
			dictionary["LOC"] = "ZEK";
			AssertEquals("Value in dictionary", dictionary["LOC"], "ZEK");
			AssertEquals("Event has been fired", true, eventIsFired);
		}

		public void TestIndexer_KeyIsUnique_AddItem()
		{
			var eventIsFired = false;
			var dictionary = new ObservableDictionary<string, string>();
			dictionary.CollectionChanged += (s, e) =>
			{
				var item = (KeyValuePair<string, string>)e.NewItems[0];
				AssertEquals("Action type in event", NotifyCollectionChangedAction.Add, e.Action);
				AssertEquals("Item's key in event", "LOC", item.Key);
				AssertEquals("Item's value in event", "ZEK", item.Value);
				eventIsFired = true;
			};
			dictionary["LOC"] = "ZEK";
			AssertEquals("Value in dictionary", dictionary["LOC"], "ZEK");
			AssertEquals("Event has been fired", true, eventIsFired);
		}

		#endregion
		#region Equals

		public void TestEquals_DictionaryWithDifferentKeysValues_ReturnFalse()
		{
			var dictionary = new ObservableDictionary<string, string> { { "LOC", "UAIEV" } };
			var otherDictionary = new ObservableDictionary<string, string> { { "LOC2", "UAIEV" } };
			Assert(!dictionary.Equals(otherDictionary));
		}

		public void TestEquals_Null_ReturnFalse()
		{
			var dictionary = new ObservableDictionary<string, string>();
			Assert(!dictionary.Equals(null));
		}

		public void TestEquals_DictionaryWithDifferentCount_ReturnFalse()
		{
			var dictionary = new ObservableDictionary<string, string>();
			var item = new KeyValuePair<string, string>("LOC", "UAIEV");
			var secondItem = new KeyValuePair<string, string>("LOC2", "UAIEV");
			dictionary.Add(item);
			dictionary.Add(secondItem);

			var otherDictionary = new ObservableDictionary<string, string> { item, secondItem, { "LOC3", "UAIEV" } };
			Assert(!dictionary.Equals(otherDictionary));
		}

		public void TestEquals_DictionaryWithSameKeysValues_ReturnTrue()
		{
			var dictionary = new ObservableDictionary<string, string>();
			var item = new KeyValuePair<string, string>("LOC", "UAIEV");
			var secondItem = new KeyValuePair<string, string>("LOC2", "UAIEV");
			dictionary.Add(item);
			dictionary.Add(secondItem);

			var otherDictionary = new ObservableDictionary<string, string> { secondItem, item };
			Assert(dictionary.Equals(otherDictionary));
		}
		public void TestEquals_DictionaryWithSameKeysValues_ReturnTrue2()
		{
			var dictionary = new ObservableDictionary<string, string> { { "k1", "v1" }, { "k2", "v2" } };
			var otherDictionary = new ObservableDictionary<string, string> { { "k2", "v2" }, { "k1", "v1" } };
			Assert(dictionary.Equals(otherDictionary));
		}
		public void TestEquals_SameDictionary_ReturnTrue()
		{
			var dictionary = new ObservableDictionary<string, string> { { "LOC", "UAIEV" } };

			Assert(dictionary.Equals(dictionary));
		}
		#endregion Equals
		#region Internal
		void CheckEventArgs(NotifyCollectionChangedEventArgs e, NotifyCollectionChangedAction expectedAction, IEnumerable<KeyValuePair<string, string>> expectedNewItems, IEnumerable<KeyValuePair<string, string>> expectedOldItems)
		{
			Argument.NotNull(e, nameof(e)); // Suggested By ReviewBot 
			var actualNewItems = e.NewItems == null ? new List<KeyValuePair<string, string>>() : e.NewItems.Cast<KeyValuePair<string, string>>();
			var actualOldItems = e.OldItems == null ? new List<KeyValuePair<string, string>>() : e.OldItems.Cast<KeyValuePair<string, string>>();
			if (expectedOldItems == null)
			{
				expectedOldItems = new List<KeyValuePair<string, string>>();
			}

			if (expectedNewItems == null)
			{
				expectedNewItems = new List<KeyValuePair<string, string>>();
			}

			AssertEquals("Action type in event", expectedAction, e.Action);
			AssertContainsExactElementsInAnyOrder("", new KeyValueComparer(), expectedNewItems, actualNewItems);
			AssertContainsExactElementsInAnyOrder("", new KeyValueComparer(), expectedOldItems, actualOldItems);
		}

		#endregion
		#region Types
		class KeyValueComparer : IEqualityComparer<KeyValuePair<string, string>>
		{
			public bool Equals(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
			{
				return StringComparer.InvariantCultureIgnoreCase.Compare(x.Key, y.Key) == 0 & StringComparer.InvariantCultureIgnoreCase.Compare(x.Value, y.Value) == 0;
			}

			public int GetHashCode(KeyValuePair<string, string> obj)
			{
				throw new NotImplementedException();
			}
		}
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.Application;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.OverrideLevels;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Registry.Testing
{
	sealed class RegistryItemSaveHandlerTest : TestCaseWithFactory
	{
		readonly string[] brokenRegistryItems = { "HouseBillOfLadingTermsAndConditionsImages", "CustomsQuarantineChargeCode", "DocumentImages" };

		[SnailTest]
		public void TestAllRegistryItemsDefaultValuesCanBeSaved()
		{
			var itemsThatCanBeExported = ObjectFactory.Get<IRegistryItemSetLocator>()
				.GetAllRegistryItems()
				.Where(item => item.CanBeExported)
				.DistinctBy(item => item.Name)
				.ToArray();

			var level = new DefaultOverrideLevel();

			var result = SaveAndLoadItems(level, itemsThatCanBeExported);

			AssertEquals("No items should have failed", "", string.Join(", ", result.FailedItems.Select(item => item.name)));
			AssertEquals("All items should have been exported", itemsThatCanBeExported.Length, result.SuccessfulItems.Count);
		}

		public void TestWhatItemsDefaultValuesAreComparedIncorrectly()
		{
			var allItemsToSave = ObjectFactory.Get<IRegistryItemSetLocator>()
				.GetAllRegistryItems()
				.Where(item => item.CanBeExported)
				.DistinctBy(item => item.Name)
				.ToArray();

			var level = new DefaultOverrideLevel();

			var result = SaveAndLoadItems(level, allItemsToSave);

			AssertEquals("No items should have failed", 0, result.FailedItems.Count);

			var comparison = result.AsComparison(level);

			var message = new StringBuilder();
			message.Append("<b> The following items comparison failed when their default values were serialized then deserialized </b> <br />");
			message.Append("<table border=\"1\">");
			message.Append("<tr><th>Count</th><th>DataType.GetType()</th><th>Registry item names</th></tr>");

			var brokenItems = comparison.ItemsThatDifferBetweenBaseAndOverride
				.Where(item => !brokenRegistryItems.Contains(item.Name))
				.GroupBy(item => item.DataType.GetType())
				.OrderByDescending(group => group.Count());

			foreach (var group in brokenItems)
			{
				var itemNames = string.Join(", ", @group.Select(item => item.Name));
				message.AppendFormat("<tr><td>{0}</td><td>{1}</td><td>{2}</td></tr>", group.Count(), group.Key.Name, itemNames);
			}

			message.Append("</table>");

			HtmlAssert(message.ToString(), !brokenItems.Any());

			// If you fix a broken item, please remove it from the broken items list, if there are no more items, please remove the following code
			var itemsThatWereBrokenAndNowAreNot = brokenRegistryItems.Except(comparison.ItemsThatDifferBetweenBaseAndOverride.Select(item => item.Name)).ToArray();
			Assert("These items were broken and now are not. Please remove them from the exemptions: " + string.Join(", ", itemsThatWereBrokenAndNowAreNot), !itemsThatWereBrokenAndNowAreNot.Any());
		}

		public void TestGetOverrideLevel()
		{
			var registryItems = Enumerable.Range(0, 3)
				.Select(i => new IntRegistryItem("default" + i, (NoResString)"", (NoResString)"", (NoResString)"", RegistryStorageFlags.All, 0))
				.Cast<IRegistryItem>()
				.ToDictionary(item => item.Name);

			var itemsWithoutOverrides = registryItems.Values.ToArray();

			var loadedItems = LoadItems(registryItems,
				Tuple.Create("override1", "First Item", (object)1),
				Tuple.Create("override2", "Second Item", (object)1),
				Tuple.Create("override3", "Third Item", (object)1));

			var itemsWithOverrides = registryItems.Values.Except(itemsWithoutOverrides).ToArray();

			var defaultLevel = new DefaultOverrideLevel();
			var overrideLevel = loadedItems.AsOverrideLevel(defaultLevel);

			CombineAssertions(() =>
			{
				foreach (var item in itemsWithoutOverrides)
				{
					AssertEquals("Items without overrides should return their default value - " + item.Name, 0, overrideLevel.GetValueOf(item));
				}

				foreach (var item in itemsWithOverrides)
				{
					AssertEquals("Items with overrides should return the overriden value - " + item.Name, 1, overrideLevel.GetValueOf(item));
				}
			});
		}

		public void TestCorruptValuesDontCrashTheWholeThing()
		{
			var invalidBinaryValueForDataType = Convert.ToBase64String(Encoding.Default.GetBytes("This wont be able to fit into an int"));

			var loadedItems = LoadItems(
				Tuple.Create("firstOkItem", "First Item", (object)1),
				Tuple.Create("failingItem", "I dont work!", (object)invalidBinaryValueForDataType),
				Tuple.Create("secondOkItem", "Second Item", (object)1));

			AssertEquals("The other two items are still loaded", 2, loadedItems.SuccessfulItems.Count);
			AssertEquals("failingItem", loadedItems.FailedItems.Single().name);
		}

		public void TestMissingValuesDontCrashTheWholeThing()
		{
			var userNotification = (UnitTestUserNotification)Globals.Message;
			userNotification.AddAnswer(ZDialogResult.Yes);

			var loadedItems = LoadItems(
				Tuple.Create("firstFailItem", "First Item", (object)null),
				Tuple.Create("secondFailItem", (string)null, (object)1),
				Tuple.Create((string)null, "Third Item", (object)1),
				Tuple.Create("onlyOkItem", "Fourth Item", (object)1));

			AssertEquals("Only one item was successful", 1, loadedItems.SuccessfulItems.Count);
			AssertEquals("It was the expected item", "onlyOkItem", loadedItems.SuccessfulItems.Single().name);
			AssertEquals("The other items failed", 3, loadedItems.FailedItems.Count);
		}

		public void TestSaveAndLoadFiles()
		{
			var levelToSave = new SystemOverrideLevel();
			var items = CreateItemsWithOverrides(10, levelToSave, 5);

			var loaded = SaveAndLoadItems(levelToSave, items);

			AssertEquals(items.Count, loaded.SuccessfulItems.Count);

			var loadedValues = loaded.SuccessfulItems.Select(item => item.value);
			AssertArrayEqualsByElements("All should have the override value on the new item", Enumerable.Repeat(5, 10).Cast<object>().ToArray(), loadedValues.ToArray());
		}

		public void TestLoadItemsWhenSomeSavedItemsDontExistAnyMore()
		{
			const int itemsWithOverrides = 10;
			const int missingItems = 3;

			var levelToSave = new SystemOverrideLevel();

			var itemsWithChanges = CreateItemsWithOverrides(itemsWithOverrides, levelToSave, 5);
			var allItems = CreateItems(5).Concat(itemsWithChanges.Take(itemsWithOverrides - missingItems));

			var loaded = SaveAndLoadItems(levelToSave, itemsWithChanges, allItems);

			AssertEquals(missingItems, loaded.FailedItems.Count);
		}

		#region Common tools

		LoadedRegistryItems LoadItems(params Tuple<string, string, object>[] savedItems)
		{
			return LoadItems(new Dictionary<string, IRegistryItem>(), savedItems);
		}

		LoadedRegistryItems LoadItems(IDictionary<string, IRegistryItem> registryItems, params Tuple<string, string, object>[] savedItems)
		{
			var doc = CreateXmlDoc(registryItems, savedItems);

			using (var memoryStream = new MemoryStream())
			{
				doc.Save(memoryStream);
				memoryStream.Seek(0, SeekOrigin.Begin);

				return new RegistryItemSaveHandler().LoadItems(registryItems.Values, memoryStream);
			}
		}

		public static XElement CreateXmlDoc(IDictionary<string, IRegistryItem> registryItems, params Tuple<string, string, object>[] items)
		{
			var elements = new List<XElement>();

			foreach (var itemTuple in items)
			{
				string name = itemTuple.Item1, caption = itemTuple.Item2;

				IRegistryItem registryItem = null;
				if (name != null && !registryItems.TryGetValue(name, out registryItem))
				{
					registryItem = new IntRegistryItem(name, (NoResString)"", (NoResString)caption, (NoResString)"", RegistryStorageFlags.All);
					registryItems.Add(name, registryItem);
				}

				var dataType = registryItem == null ? new IntRegistryDataType() : registryItem.DataType;
				var base64Data = itemTuple.Item3 == null ? null : (itemTuple.Item3 as string ?? Serialize(dataType, itemTuple.Item3));

				elements.Add(AsItemNode(name, caption, base64Data));
			}

			var itemCollection = new XElement(RegistryItemSaveHandler.ItemCollectionName, elements);
			return new XElement(RegistryItemSaveHandler.RootNodeName, itemCollection);
		}

		static string Serialize(IRegistryDataType dataType, object value)
		{
			return Convert.ToBase64String(dataType.Serialise(value));
		}

		static XElement AsItemNode(string name, string caption, string binaryData)
		{
			var root = new XElement(RegistryItemSaveHandler.ItemNodeName);

			if (name != null)
			{
				root.SetElementValue(RegistryItemSaveHandler.ItemNameNodeName, name);
			}

			if (caption != null)
			{
				root.SetElementValue(RegistryItemSaveHandler.ItemCaptionNodeName, caption);
			}

			if (binaryData != null)
			{
				root.SetElementValue(RegistryItemSaveHandler.ItemDataNodeName, binaryData);
			}

			return root;
		}

		public LoadedRegistryItems SaveAndLoadItems(IOverrideLevel levelToSave, IEnumerable<IRegistryItem> itemsToSave, IEnumerable<IRegistryItem> itemsForLoad = null)
		{
			var saveHandler = new RegistryItemSaveHandler();
			using (var stream = new MemoryStream())
			{
				saveHandler.SaveItems(itemsToSave, levelToSave, stream);
				stream.Seek(0, SeekOrigin.Begin);

				return saveHandler.LoadItems(itemsForLoad ?? itemsToSave, stream);
			}
		}

		public List<IRegistryItem> CreateItems(int n)
		{
			return Enumerable.Range(0, n).Select(i => NewRegistryItem()).ToList();
		}

		public List<IRegistryItem> CreateItemsWithOverrides(int howManyItems, IOverrideLevel level, object value)
		{
			var items = CreateItems(howManyItems);
			items.ForEach(item => level.SetValueOf(item, value));

			return items;
		}

		static string GetRandomName()
		{
			return Guid.NewGuid().ToString("N");
		}

		public static IRegistryItem NewRegistryItem(Type t = null, string name = null, object defaultValue = null, RegistryStorageFlags storage = RegistryStorageFlags.All)
		{
			var emptyString = (NoResString)"";

			name = name ?? GetRandomName();

			IRegistryItem item;
			if (t == null || t.IsAssignableFrom(typeof(int)))
			{
				item = new IntRegistryItem(name, emptyString, emptyString, storage, (int)(defaultValue ?? 0));
			}
			else if (t.IsAssignableFrom(typeof(string)))
			{
				item = new StringRegistryItem(name, emptyString, emptyString, emptyString, storage, (string)defaultValue);
			}
			else if (t.IsAssignableFrom(typeof(ZGuid)))
			{
				item = new GuidRegistryItem(name, emptyString, emptyString, emptyString, storage, (Guid)(defaultValue ?? Guid.Empty));
			}
			else if (t.IsAssignableFrom(typeof(DateTime)))
			{
				item = new DateTimeRegistryItem(name, emptyString, emptyString, emptyString, storage, (DateTime)(defaultValue ?? ZDateTime.BrettsBirthday.ToDateTime()));
			}
			else
			{
				throw new ArgumentException("Given type is not supported: " + t.Name);
			}

			return item;
		}

		#endregion
	}
}

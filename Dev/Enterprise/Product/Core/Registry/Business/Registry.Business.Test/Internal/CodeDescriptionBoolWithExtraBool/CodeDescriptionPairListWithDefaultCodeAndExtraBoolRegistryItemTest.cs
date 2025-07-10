using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem))]
	sealed class CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<ICodeDescriptionPairListWithDefaultCodeAndExtraBool, SystemDefinableCodeDescriptionBoolWithExtraBoolCollection>
	{
		public void TestCodeDescriptionPairList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("SMV", "Some Value");
			list.AddPair("ANV", "Another");
			var item = new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem("Name", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(list, true));

			Assert(((ICodeDescriptionPairListProvider)item).CodeDescriptionPairList.ContainsCode("SMV"));
			Assert(((ICodeDescriptionPairListProvider)item).CodeDescriptionPairList.ContainsCode("ANV"));
		}

		public void TestConstructor()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("a", "b");
			list.AddPair("x", "y");

			var item = new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem("e", (NoResString)"f", (NoResString)"g", (NoResString)"h", RegistryStorageFlags.Company, RegistryOptions.Default, new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(list, true));
			AssertEquals("Name", "e", item.Name);
			AssertEquals("Category", "f", item.Category);
			AssertEquals("Caption", "g", item.Caption);
			AssertEquals("Hint", "h", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("DataType.SystemDefinedList", list, ((CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType)item.DataType).SystemDefinedList);

			var defaultValue = (SystemDefinableCodeDescriptionBoolWithExtraBoolCollection)item.DefaultValue;
			AssertEquals("DefaultValue.Count", 2, defaultValue.Count);
			AssertEquals("DefaultValue[0].Code", "a", defaultValue[0].Code);
			AssertEquals("DefaultValue[0].SystemDefined", true, defaultValue[0].SystemDefined);
			AssertEquals("DefaultValue[1].Code", "x", defaultValue[1].Code);
			AssertEquals("DefaultValue[1].SystemDefined", true, defaultValue[1].SystemDefined);
		}

		public void TestDataType_CannotEditDefaultValue()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair("a", "b");
			list.AddPair("x", "y");
			var userDefinedList = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection();
			var element1 = userDefinedList.AddNew();
			var element2 = userDefinedList.AddNew();
			element1.Code = "123";
			element1.Bool = true;
			element2.Code = "789";

			//Default list empty
			var item = new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System, RegistryOptions.Default, new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(null, true));
			byte[] serializedList = item.DataType.Serialise(userDefinedList);

			AssertEquals("Name", "a", item.Name);
			AssertEquals("Category", "b", item.Category);
			AssertEquals("Caption", "c", item.Caption);
			AssertEquals("Hint", "d", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("DefaultValue.Count", 0, item.DefaultValue.Count);

			//Default list with 2 pairs
			item = new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem("e", (NoResString)"f", (NoResString)"g", (NoResString)"h", RegistryStorageFlags.Company, RegistryOptions.Default, new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(list, true));
			AssertEquals("Name", "e", item.Name);
			AssertEquals("Category", "f", item.Category);
			AssertEquals("Caption", "g", item.Caption);
			AssertEquals("Hint", "h", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("DataType.SystemDefinedList", list, ((CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType)item.DataType).SystemDefinedList);
			var defaultValue = (SystemDefinableCodeDescriptionBoolWithExtraBoolCollection)item.DefaultValue;
			AssertEquals("DefaultValue.Count", 2, defaultValue.Count);
			AssertEquals("DefaultValue[0].Code", "a", defaultValue[0].Code);
			AssertEquals("DefaultValue[0].SystemDefined", true, defaultValue[0].SystemDefined);
			AssertEquals("DefaultValue[1].Code", "x", defaultValue[1].Code);
			AssertEquals("DefaultValue[1].SystemDefined", true, defaultValue[1].SystemDefined);

			//Default list with pairs + 2 deserilised pairs
			var collection = (SystemDefinableCodeDescriptionBoolWithExtraBoolCollection)item.DataType.Deserialise(serializedList);
			AssertEquals("DefaultValue.Count", 4, collection.Count);
			AssertEquals("DefaultValue[0].Code", "a", collection[0].Code);
			AssertEquals("DefaultValue[0].SystemDefined", true, collection[0].SystemDefined);
			AssertEquals("DefaultValue[1].Code", "x", collection[1].Code);
			AssertEquals("DefaultValue[1].SystemDefined", true, collection[1].SystemDefined);
			AssertEquals("DefaultValue[2].Code", "123", collection[2].Code);
			AssertEquals("DefaultValue[2].SystemDefined", false, collection[2].SystemDefined);
			AssertEquals("DefaultValue[3].Code", "789", collection[3].Code);
			AssertEquals("DefaultValue[3].SystemDefined", false, collection[3].SystemDefined);
		}

		public void TestSecondBoolColumnDefaultValues()
		{
			var pairList = new CodeDescriptionPairList();
			pairList.AddPair("CODE1", "Code 1");
			pairList.AddPair("CODE2", "Code 2");
			pairList.AddPair("CODE3", "Code 3");

			CombineAssertions(() =>
			{
				Test(new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(pairList, true));
				Test(new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(pairList, true, "CODE1"));
				Test(new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(pairList, true, "CODE2", "CODE3"));
				Test(new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(pairList, true, "CODE2", "CODE3", "CODE1"));
				Test(new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(pairList, true, "CODE1", "CODE2", "CODE3", "CODE4"));
			});

			void Test(CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType registryDataType)
			{
				// Arrange
				var registryItem = new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem("e", (NoResString)"f", (NoResString)"g", (NoResString)"h", RegistryStorageFlags.Company, RegistryOptions.Default,
					registryDataType);
				var collection = (SystemDefinableCodeDescriptionBoolWithExtraBoolCollection)registryItem.DefaultValue;

				// Act
				var result = collection
					.Cast<SystemDefinableCodeDescriptionBoolWithExtraBool>()
					.Where(b => b.Bool2)
					.Select(b => b.Code.ToString());

				// Assert
				AssertContainsExactElementsInAnyOrder(registryDataType.ExtraBoolCheckedCodes, result);
			}
		}

		public void TestRegistryOptions()
		{
			CombineAssertions(() =>
			{
				Test(RegistryOptions.Default);
				Test(RegistryOptions.IsHidden);
				Test(RegistryOptions.IsOnlyEditableBySupportIfHosted | RegistryOptions.IsPasswordVisibleForControllerUser);
			});

			void Test(RegistryOptions registryOptions)
			{
				// Arrange
				var registryItem = new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem("Name", null, null, null, RegistryStorageFlags.System, registryOptions, new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(null, true));

				// Act
				var result = registryItem.Options;

				// Assert
				AssertEquals(registryOptions, result);
			}
		}

		public void TestValuesCanBeAdded()
		{
			var pairList = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("ABC", "ABC Description"),
				new CodeDescriptionPair("XYZ", "XYZ Description"),
			};

			CombineAssertions(() =>
			{
				Test(true);
				Test(false);
			});

			void Test(bool expected)
			{
				// Arrange
				var registryItem = new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem("Name", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(pairList, true),
					expected);
				var collection = (SystemDefinableCodeDescriptionBoolWithExtraBoolCollection)registryItem.Value;

				// Act
				var result = collection.AllowNew;

				// Assert
				AssertEquals(expected, result);
			}
		}

		public void TestValuesCanBeAddedByDefault()
		{
			// Arrange
			var pairList = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("ABC", "ABC Description"),
				new CodeDescriptionPair("XYZ", "XYZ Description"),
			};
			var registryItem = new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem("Name", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(pairList, true));
			var collection = (SystemDefinableCodeDescriptionBoolWithExtraBoolCollection)registryItem.Value;

			// Act
			var result = collection.AllowNew;

			// Assert
			AssertEquals(true, result);
		}

		protected override StronglyTypedRegistryItem<ICodeDescriptionPairListWithDefaultCodeAndExtraBool, SystemDefinableCodeDescriptionBoolWithExtraBoolCollection> GetNewRegistryItem()
		{
			return new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new CodeDescriptionPairListWithDefaultCodeAndExtraBoolRegistryDataType(null, true));
		}

		protected override SystemDefinableCodeDescriptionBoolWithExtraBoolCollection ValidValue
		{
			get
			{
				var collection = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection();
				var element = collection.AddNew();
				element.Code = "x";
				element.Bool = true;
				return collection;
			}
		}
	}
}

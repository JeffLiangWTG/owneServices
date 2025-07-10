using System.Linq;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionPairListWithDefaultCodeRegistryItem))]
	sealed class CodeDescriptionPairListWithDefaultCodeRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<ICodeDescriptionPairListWithDefaultCode, SystemDefinableCodeDescriptionBoolCollection>
	{
		public void TestCodeDescriptionPairList()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("SMV", "Some Value");
			list.AddPair("ANV", "Another");
			CodeDescriptionPairListWithDefaultCodeRegistryItem item = new CodeDescriptionPairListWithDefaultCodeRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.System, list, true);

			Assert(((ICodeDescriptionPairListProvider)item).CodeDescriptionPairList.ContainsCode("SMV"));
			Assert(((ICodeDescriptionPairListProvider)item).CodeDescriptionPairList.ContainsCode("ANV"));
		}

		public void TestConstructor()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("a", "b");
			list.AddPair("x", "y");

			CodeDescriptionPairListWithDefaultCodeRegistryItem item = new CodeDescriptionPairListWithDefaultCodeRegistryItem("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System, true);
			AssertEquals("Name", "a", item.Name);
			AssertEquals("Category", "b", item.Category);
			AssertEquals("Caption", "c", item.Caption);
			AssertEquals("Hint", "d", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("DefaultValue.Count", 0, item.DefaultValue.Count);

			item = new CodeDescriptionPairListWithDefaultCodeRegistryItem("e", (NoResString)"f", (NoResString)"g", (NoResString)"h", RegistryStorageFlags.Company, list, true);
			AssertEquals("Name", "e", item.Name);
			AssertEquals("Category", "f", item.Category);
			AssertEquals("Caption", "g", item.Caption);
			AssertEquals("Hint", "h", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("DataType.SystemDefinedList", list, ((CodeDescriptionPairListWithDefaultCodeRegistryDataType)item.DataType).SystemDefinedList);

			SystemDefinableCodeDescriptionBoolCollection defaultValue = (SystemDefinableCodeDescriptionBoolCollection)item.DefaultValue;
			AssertEquals("DefaultValue.Count", 2, defaultValue.Count);
			AssertEquals("DefaultValue[0].Code", "a", defaultValue[0].Code);
			AssertEquals("DefaultValue[0].SystemDefined", true, defaultValue[0].SystemDefined);
			AssertEquals("DefaultValue[1].Code", "x", defaultValue[1].Code);
			AssertEquals("DefaultValue[1].SystemDefined", true, defaultValue[1].SystemDefined);

			list = new CodeDescriptionPairList();
			list.AddPair("1", "2");
			list.AddPair("8", "9");

			item = new CodeDescriptionPairListWithDefaultCodeRegistryItem("i", (NoResString)"j", (NoResString)"k", (NoResString)"l", RegistryStorageFlags.Branch, list, true, true);
			AssertEquals("Name", "i", item.Name);
			AssertEquals("Category", "j", item.Category);
			AssertEquals("Caption", "k", item.Caption);
			AssertEquals("Hint", "l", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Branch, item.Storage);
			AssertNull("DataType.SystemDefinedList", ((CodeDescriptionPairListWithDefaultCodeRegistryDataType)item.DataType).SystemDefinedList);

			defaultValue = (SystemDefinableCodeDescriptionBoolCollection)item.DefaultValue;
			AssertEquals("DefaultValue.Count", 2, defaultValue.Count);
			AssertEquals("DefaultValue[0].Code", "1", defaultValue[0].Code);
			AssertEquals("DefaultValue[0].SystemDefined", false, defaultValue[0].SystemDefined);
			AssertEquals("DefaultValue[1].Code", "8", defaultValue[1].Code);
			AssertEquals("DefaultValue[1].SystemDefined", false, defaultValue[1].SystemDefined);

			item = new CodeDescriptionPairListWithDefaultCodeRegistryItem("e", (NoResString)"f", (NoResString)"g", (NoResString)"h", RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport, list, true);
			AssertEquals("Name", "e", item.Name);
			AssertEquals("Category", "f", item.Category);
			AssertEquals("Caption", "g", item.Caption);
			AssertEquals("Hint", "h", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertEquals("DataType.SystemDefinedList", list, ((CodeDescriptionPairListWithDefaultCodeRegistryDataType)item.DataType).SystemDefinedList);

			item = new CodeDescriptionPairListWithDefaultCodeRegistryItem("i", (NoResString)"j", (NoResString)"k", (NoResString)"l", RegistryStorageFlags.Branch, RegistryOptions.IsOnlyForSupport, list, true, true);
			AssertEquals("Name", "i", item.Name);
			AssertEquals("Category", "j", item.Category);
			AssertEquals("Caption", "k", item.Caption);
			AssertEquals("Hint", "l", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Branch, item.Storage);
			AssertEquals("Options", RegistryOptions.IsOnlyForSupport, item.Options);
			AssertNull("DataType.SystemDefinedList", ((CodeDescriptionPairListWithDefaultCodeRegistryDataType)item.DataType).SystemDefinedList);

			item = new CodeDescriptionPairListWithDefaultCodeRegistryItem("m", new[] { (NoResString)"n", (NoResString)"o" }, (NoResString)"p", (NoResString)"q", RegistryStorageFlags.Branch, list, true, true, 0);
			AssertEquals("Name", "m", item.Name);
			AssertContainsExactElementsInAnyOrder("Category", new[] { (NoResString)"n", (NoResString)"o" }, item.Categories);
			AssertEquals("Caption", "p", item.Caption);
			AssertEquals("Hint", "q", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Branch, item.Storage);
			AssertNull("DataType.SystemDefinedList", ((CodeDescriptionPairListWithDefaultCodeRegistryDataType)item.DataType).SystemDefinedList);
		}

		public void TestDataType_CannotEditDefaultValue()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("a", "b");
			list.AddPair("x", "y");
			SystemDefinableCodeDescriptionBoolCollection userDefinedList = new SystemDefinableCodeDescriptionBoolCollection();
			SystemDefinableCodeDescriptionBool element1 = userDefinedList.AddNew();
			SystemDefinableCodeDescriptionBool element2 = userDefinedList.AddNew();
			element1.Code = "123";
			element1.Description = (NoResString)"123 Description";
			element1.Bool = true;
			element2.Code = "789";
			element2.Description = (NoResString)"789 Description";

			//Default list empty
			CodeDescriptionPairListWithDefaultCodeRegistryItem item = new CodeDescriptionPairListWithDefaultCodeRegistryItem("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System, true);
			byte[] serializedList = item.DataType.Serialise(userDefinedList);

			AssertEquals("Name", "a", item.Name);
			AssertEquals("Category", "b", item.Category);
			AssertEquals("Caption", "c", item.Caption);
			AssertEquals("Hint", "d", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.System, item.Storage);
			AssertEquals("DefaultValue.Count", 0, item.DefaultValue.Count);

			//Default list with 2 pairs
			item = new CodeDescriptionPairListWithDefaultCodeRegistryItem("e", (NoResString)"f", (NoResString)"g", (NoResString)"h", RegistryStorageFlags.Company, list, true);
			AssertEquals("Name", "e", item.Name);
			AssertEquals("Category", "f", item.Category);
			AssertEquals("Caption", "g", item.Caption);
			AssertEquals("Hint", "h", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, item.Storage);
			AssertEquals("DataType.SystemDefinedList", list, ((CodeDescriptionPairListWithDefaultCodeRegistryDataType)item.DataType).SystemDefinedList);
			SystemDefinableCodeDescriptionBoolCollection defaultValue = (SystemDefinableCodeDescriptionBoolCollection)item.DefaultValue;
			AssertEquals("DefaultValue.Count", 2, defaultValue.Count);
			AssertEquals("DefaultValue[0].Code", "a", defaultValue[0].Code);
			AssertEquals("DefaultValue[0].SystemDefined", true, defaultValue[0].SystemDefined);
			AssertEquals("DefaultValue[1].Code", "x", defaultValue[1].Code);
			AssertEquals("DefaultValue[1].SystemDefined", true, defaultValue[1].SystemDefined);

			//Default list with pairs + 2 deserilised pairs
			SystemDefinableCodeDescriptionBoolCollection collection = (SystemDefinableCodeDescriptionBoolCollection)item.DataType.Deserialise(serializedList);
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

		public void TestDataType_CanEditDefaultValue()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("a", "b");
			list.AddPair("x", "y");
			SystemDefinableCodeDescriptionBoolCollection userDefinedList = new SystemDefinableCodeDescriptionBoolCollection();
			SystemDefinableCodeDescriptionBool element1 = userDefinedList.AddNew();
			SystemDefinableCodeDescriptionBool element2 = userDefinedList.AddNew();
			element1.Code = "123";
			element1.Description = (NoResString)"123 Description";
			element1.Bool = true;
			element2.Code = "789";
			element2.Description = (NoResString)"789 Description";

			//Default list with 2 pairs
			CodeDescriptionPairListWithDefaultCodeRegistryItem item = new CodeDescriptionPairListWithDefaultCodeRegistryItem("e", (NoResString)"f", (NoResString)"g", (NoResString)"h", RegistryStorageFlags.Company, list, true, true);
			byte[] serializedList = item.DataType.Serialise(userDefinedList);
			AssertEquals("Name", "e", item.Name);
			AssertEquals("Category", "f", item.Category);
			AssertEquals("Caption", "g", item.Caption);
			AssertEquals("Hint", "h", item.Hint);
			AssertEquals("Storage", RegistryStorageFlags.Company, item.Storage);
			AssertNull("DataType.SystemDefinedList", ((CodeDescriptionPairListWithDefaultCodeRegistryDataType)item.DataType).SystemDefinedList);
			SystemDefinableCodeDescriptionBoolCollection defaultValue = (SystemDefinableCodeDescriptionBoolCollection)item.DefaultValue;
			AssertEquals("DefaultValue.Count", 2, defaultValue.Count);
			AssertEquals("DefaultValue[0].Code", "a", defaultValue[0].Code);
			AssertEquals("DefaultValue[0].SystemDefined", false, defaultValue[0].SystemDefined);
			AssertEquals("DefaultValue[1].Code", "x", defaultValue[1].Code);
			AssertEquals("DefaultValue[1].SystemDefined", false, defaultValue[1].SystemDefined);

			//2 deserilised pairs
			SystemDefinableCodeDescriptionBoolCollection collection = (SystemDefinableCodeDescriptionBoolCollection)item.DataType.Deserialise(serializedList);
			AssertEquals("DefaultValue.Count", 2, collection.Count);
			AssertEquals("DefaultValue[0].Code", "123", collection[0].Code);
			AssertEquals("DefaultValue[0].SystemDefined", false, collection[0].SystemDefined);
			AssertEquals("DefaultValue[1].Code", "789", collection[1].Code);
			AssertEquals("DefaultValue[1].SystemDefined", false, collection[1].SystemDefined);
		}

		public void TestGetCaptionsInEnglish()
		{
			using (IMockResourceStringCache chsMockData = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var descriptionBool1 = new SystemDefinableCodeDescriptionBool
				{
					Description = ResString._GetMultilingualString(555, "1", "One")
				};
				var descriptionBool2 = new SystemDefinableCodeDescriptionBool
				{
					Description = ResString._GetMultilingualString(555, "2", "Two")
				};

				chsMockData.Put("1", new ResourceStringData("1", "一"));
				chsMockData.Put("2", new ResourceStringData("2", "二"));

				var list = new CodeDescriptionPairList { descriptionBool1, descriptionBool2 };
				var registryItem = new CodeDescriptionPairListWithDefaultCodeRegistryItem("e", (NoResString)"f", (NoResString)"g",
					(NoResString)"h", RegistryStorageFlags.Company, list, true);

				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
				{
					var captions = registryItem.GetCaptions(registryItem.DefaultValue).ToList();
					AssertEquals("One", captions[0]);
					AssertEquals("Two", captions[1]);
				}
			}
		}

		protected override StronglyTypedRegistryItem<ICodeDescriptionPairListWithDefaultCode, SystemDefinableCodeDescriptionBoolCollection> GetNewRegistryItem()
		{
			return new CodeDescriptionPairListWithDefaultCodeRegistryItem("", null, null, null, RegistryStorageFlags.System, true);
		}

		protected override SystemDefinableCodeDescriptionBoolCollection ValidValue
		{
			get
			{
				SystemDefinableCodeDescriptionBoolCollection collection = new SystemDefinableCodeDescriptionBoolCollection();
				SystemDefinableCodeDescriptionBool element = collection.AddNew();
				element.Code = "x";
				element.Description = (NoResString)"y";
				element.Bool = true;
				return collection;
			}
		}
	}
}

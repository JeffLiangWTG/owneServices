using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(OptionalTemplateSheetCollection))]
	sealed class OptionalTemplateSheetCollectionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new OptionalTemplateSheetCollection(Factory, new ValidatorPack());
		}

		public void TestAddContainsCountAndClear()
		{
			var collection = new OptionalTemplateSheetCollection(new BusinessObjectFactory(), new ValidatorPack());
			collection.Add("Sheet1");
			AssertEquals("Should Contain sheet1", 1, collection.Count);
			AssertEquals("Should Contain sheet1", true, collection.Contains("Sheet1"));

			collection.Add("Sheet2");
			AssertEquals("Should Contain sheet2", 2, collection.Count);
			AssertEquals("Should Contain sheet2", true, collection.Contains("Sheet1"));

			collection.Add("Sheet1");
			AssertEquals("Should not add 1 again", 2, collection.Count);

			collection.Clear();
			AssertEquals("Count should be 0", 0, collection.Count);
			AssertEquals("Should not Contain sheet1", false, collection.Contains("Sheet1"));
			AssertEquals("Should not Contain sheet2", false, collection.Contains("Sheet2"));
		}

		public void TestIndexer()
		{
			var collection = new OptionalTemplateSheetCollection(new BusinessObjectFactory(), new ValidatorPack());
			collection.Add("Sheet1");
			AssertEquals("Should be able to use Sheet1 as index", "Sheet1", collection["Sheet1"].Name);
			AssertNull("Should not be able to use Sheet2 as index", collection["Sheet2"]);
		}

		public void TestAtLeastOneOfGroupValidation()
		{
			var collection = new OptionalTemplateSheetCollection(new BusinessObjectFactory(), new ValidatorPack());
			collection.Add("Sheet1");
			collection.Add("Sheet2");
			foreach (OptionalTemplateSheet sheet in collection)
			{
				sheet.Validate();
				AssertEquals("Sheet is not selected so should have error", true, sheet.SelectedInfo.HasError("At least one of the 'Sheet1' and 'Sheet2' should have data."));
			}

			collection["Sheet1"].Selected = true;
			foreach (OptionalTemplateSheet sheet in collection)
			{
				sheet.Validate();
				AssertEquals("Sheet1 is selected so there should be no errors", false, sheet.SelectedInfo.HasErrors());
			}
		}

		public void TestAddedSheetRegisteredAsEditable()
		{
			var collection = new OptionalTemplateSheetCollection(new BusinessObjectFactory(), new ValidatorPack());
			collection.Add("Sheet1");
			AssertEquals("Sheed should be registered on collection as editable", true, collection.IsRegisteredEditableChildObject(collection["Sheet1"]));
		}

		public void TestAddSheetSetsOrder()
		{
			var collection = new OptionalTemplateSheetCollection(new BusinessObjectFactory(), new ValidatorPack());
			collection.Add("c");
			collection.Add("d");
			collection.Add("a");
			collection.Add("b");

			AssertEquals(0, collection["c"].Order);
			AssertEquals(1, collection["d"].Order);
			AssertEquals(2, collection["a"].Order);
			AssertEquals(3, collection["b"].Order);
		}

		public void TestJsonConverter()
		{
			var collection = new OptionalTemplateSheetCollection(new BusinessObjectFactory(), new ValidatorPack());
			collection.Add("a");
			collection.Add("b");
			collection["b"].Selected = true;

			var result = JsonConverterHelper.Serialize(collection);
			var deserialisedCollection = JsonConverterHelper.Deserialize<OptionalTemplateSheetCollection>(result);

			AssertEquals("Should Contain a", true, deserialisedCollection.Contains("a"));
			AssertEquals("Should Contain b", true, deserialisedCollection.Contains("b"));
			AssertEquals("a should not be selected", false, deserialisedCollection["a"].Selected);
			AssertEquals("b should be selected", true, deserialisedCollection["b"].Selected);
		}
	}
}

using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Testing
{
	public abstract class CodeDescriptionBoolWithExtraBoolCollectionAbstractTest<T> : CodeDescriptionBoolCollectionAbstractTest<T> where T : CodeDescriptionBoolWithExtraBoolCollection
	{
		public void TestIsSystemDefined()
		{
			CodeDescriptionPairList systemdDefinedList = new CodeDescriptionPairList();
			systemdDefinedList.AddPair("ABC", "ABC Description");
			systemdDefinedList.AddPair("XYZ", "XYZ Description");

			var collection = new CodeDescriptionBoolWithExtraBoolCollection(systemdDefinedList, false, 3);
			AssertEquals("Collection[0].Code", "ABC", collection[0].Code);
			AssertEquals("Collection[0].Description", "ABC Description", collection[0].Description);
			AssertEquals("Collection[0].SystemDefined", true, collection[0].SystemDefined);
			AssertEquals("Collection[0].Bool2", false, collection[0].Bool2);

			AssertEquals("Collection[1].Code", "XYZ", collection[1].Code);
			AssertEquals("Collection[1].Description", "XYZ Description", collection[1].Description);
			AssertEquals("Collection[1].SystemDefined", true, collection[1].SystemDefined);
			AssertEquals("Collection[1].Bool2", false, collection[1].Bool2);

			var newElement = collection.AddNew();
			Assert("SystemDefined should be false on a new element", !newElement.SystemDefined);
		}

		public override void TestSerialiseAndDeserialise()
		{
			IRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(Collection.GetType());

			var element1 = Collection.AddNew();
			var element2 = Collection.AddNew();
			var element3 = Collection.AddNew();
			var element4 = Collection.AddNew();

			element1.Code = "a";
			element2.Code = "b";
			element3.Code = "c";
			element4.Code = "d";

			element1.Description = (NoResString)"a Description";
			element2.Description = (NoResString)"b Description";
			element3.Description = (NoResString)"c Description";
			element4.Description = (NoResString)"d Description";

			element1.SystemDefined = false;
			element2.SystemDefined = false;
			element3.SystemDefined = true;
			element4.SystemDefined = true;

			element1.Bool2 = false;
			element2.Bool2 = true;
			element3.Bool2 = false;
			element4.Bool2 = true;

			byte[] bytes = dataType.Serialise(Collection);

			var deserialisedCollection = (CodeDescriptionBoolWithExtraBoolCollection)dataType.Deserialise(bytes);
			AssertEquals("Count", 4, deserialisedCollection.Count);

			AssertEquals("[0].Code", "a", deserialisedCollection[0].Code);
			AssertEquals("[0].Description", "a Description", deserialisedCollection[0].Description);
			AssertEquals("[0].SystemDefined", false, deserialisedCollection[0].SystemDefined);
			AssertEquals("[0].Bool2", false, deserialisedCollection[0].Bool2);

			AssertEquals("[1].Code", "b", deserialisedCollection[1].Code);
			AssertEquals("[1].Description", "b Description", deserialisedCollection[1].Description);
			AssertEquals("[1].SystemDefined", false, deserialisedCollection[1].SystemDefined);
			AssertEquals("[1].Bool2", true, deserialisedCollection[1].Bool2);

			AssertEquals("[2].Code", "c", deserialisedCollection[2].Code);
			AssertEquals("[2].Description", "c Description", deserialisedCollection[2].Description);
			AssertEquals("[2].SystemDefined", false, deserialisedCollection[2].SystemDefined);
			AssertEquals("[2].Bool2", false, deserialisedCollection[2].Bool2);

			AssertEquals("[3].Code", "d", deserialisedCollection[3].Code);
			AssertEquals("[3].Description", "d Description", deserialisedCollection[3].Description);
			AssertEquals("[3].SystemDefined", false, deserialisedCollection[3].SystemDefined);
			AssertEquals("[3].Bool2", true, deserialisedCollection[3].Bool2);
		}

		public override void TestDefaultBoolForNewChild()
		{
			var element = Collection.AddNew();
			AssertEquals("AddNew().Bool", false, element.Bool);
			AssertEquals("AddNew().Bool2", false, element.Bool2);
		}

		public void TestGetCodeFromDescription()
		{
			var element1 = Collection.AddNew();
			var element2 = Collection.AddNew();
			element1.Code = "x";
			element1.Description = (NoResString)"xd";
			element2.Code = "y";
			element2.Description = (NoResString)"yd";
			AssertEquals("GetCodeFromDescription(\"xd\")", "x", Collection.GetCodeFromDescription("xd"));
			AssertEquals("GetCodeFromDescription(\"yd\")", "y", Collection.GetCodeFromDescription("yd"));
			AssertEquals("GetCodeFromDescription(\"zd\")", "", Collection.GetCodeFromDescription("zd"));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionBoolWithExtraBool();
		}
	}
}

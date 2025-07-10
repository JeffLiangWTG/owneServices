using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SystemDefinableCodeDescriptionBoolWithExtraBoolCollection))]
	sealed class SystemDefinableCodeDescriptionBoolWithExtraBoolCollectionTest : CodeDescriptionBoolCollectionAbstractTest<SystemDefinableCodeDescriptionBoolWithExtraBoolCollection>
	{
		public void TestPopulate()
		{
			CodeDescriptionPairList systemdDefinedList = new CodeDescriptionPairList();
			systemdDefinedList.AddPair("ABC", "ABC Description");
			systemdDefinedList.AddPair("XYZ", "XYZ Description");

			SystemDefinableCodeDescriptionBoolWithExtraBoolCollection userDefinedList = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection();
			SystemDefinableCodeDescriptionBoolWithExtraBool element1 = userDefinedList.AddNew();
			SystemDefinableCodeDescriptionBoolWithExtraBool element2 = userDefinedList.AddNew();
			element1.Code = "123";
			element1.Description = (NoResString)"123 Description";
			element1.Bool = true;
			element2.Code = "789";
			element2.Description = (NoResString)"789 Description";
			element2.Bool2 = true;

			IRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(SystemDefinableCodeDescriptionBoolWithExtraBoolCollection));
			byte[] serialisedUserDefinedList = dataType.Serialise(userDefinedList);

			SystemDefinableCodeDescriptionBoolWithExtraBoolCollection collection = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(3, systemdDefinedList, false);
			collection.Populate(serialisedUserDefinedList);

			AssertEquals("Count", 4, collection.Count);
			AssertEquals("DefaultCode", "123", collection.DefaultCode);

			AssertEquals("Collection[0].Code", "ABC", collection[0].Code);
			AssertEquals("Collection[0].Description", "ABC Description", collection[0].Description);
			AssertEquals("Collection[0].SystemDefined", true, collection[0].SystemDefined);
			AssertEquals("Collection[0].Bool2", false, collection[0].Bool2);

			AssertEquals("Collection[1].Code", "XYZ", collection[1].Code);
			AssertEquals("Collection[1].Description", "XYZ Description", collection[1].Description);
			AssertEquals("Collection[1].SystemDefined", true, collection[1].SystemDefined);
			AssertEquals("Collection[1].Bool2", false, collection[1].Bool2);

			AssertEquals("Collection[2].Code", "123", collection[2].Code);
			AssertEquals("Collection[2].Description", "123 Description", collection[2].Description);
			AssertEquals("Collection[2].SystemDefined", false, collection[2].SystemDefined);
			AssertEquals("Collection[2].Bool2", false, collection[2].Bool2);

			AssertEquals("Collection[3].Code", "789", collection[3].Code);
			AssertEquals("Collection[3].Description", "789 Description", collection[3].Description);
			AssertEquals("Collection[3].SystemDefined", false, collection[3].SystemDefined);
			AssertEquals("Collection[3].Bool2", true, collection[3].Bool2);
		}

		public void TestIsSystemDefined()
		{
			CodeDescriptionPairList systemdDefinedList = new CodeDescriptionPairList();
			systemdDefinedList.AddPair("ABC", "ABC Description");
			systemdDefinedList.AddPair("XYZ", "XYZ Description");

			SystemDefinableCodeDescriptionBoolWithExtraBoolCollection collection = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(3, systemdDefinedList, false);
			AssertEquals("Collection[0].Code", "ABC", collection[0].Code);
			AssertEquals("Collection[0].Description", "ABC Description", collection[0].Description);
			AssertEquals("Collection[0].SystemDefined", true, collection[0].SystemDefined);
			AssertEquals("Collection[0].Bool2", false, collection[0].Bool2);

			AssertEquals("Collection[1].Code", "XYZ", collection[1].Code);
			AssertEquals("Collection[1].Description", "XYZ Description", collection[1].Description);
			AssertEquals("Collection[1].SystemDefined", true, collection[1].SystemDefined);
			AssertEquals("Collection[1].Bool2", false, collection[1].Bool2);

			SystemDefinableCodeDescriptionBoolWithExtraBool newElement = collection.AddNew();
			Assert("SystemDefined should be false on a new element", !newElement.SystemDefined);

			collection = new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(3, systemdDefinedList, true);
			AssertEquals("Collection[0].Code", "ABC", collection[0].Code);
			AssertEquals("Collection[0].Description", "ABC Description", collection[0].Description);
			AssertEquals("Collection[0].SystemDefined", false, collection[0].SystemDefined);
			AssertEquals("Collection[0].Bool2", false, collection[0].Bool2);

			AssertEquals("Collection[1].Code", "XYZ", collection[1].Code);
			AssertEquals("Collection[1].Description", "XYZ Description", collection[1].Description);
			AssertEquals("Collection[1].SystemDefined", false, collection[1].SystemDefined);
			AssertEquals("Collection[1].Bool2", false, collection[1].Bool2);

			newElement = collection.AddNew();
			Assert("SystemDefined should be false on a new element", !newElement.SystemDefined);
		}

		public void TestAddSetsDefaultElement()
		{
			SystemDefinableCodeDescriptionBoolWithExtraBool element1 = new SystemDefinableCodeDescriptionBoolWithExtraBool();
			SystemDefinableCodeDescriptionBoolWithExtraBool element2 = new SystemDefinableCodeDescriptionBoolWithExtraBool();
			SystemDefinableCodeDescriptionBoolWithExtraBool element3 = new SystemDefinableCodeDescriptionBoolWithExtraBool();
			SystemDefinableCodeDescriptionBoolWithExtraBool element4 = new SystemDefinableCodeDescriptionBoolWithExtraBool();

			element1.Code = "";
			element2.Code = "x";
			element3.Code = "y";
			element4.Code = "y";

			Collection.SetDefaultCode("", false);
			Collection.Add(element1);
			Collection.Add(element2);
			Collection.Add(element3);
			Collection.Add(element4);
			AssertNull("DefaultElement", Collection.DefaultElement);

			Collection.SetDefaultCode("y", false);
			Collection.Add(element1);
			Collection.Add(element2);
			Collection.Add(element3);
			Collection.Add(element4);
			AssertEquals("DefaultElement", element3, Collection.DefaultElement);
		}

		public override void TestSerialiseAndDeserialise()
		{
			IRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(SystemDefinableCodeDescriptionBoolWithExtraBoolCollection));

			SystemDefinableCodeDescriptionBoolWithExtraBool element1 = Collection.AddNew();
			SystemDefinableCodeDescriptionBoolWithExtraBool element2 = Collection.AddNew();
			SystemDefinableCodeDescriptionBoolWithExtraBool element3 = Collection.AddNew();
			SystemDefinableCodeDescriptionBoolWithExtraBool element4 = Collection.AddNew();

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

			Collection.SetDefaultCode("123", false);

			byte[] bytes = dataType.Serialise(Collection);
			SystemDefinableCodeDescriptionBoolWithExtraBoolCollection deserialisedCollection = (SystemDefinableCodeDescriptionBoolWithExtraBoolCollection)dataType.Deserialise(bytes);
			AssertEquals("Count", 2, deserialisedCollection.Count);
			AssertEquals("DefaultCode", "123", deserialisedCollection.DefaultCode);

			AssertEquals("[0].Code", "a", deserialisedCollection[0].Code);
			AssertEquals("[0].Description", "a Description", deserialisedCollection[0].Description);
			AssertEquals("[0].SystemDefined", false, deserialisedCollection[0].SystemDefined);
			AssertEquals("[0].Bool2", false, deserialisedCollection[0].Bool2);

			AssertEquals("[1].Code", "b", deserialisedCollection[1].Code);
			AssertEquals("[1].Description", "b Description", deserialisedCollection[1].Description);
			AssertEquals("[1].SystemDefined", false, deserialisedCollection[1].SystemDefined);
			AssertEquals("[1].Bool2", true, deserialisedCollection[1].Bool2);
		}

		public void TestSetDefaultCode()
		{
			SystemDefinableCodeDescriptionBoolWithExtraBool element = Collection.AddNew();
			Collection.DefaultElement = element;

			Collection.SetDefaultCode("x", false);
			AssertEquals("DefaultCode", "x", Collection.DefaultCode);
			AssertEquals("DefaultElement", element, Collection.DefaultElement);

			Collection.SetDefaultCode("y", true);
			AssertEquals("DefaultCode", "y", Collection.DefaultCode);
			AssertNull("DefaultElement", Collection.DefaultElement);
		}

		public void TestDefaultElement()
		{
			SystemDefinableCodeDescriptionBoolWithExtraBool element1 = Collection.AddNew();
			SystemDefinableCodeDescriptionBoolWithExtraBool element2 = Collection.AddNew();
			element1.Code = "x";
			element2.Code = "y";
			AssertNull("DefaultElement", Collection.DefaultElement);

			Collection.SetDefaultCode("y", true);
			AssertEquals("DefaultCode", "y", Collection.DefaultCode);
			AssertEquals("DefaultElement", element2, Collection.DefaultElement);

			Collection.DefaultElement = element1;
			AssertEquals("DefaultCode", "x", Collection.DefaultCode);
			AssertEquals("DefaultElement", element1, Collection.DefaultElement);
		}

		public override void TestDefaultBoolForNewChild()
		{
			SystemDefinableCodeDescriptionBoolWithExtraBool element = Collection.AddNew();
			AssertEquals("AddNew().Bool", false, element.Bool);
			AssertEquals("AddNew().Bool2", false, element.Bool2);
		}

		public void TestGetCodeFromDescription()
		{
			SystemDefinableCodeDescriptionBoolWithExtraBool element1 = Collection.AddNew();
			SystemDefinableCodeDescriptionBoolWithExtraBool element2 = Collection.AddNew();
			element1.Code = "x";
			element1.Description = (NoResString)"xd";
			element2.Code = "y";
			element2.Description = (NoResString)"yd";
			AssertEquals("GetCodeFromDescription(\"xd\")", "x", Collection.GetCodeFromDescription("xd"));
			AssertEquals("GetCodeFromDescription(\"yd\")", "y", Collection.GetCodeFromDescription("yd"));
			AssertEquals("GetCodeFromDescription(\"zd\")", "", Collection.GetCodeFromDescription("zd"));
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
				Test(true, new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(true));
				Test(false, new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(false));
				Test(true, new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(3, pairList, ZBool.False, true));
				Test(false, new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(3, pairList, ZBool.False, false));
			});

			void Test(bool expected, IBusiness collection)
			{
				// Arrange

				// Act
				var result = collection.AllowNew;

				// Assert
				AssertEquals(expected, result);
			}
		}

		public void TestValuesCanBeAddedByDefault()
		{
			var pairList = new CodeDescriptionPairList
			{
				new CodeDescriptionPair("ABC", "ABC Description"),
				new CodeDescriptionPair("XYZ", "XYZ Description"),
			};

			CombineAssertions(() =>
			{
				Test(new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection());
				Test(new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(3));
				Test(new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(3, pairList, true));
				Test(new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection(3, pairList, false));
			});

			void Test(IBusiness collection)
			{
				// Arrange

				// Act
				var result = collection.AllowNew;

				// Assert
				AssertEquals(true, result);
			}
		}

		protected override SystemDefinableCodeDescriptionBoolWithExtraBoolCollection GetCollectionToTest()
		{
			return new SystemDefinableCodeDescriptionBoolWithExtraBoolCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SystemDefinableCodeDescriptionBoolWithExtraBool();
		}
	}
}

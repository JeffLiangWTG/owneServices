using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SystemDefinableCodeDescriptionBoolCollection))]
	sealed class SystemDefinableCodeDescriptionBoolCollectionTest : CodeDescriptionBoolCollectionAbstractTest<SystemDefinableCodeDescriptionBoolCollection>
	{
		#region TestPopulate

		public void TestPopulate()
		{
			CodeDescriptionPairList systemdDefinedList = new CodeDescriptionPairList();
			systemdDefinedList.AddPair("ABC", "ABC Description");
			systemdDefinedList.AddPair("XYZ", "XYZ Description");

			SystemDefinableCodeDescriptionBoolCollection userDefinedList = new SystemDefinableCodeDescriptionBoolCollection();
			SystemDefinableCodeDescriptionBool element1 = userDefinedList.AddNew();
			SystemDefinableCodeDescriptionBool element2 = userDefinedList.AddNew();
			element1.Code = "123";
			element1.Description = (NoResString)"123 Description";
			element1.Bool = true;
			element2.Code = "789";
			element2.Description = (NoResString)"789 Description";

			IRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(SystemDefinableCodeDescriptionBoolCollection));
			byte[] serialisedUserDefinedList = dataType.Serialise(userDefinedList);

			SystemDefinableCodeDescriptionBoolCollection collection = new SystemDefinableCodeDescriptionBoolCollection(3, systemdDefinedList, false);
			collection.Populate(serialisedUserDefinedList);

			AssertEquals("Count", 4, collection.Count);
			AssertEquals("DefaultCode", "123", collection.DefaultCode);

			AssertEquals("Collection[0].Code", "ABC", collection[0].Code);
			AssertEquals("Collection[0].Description", "ABC Description", collection[0].Description);
			AssertEquals("Collection[0].SystemDefined", true, collection[0].SystemDefined);

			AssertEquals("Collection[1].Code", "XYZ", collection[1].Code);
			AssertEquals("Collection[1].Description", "XYZ Description", collection[1].Description);
			AssertEquals("Collection[1].SystemDefined", true, collection[1].SystemDefined);

			AssertEquals("Collection[2].Code", "123", collection[2].Code);
			AssertEquals("Collection[2].Description", "123 Description", collection[2].Description);
			AssertEquals("Collection[2].SystemDefined", false, collection[2].SystemDefined);

			AssertEquals("Collection[3].Code", "789", collection[3].Code);
			AssertEquals("Collection[3].Description", "789 Description", collection[3].Description);
			AssertEquals("Collection[3].SystemDefined", false, collection[3].SystemDefined);
		}

		#endregion

		#region TestIsSystemDefined

		public void TestIsSystemDefined()
		{
			CodeDescriptionPairList systemdDefinedList = new CodeDescriptionPairList();
			systemdDefinedList.AddPair("ABC", "ABC Description");
			systemdDefinedList.AddPair("XYZ", "XYZ Description");

			SystemDefinableCodeDescriptionBoolCollection collection = new SystemDefinableCodeDescriptionBoolCollection(3, systemdDefinedList, false);
			AssertEquals("Collection[0].Code", "ABC", collection[0].Code);
			AssertEquals("Collection[0].Description", "ABC Description", collection[0].Description);
			AssertEquals("Collection[0].SystemDefined", true, collection[0].SystemDefined);

			AssertEquals("Collection[1].Code", "XYZ", collection[1].Code);
			AssertEquals("Collection[1].Description", "XYZ Description", collection[1].Description);
			AssertEquals("Collection[1].SystemDefined", true, collection[1].SystemDefined);

			SystemDefinableCodeDescriptionBool newElement = collection.AddNew();
			Assert("SystemDefined should be false on a new element", !newElement.SystemDefined);

			collection = new SystemDefinableCodeDescriptionBoolCollection(3, systemdDefinedList, true);
			AssertEquals("Collection[0].Code", "ABC", collection[0].Code);
			AssertEquals("Collection[0].Description", "ABC Description", collection[0].Description);
			AssertEquals("Collection[0].SystemDefined", false, collection[0].SystemDefined);

			AssertEquals("Collection[1].Code", "XYZ", collection[1].Code);
			AssertEquals("Collection[1].Description", "XYZ Description", collection[1].Description);
			AssertEquals("Collection[1].SystemDefined", false, collection[1].SystemDefined);

			newElement = collection.AddNew();
			Assert("SystemDefined should be false on a new element", !newElement.SystemDefined);
		}

		#endregion

		#region TestAddSetsDefaultElement

		public void TestAddSetsDefaultElement()
		{
			SystemDefinableCodeDescriptionBool element1 = new SystemDefinableCodeDescriptionBool();
			SystemDefinableCodeDescriptionBool element2 = new SystemDefinableCodeDescriptionBool();
			SystemDefinableCodeDescriptionBool element3 = new SystemDefinableCodeDescriptionBool();
			SystemDefinableCodeDescriptionBool element4 = new SystemDefinableCodeDescriptionBool();

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

		#endregion

		#region TestSerialiseAndDeserialise

		public override void TestSerialiseAndDeserialise()
		{
			IRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(SystemDefinableCodeDescriptionBoolCollection));

			SystemDefinableCodeDescriptionBool element1 = Collection.AddNew();
			SystemDefinableCodeDescriptionBool element2 = Collection.AddNew();
			SystemDefinableCodeDescriptionBool element3 = Collection.AddNew();
			SystemDefinableCodeDescriptionBool element4 = Collection.AddNew();

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

			Collection.SetDefaultCode("123", false);

			byte[] bytes = dataType.Serialise(Collection);
			SystemDefinableCodeDescriptionBoolCollection deserialisedCollection = (SystemDefinableCodeDescriptionBoolCollection)dataType.Deserialise(bytes);
			AssertEquals("Count", 2, deserialisedCollection.Count);
			AssertEquals("DefaultCode", "123", deserialisedCollection.DefaultCode);

			AssertEquals("[0].Code", "a", deserialisedCollection[0].Code);
			AssertEquals("[0].Description", "a Description", deserialisedCollection[0].Description);
			AssertEquals("[0].SystemDefined", false, deserialisedCollection[0].SystemDefined);

			AssertEquals("[1].Code", "b", deserialisedCollection[1].Code);
			AssertEquals("[1].Description", "b Description", deserialisedCollection[1].Description);
			AssertEquals("[1].SystemDefined", false, deserialisedCollection[1].SystemDefined);
		}

		#endregion

		#region TestSetDefaultcode

		public void TestSetDefaultcode()
		{
			SystemDefinableCodeDescriptionBool element = Collection.AddNew();
			Collection.DefaultElement = element;

			Collection.SetDefaultCode("x", false);
			AssertEquals("DefaultCode", "x", Collection.DefaultCode);
			AssertEquals("DefaultElement", element, Collection.DefaultElement);

			Collection.SetDefaultCode("y", true);
			AssertEquals("DefaultCode", "y", Collection.DefaultCode);
			AssertNull("DefaultElement", Collection.DefaultElement);
		}

		#endregion

		#region TestDefaultElement

		public void TestDefaultElement()
		{
			SystemDefinableCodeDescriptionBool element1 = Collection.AddNew();
			SystemDefinableCodeDescriptionBool element2 = Collection.AddNew();
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

		#endregion

		#region TestDefaultBoolForNewChild

		public override void TestDefaultBoolForNewChild()
		{
			SystemDefinableCodeDescriptionBool element = Collection.AddNew();
			AssertEquals("AddNew().Bool", false, element.Bool);
		}

		#endregion

		#region TestGetCodeFromDescription

		public void TestGetCodeFromDescription()
		{
			SystemDefinableCodeDescriptionBool element1 = Collection.AddNew();
			SystemDefinableCodeDescriptionBool element2 = Collection.AddNew();
			element1.Code = "x";
			element1.Description = (NoResString)"xd";
			element2.Code = "y";
			element2.Description = (NoResString)"yd";
			AssertEquals("GetCodeFromDescription(\"xd\")", "x", Collection.GetCodeFromDescription("xd"));
			AssertEquals("GetCodeFromDescription(\"yd\")", "y", Collection.GetCodeFromDescription("yd"));
			AssertEquals("GetCodeFromDescription(\"zd\")", "", Collection.GetCodeFromDescription("zd"));
		}

		#endregion

		#region TestDefaultColumnReadOnly

		public void TestDefaultColumnReadOnly()
		{
			var systemDefinedList = new CodeDescriptionPairList();
			var abcCodeDescription = new CodeDescriptionBoolDefaultReadonly() { Code = "ABC", Description = (NoResString)"ABC Description" };
			var xyzCodeDescription = new CodeDescriptionBoolDefaultReadonly() { Code = "XYZ", Description = (NoResString)"XYZ Description", DefaultColumnReadOnly = true };
			systemDefinedList.Add(abcCodeDescription);
			systemDefinedList.Add(xyzCodeDescription);

			var collection = new SystemDefinableCodeDescriptionBoolCollection(3, systemDefinedList, true);
			AssertEquals("Collection[0].DefaultColumnReadOnly", false, collection[0].DefaultColumnReadOnly);
			AssertEquals("Collection[1].DefaultColumnReadOnly", true, collection[1].DefaultColumnReadOnly);

			var newElement = collection.AddNew();
			AssertEquals("DefaultColumnReadOnly should be false on a new element.", false, collection[2].DefaultColumnReadOnly);

			newElement.DefaultColumnReadOnly = true;
			AssertEquals("DefaultColumnReadOnly should be true.", true, collection[2].DefaultColumnReadOnly);
		}

		#endregion

		public void TestGetCodeDescriptionPairListWithDefaultCode()
		{
			string expectedCode = "QWE";
			Collection.SetDefaultCode(expectedCode, false);
			var pairList = Collection.GetCodeDescriptionPairList();
			AssertEquals("CodeDescriptionPairList generated from the collection should keep DefaultCode as well.", expectedCode, pairList.DefaultCode);
		}

		protected override SystemDefinableCodeDescriptionBoolCollection GetCollectionToTest()
		{
			return new SystemDefinableCodeDescriptionBoolCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SystemDefinableCodeDescriptionBool();
		}
	}
}

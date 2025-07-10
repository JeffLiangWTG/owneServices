using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithGroupCollection))]
	sealed class CodeDescriptionWithGroupCollectionTest : CodeDescriptionWithGroupCollectionAbstractTest<CodeDescriptionWithGroupCollection>
	{
		protected override CodeDescriptionWithGroupCollection GetCollectionToTest()
		{
			var groupLookup = new CodeDescriptionPairList();
			groupLookup.AddPair("ABC", "Group 1");
			groupLookup.AddPair("CDE", "Group 2");
			groupLookup.AddPair("EFG", "Group 3");

			return new CodeDescriptionWithGroupCollection(groupLookup, "ABC");
		}

		public void TestSerialiseAndDeserialise()
		{
			IRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(CodeDescriptionWithGroupCollection));

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

			element1.Group = "ABC";
			element2.Group = "CDE";
			element3.Group = "EFG";
			element4.Group = "ABC";

			element1.SystemDefined = false;
			element2.SystemDefined = false;
			element3.SystemDefined = true;
			element4.SystemDefined = true;

			byte[] bytes = dataType.Serialise(Collection);
			var deserialisedCollection = (CodeDescriptionWithGroupCollection)dataType.Deserialise(bytes);
			AssertEquals("Count", 4, deserialisedCollection.Count);

			AssertEquals("[0].Code", "a", deserialisedCollection[0].Code);
			AssertEquals("[0].Description", "a Description", deserialisedCollection[0].Description);
			AssertEquals("[0].Group", "ABC", deserialisedCollection[0].Group);
			AssertEquals("[0].SystemDefined", false, deserialisedCollection[0].SystemDefined);

			AssertEquals("[1].Code", "b", deserialisedCollection[1].Code);
			AssertEquals("[1].Description", "b Description", deserialisedCollection[1].Description);
			AssertEquals("[1].Group", "CDE", deserialisedCollection[1].Group);
			AssertEquals("[1].SystemDefined", false, deserialisedCollection[1].SystemDefined);

			AssertEquals("[2].Code", "c", deserialisedCollection[2].Code);
			AssertEquals("[2].Description", "c Description", deserialisedCollection[2].Description);
			AssertEquals("[2].Group", "EFG", deserialisedCollection[2].Group);
			AssertEquals("[2].SystemDefined", true, deserialisedCollection[2].SystemDefined);

			AssertEquals("[3].Code", "d", deserialisedCollection[3].Code);
			AssertEquals("[3].Description", "d Description", deserialisedCollection[3].Description);
			AssertEquals("[3].Group", "ABC", deserialisedCollection[3].Group);
			AssertEquals("[3].SystemDefined", true, deserialisedCollection[3].SystemDefined);
		}
	}
}

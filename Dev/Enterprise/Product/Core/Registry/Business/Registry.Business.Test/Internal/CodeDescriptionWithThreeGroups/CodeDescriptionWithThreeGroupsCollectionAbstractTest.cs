using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithThreeGroupsCollection))]
	public abstract class CodeDescriptionWithThreeGroupsCollectionAbstractTest<T> : RegistryBusinessObjectCollectionTestCase<CodeDescriptionWithThreeGroupsCollection> where T : CodeDescriptionWithThreeGroupsCollection
	{
		public virtual void TestDefaultGroupForNewChild()
		{
			var collectionType = Collection.GetType();

			var groupLookup = new CodeDescriptionPairList();
			groupLookup.AddPair("ABC", "Group 1");
			groupLookup.AddPair("CDE", "Group 2");
			groupLookup.AddPair("EFG", "Group 3");
			groupLookup.AddPair("NA", "Not applicable");
			var group2Lookup = new CodeDescriptionPairList();
			group2Lookup.AddPair("123", "Group 1");
			group2Lookup.AddPair("456", "Group 4");
			group2Lookup.AddPair("789", "Group 7");
			group2Lookup.AddPair("NA", "Not applicable");
			var group3Lookup = new CodeDescriptionPairList();
			group3Lookup.AddPair("Z1", "Group z");
			group3Lookup.AddPair("Y2", "Group y");
			group3Lookup.AddPair("X3", "Group x");
			group3Lookup.AddPair("NA", "Not applicable");

			groupLookup.DefaultCode = "ABC";
			group2Lookup.DefaultCode = "123";
			group3Lookup.DefaultCode = "Z1";
			var collection1 = (CodeDescriptionWithThreeGroupsCollection)Activator.CreateInstance(collectionType, new object[] { groupLookup, group2Lookup, group3Lookup, 17 });

			AssertEquals("Collection1.AddNew().Group", "ABC", collection1.AddNew().Group);
			AssertEquals("Collection1.AddNew().Group2", "123", collection1.AddNew().Group2);
			AssertEquals("Collection1.AddNew().Group3", "Z1", collection1.AddNew().Group3);

			var clone1 = (CodeDescriptionWithThreeGroupsCollection)collection1.Clone(null, Factory);

			AssertEquals("Collection1.AddNew().Group", "ABC", clone1.AddNew().Group);
			AssertEquals("Collection1.AddNew().Group2", "123", clone1.AddNew().Group2);
		}

		public void TestGetThreeGroupsFromCode()
		{
			var element1 = Collection.AddNew();
			var element2 = Collection.AddNew();

			element1.Code = "ABC";
			element1.Group = "CDE";
			element1.Group2 = "DEF";
			element1.Group3 = "EFG";

			element2.Code = "XYZ";
			element2.Group = "EFG";
			element2.Group2 = "FGH";
			element2.Group3 = "GHI";

			AssertEquals("GetThreeGroupsFromCode(\"ABC\")", Tuple.Create<string, string, string>("CDE", "DEF", "EFG"), Collection.GetThreeGroupsFromCode("ABC"));
			AssertEquals("GetThreeGroupsFromCode(\"XYZ\")", Tuple.Create<string, string, string>("EFG", "FGH", "GHI"), Collection.GetThreeGroupsFromCode("XYZ"));
			AssertEquals("GetThreeGroupsFromCode(\"!@#\")", Tuple.Create<string, string, string>("", "", ""), Collection.GetThreeGroupsFromCode("!@#"));
		}

		public void TestICodeDescriptionPairListMembers()
		{
			var element1 = Collection.AddNew();
			var element2 = Collection.AddNew();

			element1.Code = "ABC";
			element2.Code = "XYZ";

			var list = (ICodeDescriptionWithThreeGroupsList)Collection;

			AssertEquals("List[0]", element1, list[0]);
			AssertEquals("List[1]", element2, list[1]);

			var pairList = (ICodeDescriptionPairList)Collection;

			AssertEquals("PairList.ContainsCode(\"ABC\")", true, pairList.ContainsCode("ABC"));
			AssertEquals("PairList.ContainsCode(\"XYZ\")", true, pairList.ContainsCode("XYZ"));
			AssertEquals("PairList.ContainsCode(\"!@#\")", false, pairList.ContainsCode("!@#"));
		}

		public virtual void TestSerialiseAndDeserialise()
		{
			if (GetCollectionToTest().GetType() != typeof(CodeDescriptionWithThreeGroupsCollection))
			{ Assert(true); return; }

			IRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(CodeDescriptionWithThreeGroupsCollection));

			var element1 = Collection.AddNew();
			var element2 = Collection.AddNew();
			var element3 = Collection.AddNew();
			var element4 = Collection.AddNew();

			element1.Code = "a";
			element1.Group = "ABC";
			element1.Group2 = "123";
			element1.Group3 = "X3";
			element1.SystemDefined = false;

			element2.Code = "b";
			element2.Group = "";
			element2.Group2 = "123";
			element2.Group3 = "Z1";
			element2.SystemDefined = false;

			element3.Code = "c";
			element3.Group = "EFG";
			element3.Group2 = "789";
			element3.Group3 = "NA";
			element3.SystemDefined = true;

			element4.Code = "d";
			element4.Group = "NA";
			element4.Group2 = "NA";
			element4.Group3 = "Y2";
			element4.SystemDefined = true;

			byte[] bytes = dataType.Serialise(Collection);
			var deserialisedCollection = (CodeDescriptionWithThreeGroupsCollection)dataType.Deserialise(bytes);
			AssertEquals("Count", 4, deserialisedCollection.Count);

			AssertAllProperties("a", "ABC", "123", "X3", false, deserialisedCollection[0]);
			AssertAllProperties("b", "", "123", "Z1", false, deserialisedCollection[1]);
			AssertAllProperties("c", "EFG", "789", "NA", true, deserialisedCollection[2]);
			AssertAllProperties("d", "NA", "NA", "Y2", true, deserialisedCollection[3]);
		}

		void AssertAllProperties(string code, string group, string group2, string group3, bool systemDefined,
			CodeDescriptionWithThreeGroups codeDescriptionWithThreeGroups)
		{
			AssertEquals("Code", code, codeDescriptionWithThreeGroups.Code);
			AssertEquals("Group", group, codeDescriptionWithThreeGroups.Group);
			AssertEquals("Group2", group2, codeDescriptionWithThreeGroups.Group2);
			AssertEquals("Group3", group3, codeDescriptionWithThreeGroups.Group3);
			AssertEquals("SystemDefined", systemDefined, codeDescriptionWithThreeGroups.SystemDefined);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CodeDescriptionWithThreeGroups();
		}

		#endregion
	}
}

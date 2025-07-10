using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business.Testing
{
	public abstract class CodeDescriptionBoolCollectionAbstractTest<T> : RegistryBusinessObjectCollectionTestCase<T> where T : CodeDescriptionBoolCollection
	{
		public virtual void TestDefaultBoolForNewChild()
		{
			Type collectionType = Collection.GetType();

			ReadOnlyCodeDescriptionPairList list = new ReadOnlyCodeDescriptionPairList();
			CodeDescriptionBoolCollection collection1 = (CodeDescriptionBoolCollection)Activator.CreateInstance(collectionType, new object[] { list, true });
			CodeDescriptionBoolCollection collection2 = (CodeDescriptionBoolCollection)Activator.CreateInstance(collectionType, new object[] { list, false });
			CodeDescriptionBoolCollection collection3 = (CodeDescriptionBoolCollection)Activator.CreateInstance(collectionType, new object[] { list });

			AssertEquals("Collection1.AddNew().Bool", true, collection1.AddNew().Bool);
			AssertEquals("Collection2.AddNew().Bool", false, collection2.AddNew().Bool);
			AssertEquals("Collection3.AddNew().Bool", false, collection3.AddNew().Bool);

			CodeDescriptionBoolCollection clone1 = (CodeDescriptionBoolCollection)collection1.Clone(null, Factory);
			CodeDescriptionBoolCollection clone2 = (CodeDescriptionBoolCollection)collection2.Clone(null, Factory);
			CodeDescriptionBoolCollection clone3 = (CodeDescriptionBoolCollection)collection3.Clone(null, Factory);

			AssertEquals("Clone1.AddNew().Bool", true, clone1.AddNew().Bool);
			AssertEquals("Clone2.AddNew().Bool", false, clone2.AddNew().Bool);
			AssertEquals("Clone3.AddNew().Bool", false, clone3.AddNew().Bool);
		}

		public void TestGetBoolAndDescriptionFromCode()
		{
			CodeDescriptionBool element1 = Collection.AddNew();
			CodeDescriptionBool element2 = Collection.AddNew();

			element1.Code = "ABC";
			element1.Description = (NoResString)"ABC Description";
			element1.Bool = true;

			element2.Code = "XYZ";
			element2.Description = (NoResString)"XYZ Description";
			element2.Bool = false;

			AssertEquals("GetDescriptionFromCode(\"ABC\")", "ABC Description", Collection.GetDescriptionFromCode("ABC"));
			AssertEquals("GetBoolFromCode(\"ABC\")", true, Collection.GetBoolFromCode("ABC"));

			AssertEquals("GetDescriptionFromCode(\"XYZ\")", "XYZ Description", Collection.GetDescriptionFromCode("XYZ"));
			AssertEquals("GetBoolFromCode(\"XYZ\")", false, Collection.GetBoolFromCode("XYZ"));

			AssertEquals("GetDescriptionFromCode(\"!@#\")", "", Collection.GetDescriptionFromCode("!@#"));
			AssertEquals("GetBoolFromCode(\"!@#\")", false, Collection.GetBoolFromCode("!@#"));
		}

		public void TestInterfaceMembers()
		{
			CodeDescriptionBool element1 = Collection.AddNew();
			CodeDescriptionBool element2 = Collection.AddNew();

			element1.Code = "ABC";
			element2.Code = "XYZ";

			ICodeDescriptionBoolList boolList = Collection;

			AssertEquals("BoolList[0]", element1, boolList[0]);
			AssertEquals("BoolList[1]", element2, boolList[1]);

			ICodeDescriptionPairList pairList = Collection;

			AssertEquals("PairList.ContainsCode(\"ABC\")", true, pairList.ContainsCode("ABC"));
			AssertEquals("PairList.ContainsCode(\"XYZ\")", true, pairList.ContainsCode("XYZ"));
			AssertEquals("PairList.ContainsCode(\"!@#\")", false, pairList.ContainsCode("!@#"));
		}

		public void TestGetActiveCodeDescriptionPairList()
		{
			CodeDescriptionBoolCollection collection = new CodeDescriptionBoolCollection();
			CodeDescriptionBool element1 = collection.AddNew();
			element1.Code = "AAA";
			element1.Description = (NoResString)"Desc A";
			element1.Bool = true;
			CodeDescriptionBool element2 = collection.AddNew();
			element2.Code = "BBB";
			element2.Description = (NoResString)"Desc B";
			element2.Bool = false;
			CodeDescriptionBool element3 = collection.AddNew();
			element3.Code = "CCC";
			element3.Description = (NoResString)"Desc C";
			element3.Bool = true;

			CodeDescriptionPairList activeList = collection.GetActiveCodeDescriptionPairList();
			AssertEquals(2, activeList.Count);
			AssertEquals("AAA", activeList[0].Code);
			AssertEquals("CCC", activeList[1].Code);
		}

		public virtual void TestSerialiseAndDeserialise()
		{
			//this test is only for CodeDescriptionBool/CodeDescriptionBoolCollection.
			if (GetCollectionToTest().GetType() != typeof(CodeDescriptionBoolCollection))
			{ Assert(true); return; }

			IRegistryDataType dataType = new DummyNonPersistentBusinessObjectRegistryDataType(typeof(CodeDescriptionBoolCollection));

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

			byte[] bytes = dataType.Serialise(Collection);
			var deserialisedCollection = (CodeDescriptionBoolCollection)dataType.Deserialise(bytes);
			AssertEquals("Count", 4, deserialisedCollection.Count);

			AssertEquals("[0].Code", "a", deserialisedCollection[0].Code);
			AssertEquals("[0].Description", "a Description", deserialisedCollection[0].Description);
			AssertEquals("[0].SystemDefined", false, deserialisedCollection[0].SystemDefined);

			AssertEquals("[1].Code", "b", deserialisedCollection[1].Code);
			AssertEquals("[1].Description", "b Description", deserialisedCollection[1].Description);
			AssertEquals("[1].SystemDefined", false, deserialisedCollection[1].SystemDefined);

			AssertEquals("[2].Code", "c", deserialisedCollection[2].Code);
			AssertEquals("[2].Description", "c Description", deserialisedCollection[2].Description);
			AssertEquals("[2].SystemDefined", true, deserialisedCollection[2].SystemDefined);

			AssertEquals("[3].Code", "d", deserialisedCollection[3].Code);
			AssertEquals("[3].Description", "d Description", deserialisedCollection[3].Description);
			AssertEquals("[3].SystemDefined", true, deserialisedCollection[3].SystemDefined);
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
			return new CodeDescriptionBool();
		}

		#endregion
	}
}

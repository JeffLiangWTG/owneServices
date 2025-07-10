using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CodeDescriptionWithGroupCollection))]
	public abstract class CodeDescriptionWithGroupCollectionAbstractTest<T> : RegistryBusinessObjectCollectionTestCase<T> where T : CodeDescriptionWithGroupCollection
	{
		public virtual void TestDefaultGroupForNewChild()
		{
			var collectionType = Collection.GetType();

			var groupLookup = new CodeDescriptionPairList();
			groupLookup.AddPair("ABC", "Group 1");
			groupLookup.AddPair("CDE", "Group 2");
			groupLookup.AddPair("EFG", "Group 3");

			var collection1 = (T)Activator.CreateInstance(collectionType, new object[] { groupLookup, "ABC" });
			var collection2 = (T)Activator.CreateInstance(collectionType, new object[] { groupLookup, "CDE" });
			var collection3 = (T)Activator.CreateInstance(collectionType, new object[] { groupLookup, "" });

			AssertEquals("Collection1.AddNew().Group", "ABC", collection1.AddNew().Group);
			AssertEquals("Collection2.AddNew().Group", "CDE", collection2.AddNew().Group);
			AssertEquals("Collection3.AddNew().Group", "", collection3.AddNew().Group);

			var clone1 = (T)collection1.Clone(null, Factory);
			var clone2 = (T)collection2.Clone(null, Factory);
			var clone3 = (T)collection3.Clone(null, Factory);

			AssertEquals("Collection1.AddNew().Group", "ABC", clone1.AddNew().Group);
			AssertEquals("Collection2.AddNew().Group", "CDE", clone2.AddNew().Group);
			AssertEquals("Collection3.AddNew().Group", "", clone3.AddNew().Group);
		}

		public void TestGetGroupFromCode()
		{
			var element1 = Collection.AddNew();
			var element2 = Collection.AddNew();

			element1.Code = "ABC";
			element1.Description = (NoResString)"ABC Description";
			element1.Group = "CDE";

			element2.Code = "XYZ";
			element2.Description = (NoResString)"XYZ Description";
			element2.Group = "EFG";

			AssertEquals("GetGroupFromCode(\"ABC\")", "CDE", Collection.GetGroupFromCode("ABC"));
			AssertEquals("GetGroupFromCode(\"XYZ\")", "EFG", Collection.GetGroupFromCode("XYZ"));
			AssertEquals("GetGroupFromCode(\"!@#\")", "", Collection.GetGroupFromCode("!@#"));
		}

		public void TestICodeDescriptionPairListMembers()
		{
			var element1 = Collection.AddNew();
			var element2 = Collection.AddNew();

			element1.Code = "ABC";
			element2.Code = "XYZ";

			var list = (ICodeDescriptionWithGroupList)Collection;

			AssertEquals("List[0]", element1, list[0]);
			AssertEquals("List[1]", element2, list[1]);

			var pairList = (ICodeDescriptionPairList)Collection;

			AssertEquals("PairList.ContainsCode(\"ABC\")", true, pairList.ContainsCode("ABC"));
			AssertEquals("PairList.ContainsCode(\"XYZ\")", true, pairList.ContainsCode("XYZ"));
			AssertEquals("PairList.ContainsCode(\"!@#\")", false, pairList.ContainsCode("!@#"));
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
			return new CodeDescriptionWithGroup();
		}

		protected new T Collection
		{
			get { return base.Collection; }
		}

		#endregion
	}
}

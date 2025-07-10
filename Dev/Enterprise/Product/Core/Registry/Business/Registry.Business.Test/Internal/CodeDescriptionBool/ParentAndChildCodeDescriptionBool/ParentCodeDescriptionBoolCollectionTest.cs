using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ParentCodeDescriptionBoolCollection))]
	sealed class ParentCodeDescriptionBoolCollectionTest : CodeDescriptionBoolCollectionAbstractTest<ParentCodeDescriptionBoolCollection>
	{
		public void TestGetChildList()
		{
			ParentCodeDescriptionBool parent1 = Collection.AddNew();
			ParentCodeDescriptionBool parent2 = Collection.AddNew();
			ParentCodeDescriptionBool parent3 = Collection.AddNew();

			parent1.Code = "P1";
			parent2.Code = "P2";
			parent3.Code = "P3";

			CodeDescriptionBool child1A = parent1.ChildList.AddNew();
			CodeDescriptionBool child1B = parent1.ChildList.AddNew();
			CodeDescriptionBool child1C = parent1.ChildList.AddNew();
			child1A.Code = "C1";
			child1B.Code = "C2";
			child1C.Code = "C3";

			CodeDescriptionBool child2A = parent2.ChildList.AddNew();
			CodeDescriptionBool child2B = parent2.ChildList.AddNew();
			child2A.Code = "C3";
			child2B.Code = "C4";

			ICodeDescriptionBoolList list1 = Collection.GetChildList("P1");
			AssertEquals("GetChildList(\"P1\").Count", 3, list1.Count);
			AssertEquals("GetChildList(\"P1\")[0].Code", "C1", list1[0].Code);
			AssertEquals("GetChildList(\"P1\")[1].Code", "C2", list1[1].Code);
			AssertEquals("GetChildList(\"P1\")[2].Code", "C3", list1[2].Code);

			ICodeDescriptionBoolList list2 = Collection.GetChildList("P2");
			AssertEquals("GetChildList(\"P2\").Count", 2, list2.Count);
			AssertEquals("GetChildList(\"P2\")[0].Code", "C3", list2[0].Code);
			AssertEquals("GetChildList(\"P2\")[1].Code", "C4", list2[1].Code);

			ICodeDescriptionBoolList list3 = Collection.GetChildList("P3");
			AssertEquals("GetChildList(\"P3\").Count", 1, list3.Count);
			AssertEquals("GetChildList(\"P3\")[0].Code", "UDF", list3[0].Code);
			AssertEquals("GetChildList(\"P3\")[0].Description", "Undefined", list3[0].Description);

			ICodeDescriptionBoolList list4 = Collection.GetChildList("P4");
			AssertEquals("GetChildList(\"P4\").Count", 1, list4.Count);
			AssertEquals("GetChildList(\"P4\")[0].Code", "UDF", list4[0].Code);
			AssertEquals("GetChildList(\"P4\")[0].Description", "Undefined", list4[0].Description);
		}

		#region Implementation

		protected override ParentCodeDescriptionBoolCollection GetCollectionToTest()
		{
			return new ParentCodeDescriptionBoolCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ParentCodeDescriptionBool();
		}

		#endregion
	}
}

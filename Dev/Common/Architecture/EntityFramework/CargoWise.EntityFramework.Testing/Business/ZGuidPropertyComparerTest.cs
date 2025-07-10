namespace CargoWise.EntityFramework.Testing
{
	using System.Collections;
	using System.ComponentModel;
	using System.Data;
	using Enterprise.ZArchitecture.Core;
	using NUnit.Framework;

	sealed class ZGuidPropertyComparerTest : TestCaseWithDummy
	{
		public void TestCompare()
		{
			DummyBusinessObject dummy1 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			dummy1.Z0_Guid = dummy1.PK;
			dummy1.Z0_Code = "AAA";

			DummyBusinessObject dummy2 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			dummy2.Z0_Guid = dummy2.PK;
			dummy2.Z0_Code = "BBB";

			DummyBusinessObject dummy3 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			dummy3.Z0_Guid = dummy3.PK;
			dummy3.Z0_Code = "CCC";

			dummy1.Collection.Add(dummy1);
			dummy1.Collection.Add(dummy2);
			dummy1.Collection.Add(dummy3);
			dummy2.Collection.Add(dummy1);
			dummy2.Collection.Add(dummy2);
			dummy2.Collection.Add(dummy3);
			dummy3.Collection.Add(dummy1);
			dummy3.Collection.Add(dummy2);
			dummy3.Collection.Add(dummy3);

			ZGuidPropertyComparer ascComparer = new ZGuidPropertyComparer(Dummy.Z0_GuidInfo.PropertyDescriptor, ListSortDirection.Ascending, "Collection");
			AssertEquals("Dummy1 before Dummy2", -1, ascComparer.Compare(dummy1, dummy2));
			AssertEquals("Dummy1 before Dummy3", -1, ascComparer.Compare(dummy1, dummy3));
			AssertEquals("Dummy2 before Dummy3", -1, ascComparer.Compare(dummy2, dummy3));
			AssertEquals("Dummy1 same as Dummy1", 0, ascComparer.Compare(dummy1, dummy1));
			AssertEquals("Dummy2 same as Dummy2", 0, ascComparer.Compare(dummy2, dummy2));
			AssertEquals("Dummy3 same as Dummy3", 0, ascComparer.Compare(dummy3, dummy3));
			AssertEquals("Dummy3 after Dummy2", 1, ascComparer.Compare(dummy3, dummy2));
			AssertEquals("Dummy3 after Dummy1", 1, ascComparer.Compare(dummy3, dummy1));
			AssertEquals("Dummy2 after Dummy1", 1, ascComparer.Compare(dummy2, dummy1));

			ZGuidPropertyComparer descComparer = new ZGuidPropertyComparer(Dummy.Z0_GuidInfo.PropertyDescriptor, ListSortDirection.Descending, "Collection");
			AssertEquals("Dummy1 after Dummy2", 1, descComparer.Compare(dummy1, dummy2));
			AssertEquals("Dummy1 after Dummy3", 1, descComparer.Compare(dummy1, dummy3));
			AssertEquals("Dummy2 after Dummy3", 1, descComparer.Compare(dummy2, dummy3));
			AssertEquals("Dummy1 same as Dummy1", 0, descComparer.Compare(dummy1, dummy1));
			AssertEquals("Dummy2 same as Dummy2", 0, descComparer.Compare(dummy2, dummy2));
			AssertEquals("Dummy3 same as Dummy3", 0, descComparer.Compare(dummy3, dummy3));
			AssertEquals("Dummy3 before Dummy2", -1, descComparer.Compare(dummy3, dummy2));
			AssertEquals("Dummy3 before Dummy1", -1, descComparer.Compare(dummy3, dummy1));
			AssertEquals("Dummy2 before Dummy1", -1, descComparer.Compare(dummy2, dummy1));
		}

		public void TestCompare_WithMultiTypeFields()
		{
			DummyBusinessObject dummy1 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			dummy1.Z0_Description = dummy1.PK.ToString();
			dummy1.Z0_Code = "AAA";

			DummyBusinessObject dummy2 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			dummy2.Z0_Description = "BBB";
			dummy2.Z0_Code = "BBB";

			DummyBusinessObject dummy3 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			dummy3.Z0_Description = dummy3.PK.ToString();
			dummy3.Z0_Code = "CCC";

			dummy1.Collection.Add(dummy1);
			dummy1.Collection.Add(dummy2);
			dummy1.Collection.Add(dummy3);
			dummy2.Collection.Add(dummy1);
			dummy2.Collection.Add(dummy2);
			dummy2.Collection.Add(dummy3);
			dummy3.Collection.Add(dummy1);
			dummy3.Collection.Add(dummy2);
			dummy3.Collection.Add(dummy3);

			ZGuidPropertyComparer ascComparer = new ZGuidPropertyComparer(Dummy.Z0_DescriptionInfo.PropertyDescriptor, ListSortDirection.Ascending, "Collection");
			AssertEquals("Dummy1 before Dummy2", -1, ascComparer.Compare(dummy1, dummy2));
			AssertEquals("Dummy1 before Dummy3", -1, ascComparer.Compare(dummy1, dummy3));
			AssertEquals("Dummy2 before Dummy3", -1, ascComparer.Compare(dummy2, dummy3));
			AssertEquals("Dummy1 same as Dummy1", 0, ascComparer.Compare(dummy1, dummy1));
			AssertEquals("Dummy2 same as Dummy2", 0, ascComparer.Compare(dummy2, dummy2));
			AssertEquals("Dummy3 same as Dummy3", 0, ascComparer.Compare(dummy3, dummy3));
			AssertEquals("Dummy3 after Dummy2", 1, ascComparer.Compare(dummy3, dummy2));
			AssertEquals("Dummy3 after Dummy1", 1, ascComparer.Compare(dummy3, dummy1));
			AssertEquals("Dummy2 after Dummy1", 1, ascComparer.Compare(dummy2, dummy1));

			ZGuidPropertyComparer descComparer = new ZGuidPropertyComparer(Dummy.Z0_DescriptionInfo.PropertyDescriptor, ListSortDirection.Descending, "Collection");
			AssertEquals("Dummy1 after Dummy2", 1, descComparer.Compare(dummy1, dummy2));
			AssertEquals("Dummy1 after Dummy3", 1, descComparer.Compare(dummy1, dummy3));
			AssertEquals("Dummy2 after Dummy3", 1, descComparer.Compare(dummy2, dummy3));
			AssertEquals("Dummy1 same as Dummy1", 0, descComparer.Compare(dummy1, dummy1));
			AssertEquals("Dummy2 same as Dummy2", 0, descComparer.Compare(dummy2, dummy2));
			AssertEquals("Dummy3 same as Dummy3", 0, descComparer.Compare(dummy3, dummy3));
			AssertEquals("Dummy3 before Dummy2", -1, descComparer.Compare(dummy3, dummy2));
			AssertEquals("Dummy3 before Dummy1", -1, descComparer.Compare(dummy3, dummy1));
			AssertEquals("Dummy2 before Dummy1", -1, descComparer.Compare(dummy2, dummy1));
		}

		[ExpectNoExceptions]
		public void TestCompare_WithLhsWithFindBoxListProviderAndRhsWithout()
		{
			DummyBusinessObjectWithDynamicList dummy1 = (DummyBusinessObjectWithDynamicList)Factory.New(typeof(DummyBusinessObjectWithDynamicList));
			DummyBusinessObjectWithDynamicList dummy2 = (DummyBusinessObjectWithDynamicList)Factory.New(typeof(DummyBusinessObjectWithDynamicList));
			dummy2.UseCodeDescriptionPairList = true;

			ZGuidPropertyComparer comparer = new ZGuidPropertyComparer(dummy1.Z0_VarCharMaxInfo.PropertyDescriptor, ListSortDirection.Ascending, "List");
			AssertEquals(0, comparer.Compare(dummy1, dummy2));
		}

		#region Test Classes

		class DummyBusinessObjectWithDynamicList : DummyBusinessObject
		{
			public DummyBusinessObjectWithDynamicList(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public IList List
			{
				get { return Z0_Code == "CDS" ? new CodeDescriptionPairList() : Collection; }
			}

			public bool UseCodeDescriptionPairList
			{
				get { return Z0_Code == "CDS"; }
				set { Z0_Code = "CDS"; }
			}
		}

		#endregion
	}
}

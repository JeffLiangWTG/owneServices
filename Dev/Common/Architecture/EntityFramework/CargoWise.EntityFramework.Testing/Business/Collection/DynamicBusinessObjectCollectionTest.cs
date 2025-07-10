using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DynamicBusinessObjectCollectionTest : TestCaseWithDummy
	{
		public void TestWhere()
		{
			Dummy.Z0_Number = 5;
			DummyBusinessObject dummy2 = CreateDummy("", 12);
			Factory.Save();

			DynamicBusinessObjectCollection coll = new DynamicBusinessObjectCollection(Factory);
			coll.Load("select * from dbo.DUMMYBIZO");

			AssertEquals(1, coll.Where(x => x[DummyBizoSchema.Z0_Number].Equals(new ZInt(5))).Count());
		}

		public void TestSelect()
		{
			Dummy.Z0_Number = 5;
			DummyBusinessObject dummy2 = CreateDummy("", 12);
			Factory.Save();

			DynamicBusinessObjectCollection coll = new DynamicBusinessObjectCollection(Factory);
			coll.Load("select * from dbo.DUMMYBIZO");

			AssertEquals(17, coll.Select(x => ((ZInt)x[DummyBizoSchema.Z0_Number])).Sum(x => x));
		}

		public void TestRefreshBindingIncludingChildren()
		{
			Dummy.Z0_Number = 5;
			DummyBusinessObject dummy2 = CreateDummy("", 12);
			Factory.Save();

			DynamicBusinessObjectCollection coll = new DynamicBusinessObjectCollection(Factory);
			coll.Load("select * from dbo.DUMMYBIZO");
			AssertEquals(2, coll.Count);

			bool child1OnElementFired = false;
			bool child2OnElementFired = false;
			DynamicBusinessObject testChild1 = coll[0];
			((IBusiness)testChild1).ListChanged += new ListChangedEventHandler(delegate
			{ child1OnElementFired = true; });

			DynamicBusinessObject testChild2 = coll[1];
			((IBusiness)testChild2).ListChanged += new ListChangedEventHandler(delegate
			{ child2OnElementFired = true; });

			((IBusinessObjectState)coll).RefreshBindingIncludingChildren();
			AssertEquals("child 1 element changed event fired", true, child1OnElementFired);
			AssertEquals("child 2 element changed event fired", true, child2OnElementFired);
		}

		public void TestIListWithNotifications()
		{
			Dummy.Z0_Number = 5;
			DummyBusinessObject dummy2 = CreateDummy("", 12);
			Factory.Save();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("select * from dbo.DUMMYBIZO order by Z0_Number");
			AssertEquals(2, collection.Count);

			IBusinessObjectCollection list = collection;
			AssertEquals(new ZInt(5), ((DynamicBusinessObject)list[0])[DummyBizoSchema.Z0_Number]);
			AssertEquals(new ZInt(12), ((DynamicBusinessObject)list[1])[DummyBizoSchema.Z0_Number]);

			AssertEquals(1, list.IndexOf(collection[1], 0, 2));
			AssertEquals(-1, list.IndexOf(collection[0], 1, 1));
		}

		public void TestIListIndexOf()
		{
			Dummy.Z0_Number = 99;
			var dummy1 = CreateDummy("D1", 2);
			var dummy2 = CreateDummy("D2", 5);
			var dummy3 = CreateDummy("D3", 4);
			var dummy4 = CreateDummy("D4", 1);
			Factory.Save();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("select * from dbo.DUMMYBIZO order by Z0_Number");
			AssertEquals(5, collection.Count);

			DynamicBusinessObjectCollection collection2 = new DynamicBusinessObjectCollection(Factory);
			collection2.Load("select * from dbo.DUMMYBIZO order by Z0_Description");
			AssertEquals(5, collection2.Count);

			IList list = collection;
			AssertEquals(2, list.IndexOf(collection[2]));
			AssertEquals(-1, list.IndexOf(collection2[2]));
		}

		public void TestIListContains()
		{
			Dummy.Z0_Number = 99;
			var dummy1 = CreateDummy("D1", 2);
			var dummy2 = CreateDummy("D2", 5);
			var dummy3 = CreateDummy("D3", 4);
			var dummy4 = CreateDummy("D4", 1);
			Factory.Save();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("select * from dbo.DUMMYBIZO order by Z0_Number");
			AssertEquals(5, collection.Count);

			DynamicBusinessObjectCollection collection2 = new DynamicBusinessObjectCollection(Factory);
			collection2.Load("select * from dbo.DUMMYBIZO order by Z0_Description");
			AssertEquals(5, collection2.Count);

			IList list = collection;
			Assert(list.Contains(collection[2]));
			Assert(!list.Contains(collection2[2]));
		}

		class DynamicDummyBizO : DynamicBusinessObject, IObsoleteValidation
		{
			public DynamicDummyBizO(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZGuid Z0_Guid
			{
				get { return new ZGuid(((IBusinessObjectInternals)this).Row[Z0_GuidInfo.Name]); }
			}

			public ZPropertyInfo Z0_GuidInfo
			{
				get { return GetZPropertyInfo(nameof(Z0_Guid)); }
			}

			public ZString Z0_Code
			{
				get { return new ZString(((IBusinessObjectInternals)this).Row[Z0_CodeInfo.Name]); }
			}

			public ZPropertyInfo Z0_CodeInfo
			{
				get { return GetZPropertyInfo(nameof(Z0_Code)); }
			}

			public DummyBusinessObjectCollection LookupList
			{
				get { return new DummyBusinessObjectCollection(Factory); }
			}

			public ZDateTime Z0_SmallDateTime
			{
				get { return new ZDateTime(((IBusinessObjectInternals)this).Row[Z0_SmallDateTimeInfo.Name]); }
			}

			public ZPropertyInfo Z0_SmallDateTimeInfo
			{
				get { return GetZPropertyInfo(nameof(Z0_SmallDateTime)); }
			}
		}

		public void TestGetGuidComparerForSort()
		{
			ZGuid[] bizOPks = new ZGuid[10];
			for (int i = 0; i < 10; i++)
			{
				DummyBusinessObjectInList bizO = Factory.New<DummyBusinessObjectInList>();
				bizO.Z0_Code = (9 - i).ToString();
				bizO.Z0_Guid = bizO.PK;
				bizO.LookupList.Add(bizO);
				bizOPks[i] = bizO.PK;
			}
			ZQuery query = new ZQuery(DummyBizoSchema.PK, bizOPks);

			Factory.Save();

			DynamicBusinessObjectCollection<DynamicDummyBizO> collection = new DynamicBusinessObjectCollection<DynamicDummyBizO>(Factory);
			collection.Load("select * from dbo.DUMMYBIZO where " + query.ParameterisedText.ParameterisedQueryText, query.Params);
			((IBusinessObjectCollection)collection).AddGuidListMapping(DummyBizoSchema.Z0_Guid.Name, "LookupList");

			collection.ApplySort(new SortInfo(DummyBizoSchema.Z0_Guid.Name, ListSortDirection.Ascending));

			for (int i = 0; i < 10; i++)
			{
				AssertEquals(i.ToString(), collection[i][DummyBizoSchema.Z0_Code.Name]);
			}
		}

		public void TestTimeSort()
		{
			var bizOPks = new ZGuid[3];
			var bizOs = new DummyBusinessObjectInList[3];
			for (int i = 0; i < 3; i++)
			{
				DummyBusinessObjectInList bizO = Factory.New<DummyBusinessObjectInList>();
				bizOPks[i] = bizO.PK;
				bizOs[i] = bizO;
			}
			bizOs[0].Z0_Code = "0";
			bizOs[1].Z0_Code = "1";
			bizOs[2].Z0_Code = "2";
			bizOs[0].Z0_SmallDateTime = new ZDateTime(2022, 1, 1, 0, 1, 0);
			bizOs[1].Z0_SmallDateTime = new ZDateTime(2020, 1, 1, 0, 2, 0);
			bizOs[2].Z0_SmallDateTime = new ZDateTime(2021, 1, 1, 0, 3, 0);

			var query = new ZQuery(DummyBizoSchema.PK, bizOPks);

			Factory.Save();

			var collection = new DynamicBusinessObjectCollection<DynamicDummyBizO>(Factory);
			collection.Load("select * from dbo.DUMMYBIZO where " + query.ParameterisedText.ParameterisedQueryText, query.Params);
			((IBusinessObjectCollection)collection).AddIsTime(DummyBizoSchema.Z0_SmallDateTime.Name);

			collection.ApplySort(new SortInfo(DummyBizoSchema.Z0_SmallDateTime.Name, ListSortDirection.Ascending));
			AssertEquals("0", collection[0].Z0_Code);
			AssertEquals("1", collection[1].Z0_Code);
			AssertEquals("2", collection[2].Z0_Code);

			collection.ApplySort(new SortInfo(DummyBizoSchema.Z0_SmallDateTime.Name, ListSortDirection.Descending));
			AssertEquals("2", collection[0].Z0_Code);
			AssertEquals("1", collection[1].Z0_Code);
			AssertEquals("0", collection[2].Z0_Code);
		}

		public void TestSort()
		{
			Dummy.Z0_Number = 5;
			DummyBusinessObject dummy2 = CreateDummy("", 12);
			Factory.Save();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("select * from dbo.DUMMYBIZO");
			AssertEquals(2, collection.Count);

			PropertyDescriptor prop = TypeDescriptor.GetProperties(collection[0])["Z0_Number"];
			collection.Sorter.ApplySort(prop, ListSortDirection.Ascending);
			AssertEquals(5, collection[0]["Z0_Number"]);
			AssertEquals(12, collection[1]["Z0_Number"]);
			AssertEquals(ListSortDirection.Ascending, collection.Sorter.SortDirection);
			AssertEquals("Z0_Number", collection.Sorter.SortProperty.Name);

			collection.Sorter.ApplySort(prop, ListSortDirection.Descending);
			AssertEquals(12, collection[0]["Z0_Number"]);
			AssertEquals(5, collection[1]["Z0_Number"]);
			AssertEquals(ListSortDirection.Descending, collection.Sorter.SortDirection);
			AssertEquals("Z0_Number", collection.Sorter.SortProperty.Name);

			collection.Sorter.RemoveSort();
			AssertNull(collection.Sorter.SortProperty);
		}

		public void TestToArray()
		{
			SaveTestData();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("select * from dbo.DUMMYBIZO where Z0_Number = @Num", new ZSqlParameterCollection(ZSqlParameter.New("@Num", 12, DummyBizoSchema.Z0_Number)));

			BusinessObject[] array = ((IBusinessObjectCollection)collection).ToArray();

			AssertEquals(collection.Count, array.Length);
		}

		public void TestReload()
		{
			SaveTestData();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("select * from dbo.DUMMYBIZO where Z0_Number = @Num", new ZSqlParameterCollection(ZSqlParameter.New("@Num", 12, DummyBizoSchema.Z0_Number)));

			AssertEquals("Count", 2, collection.Count);
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject newDummy = CreateDummy(factory2, "NewDummy", 12);
			DummyBusinessObject badDummy = CreateDummy(factory2, "BadDummy", 5);
			factory2.Save();

			collection.Reload();

			AssertEquals("Count", 3, collection.Count);

			foreach (DynamicBusinessObject dummy in collection)
			{
				if (dummy[AutoDummyBizo.Schema.Z0_Description].ToString() == "BadDummy")
				{
					Fail("Should not contain BadDummy");
				}
			}
		}

		public void TestManageForDataRefresh()
		{
			SaveTestData();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("select * from dbo.DUMMYBIZO where Z0_Number = @Num", new ZSqlParameterCollection(ZSqlParameter.New("@Num", 12, DummyBizoSchema.Z0_Number)));

			AssertEquals("Count", 2, collection.Count);

			collection.ManageForDataRefresh(typeof(DummyBusinessObject));

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject newDummy = CreateDummy(factory2, "NewDummy", 12);
			factory2.Save();

			AssertEquals("Count", 3, collection.Count);

			collection.ManageForDataRefresh(typeof(DummyPivot));

			DummyBusinessObject newDummy2 = CreateDummy(factory2, "NewDummy2", 12);
			factory2.Save();

			AssertEquals("Count", 3, collection.Count);

			DummyPivot newDifferentTable = factory2.New<DummyPivot>();
			factory2.Save();

			AssertEquals("Count", 4, collection.Count);
		}

		public void TestStopManagingForDataRefresh()
		{
			SaveTestData();

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("select * from dbo.DUMMYBIZO where Z0_Number = @Num", new ZSqlParameterCollection(ZSqlParameter.New("@Num", 12, DummyBizoSchema.Z0_Number)));

			AssertEquals("Count", 2, collection.Count);

			collection.ManageForDataRefresh(typeof(DummyBusinessObject));

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject newDummy = (DummyBusinessObject)factory2.New(typeof(DummyBusinessObject));
			newDummy.Z0_Description = "NewDummy";
			newDummy.Z0_Number = 12;

			factory2.Save();

			AssertEquals("Count", 3, collection.Count);

			collection.StopManagingForDataRefresh(typeof(DummyBusinessObject));

			DummyBusinessObject newDummy2 = (DummyBusinessObject)factory2.New(typeof(DummyBusinessObject));
			newDummy2.Z0_Description = "NewDummy2";
			newDummy2.Z0_Number = 12;
			factory2.Save();

			AssertEquals("Count", 3, collection.Count);
		}

		public void TestSupportChangeNotification()
		{
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			AssertEquals("SupportChangeNotification", true, ((IBindingList)collection).SupportsChangeNotification);
		}

		public void TestRemoveAll()
		{
			SaveTestData();
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("select * from dbo.DUMMYBIZO where Z0_Number = @Num", new ZSqlParameterCollection(ZSqlParameter.New("@Num", 12, DummyBizoSchema.Z0_Number)));

			AssertEquals("Precondition : Count", 2, collection.Count);
			collection.RemoveAll();
			AssertEquals("Count after clear()", 0, collection.Count);
		}

		public void TestGetItemProperties()
		{
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);

			collection.Load("select OH_PK, OH_Code from dbo.OrgHeader");
			AssertNotNull("Should have the property for OH_Code", ((ITypedList)collection).GetItemProperties(null)["OH_Code"]);

			collection.Load("select OH_PK, OH_FullName from dbo.OrgHeader");
			AssertNull("Should not have the non-existant property for OH_Code", ((ITypedList)collection).GetItemProperties(null)["OH_Code"]);
			AssertNotNull("Should have the property for OH_FullName", ((ITypedList)collection).GetItemProperties(null)["OH_FullName"]);

			collection.Load("select OH_PK, OH_FullName from dbo.OrgHeader where OH_Code=@Nothing", new ZSqlParameter[] { ZSqlParameter.New("@Nothing", "nothing", OrgHeaderSchema.OH_Code) });
			AssertEquals("Should not return properties when there are no columns to know about", 0, ((ITypedList)collection).GetItemProperties(null).Count);
		}

		public void TestLoadOnlyFiresListResetOnce()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var newDummy = CreateDummy("DESC", 12);
			Factory.Save();

			var listChangedCalled = 0;
			var collection = new DynamicBusinessObjectCollection(Factory);
			((IBindingList)collection).ListChanged += (sender, e) => listChangedCalled++;

			collection.Load("select * from dbo.DUMMYBIZO where Z0_Number = @Num", new ZSqlParameterCollection(ZSqlParameter.New("@Num", 12, DummyBizoSchema.Z0_Number)));
			AssertEquals("One object should have loaded", 1, collection.Count);
			AssertEquals("ListChanged should have called only once", 1, listChangedCalled);
		}

		public void TestCanContinueWithSave()
		{
			IBusiness collection = new DynamicBusinessObjectCollection(Factory);
			AssertEquals(true, collection.CanContinueWithSave);
		}

		public void TestTypeOfElements()
		{
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			AssertEquals(typeof(DynamicBusinessObject), collection.TypeOfElements);
		}

		public void TestIsLoaded()
		{
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			AssertEquals(false, ((IBusinessObjectCollection)collection).IsLoaded);
			collection.Load("select * from dbo.DUMMYBIZO");
			AssertEquals(true, ((IBusinessObjectCollection)collection).IsLoaded);
		}

		public void TestGetEnumerator()
		{
			SaveTestData();

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("select * from dbo.DUMMYBIZO where Z0_Number = @Num", new ZSqlParameterCollection(ZSqlParameter.New("@Num", 12, DummyBizoSchema.Z0_Number)));

			var elements = new List<DynamicBusinessObject>();
			var enumerator = collection.GetEnumerator();
			while (enumerator.MoveNext())
			{
				elements.Add(enumerator.Current);
			}

			AssertContainsExactElementsInAnyOrder(collection, elements);
		}

		public void TestGetEnumerator_IEnumerable()
		{
			SaveTestData();

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("select * from dbo.DUMMYBIZO where Z0_Number = @Num", new ZSqlParameterCollection(ZSqlParameter.New("@Num", 12, DummyBizoSchema.Z0_Number)));

			var elements = new List<DynamicBusinessObject>();
			var enumerator = ((IEnumerable)collection).GetEnumerator();
			while (enumerator.MoveNext())
			{
				elements.Add((DynamicBusinessObject)enumerator.Current);
			}

			AssertContainsExactElementsInAnyOrder(collection, elements);
		}

		#region Implementation

		DummyBusinessObject CreateDummy(ZString description, ZInt number)
		{
			return CreateDummy(Factory, description, number);
		}

		DummyBusinessObject CreateDummy(BusinessObjectFactory factory, ZString description, ZInt number)
		{
			var result = factory.New<DummyBusinessObject>();
			result.Z0_Description = description;
			result.Z0_Number = number;
			return result;
		}

		void SaveTestData()
		{
			var dummy = CreateDummy("DESC", 12);
			var dummy2 = CreateDummy("", 12);
			Factory.Save();
		}

		#endregion // Implementation
	}
}

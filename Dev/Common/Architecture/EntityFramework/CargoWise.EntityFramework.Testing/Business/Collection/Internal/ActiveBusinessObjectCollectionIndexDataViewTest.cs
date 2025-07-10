using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ActiveBusinessObjectCollectionIndexDataViewTest : ActiveBusinessObjectCollectionIndexTest
	{
		#region Performance

		public void TestPerformance_CreatingIdenticalIndexesIsO1()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			var query = new ZQuery(DummyBizoSchema.PK, dummy.PK);

			for (int i = 0; i < 1000; i++)
			{
				var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, query);
				dummy.RegisterEditableChildObject(collection);
			}
			var cache = DataViewCache.GetInstance_Test(dummy.Table);
			AssertEquals(0, cache.Cache_Test.Count);
		}

		public void TestPerformance_CreatingDifferentIndexesIsO1()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			for (int i = 0; i < 1000; i++)
			{
				var query = new ZQuery(DummyBizoSchema.PK, ZGuid.NewZGuid());
				var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, query);
				dummy.RegisterEditableChildObject(collection);
			}
			var cache = DataViewCache.GetInstance_Test(dummy.Table);
			AssertEquals(0, cache.Cache_Test.Count);
		}

		public void TestPerformance_ForeignKey()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			for (int i = 0; i < 100; i++)
			{
				var nextDummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				nextDummy.Z0_Guid = dummy.PK;
				var query = new ZQuery(DummyBizoSchema.Z0_Guid, dummy.PK);
				var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, query);
				dummy.RegisterEditableChildObject(collection);
				AssertEquals(1, collection.Count);
				dummy = nextDummy;
			}
			var cache = DataViewCache.GetInstance_Test(dummy.Table);
			AssertEquals(1, cache.Cache_Test.Count);
		}

		public void TestPerformance_ForeignKeyWithFilter()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			for (int i = 0; i < 100; i++)
			{
				var nextDummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				var dudDummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				dudDummy.Z0_Guid = nextDummy.Z0_Guid = dummy.PK;
				nextDummy.Z0_Description = "Jam";
				var query = new ZQuery(DummyBizoSchema.Z0_Guid, dummy.PK);
				query.AddToFilter(DummyBizoSchema.Z0_Description, "Jam");
				var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, query);
				dummy.RegisterEditableChildObject(collection);
				AssertEquals(1, collection.Count);
				Assert(collection.Contains(nextDummy));
				dummy = nextDummy;
			}
			var cache = DataViewCache.GetInstance_Test(dummy.Table);
			AssertEquals(1, cache.Cache_Test.Count);
		}

		public void TestPerformance_ForeignKeyWithFilter_ZQueryVariant2()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			for (int i = 0; i < 100; i++)
			{
				var nextDummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				var dudDummy = Factory.NewWithValidTestData<DummyBusinessObject>();
				dudDummy.Z0_Guid = nextDummy.Z0_Guid = dummy.PK;
				nextDummy.Z0_Description = "Jam";
				var subQuery1 = new ZQuery(DummyBizoSchema.Z0_Guid, dummy.PK);
				var subQuery2 = new ZQuery(DummyBizoSchema.Z0_Description, "Jam");
				var query = new ZQuery(subQuery1, subQuery2);
				var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, query);
				dummy.RegisterEditableChildObject(collection);
				AssertEquals(1, collection.Count);
				Assert(collection.Contains(nextDummy));
				dummy = nextDummy;
			}
			var cache = DataViewCache.GetInstance_Test(dummy.Table);
			AssertEquals(1, cache.Cache_Test.Count);
		}

		#endregion

		#region Unexpected data rows

		public void TestPopulateCacheCoreSkipsDuplicates()
		{
			var dummyBizo = Factory.New<DummyBusinessObject>();

			Factory.Save();

			var dataTable = new DataTable("test");
			dataTable.Columns.Add("PK", typeof(Guid));

			var row1 = dataTable.Rows.Add();
			row1.ItemArray = new object[] { dummyBizo.PK.ToGuid() };

			var row2 = dataTable.Rows.Add();
			row2.ItemArray = new object[] { dummyBizo.PK.ToGuid() };

			var abocidv = new MockActiveBusinessObjectCollectionIndexDataView(
				Factory,
				typeof(ActiveBusinessObjectCollection<DummyBusinessObject>),
				typeof(DummyBusinessObject),
				new CollectionRelationship(typeof(DummyBusinessObject),
				new ZQuery(DummyBizoSchema.Z0_Description, "Relationship1")),
				new ZQuery(DummyBizoSchema.Z0_Description, "Filter1"),
				new CompareWithGuid(), null, null)
			{
				EnumeratorRows = new List<DataRow>(new DataRow[]
				{
					row1, row2
				})
			};

			var result = abocidv.PopulateCacheCoreExposed();

			AssertEquals("Should only be 1 item in the list, duplicate should be skipped.", 1, result.Count);

			AssertEquals("No error should be thrown for duplicate items.", "", ErrorReporter.LastMessageReported);
		}

		public void TestPopulateCacheCoreSkipsDeletedRows()
		{
			var dummyBizo1 = Factory.New<DummyBusinessObject>();
			var dummyBizo2 = Factory.New<DummyBusinessObject>();

			Factory.Save();

			var dataTable = new DataTable("test");
			dataTable.Columns.Add("PK", typeof(Guid));
			var dataView = new DataView(dataTable);

			var row1 = dataTable.Rows.Add();
			row1.ItemArray = new object[] { dummyBizo1.PK.ToGuid() };

			var row2 = dataTable.Rows.Add();
			row2.ItemArray = new object[] { dummyBizo2.PK.ToGuid() };
			row2.Delete();

			var abocidv = new MockActiveBusinessObjectCollectionIndexDataView(
				Factory,
				typeof(ActiveBusinessObjectCollection<DummyBusinessObject>),
				typeof(DummyBusinessObject),
				new CollectionRelationship(typeof(DummyBusinessObject),
				new ZQuery(DummyBizoSchema.Z0_Description, "Relationship1")),
				new ZQuery(DummyBizoSchema.Z0_Description, "Filter1"),
				new CompareWithGuid(), null, null)
			{
				EnumeratorRows = new List<DataRow>(new DataRow[]
				{
					row1, row2
				})
			};

			var result = abocidv.PopulateCacheCoreExposed();

			AssertEquals("Should only be 1 item in the list, deleted/detached rows should be skipped.", 1, result.Count);

			AssertEquals("No deleted/detached rows should be accessed, and no error should be thrown.", "", ErrorReporter.LastMessageReported);
		}

		#endregion

		#region MockActiveBusinessObjectCollectionIndexDataView

		internal class MockActiveBusinessObjectCollectionIndexDataView : ActiveBusinessObjectCollectionIndexDataView<DummyBusinessObject>
		{
			internal MockActiveBusinessObjectCollectionIndexDataView(BusinessObjectFactory factory, Type collectionType, Type elementType, ICollectionRelationship relationship, ZQuery additionalFilter, IComparer sortComparer, object[] collectionState,
			ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.CacheKey cacheKey) : base(factory, collectionType, elementType, relationship, additionalFilter, sortComparer, collectionState, cacheKey)
			{
			}

			internal List<DataRow> EnumeratorRows;

			protected override IEnumerator GetDataViewEnumerator()
			{
				return EnumeratorRows.GetEnumerator();
			}

			internal BusinessObjectList PopulateCacheCoreExposed() => base.PopulateCacheCore();
		}
		#endregion

		#region CompareWithGuid

		class CompareWithGuid : IComparer
		{
			int IComparer.Compare(object x, object y)
			{
				return ((BusinessObject)x).ToString().CompareTo(((BusinessObject)y).ToString());
			}

			public CompareWithGuid()
			{
			}
		}

		#endregion
	}
}

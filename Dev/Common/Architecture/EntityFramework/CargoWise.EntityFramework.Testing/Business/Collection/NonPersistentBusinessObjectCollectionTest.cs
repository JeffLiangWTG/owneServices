using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class NonPersistentBusinessObjectCollectionTest : TestCaseWithFactory
	{
		public void TestResortAfterSuspendListChanged()
		{
			var collection = GetDummyNonPersistentBusinessObjectCollectionForTest();
			AssertEquals("code11", collection[0].Code);
			AssertEquals("code22", collection[1].Code);
			collection.Sort("Code", ListSortDirection.Descending);
			AssertEquals("code22", collection[0].Code);
			AssertEquals("code11", collection[1].Code);
			using (collection.SuspendListChanged())
			{
				var bizo3 = collection.AddNew();
				bizo3.Code = "code33";
				var bizo4 = collection.AddNew();
				bizo4.Code = "code44";
			}
			AssertEquals("code44", collection[0].Code);
			AssertEquals("code33", collection[1].Code);
			AssertEquals("code22", collection[2].Code);
			AssertEquals("code11", collection[3].Code);
		}

		public void TestLoad()
		{
			try
			{
				new DummyNonPersistentBusinessObjectCollection(Factory).Load();
				AssertNotNull("Reported exceptions count", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				AssertEquals("Reported exceptions count", true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				new DummyNonPersistentBusinessObjectCollection(Factory).Load(new ZQuery());
				AssertNotNull("Reported exceptions count", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestCreateNewBusinessObject()
		{
			DummyNonPersistentBusinessObjectCollection collection = new DummyNonPersistentBusinessObjectCollection(Factory);
			IBindingList bindingList = collection;

			DummyNonPersistentBusinessObject bizObj = bindingList.AddNew() as DummyNonPersistentBusinessObject;

			AssertNotNull("Created DummyNonPersistentBusinessObject", bizObj);
			Assert("Should have Collection as Parent", ((IList)bizObj.ParentCollections).Contains(collection));

			bindingList.ListChanged += new ListChangedEventHandler(BindingList_ListChanged);
			AssertEquals("ListChanged should not have fired on collection", false, HasListChangeFired);

			bizObj.OnElementChanged();
			AssertEquals("ListChanged should have fired on collection", true, HasListChangeFired);
		}

		public void TestNotifyUncommittedCancelEdit_DoesDeleteElement()
		{
			DummyNonPersistentBusinessObjectCollection collection = new DummyNonPersistentBusinessObjectCollection(Factory);
			DummyNonPersistentBusinessObject bizObj = (DummyNonPersistentBusinessObject)((IBindingList)collection).AddNew();
			Assert("Precondition", !bizObj.IsDeleted);

			((ICancelAddNew)collection).CancelNew(collection.Count - 1);
			Assert("Non-persistent BizObj should behave like persistent biz obj when Add is cancelled", bizObj.IsDeleted);
		}

		public void TestGenericisedCollection()
		{
			NonPersistentBusinessObjectCollection<DummyNonPersistentBusinessObject> collection = new DummyGenericNonPersistentBusinessObjectCollection();
			DummyNonPersistentBusinessObject dummy1FromAddNew = collection.AddNew();
			DummyNonPersistentBusinessObject dummy1FromIndexer = collection[0];
			AssertEquals("Dummy1FromAddNew = Dummy1FromIndexer", dummy1FromAddNew, dummy1FromIndexer);
			AssertEquals("Collection.Count", 1, collection.Count);
			DummyNonPersistentBusinessObject dummy2FromAddNew = collection.AddNew();
			DummyNonPersistentBusinessObject dummy2FromIndexer = collection[1];
			AssertEquals("Dummy2FromAddNew = Dummy2FromIndexer", dummy2FromAddNew, dummy2FromIndexer);
			AssertEquals("Collection.Count", 2, collection.Count);
			collection.RemoveAndDelete(dummy1FromAddNew);
			AssertEquals("Collection.Count", 1, collection.Count);
			AssertEquals("Dummy1FromAddNew.IsDeleted", true, dummy1FromAddNew.IsDeleted);
			AssertEquals("Dummy2FromAddNew.IsDeleted", false, dummy2FromAddNew.IsDeleted);
			collection.RemoveAndDeleteAll();
			AssertEquals("Collection.Count", 0, collection.Count);
			AssertEquals("Dummy1FromAddNew.IsDeleted", true, dummy1FromAddNew.IsDeleted);
			AssertEquals("Dummy2FromAddNew.IsDeleted", true, dummy2FromAddNew.IsDeleted);
		}

		public void TestHasChangesIsTrueWhenChildDeleted()
		{
			DummyNonPersistentBusinessObjectCollection collection = new DummyNonPersistentBusinessObjectCollection(Factory);
			DummyNonPersistentBusinessObject uncommittedBizObj = (DummyNonPersistentBusinessObject)((IBindingList)collection).AddNew();

			collection.HasChanges = false;
			collection.RemoveAndDelete(uncommittedBizObj);
			AssertEquals(false, collection.HasChanges);

			DummyNonPersistentBusinessObject committedBizObj = new DummyNonPersistentBusinessObject();
			collection.Add(committedBizObj);
			collection.RemoveAndDelete(committedBizObj);
			AssertEquals(true, collection.HasChanges);
		}

		bool HasListChangeFired;

		void BindingList_ListChanged(object sender, ListChangedEventArgs e)
		{
			HasListChangeFired = true;
		}

		DummyNonPersistentBusinessObjectCollection GetDummyNonPersistentBusinessObjectCollectionForTest()
		{
			var collection = new DummyNonPersistentBusinessObjectCollection(Factory);
			var bo1 = collection.AddNew();
			bo1.Code = "code11";
			bo1.Description = "description11";

			var bo2 = collection.AddNew();
			bo2.Code = "code22";
			bo2.Description = "description22";

			return collection;
		}

		DummyNonPersistentBusinessObjectWithoutAttributeCollection GetDummyNonPersistentBusinessObjectWithoutAttributeCollectionForTest()
		{
			var collection = new DummyNonPersistentBusinessObjectWithoutAttributeCollection(Factory);
			var bo1 = collection.AddNew();
			bo1.Code = "code11";
			bo1.Description = "description11";

			var bo2 = collection.AddNew();
			bo2.Code = "code22";
			bo2.Description = "description22";

			return collection;
		}

		public void TestIFindBoxListProviderDescriptionEx()
		{
			var collection = GetDummyNonPersistentBusinessObjectCollectionForTest();
			var findBoxListProviderDescriptionEx = collection as IFindBoxListProviderDescriptionEx;
			AssertEquals("code11", findBoxListProviderDescriptionEx.CodeFromDescription("description11"));
			AssertEquals("description22", findBoxListProviderDescriptionEx.NearestDescriptionMatch("description2", false));

			DummyNonPersistentBusinessObjectWithoutAttributeCollection collection2 = GetDummyNonPersistentBusinessObjectWithoutAttributeCollectionForTest();
			var findBoxListProviderDescriptionEx2 = collection2 as IFindBoxListProviderDescriptionEx;
			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{ findBoxListProviderDescriptionEx2.CodeFromDescription("description11"); });
			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{ findBoxListProviderDescriptionEx2.NearestDescriptionMatch("description2", false); });
		}

		public void TestIFindBoxListProvider()
		{
			var collection = GetDummyNonPersistentBusinessObjectCollectionForTest();
			var findBoxListProvider = collection as IFindBoxListProvider;

			AssertEquals("code11", findBoxListProvider.NearestMatch("code1", false, -1).Item1);
			AssertEquals("description11", findBoxListProvider.DescriptionFromCode("code11"));
			AssertEquals(string.Empty, findBoxListProvider.DescriptionFromPrimaryKey(ZGuid.NewZGuid()));
			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{ findBoxListProvider.PrimaryKeyFromCode("code"); });
			AssertEquals(collection[0], findBoxListProvider.GetBusinessObjectFromCode("code11"));
			AssertEquals(collection[0], findBoxListProvider.GetBusinessObjectFromCodeWithoutFilter("code11"));
			AssertEquals(string.Empty, findBoxListProvider.CodeFromPrimaryKey(ZGuid.NewZGuid()));
			AssertEquals(2, findBoxListProvider.List.Count);

			DummyNonPersistentBusinessObjectWithoutAttributeCollection collection2 = GetDummyNonPersistentBusinessObjectWithoutAttributeCollectionForTest();
			var findBoxListProvider2 = collection2 as IFindBoxListProvider;

			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{ findBoxListProvider2.NearestMatch("code", false, -1); });
			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{ findBoxListProvider2.DescriptionFromCode("code"); });
			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{ findBoxListProvider2.DescriptionFromPrimaryKey(ZGuid.NewZGuid()); });
			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{ findBoxListProvider2.PrimaryKeyFromCode("code"); });
			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{ findBoxListProvider2.GetBusinessObjectsFromCode("code"); });
			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{ findBoxListProvider2.GetBusinessObjectsFromCodeWithoutFilter("code"); });
			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{ findBoxListProvider2.CodeFromPrimaryKey(ZGuid.NewZGuid()); });
			AssertEquals(2, findBoxListProvider2.List.Count);
		}

		public void TestIFindBoxListProviderEx()
		{
			var collection = GetDummyNonPersistentBusinessObjectCollectionForTest();

			var findBoxListProviderEx = collection as IFindBoxListProviderEx;
			IList<AlternateKey> keys = findBoxListProviderEx.AlternateKeys;
			AssertEquals(1, keys.Count);
			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{ findBoxListProviderEx.AlternateKeyFromPrimaryKey("", ZGuid.NewZGuid()); });
			AssertExceptionThrown(typeof(NotImplementedException), delegate
			{ findBoxListProviderEx.PrimaryKeyFromAlternateKey("Description", new ZString("description")); });
		}

		public void TestGetNonPersistentTable()
		{
			var collection = GetDummyNonPersistentBusinessObjectCollectionForTest();

			Assert("Precondition", collection.Count >= 2);
			var collectionInternal = collection as INonPersistentBusinessObjectCollectionInternal;
			var table = collectionInternal.GetNonPersistentTable(collection[0]);
			AssertSame("Should generate and use only 1 data table", table, collectionInternal.GetNonPersistentTable(collection[1]));
			AssertSame("Should return same table for compatible object", table, collectionInternal.GetNonPersistentTable(new DummyNonPersistentBusinessObject()));
			AssertNull("Should return null for non-compatible object", collectionInternal.GetNonPersistentTable(new DummyNonPersistentBusinessObjectWithoutAttribute()));
		}

		#region Support Classes

		public class DummyGenericNonPersistentBusinessObjectCollection : NonPersistentBusinessObjectCollection<DummyNonPersistentBusinessObject>
		{
			public DummyGenericNonPersistentBusinessObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public DummyGenericNonPersistentBusinessObjectCollection()
				: base()
			{
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new DummyNonPersistentBusinessObject();
			}

			[TestedType(typeof(DummyGenericNonPersistentBusinessObjectCollection))]
			sealed class DummyNonPersistentBusinessObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DummyGenericNonPersistentBusinessObjectCollection>
			{
				protected override BusinessObject GetNewElementToAddToTheCollection()
				{
					return new DummyNonPersistentBusinessObject();
				}

				protected override DummyGenericNonPersistentBusinessObjectCollection GetCollectionToTest()
				{
					return new DummyGenericNonPersistentBusinessObjectCollection(Factory);
				}
			}
		}

		#endregion
	}
}

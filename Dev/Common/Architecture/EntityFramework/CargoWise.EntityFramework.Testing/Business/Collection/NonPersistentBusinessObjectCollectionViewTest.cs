using System.Collections;
using System.ComponentModel;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class NonPersistentBusinessObjectCollectionViewTest : SubsetBusinessObjectCollectionTestCase<DummyBaseBusinessObject>
	{
		public new void TestLoad()
		{
			try
			{
				new DummyNonPersistentBusinessObjectCollectionView(new DummyNonPersistentBusinessObjectCollection(Factory)).Load();
				AssertNotNull("Reported exceptions count", ErrorReporter.LastKeyReported);
				ErrorReporter.Clear();
				AssertEquals("Reported exceptions count", true, string.IsNullOrEmpty(ErrorReporter.LastKeyReported));
				new DummyNonPersistentBusinessObjectCollectionView(new DummyNonPersistentBusinessObjectCollection(Factory)).Load(new ZQuery());
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
			DummyNonPersistentBusinessObjectCollectionView collectionView = new DummyNonPersistentBusinessObjectCollectionView(collection);
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
			DummyNonPersistentBusinessObjectCollectionView collectionView = new DummyNonPersistentBusinessObjectCollectionView(collection);
			DummyNonPersistentBusinessObject bizObj = (DummyNonPersistentBusinessObject)((IBindingList)collectionView).AddNew();
			Assert("Precondition", !bizObj.IsDeleted);

			((ICancelAddNew)collectionView).CancelNew(collectionView.Count - 1);
			Assert("Non-persistent BizObj should behave like persistent biz obj when Add is cancelled", bizObj.IsDeleted);
		}

		public void TestGenericisedCollection()
		{
			NonPersistentBusinessObjectCollection<DummyNonPersistentBusinessObject> collection = new DummyGenericNonPersistentBusinessObjectCollection();
			NonPersistentBusinessObjectCollectionView<DummyNonPersistentBusinessObject> collectionView =
				new DummyGenericNonPersistentBusinessObjectCollectionView(collection);
			DummyNonPersistentBusinessObject dummy1FromAddNew = collection.AddNew();
			DummyNonPersistentBusinessObject dummy1FromIndexer = collection[0];
			AssertEquals("dummy1FromAddNew = dummy1FromIndexer", dummy1FromAddNew, dummy1FromIndexer);
			AssertEquals("Collection.Count", 1, collection.Count);
			DummyNonPersistentBusinessObject dummy2FromAddNew = collection.AddNew();
			DummyNonPersistentBusinessObject dummy2FromIndexer = collection[1];
			AssertEquals("dummy2FromAddNew = dummy2FromIndexer", dummy2FromAddNew, dummy2FromIndexer);
			AssertEquals("Collection.Count", 2, collection.Count);
			collection.RemoveAndDelete(dummy1FromAddNew);
			AssertEquals("Collection.Count", 1, collection.Count);
			AssertEquals("dummy1FromAddNew.IsDeleted", true, dummy1FromAddNew.IsDeleted);
			AssertEquals("dummy2FromAddNew.IsDeleted", false, dummy2FromAddNew.IsDeleted);
			collection.RemoveAndDeleteAll();
			AssertEquals("Collection.Count", 0, collection.Count);
			AssertEquals("dummy1FromAddNew.IsDeleted", true, dummy1FromAddNew.IsDeleted);
			AssertEquals("dummy2FromAddNew.IsDeleted", true, dummy2FromAddNew.IsDeleted);
		}

		bool HasListChangeFired;

		void BindingList_ListChanged(object sender, ListChangedEventArgs e)
		{
			HasListChangeFired = true;
		}

		protected override ISubsetBusinessObjectCollection GetNewCollection(BusinessObjectCollection collectionToFilter)
		{
			return new DummyViewCollection(collectionToFilter);
		}

		#region Support Classes

		public class DummyNonPersistentBusinessObjectCollectionView : NonPersistentBusinessObjectCollectionView<DummyNonPersistentBusinessObject>
		{
			public DummyNonPersistentBusinessObjectCollectionView(NonPersistentBusinessObjectCollection<DummyNonPersistentBusinessObject> collectionToFilter)
				: base(collectionToFilter)
			{
			}

			protected override DummyNonPersistentBusinessObject CreateNonPersistentBusinessObject()
			{
				return new DummyNonPersistentBusinessObject();
			}

			protected override bool IsThisPartOfTheCollection(BusinessObject element)
			{
				return true;
			}
		}

		public class DummyGenericNonPersistentBusinessObjectCollectionView : NonPersistentBusinessObjectCollectionView<DummyNonPersistentBusinessObject>
		{
			public DummyGenericNonPersistentBusinessObjectCollectionView(NonPersistentBusinessObjectCollection<DummyNonPersistentBusinessObject> collectionToFilter)
				: base(collectionToFilter)
			{
			}

			public DummyGenericNonPersistentBusinessObjectCollectionView()
				: base()
			{
			}

			protected override DummyNonPersistentBusinessObject CreateNonPersistentBusinessObject()
			{
				return new DummyNonPersistentBusinessObject();
			}

			protected override bool IsThisPartOfTheCollection(BusinessObject element)
			{
				return true;
			}
		}

		public class DummyNonPersistentBusinessObjectCollection : NonPersistentBusinessObjectCollection<DummyNonPersistentBusinessObject>
		{
			public DummyNonPersistentBusinessObjectCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new DummyNonPersistentBusinessObject();
			}
		}

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

		public class DummyNonPersistentBusinessObject : NonPersistentBusinessObject
		{
		}

		#endregion
	}
}

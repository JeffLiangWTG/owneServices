using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.DocWrappers.Testing
{
	public abstract class DocumentWrapperCollectionTest<T> : NonPersistentBusinessObjectCollectionTestCase<T> where T : DocumentWrapperCollection
	{
		[ExpectNoExceptions]
		public void TestLoad()
		{
			object objectToWrap = GetNewObjectToWrap();
			if (objectToWrap != null)
			{
				DocumentWrapperCollection collection = GetNewDocumentWrapperCollection();
				collection.Load(new object[] { objectToWrap });
			}
		}

		public override void TestTypedget_Item()
		{
			DocumentWrapperCollection collection = GetNewCollectionWithAtLeastOneObjectInIt();
			MethodInfo indexerMethodInfo = GetIndexerMethodInfo(collection, typeof(int));
			if (indexerMethodInfo != null)
			{
				var result = (DocumentWrapper)indexerMethodInfo.Invoke(collection, new object[] { 0 });
				AssertNotNull("Return Type of get_Item", result);
			}
			else
			{
				Assert(true);
			}
		}

		public virtual void TestTypedStringIndexer()
		{
			DocumentWrapperCollection collection = GetNewDocumentWrapperCollection();
			collection.RemoveAll();
			DocumentWrapper wrapper1 = AddNewDocumentWrapperToCollection(collection);
			DocumentWrapper wrapper2 = AddNewDocumentWrapperToCollection(collection);
			DocumentWrapper wrapper3 = AddNewDocumentWrapperToCollection(collection);

			AssertEquals("collection[1]", wrapper1, collection["1"]);
			AssertEquals("collection[2]", wrapper2, collection["2"]);
			AssertEquals("collection[3]", wrapper3, collection["3"]);
			AssertEquals("collection[4]", null, collection["4"]);

			AssertEquals("collection[count]", wrapper3, collection["count"]);
			AssertEquals("collection[count-1]", wrapper2, collection["count-1"]);
			AssertEquals("collection[count-2]", wrapper1, collection["count-2"]);
			AssertEquals("collection[count-3]", null, collection["count-3"]);

			AssertEquals("collection[count - 1]  (spaces should be ignored) ", wrapper2, collection["count - 1"]);
			AssertEquals("collection[COUNT]      (case should be ignored)   ", wrapper3, collection["COUNT"]);

			AssertEquals("collection[first]", wrapper1, collection["first"]);
			AssertEquals("collection[last]", wrapper3, collection["last"]);

			AssertEquals("collection[AnyThingIDon'tRecognise]", null, collection["AnyWordIDon'tRecognise"]);
		}

		protected abstract object GetNewObjectToWrap();
		protected abstract T GetNewDocumentWrapperCollection();

		#region Implementation
		MethodInfo GetIndexerMethodInfo(DocumentWrapperCollection collection, Type indexerType)
		{
			MethodInfo indexerMethodInfo = null;
			try
			{
				indexerMethodInfo = collection.GetType().GetMethod("get_Item", new Type[] { indexerType });
			}
			catch (TargetInvocationException e)
			{
				if (!(e.InnerException is NotSupportedException))
				{
					throw;
				}
			}
			return indexerMethodInfo;
		}

		protected T GetNewCollectionWithAtLeastOneObjectInIt()
		{
			T collection = GetNewDocumentWrapperCollection();
			if (collection.Count == 0)
			{
				AddNewDocumentWrapperToCollection(collection);
			}
			return collection;
		}

		protected virtual DocumentWrapper AddNewDocumentWrapperToCollection(DocumentWrapperCollection collection)
		{
			DocumentWrapper wrapper = collection.WrapObject(GetNewObjectToWrap());
			collection.Add(wrapper);
			return wrapper;
		}

		protected override T GetCollectionToTest()
		{
			return GetNewCollectionWithAtLeastOneObjectInIt();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion

		public override void TestAdd()
		{
			Assert("Functionality Not Required by DocumentWrapperCollections", true);
		}

		public sealed override void TestAddNew()
		{
			Assert("Functionality Not Required by DocumentWrapperCollections", true);
		}

		public sealed override void TestTypedAddNew()
		{
			Assert("Functionality Not Required by DocumentWrapperCollections", true);
		}

		public sealed override void TestRemoveFromRelationship()
		{
			Assert("Functionality Not Required by DocumentWrapperCollections", true);
		}

		public sealed override void TestDelete()
		{
			Assert("Functionality Not Required by DocumentWrapperCollections", true);
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Functionality Not Required by DocumentWrapperCollections", true);
		}
	}
}

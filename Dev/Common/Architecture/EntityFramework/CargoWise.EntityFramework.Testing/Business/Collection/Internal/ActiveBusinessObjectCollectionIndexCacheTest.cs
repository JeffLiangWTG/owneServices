using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ActiveBusinessObjectCollectionIndexCacheTest : TestCaseWithFactory
	{
		public void TestGetInstance()
		{
			ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject> cache1 = ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.GetInstance(Factory);
			ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject> cache2 = ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.GetInstance(Factory);
			AssertEquals("Instance should be cached by factory", true, cache1 == cache2);
		}

		public void TestGetIndex()
		{
			AssertGetIndex<DummyBusinessObject>(Factory, typeof(ActiveBusinessObjectCollection<DummyBusinessObject>), typeof(DummyBusinessObject), Relationship, Filter, null, null);
			AssertGetIndex<DummyChildBusinessObject>(Factory, typeof(ActiveBusinessObjectCollection<DummyChildBusinessObject>), typeof(DummyChildBusinessObject), Relationship, Filter, null, null);
			AssertGetIndex<DummyBusinessObject>(Factory, typeof(ActiveBusinessObjectCollection<DummyBusinessObject>), typeof(DummyBusinessObject), Relationship2, Filter, null, null);
			AssertGetIndex<DummyBusinessObject>(Factory, typeof(ActiveBusinessObjectCollection<DummyBusinessObject>), typeof(DummyBusinessObject), Relationship, Filter2, null, null);
			AssertGetIndex<DummyBusinessObject>(Factory, typeof(ActiveBusinessObjectCollection<DummyBusinessObject>), typeof(DummyBusinessObject), Relationship3, Filter, null, null);
			AssertGetIndex<DummyBusinessObject>(Factory, typeof(ActiveBusinessObjectCollection<DummyBusinessObject>), typeof(DummyBusinessObject), Relationship, Filter3, null, null);
			AssertGetIndex<DummyBusinessObject>(Factory, typeof(ActiveBusinessObjectCollection<DummyBusinessObject>), typeof(DummyBusinessObject), Relationship, Filter2, new PropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code.Name, System.ComponentModel.ListSortDirection.Ascending), null);
			AssertGetIndex<DummyBusinessObject>(Factory, typeof(ActiveBusinessObjectCollection<DummyBusinessObject>), typeof(DummyBusinessObject), Relationship, Filter2, new PropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code.Name, System.ComponentModel.ListSortDirection.Descending), null);
			AssertGetIndex<DummyBusinessObject>(Factory, typeof(ActiveBusinessObjectCollection<DummyBusinessObject>), typeof(DummyBusinessObject), Relationship3, Filter2, new PropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_DateTimeOffset.Name, System.ComponentModel.ListSortDirection.Ascending), null);
			AssertGetIndex<DummyBusinessObject>(Factory, typeof(ActiveBusinessObjectCollection<DummyBusinessObject>), typeof(DummyBusinessObject), Relationship, Filter3, new PropertyComparer(typeof(DummyBusinessObject), DummyBizoSchema.Z0_DateTimeOffset.Name, System.ComponentModel.ListSortDirection.Descending), null);
			AssertGetIndex<DummyBusinessObject>(new BusinessObjectFactory(), typeof(ActiveBusinessObjectCollection<DummyBusinessObject>), typeof(DummyBusinessObject), Relationship, Filter, null, null);
			AssertGetIndex<DummyBusinessObject>(new BusinessObjectFactory(), typeof(ActiveBusinessObjectCollection<DummyBusinessObject>), typeof(DummyBusinessObject), Relationship, Filter, null, new object[] { 1 });
			AssertGetIndex<DummyBusinessObject>(new BusinessObjectFactory(), typeof(ActiveBusinessObjectCollection<DummyBusinessObject>), typeof(DummyBusinessObject), Relationship, Filter, null, new object[] { 1, 2 });
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestGetIndex_WithComparerThatDoesntOverrideEquals()
		{
			GetCache<DummyBusinessObject>().GetIndex(typeof(ActiveBusinessObjectCollection<DummyBusinessObject>), typeof(DummyBusinessObject), Relationship, Filter, new ComparerWithoutEqualsOrGetHashCode(), null);
		}

		public void TestIndexGarbagedCollected()
		{
			var indexRef = CreateUnreferencedIndex();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			Factory.New<DummyBusinessObject>(); // a ListChanged must occur for the index to detect it is no longer in use
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			AssertEquals("Index should be collected", false, indexRef.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		WeakReference CreateUnreferencedIndex()
		{
			var collection1 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
			var collection2 = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);

			var index = (ActiveBusinessObjectCollectionIndex<DummyBusinessObject>)collection1.GetType().InvokeMember("Index", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty, null, collection1, null);
			var loaded = index.Count;
			return new WeakReference(index);
		}

		public void TestCollectionIndexDoesNotSubscribeToFactorySavedEvent()
		{
			var collectionType = typeof(string);
			var elementType = typeof(DummyBusinessObject);
			var relationship = Relationship;
			var additionalFilter = Filter;
			var comparer = new CompareWithDefinedHash(typeof(List<string>).GetHashCode());
			object[] collectionState = null;

			var cache = ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.GetInstance(Factory);
			cache.GetIndex(collectionType, elementType, relationship, additionalFilter, comparer, collectionState);

			var numberOfSubscribedHandlers = Factory.NumberOfSubscribedHandlersToSaved;

			for (var counter = 2; counter < 100; counter++)
			{
				cache = ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.GetInstance(Factory);
				cache.GetIndex(collectionType, elementType, relationship, new ZQuery(DummyBizoSchema.Z0_Description, $"Filter{counter}"), comparer, collectionState);
			}

			AssertEquals("Creation of ActiveBusinessOjbectCollections Should Not Change Save Event Handlers", numberOfSubscribedHandlers, Factory.NumberOfSubscribedHandlersToSaved);
		}

		public void TestCacheKeyEqualDifferentCollectionTypes()
		{
			Type collectionType1 = typeof(string);
			Type collectionType2 = typeof(List<string>);
			Type elementType = typeof(DummyBusinessObject);
			ICollectionRelationship relationship = Relationship;
			ZQuery additionalFilter = Filter;
			IComparer comparer = null;
			object[] collectionState = null;

			ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.CacheKey ck0 = new ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.CacheKey(collectionType1, elementType, relationship, additionalFilter, comparer, collectionState);
			ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.CacheKey ck1 = new ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.CacheKey(collectionType1, elementType, relationship, additionalFilter, comparer, collectionState);
			ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.CacheKey ck2 = new ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.CacheKey(collectionType2, elementType, relationship, additionalFilter, comparer, collectionState);

			Assert(ck0.Equals(ck1));
			Assert(!ck0.Equals(ck2));
		}

		public void TestDisposingActiveBusinessObjectCollectionIndex()
		{
			Type collectionType1 = typeof(string);
			Type collectionType2 = typeof(List<string>);
			Type elementType = typeof(DummyBusinessObject);
			ICollectionRelationship relationship = Relationship;
			ZQuery additionalFilter = Filter;

			// Create two compare object to generate the same hash code in the CacheKey object
			IComparer comparer1 = new CompareWithDefinedHash(typeof(List<string>).GetHashCode());
			IComparer comparer2 = new CompareWithDefinedHash(typeof(string).GetHashCode());
			object[] collectionState = null;

			ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject> cache = ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.GetInstance(Factory);

			using (ActiveBusinessObjectCollectionIndex<DummyBusinessObject> id1 = cache.GetIndex(collectionType1, elementType, relationship, additionalFilter, comparer1, collectionState))
			using (ActiveBusinessObjectCollectionIndex<DummyBusinessObject> id2 = cache.GetIndex(collectionType2, elementType, relationship, additionalFilter, comparer2, collectionState))
			{
				Assert(!ActiveBusinessObjectCollectionIndex<DummyBusinessObject>.ReferenceEquals(id1, id2));
				id1.Dispose();
				id2.Dispose();
				// An object should be able to be disposed multiple times
				id2.Dispose();
			}
		}

		public void TestDisposeCollectionIndex_DoesNotExistInTheCache()
		{
			var collectionType = typeof(string);
			var elementType = typeof(DummyBusinessObject);
			var relationship = Relationship;
			var additionalFilter = Filter;
			var comparer = new CompareWithDefinedHash(typeof(List<string>).GetHashCode());
			object[] collectionState = null;

			var cache = ActiveBusinessObjectCollectionIndexCache<DummyBusinessObject>.GetInstance(Factory);
			var id = cache.GetIndex(collectionType, elementType, relationship, additionalFilter, comparer, collectionState);
			cache.CacheForTest.Remove(id.cacheKey);

			AssertExceptionThrown<InvalidOperationException>("Error message",
				$"Collection index cannot be removed from the cache because it doesn't exist in the cache\r\nType: CargoWise.EntityFramework.Testing.DummyBusinessObject;\r\nFactory instance: {Factory._Instance};\r\nAdditional filter: Z0_Description = 'Filter1';\r\nCollection type: System.String;\r\nElement type: CargoWise.EntityFramework.Testing.DummyBusinessObject;",
				() => id.Dispose());
		}

		#region Test Classes

		class CompareWithDefinedHash : IComparer
		{
			int IComparer.Compare(object x, object y)
			{
				return 0;
			}

			public CompareWithDefinedHash(int hash)
			{
				hashCode = hash;
			}

			public override bool Equals(Object obj)
			{
				return true;
			}

			public override int GetHashCode()
			{
				return hashCode;
			}
			readonly int hashCode;
		}

		class ComparerWithoutEqualsOrGetHashCode : IComparer
		{
			int IComparer.Compare(object x, object y)
			{
				return 0;
			}
		}

		#endregion

		#region Implementation

		void AssertGetIndex<T>(BusinessObjectFactory factory, Type collectionType, Type elementType, ICollectionRelationship relationship, ZQuery filter, IComparer comparer, object[] collectionState) where T : BusinessObject
		{
			ActiveBusinessObjectCollectionIndex<T> index = GetCache<T>().GetIndex(collectionType, elementType, relationship, filter, comparer, collectionState);

			AssertEquals("Factory", Factory, index.Factory);
			AssertEquals("ElementType", elementType, index.ElementType);
			AssertEquals("Relationship", relationship, index.Relationship);
			AssertEquals("Filter", filter, index.AdditionalFilter);
			AssertEquals("Filter", comparer, index.SortComparer);
			AssertEquals("CollectionState", collectionState == null ? 0 : collectionState.Length, index.CollectionState == null ? 0 : index.CollectionState.Length);

			if (collectionState != null)
			{
				for (int i = 0; i < collectionState.Length; i++)
				{
					AssertEquals("CollectionState", collectionState[i], index.CollectionState[i]);
				}
			}
		}

		ActiveBusinessObjectCollectionIndexCache<T> GetCache<T>() where T : BusinessObject
		{
			return ActiveBusinessObjectCollectionIndexCache<T>.GetInstance(Factory);
		}

		ZQuery Filter
		{
			get
			{
				if (filter == null)
				{
					filter = new ZQuery(DummyBizoSchema.Z0_Description, "Filter1");
				}
				return filter;
			}
		}
		ZQuery filter;

		ZQuery Filter2
		{
			get
			{
				if (filter2 == null)
				{
					filter2 = new ZQuery(DummyBizoSchema.Z0_Description, "Filter2");
				}
				return filter2;
			}
		}
		ZQuery filter2;

		ZQuery Filter3
		{
			get
			{
				if (filter3 == null)
				{
					filter3 = new ZQuery(DummyBizoSchema.Z0_DateTimeOffset, new ZDateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));
				}
				return filter3;
			}
		}
		ZQuery filter3;

		ICollectionRelationship Relationship
		{
			get
			{
				if (relationship == null)
				{
					relationship = new CollectionRelationship(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Description, "Relationship1"));
				}
				return relationship;
			}
		}
		ICollectionRelationship relationship;

		ICollectionRelationship Relationship2
		{
			get
			{
				if (relationship2 == null)
				{
					relationship2 = new CollectionRelationship(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Description, "Relationship2"));
				}
				return relationship2;
			}
		}
		ICollectionRelationship relationship2;

		ICollectionRelationship Relationship3
		{
			get
			{
				if (relationship3 == null)
				{
					relationship3 = new CollectionRelationship(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_DateTimeOffset, new ZDateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero)));
				}
				return relationship3;
			}
		}
		ICollectionRelationship relationship3;

		#endregion
	}
}

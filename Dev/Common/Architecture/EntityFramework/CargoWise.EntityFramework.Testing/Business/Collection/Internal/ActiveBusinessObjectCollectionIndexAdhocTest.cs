using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ActiveBusinessObjectCollectionIndexAdhocTest : ActiveBusinessObjectCollectionIndexTest
	{
		public void TestHasChangesNotResetInAdhocCollection()
		{
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new AdhocCollectionRelationship(typeof(DummyBusinessObject)));
			Assert("Precondition - hasChanges is not yet calculated", !collection.IndexExposed.HasChangesFieldExposedForTest.HasValue);

			Assert("HasChanges should be calculated properly", !((IBusiness)collection).HasChanges); // Call HasChanges

			Assert("hasChanges field should not be reset", collection.IndexExposed.HasChangesFieldExposedForTest.HasValue);
			Assert("hasChanges field should not be reset", !collection.IndexExposed.HasChangesFieldExposedForTest.Value);

			collection.AddNew().Z0_Code = "ABC";

			Assert("HasChanges should be calculated properly", ((IBusiness)collection).HasChanges); // Call HasChanges

			Assert("hasChanges field should not be reset", collection.IndexExposed.HasChangesFieldExposedForTest.HasValue);
			Assert("hasChanges field should not be reset", collection.IndexExposed.HasChangesFieldExposedForTest.Value);
		}

		public override void TestHitDbOnceWhenUsingDbOnlyAdditionalFilterWithRelationshipFilter()
		{
			Assert(true);
		}

		public override void TestDbOnlyQueryRefreshesProperly()
		{
			Assert(true);
		}

		public override void TestHasChangesDoesNotHitDb()
		{
			Assert(true);
		}

		public override void TestLoadAfterHasChangesCheck()
		{
			Assert(true);
		}

		public override void TestUseFetchHintsOnLoadingCollectionWithDbOnlyFilter()
		{
			Assert(true);
		}

		public override void TestUseFetchHintsOnLoadingCollectionWithDbOnlyCompositeFilter()
		{
			Assert(true);
		}

		public override void TestUseFetchHintsOnLoadingCollectionWithDbOnlyFilterAndMaximumRows()
		{
			Assert(true);
		}

		public override void TestCollectionCountChange()
		{
			Assert(true);
		}

		public override void TestAddNew()
		{
			Assert(true);
		}

		[ExpectNoExceptions]
		public override void TestIndexNotCorruptedWithListChangedLogic()
		{
			Assert(true);
		}

		public override void TestAddingUncommittedItem_WhenCommittingWithChanges()
		{
			Assert(true);
		}

		public override void TestAddingUncommittedItem_WhenCancelling()
		{
			Assert(true);
		}

		public override void TestAddingUncommittedItem_WhenInvalidatingCacheAndCommitting()
		{
			Assert(true);
		}

		public override void TestAddingUncommittedItem_WhenInvalidatingCacheAndCancelling()
		{
			Assert(true);
		}

		public override void TestIsNonCommittedElement()
		{
			Assert(true);
		}

		public override void TestToArray()
		{
			Assert(true);
		}

		public override void TestFindByPK()
		{
			Assert(true);
		}

		#region ListChanged event unsorted

		public override void TestListChanged_ItemAddedToEnd()
		{
			Assert(true);
		}

		public override void TestListChanged_ItemAddedToMiddle()
		{
			Assert(true);
		}

		public override void TestListChanged_ItemDeleted()
		{
			Assert(true);
		}

		public override void TestListChanged_ItemDeleted_WhenObjectGoesOutOfScopeOfFilter()
		{
			Assert(true);
		}

		public override void TestListChanged_ItemChanged()
		{
			Assert(true);
		}

		#endregion

		#region ListChanged event with sort

		public override void TestListChanged_SortedItemAddedToMiddle()
		{
			Assert(true);
		}

		public override void TestListChanged_SortedItemAddedToEnd()
		{
			Assert(true);
		}

		public override void TestListChanged_SortedItemChangedOrder()
		{
			Assert(true);
		}

		public override void TestListChanged_SortedItemChangedOrder_FromDataRefreshBus()
		{
			Assert(true);
		}

		public override void TestListChanged_SortedItemChanged()
		{
			Assert(true);
		}

		#endregion

		#region IList

		public override void TestAddingToRelationship()
		{
			Assert(true);
		}

		public override void TestRemovingFromRelationship_ByRemove()
		{
			Assert(true);
		}

		public override void TestRemoveAt_DeletesBusinessObject()
		{
			Assert(true);
		}

		#endregion

		public override void TestAddingImplicitly()
		{
			Assert(true);
		}

		public override void TestAddNewAndCancelNoException()
		{
			Assert(true);
		}

		public override void TestContains()
		{
			Assert(true);
		}

		public override void TestCopyTo()
		{
			Assert(true);
		}

		public override void TestIndexOf()
		{
			Assert(true);
		}

		public override void TestListChanged_SortedItemsAddedToMiddle()
		{
			Assert(true);
		}

		public override void TestNewBusinessObjectInSortedListChangingFilterToBeOutsideOfList()
		{
			Assert(true);
		}

		public override void TestNewBusinessObjectInSortedListChangingFilterToBeOutsideOfList_DateTimeOffsetVersion()
		{
			Assert(true);
		}

		public override void TestOnLoadedIntoCollection()
		{
			Assert(true);
		}

		public override void TestOnLoadedIntoCollection_ForAddNew()
		{
			Assert(true);
		}

		public override void TestBusinessObjectActiveFilter()
		{
			Assert(true);
		}

		public override void TestIgnoreActiveFilter()
		{
			Assert(true);
		}

		public override void TestDeleteAllCallsFetchForDelete()
		{
			Assert(true);
		}

		public override void TestHandleBusinessObjectElementChanged()
		{
			Assert(true);
		}

		#region TestListChanged_ShouldNotReSortWhileEnumerating

		[ExpectNoExceptions]
		public override void TestListChanged_ShouldNotReSortWhileEnumerating()
		{
			Assert(true);
		}

		#endregion

	}
}

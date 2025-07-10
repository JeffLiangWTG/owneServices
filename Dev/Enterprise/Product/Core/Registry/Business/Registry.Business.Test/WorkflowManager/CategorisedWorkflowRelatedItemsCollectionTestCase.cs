using System.Linq;

namespace Enterprise.Registry.Business.Testing
{
	public abstract class CategorisedWorkflowRelatedItemsCollectionTestCase<TCollection, TParent> : RegistryBusinessObjectCollectionTestCase<TCollection>
			where TCollection : RegistryBusinessObjectCollection, ICategorisedWorkflowRelatedItemsCollection<TParent>
			where TParent : RegistryBusinessObject, ICategorisedRegistryBusinessObjectCollection
	{
		public void TestGetOrCreateInnerCollectionFromWorkflowCode_ShouldReturnNull_OnNullOrEmptyCode()
		{
			AssertNull(Collection.GetOrCreateInnerCollectionFromWorkflowCode_ForTest(null));
			AssertNull(Collection.GetOrCreateInnerCollectionFromWorkflowCode_ForTest(string.Empty));
		}

		public void TestDeleteDoesNotCorruptOnResync()
		{
			Collection.SynchroniseWithWorkflowDescriptorList();
			var count = Collection.Count;

			var parent = ((ICategorisedWorkflowRelatedItemsCollection<TParent>)Collection)[0];
			parent.InnerCollection.RemoveAll();
			Collection.MarkAsRequiringSyncronisationWithWorkflowDescriptorList_ForTest();
			Collection.SynchroniseWithWorkflowDescriptorList();
			AssertEquals(count, Collection.Count);
		}

		public void TestSynchroniseWithWorkflowDescriptorListWillAddTaskDescriptionIfCodeNotInCollection()
		{
			Collection.RemoveAll();
			AssertEquals(0, Collection.Count);

			Collection.SynchroniseWithWorkflowDescriptorList();

			var workflowDescriptorList = WorkflowDataRegistryHelper.GetWorkflowDescriptorList();
			var expectedDescriptions = workflowDescriptorList.ToArray().Select(x => x.Description);
			var actualDescriptions = Collection.Select(x => (string)((RegistryBusinessObject)x).Description);
			AssertContainsExactElementsInExactOrder("Descriptions should be added", expectedDescriptions, actualDescriptions);
		}

		public void TestSynchroniseWithWorkflowDescriptorListWillAddTaskDescriptionIfCodeAlreadyInCollectionButDidNotHaveDescription()
		{
			Collection.RemoveAll();
			AssertEquals(0, Collection.Count);

			Collection.SynchroniseWithWorkflowDescriptorList();
			foreach (RegistryBusinessObject bizo in Collection)
			{
				bizo.Description = null;
			}

			Collection.MarkAsRequiringSyncronisationWithWorkflowDescriptorList_ForTest();
			Collection.SynchroniseWithWorkflowDescriptorList();

			var workflowDescriptorList = WorkflowDataRegistryHelper.GetWorkflowDescriptorList();
			var expectedDescriptions = workflowDescriptorList.ToArray().Select(x => x.Description);
			var actualDescriptions = Collection.Select(x => (string)((RegistryBusinessObject)x).Description);
			AssertContainsExactElementsInExactOrder("Descriptions should be added", expectedDescriptions, actualDescriptions);
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected new TCollection Collection => base.Collection;
	}
}

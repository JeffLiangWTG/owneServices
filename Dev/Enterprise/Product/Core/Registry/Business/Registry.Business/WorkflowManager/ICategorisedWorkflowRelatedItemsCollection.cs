using CargoWise.Integration;

namespace Enterprise.Registry.Business
{
	public interface ICategorisedWorkflowRelatedItemsCollection<TParent> : ICodeDescriptionPairList
		where TParent : RegistryBusinessObject, ICategorisedRegistryBusinessObjectCollection
	{
		void SynchroniseWithWorkflowDescriptorList();

		new TParent this[int i] { get; }

#if DEBUG

		void MarkAsRequiringSyncronisationWithWorkflowDescriptorList_ForTest();

		RegistryBusinessObjectCollection GetOrCreateInnerCollectionFromWorkflowCode_ForTest(string code);
#endif
	}
}

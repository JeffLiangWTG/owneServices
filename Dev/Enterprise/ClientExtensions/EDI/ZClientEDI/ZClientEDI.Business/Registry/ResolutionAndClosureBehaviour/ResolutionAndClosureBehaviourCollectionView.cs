using CargoWise.Types;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.Client.EDI.Registry
{
	public class ResolutionAndClosureBehaviourCollectionView : CodeDescriptionBoolTreeView
	{
		public ResolutionAndClosureBehaviourCollectionView(ResolutionAndClosureBehaviourCollection allNodes, ZGuid parentID)
			: base(allNodes, parentID)
		{
		}

		public new ResolutionAndClosureBehaviour AddNew() => (ResolutionAndClosureBehaviour)base.AddNew();

		public new ResolutionAndClosureBehaviour this[int i] => (ResolutionAndClosureBehaviour)Elements[i];

		public new ResolutionAndClosureBehaviour FindByCode(string code) => (ResolutionAndClosureBehaviour)base.FindByCode(code);

		protected override CodeDescriptionBoolTreeNode CreateNonPersistentBusinessObject()
		{
			return new ResolutionAndClosureBehaviour();
		}
	}
}


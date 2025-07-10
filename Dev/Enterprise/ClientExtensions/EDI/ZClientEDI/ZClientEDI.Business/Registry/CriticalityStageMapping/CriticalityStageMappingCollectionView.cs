using CargoWise.Types;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.Client.EDI.Registry
{
	public class CriticalityStageMappingCollectionView : CodeDescriptionBoolTreeView
	{
		public CriticalityStageMappingCollectionView(CriticalityStageMappingCollection allNodes, ZGuid parentID)
			: base(allNodes, parentID)
		{
		}

		public new CriticalityStageMapping AddNew()
		{
			return (CriticalityStageMapping)base.AddNew();
		}

		public new CriticalityStageMapping this[int i]
		{
			get { return (CriticalityStageMapping)Elements[i]; }
		}

		protected override CodeDescriptionBoolTreeNode CreateNonPersistentBusinessObject()
		{
			return new CriticalityStageMapping();
		}

		protected override bool AllowNewCore
		{
			get { return base.AllowNewCore && !ParentID.IsEmpty; }
		}
	}
}


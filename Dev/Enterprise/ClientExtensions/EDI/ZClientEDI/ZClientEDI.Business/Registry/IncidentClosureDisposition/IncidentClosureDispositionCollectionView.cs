using CargoWise.Types;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.Client.EDI.Registry
{
	public class IncidentClosureDispositionCollectionView : CodeDescriptionBoolTreeView
	{
		public IncidentClosureDispositionCollectionView(IncidentClosureDispositionCollection allNodes, ZGuid parentID)
			: base(allNodes, parentID)
		{
		}

		public new IncidentClosureDisposition AddNew()
		{
			return (IncidentClosureDisposition)base.AddNew();
		}

		public new IncidentClosureDisposition this[int i]
		{
			get { return (IncidentClosureDisposition)Elements[i]; }
		}

		protected override CodeDescriptionBoolTreeNode CreateNonPersistentBusinessObject()
		{
			return new IncidentClosureDisposition();
		}

		protected override bool AllowNewCore => true;
	}
}


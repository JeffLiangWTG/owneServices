using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	/// <summary>
	/// For opening old Dev Work Item hyperlinks with ControllerID=NewWorkItem
	/// </summary>
	public class ObsoleteNonGenericWorkItemControllerForOldHyperlinksOnly : EDIWorkItemController
	{
		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.ObsoleteNonGenericWorkItemForOldHyperlinksOnly; }
		}
	}
}

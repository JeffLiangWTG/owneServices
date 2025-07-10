using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	/// <summary>
	/// For opening old hyperlinks with ControllerID=EngineeringTask
	/// </summary>
	public class EngineeringTaskController : EDIWorkItemController
	{
		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.EngineeringTask; }
		}
	}
}

using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	/// <summary>
	/// For opening Project hyperlinks with the transcient ControllerID=EDIProject that got created
	/// in-between Generic Projects and WiseTech Projects which are based on Generic Projects
	/// </summary>
	public class ProjectController : EDIProjectController
	{
		public override ControllerID ID
		{
			get { return Modules.ClientControllerRegistration.Project; }
		}
	}
}

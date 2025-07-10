using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(ObsoleteNonGenericWorkItemControllerForOldHyperlinksOnly))]
	public class NewWorkItemControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.ObsoleteNonGenericWorkItemForOldHyperlinksOnly;
		}
	}
}

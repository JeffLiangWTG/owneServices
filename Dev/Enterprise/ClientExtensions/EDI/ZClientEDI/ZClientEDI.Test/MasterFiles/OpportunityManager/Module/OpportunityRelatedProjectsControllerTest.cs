using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(OpportunityRelatedProjectsController))]
	internal class OpportunityRelatedProjectsControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.OpportunityRelatedProjects;
		}
	}
}

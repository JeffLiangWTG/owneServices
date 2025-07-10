using Enterprise.Client.EDI.IncidentManager.Module;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(PSQRelatedOpportunitiesController))]
	internal class PSQRelatedOpportunitiesControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.PSQRelatedOpportunities;
		}
	}
}

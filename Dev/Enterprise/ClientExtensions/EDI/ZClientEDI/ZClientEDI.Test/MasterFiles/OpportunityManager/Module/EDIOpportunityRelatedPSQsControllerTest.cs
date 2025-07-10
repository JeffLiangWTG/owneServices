using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDIOpportunityRelatedPSQsController))]
	internal class EDIOpportunityRelatedPSQsControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ClientControllerRegistration.EDIOpportunityRelatedPSQs;
		}
	}
}

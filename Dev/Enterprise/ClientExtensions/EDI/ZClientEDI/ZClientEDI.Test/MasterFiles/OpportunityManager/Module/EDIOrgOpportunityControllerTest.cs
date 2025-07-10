using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(EDIOrgOpportunityController))]
	internal class EDIOrgOpportunityControllerTest : ZControllerBasherTest
	{
		public void TestForm()
		{
			EDIOrgOpportunityController controller = new EDIOrgOpportunityController();
			EDIOrgOpportunity opportunity = Factory.New<EDIOrgOpportunity>();
			using (IZForm form = controller.ShowFormForNewEntity(opportunity))
			{
				AssertEquals(typeof(EDIOpportunityManagementForm), form.GetType());
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Opportunity;
		}
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	internal class OpportunityRelatedProjectsPlugInTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			using (OpportunityRelatedProjectsPlugIn plugIn = new OpportunityRelatedProjectsPlugIn(Opportunity))
			{
				AssertEquals(typeof(OpportunityRelatedProjectsUserControl), plugIn.UserControl.GetType());
				AssertEquals(Opportunity, plugIn.BusinessEntity);
				AssertEquals("Related Projects", plugIn.Name);
			}
		}

		EDIOrgOpportunity Opportunity
		{
			get
			{
				return opportunity ?? (opportunity = Factory.New<EDIOrgOpportunity>());
			}
		}

		EDIOrgOpportunity opportunity;
	}
}

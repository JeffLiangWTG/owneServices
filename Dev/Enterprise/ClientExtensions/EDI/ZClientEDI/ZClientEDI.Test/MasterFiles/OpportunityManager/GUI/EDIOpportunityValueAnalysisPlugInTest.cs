using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	public class EDIOpportunityValueAnalysisPlugInTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			using (EDIOpportunityValueAnalysisPlugInForTest plugIn = new EDIOpportunityValueAnalysisPlugInForTest(Opportunity))
			{
				AssertEquals(typeof(EDIOpportunityValueAnalysisUserControl), plugIn.UserControl.GetType());
				AssertEquals(Opportunity, plugIn.BusinessEntity);
				AssertEquals("Value Analysis", plugIn.Name);
			}
		}

		public void TestGetNewUserControl()
		{
			using (EDIOpportunityValueAnalysisPlugInForTest plugIn = new EDIOpportunityValueAnalysisPlugInForTest(Opportunity))
			{
				using (EDIOpportunityValueAnalysisUserControl userControl = plugIn.UserControl as EDIOpportunityValueAnalysisUserControl)
				{
					AssertNotNull("GetNewUserControl", userControl);
				}
			}
		}

		#region Implementation

		class EDIOpportunityValueAnalysisPlugInForTest : EDIOpportunityValueAnalysisPlugIn
		{
			public EDIOpportunityValueAnalysisPlugInForTest(EDIOrgOpportunity hostBusinessEntity) : base(hostBusinessEntity)
			{
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
		#endregion
	}
}

using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI.Testing
{
	public class EDIOpportunityRelatedPSQsPlugInTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			using (EDIOpportunityRelatedPSQsPlugInForTest plugIn = new EDIOpportunityRelatedPSQsPlugInForTest(Opportunity))
			{
				AssertEquals(typeof(EDIOpportunityRelatedPSQsUserControl), plugIn.UserControl.GetType());
				AssertEquals(Opportunity, plugIn.BusinessEntity);
				AssertEquals("Related PSQs", plugIn.Name);
			}
		}

		public void TestGetNewUserControl()
		{
			using (EDIOpportunityRelatedPSQsPlugInForTest plugIn = new EDIOpportunityRelatedPSQsPlugInForTest(Opportunity))
			{
				using (EDIOpportunityRelatedPSQsUserControl userControl = plugIn.UserControl as EDIOpportunityRelatedPSQsUserControl)
				{
					AssertNotNull("GetNewUserControl", userControl);
				}
			}
		}

		#region Implementation

		class EDIOpportunityRelatedPSQsPlugInForTest : EDIOpportunityRelatedPSQsPlugIn
		{
			public EDIOpportunityRelatedPSQsPlugInForTest(EDIOrgOpportunity hostBusinessEntity) : base(hostBusinessEntity)
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

using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI.Testing
{
	public class PSQOpportunitiesPlugInTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			using (PSQOpportunitiesPlugInForTest plugIn = new PSQOpportunitiesPlugInForTest(PSQ))
			{
				AssertEquals(typeof(PSQRelatedOpportunitiesUserControl), plugIn.UserControl.GetType());
				AssertEquals(PSQ, plugIn.BusinessEntity);
				AssertEquals("Related Opportunities", plugIn.Name);
			}
		}

		public void TestGetNewUserControl()
		{
			using (PSQOpportunitiesPlugInForTest plugIn = new PSQOpportunitiesPlugInForTest(PSQ))
			{
				using (PSQRelatedOpportunitiesUserControl userControl = plugIn.UserControl as PSQRelatedOpportunitiesUserControl)
				{
					AssertNotNull("GetNewUserControl", userControl);
				}
			}
		}

		#region Implementation

		class PSQOpportunitiesPlugInForTest : PSQRelatedOpportunitiesPlugIn
		{
			public PSQOpportunitiesPlugInForTest(ProfessionalServicesQuote hostBusinessEntity) : base(hostBusinessEntity)
			{
			}
		}

		ProfessionalServicesQuote PSQ
		{
			get
			{
				return psq ?? (psq = Factory.NewWithValidTestData<ProfessionalServicesQuote>());
			}
		}

		ProfessionalServicesQuote psq;
		#endregion
	}
}

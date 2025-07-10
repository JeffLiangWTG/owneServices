using CargoWise.Application;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.GUI.Testing
{
	public class ApportionmentSecurityOverrideProviderTest : SecurityOverrideProviderWithJobReopenSupportTest<ApportionmentSecurityOverrideProvider>
	{
		protected override ApportionmentSecurityOverrideProvider GetSecurityProvider(bool showApprovalRequestButton = false, bool alwaysCreateApprovalRequest = false, bool keepLoginFormResultAfterFirstUserAnswer = false)
		{
			return new ApportionmentSecurityOverrideProvider(new ApportionmentListing(Factory, (IJobCostingPlugIn)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.Forwarding.IForwardingConsol)))));
		}

		protected override bool ShouldPromptForGranted
		{
			get { return true; }
		}

		protected override bool IsApprovalRequestButtonSupported
		{
			get { return false; }
		}
	}
}

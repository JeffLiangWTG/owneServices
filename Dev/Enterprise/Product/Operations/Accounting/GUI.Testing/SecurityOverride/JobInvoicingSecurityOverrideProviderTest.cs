namespace Enterprise.Accounting.GUI.Testing
{
	public class JobInvoicingSecurityOverrideProviderTest : SecurityOverrideProviderWithJobReopenSupportTest<JobInvoicingSecurityOverrideProvider>
	{
		protected override JobInvoicingSecurityOverrideProvider GetSecurityProvider(bool showApprovalRequestButton = false, bool alwaysCreateApprovalRequest = false, bool keepLoginFormResultAfterFirstUserAnswer = false)
		{
			return new JobInvoicingSecurityOverrideProvider();
		}

		protected override bool ShouldPromptForGranted
		{
			get	{ return false;	}
		}

		protected override bool IsApprovalRequestButtonSupported
		{
			get { return false; }
		}
	}
}

namespace Enterprise.Accounting.GUI
{
	public class JobInvoicingSecurityOverrideProvider : SecurityOverrideProviderWithJobReopenSupport
	{
		protected override string GetReopenClosedJobSecurityOverrideMessage()
		{
			return SecurityOverrideProviderWithJobReopenSupport.ReopenClosedJobSecurityOverrideMessageOneJob;
		}

		protected override string GetReopenRestrictedClosedJobSecurityOverrideMessage()
		{
			return SecurityOverrideProviderWithJobReopenSupport.ReopenRestrictedClosedJobSecurityOverrideMessageOneJob;
		}

		protected override bool ShouldPromptForGranted
		{
			get { return false; }
		}
	}
}

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class WithAlternateEmailBodyHeaderFooter : NotificationBufferForTest
	{
		protected override string EmailBodyHeader
		{
			get { return "Alternate Header"; }
		}

		protected override string EmailBodyFooter
		{
			get { return "Alternate Footer"; }
		}
	}
}

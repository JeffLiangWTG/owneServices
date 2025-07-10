namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	using System;

	class OverseasAgentChargesPosterEmail : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get
			{
				return typeof(JobRevenuePosterEmail);
			}
		}
	}
}

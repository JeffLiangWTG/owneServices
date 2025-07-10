using System;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Messaging.Business.Testing
{
	class InboundInterchangeProcessorWithEnvironmentSwitchForTesting : InboundInterchangeProcessorForTesting2
	{
		public InboundInterchangeProcessorWithEnvironmentSwitchForTesting(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool SupportEnvironmentSwitch => true;
		protected override bool IsNoBranchFilter => true;
		protected override bool ProcessInterchange(EDIInterchange interchange)
		{
			if (interchange.EI_GB != GlbBranch.CurrentBranch.PK)
			{
				throw new InvalidOperationException("Environment should have been switched");
			}
			return true;
		}
	}
}

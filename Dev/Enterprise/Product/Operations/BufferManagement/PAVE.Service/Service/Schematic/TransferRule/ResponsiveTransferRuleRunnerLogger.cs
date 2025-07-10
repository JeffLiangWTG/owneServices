using System;
using CargoWise.PAVE.Common.Interfaces;
using Enterprise.BufferManagement.Business;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Service
{
	internal class ResponsiveTransferRuleRunnerLogger : TransferRuleRunnerLogger
	{
		public ResponsiveTransferRuleRunnerLogger(ILogger logger, IPAVESystem system)
			: base(logger, system)
		{
		}

		protected override string GetFormattedMessage(string message)
		{
			return FormattableString.Invariant($"{base.GetFormattedMessage(message)} [{nameof(ResponsiveTransferRuleRunner)}]"); // Service Task Logging
		}
	}
}

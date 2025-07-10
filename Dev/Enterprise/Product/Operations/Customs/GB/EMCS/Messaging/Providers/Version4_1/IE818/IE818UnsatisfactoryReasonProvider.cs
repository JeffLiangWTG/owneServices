using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie818;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS.Messaging.Version4_1
{
	public sealed class IE818UnsatisfactoryReasonProvider : IIE818UnsatisfactoryReason
	{
		public IE818UnsatisfactoryReasonProvider(UnsatisfactoryReasonType reason)
		{
			this.reason = Argument.NotNull(reason, nameof(reason));
		}
		readonly UnsatisfactoryReasonType reason;

		public ZString ReasonCode => reason.UnsatisfactoryReasonCode;

		public ZString ComplementaryInformation => reason.ComplementaryInformation?.Value;
	}
}

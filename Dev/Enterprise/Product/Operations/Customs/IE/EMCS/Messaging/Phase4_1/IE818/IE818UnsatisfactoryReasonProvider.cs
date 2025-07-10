using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.EMCSPhase4_1.IE818;
using CargoWise.Types;

namespace Enterprise.Customs.IE.EMCS.Messaging.Phase4_1
{
	public class IE818UnsatisfactoryReasonProvider : IIE818UnsatisfactoryReason
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

using CargoWise.Common;
using CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging.Version2_4
{
	public class ED818UnsatisfactoryReasonProvider : IED818UnsatisfactoryReason
	{
		public ED818UnsatisfactoryReasonProvider(ED818CBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceiptUnsatisfactoryReason reason)
		{
			this.reason = Argument.NotNull(reason, nameof(reason));
		}

		readonly ED818CBodyAcceptedOrRejectedReportOfReceiptBodyReportOfReceiptUnsatisfactoryReason reason;

		public ZString ReasonCode => reason.UnsatisfactoryReasonCode;

		public ZString ComplementaryInformation => reason.ComplementaryInformation;
	}
}

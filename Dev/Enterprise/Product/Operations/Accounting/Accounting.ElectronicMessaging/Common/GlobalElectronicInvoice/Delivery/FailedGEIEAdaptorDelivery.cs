using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class FailedGEIEAdaptorDelivery : GEIEAdaptorDelivery
	{
		protected override string GetRecipientID(IEDICommunicationsMode mode)
		{
			return mode.EK_Destination;
		}

		protected override string InterchangeQueuedStatus => EDIInterchangeStatusList.Codes.Failed;
		protected override string MessageQueuedStatus => EDIMessageStatusList.Codes.Failed;
	}
}

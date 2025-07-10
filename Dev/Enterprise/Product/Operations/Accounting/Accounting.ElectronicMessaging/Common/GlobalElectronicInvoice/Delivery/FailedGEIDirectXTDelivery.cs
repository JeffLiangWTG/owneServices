using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class FailedGEIDirectXTDelivery : GEIDirectXTDelivery
	{
		protected override string InterchangeQueuedStatus => EDIInterchangeStatusList.Codes.Failed;
		protected override string MessageQueuedStatus => EDIMessageStatusList.Codes.Failed;
	}
}

using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class FailedGEIEHubDelivery : GEIEHubDelivery
	{
		protected override string InterchangeQueuedStatus => EDIInterchangeStatusList.Codes.Failed;
		protected override string MessageQueuedStatus => EDIMessageStatusList.Codes.Failed;
	}
}

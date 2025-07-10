using System.Linq;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.MessageStatusAcknowledgment)]
	class MessageStatusAcknowledgmentHandler : MessageStatusHandler
	{
		protected override void UpdateInterchangeStatus(EDIInterchange interchange)
		{
			new MessageStatusManager(interchange).Update(EDIInterchangeStatusList.Codes.Acknowledged);
		}

		protected override string BestMatchOutgoingInterchangeStatus
		{
			get { return EDIInterchange.Status.Sent; }
		}

		protected override string[] SupportedMatchOutgoingInterchangeStatuses
		{
			get { return new string[] { EDIInterchange.Status.Failed, EDIInterchange.Status.eHubPending, EDIInterchange.Status.eHubQueued }
					.Concat(SupportedLogOnlyMatchOutgoingInterchangeStatuses).ToArray();
			}
		}

		protected override string[] SupportedLogOnlyMatchOutgoingInterchangeStatuses
		{
			get
			{
				return new string[]
				{
					EDIInterchange.Status.Acknowledged
				};
			}
		}

		protected override string GetInterchangeType()
		{
			return EDIInterchangeTypeList.Codes.MessageStatusAcknowledgment;
		}

		protected override string ResultStatus => EDIInterchange.Status.Acknowledged;
	}
}

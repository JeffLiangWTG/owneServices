using System.Linq;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.MSS)]
	class MessageStatusSuccessHandler : MessageStatusHandler
	{
		protected override void UpdateInterchangeStatus(EDIInterchange interchange)
		{
			new MessageStatusManager(interchange).Update(EDIInterchange.Status.Sent);
		}

		protected override string BestMatchOutgoingInterchangeStatus
		{
			get { return EDIInterchange.Status.eHubPending; }
		}

		protected override string[] SupportedMatchOutgoingInterchangeStatuses
		{
			get
			{
				return new string[] { EDIInterchange.Status.Failed, EDIInterchange.Status.eHubQueued }
					.Concat(SupportedLogOnlyMatchOutgoingInterchangeStatuses).ToArray();
			}
		}

		protected override string[] SupportedLogOnlyMatchOutgoingInterchangeStatuses
		{
			get
			{
				return new string[] {
					EDIInterchange.Status.Acknowledged,
					EDIInterchange.Status.Sent
				};
			}
		}

		protected override string GetInterchangeType()
		{
			return EDIInterchangeTypeList.Codes.MSS;
		}

		protected override string ResultStatus => EDIInterchange.Status.Sent;
	}
}

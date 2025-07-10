using System.Linq;
using CargoWise.eHub.Common.Extensions;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.MSF)]
	class MessageStatusFailedHandler : MessageStatusHandler
	{
		protected override void UpdateInterchangeStatus(EDIInterchange interchange)
		{
			new MessageStatusManager(interchange).Fail(Message.MessageStream.ReadToEnd());
		}

		protected override string BestMatchOutgoingInterchangeStatus
		{
			get { return EDIInterchange.Status.eHubPending; }
		}

		protected override string[] SupportedMatchOutgoingInterchangeStatuses
		{
			get
			{
				return new string[] { EDIInterchange.Status.eHubQueued }
					.Concat(SupportedLogOnlyMatchOutgoingInterchangeStatuses).ToArray();
			}
		}

		protected override string[] SupportedLogOnlyMatchOutgoingInterchangeStatuses
		{
			get
			{
				return new string[]
				{
					EDIInterchange.Status.Acknowledged,
					EDIInterchange.Status.Sent,
					EDIInterchange.Status.Failed
				};
			}
		}

		protected override string GetInterchangeType()
		{
			return EDIInterchangeTypeList.Codes.MSF;
		}

		protected override string ResultStatus => EDIInterchange.Status.Failed;
	}
}

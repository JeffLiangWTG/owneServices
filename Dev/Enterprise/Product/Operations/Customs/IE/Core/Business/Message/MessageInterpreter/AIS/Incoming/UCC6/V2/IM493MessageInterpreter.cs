using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM493MessageInterpreter : InboundMessageInterpreter<IM493Provider>
	{
		public IM493MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM493Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("1558CE2D-95F8-40EF-B9B1-412C07A1E958", "An Amendment Notification for Partial or Deferred Quota Allocation (IM493) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
		}
	}
}

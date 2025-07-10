using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR864CMessageInterpreter : InboundMessageInterpreter<TR864CProvider>
	{
		public TR864CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TR864CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("457DE476-E7BB-4951-A1C6-621B0C85C48C", "A Declaration Invalidation Request Cancellation (TR864) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (Res.GetString("325FF426-5D52-4276-8944-BDC2318B71B3", "Invalidation Request Cancellation Reason"), provider.InvalidationRequestCancellationReason);
		}
	}
}

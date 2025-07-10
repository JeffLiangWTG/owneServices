using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR862CMessageInterpreter : InboundMessageInterpreter<TR862CProvider>
	{
		public TR862CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TR862CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("5D752A0C-067F-44A1-95A0-EB78B193118A", "A Declaration Amendment Request Cancellation (TR862) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (Res.GetString("852717B9-EF35-4B97-9901-201486067EE7", "Amendment request cancellation Reason"), provider.AmendmentRequestCancellationReason);
		}
	}
}

using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR884CMessageInterpreter : InboundMessageInterpreter<TR884CProvider>
	{
		public TR884CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TR884CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("5A3D6CFD-86BB-4A78-9924-D0C2DE1173A6", "A Document Presentation Request Cancellation (TR884) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (Res.GetString("FD63391C-11CA-4945-9034-906A01231FA1", "Document Presentation Request Cancellation Reason"), provider.PresentationRequestCancellationReason);
		}
	}
}

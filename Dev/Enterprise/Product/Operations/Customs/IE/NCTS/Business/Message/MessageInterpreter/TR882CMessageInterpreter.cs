using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class TR882CMessageInterpreter : InboundMessageInterpreter<TR882CProvider>
	{
		public TR882CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TR882CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("A3242166-044C-4CAD-AF58-3E316149B6BF", "A Document Upload Request Cancellation (TR882) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (Res.GetString("C7A8ADB0-CA54-4992-8881-AECCD0114A21", "Document Upload Request Cancellation Reason"), provider.UploadRequestCancellationReason);
		}
	}
}

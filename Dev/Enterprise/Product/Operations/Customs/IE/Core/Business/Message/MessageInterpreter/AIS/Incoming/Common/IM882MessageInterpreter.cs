using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM882MessageInterpreter : InboundMessageInterpreter<IM882Provider>
	{
		public IM882MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM882Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("3908388B-698A-4077-B576-192A8725FEB8", "A Documents Upload Request Cancellation (IM882) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (CommonResStrings.DocumentsUploadRequestCancellationReason, provider.DocumentsUploadRequestCancellationReason);
		}
	}
}

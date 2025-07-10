using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM884MessageInterpreter : InboundMessageInterpreter<IM884Provider>
	{
		public IM884MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM884Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("BD295361-0375-4940-B501-4BF5FC2DCBC2", "A Documents Presentation Request Cancellation (IM884) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (CommonResStrings.DocumentsPresentRequestCancellationReason, provider.CancellationReason);
		}
	}
}

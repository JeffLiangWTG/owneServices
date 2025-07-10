using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS333MessageInterpreter : InboundMessageInterpreter<TS333Provider>
	{
		public TS333MessageInterpreter(AISUCC5InboundEDIMessage message, TS333Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("725979C5-45E5-4E84-8A8C-3E5B7B0A149C", "A Presentation Notification Rejection (TS333) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.RejectionDate, provider.RejectionDate.ToShortDateString());
			yield return (CommonResStrings.RejectionReason, provider.RejectionReason);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
			=> provider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}

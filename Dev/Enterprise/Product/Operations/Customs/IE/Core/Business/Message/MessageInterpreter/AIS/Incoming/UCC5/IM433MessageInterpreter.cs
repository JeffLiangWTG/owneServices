using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM433MessageInterpreter : InboundMessageInterpreter<IM433Provider>
	{
		public IM433MessageInterpreter(AISUCC5InboundEDIMessage message, IM433Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("8C53E37E-1AF9-445A-B7DB-C30E0186B461", "A Presentation Notification Rejection (IM433) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.RejectionDate, provider.RejectionDate.ToShortDateString());
			yield return (CommonResStrings.RejectionReason, provider.RejectionReason);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails() =>
			provider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}

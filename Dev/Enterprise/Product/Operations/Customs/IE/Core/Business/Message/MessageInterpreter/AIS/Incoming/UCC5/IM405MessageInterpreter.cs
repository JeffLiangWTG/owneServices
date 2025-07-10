using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM405MessageInterpreter : InboundMessageInterpreter<IM405Provider>
	{
		public IM405MessageInterpreter(AISUCC5InboundEDIMessage message, IM405Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("88EC38A9-E918-439D-B09C-718F07EBD9A6", "An Amendment Request Rejection (IM405) message has been received from customs for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.AmendmentRejectionDate, provider.AmendmentRejectionDate.ToShortDateString());
			yield return (CommonResStrings.AmendmentRejectionReason, provider.AmendmentRejectionMotivationText);
			yield return (CommonResStrings.Remarks, provider.Remarks);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails() =>
			provider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}

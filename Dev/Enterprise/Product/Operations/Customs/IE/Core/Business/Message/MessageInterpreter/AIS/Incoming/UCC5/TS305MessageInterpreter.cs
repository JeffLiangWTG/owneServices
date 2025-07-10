using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS305MessageInterpreter : InboundMessageInterpreter<TS305Provider>
	{
		public TS305MessageInterpreter(AISUCC5InboundEDIMessage message, TS305Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("FCBFECCA-56D9-4F05-9231-E01791A46184", "An Amendment Request Rejection (TS305) message has been received for TSD {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.AmendmentRejectionDate, provider.AmendmentRejectionDate.ToLongTimeString());
			yield return (CommonResStrings.AmendmentRejectionReason, provider.RejectionReason);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
			=> provider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}

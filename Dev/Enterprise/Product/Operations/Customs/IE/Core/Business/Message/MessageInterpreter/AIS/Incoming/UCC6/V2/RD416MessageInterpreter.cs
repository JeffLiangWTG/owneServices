using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public sealed class RD416MessageInterpreter : InboundMessageInterpreter<RD416Provider>
	{
		public RD416MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, RD416Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => CommonResStrings.GetRD416MessageInterpreterSummary(relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.ApplicationReferenceID, provider.ApplicationReferenceId);
			yield return (CommonResStrings.RejectionDate, provider.RejectionDate.ToShortDateString());
			yield return (CommonResStrings.RejectionReason, provider.RejectionReason);
			yield return (CommonResStrings.ApplicantEORINumber, provider.Applicant);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
			=> provider.FunctionalErrors.GetFunctionalErrorDetails(MessageCreatedDate, factory);
	}
}

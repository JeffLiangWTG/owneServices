using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
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
			yield return (Res.GetString("744086C1-558C-4510-8C6B-E800DB53E518", "Applicant"), provider.Applicant);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
			=> provider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}

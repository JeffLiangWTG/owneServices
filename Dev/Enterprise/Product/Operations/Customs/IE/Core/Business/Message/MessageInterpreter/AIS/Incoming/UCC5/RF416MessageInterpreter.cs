using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public sealed class RF416MessageInterpreter : InboundMessageInterpreter<RF416Provider>
	{
		public RF416MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, RF416Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => CommonResStrings.GetRF416MessageInterpreterSummary(relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.ApplicationReferenceID, provider.ApplicationReferenceId);
			yield return (CommonResStrings.RejectionDateAndTime, provider.RejectionDate);
			yield return (CommonResStrings.RejectionReason, provider.RejectionReason);
			yield return (CommonResStrings.DecisionTakingCustomsAuthority, provider.DecisionTakingCustomsAuthority);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
			=> provider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}

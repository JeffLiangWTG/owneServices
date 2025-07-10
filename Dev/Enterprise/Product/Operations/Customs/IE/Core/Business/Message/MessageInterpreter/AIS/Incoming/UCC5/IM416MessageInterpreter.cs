using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM416MessageInterpreter : InboundMessageInterpreter<IM416Provider>
	{
		public IM416MessageInterpreter(AISUCC5InboundEDIMessage message, IM416Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => CommonResStrings.GetIM416MessageInterpreterSummary(relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.AdditionalDeclarationType, provider.AdditionalDeclarationType);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.RejectionDate, provider.RejectionDate.ToShortDateString());
			yield return (CommonResStrings.RejectionMotivationText, provider.RejectionMotivationText);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails() =>
			provider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}

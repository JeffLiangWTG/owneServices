using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS316MessageInterpreter : InboundMessageInterpreter<TS316Provider>
	{
		public TS316MessageInterpreter(AISUCC5InboundEDIMessage message, TS316Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => CommonResStrings.GetTS316MessageInterpreterSummary(relatedJob.JobNumber);

		protected override string MessageDetailsSummary => CommonResStrings.TS316MessageInterpreterMessageDetailsSummary;

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LRN);
			yield return (CommonResStrings.DeclarationRejectionDate, provider.RejectionDate.ToShortDateString());
			yield return (CommonResStrings.DeclarationRejectionReason, provider.RejectionMotivationText);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails() =>
			provider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}

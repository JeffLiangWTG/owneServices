using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS316MessageInterpreter : InboundMessageInterpreter<TS316Provider>
	{
		public TS316MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TS316Provider provider) : base(message, provider)
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
			provider.FunctionalErrors.Select(x => (string.Empty, x.GetMessageDetails(GetCodeAndDescription(x.ErrorCode, UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL180))));
	}
}

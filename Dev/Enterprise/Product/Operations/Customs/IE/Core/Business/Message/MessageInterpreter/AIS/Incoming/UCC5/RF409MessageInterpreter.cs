using System.Collections.Generic;
using System.Linq;
using RF409Provider = Enterprise.Customs.IE.Messaging.UCC5.RF409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public sealed class RF409MessageInterpreter : InboundMessageInterpreter<RF409Provider>
	{
		public RF409MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, RF409Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => CommonResStrings.GetRF409MessageInterpreterSummary(relatedJob.JobNumber, provider.RefundApplicationAccepted);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.ApplicationReferenceID, provider.ApplicationReferenceId);
			yield return (CommonResStrings.ApplicationDecisionCodeType, provider.ApplicationDecisionCodeType);
			yield return (CommonResStrings.RefundApplicationAccepted, provider.RefundApplicationAccepted.ToString());
			yield return (CommonResStrings.DecisionTakingCustomsAuthority, provider.DecisionTakingCustomsAuthority);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.TimeLimitForCompletionOfFormalities, provider.TimeLimit);
			yield return (CommonResStrings.StatementOfTheDecisionTakingCustomsAuthority, provider.StatementOfTheDecision);
			yield return (CommonResStrings.DescriptionOfGrounds, provider.DescriptionOfGrounds);
		}

		protected override IEnumerable<(string summary, IEnumerable<string[]>)> GetAdditionalMessageWithFreeNumberOfColumns()
		{
			yield return (CommonResStrings.GeneralRemarks, MessageInterpreterHelper.GetGeneralRemarks(provider.GeneralRemarks.ToArray()));
		}
	}
}

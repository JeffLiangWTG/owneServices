using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public sealed class RD409MessageInterpreter : InboundMessageInterpreter<RD409Provider>
	{
		public RD409MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, RD409Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => CommonResStrings.GetRD409MessageInterpreterSummary(relatedJob.JobNumber, provider.ReasonNotApproved.IsEmpty);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.ApplicationReferenceID, provider.ApplicationReferenceId);
			yield return (CommonResStrings.Date, provider.Date);
			yield return (CommonResStrings.ApplicantEORINumber, provider.Applicant);
			yield return (CommonResStrings.DepositRefundApplicationApproved, provider.DepositRefundApplicationApproved.ToString());
			yield return (CommonResStrings.ReasonNotApproved, provider.ReasonNotApproved);
			yield return (CommonResStrings.StatementOfTheDecisionTakingCustomsAuthority, provider.StatementOfTheDecisionTakingCustomsAuthority);
		}
	}
}

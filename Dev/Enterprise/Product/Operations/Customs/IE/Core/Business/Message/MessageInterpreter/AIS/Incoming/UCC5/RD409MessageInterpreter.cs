using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
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

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
		{
			yield return (Res.GetString("87E767AE-2041-4073-8B44-811AE87E018D", "Amount of Deposit Refund"), new (string, string)[]
			{
				(Res.GetString("87E767AE-2041-4073-8B44-811AE87E018D", "Amount of Deposit Refund"), Res.GetString("529A71E2-C53E-47CE-BF14-8CBA93218059", "Payer EORI for Refund")),
				(provider.AmountOfDepositRefund.ToString(), provider.PayerEORIForRefund),
			});
		}
	}
}

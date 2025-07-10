using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC6.V1;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1
{
	public sealed class RF416MessageInterpreter : InboundMessageInterpreter<RF416Provider>
	{
		public RF416MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, RF416Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("C92414A7-8629-4B91-BA27-B20D32396F59", "A Refund Application Rejection (RF416) has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.ApplicationReferenceID, provider.ApplicationReferenceId);
			yield return (CommonResStrings.RejectionDateAndTime, provider.RejectionDate);
			yield return (CommonResStrings.RejectionReason, provider.RejectionReason);
			yield return (CommonResStrings.DecisionTakingCustomsAuthority, provider.DecisionTakingCustomsAuthority);
			yield return (Res.GetString("926BF7F3-ACB9-417D-A085-10BDD969BF6D", "Applicant/Holder of the authorization or decision identification"), provider.ApplicantOrHolderIdentification);
			yield return (Res.GetString("3C918889-7C01-455A-ACFA-07B235FB0059", "Representative Identification"), provider.RepresentativeIdentification);
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
			=> provider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}

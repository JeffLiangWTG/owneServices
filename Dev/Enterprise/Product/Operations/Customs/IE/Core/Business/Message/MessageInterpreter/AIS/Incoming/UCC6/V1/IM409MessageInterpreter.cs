using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;
using IM409Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1
{
	public class IM409MessageInterpreter : InboundMessageInterpreter<IIM409Provider>
	{
		public IM409MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM409Provider provider) : base(message, provider)
		{
			messageProvider = provider;
		}

		readonly IM409Provider messageProvider;

		protected override string Summary => Res.GetString("741646dd-6d46-447b-8fd6-c6bce1c73ab8", "An Invalidation Request Decision (IM409) message has been received from customs for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (CommonResStrings.InvalidationDecision, messageProvider.InvalidationDecision.ToString());
			yield return (CommonResStrings.InvalidationInitiatedByCustoms, messageProvider.InvalidationInitiatedByCustoms.ToString());
			yield return (CommonResStrings.InvalidationJustification, messageProvider.InvalidationJustification);
			yield return (CommonResStrings.DateOfInvalidationDecision, messageProvider.DateOfInvalidationDecision.ToShortDateString());
			yield return (CommonResStrings.DateOfInvalidationRequest, messageProvider.DateOfInvalidationRequest.ToShortDateString());
			yield return (CommonResStrings.DateOfInvalidation, messageProvider.DateOfInvalidation.ToShortDateString());
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails() =>
			messageProvider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}

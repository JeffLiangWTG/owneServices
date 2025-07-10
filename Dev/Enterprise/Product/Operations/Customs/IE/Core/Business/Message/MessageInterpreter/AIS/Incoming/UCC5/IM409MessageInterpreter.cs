using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM409MessageInterpreter : InboundMessageInterpreter<IM409Provider>
	{
		public IM409MessageInterpreter(AISUCC5InboundEDIMessage message, IM409Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("8D83D735-22FF-401A-9CE2-C651B19902A7", "An Invalidation Request Decision (IM409) message has been received from customs for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.InvalidationDecision, provider.InvalidationDecision.ToString());
			yield return (CommonResStrings.InvalidationInitiatedByCustoms, provider.InvalidationInitiatedByCustoms.ToString());
			yield return (CommonResStrings.InvalidationJustification, provider.InvalidationJustification);
			yield return (CommonResStrings.DateOfInvalidationDecision, provider.DateOfInvalidationDecision.ToShortDateString());
			yield return (CommonResStrings.DateOfInvalidationRequest, provider.DateOfInvalidationRequest.ToShortDateString());
			yield return (CommonResStrings.DateOfInvalidation, provider.DateOfInvalidation.ToShortDateString());
		}

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails() =>
			provider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}

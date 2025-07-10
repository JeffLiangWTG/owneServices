using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS309MessageInterpreter : InboundMessageInterpreter<TS309Provider>
	{
		public TS309MessageInterpreter(AISUCC5InboundEDIMessage message, TS309Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("B05EF3A3-6C6F-43C9-9392-D5ECD32ED250", "A Temporary Storage Declaration Invalidation Decision (TS309) message has been received for TSD {0}.", relatedJob.JobNumber);

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

		protected override IEnumerable<(string summary, IEnumerable<(string key, string value)>)> GetAdditionalMessageDetails()
			=> provider.FunctionalErrors.GetFunctionalErrorDetailsUCC5(MessageCreatedDate, factory);
	}
}

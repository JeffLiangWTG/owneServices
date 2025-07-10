using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM410MessageInterpreter : InboundMessageInterpreter<IM410Provider>
	{
		public IM410MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM410Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("9086A36A-23F2-49FA-ADEF-D5C6D42D9644", "An Invalidation of Customs Declaration (IM410) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.CustomsRegistrationNumber, provider.CustomsRegistrationNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.InvalidationDecisionDateAndTime, provider.InvalidationDecisionDateAndTime.ToLongTimeString());
			yield return (CommonResStrings.InvalidationRequestDateAndTime, provider.InvalidationRequestDateAndTime.ToLongTimeString());
			yield return (CommonResStrings.InvalidationInitiatedByCustoms, provider.InvalidationInitiatedByCustoms);
			yield return (CommonResStrings.InvalidationJustification, provider.InvalidationJustification);
		}
	}
}

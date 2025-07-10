using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX864MessageInterpreter : InboundMessageInterpreter<EX864Provider>
	{
		public EX864MessageInterpreter(AESInboundEDIMessage message, EX864Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"9D2947D2-5D40-48CE-B971-B75D7DC22770",
			"A Declaration Invalidation Request Cancellation message has been received from Customs for Job {0} through the EX864 message stating that Revenue have now decided to cancel the Invalidation request which was earlier sent through EX564 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (Res.GetString("5298E5D7-98E4-4001-94BA-3121885BFED0", "Invalidation Request Cancellation Reason"), provider.InvalidationRequestCancellationReason);
		}
	}
}

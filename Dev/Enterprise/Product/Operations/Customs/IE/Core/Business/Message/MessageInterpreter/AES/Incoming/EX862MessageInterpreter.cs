using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public class EX862MessageInterpreter : InboundMessageInterpreter<EX862Provider>
	{
		public EX862MessageInterpreter(AESInboundEDIMessage message, EX862Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"37F11806-A95C-412A-A713-74242EE02088",
			"A Declaration Amendment Request Cancellation message has been received from Customs for Job {0} through the EX862 message stating that declaration amendment request that was initiated through message EX562 has now been canceled. ",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.CaseId, provider.CaseId);
			yield return (Res.GetString("039E8CAF-AF09-4A80-BEE0-0BA9D13F7FAF", "Amendment Request Cancellation Reason"), provider.AmendmentRequestCancellationReason);
		}
	}
}

using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM862MessageInterpreter : InboundMessageInterpreter<IM862Provider>
	{
		public IM862MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM862Provider provider) : base(message, provider)
		{
			this.messageProvider = provider;
		}

		readonly IM862Provider messageProvider;

		protected override string Summary => Res.GetString("D68A9226-CFEF-45F0-995D-30251358C913", "A Declaration Amendment Request Cancellation (IM862) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (CommonResStrings.CaseId, messageProvider.CaseID);
			yield return (CommonResStrings.AmendmentRequestCancellationReason, messageProvider.AmendmentRequestCancellationReason);
		}
	}
}

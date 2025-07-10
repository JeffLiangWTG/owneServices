using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM864MessageInterpreter : InboundMessageInterpreter<IM864Provider>
	{
		public IM864MessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, IM864Provider provider) : base(message, provider)
		{
			this.messageProvider = provider;
		}

		readonly IM864Provider messageProvider;

		protected override string Summary => Res.GetString("2DFB31C5-B695-45BA-BCC3-A99D702768DD", "An Invalidation Request Cancellation (IM864) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, messageProvider.MovementReferenceNumber);
			yield return (CommonResStrings.CaseId, messageProvider.CaseId);
			yield return (CommonResStrings.InvalidationRequestCancellationReason, messageProvider.InvalidationRequestCancellationReason);
		}
	}
}

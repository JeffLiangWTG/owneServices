using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM451MessageInterpreter : InboundMessageInterpreter<IM451Provider>
	{
		public IM451MessageInterpreter(AISUCC5InboundEDIMessage message, IM451Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("0CE9657D-D823-4C83-9A90-D71F1B095AF9", "A Release Rejection (IM451) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.AdditionalDeclarationType, provider.AdditionalDeclarationType);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.RejectionReason, provider.RejectionReason);
		}
	}
}

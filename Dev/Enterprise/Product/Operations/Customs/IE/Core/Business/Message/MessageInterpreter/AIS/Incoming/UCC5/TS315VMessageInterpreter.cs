using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class TS315VMessageInterpreter : InboundMessageInterpreter<TS315VProvider>
	{
		public TS315VMessageInterpreter(AISUCC5InboundEDIMessage message, TS315VProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("2333D174-BE42-41CA-B327-51D40C217F0B", "A Temporary Storage Declaration Registration Acceptance message (TS315V) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LRN);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.DeclarationAcknowledgementDate, provider.AcknowledgementDate.ToShortDateString());
		}
	}
}

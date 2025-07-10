using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class TS315VMessageInterpreter : InboundMessageInterpreter<TS315VProvider>
	{
		public TS315VMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, TS315VProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("E76B4F67-A834-47A4-B3B6-E3BB45F48AC7", "A [G4 | G4+G3 | Manifest] Declaration Registration (TS315V) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.DeclarationAcknowledgementDate, provider.DeclarationAcknowledgementDate.ToShortDateString());
		}
	}
}

using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM415VMessageInterpreter : InboundMessageInterpreter<IM415VProvider>
	{
		public IM415VMessageInterpreter(AISUCC5InboundEDIMessage message, IM415VProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("86F73250-24C8-440D-9BF4-8EB9592F50FB", "A Customs Declaration Acknowledgement (IM415V) message has been received from customs for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.AdditionalDeclarationType, provider.AdditionalDeclarationType);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.DeclarationAcknowledgementDate, provider.DeclarationAcknowledgementDate.ToISO8601ShortDateString());
		}
	}
}

using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class IM415VMessageInterpreter : InboundMessageInterpreter<IM415VProvider>
	{
		public IM415VMessageInterpreter(AISInboundEDIMessage message, IM415VProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString("7A59D65D-372C-4E43-936F-9769C67A7859", "A Customs Declaration Acknowledgment (IM415V) message has been received for Job {0}.", relatedJob.JobNumber);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.AdditionalDeclarationType, provider.AdditionalDeclarationType);
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.DeclarationAcknowledgementDate, provider.DeclarationAcknowledgementDate.ToISO8601ShortDateString());
		}
	}
}

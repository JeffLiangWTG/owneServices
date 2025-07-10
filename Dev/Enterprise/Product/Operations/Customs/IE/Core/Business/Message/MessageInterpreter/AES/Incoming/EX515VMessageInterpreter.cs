using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class EX515VMessageInterpreter : InboundMessageInterpreter<EX515VProvider>
	{
		public EX515VMessageInterpreter(AESInboundEDIMessage message, EX515VProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"AF3495FA-1A43-40C1-AAB8-6054F4C2DF47",
			"An Export Declaration Acknowledgement message has been received from Customs for Job {0} through the EX515V message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, Res.GetString("F5E962B2-0DC2-42A1-8997-989B387E6284", "Acknowledged by Customs(MRN Allocated)"));
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.DeclarationAcknowledgementDate, provider.DeclarationAcknowledgementDate.ToLongTimeString());
		}
	}
}

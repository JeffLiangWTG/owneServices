using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC528MessageInterpreter : InboundMessageInterpreter<CC528CProvider>
	{
		public CC528MessageInterpreter(AESInboundEDIMessage message, CC528CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"E9684CE5-5E7D-4ADD-9A41-3BD0D81FDFD9",
			"An Export MRN Allocation message has been received from Customs for Job {0} through the IE528 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, Res.GetString("C7212300-56B8-4FD0-B812-19C6BD024A6E", "Accepted by Customs (MRN Allocated)"));
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.AcceptanceDate, provider.DeclarationAcceptanceDate.ToShortDateString());
		}
	}
}

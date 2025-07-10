using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.UCC5;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM428MessageInterpreter : InboundMessageInterpreter<IM428Provider>
	{
		public IM428MessageInterpreter(AISUCC5InboundEDIMessage message, IM428Provider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"2E0BDED9-2F3F-42D8-B6F4-4EB4E1F13A8B",
			"An Export MRN Allocation (IM428) message has been received from customs for Job {0}.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, Res.GetString("C0E61336-2217-4306-AF1A-A4DEF8DDECC1", "Accepted by Customs (MRN Allocated)"));
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.AcceptanceDate, provider.DeclarationAcceptanceDate);
		}
	}
}

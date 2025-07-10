using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC551MessageInterpreter : InboundMessageInterpreter<CC551CProvider>
	{
		public CC551MessageInterpreter(AESInboundEDIMessage message, CC551CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"45683E42-1F0B-47B0-8BE3-468F1618C52B",
			"An Export No Release message has been received from Customs for Job {0} through the IE551 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, Res.GetString("AD585914-F946-4F21-83EE-D0A2A850C440", "Export Release Rejected (REJ)"));
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("BFC237FE-5276-45F7-9A18-FEFFC6186B54", "Issues Reported"), provider.OtherThingsToReport);
			yield return (Res.GetString("E94D19D1-0959-493A-803D-985F72C04457", "Result Date"), provider.ControlResultDate.ToShortDateString());
			yield return (Res.GetString("77D0909F-0503-42E7-8A18-4FF597E2A5AD", "Result Text"), provider.ControlResultText);
		}
	}
}

using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC019CMessageInterpreter : InboundMessageInterpreter<CC019CProvider>
	{
		public CC019CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC019CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"6035070F-B63C-4B68-9F1D-969323464F7F",
			"A Discrepancy message has been received from Customs for Job {0} through the IE019 message.",
		relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (Res.GetString("EF3E9F58-FA7B-476A-BACD-5736022CBCD0", "Discrepancy Date"), provider.DiscrepancyDate.ToShortDateString());
			yield return (Res.GetString("D8FDE1A8-B003-411F-AD3D-678BAB7B416D", "Discrepancy Notification Text"), provider.DiscrepancyNotificationText);
		}
	}
}

using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC004CMessageInterpreter : InboundMessageInterpreter<CC004CProvider>
	{
		public CC004CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC004CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"86777570-D000-416B-8BCC-2D205256CBB1",
			"An Amendment Acceptance message has been received from Customs for Job {0} through the IE004 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MRN);
			yield return (CommonResStrings.AmendmentSubmissionDateAndTime, provider.AmendmentSubmissionDateTime.ToLongTimeString());
			yield return (CommonResStrings.AmendmentAcceptanceDateAndTime, provider.AmendmentAcceptanceDateTime.ToLongTimeString());
		}
	}
}

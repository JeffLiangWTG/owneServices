using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC574MessageInterpreter : InboundMessageInterpreter<CC574CProvider>
	{
		public CC574MessageInterpreter(AESInboundEDIMessage message, CC574CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"4F135EFD-7F94-42A9-A08F-64E9579CFAEF",
			"An Amendment Acceptance message has been received from Customs for Job {0} through the IE574 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.Status, Res.GetString("F1BF7492-6E12-4F15-AAFA-892DF0E4A5E0", "REL- Released for Export"));
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (CommonResStrings.AmendmentSubmissionDateAndTime, provider.AmendmentDateAndTime.ToLongTimeString());
			yield return (CommonResStrings.AmendmentAcceptanceDateAndTime, provider.AmendmentAcceptanceDateAndTime.ToLongTimeString());
		}
	}
}

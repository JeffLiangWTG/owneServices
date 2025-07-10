using System.Collections.Generic;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class CC051CMessageInterpreter : InboundMessageInterpreter<CC051CProvider>
	{
		public CC051CMessageInterpreter(Enterprise.Messaging.Business.EDIMessage message, CC051CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"AEA0C802-A632-413F-A8E5-37A72A056373",
			"A No Release for Transit (IE051) Message has been received for Job {0}.",
		relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("470E6CC8-0E61-42A5-AE9C-F567E6879033", "Declaration Submission Date And Time"), provider.DeclarationSubmissionDateAndTime.ToLongTimeString());
			yield return (Res.GetString("23F973AD-F07B-4FEF-983C-89F749B5A504", "No Release Motivation Code"), provider.NoReleaseMotivationCode);
			yield return (Res.GetString("C2C48E2D-AC50-4445-8B6B-B2F7F66A637A", "No Release Motivation Text"), provider.NoReleaseMotivationText);
		}
	}
}

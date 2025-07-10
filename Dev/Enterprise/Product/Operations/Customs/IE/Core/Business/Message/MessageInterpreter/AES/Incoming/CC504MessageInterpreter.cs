using System.Collections.Generic;
using Enterprise.Customs.IE.Messaging.AES;

namespace Enterprise.Customs.IE.Business.AES
{
	public class CC504MessageInterpreter : InboundMessageInterpreter<CC504CProvider>
	{
		public CC504MessageInterpreter(AESInboundEDIMessage message, CC504CProvider provider) : base(message, provider)
		{
		}

		protected override string Summary => Res.GetString(
			"B9C606C9-450F-4AE6-AAC7-5B6E030B58EB",
			"An Amendment Acceptance message has been received from Customs for Job {0} through the IE504 message.",
			relatedJob.JobNumber
		);

		protected override IEnumerable<(string Key, string Value)> GetMessageDetails()
		{
			yield return (CommonResStrings.LocalReferenceNumber, provider.LocalReferenceNumber);
			yield return (CommonResStrings.MovementReferenceNumber, provider.MovementReferenceNumber);
			yield return (Res.GetString("24A6F5AB-7227-4DB1-BA31-58E4FA04A99E", "Amendment Submission Date and Time"), provider.AmendmentSubmissionDate.ToLongTimeString());
			yield return (Res.GetString("387BD483-38E9-461B-A77B-08CFA33DBD1E", "Amendment Acceptance Date and Time"), provider.AmendmentAcceptanceDate.ToLongTimeString());
		}
	}
}
